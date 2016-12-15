using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MPProtocol;

/*
 * 
 * 
 * 現在一切都是亂數，要改成邏輯判斷
 * 全都亂寫的 沒AI
 */

public class SpawnController : MonoBehaviour
{
    private PoolManager poolManager;        // 物件池
    private MiceSpawner miceSpawner;        // 老鼠產生器
    private BattleManager battleManager;
    private IEnumerator coroutine;          // 存放協程用
    private IEnumerator lastCoroutine;      // 存放上一個協程用
    private IEnumerator randCoroutine;      // 存放存隨機產生的協程

    public GameObject myPanel;              // Battle Panel

    public int holeLimit;                   // 地洞上限
    public int spawnCount;                  // 預設產生數量
    public float spawnTime;                 // 老鼠產生速度

    public float intervalTime;              // 間隔時間
    [Range(0.0F, 1.0F)]
    public float lerpTime;                  // 老鼠產生間隔加速度


    public SpawnMode spawnMode = SpawnMode.EasyMode;        // 正式版 要改private
    public SpawnStatus spawnStatus = SpawnStatus.LineL;


    protected int difficultyLevel;                  // 產生老鼠的種類級別 (越大越多種類)
    //private bool randSpawn;
    private bool isSyncStart;

    int normalScore = 500, normalMaxScore = 1000, normalCombo = 50, normalTime = 120;
    int hardScore = 1000, hardMaxScore = 3000, hardCombo = 75, hardTime = 200;
    int carzyScore = 3000, carzyMaxScore = 10000, carzyCombo = 100, carzyTime = 270;
    int gameOverTime = 300;

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

        Global.photonService.ApplySkillEvent += OnApplySkill;
        Global.photonService.LoadSceneEvent += OnLoadScene;
        Global.spawnFlag = true;

        Global.MiceCount = 0;

        difficultyLevel = 1;
        //randSpawn = true;
        isSyncStart = true;
    }

    void Update()
    {
        if (poolManager.mergeFlag && poolManager.poolingFlag && isSyncStart)
        {
            isSyncStart = false;
            Global.photonService.SyncGameStart();
        }

        if (Global.isGameStart)
        {
            // Debug.Log("Game Start!");
            #region   -- 隨機產生老鼠 --
            if (spawnMode != SpawnMode.EasyMode && Global.spawnFlag)
            {
                randCoroutine = Spawn(SpawnStatus.Random, "RabbitMice", spawnTime, intervalTime, lerpTime, Random.Range(1, 4), true, 1);
                StartCoroutine(randCoroutine);
            }
            #endregion

            #region   -- 產生老鼠 --
            if (poolManager.mergeFlag && poolManager.poolingFlag && Global.spawnFlag)          // 如果 物件池初始化完成 且 可以產生
            {
                Global.spawnFlag = false;

                Spawn(spawnStatus, "EggMice", spawnTime, intervalTime, lerpTime, spawnCount, false);                 // 2D                

                Random.seed = unchecked((int)System.DateTime.Now.Ticks);

                switch (difficultyLevel)
                {
                    case 1:
                        SelectStatus(Random.Range(0, 2) + 1);   // 1~2
                        break;
                    case 2:
                        SelectStatus(Random.Range(0, 3) + 1);   // 1~3
                        break;
                    case 3:
                        SelectStatus(Random.Range(0, 4) + 1);   // 1~4
                        break;
                    case 4:
                        SelectStatus(Random.Range(0, 6) + 1);  // 1~6
                        break;
                }
            }
            #endregion

            #region Select SpawnMode 亂寫

            if (battleManager.gameTime > gameOverTime)
            {
                ChangeSpawnMode(SpawnMode.EndTimeMode);
            }
            else if ((battleManager.score > carzyScore && battleManager.combo > carzyCombo) || battleManager.maxScore > carzyMaxScore || battleManager.gameTime > carzyTime)
            {
                ChangeSpawnMode(SpawnMode.CarzyMode);
                difficultyLevel = 4;
            }
            else if ((battleManager.score > hardScore && battleManager.combo > hardCombo) || battleManager.maxScore > hardMaxScore || battleManager.gameTime > hardTime)
            {
                ChangeSpawnMode(SpawnMode.HardMode);
                difficultyLevel = 3;
            }
            else if ((battleManager.score > normalScore && battleManager.combo > normalCombo) || battleManager.maxScore > normalMaxScore || battleManager.gameTime > normalTime)
            {
                ChangeSpawnMode(SpawnMode.NormalMode);
                difficultyLevel = 2;
            }
            else if ((battleManager.score < normalScore && battleManager.combo < normalCombo) || battleManager.maxScore < normalMaxScore || battleManager.gameTime < normalTime)
            {
                ChangeSpawnMode(SpawnMode.EasyMode);
                difficultyLevel = 1;
            }
            #endregion
        }
    }

    protected void SelectStatus(int status)  // Select SpawnStatus 的副程式
    {
        switch (status)
        {
            case 1: // 1D形狀
                spawnStatus = (byte)10 + (SpawnStatus)Random.Range(0, 6) + 1;   // 1~6                     
                break;
            case 2: // 反向 1D形狀
                spawnStatus = (byte)50 + (SpawnStatus)Random.Range(0, 6) + 1;   // 1~6                     
                break;
            case 3: // 2D形狀
                spawnStatus = (byte)100 + (SpawnStatus)Random.Range(0, 12) + 1;   // 1~12                       
                break;
            case 4:// 反向 2D形狀
                spawnStatus = (byte)150 + (SpawnStatus)Random.Range(0, 12) + 1;   // 1~12                       
                break;
            case 5: // 自訂形狀
                spawnStatus = (byte)200 + (SpawnStatus)Random.Range(0, 6) + 1;   // 1~6                        
                break;
            case 6: // 反向 自訂
                spawnStatus = (byte)225 + (SpawnStatus)Random.Range(0, 6) + 1;   // 1~6                      
                break;
        }
    }

    protected void ChangeSpawnMode(SpawnMode mode)
    {
        switch (mode)
        {
            case SpawnMode.EasyMode:
                {
                    spawnCount = 6;
                    lerpTime = 0.035f;
                    spawnTime = 0.35f;
                    intervalTime = 1f;
                    break;
                }
            case SpawnMode.NormalMode:
                {
                    spawnCount = 9;
                    lerpTime = 0.065f;
                    spawnTime = 0.4f;
                    intervalTime = 1.8f;
                    break;
                }
            case SpawnMode.HardMode:
                {
                    spawnCount = 12;
                    lerpTime = 0.06f;
                    spawnTime = 0.45f;
                    intervalTime = 3f;
                    break;
                }
            case SpawnMode.CarzyMode:
                {
                    spawnCount = 12;
                    lerpTime = 0.15f;
                    spawnTime = 0.35f;
                    intervalTime = 2.5f;
                    break;
                }
            case SpawnMode.EndTimeMode:
                {
                    spawnCount = 24;
                    lerpTime = 0.075f;
                    spawnTime = 0.25f;
                    intervalTime = 2f;
                    break;
                }
        }
        spawnMode = mode;
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
    protected IEnumerator Spawn(SpawnStatus spawnStatus, string miceName, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool isSkill, float RepeatTime)
    {
        Spawn(spawnStatus, miceName, spawnTime, intervalTime, lerpTime, spawnCount, isSkill);
        yield return new WaitForSeconds(RepeatTime);
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
    public void Spawn(SpawnStatus spawnStatus, string miceName, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool isSkill)
    {
        Random.seed = unchecked((int)System.DateTime.Now.Ticks);

        if (spawnStatus < SpawnStatus.SpawnData1D)
        {
            sbyte[] data = SpawnData.GetSpawnData(SpawnStatus.Random) as sbyte[];
            RunCoroutine(coroutine = miceSpawner.SpawnByRandom(miceName, data, spawnTime, intervalTime, lerpTime, spawnCount, isSkill));
        }
        else if (spawnStatus > SpawnStatus.SpawnData1D && spawnStatus < SpawnStatus.SpawnDataRe1D)
        {
            sbyte[] data = SpawnData.GetSpawnData(spawnStatus) as sbyte[];
            RunCoroutine(coroutine = miceSpawner.SpawnBy1D(miceName, data, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, data.Length), isSkill));
        }
        else if (spawnStatus > SpawnStatus.SpawnDataRe1D && spawnStatus < SpawnStatus.SpawnData2D)
        {
            sbyte[] data = SpawnData.GetSpawnData(spawnStatus) as sbyte[];
            RunCoroutine(coroutine = miceSpawner.ReSpawnBy1D(miceName, data, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, data.Length) + 1, isSkill));
        }
        else if (spawnStatus > SpawnStatus.SpawnData2D && spawnStatus < SpawnStatus.SpawnDataRe2D)
        {
            sbyte[,] data = SpawnData.GetSpawnData(spawnStatus) as sbyte[,];
            RunCoroutine(coroutine = miceSpawner.SpawnBy2D(miceName, data, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, data.GetLength(0)), Random.Range(0, data.GetLength(1)), isSkill));
        }
        else if (spawnStatus > SpawnStatus.SpawnDataRe2D && spawnStatus < SpawnStatus.SpawnDataCustom)
        {
            sbyte[,] data = SpawnData.GetSpawnData(spawnStatus) as sbyte[,];
            RunCoroutine(coroutine = miceSpawner.ReSpawnBy2D(miceName, data, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, data.GetLength(0)) + 1, Random.Range(0, data.GetLength(1)) + 1, isSkill));
        }
        else if (spawnStatus > SpawnStatus.SpawnDataCustom && spawnStatus < SpawnStatus.SpawnDataReCustom)
        {
            sbyte[][] data = SpawnData.GetSpawnData(spawnStatus) as sbyte[][];
            RunCoroutine(coroutine = miceSpawner.SpawnByCustom(miceName, data, spawnTime, intervalTime, lerpTime, spawnCount, isSkill));
        }
        else if (spawnStatus > SpawnStatus.SpawnDataReCustom)
        {
            sbyte[][] data = SpawnData.GetSpawnData(spawnStatus) as sbyte[][];
            RunCoroutine(coroutine = miceSpawner.ReSpawnByCustom(miceName, data, spawnTime, intervalTime, lerpTime, spawnCount, isSkill));
        }
        else
        {
            Debug.LogError("Unknown Spawn Data !");
        }
    }

    void RunCoroutine(IEnumerator coroutine) // 開始協程並儲存
    {
        lastCoroutine = coroutine;
        StartCoroutine(coroutine);
    }

    void OnApplySkill(string miceName)     // 收到技能攻擊 (目前是測試數值 扣分)
    {
        Debug.Log("OnApplySkill miceName:" + miceName);
        // StopCoroutine(coroutine);
        Spawn(spawnStatus, miceName, spawnTime, intervalTime, lerpTime, spawnCount, true);                 // 2D              

    }

    void OnLoadScene()
    {
        Global.photonService.ApplySkillEvent -= OnApplySkill;
        Global.photonService.LoadSceneEvent -= OnLoadScene;
    }

    void OnGameStart()
    {
        Global.isGameStart = true;
    }

    /* 超亂亂數
 * using System;
public Guid RNGGuid() // 超亂亂數
{
    var rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
    var data = new byte[16];
    rng.GetBytes(data);
    return new Guid(data);
}
*/
}
