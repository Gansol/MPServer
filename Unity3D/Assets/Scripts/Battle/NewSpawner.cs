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

public class NewSpawner : MonoBehaviour
{
    public GameObject[] hole;
    private PoolManager poolManager;
    [Range(2, 5)]
    public float miceSize;
    private Vector3 _miceSize;

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
            holePos = ((float)holePos / (float)maxValue == 1) ? 0 : holePos; // 如果出生位置=陣列長度 重新(0)開始 避免超出最大值
        else
            holePos = (holePos < 0) ? maxValue : holePos; // 如果出生位置=陣列長度 重新(0)開始 避免超出最大值

        return holePos;
    }

    /// <summary>
    /// 1D 隨機產生。
    /// </summary>
    /// <param name="holeArray">1D陣列產生方式</param>
    /// <param name="spawnTime">產生間隔時間</param>
    public IEnumerator SpawnByRandom(string miceName, sbyte[] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool isSkill)
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
            SetDefaultValue(holePos, holeArray.Length, false);
            InstantiateMice(miceName, hole[holeArray[holePos] - 1]);
            count++;
            yield return new WaitForSeconds(spawnTime);
        }

        Global.spawnFlag = true;
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
    public IEnumerator SpawnBy1D(string miceName, sbyte[] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, int randomPos, bool isSkill, bool reSpawn)
    {
        int count = 0, holePos = 0;

        if (randomPos < 0) randomPos = 0;     // 如果隨機起始位置值=-1 = 不隨機=0

        for (holePos = randomPos; count < spawnCount; )
        {
            InstantiateMice(miceName, hole[holeArray[holePos] - 1]);
            yield return new WaitForSeconds(spawnTime);

            spawnTime = Mathf.Lerp(spawnTime, 0f, lerpTime);
            holePos += (reSpawn) ? -1 : 1;

            SetDefaultValue(holePos, holeArray.Length, reSpawn);

            count++;
        }
    }

    /// <summary>
    /// 正向產生。holeArray[,]=2D陣列產生方式,spawnTime=老鼠間隔時間,intervalTime=產生間隔
    /// </summary>
    /// <param name="holeArray"></param>
    /// <param name="spawnTime"></param>
    /// <returns></returns>
    public IEnumerator SpawnBy2D(string miceName, sbyte[,] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, Vector2 randPos, bool isSkill, bool reSpawn)
    {
        int count = 0;

        if (randPos.x < 0) randPos.x = 0;
        if (randPos.y < 0) randPos.y = 0;

        if (holeArray.GetLength(0) >= 4)
        {
            intervalTime /= 3;
            lerpTime *= 2;
            spawnTime /= 2;
        }

        for (int i = (int)randPos.x; i < holeArray.GetLength(0); )    // 1D陣列
        {
            for (int j = (int)randPos.y; j < holeArray.GetLength(1); )    // 2D陣列
            {
                InstantiateMice(miceName, hole[holeArray[i, j] - 1]);
                yield return new WaitForSeconds(spawnTime);

                j += (reSpawn) ? -1 : 1;
                SetDefaultValue(j, holeArray.GetLength(1), reSpawn);
                count++;
            }
            i += (reSpawn) ? -1 : 1;
            SetDefaultValue(i, holeArray.GetLength(1), reSpawn);
        }
    }


    /// <summary>
    /// 正向產生自訂方式。holeArray[][]=2D自訂陣列,spawnTime=老鼠間隔時間,intervalTime=產生間隔
    /// </summary>
    /// <param name="holeArray"></param>
    /// <param name="spawnTime"></param>
    /// <returns></returns>
    public IEnumerator SpawnByCustom(string miceName, sbyte[][] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool isSkill,bool reSpawn)
    {
        for (int i = 0; i < holeArray.GetLength(0);)    // 1D陣列
        {
            for (int j = 0; j < holeArray[i].Length; )    // 2D陣列
            {
                InstantiateMice(miceName, hole[holeArray[i][j] - 1]);
                j += (reSpawn) ? -1 : 1;
                SetDefaultValue(j, holeArray[i].Length, reSpawn);
                yield return new WaitForSeconds(spawnTime);
            }
            i += (reSpawn) ? -1 : 1;
            SetDefaultValue(i, holeArray.GetLength(0), reSpawn);

            intervalTime = Mathf.Lerp(intervalTime, 0f, lerpTime);
            yield return new WaitForSeconds(intervalTime / 5);
        }
    }

    public void SpawnBoss(string miceName, int hp)
    {
        // return null;
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
