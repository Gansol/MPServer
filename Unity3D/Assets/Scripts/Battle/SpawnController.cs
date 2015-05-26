using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MPProtocol;
using System;

/* 產生方式
 *
        LineL = 101,                        // 左到右 直線產生
        LineR = 102,                        // 右到左 直線產生
        LinkLineL = 103,                    // 左到右 > 右到左 接續產生
        LinkLineR = 104,                    // 右到左 > 左到右 接續產生
        CircleLD = 105,                     // 左下開始 繞圈方式產生
        CircleRU = 106,                     // 右上開始 繞圈方式產生
        VerticalL = 107,                    // 左邊開始 水平產生
        VerticalR = 108,                    // 右邊開始 水平產生
        LinkVertL = 109,                    // 左邊開始 水平接續產生
        LinkVertR = 110,                    // 右邊開始 水平接續產生
        HorizontalD = 111,                  // 下方開始 垂直產生
        HorizontalU = 112,                  // 上方開始 垂直產生
        LinkHorD = 113,                     // 下方開始 垂直接續產生
        LinkHorU = 114,                     // 上方開始 垂直接續產生
        HorTwin = 115,                      // 垂直生2個
        VertTwin = 116,                     // 水平生2個
        LinkHorTwin = 117,                  // 垂直接續產生 每次2個 到最後
        LinkVertTwin = 118,                 // 水平接續產生 每次2個 到最後
        TriangleLD = 119,                   // 左下開始 三角形
        TriangleRD = 120,                   // 右下開始 三角形
        TriangleLU = 121,                   // 左上開始 三角形
        TriangleRU = 122,                   // 右上開始 三角形
        BevelL = 123,                       // 左邊開始 45度斜角
        BevelR = 124,                       // 右邊開始 45度斜角

        //反向
        ReLineL = 201,                       // 左到右 直線產生
        ReLineR = 202,                       // 右到左 直線產生
        ReLinkLineL = 203,                   // 左到右 > 右到左 接續產生
        ReLinkLineR = 204,                   // 右到左 > 左到右 接續產生
        ReCircleLD = 205,                    // 左下開始 繞圈方式產生
        ReCircleRU = 206,                    // 右上開始 繞圈方式產生
        ReVerticalL = 207,                   // 左邊開始 水平產生
        ReVerticalR = 208,                   // 右邊開始 水平產生
        ReLinkVertL = 209,                   // 左邊開始 水平接續產生
        ReLinkVertR = 210,                   // 右邊開始 水平接續產生
        ReHorizontalD = 211,                 // 下方開始 垂直產生
        ReHorizontalU = 212,                 // 上方開始 垂直產生
        ReLinkHorD = 213,                    // 下方開始 垂直接續產生
        ReLinkHorU = 214,                    // 上方開始 垂直接續產生
        ReHorTwin = 215,                     // 垂直生2個
        ReVertTwin = 216,                    // 水平生2個
        ReLinkHorTwin = 217,                 // 垂直接續產生 每次2個 到最後
        ReLinkVertTwin = 218,                // 水平接續產生 每次2個 到最後
        ReTriangleLD = 219,                   // 左下開始 三角形
        ReTriangleRD = 220,                   // 右下開始 三角形
        ReTriangleLU = 221,                   // 左上開始 三角形
        ReTriangleRU = 222,                   // 右上開始 三角形
        ReBevelL = 223,                       // 左邊開始 45度斜角
        ReBevelR = 224,                       // 右邊開始 45度斜角

        ByNum = 1,                        // 一次產生 多少數量
        Random = 2,                       // 隨機
 */

public class SpawnController : MonoBehaviour
{
    private PoolManager poolManager;        // 物件池
    private HoleState holeState;            // 地洞狀態
    private MiceSpawner miceSpawner;        // 老鼠產生器
    private IEnumerator coroutine;          // 存放協程用
    private IEnumerator lastCoroutine;      // 存放上一個協程用

    private string holeRoot;                // 地洞的路徑值
    private int holeIndex;                  // 地洞Index

    public GameObject[] Hole;               // 存放場景每個地洞
    public GameObject myPanel;              // Battle Panel

    public int holeLimit;                   // 地洞上限
    public int spawnCount;                  // 預設產生數量
    public float spawnTime;                 // 老鼠產生速度
    public float intervalTime;              // 產生間隔速度
    public float lerpTime;                  // 間隔加速度

    //public int NGUIDepth;                   // 物件深度

    public bool spawnFlag;                  // 是否產生 基本鼠的Flag
    public bool testFlag;                   // grandmonther know it

    public SpawnMode spawnMode = SpawnMode.EasyMode;        // 正式版 要改private
    public SpawnStatus spawnStatus = SpawnStatus.LineL;
    public MissionMode missionMode = MissionMode.Closed;

    public enum SpawnMode : byte
    {
        Random = 0,
        EasyMode = 1,
        NormalMode = 2,
        HardMode = 3,
        CarzyMode = 4,
        MissionMode = 5,
        EndTimeMode = 6,
        HelpMode = 7,
    }

    public enum MissionMode : byte
    {
        Closed = 0,
        Open = 1,
    }

    void Start()
    {
        poolManager = GetComponent<PoolManager>();
        miceSpawner = GetComponent<MiceSpawner>();

        Global.photonService.ApplyDamageEvent += OnApplyDamageEvent;
        spawnFlag = true;

        holeIndex = 0;
        Global.MiceCount = 0;
        intervalTime = 1;
        holeRoot = "GameUI/Camera/Battle(Panel)/Hole";
    }

    void Update()
    {
        if (poolManager.mergeFlag && poolManager.poolingFlag && spawnFlag)          // 如果 物件池初始化完成 且 可以產生
        {
            spawnFlag = false;
            if ((byte)spawnStatus > 0 && (byte)spawnStatus < 200)
            {
                Spawn(spawnStatus, 1, spawnTime, lerpTime, spawnCount);                 // 1D
            }
            else if ((byte)spawnStatus > 200 && (byte)spawnStatus <= 255)
            {
                Spawn(spawnStatus, 1, spawnTime, lerpTime, spawnCount);                 // 2D                
            }
            spawnStatus = (byte)spawnStatus + (SpawnStatus)1;                           // 執行完畢後+1下次換別的 或用Random >10 2D時候換 2D Spwan
            
        }

        if (missionMode == MissionMode.Open)    // 要寫到別的
        {
            // 任務開始時
        }
    }


    private void ChangeSpawnMode(SpawnMode spawnMode)
    {
        switch (spawnMode)
        {
            case SpawnMode.EasyMode:
                // to do change speed etc ..
                break;
            case SpawnMode.NormalMode:
                break;
            case SpawnMode.HardMode:
                break;
            case SpawnMode.CarzyMode:
                break;
            case SpawnMode.EndTimeMode:
                break;
        }
    }

    /// <summary>
    /// 1D老鼠產生器 (產生方式,老鼠ID,速度,加速度,數量)
    /// 加速度0~1
    /// </summary>
    private void Spawn(SpawnStatus spawnStatus, int miceID, float spawnTime, float lerpTime, int spawnCount)
    {
        Spawn(spawnStatus, miceID, spawnTime, 0, lerpTime, spawnCount);
    }

    /// <summary>
    /// 2D老鼠產生器 (產生方式,老鼠ID,速度,間隔時間,加速度,數量)
    /// 加速度0~1
    /// </summary>
    private void Spawn(SpawnStatus spawnStatus, int miceID, float spawnTime, float intervalTime, float lerpTime, int spawnCount)
    {
        switch (spawnStatus)              // 產生模式選擇
        {
            case SpawnStatus.LineL:
                {
                    //coroutine = miceSpawner.ReSpawnBy1D(miceID, SpawnData.aLineL, spawnTime, lerpTime, amount);
                    coroutine = miceSpawner.SpawnBy1D(miceID, SpawnData.aLineL, spawnTime, lerpTime, spawnCount);
                    lastCoroutine = coroutine;
                    StartCoroutine(coroutine);
                    break;
                }
            case SpawnStatus.LineR:
                {
                    coroutine = miceSpawner.SpawnBy1D(miceID, SpawnData.aLineR, spawnTime, lerpTime, spawnCount);
                    lastCoroutine = coroutine;
                    StartCoroutine(coroutine);
                    break;
                }
        }
    }


    void OnApplyDamageEvent(int miceID)     // 收到技能攻擊 (目前是測試數值 扣分)
    {

        // Spawn((byte)SpawnStatus.Circle, miceID, speed, 10); 要改

    }
}
