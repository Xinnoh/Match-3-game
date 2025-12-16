using System;
using System.Collections;
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

    [SerializeField] float fadeInDuration = 0.25f;

    public void Init(GemSO gemSOParent, int gx, int gy, GridSystem g, bool fadeIn)
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

        MatchDetector matchDetector = GetComponent<MatchDetector>();
        matchDetector.attackStat = gemSOParent.attackStat;

        

        if (fadeIn)
            StartCoroutine(FadeIn());
    }

    void Update()
    {
        LerpTowardsTransform();
    }


    void OnMouseDown()
    {
        if (!CanDrag()) return;

        dragging = true;
        startPos = spriteTransform.position;
        grid.StartDrag(this);

        // Raise sprite above others
        if (sr != null)
            sr.sortingOrder = 2;
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

        StartCoroutine(ResetSortingOrder());
    }

    private IEnumerator ResetSortingOrder()
    {
        while ((spriteTransform.position - transform.position).sqrMagnitude > 0.01f)
            yield return null;

        if (sr != null)
            sr.sortingOrder = 0;
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

    private void LerpTowardsTransform()
    {
        if (!dragging && !gemFall.isFalling)
            spriteTransform.position = Vector3.Lerp(
                spriteTransform.position,
                transform.position,
                12f * Time.deltaTime
            );
    }

    // Fade in effect when spawned at top
    private IEnumerator FadeIn()
    {
        Color c = sr.color;
        c.a = 0f;
        sr.color = c;

        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Clamp01(t / fadeInDuration);
            sr.color = c;
            yield return null;
        }

        c.a = 1f;
        sr.color = c;
    }
}
