using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchQueueManager : MonoBehaviour
{
    public static MatchQueueManager Instance;

    [SerializeField] float delayBetweenMatches = 0.3f;

    private Queue<List<GemMatchEffect>> matchQueue = new Queue<List<GemMatchEffect>>();
    private bool isPlaying = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void EnqueueMatch(List<GemMatchEffect> effects)
    {
        if (effects != null && effects.Count > 0)
        {
            matchQueue.Enqueue(effects);
            if (!isPlaying)
                StartCoroutine(PlayQueue());
        }
    }

    private IEnumerator PlayQueue()
    {
        isPlaying = true;

        while (matchQueue.Count > 0)
        {
            var effects = matchQueue.Dequeue();

            // Play all effects in this match simultaneously
            foreach (var effect in effects)
            {
                effect.PlayMatchEffect();
            }

            // Wait for a short delay before next match in the queue
            yield return new WaitForSeconds(delayBetweenMatches);
        }

        isPlaying = false;
    }
}
