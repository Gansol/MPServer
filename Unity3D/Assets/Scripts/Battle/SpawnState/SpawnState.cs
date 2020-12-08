using UnityEngine;
using System.Collections;

public abstract class SpawnState
{
    protected float spawnIntervalTime;
    protected float spawnIntervalTimes;

    public abstract IEnumerator Spawn(short miceID, BattleAIStateAttr stateAttr, bool reSpawn);

    public SpawnState(float spawnIntervalTimes)
    {
        this.spawnIntervalTimes = spawnIntervalTimes;
    }

    public float GetIntervalTime()
    {
        return spawnIntervalTime;
    }
}
