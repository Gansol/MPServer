using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiedAIState : IAIState
{

    public DiedAIState()
    {
        Debug.Log("Died State");
        m_CreatureAI.Set_ENUM_AIState(ICreature.ENUM_CreatureState.Die);
    }



    public override void Update()
    {

    }
}
