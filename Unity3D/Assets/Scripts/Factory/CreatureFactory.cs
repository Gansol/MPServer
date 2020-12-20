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
 * SpawnBoss 的屬性錯誤 手動輸入的
 * 
 * 
 */

public class CreatureFactory :IFactory
{
    MPGame m_MPGame;
    [Range(2, 5)]
    private float miceSize;
    private List<GameObject> _hole;
    //private BattleManager _battleManager;
  //  private PoolManager poolManager;
    private Vector3 _miceSize;
    private Coroutine coroutine;
    //private SpawnState spawnState = null;
    private SpawnStatus spawnStatus = SpawnStatus.LineL;
    //private ENUM_SpawnMethod spawnMethod = ENUM_SpawnMethod.Swimming;

    //private GameLoop 
    //public MPFactory(MPGame MPGame)
    //    : base(MPGame)
    //{
    //    Initinal();
    //}

    public CreatureFactory(/*PoolManager poolManager, List<GameObject> hole*/)
    {
        //  objFactory.TestMethod();
        miceSize = 3.5f;
        _hole = MPGame.Instance.GetBattleSystem().GetHole();
        Global.photonService.ApplySkillMiceEvent += OnApplySkillMice;
        Global.photonService.LoadSceneEvent += OnLoadScene;
    }

    #region -- SpawnByRandom --
    /// <summary>
    /// 1D 隨機產生。
    /// </summary>
    /// <param name="holeArray">1D陣列產生方式</param>
    /// <param name="spawnTime">產生間隔時間</param>
    public Coroutine SpawnByRandom(short miceID, sbyte[] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool isSkill)
    {
        return MPGame.Instance.StartCoroutine(IESpawnByRandom(miceID, holeArray, spawnTime, intervalTime, lerpTime, spawnCount, isSkill));
    }

    private IEnumerator IESpawnByRandom(short miceID, sbyte[] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool isSkill)
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
            holePos = SetStartPos(holeArray.Length, holePos, false);
            m_MPGame.GetPoolSystem().InstantiateMice( miceID, miceSize, _hole[rndHoleArray[holePos]].transform, false);
            count++;
            yield return new WaitForSeconds(spawnTime);
        }

        yield return new WaitForSeconds(intervalTime);
        Global.spawnFlag = true;
    }
    #endregion

    /// <summary>
    /// 1D陣列 順序產生
    /// </summary>
    /// <param name="miceID">老鼠ID</param>
    /// <param name="holeArray">資料陣列</param>
    /// <param name="spawnTime">每個間隔時間</param>
    /// <param name="intervalTime">每組間隔時間</param>
    /// <param name="lerpTime">加速度</param>
    /// <param name="spawnCount">數量</param>
    /// <param name="randomPos">隨機位置(-1=不隨機)</param>
    /// <param name="isSkill">是否為技能</param>
    /// <param name="reSpawn">正反產生</param>
    /// <returns></returns>
    public Coroutine SpawnBy1D(short miceID, sbyte[] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, int randomPos, bool isSkill, bool reSpawn, bool impose)
    {
        coroutine = MPGame.Instance.StartCoroutine(IESpawnBy1D(miceID, holeArray, spawnTime, intervalTime, lerpTime, spawnCount, randomPos, isSkill, reSpawn, impose));
        return coroutine;
    }


    #region -- SpawnBy1D --
    /// <summary>
    /// 1D陣列 順序產生
    /// </summary>
    /// <param name="miceID">老鼠ID</param>
    /// <param name="holeArray">資料陣列</param>
    /// <param name="spawnTime">每個間隔時間</param>
    /// <param name="intervalTime">每組間隔時間</param>
    /// <param name="lerpTime">加速度</param>
    /// <param name="spawnCount">數量</param>
    /// <param name="randomPos">隨機位置(-1=不隨機)</param>
    /// <param name="isSkill">是否為技能</param>
    /// <param name="reSpawn">正反產生</param>
    /// <returns></returns>
    public Coroutine SpawnBy1D(short miceID, sbyte[] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, int randomPos, bool isSkill, bool reSpawn)
    {
        coroutine = MPGame.Instance.StartCoroutine(IESpawnBy1D(miceID, holeArray, spawnTime, intervalTime, lerpTime, spawnCount, randomPos, isSkill, reSpawn, false));
        return coroutine;
    }



    /// <summary>
    /// 1D陣列 指定位置 產生 (startPos>endPos)
    /// </summary>
    /// <param name="startPos">陣列起始位置</param>
    /// <param name="endPos">陣列結束位置</param>
    /// <param name="miceID">老鼠ID</param>
    /// <param name="holeArray">1D 資料陣列</param>
    /// <param name="spawnTime">每個間隔時間</param>
    /// <param name="intervalTime">加速度</param>
    /// <param name="lerpTime">加速度</param>
    /// <param name="isSkill">是否為技能</param>
    /// <param name="reSpawn">正反產生</param>
    /// <returns>Coroutine</returns>
    public Coroutine SpawnBy1D(int startPos, int endPos, short miceID, sbyte[] holeArray, float spawnTime, float intervalTime, float lerpTime, bool isSkill, bool reSpawn)
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

        for (int i = startPos; count < spawnCount;)
        {
            try
            {
                buffer.Add(holeArray[i]);
                count++;
                i += (reSpawn) ? -1 : 1;
            }
            catch
            {
                Debug.Log("i: " + i + "holeArray: " + holeArray.Length + "count: " + count + "reSpawn: " + reSpawn);
                throw;
            }
        }

        coroutine = MPGame.Instance.StartCoroutine(IESpawnBy1D(miceID, buffer.ToArray(), spawnTime, intervalTime, lerpTime, spawnCount, -1, isSkill, reSpawn, false));
        return coroutine;
    }

    private IEnumerator IESpawnBy1D(short miceID, sbyte[] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, int randomPos, bool isSkill, bool reSpawn, bool impose)
    {
        //   Debug.Log("1D spawnCount:" + spawnCount);
        int count = 0, holePos = 0;

        holePos = SetStartPos(holeArray.Length, randomPos, reSpawn);  // 設定起始位置

        while (count < spawnCount)
        {
            try
            {
                // objFactory.TestMethod();
                m_MPGame.GetPoolSystem().InstantiateMice( miceID, miceSize, _hole[holeArray[holePos]].transform, impose);
            }
            catch (Exception e)
            {
                Debug.Log("【IESpawnBy1D Error】   randomPos: " + randomPos + " reSpawn: " + reSpawn + " holePos:" + holePos + " count:" + count + " spawnCount:" + spawnCount + "G:" + Global.dictBattleMiceRefs.Count);
                throw e;
            }

            holePos += (reSpawn) ? -1 : 1;
            holePos = SetStartPos(holeArray.Length, holePos, reSpawn);  // 重設起始位置
            count++;

            yield return new WaitForSeconds(spawnTime);
            spawnTime = Mathf.Lerp(spawnTime, 0f, lerpTime);
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
    /// <param name="miceID">老鼠ID</param>
    /// <param name="holeArray">規則陣列</param>
    /// <param name="spawnTime">每個間隔時間</param>
    /// <param name="intervalTime">每組間隔時間</param>
    /// <param name="lerpTime">加速度</param>
    /// <param name="spawnCount">數量</param>
    /// <param name="randPos">隨機位置 (-1不隨機)</param>
    /// <param name="isSkill">是否為技能</param>
    /// <param name="reSpawn">正反產生</param>
    /// <returns></returns>
    public Coroutine SpawnBy2D(short miceID, sbyte[,] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, Vector2 randPos, bool isSkill, bool reSpawn)
    {
        return MPGame.Instance.StartCoroutine(IESpawnBy2D(miceID, holeArray, spawnTime, intervalTime, lerpTime, spawnCount, randPos, isSkill, reSpawn));
    }

    /// <summary>
    /// 2D 指定位置產生(禁止不規則陣列)(startPos>endPos)
    /// </summary>
    /// <param name="startPos">陣列起始位置</param>
    /// <param name="endPos">陣列結束位置</param>
    /// <param name="miceID">老鼠ID</param>
    /// <param name="holeArray">規則陣列</param>
    /// <param name="spawnTime">每個間隔時間</param>
    /// <param name="intervalTime">每組間隔時間</param>
    /// <param name="lerpTime">加速度</param>
    /// <param name="startAt">開始位置</param>
    /// <param name="isSkill">是否為技能</param>
    /// <param name="reSpawn">正反產生</param>
    /// <returns></returns>
    public Coroutine SpawnBy2D(Vector2 startPos, Vector2 endPos, short miceID, sbyte[,] holeArray, float spawnTime, float intervalTime, float lerpTime, Vector2 startAt, bool isSkill, bool reSpawn)
    {
        int height = Math.Abs(((int)endPos.x - (int)startPos.x) + 1);
        int spawnCount = -1; // 梯形公式
        int count = 0;
        sbyte[,] buffer;

        // 新增 暫存陣列 避免取值長度與原始陣列長度不同
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

        // 取出資料存入陣列 (開始位置>結束位置資料)
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
        return MPGame.Instance.StartCoroutine(IESpawnBy2D(miceID, buffer, spawnTime, intervalTime, lerpTime, spawnCount, startAt, isSkill, reSpawn));
    }


    private IEnumerator IESpawnBy2D(short miceID, sbyte[,] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, Vector2 randPos, bool isSkill, bool reSpawn)
    {
        Debug.Log("2D spawnCount:" + spawnCount);

        int count = 0;
        int i = SetStartPos(holeArray.GetLength(0), (int)randPos.x, reSpawn);
        int j = SetStartPos(holeArray.GetLength(1), (int)randPos.y, reSpawn);

        // 長度較長的資料列 減少間隔
        if (holeArray.GetLength(0) >= 4)
        {
            intervalTime /= 3;
            lerpTime *= 2;
            spawnTime /= 2;
        }

        while (count < spawnCount)    // 1D陣列
        {
            i = SetStartPos(holeArray.GetLength(0), i, reSpawn);
            j = SetStartPos(holeArray.GetLength(1), j, reSpawn);

            while (j >= 0 && j < holeArray.GetLength(1) && count < spawnCount)    // 2D陣列
            {
                m_MPGame.GetPoolSystem().InstantiateMice( miceID, miceSize, _hole[holeArray[i, j]].transform, false);
                count++;
                //Debug.Log("count:" + count + "  i:" + i + "  j:" + j);
                j += (reSpawn) ? -1 : 1;
                yield return new WaitForSeconds(spawnTime);
            }

            i += (reSpawn) ? -1 : 1;
            intervalTime = Mathf.Lerp(intervalTime, 0f, lerpTime);
            yield return new WaitForSeconds(intervalTime / 3);
        }
        Global.spawnFlag = true;
    }
    #endregion

    #region -- Spawn --
    /// <summary>
    /// 產生老鼠
    /// </summary>
    /// <param name="range">(int)產生老鼠資料區間</param>
    /// <param name="miceID">老鼠ID</param>
    /// <param name="spawnTime">每個間隔時間</param>
    /// <param name="intervalTime">每組間隔時間</param>
    /// <param name="lerpTime">加速度</param>
    /// <param name="spawnCount">數量</param>
    /// <param name="bRndPos"></param>
    /// <param name="isSkill"></param>
    /// <param name="reSpawn"></param>
    /// <returns></returns>
    public Coroutine Spawn(short miceID, BattleAIStateAttr stateAttr,bool bRndPos,bool isSkill ,bool reSpawn /*   Vector2 range, short miceID, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool bRndPos, bool isSkill, bool reSpawn*/)
    {
        int rndRange = UnityEngine.Random.Range((int)stateAttr.minStatus , (int)stateAttr.maxStatus + 1);

        spawnStatus = SelectStatus(rndRange);
        //  _battleManager.spawnStatus = spawnStatus;    // 測試 Debug顯示用
        return IESpawn(spawnStatus, miceID, stateAttr.spawnTime, stateAttr.intervalTime, stateAttr.lerpTime, stateAttr. spawnCount, bRndPos, isSkill, reSpawn);
    }


    private Coroutine IESpawn(SpawnStatus spawnStatus, short miceID, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool bRndPos, bool isSkill, bool reSpawn)
    {
        //        Debug.Log("Spawn spawnCount:" + spawnCount);
        UnityEngine.Random.InitState(unchecked((int)System.DateTime.Now.Ticks));
        //bool reSpawn = System.Convert.ToBoolean(Random.Range(0, 1 + 1));

        // Random
        if (spawnStatus < SpawnStatus.SpawnData1D)
        {
            sbyte[] data = SpawnData.GetSpawnData(SpawnStatus.Random) as sbyte[];
            coroutine = SpawnByRandom(miceID, data, spawnTime, intervalTime, lerpTime, spawnCount, isSkill);
        }

        // SpawnBy1D
        else if (spawnStatus > SpawnStatus.SpawnData1D && spawnStatus < SpawnStatus.SpawnData2D)
        {
            sbyte[] data = SpawnData.GetSpawnData(spawnStatus) as sbyte[];
            int rndPos = -1;

            if (bRndPos) rndPos = UnityEngine.Random.Range(0, data.Length);
            coroutine = SpawnBy1D(miceID, data, spawnTime, intervalTime, lerpTime, spawnCount, rndPos, isSkill, reSpawn);
        }

        // SpawnBy2D
        else if (spawnStatus > SpawnStatus.SpawnData2D /*&& spawnStatus < SpawnStatus.SpawnDataCustom*/)
        {
            sbyte[,] data = SpawnData.GetSpawnData(spawnStatus) as sbyte[,];
            Vector2 rndPos = new Vector2(-1, -1);
            if (data == null)
                Debug.LogError("spawnStatus : " + spawnStatus.ToString());
            if (bRndPos) rndPos = new Vector2(UnityEngine.Random.Range(0, data.GetLength(0)), UnityEngine.Random.Range(0, data.GetLength(1)));
            coroutine = SpawnBy2D(miceID, data, spawnTime, intervalTime, lerpTime, spawnCount, rndPos, isSkill, reSpawn);
            Debug.Log("spawnStatus : " + spawnStatus.ToString() + "Respawn: " + reSpawn);
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
    public Coroutine SpawnSpecial( short miceID, BattleAIStateAttr stateAttr /*  SpawnState spawnState, short miceID, float spawnTime, float intervalTime, float lerpTime, int spawnCount,*/ ,bool reSpawn)
    {
        //        Debug.Log("SpawnSpecial spawnCount:" + spawnState);

        //    _battleManager.spawnStatus = spawnStatus;    // 測試 Debug顯示用
        coroutine = MPGame.Instance.StartCoroutine(stateAttr.spawnState.Spawn( miceID, stateAttr, reSpawn));
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
        if (randomPos < 0 || randomPos >= arrayLength) randomPos = (reSpawn) ? arrayLength - 1 : 0;
        return randomPos;
    }

    ///// <summary>
    ///// 如果超過陣列大小 回復初始值
    ///// </summary>
    ///// <param name="holePos">目前位置</param>
    ///// <param name="maxValue">最大值</param>
    ///// <param name="reSpawn">正反產生</param>
    ///// <returns></returns>
    //private int ResetStartPos(int holePos, int maxValue, bool reSpawn)
    //{
    //    if (!reSpawn)
    //    {
    //        return holePos = ((float)holePos / (float)maxValue == 1) ? 0 : holePos; // 如果出生位置=陣列長度 重新(0)開始 避免超出最大值
    //    }
    //    else
    //    {
    //        return holePos = (holePos < 0 || (float)holePos / (float)maxValue == 1) ? maxValue - 1 : holePos; // 如果出生位置=陣列長度 重新(0)開始 避免超出最大值
    //    }
    //}
    #endregion


    void OnLoadScene()
    {
        Global.photonService.LoadSceneEvent -= OnLoadScene;
    }
    void OnApplySkillMice(short miceID)     // 收到技能攻擊 
    {
        Debug.Log("OnApplySkill miceID:" + miceID);

        //   0.OnApplySkill需要改寫，接收數量、參數
        SpawnByRandom(miceID, (sbyte[])SpawnData.GetSpawnData(SpawnStatus.LineL), 1.5f, 0.25f, 0.25f, 6, true);
    }

    ~CreatureFactory()
    {
        Global.photonService.LoadSceneEvent -= OnLoadScene;
        Global.photonService.ApplySkillMiceEvent -= OnApplySkillMice;
    }
}
