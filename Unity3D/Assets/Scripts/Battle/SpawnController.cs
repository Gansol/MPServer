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


    protected int level;                  // 產生老鼠的種類級別 (越大越多種類)
    private bool randSpawn;
    private bool isSyncStart;

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

        level = 1;
        randSpawn = true;
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

                switch (level)
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
            if (battleManager.score < 200 && battleManager.maxScore < 500)        // 簡單模式     
            {
                ChangeSpawnMode(SpawnMode.EasyMode);
                level = 1;
            }
            else if (battleManager.gameTime > 300)                              // 強制結束模式
            {
                ChangeSpawnMode(SpawnMode.EndTimeMode);
            }
            else if (battleManager.maxScore > 1200 && battleManager.combo < 75 && battleManager.combo > 25) // 如果已經到瘋狂模式過 但是斷康了
            {                                                                   // Combo<75時回到 困難模式
                ChangeSpawnMode(SpawnMode.HardMode);
                level = 3;
            }
            else if (battleManager.maxScore > 1200 && battleManager.combo < 25 && battleManager.maxScore < 1500) // 如果已經到瘋狂模式過 但是斷康了
            {                                                                   // Combo<25時回到 普通模式
                ChangeSpawnMode(SpawnMode.NormalMode);
                level = 2;
            }
            else if (battleManager.score > 1200 && battleManager.combo > 100)    // 瘋狂模式
            {
                ChangeSpawnMode(SpawnMode.CarzyMode);
                level = 4;
            }
            else if (battleManager.maxScore > 800 && battleManager.combo < 75 && battleManager.combo > 50)     // 如果已經到困難模式過 但是斷康了
            {                                                                                                  // Combo<50時回到 困難模式
                ChangeSpawnMode(SpawnMode.HardMode);
                level = 3;
            }
            else if (battleManager.maxScore > 800 && battleManager.combo < 50)    // 如果已經到困難模式過 但是斷康了
            {                                                                      // Combo<50時回到 普通模式
                ChangeSpawnMode(SpawnMode.NormalMode);
                level = 2;
            }
            else if (battleManager.score > 800 && battleManager.combo > 75)     // 困難模式
            {
                ChangeSpawnMode(SpawnMode.HardMode);
                level = 3;
            }
            else if (battleManager.score > 500 && battleManager.combo > 50)     // 普通模式
            {
                ChangeSpawnMode(SpawnMode.NormalMode);
                level = 2;
            }
            #endregion
        }
    }

    protected void SelectStatus(int level)  // Select SpawnStatus 的副程式
    {
        switch (level)
        {
            case 1: // 1D形狀
                spawnStatus = (byte)100 + (SpawnStatus)Random.Range(0, 6) + 1;   // 1~6                     
                break;
            case 2: // 反向 1D形狀
                spawnStatus = (byte)150 + (SpawnStatus)Random.Range(0, 6) + 1;   // 1~6                     
                break;
            case 3: // 2D形狀
                spawnStatus = (byte)200 + (SpawnStatus)Random.Range(0, 12) + 1;   // 1~12                       
                break;
            case 4:// 反向 2D形狀
                spawnStatus = (byte)226 + (SpawnStatus)Random.Range(0, 12) + 1;   // 1~12                       
                break;
            case 5: // 自訂形狀
                spawnStatus = (byte)212 + (SpawnStatus)Random.Range(0, 6) + 1;   // 1~6                        
                break;
            case 6: // 反向 自訂
                spawnStatus = (byte)238 + (SpawnStatus)Random.Range(0, 6) + 1;   // 1~6                      
                break;
        }
    }

    protected void ChangeSpawnMode(SpawnMode mode)
    {
        switch (mode)
        {
            case SpawnMode.EasyMode:
                spawnMode = SpawnMode.EasyMode;
                spawnCount = 6;
                lerpTime = 0.035f;
                spawnTime = 0.35f;
                intervalTime = 1f;

                break;

            case SpawnMode.NormalMode:
                spawnMode = SpawnMode.NormalMode;
                spawnCount = 9;
                lerpTime = 0.065f;
                spawnTime = 0.5f;
                intervalTime = 2f;

                break;

            case SpawnMode.HardMode:
                spawnMode = SpawnMode.HardMode;
                spawnCount = 12;
                lerpTime = 0.055f;
                spawnTime = 0.4f;
                intervalTime = 3f;

                break;

            case SpawnMode.CarzyMode:
                spawnMode = SpawnMode.CarzyMode;
                spawnCount = 12;
                lerpTime = 0.30f;
                spawnTime = 0.35f;
                intervalTime = 2f;
                break;

            case SpawnMode.EndTimeMode:
                spawnMode = SpawnMode.EndTimeMode;
                spawnCount = 24;
                lerpTime = 0.075f;
                spawnTime = 0.25f;
                intervalTime = 2f;
                break;
        }
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
    protected void Spawn(SpawnStatus spawnStatus, string miceName, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool isSkill)
    {
        Random.seed = unchecked((int)System.DateTime.Now.Ticks);

        if ((byte)spawnStatus < 200)
        {
            switch (spawnStatus)              // 產生模式選擇
            {
                #region Case 1D
                case SpawnStatus.Random:
                    {
                        RunCoroutine(coroutine = miceSpawner.SpawnByRandom(miceName, SpawnData.aLineL, spawnTime, intervalTime, lerpTime, spawnCount, isSkill));
                        break;
                    }
                case SpawnStatus.LineL:
                    {
                        RunCoroutine(coroutine = miceSpawner.SpawnBy1D(miceName, SpawnData.aLineL, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aLineL.Length), isSkill));
                        break;
                    }
                case SpawnStatus.LineR:
                    {
                        RunCoroutine(coroutine = miceSpawner.SpawnBy1D(miceName, SpawnData.aLineR, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aLineR.Length), isSkill));
                        break;
                    }
                case SpawnStatus.LinkLineL:
                    {
                        RunCoroutine(coroutine = miceSpawner.SpawnBy1D(miceName, SpawnData.aLinkLineL, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aLinkLineL.Length), isSkill));
                        break;
                    }
                case SpawnStatus.LinkLineR:
                    {
                        RunCoroutine(coroutine = miceSpawner.SpawnBy1D(miceName, SpawnData.aLinkLineR, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aLinkLineR.Length), isSkill));
                        break;
                    }
                case SpawnStatus.CircleLD:
                    {
                        RunCoroutine(coroutine = miceSpawner.SpawnBy1D(miceName, SpawnData.aCircleLD, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aCircleLD.Length), isSkill));
                        break;
                    }
                case SpawnStatus.CircleRU:
                    {
                        RunCoroutine(coroutine = miceSpawner.SpawnBy1D(miceName, SpawnData.aCircleRU, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aCircleRU.Length), isSkill));
                        break;
                    }
                #endregion

                #region Case Opposite 1D
                case SpawnStatus.ReLineL:
                    {
                        RunCoroutine(coroutine = miceSpawner.ReSpawnBy1D(miceName, SpawnData.aLineL, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aCircleRU.Length) + 1, isSkill));
                        break;
                    }
                case SpawnStatus.ReLineR:
                    {
                        RunCoroutine(coroutine = miceSpawner.ReSpawnBy1D(miceName, SpawnData.aLineR, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aCircleRU.Length) + 1, isSkill));
                        break;
                    }
                case SpawnStatus.ReLinkLineL:
                    {
                        RunCoroutine(coroutine = miceSpawner.ReSpawnBy1D(miceName, SpawnData.aLinkLineL, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aCircleRU.Length) + 1, isSkill));
                        break;
                    }
                case SpawnStatus.ReLinkLineR:
                    {
                        RunCoroutine(coroutine = miceSpawner.ReSpawnBy1D(miceName, SpawnData.aLinkLineR, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aCircleRU.Length) + 1, isSkill));
                        break;
                    }
                case SpawnStatus.ReCircleLD:
                    {
                        RunCoroutine(coroutine = miceSpawner.ReSpawnBy1D(miceName, SpawnData.aCircleLD, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aCircleRU.Length) + 1, isSkill));
                        break;
                    }
                case SpawnStatus.ReCircleRU:
                    {
                        RunCoroutine(coroutine = miceSpawner.ReSpawnBy1D(miceName, SpawnData.aCircleRU, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aCircleRU.Length) + 1, isSkill));
                        break;
                    }
                #endregion
            }
        }
        else
        {
            switch (spawnStatus)              // 產生模式選擇
            {
                #region Case 2D
                case SpawnStatus.VerticalL:
                    {
                        RunCoroutine(coroutine = miceSpawner.SpawnBy2D(miceName, SpawnData.aVertL2D, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aVertL2D.GetLength(0)), Random.Range(0, SpawnData.aVertL2D.GetLength(1)), isSkill));
                        break;
                    }
                case SpawnStatus.VerticalR:
                    {
                        RunCoroutine(coroutine = miceSpawner.SpawnBy2D(miceName, SpawnData.aVertR2D, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aVertR2D.GetLength(0)), Random.Range(0, SpawnData.aVertR2D.GetLength(1)), isSkill));
                        break;
                    }
                case SpawnStatus.LinkVertL:
                    {
                        RunCoroutine(coroutine = miceSpawner.SpawnBy2D(miceName, SpawnData.aLinkVertL2D, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aLinkVertL2D.GetLength(0)), Random.Range(0, SpawnData.aLinkVertL2D.GetLength(1)), isSkill));
                        break;
                    }
                case SpawnStatus.LinkVertR:
                    {
                        RunCoroutine(coroutine = miceSpawner.SpawnBy2D(miceName, SpawnData.aLinkVertR2D, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aLinkVertR2D.GetLength(0)), Random.Range(0, SpawnData.aLinkVertR2D.GetLength(1)), isSkill));
                        break;
                    }
                case SpawnStatus.HorizontalD:
                    {
                        RunCoroutine(coroutine = miceSpawner.SpawnBy2D(miceName, SpawnData.aHorD2D, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aHorD2D.GetLength(0)), Random.Range(0, SpawnData.aHorD2D.GetLength(1)), isSkill));
                        break;
                    }
                case SpawnStatus.HorizontalU:
                    {
                        RunCoroutine(coroutine = miceSpawner.SpawnBy2D(miceName, SpawnData.aHorU2D, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aHorU2D.GetLength(0)), Random.Range(0, SpawnData.aHorU2D.GetLength(1)), isSkill));
                        break;
                    }
                case SpawnStatus.LinkHorD:
                    {
                        RunCoroutine(coroutine = miceSpawner.SpawnBy2D(miceName, SpawnData.aLinkHorD2D, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aLinkHorD2D.GetLength(0)), Random.Range(0, SpawnData.aLinkHorD2D.GetLength(1)), isSkill));
                        break;
                    }
                case SpawnStatus.LinkHorU:
                    {
                        RunCoroutine(coroutine = miceSpawner.SpawnBy2D(miceName, SpawnData.aLinkHorU2D, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aLinkHorU2D.GetLength(0)), Random.Range(0, SpawnData.aLinkHorU2D.GetLength(1)), isSkill));
                        break;
                    }
                case SpawnStatus.HorTwin:
                    {
                        RunCoroutine(coroutine = miceSpawner.SpawnBy2D(miceName, SpawnData.aHorTwin2D, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aHorTwin2D.GetLength(0)), Random.Range(0, SpawnData.aHorTwin2D.GetLength(1)), isSkill));
                        break;
                    }
                case SpawnStatus.VertTwin:
                    {
                        RunCoroutine(coroutine = miceSpawner.SpawnBy2D(miceName, SpawnData.aVertTwin2D, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aVertTwin2D.GetLength(0)), Random.Range(0, SpawnData.aVertTwin2D.GetLength(1)), isSkill));
                        break;
                    }
                case SpawnStatus.LinkHorTwin:
                    {
                        RunCoroutine(coroutine = miceSpawner.SpawnBy2D(miceName, SpawnData.aLinkHorTwin2D, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aLinkHorTwin2D.GetLength(0)), Random.Range(0, SpawnData.aLinkHorTwin2D.GetLength(1)), isSkill));
                        break;
                    }
                case SpawnStatus.LinkVertTwin:
                    {
                        RunCoroutine(coroutine = miceSpawner.SpawnBy2D(miceName, SpawnData.aLinkVertTwin2D, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aLinkVertTwin2D.GetLength(0)), Random.Range(0, SpawnData.aLinkVertTwin2D.GetLength(1)), isSkill));
                        break;
                    }
                #endregion

                #region Case Opposite 2D
                case SpawnStatus.ReVerticalL:
                    {
                        RunCoroutine(coroutine = miceSpawner.ReSpawnBy2D(miceName, SpawnData.aVertL2D, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aVertL2D.GetLength(0)) + 1, Random.Range(0, SpawnData.aVertL2D.GetLength(1)) + 1, isSkill));
                        break;
                    }
                case SpawnStatus.ReVerticalR:
                    {
                        RunCoroutine(coroutine = miceSpawner.ReSpawnBy2D(miceName, SpawnData.aVertR2D, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aVertR2D.GetLength(0)) + 1, Random.Range(0, SpawnData.aVertR2D.GetLength(1)) + 1, isSkill));
                        break;
                    }
                case SpawnStatus.ReLinkVertL:
                    {
                        RunCoroutine(coroutine = miceSpawner.ReSpawnBy2D(miceName, SpawnData.aLinkVertL2D, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aLinkVertL2D.GetLength(0)) + 1, Random.Range(0, SpawnData.aLinkVertL2D.GetLength(1)) + 1, isSkill));
                        break;
                    }
                case SpawnStatus.ReLinkVertR:
                    {
                        RunCoroutine(coroutine = miceSpawner.ReSpawnBy2D(miceName, SpawnData.aLinkVertR2D, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aLinkVertR2D.GetLength(0)) + 1, Random.Range(0, SpawnData.aLinkVertR2D.GetLength(1)) + 1, isSkill));
                        break;
                    }
                case SpawnStatus.ReHorizontalD:
                    {
                        RunCoroutine(coroutine = miceSpawner.ReSpawnBy2D(miceName, SpawnData.aHorD2D, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aHorD2D.GetLength(0)) + 1, Random.Range(0, SpawnData.aHorD2D.GetLength(1)) + 1, isSkill));
                        break;
                    }
                case SpawnStatus.ReHorizontalU:
                    {
                        RunCoroutine(coroutine = miceSpawner.ReSpawnBy2D(miceName, SpawnData.aHorU2D, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aHorU2D.GetLength(0)) + 1, Random.Range(0, SpawnData.aHorU2D.GetLength(1)) + 1, isSkill));
                        break;
                    }
                case SpawnStatus.ReLinkHorD:
                    {
                        RunCoroutine(coroutine = miceSpawner.ReSpawnBy2D(miceName, SpawnData.aLinkHorD2D, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aLinkHorD2D.GetLength(0)) + 1, Random.Range(0, SpawnData.aLinkHorD2D.GetLength(1)) + 1, isSkill));
                        break;
                    }
                case SpawnStatus.ReLinkHorU:
                    {
                        RunCoroutine(coroutine = miceSpawner.ReSpawnBy2D(miceName, SpawnData.aLinkHorU2D, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aLinkHorU2D.GetLength(0)) + 1, Random.Range(0, SpawnData.aLinkHorU2D.GetLength(1)) + 1, isSkill));
                        break;
                    }
                case SpawnStatus.ReHorTwin:
                    {
                        RunCoroutine(coroutine = miceSpawner.ReSpawnBy2D(miceName, SpawnData.aHorTwin2D, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aHorTwin2D.GetLength(0)) + 1, Random.Range(0, SpawnData.aHorTwin2D.GetLength(1)) + 1, isSkill));
                        break;
                    }
                case SpawnStatus.ReVertTwin:
                    {
                        RunCoroutine(coroutine = miceSpawner.ReSpawnBy2D(miceName, SpawnData.aVertTwin2D, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aVertTwin2D.GetLength(0)) + 1, Random.Range(0, SpawnData.aVertTwin2D.GetLength(1)) + 1, isSkill));
                        break;
                    }
                case SpawnStatus.ReLinkHorTwin:
                    {
                        RunCoroutine(coroutine = miceSpawner.ReSpawnBy2D(miceName, SpawnData.aLinkHorTwin2D, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aLinkHorTwin2D.GetLength(0)) + 1, Random.Range(0, SpawnData.aLinkHorTwin2D.GetLength(1)) + 1, isSkill));
                        break;
                    }
                case SpawnStatus.ReLinkVertTwin:
                    {
                        RunCoroutine(coroutine = miceSpawner.ReSpawnBy2D(miceName, SpawnData.aLinkVertTwin2D, spawnTime, intervalTime, lerpTime, spawnCount, Random.Range(0, SpawnData.aLinkVertTwin2D.GetLength(0)) + 1, Random.Range(0, SpawnData.aLinkVertTwin2D.GetLength(1)) + 1, isSkill));
                        break;
                    }
                #endregion

                #region Case Custom
                case SpawnStatus.TriangleLD:
                    {
                        RunCoroutine(coroutine = miceSpawner.SpawnByCustom(miceName, SpawnData.jaTriangleLD2D, spawnTime, intervalTime, lerpTime, spawnCount, isSkill));
                        break;
                    }
                case SpawnStatus.TriangleLU:
                    {
                        RunCoroutine(coroutine = miceSpawner.SpawnByCustom(miceName, SpawnData.jaTriangleLU2D, spawnTime, intervalTime, lerpTime, spawnCount, isSkill));
                        break;
                    }
                case SpawnStatus.TriangleRD:
                    {
                        RunCoroutine(coroutine = miceSpawner.SpawnByCustom(miceName, SpawnData.jaTriangleRD2D, spawnTime, intervalTime, lerpTime, spawnCount, isSkill));
                        break;
                    }
                case SpawnStatus.TriangleRU:
                    {
                        RunCoroutine(coroutine = miceSpawner.SpawnByCustom(miceName, SpawnData.jaTriangleRU2D, spawnTime, intervalTime, lerpTime, spawnCount, isSkill));
                        break;
                    }
                case SpawnStatus.BevelL:
                    {
                        RunCoroutine(coroutine = miceSpawner.SpawnByCustom(miceName, SpawnData.jaBevelL2D, spawnTime, intervalTime, lerpTime, spawnCount, isSkill));
                        break;
                    }
                case SpawnStatus.BevelR:
                    {
                        RunCoroutine(coroutine = miceSpawner.SpawnByCustom(miceName, SpawnData.jaBevelR2D, spawnTime, intervalTime, lerpTime, spawnCount, isSkill));
                        break;
                    }
                #endregion

                #region Case Opposite Custom
                case SpawnStatus.ReTriangleLD:
                    {
                        RunCoroutine(coroutine = miceSpawner.ReSpawnByCustom(miceName, SpawnData.jaTriangleLD2D, spawnTime, intervalTime, lerpTime, spawnCount, isSkill));
                        break;
                    }
                case SpawnStatus.ReTriangleLU:
                    {
                        RunCoroutine(coroutine = miceSpawner.ReSpawnByCustom(miceName, SpawnData.jaTriangleLU2D, spawnTime, intervalTime, lerpTime, spawnCount, isSkill));
                        break;
                    }
                case SpawnStatus.ReTriangleRD:
                    {
                        RunCoroutine(coroutine = miceSpawner.ReSpawnByCustom(miceName, SpawnData.jaTriangleRD2D, spawnTime, intervalTime, lerpTime, spawnCount, isSkill));
                        break;
                    }
                case SpawnStatus.ReTriangleRU:
                    {
                        RunCoroutine(coroutine = miceSpawner.ReSpawnByCustom(miceName, SpawnData.jaTriangleRU2D, spawnTime, intervalTime, lerpTime, spawnCount, isSkill));
                        break;
                    }
                case SpawnStatus.ReBevelL:
                    {
                        RunCoroutine(coroutine = miceSpawner.ReSpawnByCustom(miceName, SpawnData.jaBevelL2D, spawnTime, intervalTime, lerpTime, spawnCount, isSkill));
                        break;
                    }
                case SpawnStatus.ReBevelR:
                    {
                        RunCoroutine(coroutine = miceSpawner.ReSpawnByCustom(miceName, SpawnData.jaBevelR2D, spawnTime, intervalTime, lerpTime, spawnCount, isSkill));
                        break;
                    }
                #endregion
            }
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
