using UnityEngine;
using System.Collections;

public class CrossHorSpawnState : SpawnState
{
    public CrossHorSpawnState(float spawnIntervalTimes) : base(spawnIntervalTimes)
    {
    }

    public override IEnumerator Spawn(MiceSpawner spawner, string miceName, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool reSpawn)
    {
        spawnIntervalTime = 5.5f * spawnIntervalTimes;
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        Debug.Log("Cross Hor State");
        spawner.SpawnBy1D(miceName, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineHorA), spawnTime * spawnIntervalTimes, intervalTime, lerpTime, 3, -1, false, reSpawn);
        yield return new WaitForSeconds(.2f * spawnIntervalTimes);
        spawner.SpawnBy1D(miceName, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineHorC), spawnTime * spawnIntervalTimes, intervalTime, lerpTime, 3, -1, false, reSpawn);
        yield return new WaitForSeconds(1.2f * spawnIntervalTimes);
        spawner.SpawnBy1D(miceName, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineHorB), spawnTime * spawnIntervalTimes, intervalTime, lerpTime, 3, -1, false, !reSpawn);
        yield return new WaitForSeconds(.2f * spawnIntervalTimes);
        spawner.SpawnBy1D(miceName, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineHorD), spawnTime * spawnIntervalTimes, intervalTime, lerpTime, 3, -1, false, !reSpawn);
    
    }
}
