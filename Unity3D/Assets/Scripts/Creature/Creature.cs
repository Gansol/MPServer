using UnityEngine;
using System.Collections;

public abstract class Creature : MonoBehaviour
{
    protected SkillBase m_Skill = null;
    protected AIState m_AIState = null;
    protected AnimatorState m_AnimState = null;
    protected CreatureAttr m_Arribute = null;


    public abstract void SetSkill(SkillBase skill);
    public abstract void SetState(AIState state);
    public abstract void SetAnimState(AnimatorState state);
    public abstract void SetArribute(CreatureAttr arribute);

    protected abstract void OnInjured(short damage, bool myAttack);

    /// <summary>
    /// 死亡
    /// </summary>
    /// <param name="lifeTime">存活時間</param>
    protected abstract void OnDead(float lifeTime);

    public virtual void Play(AnimatorState.ENUM_AnimatorState state)
    {
        if (m_AnimState!=null)
            m_AnimState.Play(state);
    }
}
