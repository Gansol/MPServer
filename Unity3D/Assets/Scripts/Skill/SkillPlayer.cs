using UnityEngine;
using System.Collections;

public abstract class SkillPlayer : ISkill
{
    protected int useTimes = 0;

    public SkillPlayer(SkillAttr skill)
        : base(skill)
    {
    }

    public override void Initialize()
    {
        throw new System.NotImplementedException();
    }

    public override void UpdateEffect()
    {
        throw new System.NotImplementedException();
    }

    public override void Release()
    {
        throw new System.NotImplementedException();
    }

    public override void Display(ICreature creature/*, CreatureAttr arribute/*, IAIState state*/)
    {
        throw new System.NotImplementedException();
    }

    public override void Display()
    {
        throw new System.NotImplementedException();
    }
}
