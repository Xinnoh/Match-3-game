using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchQueueManager : MonoBehaviour
{
    public static MatchQueueManager Instance;

    [SerializeField] float delayBetweenMatches = 0.2f;

    /// <summary>
    /// Stores each match made, and plays them in order after a delay
    /// </summary>

    private class MatchItem
    {
        public List<GemMatchEffect> effects;
        public bool isSpecial;
        public MatchItem(List<GemMatchEffect> e, bool s) { effects = e; isSpecial = s; }
    }

    private Queue<MatchItem> matchQueue = new Queue<MatchItem>();
    private bool isPlaying = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void EnqueueMatch(List<GemMatchEffect> effects, bool isSpecial)
    {
        if (effects == null || effects.Count == 0) return;

        matchQueue.Enqueue(new MatchItem(effects, isSpecial));

        if (!isPlaying)
            StartCoroutine(PlayQueue());
    }

    private IEnumerator PlayQueue()
    {
        isPlaying = true;

        while (matchQueue.Count > 0)
        {
            var item = matchQueue.Dequeue();

            ScoreManager.Instance.OnMatch(item.effects.Count);

            for (int i = 0; i < item.effects.Count; i++)
            {
                bool passSpecial = item.isSpecial && i == 0;
                item.effects[i].PlayMatchEffect(passSpecial);
            }

            yield return new WaitForSeconds(delayBetweenMatches);
        }

        isPlaying = false;
    }
}
