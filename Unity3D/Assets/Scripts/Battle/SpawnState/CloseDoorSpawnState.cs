using UnityEngine;
using System.Collections;

public class CloseDoorSpawnState : SpawnState
{
    public CloseDoorSpawnState(float spawnIntervalTimes)
        : base(spawnIntervalTimes)
    {
    }

    public override IEnumerator Spawn(short miceID, BattleAIStateAttr stateAttr, bool reSpawn)
    {
        spawnIntervalTime = 6f * spawnIntervalTimes;
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        Debug.Log("CloseDoor State: " + reSpawn);
        //spawner.SpawnByCustom(miceName, (sbyte[][])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.TriangleLD), spawnTime * spawnIntervalTimes, intervalTime, lerpTime, 6, new Vector2(-1, -1), false, false);
        //yield return new WaitForSeconds(.5f * spawnIntervalTimes);
        //spawner.SpawnByCustom(miceName, (sbyte[][])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.TriangleRU), spawnTime * spawnIntervalTimes, intervalTime, lerpTime, 6, new Vector2(-1, -1), false, false);


       MPGFactory.GetCreatureFactory().SpawnBy1D( 0, 2, miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertA), stateAttr.spawnTime / 2 * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, false, false);
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertC), stateAttr.spawnTime / 2 * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, 3, 3, false, true);
        yield return new WaitForSeconds(.75f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy1D(0, 1, miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertB), stateAttr.spawnTime / 2 * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, false, false);
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertB), stateAttr.spawnTime / 2 * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, 2, 3, false, true);
        yield return new WaitForSeconds(.5f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy1D(0, 0, miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertC), stateAttr.spawnTime / 2 * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, false, false);
        MPGFactory.GetCreatureFactory().SpawnBy1D(miceID, (sbyte[])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineVertA), stateAttr.spawnTime / 2 * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, 1, 3, false, true);
        yield return new WaitForSeconds(1f * spawnIntervalTimes);


        yield return new WaitForSeconds(1f * spawnIntervalTimes);

        yield return new WaitForSeconds(.67f * spawnIntervalTimes);

        yield return new WaitForSeconds(1f * spawnIntervalTimes);

    }
}
