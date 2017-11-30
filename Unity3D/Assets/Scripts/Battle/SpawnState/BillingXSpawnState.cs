using UnityEngine;
using System.Collections;

public class BillingXSpawnState : SpawnState
{

    public BillingXSpawnState(float spawnIntervalTimes)
        : base(spawnIntervalTimes)
    {
    }

    public override IEnumerator Spawn(SpawnAI spawner, short miceID, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool reSpawn)
    {
        spawnTime *= spawnIntervalTimes;
        spawnIntervalTime = 10f * spawnIntervalTimes;
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        Debug.Log("BillingX State  " + reSpawn);


        spawner.SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.BevelL), spawnTime / 2 * spawnIntervalTimes, intervalTime, lerpTime, 12, -1, false, reSpawn);
        yield return new WaitForSeconds((spawnTime * 12 * spawnIntervalTimes) + 1);
        spawner.SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.BevelR), spawnTime / 2 * spawnIntervalTimes, intervalTime, lerpTime, 12, -1, false, reSpawn);
        yield return new WaitForSeconds((spawnTime * 12 * spawnIntervalTimes) + 1);
    }
}
