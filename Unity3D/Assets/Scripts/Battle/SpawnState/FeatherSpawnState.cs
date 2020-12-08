using UnityEngine;
using System.Collections;

public class FeatherSpawnState : SpawnState
{

    public FeatherSpawnState(float spawnIntervalTimes)
        : base(spawnIntervalTimes)
    {
    }

    public override IEnumerator Spawn( short miceID,BattleAIStateAttr stateAttr, bool reSpawn)
    {
        spawnIntervalTime = 6f * spawnIntervalTimes;
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        Debug.Log("Feather State");
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertB), stateAttr.spawnTime * 1.05f * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, 4, -1, false, reSpawn);
        yield return new WaitForSeconds(.66f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertA), stateAttr.spawnTime * 1.05f * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, 4, -1, false, reSpawn);
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertC), stateAttr.spawnTime * 1.05f * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, 4, -1, false, reSpawn);
    }
}
