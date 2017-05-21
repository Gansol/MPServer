using UnityEngine;
using System.Collections;

public class BillingHVSpawnState : SpawnState
{

    public BillingHVSpawnState(float spawnIntervalTimes)
        : base(spawnIntervalTimes)
    {
    }

    public override IEnumerator Spawn(MPFactory spawner, short miceID, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool reSpawn)
    {
        spawnTime *= spawnIntervalTimes;
        spawnIntervalTime = 10f * spawnIntervalTimes;
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        Debug.Log("Cross State  " + reSpawn);
        spawner.SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertA), spawnTime * .66f, intervalTime, lerpTime, 4, 3, false, !reSpawn);
        yield return new WaitForSeconds(1.5f * spawnIntervalTimes);
        spawner.SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineHorD), spawnTime * .75f, intervalTime, lerpTime, 3, 2, false, !reSpawn);
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        spawner.SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertB), spawnTime * .66f, intervalTime, lerpTime, 4, -1, false, reSpawn);
        yield return new WaitForSeconds(1.5f * spawnIntervalTimes);
        spawner.SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineHorC), spawnTime * .75f, intervalTime, lerpTime, 3, -1, false, reSpawn);
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        spawner.SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertC), spawnTime * .66f, intervalTime, lerpTime, 4, 3, false, !reSpawn);
        yield return new WaitForSeconds(1.5f * spawnIntervalTimes);
        spawner.SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineHorB), spawnTime * .75f, intervalTime, lerpTime, 3, 2, false, !reSpawn);
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        spawner.SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineHorA), spawnTime * .75f, intervalTime, lerpTime, 3, -1, false, reSpawn);
        yield return new WaitForSeconds(.5f * spawnIntervalTimes);

    }
}
