using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using MPProtocol;
using System.Linq;
/* ***************************************************************
 * -----Copyright © 2020 Gansol Studio.  All Rights Reserved.-----
 * -----------            CC BY-NC-SA 4.0            -------------
 * -----------  @Website:  EasyUnity@blogspot.com    -------------
 * -----------  @Email:    GansolTW@gmail.com        -------------
 * -----------  @Author:   Krola.                    -------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * 
 * 
 * ***************************************************************
 *                           ChangeLog
 * 20201027 v3.0.0  繼承重構    
 * ****************************************************************/
public class BattleManager : GameSystem
{
    private AttachBtn_BattleUI UI;
    public IBattleAIState battleAIState;           // 產生AI狀態 正式版要改private
    public PlayerAIState playerAIState;             // Player狀態
    public GameObject startAnim;
    protected GameObject m_RootUI = null;

    public UISprite gameMode;

    private SpawnAI spawnAI;
    private PoolManager poolManager;        // 物件池     
    private MissionManager missionManager;  // 任務管理員
    private BattleUI battleUI;            // HUD
    private BotAI BotAI;
    //   public Dictionary<GameObject, GameObject> mice = new Dictionary<GameObject, GameObject>();

    public ENUM_BattleAIState battleState = ENUM_BattleAIState.EasyMode;        // 正式版 要改private
    public SpawnStatus spawnStatus = SpawnStatus.LineL;  // 測試用
    public int feverEnergy;           // FeverTime 能量
    private List<GameObject> hole = null;

    private Dictionary<string, Dictionary<string, object>> dictMiceUseCount;
    private Dictionary<string, Dictionary<string, object>> dictItemUseCount;
    private readonly Dictionary<int, GameObject> dictSkillBossMice = new Dictionary<int, GameObject>();

    // 老鼠使用量、能量使用量、失誤數、老鼠產生總量、最大連擊數、目前連擊
    private static short _eggMiceUsage, _energyUsage, _lostMice, _spawnCount, _maxCombo, _nowCombo;
    private static int _energy = 0, _tmpEnergy;       // 能量
    private static float _elapsedGameTime, _scoreRate = 1, _otherRate = 1;           // 經過時間、分數倍率、對手分數倍率
    private static bool _isPropected, _isReflection, _isInvincible;   // 錯誤 保護、反射、無敵 要在伺服器判斷 偷懶亂寫 
    private static readonly int _defaultStartTime = 3;     // 經過時間

    private int _healthValue, maxLife;
    // 生命 對手生命 死亡老鼠數量 連擊
    private short _life, _otherLife, _killMice, _combo;
    // 最高得分、最高得分、平均輸出、對手平均輸出、上次時間、分數、對手分數、對手能量、檢查時間、檢查時間點
    private float _maxScore, _gameScore, _myDPS, _otherDPS, _lastTime, _score, _otherScore, _otherEnergy, checkTime, checkPoint = 2;
    // 是否連擊 是否破分數紀錄 是否破Combo紀錄 同步開始
    private bool _isCombo, _isHighScore, _isHighCombo, isSyncStart;


    public bool SpawnFlag { get; set; }     // public 是給TestPanel使用的
    public int MissionCombo { get; private set; }


    public BattleManager(MPGame MPGame) : base(MPGame)
    {
        m_RootUI = GameObject.Find(Global.Scene.BattleAsset.ToString());
    }

    // Use this for initialization
    public override void Initinal()
    {
        Debug.Log("-------Battle Start-------");
        EventMaskSwitch.Init();
        FindHole();

        poolManager = m_RootUI.GetComponentInChildren<PoolManager>();
        missionManager = m_RootUI.GetComponentInChildren<MissionManager>();
        battleUI = m_MPGame.GetBattleUI();

        spawnAI = new SpawnAI(this, poolManager, hole);
        SetSpawnState(new EasyBattleAIState());
        playerAIState = new PlayerAIState(this);

        dictMiceUseCount = new Dictionary<string, Dictionary<string, object>>();
        dictItemUseCount = new Dictionary<string, Dictionary<string, object>>();

        Global.dictBattleMiceRefs.Clear();

        Global.photonService.MissionCompleteEvent += OnMissionComplete;
        Global.photonService.ApplyMissionEvent += OnApplyMission;
        Global.photonService.UpdateScoreEvent += OnUpdateScore;
        Global.photonService.OtherScoreEvent += OnOtherScore;
        Global.photonService.UpdateLifeEvent += OnUpdateLife;
        Global.photonService.GetOpponentLifeEvent += OnGetOpponentLife;
        Global.photonService.OtherMissionScoreEvent += OnOtherMissionComplete;
        Global.photonService.GameStartEvent += OnGameStart;
        Global.photonService.GameOverEvent += OnGameOver;
        Global.photonService.LoadSceneEvent += OnDestory;
        Global.photonService.ApplySkillMiceEvent += OnApplySkillMice;
        Global.photonService.ApplySkillItemEvent += OnApplySkillItem;
        Global.photonService.LoadSceneEvent += OnLoadScene;
        Global.photonService.BossSkillEvent += OnBossSkill;
        Global.photonService.ReLoginEvent += OnReLogin;
        Global.isExitingRoom = false;
        _isCombo = false;
        isSyncStart = true;

        maxLife = _life = _combo = _maxCombo = _nowCombo = _eggMiceUsage = _energyUsage = _lostMice = _killMice = _spawnCount = 0;
        _elapsedGameTime = _score = _maxScore = _gameScore = _otherScore = 0;
        _energy = 0;
        checkPoint = 3;
        _healthValue = 25;
        maxLife = _life;
        _scoreRate = 1;

        InitUseCount();
        ClacPlayerLife();
    }

    /// <summary>
    /// 建立 初始 老鼠、道具用量
    /// </summary>
    private void InitUseCount()
    {
        foreach (string key in Global.dictTeam.Keys.ToList())
        {
            string _itemID = System.Convert.ToString(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.miceProperty, "ItemID", key));

            dictMiceUseCount.Add(key, new Dictionary<string, object> { { "UseCount", 0 } });     // 加入老鼠使用量初始值
            dictItemUseCount.Add(_itemID, new Dictionary<string, object> { { "UseCount", 0 } });                //  加入道具使用量初始值
        }
    }

    public void OnExit()
    {
        Global.photonService.KickOther();
        Global.photonService.ExitRoom();
    }

  public override  void Update()
    {
        battleState = battleAIState.GetState();
        GameConnStatusChk();
        if (!Global.isGameStart)
            _lastTime = Time.time; // 沒作用

        // 同步開始遊戲
        if (poolManager.PoolingFlag && isSyncStart)
        {
            Debug.Log("Pooling Completed Start SyncGame");

            if (Global.MemberType == MemberType.Bot)
                BotAI = new BotAI(poolManager.GetPoolSkillMiceIDs());

            if (feverEnergy == 100)
            {
                feverEnergy = 0;
                SetPlayerState((short)ENUM_Skill.FeverTime);
            }
            isSyncStart = false;
            Global.photonService.SyncGameStart();
        }

        if (Global.isGameStart && Time.time > _lastTime + _defaultStartTime)
        {
            _elapsedGameTime = Time.time - _lastTime - _defaultStartTime;    // 遊戲經過時間
            battleState = battleAIState.GetState();
            if (_combo > _maxCombo) _maxCombo = _combo;     // 假如目前連擊數 大於 _maxCombo  更新 _maxCombo
            if (_score > _maxScore) _maxScore = _score; // 更新最高分

            // Update BotAI
            if (Global.MemberType == MemberType.Bot)
                BotAI.UpdateAI();

            // 遊戲結束判斷
            GoodGame();
        }
    }

    /// <summary>
    /// 遊戲結束
    /// </summary>
    private void GoodGame()
    {
        if (_elapsedGameTime >= Global.GameTime || _life == 0 || _otherLife == 0)
        {
            Global.isGameStart = false;
            List<string> columns = new List<string>();
            columns.Add("UseCount");
            columns.Add("Rank");
            columns.Add("Exp");
            columns.Add("ItemCount");
            string jMicesUseCount = MiniJSON.Json.Serialize(dictMiceUseCount);
            string jItemsUseCount = MiniJSON.Json.Serialize(dictItemUseCount);

            Global.photonService.GameOver((short)_gameScore, (short)_otherScore, (short)_elapsedGameTime, _maxCombo, _killMice, _lostMice, spawnCount, jMicesUseCount, jItemsUseCount, columns.ToArray());
            Debug.Log("GameOver Time! " + _elapsedGameTime + "    jMicesUseCount:" + jMicesUseCount + "jItemsUseCount:" + jItemsUseCount + "  _maxCombo:" + _maxCombo);
        }
    }

    public override void FixedUpdate()
    {
        if (Global.isGameStart && Time.time > _lastTime + 3)
        {
            battleAIState.UpdateState();            // 更新SpawnState邏輯 這裡寫時間 Time + lastime + 間隔
            playerAIState.UpdatePlayerState();      // 更新PlayerState邏輯

            if (_elapsedGameTime % 5 == 0)
            {
                _myDPS = _score / Time.timeSinceLevelLoad;
                _otherDPS = _otherScore / Time.timeSinceLevelLoad;
                //Debug.Log("_myDPS: " + _myDPS + "\n  _otherDPS: " + _otherDPS);
            }
        }
    }

    // 應該用委派的方式呼叫 FUCK 錯誤
    #region UpadateScore 更新分數
    public void UpadateScore(short miceID, float aliveTime)
    {
        if (miceID != -1 && miceID > 10000 && miceID < 11000)
        {
            UpadateCombo();
            MissionCombo++;
            _spawnCount++;
            _killMice++;
            Global.MiceCount--;
            Global.photonService.UpdateScore(miceID, _combo, aliveTime);
        }
    }
    #endregion

    #region LostScore 失去分數
    public void LostScore(short miceID, float aliveTime)
    {
        if (Global.isGameStart)
        {
            if (!_isInvincible)
            {
                //計分公式 存活時間 / 食量 / 吃東西速度 ex:4 / 1 / 0.5 = 8
                if (miceID != -1 && miceID > 10000 && miceID < 11000)
                {
                    if (!_isPropected)
                    {
                        BreakCombo();
                    }

                    Global.photonService.UpdateScore(miceID, _combo, aliveTime);
                    _spawnCount++;
                    _lostMice++;
                    Global.MiceCount--;
                }
            }
        }
    }
    #endregion

    #region UpadateEnergy 更新能量
    public void UpadateEnergy(int value)
    {
        feverEnergy = Math.Min(feverEnergy++, 100);
        _energy += value;

        _energy = Math.Min(_energy, 100);
        _energy = Math.Max(_energy, 0);
        //        Debug.Log(_energy);
    }
    #endregion

    # region UpdateUseCount 更新使用量
    public void UpdateUseCount(short miceID, short useCount)
    {
        Dictionary<string, object> data;
        dictMiceUseCount.TryGetValue(miceID.ToString(), out data);
        data["UseCount"] = useCount;
    }
    #endregion

    private void UpadateCombo()
    {
        _nowCombo++;
        //如果還在連擊 增加連擊數

        if (_isCombo)
            _combo++;

        if (_nowCombo == 5 && !_isCombo)
        {
            _isCombo = true;
            _combo = 5;
            _nowCombo = 0;
        }

        // 補血
        if (_combo != 0 && _combo % _healthValue == 0)
        {
            if (_life + 1 <= maxLife)
                Global.photonService.UpdateLife(1, false);  // FUCK 錯誤 補血由伺服器判斷
        }

        if (_isCombo)
            battleUI.ComboMsg(_combo);
    }

    public void BreakCombo()
    {
        if (Global.isGameStart)
        {
            if (!_isInvincible || !_isPropected && _combo != 0)
            {
                Global.photonService.UpdateLife(-1, false); // FUCK 扣血要由伺服器判斷
                _isCombo = false;           // 結束 連擊
                _combo = 0;                 // 恢復0
                _nowCombo = 0;
                battleUI.ComboMsg(_combo);
                battleAIState.SetValue(0, 0, battleAIState.GetIntervalTime() + .1f, 0);
            }
        }
    }

    void OnUpdateScore(Int16 score, Int16 energy)    // 更新分數時
    {
        //Debug.Log("Get Energy:" + energy);
        _tmpEnergy = energy /** 10*/;

        if (_isPropected || _isInvincible)
            score = Math.Max((short)0, score);
        //        Debug.Log("(Update)OnUpdateScore" + value);
        if (Global.isGameStart)
        {
            Int16 _tmpScore = (Int16)(score * _scoreRate);  // 真實分數 = 獲得的分數 * 倍率(＊＊＊＊＊＊＊有可能被記憶體修改＊＊＊＊＊＊＊)
            // 如果再交換分數任務下，則不取得自己增加的分數
            if (missionManager.missionMode == MissionMode.Opening)
            {
                if (missionManager.mission == Mission.Exchange && score > 0)
                {
                    _otherScore += _tmpScore;
                    if (_combo >= 5)
                        UpadateEnergy(_tmpEnergy);
                }

                if (missionManager.mission == Mission.Exchange && score < 0)    // 如果再交換分數任務下，則取得自己減少的分數
                    _score = (this.score + _tmpScore > 0) ? (_score += _tmpScore) : 0;
            }

            if (missionManager.mission != Mission.Exchange)
            {
                _score = (this.score + _tmpScore < 0) ? 0 : _score += _tmpScore;
                if (_tmpScore > 0) _gameScore += _tmpScore;
            }

            if (_combo >= 5)
                UpadateEnergy(_tmpEnergy);
        }
        //        Debug.Log(_tmpEnergy);
    }



    void OnOtherScore(Int16 value, int energy)     // 接收對手分數 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ＢＵＧ　接收對手分數要ｘ對方的倍率＊＊＊＊＊＊＊＊＊＊＊＊
    {
        if (Global.isGameStart)
        {
            Int16 _tmpScore = (Int16)(value * _scoreRate);  // 真實分數 = 獲得的分數 * 倍率(＊＊＊＊＊＊＊有可能被記憶體修改＊＊＊＊＊＊＊)
            // 如果再交換分數任務下，取得對方增加的分數
            if (missionManager.missionMode == MissionMode.Opening)
            {
                if (missionManager.mission == Mission.Exchange && value > 0)
                {
                    _score += _tmpScore;
                    _gameScore += _tmpScore;
                }

                if (missionManager.mission == Mission.Exchange && value < 0)
                    _otherScore = (this.otherScore + _tmpScore > 0) ? (_otherScore += _tmpScore) : 0;

                if (missionManager.mission == Mission.HarvestRate)
                {
                    Int16 otherScore = (Int16)(value * _otherRate);
                    _otherScore = (this.otherScore + otherScore > 0) ? _otherScore += otherScore : 0;
                }
            }

            if (missionManager.mission != Mission.Exchange && missionManager.mission != Mission.HarvestRate)
                _otherScore = (this.otherScore + _tmpScore < 0) ? 0 : _otherScore += _tmpScore;

            _otherEnergy = Math.Min(energy, 100);
            _otherEnergy = Math.Max(energy, 0);
            //            Debug.Log("OtherEnergy:" + _otherEnergy);
        }
    }

    void OnMissionComplete(Int16 missionReward)
    {
        Debug.Log("On BattleManager missionReward: " + missionReward);
        if (Global.isGameStart)
        {
            if (missionManager.mission == Mission.HarvestRate)
            {
                _scoreRate = 1;
            }
            else
            {
                _score = (this.score + missionReward < 0) ? 0 : _score += missionReward;
            }

            if (missionManager.mission == Mission.DrivingMice)
            {
                MissionCombo = 0;
            }
            //            Debug.Log("(Battle)_score:" + _score);
        }
    }

    void OnOtherMissionComplete(Int16 otherMissionReward)
    {
        if (Global.isGameStart)
        {
            if (missionManager.mission == Mission.HarvestRate)
                _scoreRate = 1;
            else
                _otherScore = (this.otherScore + otherMissionReward < 0) ? 0 : _otherScore += otherMissionReward;
        }
    }

    /// <summary>
    /// 接收任務 ---還沒寫完---
    /// </summary>
    /// <param name="mission"></param>
    /// <param name="value"></param>
    private void OnApplyMission(Mission mission, short value)
    {
        float msgValue = 0;

        if (Global.isGameStart)
        {
            switch (mission)
            {
                case Mission.Harvest: // 收穫
                    {
                        msgValue = value;
                        break;
                    }
                case Mission.DrivingMice: // 趕老鼠
                    {
                        MissionCombo = 0;
                        msgValue = value;
                        break;
                    }
                case Mission.Exchange: // 交換分數
                    {
                        break;
                    }
                case Mission.HarvestRate:
                    {
                        Debug.Log("Rate:" + value);
                        if (_score > _otherScore)
                        {
                            _scoreRate -= ((float)value / 100); //scoreRate 在伺服器以整數百分比儲存 這裡是0~1 所以要/100
                        }
                        else
                        {
                            _scoreRate += ((float)value / 100); //scoreRate 在伺服器以整數0~100 儲存 這裡是0~1 所以要/100
                        }
                        msgValue = _scoreRate;
                        _otherRate = 1 - _scoreRate;
                        _otherRate = (_otherRate == 0) ? 1 : _otherRate = 1 + _otherRate;                       // 如果 1(原始) - 1(我) = 0 代表倍率相同不調整倍率 ; 如果1 - 0.9(我) = 0.1 代表他多出0.1 ;  如果1 - 1.1(我) = -0.1 代表他少0.1 。最後 1+(+-0.1)就是答案
                        break;
                    }
                case Mission.Reduce: // 減少分數
                    {
                        break;
                    }

                case Mission.WorldBoss:
                    {
                        // 如果要測試 可以把 value改成Boss的ID
                        spawnAI.SpawnBoss(value, 0.1f, 0.1f, 6, 60);    //missionScore這裡是HP SpawnBoss 的屬性錯誤 手動輸入的
                        break;
                    }
            }
        }
    }

    private void GameConnStatusChk()
    {
        if (checkTime > checkPoint)
        {
            Global.photonService.CheckStatus();
            checkPoint += checkPoint;
            Debug.Log("Check");
        }

        checkTime += Time.deltaTime;
    }


    void OnBossSkill(ENUM_Skill skill)
    {
        SetPlayerState((short)skill);
    }


    void OnGameOver(int score, int maxScore, short maxCombo, short exp, short sliverReward, short goldReward, string jItemReward, string evaluate, short battleResult)
    {
        bool result;
        result = (battleResult > 0) ? true : false;

        if (maxScore > Global.MaxScore)
        {
            Global.MaxScore = maxScore;
            _isHighScore = true;
        }

        if (maxCombo > Global.MaxCombo)
        {
            Global.MaxCombo = maxCombo;
            _isHighCombo = true;
        }

        battleUI.GoodGameMsg(score, result, exp, sliverReward, goldReward, jItemReward, _maxCombo, _killMice, _lostMice, evaluate, _isHighScore, _isHighCombo);
        Global.photonService.LoadPlayerData(Global.Account);
    }

    void OnGameStart()
    {
        Global.isGameStart = true;
        startAnim.SetActive(true);
        Debug.Log(" ----  Game Start!  ---- ");
    }

    public void OnExitRoom()
    {
        Global.photonService.ExitRoom();
        Debug.Log("ExitRoom");
    }

    private void OnDestory()                       // 離開房間時
    {
        Global.isExitingRoom = true;        // 離開房間了
        Global.isMatching = false;          // 無配對狀態
        Global.BattleStatus = false;        // 不在戰鬥中

        // 取消委派
        Global.photonService.MissionCompleteEvent -= OnMissionComplete;
        Global.photonService.ApplyMissionEvent -= OnApplyMission;
        Global.photonService.UpdateScoreEvent -= OnUpdateScore;
        Global.photonService.OtherScoreEvent -= OnOtherScore;
        Global.photonService.UpdateLifeEvent -= OnUpdateLife;
        Global.photonService.GetOpponentLifeEvent -= OnGetOpponentLife;
        Global.photonService.OtherMissionScoreEvent -= OnOtherMissionComplete;
        Global.photonService.GameStartEvent -= OnGameStart;
        Global.photonService.GameOverEvent -= OnGameOver;
        Global.photonService.LoadSceneEvent -= OnDestory;
        //Global.photonService.ApplySkillMiceEvent -= OnApplySkillMice;
        //Global.photonService.ApplySkillItemEvent -= OnApplySkillItem;
        //Global.photonService.LoadSceneEvent -= OnLoadScene;
        Global.photonService.BossSkillEvent -= OnBossSkill;
        Global.photonService.ReLoginEvent -= OnReLogin;
    }

    public Dictionary<int, GameObject> GetSkillBossMice()
    {
        return dictSkillBossMice;
    }

    private void ClacPlayerLife() // 應該改成由伺服器接收 OnLoadPlayerLife
    {
        short value;
        // 把所有老鼠所提供的生命值加總
        foreach (KeyValuePair<string, object> item in Global.dictTeam)
        {
            value = Convert.ToInt16(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.miceProperty, "Life", item.Key));
            _life += value;
        }

        Global.photonService.UpdateLife(_life, true);    // FUCK 錯誤 生命由伺服器判斷

        foreach (KeyValuePair<string, object> item in Global.OpponentData.Team)
        {
            value = Convert.ToInt16(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.miceProperty, "Life", item.Key));
            _otherLife += value;
        }

    }

    /// <summary>
    /// 接收到技能 改變玩家狀態
    /// </summary>
    /// <param name="skillID"></param>
    private void SetPlayerState(short skillID)
    {
        Debug.Log("SetPlayerState:" + skillID);
        // BattlePanel show skill image
        SkillBase skill = MPGFactory.GetSkillFactory().GetSkill(Global.dictSkills, skillID);
        skill.SetAIController(playerAIState);
        playerAIState.SetPlayerAIState(skill.GetPlayerState(), skill);
        skill.Display();
    }

    /// <summary>
    /// 改變Battle AI狀態
    /// </summary>
    /// <param name="spawnState"></param>
    public void SetSpawnState(IBattleAIState spawnState) // FUCK 錯誤 應該使用委派
    {
        this.battleAIState = spawnState;
        this.battleAIState.SetAIController(this);
    }

    void OnUpdateLife(short value)
    {
        _life = value;
    }

    void OnGetOpponentLife(short value)
    {
        _otherLife = value;
    }


    void OnApplySkillMice(short miceID)     // 收到技能攻擊 
    {
        Debug.Log("OnApplySkill miceID:" + miceID);

        //   0.OnApplySkill需要改寫，接收數量、參數
        spawnAI.SpawnByRandom(miceID, (sbyte[])SpawnData.GetSpawnData(SpawnStatus.LineL), 1.5f, 0.25f, 0.25f, 6, true);
    }

    void OnApplySkillItem(int itemID)     // 收到技能攻擊 
    {
        if (!_isReflection || !_isPropected)
        {
            Debug.Log("OnApplySkillItem itemID:" + itemID);

            short skillID = Convert.ToInt16(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.itemProperty, "SkillID", itemID.ToString()));

            SetPlayerState(skillID);
            // to do Display Skill
        }
    }

    private void OnReLogin()
    {
        Global.isGameStart = false;
        Global.LoginStatus = false;
        Global.isMatching = false;
    }

    void OnLoadScene()
    {
        Global.photonService.ApplySkillMiceEvent -= OnApplySkillMice;
        Global.photonService.ApplySkillItemEvent -= OnApplySkillItem;
        Global.photonService.LoadSceneEvent -= OnLoadScene;
    }

    public void OnHighCombo()
    {
        battleAIState.SetValue(0, 0, battleAIState.GetIntervalTime() - .1f, 0);
    }

    /// <summary>
    /// 限測試使用
    /// </summary>
    /// <param name="lerpTime"></param>
    /// <param name="spawnTime"></param>
    /// <param name="intervalTime"></param>
    /// <param name="spawnCount"></param>
    public void SetValue(float lerpTime, float spawnTime, float intervalTime, int spawnCount)
    {
        battleAIState.SetValue(lerpTime, spawnTime, intervalTime, spawnCount);
    }



    //---亂寫區域 ---
    public static void SetInvincible(bool value)
    {
        _isInvincible = value;
    }
    public static void SetPropected(bool value)
    {
        _isPropected = value;
    }
    public static void SetRefelcetion(bool value)
    {
        _isReflection = value;
    }


    private void FindHole()
    {
        hole = new List<GameObject>();
        for (int i = 1; i <= 12; i++)
            hole.Add(GameObject.Find("Hole" + i.ToString()).gameObject);
    }

    public SpawnAI GetSpawnAI()
    {
        return spawnAI;
    }

    public ENUM_BattleAIState GetBattleState()
    {
        return battleState;
    }

    public Dictionary<string, Dictionary<string, object>> GetMiceUseCount()
    {
        return dictMiceUseCount;
    }

    public Dictionary<string, Dictionary<string, object>> GetItemUseCount()
    {
        return dictItemUseCount;
    }
    public int GetEnergy()
    {
        return _energy;
    }

    //---亂寫區域 ---



    // 外部取用 戰鬥資料
    public Int16 combo { get { return _combo; } }
    public Int16 maxCombo { get { return _maxCombo; } }
    public int tmpCombo { get { return _nowCombo; } }
    public static float gameTime { get { return _elapsedGameTime; } }
    public float score { get { return _score; } }
    public float gameScore { get { return _gameScore; } }
    public float otherScore { get { return _otherScore; } }
    public float otherEnergy { get { return _otherEnergy; } }
    public float maxScore { get { return _maxScore; } }
    public int eggMiceUsage { get { return _eggMiceUsage; } }
    public int energyUsage { get { return _energyUsage; } }
    public Int16 lostMice { get { return _lostMice; } }
    public int hitMice { get { return _killMice; } }
    public Int16 spawnCount { get { return _spawnCount; } }
    public float myDPS { get { return _myDPS; } }
    public float otherDPS { get { return _otherDPS; } }
    public static float Energy { get { return _energy; } }
    public bool isCombo { get { return _isCombo; } }

    public short life { get { return _life; } }
    public short OtherLife { get { return _otherLife; } }
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
