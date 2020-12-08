using UnityEngine;
using System.Collections;
using MPProtocol;

public class EndlessBattleAIState : IBattleAIState
{
    public EndlessBattleAIState(BattleAttr battleAttr)
        : base( battleAttr)
    { }

    public void Initialize()
    {
        stateAttr = new BattleAIStateAttr();
        stateAttr.spawnCount = 24;
        stateAttr.lerpTime = 0.075f;
        stateAttr.spawnTime = 0.25f;
        stateAttr.intervalTime = 2f;
        stateAttr.minStatus = 1;
        stateAttr.maxStatus = 3;
    }


    public override void UpdateState()
    {
        throw new System.NotImplementedException();
    }
}
