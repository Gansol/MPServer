using UnityEngine;
using System.Collections;

public class ICreatureAI  {
    protected AIState State = null;

    public virtual void UpdateAIState(AIState state){
        this.State = state;
    }
}
