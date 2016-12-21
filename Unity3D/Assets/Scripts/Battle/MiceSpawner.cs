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
    public GameObject[] hole;
    public GameObject[] micePanel;
    private PoolManager poolManager;
    [Range(2, 5)]
    public float miceSize;
    private Vector3 _miceSize;

    private Coroutine coroutine;

    void Start()
    {
        poolManager = GetComponent<PoolManager>();
        Global.photonService.ApplyMissionEvent += OnApplyMission;
        Global.photonService.LoadSceneEvent += OnLoadScene;
    }


    /// <summary>
    /// 產生老鼠
    /// </summary>
    /// <param name="miceName"></param>
    /// <param name="hole"></param>
    private void InstantiateMice(string miceName, GameObject hole)
    {
        if (hole.GetComponent<HoleState>().holeState == HoleState.State.Open)
        {
            if (Global.dictBattleMice.ContainsKey(hole.transform))
                Global.dictBattleMice.Remove(hole.transform);

            GameObject clone = poolManager.ActiveObject(miceName);
            clone.transform.gameObject.SetActive(false);
            hole.GetComponent<HoleState>().holeState = HoleState.State.Closed;
            _miceSize = hole.transform.GetChild(0).localScale / 10 * miceSize;   // Scale 版本
            clone.transform.parent = hole.transform;              // hole[-1]是因為起始值是0 
            clone.transform.localPosition = Vector2.zero;
            clone.transform.localScale = hole.transform.GetChild(0).localScale - _miceSize;  // 公式 原始大小分為10等份 10等份在減掉 要縮小的等份*乘洞的倍率(1.4~0.9) => 1.0整份-0.2份*1(洞口倍率)=0.8份 
            clone.transform.gameObject.SetActive(true);
            clone.SendMessage("Play", AnimatorState.ENUM_AnimatorState.Hello);

            Global.dictBattleMice.Add(clone.transform.parent, clone);
        }

    }

    /// <summary>
    /// 如果超過陣列大小 回復初始值
    /// </summary>
    /// <param name="holePos">目前位置</param>
    /// <param name="maxValue">最大值</param>
    /// <param name="reSpawn">正反產生</param>
    /// <returns></returns>
    private int SetDefaultValue(int holePos, int maxValue, bool reSpawn)
    {
        if (!reSpawn)
        {
            holePos = ((float)holePos / (float)maxValue == 1) ? 0 : holePos; // 如果出生位置=陣列長度 重新(0)開始 避免超出最大值
        }
        else
        {
            holePos = (holePos < 0 || (float)holePos / (float)maxValue == 1) ? maxValue - 1 : holePos; // 如果出生位置=陣列長度 重新(0)開始 避免超出最大值
        }

        return holePos;
    }

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
            holePos = SetDefaultValue(holePos, holeArray.Length, false);
            InstantiateMice(miceName, hole[rndHoleArray[holePos]]);
            count++;
            yield return new WaitForSeconds(spawnTime);
        }

        yield return new WaitForSeconds(intervalTime);
        Global.spawnFlag = true;
        SpawnController.Test--;
    }


    /// <summary>
    /// holeArray[]=1D陣列產生方式
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

    private IEnumerator IESpawnBy1D(string miceName, sbyte[] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, int randomPos, bool isSkill, bool reSpawn)
    {
        int count = 0, holePos = 0;

        if (randomPos < 0) randomPos = 0;     // 如果隨機起始位置值=-1 = 不隨機=0

        for (holePos = randomPos; count < spawnCount; )
        {
            holePos = SetDefaultValue(holePos, holeArray.Length, reSpawn);
            try
            {

                InstantiateMice(miceName, hole[holeArray[holePos]]);
            }
            catch (Exception e)
            {
                Debug.Log("holePos:" + holePos + " count:" + count + " spawnCount:" + spawnCount + "G:" + Global.dictBattleMice);
                throw e;

            }
            yield return new WaitForSeconds(spawnTime);

            spawnTime = Mathf.Lerp(spawnTime, 0f, lerpTime);
            holePos += (reSpawn) ? -1 : 1;


            count++;
        }
        Debug.Log("IESpawnBy1D Count:" + count);
        Global.spawnFlag = true;
        SpawnController.Test--;
        yield return new WaitForSeconds(intervalTime);

    }

    /// <summary>
    /// 正向產生。holeArray[,]=2D陣列產生方式,spawnTime=老鼠間隔時間,intervalTime=產生間隔
    /// </summary>
    /// <param name="holeArray"></param>
    /// <param name="spawnTime"></param>
    /// <returns></returns>
    public Coroutine SpawnBy2D(string miceName, sbyte[,] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, Vector2 randPos, bool isSkill, bool reSpawn)
    {
        return StartCoroutine(IESpawnBy2D(miceName, holeArray, spawnTime, intervalTime, lerpTime, spawnCount, randPos, isSkill, reSpawn));
    }


    private IEnumerator IESpawnBy2D(string miceName, sbyte[,] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, Vector2 randPos, bool isSkill, bool reSpawn)
    {
        int i = 0, j = 0, count = 0;

        randPos.x = (randPos.x < 0) ? 0 : randPos.x;
        randPos.y = (randPos.y < 0) ? 0 : randPos.y;

        if (holeArray.GetLength(0) >= 4)
        {
            intervalTime /= 3;
            lerpTime *= 2;
            spawnTime /= 2;
        }

        for (i = (int)randPos.x; count < spawnCount; )    // 1D陣列
        {
            i = SetDefaultValue(i, holeArray.GetLength(0), reSpawn);
            if (count < spawnCount)
            {
                for (j = (int)randPos.y; j < holeArray.GetLength(1) && count < spawnCount; )    // 2D陣列
                {
                    j = SetDefaultValue(j, holeArray.GetLength(1), reSpawn);
                    InstantiateMice(miceName, hole[holeArray[i, j]]);
                    yield return new WaitForSeconds(spawnTime);
                    j += (reSpawn) ? -1 : 1;
                    count++;
                    Debug.Log("count:" + count + "  i:" + i + "  j:" + j);
                }
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
        SpawnController.Test--;
    }


    /// <summary>
    /// 正向產生自訂方式。holeArray[][]=2D自訂陣列,spawnTime=老鼠間隔時間,intervalTime=產生間隔
    /// </summary>
    /// <param name="holeArray"></param>
    /// <param name="spawnTime"></param>
    /// <returns></returns>
    public Coroutine SpawnByCustom(string miceName, sbyte[][] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool isSkill, bool reSpawn)
    {
        return StartCoroutine(IESpawnByCustom(miceName, holeArray, spawnTime, intervalTime, lerpTime, spawnCount, isSkill, reSpawn));
    }

    private IEnumerator IESpawnByCustom(string miceName, sbyte[][] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool isSkill, bool reSpawn)
    {
        int count = 0;
        for (int i = 0; i < holeArray.GetLength(0); )    // 1D陣列
        {
            i = SetDefaultValue(i, holeArray.GetLength(0), reSpawn);
            if (count < spawnCount)
            {
                for (int j = 0; j < holeArray[i].Length && count < spawnCount; )    // 2D陣列
                {
                    j = SetDefaultValue(j, holeArray[i].Length, reSpawn);
                    InstantiateMice(miceName, hole[holeArray[i][j]]);
                    j += (reSpawn) ? -1 : 1;
                    count++;
                    yield return new WaitForSeconds(spawnTime);
                }
            }
            else
            {
                Debug.Log("IESpawnByCustom Count:" + count);
                break;
            }
            i += (reSpawn) ? -1 : 1;

            intervalTime = Mathf.Lerp(intervalTime, 0f, lerpTime);
            yield return new WaitForSeconds(intervalTime / 5);
        }
        Global.spawnFlag = true;
        SpawnController.Test--;
    }

    public void SpawnBoss(string miceName, int hp)
    {
        try
        {
            if (hole[4].GetComponent<HoleState>().holeState == HoleState.State.Closed)
            {
                Global.dictBattleMice.Remove(hole[4].transform);
                hole[4].transform.GetComponentInChildren<Mice>().gameObject.SendMessage("OnDead", 0.0f);
            }

            hole[4].GetComponent<Animator>().enabled = true;
            hole[4].GetComponent<Animator>().Play("HoleScale");

            // 要等待 動畫結束再出現
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


            MiceBossBase boss = clone.GetComponent(typeof(MiceBossBase)) as MiceBossBase;
            boss.Initialize(0.1f, 6, 60, float.Parse(miceProperty.ToString()));
            boss.SetArribute(new MiceAttr(hp));
            boss.SetSkill(new SkillCallMice());

            Global.dictBattleMice.Add(clone.transform.parent, clone);
            //clone.SendMessage("Play",)
        }
        catch (Exception e)
        {
            throw e;
        }
    }


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
