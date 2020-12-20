using UnityEngine;
using System.Collections;

public class IdleAIState : IAIState
{
    public IdleAIState()
    {
        Debug.Log( "Idle State");
        m_CreatureAI.Set_ENUM_AIState(ICreature.ENUM_CreatureState.Idle);
    }

    public override void Update()
    {
        Debug.Log("Idle Update");
    }
}
