using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    [SerializeField] int width = 8;
    [SerializeField] int height = 8;
    [SerializeField] float cellSize = 1f;
    [SerializeField] Gem gemPrefab;
    [SerializeField] List<GemSO> gemTypes;

    [SerializeField] Transform gridAnchor;   // grid starting point
    
    [SerializeField] private Gem[,] grid;   

    public Gem[,] Grid => grid;
    Gem dragging;
    public bool swapped;

    Vector3 origin;

    public Vector3 Origin => origin;
    public float CellSize => cellSize;
    public int Width => width;
    public int Height => height;

    void Start()
    {
        grid = new Gem[width, height];
        CalculateOrigin();
        Generate();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RegenerateGrid();
        }
    }

    void CalculateOrigin()
    {
        float totalW = (width - 1) * cellSize;
        float totalH = (height - 1) * cellSize;

        origin = gridAnchor.position - new Vector3(totalW / 2f, totalH / 2f, 0f);
    }

    void Generate()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                SpawnRandom(x, y);
    }
    void SpawnRandom(int x, int y)
    {
        Gem gem = null;
        GemSO so = null;
        int attempts = 0;
        int gridAttempts = 0;

        do
        {
            so = gemTypes[Random.Range(0, gemTypes.Count)];
            if (gem == null)
            {
                gem = Instantiate(gemPrefab, origin + new Vector3(x * cellSize, y * cellSize, 0), Quaternion.identity, transform);
            }
            gem.Init(so, x, y, this);
            attempts++;

            if (attempts > 10)
            {
                RegenerateGrid();
                attempts = 0;

                gridAttempts++;
                if(gridAttempts > 10)
                {
                    Debug.Log("Cannot generate grid");
                    break;
                }
            }
        }
        while (FormsInitialMatch(x, y, so));

        grid[x, y] = gem;
    }

    bool FormsInitialMatch(int x, int y, GemSO so)
    {
        // Check horizontal
        if (x >= 2)
        {
            if (grid[x - 1, y] != null && grid[x - 2, y] != null)
            {
                if (grid[x - 1, y].data == so && grid[x - 2, y].data == so)
                    return true;
            }
        }

        // Check vertical
        if (y >= 2)
        {
            if (grid[x, y - 1] != null && grid[x, y - 2] != null)
            {
                if (grid[x, y - 1].data == so && grid[x, y - 2].data == so)
                    return true;
            }
        }

        return false;
    }

    public void StartDrag(Gem g)
    {
        dragging = g;
        swapped = false;
    }

    public bool IsDragging(Gem g) => dragging == g;

    public void EndDrag()
    {
        if (dragging == null) return;

        var m = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        m.z = 0;

        m -= origin;

        int tx = Mathf.RoundToInt(m.x / cellSize);
        int ty = Mathf.RoundToInt(m.y / cellSize);

        Gem target = null;
        if (InBounds(tx, ty))
            target = grid[tx, ty];

        if (target != null && target != dragging)
        {
            Swap(dragging, target);
            swapped = true;

            // Check matches for both gems
            dragging.GetComponent<MatchDetector>().GetMatches();
            target.GetComponent<MatchDetector>().GetMatches();
        }

        // Snap back if no swap occurred
        if (!swapped)
        {
            dragging.transform.position = origin + new Vector3(dragging.x * cellSize, dragging.y * cellSize, 0);
        }

        dragging = null;
    
    }


    void Swap(Gem a, Gem b)
    {
        int ax = a.x, ay = a.y;
        int bx = b.x, by = b.y;

        grid[ax, ay] = b;
        grid[bx, by] = a;

        a.x = bx; a.y = by;
        b.x = ax; b.y = ay;

        a.transform.position = origin + new Vector3(bx * cellSize, by * cellSize, 0);
        b.transform.position = origin + new Vector3(ax * cellSize, ay * cellSize, 0);
    }

    public Gem GetGemAt(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width || y >= height) return null;
        return grid[x, y];
    }

    bool InBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }


    public void NotifyColumnAbove(int x, int startY)
    {
        List<Gem> gemsAbove = new List<Gem>();

        for (int y = startY + 1; y < Height; y++)
        {
            Gem g = grid[x, y];
            if (g != null && !g.isMatched)
                gemsAbove.Add(g);
        }

        // Sort by Y ascending (lowest first)
        gemsAbove.Sort((a, b) => a.y.CompareTo(b.y));

        foreach (var gemAbove in gemsAbove)
        {
            var fallScript = gemAbove.GetComponent<GemFall>();
            if (fallScript != null)
                fallScript.CheckIfCanFall();
        }
    }

    public void RegenerateGrid()
    {
        // Destroy existing gems
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (grid[x, y] != null)
                    Destroy(grid[x, y].gameObject);
            }
        }

        // Clear the array
        grid = new Gem[Width, Height];

        // Generate new gems
        Generate();
    }

}
