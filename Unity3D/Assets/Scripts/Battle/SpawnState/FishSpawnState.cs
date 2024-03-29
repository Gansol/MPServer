﻿using UnityEngine;
using System.Collections;

public class FishSpawnState : SpawnState
{
    public FishSpawnState(float spawnIntervalTimes)
        : base(spawnIntervalTimes)
    {
    }

    public override IEnumerator Spawn( short miceID, BattleAIStateAttr stateAttr, bool reSpawn)
    {
        spawnIntervalTime = 6f * spawnIntervalTimes;
        yield return new WaitForSeconds(1f * spawnIntervalTimes);
        Debug.Log("Fish State");
        MPGFactory.GetCreatureFactory().SpawnBy2D(new Vector2(0, 0), new Vector2(0, 1), miceID, (sbyte[,])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.HorizontalD), stateAttr. spawnTime * .75f * spawnIntervalTimes, stateAttr. intervalTime, stateAttr.lerpTime, new Vector2(-1, -1), false, reSpawn);
        yield return new WaitForSeconds(.75f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy2D(new Vector2(1, 0), new Vector2(1, 1), miceID, (sbyte[,])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.HorizontalD), stateAttr. spawnTime * .75f * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, new Vector2(-1, -1), false, reSpawn);
        yield return new WaitForSeconds(.75f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy2D(new Vector2(2, 0), new Vector2(2, 1), miceID, (sbyte[,])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.HorizontalD), stateAttr. spawnTime * .75f * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, new Vector2(-1, -1), false, reSpawn);

        yield return new WaitForSeconds(1f * spawnIntervalTimes);

        MPGFactory.GetCreatureFactory().SpawnBy2D(new Vector2(0, 2), new Vector2(0, 3), miceID, (sbyte[,])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.HorizontalD), stateAttr. spawnTime * .75f * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, new Vector2(-1, -1), false, reSpawn);
        yield return new WaitForSeconds(.75f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy2D(new Vector2(1, 2), new Vector2(1, 3), miceID, (sbyte[,])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.HorizontalD), stateAttr.spawnTime * .75f * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, new Vector2(-1, -1), false, reSpawn);
        yield return new WaitForSeconds(.75f * spawnIntervalTimes);
        MPGFactory.GetCreatureFactory().SpawnBy2D(new Vector2(2, 2), new Vector2(2, 3), miceID, (sbyte[,])SpawnData.GetSpawnData(MPProtocol.SpawnStatus.HorizontalD), stateAttr.spawnTime * .75f * spawnIntervalTimes, stateAttr.intervalTime, stateAttr.lerpTime, new Vector2(-1, -1), false, reSpawn);
        yield return new WaitForSeconds(.75f * spawnIntervalTimes);
    }
}
