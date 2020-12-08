using UnityEngine;
using System.Collections;

public class CrossSpawnState : SpawnState
{
    public CrossSpawnState(float spawnIntervalTimes)
        : base(spawnIntervalTimes)
    {
    }

    public override IEnumerator Spawn(short miceID, BattleAIStateAttr stateAttr, bool reSpawn)
    {
        stateAttr.spawnTime *= spawnIntervalTimes;
        spawnIntervalTime = 12f;
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        Debug.Log("Cross State");
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertA), stateAttr.spawnTime * .75f, stateAttr.intervalTime, stateAttr.lerpTime, 4, -1, false, !reSpawn);
        yield return new WaitForSeconds(1.5f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertB), stateAttr.spawnTime * .75f, stateAttr.intervalTime, stateAttr.lerpTime, 4, -1, false, reSpawn);
        yield return new WaitForSeconds(1.5f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertC), stateAttr.spawnTime * .75f, stateAttr.intervalTime, stateAttr.lerpTime, 4, -1, false, !reSpawn);
        yield return new WaitForSeconds(2f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineHorD), stateAttr.spawnTime * .75f, stateAttr.intervalTime, stateAttr.lerpTime, 3, -1, false, !reSpawn);
        yield return new WaitForSeconds(1.2f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineHorC), stateAttr.spawnTime / 2, stateAttr.intervalTime, stateAttr.lerpTime, 3, -1, false, reSpawn);
        yield return new WaitForSeconds(1.2f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineHorB), stateAttr.spawnTime / 2, stateAttr.intervalTime, stateAttr.lerpTime, 3, -1, false, !reSpawn);
        yield return new WaitForSeconds(1.2f);
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineHorA), stateAttr.spawnTime / 2, stateAttr.intervalTime, stateAttr.lerpTime, 3, -1, false, reSpawn);
    }
}
