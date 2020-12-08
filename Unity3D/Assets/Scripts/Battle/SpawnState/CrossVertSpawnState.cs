using UnityEngine;
using System.Collections;

public class CrossVertSpawnState : SpawnState
{
    public CrossVertSpawnState(float spawnIntervalTimes)
        : base(spawnIntervalTimes)
    {
    }

    public override IEnumerator Spawn(short miceID, BattleAIStateAttr stateAttr, bool reSpawn)
    {
        spawnIntervalTime = 5f * spawnIntervalTimes;
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        Debug.Log("CrossVert State");
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertA), stateAttr.spawnTime * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, 4, -1, false, reSpawn);
        yield return new WaitForSeconds(0.2f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertC), stateAttr.spawnTime * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, 4, -1, false, reSpawn);
        yield return new WaitForSeconds(1.5f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertB), stateAttr.spawnTime * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, 4, -1, false, !reSpawn);
    }
}
