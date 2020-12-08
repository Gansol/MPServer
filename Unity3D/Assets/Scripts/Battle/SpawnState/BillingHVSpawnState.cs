using UnityEngine;
using System.Collections;

public class BillingHVSpawnState : SpawnState
{

    public BillingHVSpawnState(float spawnIntervalTimes)
        : base(spawnIntervalTimes)
    {
    }

    public override IEnumerator Spawn(short miceID, BattleAIStateAttr stateAttr, bool reSpawn)
    {
        stateAttr.spawnTime *= spawnIntervalTimes;
        spawnIntervalTime = 10f * spawnIntervalTimes;
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        Debug.Log("Cross State  " + reSpawn);
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertA), stateAttr.spawnTime * .66f, stateAttr.intervalTime, stateAttr.lerpTime, 4, 3, false, !reSpawn);
        yield return new WaitForSeconds(1.5f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineHorD), stateAttr.spawnTime * .75f, stateAttr.intervalTime, stateAttr.lerpTime, 3, 2, false, !reSpawn);
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertB), stateAttr.spawnTime * .66f, stateAttr.intervalTime, stateAttr.lerpTime, 4, -1, false, reSpawn);
        yield return new WaitForSeconds(1.5f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineHorC), stateAttr.spawnTime * .75f, stateAttr.intervalTime, stateAttr.lerpTime, 3, -1, false, reSpawn);
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertC), stateAttr.spawnTime * .66f, stateAttr.intervalTime, stateAttr.lerpTime, 4, 3, false, !reSpawn);
        yield return new WaitForSeconds(1.5f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineHorB), stateAttr.spawnTime * .75f, stateAttr.intervalTime, stateAttr.lerpTime, 3, 2, false, !reSpawn);
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineHorA), stateAttr.spawnTime * .75f, stateAttr.intervalTime, stateAttr.lerpTime, 3, -1, false, reSpawn);
        yield return new WaitForSeconds(.5f * spawnIntervalTimes);

    }
}
