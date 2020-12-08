using UnityEngine;
using System.Collections;

public class SnakeSpawnState : SpawnState
{
    public SnakeSpawnState(float spawnIntervalTimes)
        : base(spawnIntervalTimes)
    {
    }

    public override IEnumerator Spawn( short miceID, BattleAIStateAttr stateAttr, bool reSpawn)
    {
        spawnIntervalTime = 6f * spawnIntervalTimes;
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        Debug.Log("Snake State");
        MPGFactory.GetCreatureFactory().SpawnBy1D(0, 1, miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineL), stateAttr.spawnTime / 2 * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, false, reSpawn);
        yield return new WaitForSeconds(.5f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy1D(3, 4, miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineL), stateAttr.spawnTime / 2 * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, false, reSpawn);
        yield return new WaitForSeconds(.5f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy1D(2, 3, miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LinkLineL), stateAttr.spawnTime / 2 * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, false, reSpawn);

        yield return new WaitForSeconds(1f * spawnIntervalTimes);

        MPGFactory.GetCreatureFactory().SpawnBy1D(7, 8, miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineL),stateAttr.spawnTime / 2 * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, false, !reSpawn);
        yield return new WaitForSeconds(.5f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy1D(10, 11, miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineL), stateAttr.spawnTime / 2 * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, false, !reSpawn);
        yield return new WaitForSeconds(.5f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy1D(2, 3, miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertA), stateAttr.spawnTime / 2 * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, false, !reSpawn);
    }
}
