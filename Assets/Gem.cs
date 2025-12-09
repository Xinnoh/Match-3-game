using UnityEngine;

public class Gem : MonoBehaviour
{
    public GemSO data;
    public int x;
    public int y;

    [HideInInspector] public bool canMatch = true;
    [HideInInspector] public bool isMatched;
    [HideInInspector] public bool hasNotifiedColumn = false;
    [HideInInspector] public bool toDestroy;

    public Transform spriteTransform;
    bool dragging;
    SpriteRenderer sr;
    GridSystem grid;
    Vector3 startPos;
    private GemMatchEffect gemMatchEffect;
    private GemFall gemFall;

    public void Init(GemSO so, int gx, int gy, GridSystem g)
    {
        data = so;
        x = gx;
        y = gy;
        grid = g;

        if (spriteTransform == null)
        {
            var go = new GameObject("Sprite");
            go.transform.SetParent(null);
            spriteTransform = go.transform;
            sr = go.AddComponent<SpriteRenderer>();
        }
        else
        {
            sr = spriteTransform.GetComponent<SpriteRenderer>();
        }

        sr.sprite = so.sprite;
        spriteTransform.position = transform.position;

        gemMatchEffect = GetComponent<GemMatchEffect>();
        gemMatchEffect.sprite = spriteTransform;
        gemFall = GetComponent<GemFall>();

    }

    void Update()
    {
        // Lerp towards position
        if (!dragging && !gemFall.isFalling)
            spriteTransform.position = Vector3.Lerp(
                spriteTransform.position,
                transform.position,
                12f * Time.deltaTime
            );
    }

    void OnMouseDown()
    {
        dragging = true;
        startPos = spriteTransform.position;
        grid.StartDrag(this);
    }

    void OnMouseDrag()
    {
        if (grid.IsDragging(this))
        {
            var m = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            m.z = 0;
            spriteTransform.position = m;
        }
    }

    void OnMouseUp()
    {
        dragging = false;
        grid.EndDrag();
    }

    public void OnDestroy()
    {
        if(spriteTransform != null)
            Destroy(spriteTransform.gameObject);
    }
}
