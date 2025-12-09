using System.Collections;
using UnityEngine;

public class GemFall : MonoBehaviour
{
    Gem gem;
    GridSystem grid;

    [SerializeField] float fallDelay = 0.3f;
    [SerializeField] float fallSpeed = 5f; // units per second

    [HideInInspector] public bool isFalling = false;

    void Awake()
    {
        gem = GetComponent<Gem>();
        grid = FindObjectOfType<GridSystem>();
    }

    public void CheckIfCanFall()
    {
        if (gem == null || grid == null) return;
        if (isFalling) return;

        int x = gem.x;
        int y = gem.y;

        int belowY = y - 1;
        if (belowY < 0) return;

        Gem belowGem = grid.GetGemAt(x, belowY);

        if (belowGem == null ||
            belowGem.toDestroy ||
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

        while (true)
        {
            int belowY = y - 1;
            if (belowY < 0) break;

            Gem belowGem = grid.GetGemAt(x, belowY);
            if (belowGem != null)
            {
                var fallScript = belowGem.GetComponent<GemFall>();

                bool gemShouldFall = !belowGem.toDestroy && (fallScript == null || !fallScript.isFalling);

                if (gemShouldFall) break;
            }

            yield return new WaitForSeconds(fallDelay);

            // Update grid
            grid.Grid[x, y] = null;
            grid.Grid[x, belowY] = gem;

            // Update gem coordinates immediately
            gem.y = belowY;

            // Animate sprite visually, but keep parent at grid position for logic
            Vector3 startPos = gem.spriteTransform.position;
            Vector3 targetPos = grid.Origin + new Vector3(x * grid.CellSize, belowY * grid.CellSize, 0);

            float dist = Vector3.Distance(startPos, targetPos);
            float duration = dist / 5f; // fallSpeed, adjust as needed
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                gem.spriteTransform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            gem.spriteTransform.position = targetPos;

            // Move parent to the grid location **after animation**
            gem.transform.position = targetPos;

            y = belowY;
        }

        isFalling = false;
        gem.canMatch = true;

        yield return null;

        var detector = gem.GetComponent<MatchDetector>();
        if (detector != null)
            detector.GetMatches();
    }

}
