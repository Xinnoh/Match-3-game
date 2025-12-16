using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public int score;
    [SerializeField] TextMeshProUGUI scoreText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateUI();
    }

    public void OnMatch(int matchLength, int combo, int attackPower)
    {
        float matchMultiplier = GetMultiplier(matchLength);
        float comboMultiplier = GetComboMultiplier(combo);

        int totalAttack = Mathf.RoundToInt(matchMultiplier * comboMultiplier * attackPower);

        score += totalAttack;
        UpdateUI();

        EnemyManager.Instance.TakeDamage(totalAttack);
    }

    float GetMultiplier(int matchLength)
    {
        if (matchLength >= 6) return 3f;
        if (matchLength == 5) return 2f;
        if (matchLength == 4) return 1.5f;
        return 1f;
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = score.ToString();
    }

    float GetComboMultiplier(int combo)
    {
        if (combo < 2) return 1f;
        if (combo < 5) return 1.1f;
        if (combo < 10) return 1.15f;
        if (combo < 25) return 1.2f;
        if (combo < 50) return 1.3f;
        if (combo < 75) return 1.4f;
        if (combo < 100) return 1.5f;
        if (combo < 200) return 2f;
        return 2.5f;
    }
}
