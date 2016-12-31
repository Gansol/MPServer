using UnityEngine;
using System.Collections;

public class FeatherSpawnState : SpawnState {

    public FeatherSpawnState(float spawnIntervalTimes)
        : base(spawnIntervalTimes)
    {
    }

    public override IEnumerator Spawn(MiceSpawner spawner, string miceName, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool reSpawn)
    {
        spawnIntervalTime = 4f * spawnIntervalTimes;
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        Debug.Log("Feather State");
        spawner.SpawnBy1D(miceName, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertB), spawnTime * spawnIntervalTimes, intervalTime, lerpTime, 4, -1, false, reSpawn);
        yield return new WaitForSeconds(.5f * spawnIntervalTimes);
        spawner.SpawnBy1D(miceName, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertA), spawnTime * spawnIntervalTimes, intervalTime, lerpTime, 4, -1, false, reSpawn);
        spawner.SpawnBy1D(miceName, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertC), spawnTime * spawnIntervalTimes, intervalTime, lerpTime, 4, -1, false, reSpawn);
    }
}
