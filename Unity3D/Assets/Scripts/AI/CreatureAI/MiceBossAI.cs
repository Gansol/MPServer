using UnityEngine;
using System.Collections;

public class MiceBossAI : ICreatureAI
{
    private bool _bDead;

    public MiceBossAI(ICreature mice) : base(mice)
    {
       SetAIState(new IdleAIState(/*this*/));
    }


    public override void UpdateAI()
    {
        base.UpdateAI();


        // 
        // if idle state -> play anim
        // if get onhit -> play onhit -> 強制
        // 

        // 如果為 初始狀態(閒置) 播放Hello動畫
        if (GetAIState() == (int)ICreature.ENUM_CreatureAIState.Idle && m_Creature.GetAminState().GetENUM_AnimState() != IAnimatorState.ENUM_AnimatorState.Hello)
            m_Creature.Play(IAnimatorState.ENUM_AnimatorState.Hello);

        // 如果HP<0 切換死亡狀態
        if (m_Creature.GetArribute().GetHP() < 0 || m_Creature.GetAIState()== ICreature.ENUM_CreatureAIState.Died && !_bDead )
        {
            _bDead = true;
            SetAIState(new DiedAIState(/*this*/)); // 如果外部使用SetAIState已經設定死亡會設定2次 錯誤
            m_Creature.Play(IAnimatorState.ENUM_AnimatorState.Died);
        }
        

        // if anim = disappear
    }
}
