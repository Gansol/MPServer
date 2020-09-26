using UnityEngine;
using System.Collections;

public class AIState  {

   protected ICreatureAI CreatureAI = null;
    // 設定CharacterAI的對像
    public void SetCreatureAI(ICreatureAI creatureAI)
    {
        this.CreatureAI = creatureAI;
    }
}
