using UnityEngine;
using System.Collections;

public class SwimSpawnState : SpawnState
{
    public SwimSpawnState(float spawnIntervalTimes)
        : base(spawnIntervalTimes)
    {
    }

    public override IEnumerator Spawn(short miceID,BattleAIStateAttr stateAttr, bool reSpawn)
    {
        spawnIntervalTime = 10f * spawnIntervalTimes;
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        Debug.Log("Swin State");
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineL), stateAttr.spawnTime * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, 12, -1, false, reSpawn);
        yield return new WaitForSeconds(3f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineL), stateAttr.spawnTime * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, 12, -1, false, reSpawn);
        yield return new WaitForSeconds(4f * spawnIntervalTimes);
    }

}
