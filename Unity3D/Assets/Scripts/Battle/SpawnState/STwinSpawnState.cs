using UnityEngine;
using System.Collections;

public class STwinSpawnState : SpawnState
{
    public STwinSpawnState(float spawnIntervalTimes)
        : base(spawnIntervalTimes)
    {
    }

    public override IEnumerator Spawn( short miceID, BattleAIStateAttr stateAttr, bool reSpawn)
    {
        spawnIntervalTime = 6f * spawnIntervalTimes;
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        Debug.Log("STwin State");
        MPGFactory.GetCreatureFactory().SpawnBy1D(0, 5, miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.STwin), stateAttr.spawnTime * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, false, reSpawn);
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy1D(6, 11, miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.STwin), stateAttr.spawnTime * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, false, !reSpawn);
        yield return new WaitForSeconds(2f * spawnIntervalTimes);
    }
}
