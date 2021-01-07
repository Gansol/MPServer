using UnityEngine;
using System.Collections;

public class IdleAIState : IAIState
{
    public IdleAIState(/*ICreatureAI creatureAI) : base(creatureAI*/)
    {
        Debug.Log("Idle State");
        creatureAIState = ICreature.ENUM_CreatureAIState.Idle;
    }

    public override void Update()
    {
        Debug.Log("Idle Update");
    }
}
