using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchQueueManager : MonoBehaviour
{
    public static MatchQueueManager Instance;

    [SerializeField] float delayBetweenMatches = 0.2f;

    private class MatchItem
    {
        public List<GemMatchEffect> effects;
        public bool isSpecial;
        public int attackPower;

        public MatchItem(List<GemMatchEffect> e, bool s, int a)
        {
            effects = e;
            isSpecial = s;
            attackPower = a;
        }
    }

    private Queue<MatchItem> matchQueue = new Queue<MatchItem>();
    private bool isPlaying = false;

    private int comboCount = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void EnqueueMatch(List<GemMatchEffect> effects, bool isSpecial, int attackPower)
    {
        if (effects == null || effects.Count == 0) return;

        matchQueue.Enqueue(new MatchItem(effects, isSpecial, attackPower));

        if (!isPlaying)
            StartCoroutine(PlayQueue());
    }

    private IEnumerator PlayQueue()
    {
        isPlaying = true;
        comboCount = 0;

        while (matchQueue.Count > 0)
        {
            comboCount++;

            var item = matchQueue.Dequeue();

            ScoreManager.Instance.OnMatch(
                item.effects.Count,
                comboCount,
                item.attackPower
            );

            for (int i = 0; i < item.effects.Count; i++)
            {
                bool passSpecial = item.isSpecial && i == 0;
                item.effects[i].PlayMatchEffect(passSpecial);
            }

            yield return new WaitForSeconds(delayBetweenMatches);
        }

        isPlaying = false;
    }

    public void ResetCombo()
    {
        comboCount = 0;
    }
}
