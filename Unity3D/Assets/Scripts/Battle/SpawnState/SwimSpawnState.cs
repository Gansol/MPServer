using UnityEngine;
using System.Collections;

public class SwimSpawnState : SpawnState
{
    public SwimSpawnState(float spawnIntervalTimes)
        : base(spawnIntervalTimes)
    {
    }

    public override IEnumerator Spawn(MiceSpawner spawner, string miceName, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool reSpawn)
    {
        spawnIntervalTime = 7f * spawnIntervalTimes;
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        Debug.Log("Swin State");
        spawner.SpawnBy1D(miceName, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineL), spawnTime * spawnIntervalTimes, intervalTime, lerpTime, 12, -1, false, reSpawn);
        yield return new WaitForSeconds(3f * spawnIntervalTimes);
        spawner.SpawnBy1D(miceName, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineL), spawnTime * spawnIntervalTimes, intervalTime, lerpTime, 12, -1, false, reSpawn);
        yield return new WaitForSeconds(4f * spawnIntervalTimes);
    }

}
