using UnityEngine;
using System.Collections;

public class MiceAI : ICreatureAI
{
    public MiceAI(ICreature mice) : base(mice)
    {
       SetAIState(new IdleAIState());
    }


    public override void UpdateAI()
    {
        base.UpdateAI();
        // 如果HP<0 切換死亡狀態
        if (m_Creature.GetArribute().GetHP() < 1 && m_Creature.GetAIState() != ICreature.ENUM_CreatureAIState.Died)
        {
            m_Creature.m_go.GetComponent<BoxCollider2D>().enabled = false;
            SetAIState(new DiedAIState());
            m_Creature.Play(IAnimatorState.ENUM_AnimatorState.Died);
        }

        // if anim = disappear
    }
}
