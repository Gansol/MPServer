using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MPProtocol;

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
 * 
 * 
 * 現在一切都是亂數，要改成邏輯判斷
 * 
 */

public class SpawnController : MonoBehaviour
{
    private PoolManager poolManager;        // 物件池
    private HoleState holeState;            // 地洞狀態
    private MiceSpawner miceSpawner;        // 老鼠產生器
    private BattleManager battleManager;
    private IEnumerator coroutine;          // 存放協程用
    private IEnumerator lastCoroutine;      // 存放上一個協程用

    private string holeRoot;                // 地洞的路徑值
    private int holeIndex;                  // 地洞Index

    //public GameObject[] Hole;               // 存放場景每個地洞
    public GameObject myPanel;              // Battle Panel

    public int holeLimit;                   // 地洞上限
    public int spawnCount;                  // 預設產生數量
    public float spawnTime;                 // 老鼠產生速度

    public float intervalTime = 1;              // 產生間隔速度
    [Range(0.0F, 1.0F)]
    public float lerpTime;                  // 間隔加速度

    //public int NGUIDepth;                   // 物件深度

    //public bool testFlag;                   // grandmonther know it

    public SpawnMode spawnMode = SpawnMode.EasyMode;        // 正式版 要改private
    public SpawnStatus spawnStatus = SpawnStatus.LineL;


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



    void Start()
    {
        poolManager = GetComponent<PoolManager>();
        miceSpawner = GetComponent<MiceSpawner>();
        battleManager = GetComponent<BattleManager>();

        Global.photonService.ApplyDamageEvent += OnApplyDamageEvent;
        Global.spawnFlag = true;

        holeIndex = 0;
        Global.MiceCount = 0;
        holeRoot = "GameUI/Camera/Battle(Panel)/Hole";
    }

    void Update()
    {
        //Debug.Log("STATUS:" + spawnStatus + "byte:" + (byte)spawnStatus);
        #region Select SpawnMode
        if (battleManager.score < 50)
        {
            ChangeSpawnMode(SpawnMode.EasyMode);
        }
        else if (battleManager.score > 200 && battleManager.combo > 100)
        {
            ChangeSpawnMode(SpawnMode.CarzyMode);
        }
        else if (battleManager.score > 100 && battleManager.combo > 50)
        {
            ChangeSpawnMode(SpawnMode.HardMode);
        }
        else if (battleManager.score > 50 && battleManager.combo > 25)
        {
            ChangeSpawnMode(SpawnMode.NormalMode);
        }
        #endregion


        if (poolManager.mergeFlag && poolManager.poolingFlag && Global.spawnFlag)          // 如果 物件池初始化完成 且 可以產生
        {
            Global.spawnFlag = false;
            Global.isGameStart = true;
            if ((byte)spawnStatus > 0 && (byte)spawnStatus < 200)
            {
                Spawn(spawnStatus, 1, spawnTime, lerpTime, spawnCount);                 // 1D
            }
            else if ((byte)spawnStatus > 200 && (byte)spawnStatus <= 255)
            {
                Spawn(spawnStatus, 1, spawnTime, intervalTime, lerpTime, spawnCount);                 // 2D                
            }

            if ((byte)spawnStatus < 200)
            {
                switch (Random.Range(0, 2) + 1)
                {
                    case 1:
                        spawnStatus = (byte)100 + (SpawnStatus)Random.Range(0, 6) + 1;   // 1~6                        // 執行完畢後+1下次換別的 或用Random >10 2D時候換 2D Spwan
                        break;
                    case 2:
                        spawnStatus = (byte)150 + (SpawnStatus)Random.Range(0, 6) + 1;   // 1~6                        // 執行完畢後+1下次換別的 或用Random >10 2D時候換 2D Spwan
                        break;
                }
            }
            else if ((byte)spawnStatus > 200)
            {
                switch (Random.Range(0, 2) + 1)
                {
                    case 1:
                        spawnStatus = (byte)200 + (SpawnStatus)Random.Range(0, 6) + 1;   // 1~6                        // 執行完畢後+1下次換別的 或用Random >10 2D時候換 2D Spwan
                        break;
                    case 2:
                        spawnStatus = (byte)226 + (SpawnStatus)Random.Range(0, 6) + 1;   // 1~6                        // 執行完畢後+1下次換別的 或用Random >10 2D時候換 2D Spwan
                        break;
                }
            }
        }






    }


    private void ChangeSpawnMode(SpawnMode mode)
    {
        switch (mode)
        {
            case SpawnMode.EasyMode:
                spawnMode = SpawnMode.EasyMode;
                lerpTime = 0.01f;
                spawnTime = 0.25f;
                intervalTime = 1f;
                spawnCount = 12;    // 6
                break;

            case SpawnMode.NormalMode:
                spawnMode = SpawnMode.NormalMode;
                lerpTime = 0.02f;
                spawnTime = 0.75f;
                intervalTime = 0.75f;
                spawnCount = 9;
                break;

            case SpawnMode.HardMode:
                spawnMode = SpawnMode.HardMode;
                lerpTime = 0.05f;
                spawnTime = 0.5f;
                intervalTime = 0.5f;
                spawnCount = 12;
                break;

            case SpawnMode.CarzyMode:
                spawnMode = SpawnMode.CarzyMode;
                lerpTime = 0.075f;
                spawnTime = 0.25f;
                intervalTime = 0.5f;
                spawnCount = 12;
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
            #region Case 1D
            case SpawnStatus.LineL:
                {
                    RunCoroutine(coroutine = miceSpawner.SpawnBy1D(miceID, SpawnData.aLineL, spawnTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aLineL.Length)));
                    break;
                }
            case SpawnStatus.LineR:
                {
                    RunCoroutine(coroutine = miceSpawner.SpawnBy1D(miceID, SpawnData.aLineR, spawnTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aLineR.Length)));
                    break;
                }
            case SpawnStatus.LinkLineL:
                {
                    RunCoroutine(coroutine = miceSpawner.SpawnBy1D(miceID, SpawnData.aLinkLineL, spawnTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aLinkLineL.Length)));
                    break;
                }
            case SpawnStatus.LinkLineR:
                {
                    RunCoroutine(coroutine = miceSpawner.SpawnBy1D(miceID, SpawnData.aLinkLineR, spawnTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aLinkLineR.Length)));
                    break;
                }
            case SpawnStatus.CircleLD:
                {
                    RunCoroutine(coroutine = miceSpawner.SpawnBy1D(miceID, SpawnData.aCircleLD, spawnTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aCircleLD.Length)));
                    break;
                }
            case SpawnStatus.CircleRU:
                {
                    RunCoroutine(coroutine = miceSpawner.SpawnBy1D(miceID, SpawnData.aCircleRU, spawnTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aCircleRU.Length)));
                    break;
                }
            #endregion

            #region Case Opposite 1D
            case SpawnStatus.ReLineL:
                {
                    RunCoroutine(coroutine = miceSpawner.ReSpawnBy1D(miceID, SpawnData.aLineL, spawnTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.ReLineR:
                {
                    RunCoroutine(coroutine = miceSpawner.ReSpawnBy1D(miceID, SpawnData.aLineR, spawnTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.ReLinkLineL:
                {
                    RunCoroutine(coroutine = miceSpawner.ReSpawnBy1D(miceID, SpawnData.aLinkLineL, spawnTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.ReLinkLineR:
                {
                    RunCoroutine(coroutine = miceSpawner.ReSpawnBy1D(miceID, SpawnData.aLinkLineR, spawnTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.ReCircleLD:
                {
                    RunCoroutine(coroutine = miceSpawner.ReSpawnBy1D(miceID, SpawnData.aCircleLD, spawnTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.ReCircleRU:
                {
                    RunCoroutine(coroutine = miceSpawner.ReSpawnBy1D(miceID, SpawnData.aCircleRU, spawnTime, lerpTime, spawnCount));
                    break;
                }
            #endregion

            #region Case 2D
            case SpawnStatus.VerticalL:
                {
                    Debug.Log("IN CASE : " + intervalTime);
                    RunCoroutine(coroutine = miceSpawner.SpawnBy2D(miceID, SpawnData.aVertL2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.VerticalR:
                {
                    RunCoroutine(coroutine = miceSpawner.SpawnBy2D(miceID, SpawnData.aVertR2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.LinkVertL:
                {
                    RunCoroutine(coroutine = miceSpawner.SpawnBy2D(miceID, SpawnData.aLinkVertL2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.LinkVertR:
                {
                    RunCoroutine(coroutine = miceSpawner.SpawnBy2D(miceID, SpawnData.aLinkVertR2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.HorizontalD:
                {
                    RunCoroutine(coroutine = miceSpawner.SpawnBy2D(miceID, SpawnData.aHorD2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.HorizontalU:
                {
                    RunCoroutine(coroutine = miceSpawner.SpawnBy2D(miceID, SpawnData.aHorU2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.LinkHorD:
                {
                    RunCoroutine(coroutine = miceSpawner.SpawnBy2D(miceID, SpawnData.aLinkHorD2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.LinkHorU:
                {
                    RunCoroutine(coroutine = miceSpawner.SpawnBy2D(miceID, SpawnData.aLinkHorU2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.HorTwin:
                {
                    RunCoroutine(coroutine = miceSpawner.SpawnBy2D(miceID, SpawnData.aHorTwin2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.VertTwin:
                {
                    RunCoroutine(coroutine = miceSpawner.SpawnBy2D(miceID, SpawnData.aVertTwin2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.LinkHorTwin:
                {
                    RunCoroutine(coroutine = miceSpawner.SpawnBy2D(miceID, SpawnData.aLinkHorTwin2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.LinkVertTwin:
                {
                    RunCoroutine(coroutine = miceSpawner.SpawnBy2D(miceID, SpawnData.aLinkVertTwin2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            #endregion

            #region Case Opposite 2D
            case SpawnStatus.ReVerticalL:
                {
                    RunCoroutine(coroutine = miceSpawner.ReSpawnBy2D(miceID, SpawnData.aVertL2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.ReVerticalR:
                {
                    RunCoroutine(coroutine = miceSpawner.ReSpawnBy2D(miceID, SpawnData.aVertR2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.ReLinkVertL:
                {
                    RunCoroutine(coroutine = miceSpawner.ReSpawnBy2D(miceID, SpawnData.aLinkVertL2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.ReLinkVertR:
                {
                    RunCoroutine(coroutine = miceSpawner.ReSpawnBy2D(miceID, SpawnData.aLinkVertR2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.ReHorizontalD:
                {
                    RunCoroutine(coroutine = miceSpawner.ReSpawnBy2D(miceID, SpawnData.aHorD2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.ReHorizontalU:
                {
                    RunCoroutine(coroutine = miceSpawner.ReSpawnBy2D(miceID, SpawnData.aHorU2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.ReLinkHorD:
                {
                    RunCoroutine(coroutine = miceSpawner.ReSpawnBy2D(miceID, SpawnData.aLinkHorD2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.ReLinkHorU:
                {
                    RunCoroutine(coroutine = miceSpawner.ReSpawnBy2D(miceID, SpawnData.aLinkHorU2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.ReHorTwin:
                {
                    RunCoroutine(coroutine = miceSpawner.ReSpawnBy2D(miceID, SpawnData.aHorTwin2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.ReVertTwin:
                {
                    RunCoroutine(coroutine = miceSpawner.ReSpawnBy2D(miceID, SpawnData.aVertTwin2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.ReLinkHorTwin:
                {
                    RunCoroutine(coroutine = miceSpawner.ReSpawnBy2D(miceID, SpawnData.aLinkHorTwin2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.ReLinkVertTwin:
                {
                    RunCoroutine(coroutine = miceSpawner.ReSpawnBy2D(miceID, SpawnData.aLinkVertTwin2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            #endregion

            #region Case Custom
            case SpawnStatus.TriangleLD:
                {
                    RunCoroutine(coroutine = miceSpawner.SpawnByCustom(miceID, SpawnData.jaTriangleLD2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.TriangleLU:
                {
                    RunCoroutine(coroutine = miceSpawner.SpawnByCustom(miceID, SpawnData.jaTriangleLU2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.TriangleRD:
                {
                    RunCoroutine(coroutine = miceSpawner.SpawnByCustom(miceID, SpawnData.jaTriangleRD2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.TriangleRU:
                {
                    RunCoroutine(coroutine = miceSpawner.SpawnByCustom(miceID, SpawnData.jaTriangleRU2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.BevelL:
                {
                    RunCoroutine(coroutine = miceSpawner.SpawnByCustom(miceID, SpawnData.jaBevelL2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.BevelR:
                {
                    RunCoroutine(coroutine = miceSpawner.SpawnByCustom(miceID, SpawnData.jaBevelR2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            #endregion

            #region Case Opposite Custom
            case SpawnStatus.ReTriangleLD:
                {
                    RunCoroutine(coroutine = miceSpawner.ReSpawnByCustom(miceID, SpawnData.jaTriangleLD2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.ReTriangleLU:
                {
                    RunCoroutine(coroutine = miceSpawner.ReSpawnByCustom(miceID, SpawnData.jaTriangleLU2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.ReTriangleRD:
                {
                    RunCoroutine(coroutine = miceSpawner.ReSpawnByCustom(miceID, SpawnData.jaTriangleRD2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.ReTriangleRU:
                {
                    RunCoroutine(coroutine = miceSpawner.ReSpawnByCustom(miceID, SpawnData.jaTriangleRU2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.ReBevelL:
                {
                    RunCoroutine(coroutine = miceSpawner.ReSpawnByCustom(miceID, SpawnData.jaBevelL2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            case SpawnStatus.ReBevelR:
                {
                    RunCoroutine(coroutine = miceSpawner.ReSpawnByCustom(miceID, SpawnData.jaBevelR2D, spawnTime, intervalTime, lerpTime, spawnCount));
                    break;
                }
            #endregion
        }
    }


    void RunCoroutine(IEnumerator coroutine)
    {
        lastCoroutine = coroutine;
        StartCoroutine(coroutine);
    }

    void OnApplyDamageEvent(int miceID)     // 收到技能攻擊 (目前是測試數值 扣分)
    {

        // Spawn((byte)SpawnStatus.Circle, miceID, speed, 10); 要改

    }
}
