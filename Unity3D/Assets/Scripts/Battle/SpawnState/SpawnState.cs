﻿using UnityEngine;
using System.Collections;

public abstract class SpawnState
{
    protected MPFactory spawner = null;
    protected float spawnIntervalTime;
    protected float spawnIntervalTimes;

    public abstract IEnumerator Spawn(MPFactory spawner, short miceID, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool reSpawn);

    public SpawnState(float spawnIntervalTimes)
    {
        this.spawnIntervalTimes = spawnIntervalTimes;
    }

    public float GetIntervalTime()
    {
        return spawnIntervalTime;
    }
}