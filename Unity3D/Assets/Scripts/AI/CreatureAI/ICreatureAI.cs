using UnityEngine;
using System.Collections;

public abstract class ICreatureAI  {
    protected IAIState m_AIState = null;
    protected ICreature m_Creature = null;

    public ICreatureAI(ICreature creature)
    {
        m_Creature = creature;
    }

    public virtual void UpdateAIState(){
        m_AIState.Update();
    }

    public virtual void SetAIState(IAIState state)
    {
        m_AIState = state;
    }

    public virtual void Set_ENUM_AIState(ICreature.ENUM_CreatureState enum_state)
    {
        m_Creature.SetState(enum_state);
    }
}
