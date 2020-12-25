using UnityEngine;
using System.Collections;

public abstract class ICreature 
{
    public GameObject m_go = null;
    protected ISkill m_Skill = null;
    //protected IAIState m_AIState = null;
    protected IAnimatorState m_AnimState = null;
    protected ICreatureAttr m_Arribute = null;
    protected ICreatureAI m_AI = null;
    protected ENUM_CreatureState ENUM_AIState = ENUM_CreatureState.None;

    public enum ENUM_CreatureState
    {
        None = -1,
        Hello = 0,
        Idle,
        Eat,
        ByeBye,
        Die,
        OnHit,
        Frozen,
        Fire,
        Invincible,
    }

    public abstract void SetGameObject(GameObject go);
    public abstract void SetSkill(ISkill skill);
    //public abstract void SetState(IAIState state);
    public abstract void SetAnimState(IAnimatorState state);
    public abstract void SetArribute(ICreatureAttr arribute);
    public abstract void SetAI(ICreatureAI ai);

    protected abstract void OnInjured(short damage, bool myAttack);
    public abstract void Initialize();
    public abstract void Update();
    public abstract void Release();
    /// <summary>
    /// 死亡
    /// </summary>
    /// <param name="lifeTime">存活時間</param>
    protected abstract void OnDead(float lifeTime);

    public virtual void Play(IAnimatorState.ENUM_AnimatorState state)
    {
        if (m_AnimState!=null)
            m_AnimState.Play(state);
    }
    public virtual void SetState(ENUM_CreatureState state = ENUM_CreatureState.None)
    {
        ENUM_AIState = state;
    }

    public ENUM_CreatureState GetState()
    {
        return ENUM_AIState;
    }

    public float GetSurvivalTime()
    {
        return m_AnimState.GetSurvivalTime();
    }
    public ICreatureAI GetAIState()
    {
        return m_AI;
    }

    public ICreatureAttr GetArribute()
    {
        return m_Arribute;
    }
}
