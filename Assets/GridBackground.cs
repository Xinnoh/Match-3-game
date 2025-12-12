using UnityEngine;

public class GridBackground : MonoBehaviour
{
    [SerializeField] int width = 8;
    [SerializeField] int height = 8;
    [SerializeField] float cellSize = 1f;
    [SerializeField] Transform backgroundTilePrefab;
    [SerializeField] Transform centerTarget;

    void Awake()
    {
        GenerateBackgrounds();
    }

    void GenerateBackgrounds()
    {
        float totalW = (width - 1) * cellSize;
        float totalH = (height - 1) * cellSize;

        Vector3 origin = centerTarget.position - new Vector3(totalW * 0.5f, totalH * 0.5f, 0f);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = origin + new Vector3(x * cellSize, y * cellSize, 0);
                Transform t = Instantiate(backgroundTilePrefab, pos, Quaternion.identity, transform);

                GridBox b = t.GetComponent<GridBox>();
                if (b != null)
                    b.SetCoords(x, y);
            }
        }
    }
}
