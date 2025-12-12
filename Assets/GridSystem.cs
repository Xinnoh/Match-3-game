using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    [SerializeField] int width = 8;
    [SerializeField] int height = 8;
    [SerializeField] float cellSize = 1f;
    public Gem gemPrefab;
    public List<GemSO> gemTypes;

    [SerializeField] Transform gridAnchor;   // grid starting point
    
    [SerializeField] private Gem[,] grid;   

    public Gem[,] Grid => grid;
    Gem dragging;
    public bool swapped;

    Vector3 origin;

    [SerializeField] GridBox[,] boxes;
    public GridBox[,] Boxes => boxes;

    [SerializeField] Transform boxRoot;

    public Vector3 Origin => origin;
    public float CellSize => cellSize;
    public int Width => width;
    public int Height => height;

    private GemSpawner gemSpawner;


    void Start()
    {
        LoadGridBoxes();

        grid = new Gem[width, height];
        CalculateOrigin();
        Generate();

        gemSpawner = GetComponent<GemSpawner>();
        gemSpawner.Initialise();
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RegenerateGrid();
        }
    }


    // Where to place the grid
    void CalculateOrigin()
    {
        float totalW = (width - 1) * cellSize;
        float totalH = (height - 1) * cellSize;

        origin = gridAnchor.position - new Vector3(totalW / 2f, totalH / 2f, 0f);
    }

    void LoadGridBoxes()
    {
        boxes = new GridBox[width, height];

        foreach (Transform t in boxRoot)
        {
            GridBox b = t.GetComponent<GridBox>();
            if (b != null)
                boxes[b.gx, b.gy] = b;
        }
    }

    void Generate()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                SpawnRandom(x, y);
    }

    // Place random gems on grid
    // Also makes sure there's no matches
    void SpawnRandom(int x, int y)
    {
        Gem gem = null;
        GemSO so = null;
        int attempts = 0;
        int gridAttempts = 0;

        do
        {
            if (boxes[x, y] == null)
            {
                Debug.LogError("Missing GridBox at: " + x + "," + y);
            }

            so = gemTypes[Random.Range(0, gemTypes.Count)];
            if (gem == null)
            {
                gem = Instantiate(gemPrefab, boxes[x, y].transform.position, Quaternion.identity, transform);
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
        boxes[x, y].SetGem(gem);
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

        if (InBounds(tx, ty) && boxes[tx, ty] != null)
        {
            Gem targetLocation = grid[tx, ty];
            
            // Do not swap if target is currently matching, or should not match
            if (targetLocation == null || !targetLocation.isMatched)
            {
                SwapToBox(dragging, tx, ty);
                swapped = true;

                if (targetLocation != null)
                {
                    dragging.GetComponent<MatchDetector>().GetMatches();
                    targetLocation.GetComponent<MatchDetector>().GetMatches();
                }
                else
                {
                    dragging.GetComponent<MatchDetector>().GetMatches();
                }
            }
        }

        // Snap back if no swap occurred
        if (!swapped)
        {
            dragging.transform.position = boxes[dragging.x, dragging.y].transform.position;
            boxes[dragging.x, dragging.y].SetGem(dragging);
        }

        dragging = null;
    }

    void SwapToBox(Gem a, int tx, int ty)
    {
        int ax = a.x;
        int ay = a.y;

        Gem b = grid[tx, ty];     // can be null

        // If there is a gem in the target box, swap
        if (b != null)
        {
            grid[ax, ay] = b;
            grid[tx, ty] = a;

            a.x = tx; a.y = ty;
            b.x = ax; b.y = ay;

            a.transform.position = boxes[tx, ty].transform.position;
            b.transform.position = boxes[ax, ay].transform.position;

            boxes[ax, ay].SetGem(b);
            boxes[tx, ty].SetGem(a);
        }
        else
        {
            // Move only A
            grid[ax, ay] = null;
            grid[tx, ty] = a;

            a.x = tx;
            a.y = ty;

            a.transform.position = boxes[tx, ty].transform.position;

            boxes[ax, ay].SetGem(null);
            boxes[tx, ty].SetGem(a);
        }
    }


    public Gem GetGemAt(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width || y >= height) return null;
        return grid[x, y];
    }

    public bool InBounds(int x, int y)
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
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (grid[x, y] != null)
                    Destroy(grid[x, y].gameObject);
            }
        }

        grid = new Gem[Width, Height];

        Generate();
    }

}
