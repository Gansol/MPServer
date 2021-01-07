using UnityEngine;
using System.Collections;

public abstract class ICreatureAI  {
    protected IAIState m_AIState = null;
    protected ICreature m_Creature = null;



    public ICreatureAI(ICreature creature)
    {
        m_Creature = creature;
    }

    public virtual void UpdateAI(){
        m_AIState.Update();
    }

    public virtual void SetAIState(IAIState state)
    {
        m_AIState = state;
    }

    public int GetAIState()
    {
        return m_AIState.GetAIState();
    }
}
