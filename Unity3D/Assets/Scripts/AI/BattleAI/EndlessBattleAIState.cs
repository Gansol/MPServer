using UnityEngine;
using System.Collections;
using MPProtocol;

public class EndlessBattleAIState : IBattleAIState
{
    public void Initialize()
    {
        spawnCount = 24;
        lerpTime = 0.075f;
        spawnTime = 0.25f;
        intervalTime = 2f;
        minStatus = 1;
        maxStatus = 3;
    }


    public override void UpdateState()
    {
        throw new System.NotImplementedException();
    }
}
