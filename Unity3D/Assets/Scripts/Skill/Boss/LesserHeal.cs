using UnityEngine;
using System.Collections;

public class LesserHeal : SkillBoss
{
    ICreatureAttr arribute;
    public LesserHeal(SkillAttr skill)
        : base(skill)
    {
    }

    public override void Initialize()
    {
        throw new System.NotImplementedException();
    }

    public override void Display(ICreature creature/*, CreatureAttr arribute/*, IAIState state*/)
    {
        arribute = creature.GetArribute();
    }

    public override void UpdateEffect()
    {
        if (Time.time > m_LastTime + skillData.ColdDown && (Time.time - m_StartTime) < skillData.SkillTime)
        {
            arribute.SetHP(arribute.GetHP() + Random.Range(skillData.Attr, skillData.Attr + skillData.AttrDice + 1));
            m_LastTime = Time.time;
        }

        if ((Time.time - m_StartTime) > skillData.SkillTime)
            Release();
    }

    public override void Release()
    {
        throw new System.NotImplementedException();
    }

    public override void Display()
    {
        throw new System.NotImplementedException();
    }
}
