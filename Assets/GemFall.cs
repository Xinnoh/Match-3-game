using System.Collections;
using UnityEngine;

public class GemFall : MonoBehaviour
{
    Gem gem;
    GridSystem grid;

    [SerializeField] float fallDelay = 0.3f;
    [SerializeField] float fallSpeed = 5f; 

    [HideInInspector] public bool isFalling = false;

    void Awake()
    {
        gem = GetComponent<Gem>();
        grid = FindObjectOfType<GridSystem>();
    }

    private void FixedUpdate()
    {
        CheckIfCanFall();
    }

    public void CheckIfCanFall()
    {
        if (gem == null || grid == null) return;
        if (isFalling || gem.isMatched) return;

        int x = gem.x;
        int y = gem.y;

        int belowY = y - 1;
        if (belowY < 0) return;

        GridBox belowBox = grid.Boxes[x, belowY];
        Gem belowGem = belowBox.heldGem;

        // fall if box empty OR gem inside is falling/destroying
        if (belowGem == null || belowGem.toDestroy ||
            (belowGem.TryGetComponent(out GemFall fall) && fall.isFalling))
        {
            StartCoroutine(FallRoutine(x, y));
        }
    }


    IEnumerator FallRoutine(int startX, int startY)
    {
        isFalling = true;
        gem.canMatch = false;

        int x = startX;
        int y = startY;
        int topY = grid.Height - 1 + grid.bufferRows;


        while (true)
        {
            int belowY = y - 1;
            if (belowY < 0) break;

            GridBox belowBox = grid.Boxes[x, belowY];
            Gem belowGem = belowBox.heldGem;

            if (belowGem != null)
            {
                var fallScript = belowGem.GetComponent<GemFall>();
                bool gemShouldFall = !belowGem.toDestroy && (fallScript == null || !fallScript.isFalling);
                if (gemShouldFall) break;
            }

            yield return new WaitForSeconds(fallDelay);

            // clear old box
            grid.Boxes[x, y].SetGem(null);

            // update grid and box
            grid.Grid[x, y] = null;
            grid.Grid[x, belowY] = gem;
            grid.Boxes[x, belowY].SetGem(gem);

            // update gem coords
            gem.y = belowY;

            // animate sprite
            Vector3 startPos = gem.spriteTransform.position;
            Vector3 targetPos = grid.Boxes[x, belowY].transform.position;

            float dist = Vector3.Distance(startPos, targetPos);
            float duration = dist / fallSpeed;
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                gem.spriteTransform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            gem.spriteTransform.position = targetPos;
            gem.transform.position = targetPos;

            y = belowY;

            if (y == topY - grid.bufferRows)
            {
                GridBox topBox = grid.Boxes[x, topY];
                if (topBox.heldGem == null)
                {
                    // spawn new gem above
                    Gem newGem = Object.Instantiate(grid.gemPrefab, topBox.transform.position, Quaternion.identity, grid.transform);
                    GemSO so = grid.gemTypes[Random.Range(0, grid.gemTypes.Count)];
                    newGem.Init(so, x, topY, grid);

                    // prevent it from matching until it falls
                    newGem.canMatch = false;

                    grid.Grid[x, topY] = newGem;
                    topBox.SetGem(newGem);

                    // optional: start falling automatically
                    newGem.GetComponent<GemFall>()?.CheckIfCanFall();
                }

                // current gem can now match since it reached second-from-top row
                gem.canMatch = true;
            }
        }

        isFalling = false;
        gem.canMatch = true;

        yield return null;

        gem.GetComponent<MatchDetector>()?.GetMatches();
    }
}
