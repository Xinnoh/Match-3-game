using System.Collections;
using UnityEngine;

public class GemMatchEffect : MonoBehaviour
{
    [SerializeField] Transform childToAnimate;    // the child object to scale
    [HideInInspector] public Transform sprite;
    [SerializeField] AnimationCurve growCurve;
    [SerializeField] AnimationCurve shrinkCurve;
    [SerializeField] float ringDuration = 0.3f;
    [SerializeField] float shrinkDelay = 0.2f;
    [SerializeField] float shrinkDuration = 0.3f;
    [SerializeField] float maxChildScale = 1.5f;


    [SerializeField] float flashPeakSize = 1.3f;
    [SerializeField] float flashUpTime = 0.2f;
    [SerializeField] float flashDownTime = 0.3f;

    SpriteRenderer childRenderer;

    void Awake()
    {
        if (childToAnimate != null)
            childRenderer = childToAnimate.GetComponent<SpriteRenderer>();
    }

    public void PlayMatchEffect(bool isSpecial)
    {
        if (childToAnimate != null)
        {
            if (childRenderer != null)
                childRenderer.enabled = true;

            StartCoroutine(InitialMatchEffect());
        }

        if (!isSpecial)
        {
            StartCoroutine(ShrinkAndDestroy());

        }

    }

    IEnumerator InitialMatchEffect()
    {
        float time = 0f;
        Vector3 ringStart = childToAnimate.localScale;

        // sprite flash params
        Vector3 spriteStart = sprite.localScale;

        while (time < ringDuration)
        {
            time += Time.deltaTime;
            float t = time / ringDuration;

            // ring scale
            float rs = growCurve.Evaluate(t) * maxChildScale;
            childToAnimate.localScale = ringStart * rs;

            // sprite flash up
            if (time <= flashUpTime)
            {
                float ft = time / flashUpTime;
                sprite.localScale = Vector3.Lerp(spriteStart, spriteStart * flashPeakSize, ft);
            }
            // sprite flash down
            else if (time <= flashUpTime + flashDownTime)
            {
                float ft = (time - flashUpTime) / flashDownTime;
                sprite.localScale = Vector3.Lerp(spriteStart * flashPeakSize, spriteStart, ft);
            }

            yield return null;
        }

        childToAnimate.localScale = ringStart * (growCurve.Evaluate(1f) * maxChildScale);
        sprite.localScale = spriteStart;
    }



    IEnumerator ShrinkAndDestroy()
    {
        yield return new WaitForSeconds(shrinkDelay);

        float time = 0f;
        Vector3 initialScale = sprite.localScale;

        while (time < shrinkDuration)
        {
            time += Time.deltaTime;
            float t = time / shrinkDuration;
            sprite.localScale = initialScale * shrinkCurve.Evaluate(t);
            yield return null;
        }

        // Now the gem is gone; notify above
        GridSystem grid = FindObjectOfType<GridSystem>();
        Gem g = GetComponent<Gem>();
        if (grid != null && g != null && !g.hasNotifiedColumn)
        {
            g.toDestroy = true;
            grid.NotifyColumnAbove(g.x, g.y);
            g.hasNotifiedColumn = true;
        }

        Destroy(gameObject);
    }

}
