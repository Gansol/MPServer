using UnityEngine;
using System.Collections;

public class InvincibleAIState : IAIState
{
    public InvincibleAIState(/*ICreatureAI creatureAI) : base(creatureAI*/)
    {
        Debug.Log("InvincibleAIState Initialize");
        creatureAIState = ICreature.ENUM_CreatureAIState.Invincible;
    }

    public override void Update()
    {
        Debug.Log("InvincibleAIState Update");
    }
}
