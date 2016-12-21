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


    public static int Test = 0;


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

    public float _gameDelay, _lastSpawnTime, _spawnIntervalTime;
   public SpawnState spawnState;


    // test value
    public bool tSpawnFlag, reSpawn,bTSPanel;

    public GameObject[] testBtn;

    void Start()
    {
        poolManager = GetComponent<PoolManager>();
        miceSpawner = GetComponent<MiceSpawner>();
        battleManager = GetComponent<BattleManager>();

        Global.photonService.ApplySkillEvent += OnApplySkill;
        Global.photonService.LoadSceneEvent += OnLoadScene;

        spawnState = new EasyState();
        _lastSpawnTime = 0;
        _gameDelay = 1.5f;
        _spawnIntervalTime = 1.5f;
        isSyncStart = true;
        spawnCount = 6;




        // test value
    }


    void FixedUpdate()
    {
       // spawnStatus = spawnState.GetSpawnStatus();

        // 同步開始
        if (poolManager.mergeFlag && poolManager.poolingFlag && isSyncStart)
        {
            isSyncStart = false;
            Global.photonService.SyncGameStart();
        }

        // 遊戲開始時Spawn狀態
        //if (Global.isGameStart && Time.fixedTime > (_gameDelay + _lastSpawnTime + _spawnIntervalTime) && tSpawnFlag)
        //{
        //    _lastSpawnTime = Time.fixedTime;
        //    spawnState.Spawn("EggMice", false);
        //}

        if (Global.isGameStart && Time.fixedTime > (_gameDelay + _lastSpawnTime + _spawnIntervalTime) && tSpawnFlag)
        {
            _lastSpawnTime = Time.fixedTime;
            spawnState.SpawnCustom(spawnStatus, "EggMice", spawnTime, intervalTime, lerpTime, spawnCount, false, reSpawn);
        }
    }

    void OnApplySkill(string miceName)     // 收到技能攻擊 
    {
        Debug.Log("OnApplySkill miceName:" + miceName);
        // StopCoroutine(coroutine);
        spawnState.Spawn(miceName, true);                 // 2D              

    }

    void OnLoadScene()
    {
        Global.photonService.ApplySkillEvent -= OnApplySkill;
        Global.photonService.LoadSceneEvent -= OnLoadScene;
    }

    void OnGameStart()
    {
        _lastSpawnTime = Time.fixedTime;
        Global.isGameStart = true;
    }

    public void OnBreakCombo()
    {
        _spawnIntervalTime += .1f;
        Debug.Log("Break");
    }

    public void OnHighCombo()
    {
        _spawnIntervalTime -= .1f;
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
