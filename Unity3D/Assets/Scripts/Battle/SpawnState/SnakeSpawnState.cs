using UnityEngine;
using System.Collections;

public class SnakeSpawnState : SpawnState
{
    public SnakeSpawnState(float spawnIntervalTimes)
        : base(spawnIntervalTimes)
    {
    }

    public override IEnumerator Spawn(SpawnAI spawner, short miceID, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool reSpawn)
    {
        spawnIntervalTime = 6f * spawnIntervalTimes;
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        Debug.Log("Snake State");
        spawner.SpawnBy1D(0, 1, miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineL), spawnTime / 2 * spawnIntervalTimes, intervalTime, lerpTime, false, reSpawn);
        yield return new WaitForSeconds(.5f * spawnIntervalTimes);
        spawner.SpawnBy1D(3, 4, miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineL), spawnTime / 2 * spawnIntervalTimes, intervalTime, lerpTime, false, reSpawn);
        yield return new WaitForSeconds(.5f * spawnIntervalTimes);
        spawner.SpawnBy1D(2, 3, miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LinkLineL), spawnTime / 2 * spawnIntervalTimes, intervalTime, lerpTime, false, reSpawn);

        yield return new WaitForSeconds(1f * spawnIntervalTimes);

        spawner.SpawnBy1D(7, 8, miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineL), spawnTime / 2 * spawnIntervalTimes, intervalTime, lerpTime, false, !reSpawn);
        yield return new WaitForSeconds(.5f * spawnIntervalTimes);
        spawner.SpawnBy1D(10, 11, miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineL), spawnTime / 2 * spawnIntervalTimes, intervalTime, lerpTime, false, !reSpawn);
        yield return new WaitForSeconds(.5f * spawnIntervalTimes);
        spawner.SpawnBy1D(2, 3, miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertA), spawnTime / 2 * spawnIntervalTimes, intervalTime, lerpTime, false, !reSpawn);
    }
}
