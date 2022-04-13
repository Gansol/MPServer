using UnityEngine;
using System.Collections;

public abstract class ICreature
{
    public GameObject m_go = null;
    protected ISkill m_Skill = null;
    //protected IAIState m_AIState = null;
    protected IAnimatorState m_AnimState = null;
    protected ICreatureAttr m_Attribute = null;
    protected ICreatureAI m_AI = null;

    public enum ENUM_CreatureAIState
    {
        None = -1,
        Idle = 0,
        Died,
        Invincible,
    }

    //public abstract void SetState(IAIState state);
    public abstract void SetGameObject(GameObject go);
    public abstract void SetAnimState(IAnimatorState state);
    public abstract void SetAttribute(ICreatureAttr attribute);
    public abstract void SetAI(ICreatureAI ai);
    public abstract void SetSkill(ISkill skill);

    protected abstract void OnInjured(short damage, bool myAttack);
    public abstract void Initialize();
    public abstract void Update();
    public abstract void Release();
    public abstract void OnHit();

    public virtual void Play(IAnimatorState.ENUM_AnimatorState state)
    {
        if (m_AnimState != null)
        {
            m_AnimState.Play(state,m_go);
            Debug.Log("ICreature Play:" + "   " + m_go.transform.parent.name + "   " + m_go.name + "  "+state.ToString());
        }
        else
        {
            Debug.Log("ICreature Play anim BUG");
        }
    }

    public IAnimatorState GetAminState()
    {
        return m_AnimState;
    }

    public ENUM_CreatureAIState GetAIState()
    {
        return (ENUM_CreatureAIState)m_AI.GetAIState();
    }

    public float GetSurvivalTime()
    {
        return m_AnimState.GetSurvivalTime();
    }

    public ICreatureAI GetAI()
    {
        return m_AI;
    }

    public ICreatureAttr GetAttribute()
    {
        return m_Attribute;
    }
}
