using UnityEngine;
using System.Collections;
using MPProtocol;

public abstract class SpawnState
{
    protected MiceSpawner spawner = GameObject.FindGameObjectWithTag("GM").GetComponent<MiceSpawner>();
    protected SpawnStatus spawnStatus = SpawnStatus.LineL;
    protected int spawnCount,minStatus,maxStatus;
    protected float lerpTime, spawnTime, intervalTime;
    protected Coroutine coroutine;

    public SpawnStatus GetSpawnStatus()
    {
        return spawnStatus;
    }

    /// <summary>
    /// 重複調用Spawn
    /// </summary>
    /// <param name="spawnStatus">產生方式</param>
    /// <param name="miceName">老鼠名稱</param>
    /// <param name="spawnTime">老鼠產生間隔</param>
    /// <param name="intervalTime">每次間隔</param>
    /// <param name="lerpTime">加速度</param>
    /// <param name="spawnCount">產生數量</param>
    /// <param name="isSkill">是否為技能調用</param>
    /// <param name="RepeatTime">多久調用一次</param>
    public abstract Coroutine Spawn(string miceName, bool isSkill);


    protected virtual SpawnStatus SelectStatus(int status)  // Select SpawnStatus 的副程式
    {
        switch (status)
        {
            case 0 :
                spawnStatus = SpawnStatus.Random;   // Random             
                break;
            case 1: // 1D形狀
                spawnStatus = (byte)10 + (SpawnStatus)Random.Range(0, 6) + 1;   // 1D                    
                break;
            case 2: // 2D形狀
                spawnStatus = (byte)100 + (SpawnStatus)Random.Range(0, 12) + 1;   // 2D                       
                break;
            case 3: // 自訂形狀
                spawnStatus = (byte)200 + (SpawnStatus)Random.Range(0, 6) + 1;   // Custom                       
                break;
            default:
                spawnStatus = SpawnStatus.Random;   // 1~6              
                break;
        }

        return spawnStatus;
    }


    /// <summary>
    /// 產生老鼠
    /// </summary>
    /// <param name="spawnStatus">產生方式</param>
    /// <param name="miceName">老鼠名稱</param>
    /// <param name="spawnTime">老鼠產生間隔</param>
    /// <param name="intervalTime">每次間隔</param>
    /// <param name="lerpTime">加速度</param>
    /// <param name="spawnCount">產生數量</param>
    /// <param name="isSkill">是否為技能調用</param>
    public virtual Coroutine SpawnCustom(SpawnStatus spawnStatus, string miceName, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool isSkill, bool reSpawn)
    {
        Random.seed = unchecked((int)System.DateTime.Now.Ticks);
        //bool reSpawn = System.Convert.ToBoolean(Random.Range(0, 1 + 1));

        // Random
        if (spawnStatus < SpawnStatus.SpawnData1D)
        {
            sbyte[] data = SpawnData.GetSpawnData(SpawnStatus.Random) as sbyte[];
            coroutine = spawner.SpawnByRandom(miceName, data, spawnTime, intervalTime, lerpTime, spawnCount, isSkill);
        }

        // SpawnBy1D
        else if (spawnStatus > SpawnStatus.SpawnData1D && spawnStatus < SpawnStatus.SpawnData2D)
        {
            sbyte[] data = SpawnData.GetSpawnData(spawnStatus) as sbyte[];
            coroutine = spawner.SpawnBy1D(miceName, data, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, data.Length), isSkill, reSpawn);
        }

        // SpawnBy2D
        else if (spawnStatus > SpawnStatus.SpawnData2D && spawnStatus < SpawnStatus.SpawnDataCustom)
        {
            sbyte[,] data = SpawnData.GetSpawnData(spawnStatus) as sbyte[,];
            coroutine = spawner.SpawnBy2D(miceName, data, spawnTime, intervalTime, lerpTime, spawnCount, new Vector2(Random.Range(0, data.GetLength(0)), Random.Range(0, data.GetLength(1))), isSkill, reSpawn);
        }

        // SpawnDataCustom
        else if (spawnStatus > SpawnStatus.SpawnDataCustom)
        {
            sbyte[][] data = SpawnData.GetSpawnData(spawnStatus) as sbyte[][];
            coroutine = spawner.SpawnByCustom(miceName, data, spawnTime, intervalTime, lerpTime, spawnCount, isSkill, reSpawn);
        }
        else
        {
            Debug.LogError("Unknown Spawn Data !");
        }

        return coroutine;
    }
}
