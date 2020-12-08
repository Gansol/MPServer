using UnityEngine;
using System.Collections;

public class CrossHorSpawnState : SpawnState
{
    public CrossHorSpawnState(float spawnIntervalTimes) : base(spawnIntervalTimes)
    {
    }

    public override IEnumerator Spawn(short miceID,BattleAIStateAttr stateAttr, bool reSpawn)
    {
        spawnIntervalTime = 6f * spawnIntervalTimes;
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        Debug.Log("Cross Hor State");
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineHorA), stateAttr.spawnTime * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, 3, -1, false, reSpawn);
        yield return new WaitForSeconds(.2f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineHorC), stateAttr.spawnTime * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, 3, -1, false, reSpawn);
        yield return new WaitForSeconds(1.5f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineHorB), stateAttr.spawnTime * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, 3, -1, false, !reSpawn);
        yield return new WaitForSeconds(.2f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineHorD), stateAttr.spawnTime * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, 3, -1, false, !reSpawn);
    
    }
}
