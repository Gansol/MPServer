using UnityEngine;
using System.Collections;

public class STwinSpawnState : SpawnState
{
    public STwinSpawnState(float spawnIntervalTimes)
        : base(spawnIntervalTimes)
    {
    }

    public override IEnumerator Spawn(SpawnAI spawner, short miceID, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool reSpawn)
    {
        spawnIntervalTime = 6f * spawnIntervalTimes;
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        Debug.Log("STwin State");
        spawner.SpawnBy1D(0, 5, miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.STwin), spawnTime * spawnIntervalTimes, intervalTime, lerpTime, false, reSpawn);
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        spawner.SpawnBy1D(6, 11, miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.STwin), spawnTime * spawnIntervalTimes, intervalTime, lerpTime, false, !reSpawn);
        yield return new WaitForSeconds(2f * spawnIntervalTimes);
    }
}
