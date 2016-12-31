using UnityEngine;
using System.Collections;

public class LoopCircleSpawnState : SpawnState {

    public LoopCircleSpawnState(float spawnIntervalTimes)
        : base(spawnIntervalTimes)
    {
    }

    public override IEnumerator Spawn(MiceSpawner spawner, string miceName, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool reSpawn)
    {
       // int startPos = reSpawn ? ((sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.CircleLD)).Length - 2 - 1 : -1y;

        spawnIntervalTime = 6f * spawnIntervalTimes;
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        Debug.Log("Loop Circle State");
        spawner.SpawnBy1D(miceName, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.CircleLD), spawnTime * spawnIntervalTimes, intervalTime, lerpTime, 10, 9, false, reSpawn); // 這裡respawn特殊 起始值會混亂不使用
        yield return new WaitForSeconds(1.5f * spawnIntervalTimes);
        spawner.SpawnBy1D(miceName, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineL), spawnTime * spawnIntervalTimes, intervalTime, lerpTime, 1, 4, false, reSpawn);
        yield return new WaitForSeconds(1.5f * spawnIntervalTimes);
        spawner.SpawnBy1D(miceName, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.CircleLD), spawnTime * spawnIntervalTimes, intervalTime, lerpTime, 10, 9, false, reSpawn);
        yield return new WaitForSeconds(1.5f * spawnIntervalTimes);
        spawner.SpawnBy1D(miceName, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineL), spawnTime * spawnIntervalTimes, intervalTime, lerpTime, 1, 7, false, reSpawn);
        yield return new WaitForSeconds(1.5f * spawnIntervalTimes);
    }
}
