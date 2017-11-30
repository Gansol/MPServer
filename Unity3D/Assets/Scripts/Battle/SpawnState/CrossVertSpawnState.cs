using UnityEngine;
using System.Collections;

public class CrossVertSpawnState : SpawnState
{
    public CrossVertSpawnState(float spawnIntervalTimes)
        : base(spawnIntervalTimes)
    {
    }

    public override IEnumerator Spawn(SpawnAI spawner, short miceID, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool reSpawn)
    {
        spawnIntervalTime = 5f * spawnIntervalTimes;
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        Debug.Log("CrossVert State");
        spawner.SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertA), spawnTime * spawnIntervalTimes, intervalTime, lerpTime, 4, -1, false, reSpawn);
        yield return new WaitForSeconds(0.2f * spawnIntervalTimes);
        spawner.SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertC), spawnTime * spawnIntervalTimes, intervalTime, lerpTime, 4, -1, false, reSpawn);
        yield return new WaitForSeconds(1.5f * spawnIntervalTimes);
        spawner.SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertB), spawnTime * spawnIntervalTimes, intervalTime, lerpTime, 4, -1, false, !reSpawn);
    }
}
