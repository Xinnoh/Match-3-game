using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;
    public bool timeAttack;
    private GridSystem gridSystem;
    public bool CanMove { get; private set; } = true;
    [SerializeField] private TextMeshProUGUI turnStatusText;


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        gridSystem = GetComponent<GridSystem>();
        UpdateUI();
    }


    public void LockInput()
    {
        CanMove = false;
        UpdateUI();

    }

    public void UnlockInput()
    {
        CanMove = true;
        UpdateUI();

    }


    public void CheckIfTurnOver()
    {
        if (AreAllGemsReady(gridSystem) && !CanMove)
        {
            UnlockInput();
        }
    }

    public bool AreAllGemsReady(GridSystem gridSystem)
    {
        for (int x = 0; x < gridSystem.Width; x++)
        {
            for (int y = 0; y < gridSystem.Height; y++)
            {

                // If anything could match or is matching, return false
                Gem g = gridSystem.GetGemAt(x, y);
                if (g != null && (g.couldStartMatch || g.isMatched))
                    return false;
            }
        }

        return true;
    }

    void UpdateUI()
    {
        if (turnStatusText != null)
        {
            turnStatusText.text = CanMove ? "Player Turn" : "Waiting...";
        }
    }
}
