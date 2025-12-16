using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class SkillSO : ScriptableObject
{

    public string skillName;
    public string skillDescription;

    public SkillType skillType;
    public MatchType matchType;
    public TargetType targetType;

    public Sprite sprite;
    public float damageMult;

    public float m3Proc = 1;
    public float m4Proc = 1;
    public float m5Proc = 1;


}
