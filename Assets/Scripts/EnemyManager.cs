using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{

    public static EnemyManager Instance;

    // Take damage from score manager

    // Create attacks between turns based on cooldowns

    // Call gameover when health reaches 0

    // Call gameover when turns reach 0

    private ScoreManager scoreManager;
    private EnemyHealthManager enemyHealthManager;

    public EnemySO enemySO;

    public bool isDefeated = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void Start()
    {
        enemyHealthManager = GetComponent<EnemyHealthManager>();
        scoreManager = ScoreManager.Instance;
    }



    public void TakeDamage(int damage)
    {
        enemyHealthManager.TakeDamage(damage);

    }


    public void Die()
    {
        if(isDefeated) return;

        isDefeated = true;

        Debug.Log("Enemy has died.");
        // Placeholder for death logic
    }


}
