using UnityEngine;
using System.Collections;
using MPProtocol;

public class Newcontroller : MonoBehaviour
{

    private PoolManager poolManager;        // 物件池
    private MiceSpawner miceSpawner;        // 老鼠產生器
    private BattleManager battleManager;
    private SpawnState spawnState = null;

    private bool isSyncStart;
    private float _gameDelay, _lastSpawnTime, _spawnIntervalTime;

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
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (poolManager.mergeFlag && poolManager.poolingFlag && isSyncStart)
        {
            isSyncStart = false;
            Global.photonService.SyncGameStart();
        }

        // 遊戲開始時Spawn狀態
        if (Global.isGameStart && Time.fixedTime > (_gameDelay + _lastSpawnTime + _spawnIntervalTime))
        {
            _lastSpawnTime = Time.fixedTime;

            spawnState.Spawn("EggMice", false);
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
        Global.isGameStart = true;
    }
}
