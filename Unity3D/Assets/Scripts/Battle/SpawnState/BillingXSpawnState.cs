using UnityEngine;
using System.Collections;

public class BillingXSpawnState : SpawnState
{

    public BillingXSpawnState(float spawnIntervalTimes)
        : base(spawnIntervalTimes)
    {
    }

    public override IEnumerator Spawn( short miceID, BattleAIStateAttr stateAttr, bool reSpawn)
    {
        stateAttr.spawnTime *= spawnIntervalTimes;
        spawnIntervalTime = 10f * spawnIntervalTimes;
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        Debug.Log("BillingX State  " + reSpawn);


        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.BevelL), stateAttr.spawnTime / 2 * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, 12, -1, false, reSpawn);
        yield return new WaitForSeconds((stateAttr.spawnTime * 12 * spawnIntervalTimes) + 1);
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.BevelR), stateAttr.spawnTime / 2 * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, 12, -1, false, reSpawn);
        yield return new WaitForSeconds((stateAttr.spawnTime * 12 * spawnIntervalTimes) + 1);
    }
}
