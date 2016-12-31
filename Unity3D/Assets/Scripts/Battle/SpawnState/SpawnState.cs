using UnityEngine;
using System.Collections;

public abstract class SpawnState : MonoBehaviour
{
    protected MiceSpawner spawner = null;
    protected float spawnIntervalTime;
    protected float spawnIntervalTimes;

    public abstract IEnumerator Spawn(MiceSpawner spawner, string miceName, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool reSpawn);

    public SpawnState(float spawnIntervalTimes)
    {
        this.spawnIntervalTimes = spawnIntervalTimes;
    }

    public float GetIntervalTime()
    {
        return spawnIntervalTime;
    }
}
