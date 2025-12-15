using UnityEngine;

public class Gem : MonoBehaviour
{
    [HideInInspector] public GemSO gemSO;
    public int x, y;

    public bool canMatch = true;
    [HideInInspector] public bool couldStartMatch; // Used to track when to end the turn
    public bool isMatched, isSpecialMatch;
    [HideInInspector] public bool hasNotifiedColumn = false;
    [HideInInspector] public bool toDestroy;

    public Transform spriteTransform;
    public bool dragging;
    SpriteRenderer sr;
    GridSystem grid;
    Vector3 startPos;
    private GemMatchEffect gemMatchEffect;
    private GemFall gemFall;



    public void Init(GemSO gemSOParent, int gx, int gy, GridSystem g)
    {
        gemSO = gemSOParent;
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

        sr.sprite = gemSOParent.sprite;
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
        if (!CanDrag()) return;

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

    bool CanDrag()
    {
        if (isMatched) return false;
        if (TurnManager.Instance == null) return true;
        return TurnManager.Instance.CanMove;
    }

}
