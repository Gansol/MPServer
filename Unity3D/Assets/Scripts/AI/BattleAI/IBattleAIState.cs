using UnityEngine;
using System.Collections;
using MPProtocol;
using System.Collections.Generic;




public class BattleAIStateAttr
{
    public  float spawnOffset = 0f;    // SpawnTime修正值
    public  double lastTime = 0d;
    public  int wave = 0, nextBali = 27, nextMuch = 4, nextHero = 50;
    //   protected BattleManager battleManager = null;

    //   protected SpawnAI spawnAI = null;
    public SpawnState spawnState = null;
    public ENUM_BattleAIState battleAIState = ENUM_BattleAIState.EasyMode;
    public SpawnStatus spawnStatus = SpawnStatus.LineL;
    public int totalSpawn, spawnCount, minStatus, maxStatus, minMethod, maxMethod, normalSpawn, nowCombo;          //總量 產生數量、老鼠產生資料最小值、老鼠產生資料最大值、正常產生狀態值、更換AI狀態後目前combo
    public float pervStateTime, nextStateTime, lerpTime, spawnTime, intervalTime, spawnIntervalTime, intervalOffset = .05f, minSpawnInterval, maxSpawnInterval, spawnSpeed; // 加速度、老鼠產生間隔、每組老鼠產生間隔、Spawn難度修正值、每次Spawn間隔、最大、最小間隔


    public short defaultMice = 10001, bali = 11001, much = 11002, hero = 11003;
}

public abstract class IBattleAIState
{
    protected BattleAttr battleAttr;
    protected BattleAIStateAttr stateAttr;
    protected GameObject m_RootUI = null;
    private Coroutine coroutine;

    public IBattleAIState(BattleAttr battleAttr)
    {
        m_RootUI = GameObject.Find(Global.Scene.BattleAsset.ToString());
        this.battleAttr = battleAttr;
        //if (spawnAI != null)
        //    spawnAI = new SpawnAI(m_RootUI.GetComponentInChildren<PoolManager>(), battleAttr.hole);
    }

    public abstract void UpdateState();

    public virtual SpawnStatus GetSpawnStatus()
    {
        return stateAttr.spawnStatus;
    }

    //public void SetAIController(BattleManager battleManager)
    //{
    //    this.battleManager = battleManager;
    //    spawner = battleManager.GetSpawnAI();
    //}

    /// <summary>
    /// 產生老鼠
    /// </summary>
    /// <param name="miceID">老鼠ID</param>
    /// <returns>routine</returns>
    protected virtual Coroutine Spawn(short miceID, BattleAIStateAttr stateAttr )
    {
        MPGame.Instance.GetSpawnController().AddCoroutine(new RandomSpawnCommand(miceID, stateAttr));
        MPGame.Instance.GetSpawnController().Extecutor();
        return null;
    }

    ///// <summary>
    ///// 產生老鼠
    ///// </summary>
    ///// <param name="miceID">老鼠ID</param>
    ///// <returns>routine</returns>
    //protected virtual Coroutine Spawn(short miceID, BattleAIStateAttr stateAttr)
    //{
    //    //        Debug.Log(Time.time);
    //    Random.InitState(unchecked((int)System.DateTime.Now.Ticks));
    //    bool reSpawn = System.Convert.ToBoolean(Random.Range(0, 1 + 1));
    //    // spawner = GameObject.FindGameObjectWithTag("GM").GetComponent<SpawnAI>();

    //    // spawnController spawn

    //    coroutine = MPGFactory.GetCreatureFactory().Spawn(miceID, stateAttr, true, false, reSpawn);
    //    stateAttr.totalSpawn += stateAttr.spawnCount;
    //    SpawnNaturalMice(stateAttr.totalSpawn);


    //    return coroutine;
    //}

    //生成 自然單位老鼠
    private void SpawnNaturalMice(int totalSpawn)
    {
        BattleAIStateAttr tmpAttr = new BattleAIStateAttr();
      
        if (totalSpawn > stateAttr.nextBali)
        {
            tmpAttr.spawnCount = Random.Range(0, 3 + 1);
            stateAttr.nextBali = totalSpawn + stateAttr.nextBali;
            Spawn(stateAttr.bali, tmpAttr);//錯誤
        }

        if (totalSpawn > stateAttr.nextMuch)
        {
            tmpAttr.spawnCount = 1;
            stateAttr.nextMuch = totalSpawn + stateAttr.nextMuch;
            Spawn(stateAttr.much, tmpAttr);//錯誤
        }
        if (totalSpawn > stateAttr.nextHero)
        {
            tmpAttr.spawnCount = 1;
            stateAttr.nextHero = totalSpawn + stateAttr.nextHero;
            Spawn(stateAttr.hero, tmpAttr);//錯誤
        }
    }

    /// <summary>
    /// 產生特別狀態老鼠
    /// </summary>
    /// <param name="miceID">老鼠ID</param>
    /// <param name="intervalTimes">間格倍率</param>
    /// <returns></returns>
    protected virtual Coroutine SpawnSpecial(short miceID, BattleAIStateAttr attr   /* int spawnValue, short miceID, float intervalTimes, int spawnCount*/)
    {
        stateAttr.spawnState = SelectSpawnState(Random.Range(attr.minMethod, attr.maxMethod), attr.intervalTime);
        Random.InitState(unchecked((int)System.DateTime.Now.Ticks));
        bool reSpawn = System.Convert.ToBoolean(Random.Range(0, 1 + 1));
        //   spawner = battleManager.GetSpawnAI();

        coroutine = MPGFactory.GetCreatureFactory().SpawnSpecial(miceID, stateAttr, reSpawn);
        stateAttr.totalSpawn += attr.spawnCount;
        SpawnNaturalMice(stateAttr.totalSpawn);

        
        return coroutine;
    }

    public virtual void SetValue(float lerpTime, float spawnTime, float intervalTime, int spawnCount)
    {
        if (lerpTime > 0) stateAttr.lerpTime = lerpTime;
        if (spawnTime > 0) stateAttr.spawnTime = spawnTime;
        if (intervalTime > 0) stateAttr.intervalTime = intervalTime;
        if (spawnCount > 0) stateAttr.spawnCount = spawnCount;
    }

    /// <summary>
    /// 根據combo動態設定SpawnIntervalTime
    /// </summary>
    public virtual void SetSpawnIntervalTime()
    {
        float offset;

        offset = (battleAttr.bCombo) ? -stateAttr.intervalOffset : stateAttr.intervalOffset * 5;

        if (stateAttr.spawnOffset + offset >= stateAttr.minSpawnInterval && stateAttr.spawnOffset + offset <= stateAttr.maxSpawnInterval)
            stateAttr.spawnOffset += offset;
    }

    private SpawnState SelectSpawnState(int spawnValue, float intervalTimes)
    {
        intervalTimes = (intervalTimes <= 0) ? 1 : intervalTimes;

        switch (spawnValue)
        {
            case (int)ENUM_SpawnMethod.CrossHor:
                {
                    stateAttr.spawnState = new CrossHorSpawnState(intervalTimes);
                    break;
                }
            case (int)ENUM_SpawnMethod.CrossVert:
                {
                    stateAttr.spawnState = new CrossVertSpawnState(intervalTimes);
                    break;
                }
            case (int)ENUM_SpawnMethod.Feather:
                {
                    stateAttr.spawnState = new FeatherSpawnState(intervalTimes);
                    break;
                }
            case (int)ENUM_SpawnMethod.Fish:
                {
                    stateAttr.spawnState = new FishSpawnState(intervalTimes);
                    break;
                }
            case (int)ENUM_SpawnMethod.Snake:
                {
                    stateAttr.spawnState = new SnakeSpawnState(intervalTimes);
                    break;
                }
            case (int)ENUM_SpawnMethod.Door:
                {
                    stateAttr.spawnState = new CloseDoorSpawnState(intervalTimes);
                    break;
                }
            case (int)ENUM_SpawnMethod.STwin:
                {
                    stateAttr.spawnState = new STwinSpawnState(intervalTimes);
                    break;
                }
            case (int)ENUM_SpawnMethod.Swim:
                {
                    stateAttr.spawnState = new SwimSpawnState(intervalTimes);
                    break;
                }
            case (int)ENUM_SpawnMethod.LoopCricle:
                {
                    stateAttr.spawnState = new LoopCircleSpawnState(intervalTimes);
                    break;
                }
            case (int)ENUM_SpawnMethod.Cross:
                {
                    stateAttr.spawnState = new CrossSpawnState(intervalTimes);
                    break;
                }
            case (int)ENUM_SpawnMethod.BillingHV:
                {
                    stateAttr.spawnState = new BillingHVSpawnState(intervalTimes);
                    break;
                }
            case (int)ENUM_SpawnMethod.BillingX:
                {
                    stateAttr.spawnState = new BillingXSpawnState(intervalTimes);
                    break;
                }
            default:
                Debug.Log("!!!!!!!!!!!!!!!!!!!!!! Error value : " + spawnValue);
                stateAttr.spawnState = new CrossHorSpawnState(intervalTimes);
                break;
        }
        return stateAttr.spawnState;
    }

    public virtual float GetLerpTime()
    {
        return stateAttr.lerpTime;
    }

    public virtual float GetSpawnTime()
    {
        return stateAttr.spawnTime;
    }

    public virtual float GetIntervalTime()
    {
        return stateAttr.intervalTime;
    }

    public virtual int GetSpawnCount()
    {
        return stateAttr.spawnCount;
    }

    public virtual float GetSpawnOffset()
    {
        return stateAttr.spawnOffset;
    }

    public virtual void SetSpeed(float speed)
    {
        stateAttr.spawnSpeed = speed;
    }

    public virtual ENUM_BattleAIState GetState()
    {
        return stateAttr.battleAIState;
    }
}
