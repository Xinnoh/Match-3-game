using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class EnemySO : ScriptableObject
{
    public string enemyName;
    public string enemyDescription;
    public int maxHealth;
    public EnemySkillSO EnemySkills;
    public Sprite sprite;

}
