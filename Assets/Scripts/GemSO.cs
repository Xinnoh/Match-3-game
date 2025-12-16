using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class GemSO : ScriptableObject {

    public string gemName;
    public Sprite sprite;
    public int attackStat;
    public int level = 1;
    public int exp = 0;
    public SkillSO[] skills;
    public bool hasMega;

}
