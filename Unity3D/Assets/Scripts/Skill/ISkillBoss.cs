using UnityEngine;
using System.Collections;

public abstract class ISkillBoss : Skill
{

    public override void Initialize()
    {
        throw new System.NotImplementedException();
    }

    public override void Display(GameObject obj, CreatureAttr arribute, AIState state) { }
}
