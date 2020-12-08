using UnityEngine;
using System.Collections;

public class LoopCircleSpawnState : SpawnState {

    public LoopCircleSpawnState(float spawnIntervalTimes)
        : base(spawnIntervalTimes)
    {
    }

    public override IEnumerator Spawn(short miceID, BattleAIStateAttr stateAttr, bool reSpawn)
    {
       // int startPos = reSpawn ? ((sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.CircleLD)).Length - 2 - 1 : -1y;

        spawnIntervalTime = 10f * spawnIntervalTimes;
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        Debug.Log("Loop Circle State");
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.CircleLD), stateAttr.spawnTime * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, 10, 9, false, reSpawn); // 這裡respawn特殊 起始值會混亂不使用
        yield return new WaitForSeconds(1.5f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineL), stateAttr.spawnTime * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, 1, 4, false, reSpawn);
        yield return new WaitForSeconds(2f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.CircleLD), stateAttr.spawnTime * spawnIntervalTimes ,stateAttr.intervalTime, stateAttr.lerpTime, 10, 9, false, reSpawn);
        yield return new WaitForSeconds(1.5f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineL), stateAttr.spawnTime * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, 1, 7, false, reSpawn);
        yield return new WaitForSeconds(2f * spawnIntervalTimes);
    }
}
