using UnityEngine;

public class GemSpawner : MonoBehaviour
{
    GridSystem gridSystem;
    GridBox[,] boxes;

    public void Initialise()
    {
        gridSystem = GetComponent<GridSystem>();
        boxes = gridSystem.Boxes;
    }

    void Update()
    {
        SpawnTopRowIfNeeded();
    }

    void SpawnTopRowIfNeeded()
    {
        int topY = gridSystem.Height - 1;

        for (int x = 0; x < gridSystem.Width; x++)
        {

            // Only check GridBox, as requested
            if (boxes[x, topY - 1].heldGem == null && boxes[x, topY].heldGem == null)
            {
                SpawnAtTop(x, topY);
            }
        }
    }

    void SpawnAtTop(int x, int y)
    {
        GridBox box = boxes[x, y];

        Gem gem = Instantiate(
            gridSystem.gemPrefab,
            box.transform.position,
            Quaternion.identity,
            gridSystem.transform
        );

        GemSO so = gridSystem.gemTypes[Random.Range(0, gridSystem.gemTypes.Count)];

        gem.Init(so, x, y, gridSystem);

        // Update both representations
        gridSystem.Grid[x, y] = gem;
        box.SetGem(gem);
    }
}
