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

    public void EnqueueMatch(List<GemMatchEffect> effects, bool isSpecial)
    {
        if (effects != null && effects.Count > 0)
        {
            matchQueue.Enqueue(effects);
            if (!isPlaying)
                StartCoroutine(PlayQueue(isSpecial));
        }
    }

    private IEnumerator PlayQueue(bool isSpecial)
    {
        isPlaying = true;

        while (matchQueue.Count > 0)
        {
            var effects = matchQueue.Dequeue();

            for (int i = 0; i < effects.Count; i++)
            {
                bool passSpecial = isSpecial && i == 0;
                effects[i].PlayMatchEffect(passSpecial);
            }

            yield return new WaitForSeconds(delayBetweenMatches);
        }

        isPlaying = false;
    }

}
