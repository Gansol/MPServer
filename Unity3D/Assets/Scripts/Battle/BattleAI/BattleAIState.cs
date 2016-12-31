﻿using UnityEngine;
using System.Collections;
using MPProtocol;

public abstract class BattleAIState
{
    protected static float spawnOffset = 0f;    // SpawnTime修正值
    protected static double lastTime = 0d;
    protected BattleManager battleManager = null;
    protected MiceSpawner spawner = null;
    protected SpawnState spawnState = null;
    protected SpawnStatus spawnStatus = SpawnStatus.LineL;
    protected int spawnCount, minStatus, maxStatus, minMethod, maxMethod, normalSpawn, nowCombo;          // 產生數量、老鼠產生資料最小值、老鼠產生資料最大值、正常產生狀態值、更換AI狀態後目前combo
    protected float lerpTime, spawnTime, intervalTime, spawnIntervalTime, intervalOffset = .05f, minSpawnInterval, maxSpawnInterval,spawnSpeed; // 加速度、老鼠產生間隔、每組老鼠產生間隔、Spawn難度修正值、每次Spawn間隔、最大、最小間隔
    
    protected Coroutine coroutine;

    public abstract void UpdateState();

    public SpawnStatus GetSpawnStatus()
    {
        return spawnStatus;
    }

    public void SetAIController(BattleManager battleManager)
    {
        this.battleManager = battleManager;
    }

    /// <summary>
    /// 產生老鼠
    /// </summary>
    /// <param name="miceName">老鼠名稱</param>
    /// <returns>routine</returns>
    protected virtual Coroutine Spawn(string miceName)
    {
        //        Debug.Log(Time.time);
        Random.seed = unchecked((int)System.DateTime.Now.Ticks);
        bool reSpawn = System.Convert.ToBoolean(Random.Range(0, 1 + 1));
        spawner = GameObject.FindGameObjectWithTag("GM").GetComponent<MiceSpawner>();
        if (spawner != null)
            coroutine = spawner.Spawn(new Vector2(minStatus, maxStatus), miceName, spawnTime, intervalTime, lerpTime, spawnCount, true, false, reSpawn);
        else
            Debug.Log("Spawn Spawner is null!!!!!!!!!!!!!!!!!!!");
        return coroutine;
    }

    /// <summary>
    /// 產生特別狀態老鼠
    /// </summary>
    /// <param name="miceName">老鼠名稱</param>
    /// <param name="intervalTimes">間格倍率</param>
    /// <returns></returns>
    protected virtual Coroutine SpawnSpecial(int spawnValue, string miceName, float intervalTimes)
    {
        spawnState = SelectSpawnState(spawnValue, intervalTimes);
        Random.seed = unchecked((int)System.DateTime.Now.Ticks);
        bool reSpawn = System.Convert.ToBoolean(Random.Range(0, 1 + 1));
        spawner = GameObject.FindGameObjectWithTag("GM").GetComponent<MiceSpawner>();

        if (spawner != null)
            coroutine = spawner.SpawnSpecial(spawnState, miceName, spawnTime, intervalTime, lerpTime, spawnCount, reSpawn);
        else
            Debug.Log("SpawnSpecial Spawner is null!!!!!!!!!!!!!!!!!!!  " + this);
        return coroutine;
    }

    public void SetValue(float lerpTime, float spawnTime, float intervalTime, int spawnCount)
    {
        if (lerpTime > 0) this.lerpTime = lerpTime;
        if (spawnTime > 0) this.spawnTime = spawnTime;
        if (intervalTime > 0) this.intervalTime = intervalTime;
        if (spawnCount > 0) this.spawnCount = spawnCount;
    }

    /// <summary>
    /// 根據combo動態設定SpawnIntervalTime
    /// </summary>
    public virtual void SetSpawnIntervalTime()
    {
        float offset;

        offset = (battleManager.isCombo) ? -intervalOffset : intervalOffset * 5;

        if (spawnOffset + offset >= minSpawnInterval && spawnOffset + offset <= maxSpawnInterval)
            spawnOffset += offset;
    }

    private SpawnState SelectSpawnState(int spawnValue, float intervalTimes)
    {
        intervalTimes = (intervalTimes <= 0) ? 1 : intervalTimes;

        switch (spawnValue)
        {
            case (int)ENUM_SpawnMethod.CrossHor:
                {
                    spawnState = new CrossHorSpawnState(intervalTimes);
                    break;
                }
            case (int)ENUM_SpawnMethod.CrossVert:
                {
                    spawnState = new CrossVertSpawnState(intervalTimes);
                    break;
                }
            case (int)ENUM_SpawnMethod.Feather:
                {
                    spawnState = new FeatherSpawnState(intervalTimes);
                    break;
                }
            case (int)ENUM_SpawnMethod.Fish:
                {
                    spawnState = new FishSpawnState(intervalTimes);
                    break;
                }
            case (int)ENUM_SpawnMethod.Snake:
                {
                    spawnState = new SnakeSpawnState(intervalTimes);
                    break;
                }
            case (int)ENUM_SpawnMethod.Door:
                {
                    spawnState = new CloseDoorSpawnState(intervalTimes);
                    break;
                }
            case (int)ENUM_SpawnMethod.STwin:
                {
                    spawnState = new STwinSpawnState(intervalTimes);
                    break;
                }
            case (int)ENUM_SpawnMethod.Swim:
                {
                    spawnState = new SwimSpawnState(intervalTimes);
                    break;
                }
            case (int)ENUM_SpawnMethod.LoopCricle:
                {
                    spawnState = new LoopCircleSpawnState(intervalTimes);
                    break;
                }
            case (int)ENUM_SpawnMethod.Cross:
                {
                    spawnState = new CrossSpawnState(intervalTimes);
                    break;
                }
            case (int)ENUM_SpawnMethod.BillingHV:
                {
                    spawnState = new BillingHVSpawnState(intervalTimes);
                    break;
                }
            case (int)ENUM_SpawnMethod.BillingX:
                {
                    spawnState = new BillingXSpawnState(intervalTimes);
                    break;
                }
            default:
                Debug.Log("!!!!!!!!!!!!!!!!!!!!!! Error value : " + spawnValue);
                spawnState = new CrossHorSpawnState(intervalTimes);
                break;
        }
        return spawnState;
    }

    public float GetLerpTime()
    {
        return lerpTime;
    }

    public float GetSpawnTime()
    {
        return spawnTime;
    }

    public float GetIntervalTime()
    {
        return intervalTime;
    }

    public int GetSpawnCount()
    {
        return spawnCount;
    }

    public float GetSpawnOffset()
    {
        return spawnOffset;
    }

    public void SetSpeed(float speed)
    {
        spawnSpeed = speed;
    }
}
