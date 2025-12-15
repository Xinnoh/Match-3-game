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

    public void OnMatch(int matchLength)
    {
        float multiplier = GetMultiplier(matchLength);
        score += Mathf.RoundToInt(matchLength * multiplier);
        UpdateUI();
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
}
