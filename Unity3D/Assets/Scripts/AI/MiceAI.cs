using UnityEngine;
using System.Collections;

public class MiceAI : ICreatureAI {

    void Start()
    {
        State = new IdleAIState();
    }

    public override void UpdateAIState(AIState state)
    {
        base.UpdateAIState(state);
    }
}
