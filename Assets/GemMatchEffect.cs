using System.Collections;
using UnityEngine;

public class GemMatchEffect : MonoBehaviour
{
    [SerializeField] Transform childToAnimate;
    [HideInInspector] public Transform sprite;

    [SerializeField] AnimationCurve ringCurve;
    [SerializeField] float ringDuration = 0.3f;
    [SerializeField] float maxRingScale = 1.5f;

    [SerializeField] float pulsePeak = 1.3f;
    [SerializeField] float pulseUpTime = 0.2f;
    [SerializeField] float pulseDownTime = 0.3f;

    [SerializeField] AnimationCurve shrinkCurve;
    [SerializeField] float shrinkDelay = 0.2f;
    [SerializeField] float shrinkDuration = 0.3f;

    SpriteRenderer ringRenderer;

    void Awake()
    {
        if (childToAnimate != null)
            ringRenderer = childToAnimate.GetComponent<SpriteRenderer>();
    }

    public void PlayMatchEffect(bool isSpecial)
    {

        StartCoroutine(RingEffect());
        StartCoroutine(PulseEffect());

        if (!isSpecial)
            StartCoroutine(ShrinkDisappear());
    }


    //----------------------------
    //  EFFECT 1
    //----------------------------
    IEnumerator RingEffect()
    {
        if (ringRenderer != null)
            ringRenderer.enabled = true;

        float t = 0f;
        Vector3 startScale = Vector3.one;

        while (t < ringDuration)
        {
            t += Time.deltaTime;
            float k = t / ringDuration;

            float ringScale = ringCurve.Evaluate(k) * maxRingScale;
            childToAnimate.localScale = startScale * ringScale;

            yield return null;
        }

        childToAnimate.localScale = startScale * (ringCurve.Evaluate(1f) * maxRingScale);
    }


    //----------------------------
    //  EFFECT 2
    //----------------------------
    IEnumerator PulseEffect()
    {
        float t = 0f;
        Vector3 start = Vector3.one;

        // pulse up
        while (t < pulseUpTime)
        {
            t += Time.deltaTime;
            float k = t / pulseUpTime;
            sprite.localScale = Vector3.Lerp(start, start * pulsePeak, k);
            yield return null;
        }

        // pulse down
        t = 0f;
        while (t < pulseDownTime)
        {
            t += Time.deltaTime;
            float k = t / pulseDownTime;
            sprite.localScale = Vector3.Lerp(start * pulsePeak, start, k);
            yield return null;
        }

        sprite.localScale = start;
    }


    //----------------------------
    //  EFFECT 3
    //----------------------------
    IEnumerator ShrinkDisappear()
    {
        yield return new WaitForSeconds(shrinkDelay);

        float t = 0f;
        Vector3 start = sprite.localScale;

        while (t < shrinkDuration)
        {
            t += Time.deltaTime;
            float k = t / shrinkDuration;
            sprite.localScale = start * shrinkCurve.Evaluate(k);
            yield return null;
        }

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
