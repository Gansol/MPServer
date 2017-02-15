using UnityEngine;
using System.Collections;

public class FeatherSpawnState : SpawnState
{

    public FeatherSpawnState(float spawnIntervalTimes)
        : base(spawnIntervalTimes)
    {
    }

    public override IEnumerator Spawn(MPFactory spawner, short miceID, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool reSpawn)
    {
        spawnIntervalTime = 6f * spawnIntervalTimes;
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        Debug.Log("Feather State");
        spawner.SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertB), spawnTime * 1.05f * spawnIntervalTimes, intervalTime, lerpTime, 4, -1, false, reSpawn);
        yield return new WaitForSeconds(.66f * spawnIntervalTimes);
        spawner.SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertA), spawnTime * 1.05f * spawnIntervalTimes, intervalTime, lerpTime, 4, -1, false, reSpawn);
        spawner.SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertC), spawnTime * 1.05f * spawnIntervalTimes, intervalTime, lerpTime, 4, -1, false, reSpawn);
    }
}
