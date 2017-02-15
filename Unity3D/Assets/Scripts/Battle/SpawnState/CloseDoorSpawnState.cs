using UnityEngine;
using System.Collections;

public class CloseDoorSpawnState : SpawnState
{
    public CloseDoorSpawnState(float spawnIntervalTimes)
        : base(spawnIntervalTimes)
    {
    }

    public override IEnumerator Spawn(MPFactory spawner, short miceID, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool reSpawn)
    {
        spawnIntervalTime = 6f * spawnIntervalTimes;
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        Debug.Log("CloseDoor State: " + reSpawn);
        //spawner.SpawnByCustom(miceName, (sbyte[][])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.TriangleLD), spawnTime * spawnIntervalTimes, intervalTime, lerpTime, 6, new Vector2(-1, -1), false, false);
        //yield return new WaitForSeconds(.5f * spawnIntervalTimes);
        //spawner.SpawnByCustom(miceName, (sbyte[][])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.TriangleRU), spawnTime * spawnIntervalTimes, intervalTime, lerpTime, 6, new Vector2(-1, -1), false, false);


        spawner.SpawnBy1D(0, 2, miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertA), spawnTime / 2 * spawnIntervalTimes, intervalTime, lerpTime, false, false);
        spawner.SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertC), spawnTime / 2 * spawnIntervalTimes, intervalTime, lerpTime, 3, 3, false, true);
        yield return new WaitForSeconds(.75f * spawnIntervalTimes);
        spawner.SpawnBy1D(0, 1, miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertB), spawnTime / 2 * spawnIntervalTimes, intervalTime, lerpTime, false, false);
        spawner.SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertB), spawnTime / 2 * spawnIntervalTimes, intervalTime, lerpTime, 2, 3, false, true);
        yield return new WaitForSeconds(.5f * spawnIntervalTimes);
        spawner.SpawnBy1D(0, 0, miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertC), spawnTime / 2 * spawnIntervalTimes, intervalTime, lerpTime, false, false);
        spawner.SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertA), spawnTime / 2 * spawnIntervalTimes, intervalTime, lerpTime, 1, 3, false, true);
        yield return new WaitForSeconds(1f * spawnIntervalTimes);


        yield return new WaitForSeconds(1f * spawnIntervalTimes);

        yield return new WaitForSeconds(.67f * spawnIntervalTimes);

        yield return new WaitForSeconds(1f * spawnIntervalTimes);

    }
}
