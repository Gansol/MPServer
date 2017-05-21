using UnityEngine;
using System.Collections;

public class CrossSpawnState : SpawnState
{
    public CrossSpawnState(float spawnIntervalTimes)
        : base(spawnIntervalTimes)
    {
    }

    public override IEnumerator Spawn(MPFactory spawner, short miceID, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool reSpawn)
    {
        spawnTime *= spawnIntervalTimes;
        spawnIntervalTime = 12f;
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        Debug.Log("Cross State");
        spawner.SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertA), spawnTime * .75f, intervalTime, lerpTime, 4, -1, false, !reSpawn);
        yield return new WaitForSeconds(1.5f * spawnIntervalTimes);
        spawner.SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertB), spawnTime * .75f, intervalTime, lerpTime, 4, -1, false, reSpawn);
        yield return new WaitForSeconds(1.5f * spawnIntervalTimes);
        spawner.SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertC), spawnTime * .75f, intervalTime, lerpTime, 4, -1, false, !reSpawn);
        yield return new WaitForSeconds(2f * spawnIntervalTimes);
        spawner.SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineHorD), spawnTime * .75f, intervalTime, lerpTime, 3, -1, false, !reSpawn);
        yield return new WaitForSeconds(1.2f * spawnIntervalTimes);
        spawner.SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineHorC), spawnTime / 2, intervalTime, lerpTime, 3, -1, false, reSpawn);
        yield return new WaitForSeconds(1.2f * spawnIntervalTimes);
        spawner.SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineHorB), spawnTime / 2, intervalTime, lerpTime, 3, -1, false, !reSpawn);
        yield return new WaitForSeconds(1.2f);
        spawner.SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineHorA), spawnTime / 2, intervalTime, lerpTime, 3, -1, false, reSpawn);
    }
}
