using UnityEngine;
using System.Collections;

public abstract class IAIState
{

    protected ICreatureAI m_CreatureAI = null;
    protected ICreature.ENUM_CreatureAIState creatureAIState = ICreature.ENUM_CreatureAIState.None;


    public IAIState(/*ICreatureAI creatureAI*/)
    {
      // m_CreatureAI = creatureAI;
    }
    public abstract void Update();

    // 設定CharacterAI的對像
    public virtual void SetCreatureAI(ICreatureAI creatureAI)
    {
        m_CreatureAI = creatureAI;
    }

    public virtual void ApplyEffect(IAIState state)
    {
        m_CreatureAI.SetAIState(state);
    }

    public virtual void Release()
    {
        m_CreatureAI = null;
    }

    public int GetAIState()
    {
        return (int)creatureAIState;
    }
}
