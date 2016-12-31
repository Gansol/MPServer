using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using MPProtocol;
using System;

/*
 * 這腳本只負責生東西 時間和控制器要另外寫在Global
 * 現在是測試版 完成後要把Intantiate改成poolManager.ActiveObject()
 * 
 * 
 * 
 * 
 */

public class MiceSpawner : MonoBehaviour
{
    [Range(2, 5)]
    public float miceSize;
    public GameObject[] hole;
    public GameObject[] micePanel;

    private BattleManager battleManager;
    private PoolManager poolManager;
    private ObjectFactory objFactory;

    private Vector3 _miceSize;
    private Coroutine coroutine;
    private SpawnState spawnState = null;
    private SpawnStatus spawnStatus = SpawnStatus.LineL;
    //private ENUM_SpawnMethod spawnMethod = ENUM_SpawnMethod.Swimming;


    void Start()
    {
        battleManager = GetComponent<BattleManager>();
        objFactory = new ObjectFactory();
        poolManager = GetComponent<PoolManager>();
        Global.photonService.ApplyMissionEvent += OnApplyMission;
        Global.photonService.LoadSceneEvent += OnLoadScene;
    }


    #region -- SpawnByRandom --
    /// <summary>
    /// 1D 隨機產生。
    /// </summary>
    /// <param name="holeArray">1D陣列產生方式</param>
    /// <param name="spawnTime">產生間隔時間</param>
    public Coroutine SpawnByRandom(string miceName, sbyte[] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool isSkill)
    {
        return StartCoroutine(IESpawnByRandom(miceName, holeArray, spawnTime, intervalTime, lerpTime, spawnCount, isSkill));
    }

    private IEnumerator IESpawnByRandom(string miceName, sbyte[] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool isSkill)
    {
        //        Debug.Log("Random spawnCount:" + spawnCount);
        List<sbyte> listHole = new List<sbyte>(holeArray);
        sbyte[] rndHoleArray = new sbyte[spawnCount];       // 隨機陣列
        int holePos = 0, count = 0;

        // 產生隨機值陣列
        for (holePos = 0; holePos < spawnCount; holePos++)
        {
            int rndNum = UnityEngine.Random.Range(0, listHole.Count);
            rndHoleArray[holePos] = holeArray[rndNum];
            listHole.RemoveAt(rndNum);
        }

        //產生老鼠
        for (holePos = 0; count < spawnCount; holePos++)
        {
            holePos = ResetStartPos(holePos, holeArray.Length, false);
            objFactory.InstantiateMice(poolManager, miceName, miceSize, hole[rndHoleArray[holePos]]);
            count++;
            yield return new WaitForSeconds(spawnTime);
        }

        yield return new WaitForSeconds(intervalTime);
        Global.spawnFlag = true;
    }
    #endregion

    #region -- SpawnBy1D --
    /// <summary>
    /// 1D陣列 順序產生
    /// </summary>
    /// <param name="miceName">老鼠名稱</param>
    /// <param name="holeArray">資料陣列</param>
    /// <param name="spawnTime">每個間隔時間</param>
    /// <param name="intervalTime">每組間隔時間</param>
    /// <param name="lerpTime">加速度</param>
    /// <param name="spawnCount">數量</param>
    /// <param name="randomPos">隨機位置(-1=不隨機)</param>
    /// <param name="isSkill">是否為技能</param>
    /// <param name="reSpawn">正反產生</param>
    /// <returns></returns>
    public Coroutine SpawnBy1D(string miceName, sbyte[] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, int randomPos, bool isSkill, bool reSpawn)
    {
        coroutine = StartCoroutine(IESpawnBy1D(miceName, holeArray, spawnTime, intervalTime, lerpTime, spawnCount, randomPos, isSkill, reSpawn));
        return coroutine;
    }



    /// <summary>
    /// 1D陣列 指定位置 產生 (startPos>endPos)
    /// </summary>
    /// <param name="startPos">陣列起始位置</param>
    /// <param name="endPos">陣列結束位置</param>
    /// <param name="miceName">老鼠名稱</param>
    /// <param name="holeArray">1D 資料陣列</param>
    /// <param name="spawnTime">每個間隔時間</param>
    /// <param name="intervalTime">加速度</param>
    /// <param name="lerpTime">加速度</param>
    /// <param name="isSkill">是否為技能</param>
    /// <param name="reSpawn">正反產生</param>
    /// <returns>Coroutine</returns>
    public Coroutine SpawnBy1D(int startPos, int endPos, string miceName, sbyte[] holeArray, float spawnTime, float intervalTime, float lerpTime, bool isSkill, bool reSpawn)
    {
        int count = 0;
        int spawnCount = Mathf.Abs(endPos - startPos) + 1;
        List<sbyte> buffer = new List<sbyte>();


        //for (int i = startPos; i <= endPos; i++)
        //{
        //    buffer.Add(holeArray[i]);
        //}

        if (reSpawn) 
            startPos = endPos;

        for (int i = startPos; count < spawnCount; )
        {
            try
            {
                buffer.Add(holeArray[i]);
                count++;
                i += (reSpawn) ? -1 : 1;
            }
            catch (Exception e)
            {
                Debug.Log("i: " + i + "holeArray: " + holeArray.Length + "count: " + count + "reSpawn: " + reSpawn);
                throw e;
            }
        }

        coroutine = StartCoroutine(IESpawnBy1D(miceName, buffer.ToArray(), spawnTime, intervalTime, lerpTime, spawnCount, -1, isSkill, reSpawn));
        return coroutine;
    }

    private IEnumerator IESpawnBy1D(string miceName, sbyte[] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, int randomPos, bool isSkill, bool reSpawn)
    {
        //   Debug.Log("1D spawnCount:" + spawnCount);
        int count = 0, holePos = 0;

        randomPos = SetStartPos(holeArray.Length, randomPos, reSpawn);  // 設定起始位置

        for (holePos = randomPos; count < spawnCount; )
        {
            holePos = ResetStartPos(holePos, holeArray.Length, reSpawn);    // 重設起始位置
            try
            {
                objFactory.InstantiateMice(poolManager, miceName, miceSize, hole[holeArray[holePos]]);
            }
            catch (Exception e)
            {
                Debug.Log("randomPos: " + randomPos + " reSpawn: " + reSpawn + " holePos:" + holePos + " count:" + count + " spawnCount:" + spawnCount + "G:" + Global.dictBattleMice);
                throw e;
            }
            yield return new WaitForSeconds(spawnTime);

            spawnTime = Mathf.Lerp(spawnTime, 0f, lerpTime);
            holePos += (reSpawn) ? -1 : 1;
            count++;
        }
        //        Debug.Log("IESpawnBy1D Count:" + count);
        Global.spawnFlag = true;
        yield return new WaitForSeconds(intervalTime);

    }
    #endregion

    #region -- SpawnBy2D --

    /// <summary>
    /// 2D 順序位置產生(禁止不規則陣列)
    /// </summary>
    /// <param name="miceName">老鼠名稱</param>
    /// <param name="holeArray">規則陣列</param>
    /// <param name="spawnTime">每個間隔時間</param>
    /// <param name="intervalTime">每組間隔時間</param>
    /// <param name="lerpTime">加速度</param>
    /// <param name="spawnCount">數量</param>
    /// <param name="randPos">隨機位置 (-1不隨機)</param>
    /// <param name="isSkill">是否為技能</param>
    /// <param name="reSpawn">正反產生</param>
    /// <returns></returns>
    public Coroutine SpawnBy2D(string miceName, sbyte[,] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, Vector2 randPos, bool isSkill, bool reSpawn)
    {
        return StartCoroutine(IESpawnBy2D(miceName, holeArray, spawnTime, intervalTime, lerpTime, spawnCount, randPos, isSkill, reSpawn));
    }

    /// <summary>
    /// 2D 指定位置產生(禁止不規則陣列)(startPos>endPos)
    /// </summary>
    /// <param name="startPos">陣列起始位置</param>
    /// <param name="endPos">陣列結束位置</param>
    /// <param name="miceName">老鼠名稱</param>
    /// <param name="holeArray">規則陣列</param>
    /// <param name="spawnTime">每個間隔時間</param>
    /// <param name="intervalTime">每組間隔時間</param>
    /// <param name="lerpTime">加速度</param>
    /// <param name="startAt">開始位置</param>
    /// <param name="isSkill">是否為技能</param>
    /// <param name="reSpawn">正反產生</param>
    /// <returns></returns>
    public Coroutine SpawnBy2D(Vector2 startPos, Vector2 endPos, string miceName, sbyte[,] holeArray, float spawnTime, float intervalTime, float lerpTime, Vector2 startAt, bool isSkill, bool reSpawn)
    {
        int height = Math.Abs(((int)endPos.x - (int)startPos.x) + 1);
        int spawnCount = -1; // 梯形公式
        int count = 0;
        sbyte[,] buffer;

        if (height != 1)
        {
            spawnCount = Math.Abs((holeArray.GetLength(1) - (int)startPos.y)) + ((int)endPos.y + 1) * height / 2; // 梯形公式
            buffer = new sbyte[height, holeArray.GetLength(1)];
        }
        else
        {
            spawnCount = Math.Abs((int)endPos.y - (int)startPos.y) + 1;
            buffer = new sbyte[height, spawnCount];
        }





        int arrX = 0, arrY = 0, j = 0;

        for (int i = (int)startPos.x; count < buffer.GetLength(0) && count < spawnCount; i++)
        {
            for (j = (int)startPos.y; count < buffer.GetLength(1) && count < spawnCount; j++)
            {
                buffer[arrX, arrY] = holeArray[i, j];
                count++;
                arrY++;
            }
            j = arrY = 0;
            arrX++;
        }



        return StartCoroutine(IESpawnBy2D(miceName, buffer, spawnTime, intervalTime, lerpTime, spawnCount, startAt, isSkill, reSpawn));
    }


    private IEnumerator IESpawnBy2D(string miceName, sbyte[,] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, Vector2 randPos, bool isSkill, bool reSpawn)
    {
        Debug.Log("2D spawnCount:" + spawnCount);
        int i = 0, j = 0, count = 0;

        //// 如果是 正向 
        //if (!reSpawn)
        //{
        //    // "<0" = (不隨機) 是否為隨機。 不隨機:原始座標(0,0) , 隨機:隨機座標
        //    randPos.x = (randPos.x < 0) ? 0 : randPos.x;
        //    randPos.y = (randPos.y < 0) ? 0 : randPos.y;
        //}
        //// 如果反向
        //else
        //{
        //    // "<0" = (不隨機) 是否為隨機。 不隨機:原始座標(最大值,最大值) , 隨機:隨機座標
        //    randPos.x = (randPos.x < 0) ? holeArray.GetLength(0) : randPos.x;
        //    randPos.y = (randPos.y < 0) ? holeArray.GetLength(1) : randPos.y;
        //}


        randPos.x = SetStartPos(holeArray.GetLength(0), (int)randPos.x, reSpawn);
        randPos.y = SetStartPos(holeArray.GetLength(1), (int)randPos.y, reSpawn);

        if (holeArray.GetLength(0) >= 4)
        {
            intervalTime /= 3;
            lerpTime *= 2;
            spawnTime /= 2;
        }

        for (i = (int)randPos.x; count < spawnCount; )    // 1D陣列
        {
            i = ResetStartPos(i, holeArray.GetLength(0), reSpawn);
            if (count < spawnCount)
            {
                for (j = (int)randPos.y; j < holeArray.GetLength(1) && count < spawnCount; )    // 2D陣列
                {
                    j = ResetStartPos(j, holeArray.GetLength(1), reSpawn);
                    objFactory.InstantiateMice(poolManager, miceName, miceSize, hole[holeArray[i, j]]);
                    yield return new WaitForSeconds(spawnTime);
                    j += (reSpawn) ? -1 : 1;
                    count++;
                    //                    Debug.Log("count:" + count + "  i:" + i + "  j:" + j);
                }
                j = (reSpawn) ? holeArray.GetLength(1) - 1 : 0;
            }
            else
            {
                Debug.Log("IESpawnBy2D Count:" + count);
                break;
            }
            i += (reSpawn) ? -1 : 1;
            intervalTime = Mathf.Lerp(intervalTime, 0f, lerpTime);
            yield return new WaitForSeconds(intervalTime / 3);
        }
        Global.spawnFlag = true;
    }
    #endregion



    #region -- SpawnBoss --
    public void SpawnBoss(string miceName, int hp)
    {
        try
        {
            // 如果Hole上有Mice 移除Mice
            if (hole[4].GetComponent<HoleState>().holeState == HoleState.State.Closed)
            {
                Global.dictBattleMice.Remove(hole[4].transform);
                hole[4].transform.GetComponentInChildren<Mice>().gameObject.SendMessage("OnDead", 0.0f);
            }

            // 播放洞口動畫
            hole[4].GetComponent<Animator>().enabled = true;
            hole[4].GetComponent<Animator>().Play("HoleScale");

            // 產生Mice
            GameObject clone = poolManager.ActiveObject(miceName);
            MiceBase mice = clone.GetComponent(typeof(MiceBase)) as MiceBase;

            if (mice.enabled) mice.enabled = false;
            clone.transform.gameObject.SetActive(false);
            clone.transform.parent = hole[4].transform;
            clone.transform.localScale = new Vector3(1.3f, 1.3f, 0f);
            clone.transform.localPosition = new Vector3(0, 0, 0);
            clone.transform.gameObject.SetActive(true);
            clone.AddComponent(miceName + "Boss");


            // 取得老鼠存活時間
            object miceProperty;
            int itemID = ObjectFactory.GetItemIDFromName(miceName);
            Global.miceProperty.TryGetValue(itemID.ToString(), out miceProperty);
            Dictionary<string, object> dictMiceProperty = miceProperty as Dictionary<string, object>;
            dictMiceProperty.TryGetValue("LifeTime", out miceProperty);

            // 初始化數值
            MiceBossBase boss = clone.GetComponent(typeof(MiceBossBase)) as MiceBossBase;
            boss.Initialize(0.1f, 6, 60, float.Parse(miceProperty.ToString()));
            boss.SetArribute(new MiceAttr(hp));
            boss.SetSkill(new SkillCallMice());

            // 加入老鼠陣列
            Global.dictBattleMice.Add(clone.transform.parent, clone);
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion


    #region -- Spawn --
    /// <summary>
    /// 產生老鼠
    /// </summary>
    /// <param name="range">(int)產生老鼠資料區間</param>
    /// <param name="miceName">老鼠名稱</param>
    /// <param name="spawnTime">每個間隔時間</param>
    /// <param name="intervalTime">每組間隔時間</param>
    /// <param name="lerpTime">加速度</param>
    /// <param name="spawnCount">數量</param>
    /// <param name="bRndPos"></param>
    /// <param name="isSkill"></param>
    /// <param name="reSpawn"></param>
    /// <returns></returns>
    public Coroutine Spawn(Vector2 range, string miceName, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool bRndPos, bool isSkill, bool reSpawn)
    {
        int rndRange = UnityEngine.Random.Range((int)range.x, (int)range.y + 1);

        spawnStatus = SelectStatus(rndRange);
        battleManager.spawnStatus = spawnStatus;    // 測試 Debug顯示用
        return IESpawn(spawnStatus, miceName, spawnTime, intervalTime, lerpTime, spawnCount, bRndPos, isSkill, reSpawn);
    }


    private Coroutine IESpawn(SpawnStatus spawnStatus, string miceName, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool bRndPos, bool isSkill, bool reSpawn)
    {
        //        Debug.Log("Spawn spawnCount:" + spawnCount);
        UnityEngine.Random.seed = unchecked((int)System.DateTime.Now.Ticks);
        //bool reSpawn = System.Convert.ToBoolean(Random.Range(0, 1 + 1));

        // Random
        if (spawnStatus < SpawnStatus.SpawnData1D)
        {
            sbyte[] data = SpawnData.GetSpawnData(SpawnStatus.Random) as sbyte[];
            coroutine = SpawnByRandom(miceName, data, spawnTime, intervalTime, lerpTime, spawnCount, isSkill);
        }

        // SpawnBy1D
        else if (spawnStatus > SpawnStatus.SpawnData1D && spawnStatus < SpawnStatus.SpawnData2D)
        {
            sbyte[] data = SpawnData.GetSpawnData(spawnStatus) as sbyte[];
            int rndPos = -1;

            if (bRndPos) rndPos = UnityEngine.Random.Range(0, data.Length);
            coroutine = SpawnBy1D(miceName, data, spawnTime, intervalTime, lerpTime, spawnCount, rndPos, isSkill, reSpawn);
        }

        // SpawnBy2D
        else if (spawnStatus > SpawnStatus.SpawnData2D /*&& spawnStatus < SpawnStatus.SpawnDataCustom*/)
        {
            sbyte[,] data = SpawnData.GetSpawnData(spawnStatus) as sbyte[,];
            Vector2 rndPos = new Vector2(-1, -1);
            if (data == null)
                Debug.LogError("spawnStatus : " + spawnStatus.ToString());
            if (bRndPos) rndPos = new Vector2(UnityEngine.Random.Range(0, data.GetLength(0)), UnityEngine.Random.Range(0, data.GetLength(1)));
            coroutine = SpawnBy2D(miceName, data, spawnTime, intervalTime, lerpTime, spawnCount, rndPos, isSkill, reSpawn);
        }

        // SpawnDataCustom
//        else if (spawnStatus > SpawnStatus.SpawnDataCustom)
//        {
//            sbyte[] data = SpawnData.GetSpawnData(spawnStatus) as sbyte[];
//            int rndPos = -1;

//            if (bRndPos) rndPos = UnityEngine.Random.Range(0, data.Length);
//            coroutine = SpawnBy1D(miceName, data, spawnTime, intervalTime, lerpTime, spawnCount, rndPos, isSkill, reSpawn);
////            coroutine = StartCoroutine(IESpawnByCustom(miceName, data, spawnTime, intervalTime, lerpTime, spawnCount, isSkill, reSpawn));
//        }
        else
        {
            coroutine = null;
            Debug.LogError("Unknown Spawn Data !");
        }

        return coroutine;
    }

    /// <summary>
    /// 選擇SpawnStatus
    /// </summary>
    /// <param name="status">0:Random,1:1D,2:2D,3:Jagged</param>
    /// <returns></returns>
    protected virtual SpawnStatus SelectStatus(int status)  // Select SpawnStatus 的副程式
    {
        switch (status)
        {
            case 0:
                spawnStatus = SpawnStatus.Random;   // Random             
                break;
            case 1: // 1D形狀
                spawnStatus = (byte)10 + (SpawnStatus)UnityEngine.Random.Range(0, 8) + 1;   // 1D                    
                break;
            case 2: // 2D形狀
                spawnStatus = (byte)100 + (SpawnStatus)UnityEngine.Random.Range(0, 10) + 1;   // 2D                       
                break;
            case 3: // 自訂形狀
                spawnStatus = (byte)100 + (SpawnStatus)UnityEngine.Random.Range(0, 10) + 1;   // 2D
                //spawnStatus = (byte)200 + (SpawnStatus)UnityEngine.Random.Range(0, 6) + 1;   // Custom                       
                break;
            default:
                spawnStatus = SpawnStatus.Random;   // 1~6              
                break;
        }

        return spawnStatus;
    }
    #endregion

    #region -- SpawnSpecial --
    public Coroutine SpawnSpecial(SpawnState spawnState, string miceName, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool reSpawn)
    {
        //        Debug.Log("SpawnSpecial spawnCount:" + spawnState);

        battleManager.spawnStatus = spawnStatus;    // 測試 Debug顯示用
        coroutine = StartCoroutine(spawnState.Spawn(this, miceName, spawnTime, intervalTime, lerpTime, spawnCount, reSpawn));
        return coroutine;
    }
    #endregion


    #region -- SetDefaultValue --

    /// <summary>
    /// 取得初始位置
    /// </summary>
    /// <param name="arrayLength">陣列長度</param>
    /// <param name="randomPos">隨機位置</param>
    /// <param name="reSpawn">正反產生</param>
    /// <returns></returns>
    private int SetStartPos(int arrayLength, int randomPos, bool reSpawn)
    {
        if (randomPos < 0) randomPos = (!reSpawn) ? 0 : arrayLength - 1;
        if (randomPos <= arrayLength) return randomPos;     // 注意 < 改為 <= 可能發生錯誤
        else
            Debug.Log(" arrayLength: " + arrayLength + "  <  " + "randomPos: " + randomPos);

        return randomPos;
    }

    /// <summary>
    /// 如果超過陣列大小 回復初始值
    /// </summary>
    /// <param name="holePos">目前位置</param>
    /// <param name="maxValue">最大值</param>
    /// <param name="reSpawn">正反產生</param>
    /// <returns></returns>
    private int ResetStartPos(int holePos, int maxValue, bool reSpawn)
    {
        if (!reSpawn)
        {
            return holePos = ((float)holePos / (float)maxValue == 1) ? 0 : holePos; // 如果出生位置=陣列長度 重新(0)開始 避免超出最大值
        }
        else
        {
            return holePos = (holePos < 0 || (float)holePos / (float)maxValue == 1) ? maxValue - 1 : holePos; // 如果出生位置=陣列長度 重新(0)開始 避免超出最大值
        }
    }
    #endregion















    void OnApplyMission(Mission mission, Int16 missionScore)
    {
        if (mission == Mission.WorldBoss)
        {
            SpawnBoss("EggMice", missionScore);    //missionScore這裡是HP
        }
    }

    void OnLoadScene()
    {
        Global.photonService.ApplyMissionEvent -= OnApplyMission;
        Global.photonService.LoadSceneEvent -= OnLoadScene;
    }
}
