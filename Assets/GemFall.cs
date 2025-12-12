using System.Collections;
using UnityEngine;

public class GemFall : MonoBehaviour
{
    private Gem gem;
    private GridSystem grid;

    [SerializeField] float fallDelay = 0.3f;
    [SerializeField] float fallSpeed = 5f;

    public bool isFalling = false;

    void Awake()
    {
        gem = GetComponent<Gem>();
        grid = FindFirstObjectByType<GridSystem>();
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
        int topY = grid.Height - 1;

        while (true)
        {
            int belowY = y - 1;
            if (belowY < 0)
            {
                break;
            }

            GridBox belowBox = grid.Boxes[x, belowY];
            Gem belowGem = belowBox.heldGem;

            if (belowGem != null)
            {
                var fallScript = belowGem.GetComponent<GemFall>();
                bool gemShouldFall = !belowGem.toDestroy && (fallScript == null || !fallScript.isFalling);

                if (gemShouldFall)
                {
                    break;
                }
            }

            // This is required else spawning breaks
            yield return new WaitForSeconds(fallDelay);


            grid.Boxes[x, y].SetGem(null);
            grid.Grid[x, y] = null;
            grid.Grid[x, belowY] = gem;
            grid.Boxes[x, belowY].SetGem(gem);

            gem.y = belowY;

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
        }

        isFalling = false;
        gem.canMatch = true;

        yield return null;

        gem.GetComponent<MatchDetector>()?.GetMatches();
    }
}
