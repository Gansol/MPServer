using UnityEngine;
using System.Collections;

public class SkillShield : ISkillBoss
{
    public override void Initialize()
    {
        throw new System.NotImplementedException();
    }

    public override void Display(GameObject obj, CreatureAttr arribute, AIState state)
    {
        arribute.SetShield(50);
    }
}
