using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ByeByeAIState : IAIState
{

    public ByeByeAIState()
    {
        Debug.Log("ByeBye State");
        m_CreatureAI.Set_ENUM_AIState(ICreature.ENUM_CreatureState.ByeBye);
    }



    public override void Update()
    {

    }
}
