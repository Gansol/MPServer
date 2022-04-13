using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiedAIState : IAIState
{

    public DiedAIState(/*ICreatureAI creatureAI) : base(creatureAI*/)
    {
        Debug.Log("Died State");
        creatureAIState = ICreature.ENUM_CreatureAIState.Died;
    }



    public override void Update()
    {
     //   Debug.Log("Died State");
    }
}
