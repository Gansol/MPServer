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
public class BattleAttr
{
    public float score;                   // 分數
    public float  gameTime;         // 遊戲時間
    public float otherScore;         // 對手分數
    public short combo;               // 連擊
    public short otherLife;           // 對手生命值
    public short life;                      // 目前生命值
    public short energy;               // 能量
    public short feverEnergy;     // FeverTime 能量
    public short otherEnergy;    // 對手能量
    public bool bCombo;             // 是否連擊中
    public List<GameObject> hole;   // 老鼠洞列表
}

public class BattleSystem : IGameSystem
{
    #region Variables 變數
    private BotAI BotAI;                                            // 測試用AI
    private BattleUI m_BattleUI;                              // HUD
    private GameObject m_RootUI = null;            // panel Root
    private MissionSystem m_MissionSystem;  // 任務管理員
    private AttachBtn_BattleUI UI;                          // BTN

    private IBattleAIState battleAIState;               // 產生AI狀態 
    private PlayerAIState playerAIState;              // Player狀態
    private BattleAttr battleAttr;                              // battle數值

    private ENUM_BattleAIState battleState = ENUM_BattleAIState.EasyMode;        // 正式版 要改private
    private SpawnStatus spawnStatus = SpawnStatus.LineL;  // 測試用
    private Dictionary<string, Dictionary<string, object>> _dictMiceUseCount;   // 老鼠使用量
    private Dictionary<string, Dictionary<string, object>> _dictItemUseCount;   // 道具使用量

    private static readonly int _defaultStartTime = 3;     // 經過時間

    private static short _energyUsage;    // 能量使用量
    private static short _lostMice;             // 失誤數
    private static short _spawnCount;     // 老鼠產生總量
    private static short _maxCombo;       // 最大連擊數
    private static short _nowCombo;      // 目前連擊
    private static short _tmpEnergy;        // 測試用能量 不用刪除
    private static float _scoreRate;           // 分數倍率
    private static float _otherRate;           // 對手分數倍率

    private short _maxLife;             // 最大生命值
    private short _killMice;            //  死亡老鼠數量 
    private short _healthValue;     // 生命值
    private float _maxScore;         // 最高得分 (錯誤 沒有取得最高得分)
    private float _gameScore;      // 目前遊戲分數
    private float _myDPS;             // 平均輸出
    private float _otherDPS;         // 對手平均輸出 
    private float _lastTime;           // 上次開始時間
    private float _checkTime;       // 檢查時間
    private float _checkPoint;      // 檢查間格時間
    private bool _bHighScore;      // 是否破分數紀錄
    private bool _bHighCombo;   // 是否破Combo紀錄
    private bool _bSyncStart;       //   同步開始
    private bool _bPropected;      //  保護 要在伺服器判斷 偷懶亂寫  錯誤
    private bool _bReflection;       // 反射
    private bool _bInvincible;       // 無敵

    public bool SpawnFlag { get; set; }                      //  是給TestPanel使用的
    public int MissionCombo { get; private set; }   // 任務開始時 儲存COMBO
    private static short _eggMiceUsage; // 老鼠使用量
    //private readonly Dictionary<int, GameObject> _dictSkillBossMice = new Dictionary<int, GameObject>();
    #endregion

    public BattleSystem(MPGame MPGame) : base(MPGame)
    {
        Debug.Log("--------------- BattleSystem Create ----------------");
        _dictMiceUseCount = new Dictionary<string, Dictionary<string, object>>();
        _dictItemUseCount = new Dictionary<string, Dictionary<string, object>>();
        playerAIState = new PlayerAIState(this);
        battleAttr = new BattleAttr();
    }

    // Use this for initialization
    public override void Initialize()
    {
        Debug.Log("--------------- BattleSystem Initialize ----------------");
        EventMaskSwitch.Initialize();
        //FindHole();

        m_RootUI = GameObject.Find(Global.Scene.BattleAsset.ToString());
        UI = m_RootUI.GetComponentInChildren<AttachBtn_BattleUI>();
        m_MissionSystem = m_MPGame.GetMissionSystem();
        m_BattleUI = m_MPGame.GetBattleUI();

        battleAttr.energy = _maxLife = battleAttr.life = battleAttr.combo = _maxCombo = _nowCombo /*= _eggMiceUsage */ = _energyUsage = _lostMice = _killMice = _spawnCount = 0;
        battleAttr.gameTime = battleAttr.score = battleAttr.otherScore = _maxScore = _gameScore = 0;
        battleAttr.hole = UI.hole;

        _checkPoint = 3;
        _healthValue = 25;
        _scoreRate = _otherRate = 1;
        _checkTime = Time.time;

        Global.isExitingRoom = false;
        battleAttr.bCombo = false;
        _bSyncStart = true;

        InitUseCount();
        ClacPlayerLife();
        SetBattleAIState(new EasyBattleAIState(battleAttr));

        Global.photonService.OtherMissionScoreEvent += OnOtherMissionComplete;
        Global.photonService.MissionCompleteEvent += OnMissionComplete;
        Global.photonService.GetOpponentLifeEvent += OnUpdateOpponentLife;
        Global.photonService.ApplySkillItemEvent += OnApplySkillItem;
        Global.photonService.ApplyMissionEvent += OnApplyMission;
        Global.photonService.UpdateScoreEvent += OnUpdateScore;
        Global.photonService.OtherScoreEvent += OnUpdateOtherScore;
        Global.photonService.UpdateLifeEvent += OnUpdateLife;
        Global.photonService.GameOverEvent += OnApplyGameOver;

        Global.photonService.BossSkillEvent += OnApplyBossSkill;
        Global.photonService.ReLoginEvent += OnReLogin;

      //  Global.photonService.LoadPlayerItem(Global.Account);
      //  Global.photonService.LoadPlayerData(Global.Account);
        //  Global.photonService.LoadSceneEvent += OnLoadScene;
        //Global.photonService.ApplySkillMiceEvent += OnApplySkillMice;
    }

    #region -- InitUseCount 初始化老鼠道具用量 -- 
    /// <summary>
    /// 建立 初始 老鼠、道具用量
    /// </summary>
    private void InitUseCount()
    {
        foreach (string key in Global.dictTeam.Keys.ToList())
        {
            string _itemID = System.Convert.ToString(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.miceProperty, "ItemID", key));
            _dictMiceUseCount.Add(key, new Dictionary<string, object> { { "UseCount", 0 } });     // 加入老鼠使用量初始值
            _dictItemUseCount.Add(_itemID, new Dictionary<string, object> { { "UseCount", 0 } });                //  加入道具使用量初始值
        }
    }
    #endregion

    public override void Update()
    {
        battleState = battleAIState.GetState();// 可能錯誤 還沒實體化
        GameConnStatusChk();

        if (!Global.isGameStart)
            _lastTime = Time.time; // 沒作用

        #region // 同步開始遊戲
        if (m_MPGame.GetPoolSystem().GetPoolingComplete() && _bSyncStart)
        {
            Debug.Log("Pooling Completed Start SyncGame");

            // 如果是BOT 新增AI
            if (Global.MemberType == MemberType.Bot)
                BotAI = new BotAI(m_MPGame.GetPoolSystem().GetPoolSkillMiceIDs());



            _bSyncStart = false;
            Global.photonService.SyncGameStart();
        }
        #endregion

        #region // 遊戲邏輯
        if (Global.isGameStart && Time.time > _lastTime + _defaultStartTime)
        {
            battleAttr.gameTime = Time.time - _lastTime - _defaultStartTime;    // 遊戲經過時間
                                                                                //  battleState = battleAIState.GetState();
            if (battleAttr.combo > _maxCombo) _maxCombo = battleAttr.combo;     // 假如目前連擊數 大於 _maxCombo  更新 _maxCombo
            if (battleAttr.score > _maxScore) _maxScore = battleAttr.score; // 更新最高分

            // Update BotAI
            if (Global.MemberType == MemberType.Bot)
                BotAI.UpdateAI();

            // 如果 能量=100 開始FeverTime
            if (battleAttr.feverEnergy == 100)
            {
                battleAttr.feverEnergy = 0;
                SetPlayerState((short)ENUM_Skill.FeverTime);
            }

            // 遊戲結束判斷
            GoodGame();
        }
        #endregion
    }

    public override void FixedUpdate()
    {
        if (Global.isGameStart && Time.time > _lastTime + _defaultStartTime)
        {
            ClacDPS();                                                      // 計算DPS
            battleAIState.UpdateState();                   // 更新SpawnState邏輯 這裡寫時間 Time + lastime + 間隔
            playerAIState.UpdatePlayerState();      // 更新PlayerState邏輯
        }
    }



    #region -- ClacDPS 計算平均輸出 --
    private void ClacDPS()
    {
        if (battleAttr.gameTime % 5 == 0)
        {
            _myDPS = battleAttr.score / _lastTime + _defaultStartTime;
            _otherDPS = battleAttr.otherScore / _lastTime + _defaultStartTime;
            //Debug.Log("_myDPS: " + _myDPS + "\n  _otherDPS: " + _otherDPS);
        }
    }
    #endregion

    #region -- GoodGame 遊戲結束 --
    /// <summary>
    /// 遊戲結束
    /// </summary>
    private void GoodGame()
    {
        if (battleAttr.gameTime >= Global.GameTime || battleAttr.life == 0 || battleAttr.otherLife == 0)
        {
            Global.isGameStart = false;
            List<string> columns = new List<string>
            {
                "UseCount",
                "Rank",
                "Exp",
                "ItemCount"
            };
            string jMicesUseCount = MiniJSON.Json.Serialize(_dictMiceUseCount);
            string jItemsUseCount = MiniJSON.Json.Serialize(_dictItemUseCount);

            Global.photonService.GameOver((short)_gameScore, (short)battleAttr.otherScore, (short)battleAttr.gameTime, _maxCombo, _killMice, _lostMice, _spawnCount, jMicesUseCount, jItemsUseCount, columns.ToArray());
            Debug.Log("GameOver Time! " + battleAttr.gameTime + "    jMicesUseCount:" + jMicesUseCount + "jItemsUseCount:" + jItemsUseCount + "  _maxCombo:" + _maxCombo);
        }
    }
    #endregion

    #region -- UpadateScore 更新分數 --
    /// <summary>
    /// 更新分數
    ///  應該用委派的方式呼叫 FUCK 錯誤
    /// </summary>
    /// <param name="miceID"></param>
    /// <param name="aliveTime"></param>
    public void UpadateScore(short miceID, float aliveTime)
    {
        if (miceID != -1 && miceID > 10000 && miceID < 11000)
        {
            //Debug.Log("BattleSystem UpadateScore aliveTime:" + aliveTime);
            UpadateCombo();
            MissionCombo++;
            _spawnCount++;
            _killMice++;
            // aliveTime 好像時間不會重製 錯誤
            Global.photonService.UpdateScore(miceID, battleAttr.combo, aliveTime);
        }
    }
    #endregion

    #region -- LostScore 失去分數 --
    /// <summary>
    /// 失去分數
    /// </summary>
    /// <param name="miceID"></param>
    /// <param name="aliveTime"></param>
    public void LostScore(short miceID, float aliveTime)
    {
        if (Global.isGameStart)
        {
            if (!_bInvincible)
            {
                Debug.Log("BattleSystem LostScore aliveTime:" + aliveTime);
                //計分公式 存活時間 / 食量 / 吃東西速度 ex:4 / 1 / 0.5 = 8
                if (miceID != -1 && miceID > 10000 && miceID < 11000)
                {
                    // 如果不是保護斷COMBO狀態
                    if (!_bPropected)
                        BreakCombo();

                    Global.photonService.UpdateScore(miceID, battleAttr.combo, aliveTime);
                    _spawnCount++;
                    _lostMice++;
                }
            }
        }
    }
    #endregion

    #region -- UpadateEnergy 更新能量 --
    /// <summary>
    /// 更新能量
    /// </summary>
    /// <param name="value"></param>
    public void UpadateEnergy(short value)
    {
        battleAttr.feverEnergy = Math.Min(battleAttr.feverEnergy++, (short)100);
        battleAttr.energy += value;
        if (value < 0) _energyUsage -= value;   // 儲存能量使用總量


        battleAttr.energy = Math.Min(battleAttr.energy, (short)100);
        battleAttr.energy = Math.Max(battleAttr.energy, (short)0);
        //        Debug.Log(battleAttr.energy);
    }
    #endregion

    # region -- UpdateMiceUseCount 更新老鼠使用量 --
    /// <summary>
    /// 更新使用量 (錯誤)
    /// </summary>
    /// <param name="miceID"></param>
    /// <param name="useCount"></param>
    public void UpdateMiceUseCount(short miceID, short useCount=1)
    {
        _dictMiceUseCount[miceID.ToString()]["UseCount"] = useCount;
    }
    #endregion

    #region -- UpdateItemUseCount 更新道具使用量 --
    /// <summary>
    /// 更新道具使用量 
    /// </summary>
    /// <param name="itemID"></param>
    /// <param name="useCount"></param>
    public void UpdateItemUseCount(short itemID, short useCount = 1)
    {
        _dictItemUseCount[itemID.ToString()]["UseCount"] = useCount;
    }
    #endregion

    #region -- Combo -- 
    /// <summary>
    /// 更新Combo
    /// </summary>
    private void UpadateCombo()
    {
        _nowCombo++;

        //如果還在連擊 增加連擊數
        if (battleAttr.bCombo)
            battleAttr.combo++;

        // 連擊>5 顯示連擊
        if (_nowCombo == 5 && !battleAttr.bCombo)
        {
            battleAttr.bCombo = true;
            battleAttr.combo = 5;
            _nowCombo = 0;
        }

        // 補血
        if (battleAttr.combo != 0 && battleAttr.combo % _healthValue == 0)
            if (battleAttr.life + 1 <= _maxLife)
                Global.photonService.UpdateLife(1, false);  // FUCK 錯誤 補血由伺服器判斷

        // 如果連擊中 顯示Combo文字
        if (battleAttr.bCombo)
            m_BattleUI.ComboMsg(battleAttr.combo);
    }

    /// <summary>
    /// 中斷Combo
    /// </summary>
    public void BreakCombo()
    {
        if (Global.isGameStart)
        {
            if (!_bInvincible || !_bPropected && battleAttr.combo != 0)
            {
                Global.photonService.UpdateLife(-1, false); // FUCK 扣血要由伺服器判斷
                battleAttr.bCombo = false;           // 結束 連擊
                battleAttr.combo = 0;                 // 恢復0
                _nowCombo = 0;
                m_BattleUI.ComboMsg(battleAttr.combo);
                battleAIState.SetValue(0, 0, battleAIState.GetIntervalTime() + .1f, 0);
            }
        }
    }
    #endregion

    #region -- OnUpdateScore 更新分數時 -- 
    void OnUpdateScore(short score, short energy)    // 更新分數時
    {
        //Debug.Log("Get Energy:" + energy);
        _tmpEnergy = energy /** 10*/;

        if (_bPropected || _bInvincible)
            score = Math.Max((short)0, score);
        //        Debug.Log("(Update)OnUpdateScore" + value);
        if (Global.isGameStart)
        {
            short _tmpScore = (short)(score * _scoreRate);  // 真實分數 = 獲得的分數 * 倍率(＊＊＊＊＊＊＊有可能被記憶體修改＊＊＊＊＊＊＊)
            // 如果再交換分數任務下，則不取得自己增加的分數
            if (m_MissionSystem.MissionMode == MissionMode.Opening)
            {
                if (m_MissionSystem.Mission == Mission.Exchange && score > 0)
                {
                    battleAttr.otherScore += _tmpScore;
                    if (battleAttr.combo >= 5)
                        UpadateEnergy(_tmpEnergy);
                }

                if (m_MissionSystem.Mission == Mission.Exchange && score < 0)    // 如果再交換分數任務下，則取得自己減少的分數
                    battleAttr.score = (battleAttr.score + _tmpScore > 0) ? (battleAttr.score += _tmpScore) : 0;
            }

            if (m_MissionSystem.Mission != Mission.Exchange)
            {
                battleAttr.score = (battleAttr.score + _tmpScore < 0) ? 0 : battleAttr.score += _tmpScore;
                if (_tmpScore > 0) _gameScore += _tmpScore;
            }

            if (battleAttr.combo >= 5)
                UpadateEnergy(_tmpEnergy);
        }
        //        Debug.Log(_tmpEnergy);
    }
    #endregion

    #region -- OnUpdateOtherScore 接收對手分數 -- 
    /// <summary>
    /// 接收對手分數 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="energy"></param>
    void OnUpdateOtherScore(short value, short energy)     // 接收對手分數 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊BUG　接收對手分數要ｘ對方的倍率＊＊＊＊＊＊＊＊＊＊＊＊
    {
        if (Global.isGameStart)
        {
            short _tmpScore = (short)(value * _scoreRate);  // 真實分數 = 獲得的分數 * 倍率(＊＊＊＊＊＊＊有可能被記憶體修改＊＊＊＊＊＊＊)
            // 如果再交換分數任務下，取得對方增加的分數
            if (m_MissionSystem.MissionMode == MissionMode.Opening)
            {
                if (m_MissionSystem.Mission == Mission.Exchange && value > 0)
                {
                    battleAttr.score += _tmpScore;
                    _gameScore += _tmpScore;
                }

                if (m_MissionSystem.Mission == Mission.Exchange && value < 0)
                    battleAttr.otherScore = (battleAttr.otherScore + _tmpScore > 0) ? (battleAttr.otherScore += _tmpScore) : 0;

                if (m_MissionSystem.Mission == Mission.HarvestRate)
                {
                    Int16 otherScore = (Int16)(value * _otherRate);
                    battleAttr.otherScore = (battleAttr.otherScore + otherScore > 0) ? battleAttr.otherScore += otherScore : 0;
                }
            }

            if (m_MissionSystem.Mission != Mission.Exchange && m_MissionSystem.Mission != Mission.HarvestRate)
                battleAttr.otherScore = (battleAttr.otherScore + _tmpScore < 0) ? 0 : battleAttr.otherScore += _tmpScore;

            battleAttr.otherEnergy = Math.Min(energy, (short)100);
            battleAttr.otherEnergy = Math.Max(energy, (short)0);
            //            Debug.Log("OtherEnergy:" + _otherEnergy);
        }
    }
    #endregion

    #region --OnMissionComplete 當任務完成時 -- 
    /// <summary>
    /// 當任務完成時
    /// </summary>
    /// <param name="missionReward"></param>
    void OnMissionComplete(short missionReward)
    {
        Debug.Log("On BattleManager missionReward: " + missionReward);
        if (Global.isGameStart)
        {
            if (m_MissionSystem.Mission == Mission.HarvestRate)
            {
                _scoreRate = 1;
            }
            else
            {
                battleAttr.score = (battleAttr.score + missionReward < 0) ? 0 : battleAttr.score += missionReward;
            }

            if (m_MissionSystem.Mission == Mission.DrivingMice)
            {
                MissionCombo = 0;
            }
            //            Debug.Log("(Battle) battleAttr.score:" +  battleAttr.score);
        }
    }
    #endregion

    #region -- OnOtherMissionComplete 當對手完成任務時 -- 
    /// <summary>
    /// 當對手完成任務時
    /// </summary>
    /// <param name="otherMissionReward"></param>
    void OnOtherMissionComplete(short otherMissionReward)
    {
        if (Global.isGameStart)
        {
            if (m_MissionSystem.Mission == Mission.HarvestRate)
                _scoreRate = 1;
            else
                battleAttr.otherScore = (battleAttr.otherScore + otherMissionReward < 0) ? 0 : battleAttr.otherScore += otherMissionReward;
        }
    }
    #endregion

    #region -- OnApplyMission 接收任務 -- 

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
                        if (battleAttr.score > battleAttr.otherScore)
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
                        m_MPGame.GetPoolSystem().SpawnBoss(UI.hole[4].transform, value, 0.1f, 0.1f, 6, 60);//missionScore這裡是HP SpawnBoss 的屬性錯誤 手動輸入的
                        break;
                    }
            }
        }
    }
    #endregion

    #region -- GameConnStatusChk 遊戲狀態檢查 --
    private void GameConnStatusChk()
    {
        if (_checkTime > (Time.time + _checkPoint))
        {
            Global.photonService.CheckStatus();
            _checkPoint += _checkPoint;
            _checkTime = Time.time;
            Debug.Log("Check");
        }
    }
    #endregion

    #region -- OnApplyBossSkill 接收Boss技能 -- 
    void OnApplyBossSkill(ENUM_Skill skill)
    {
        SetPlayerState((short)skill);
    }
    #endregion

    #region -- OnApplyGameOver -- 
    /// <summary>
    /// 收到 遊戲結束
    /// </summary>
    /// <param name="score">得分</param>
    /// <param name="maxScore">最高得分</param>
    /// <param name="maxCombo">最大Combo</param>
    /// <param name="exp">獲得經驗值</param>
    /// <param name="sliverReward">銀幣獎勵</param>
    /// <param name="goldReward">金幣獎勵</param>
    /// <param name="jItemReward">道具獎勵</param>
    /// <param name="evaluate">評價</param>
    /// <param name="battleResult">win:1 lost:0</param>
    void OnApplyGameOver(int score, int maxScore, short maxCombo, short exp, short sliverReward, short goldReward, string jItemReward, string evaluate, short battleResult)
    {
        bool result;
        result = (battleResult > 0) ? true : false;

        if (maxScore > Global.MaxScore)
        {
            Global.MaxScore = maxScore;
            _bHighScore = true;
        }

        if (maxCombo > Global.MaxCombo)
        {
            Global.MaxCombo = maxCombo;
            _bHighCombo = true;
        }

        m_BattleUI.GoodGameMsg(score, result, exp, sliverReward, goldReward, jItemReward, _maxCombo, _killMice, _lostMice, evaluate, _bHighScore, _bHighCombo);
        Global.photonService.LoadPlayerData(Global.Account);
    }
    #endregion

    #region -- OnApplyOtherExitRoom 收到對手離開房間  -- 
    public void OnApplyOtherExitRoom()
    {
        Global.photonService.ExitRoom();
        Debug.Log("ExitRoom");
    }
    #endregion

    #region -- ClacPlayerLife 累計玩家生命(錯誤) -- 
    private void ClacPlayerLife() // 應該改成由伺服器接收 OnLoadPlayerLife
    {
        short value;
        // 把所有老鼠所提供的生命值加總
        foreach (KeyValuePair<string, object> item in Global.dictTeam)
        {
            value = Convert.ToInt16(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.miceProperty, "Life", item.Key));
            battleAttr.life += value;
        }

        Global.photonService.UpdateLife(battleAttr.life, true);    // FUCK 錯誤 生命由伺服器判斷

        foreach (KeyValuePair<string, object> item in Global.OpponentData.Team)
        {
            value = Convert.ToInt16(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.miceProperty, "Life", item.Key));
            battleAttr.otherLife += value;
        }

        _maxLife = battleAttr.life;
    }
    #endregion

    #region -- SetPlayerState 改變玩家狀態  -- 
    /// <summary>
    /// 接收到技能 改變玩家狀態
    /// </summary>
    /// <param name="skillID"></param>
    private void SetPlayerState(short skillID)
    {
        Debug.Log("SetPlayerState:" + skillID);
        // BattlePanel show skill image
        ISkill skill = MPGFactory.GetSkillFactory().GetSkill(Global.dictSkills, skillID);
        skill.SetAIController(playerAIState);
        playerAIState.SetPlayerAIState(skill.GetPlayerState(), skill);
        skill.Display();
    }
    #endregion

    #region -- SetBattleAIState 改變Battle AI狀態 -- 
    /// <summary>
    /// 改變Battle AI狀態
    /// </summary>
    /// <param name="battleState"></param>
    public void SetBattleAIState(IBattleAIState battleState) // FUCK 錯誤 應該使用委派
    {
        battleAIState = battleState;
    }
    #endregion

    #region -- OnUpdateData 收到伺服器資料 -- 
    void OnUpdateLife(short value)
    {
        battleAttr.life = value;
    }

    void OnUpdateOpponentLife(short value)
    {
        battleAttr.otherLife = value;
    }
    #endregion

    #region --OnApplySkillItem 收到技能攻擊 --
    /// <summary>
    /// 收到技能攻擊，設定技能狀態
    /// </summary>
    /// <param name="itemID"></param>
    void OnApplySkillItem(int itemID)
    {
        if (!_bReflection || !_bPropected)
        {
            short skillID = Convert.ToInt16(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.itemProperty, "SkillID", itemID.ToString()));
            SetPlayerState(skillID);
            //Debug.Log("OnApplySkillItem itemID:" + itemID);
            // to do Display Skill
        }
    }
    #endregion

    #region -- OnReLogin 重新連線 --
    private void OnReLogin()
    {
        Global.isGameStart = false;
        Global.LoginStatus = false;
        Global.isMatching = false;
    }
    #endregion

    #region -- GetBattleState 取得Battle狀態 -- 
    public ENUM_BattleAIState GetBattleState()
    {
        return battleState;
    }
    #endregion

    #region -- GetDPS 取得平均輸出 --
    /// <summary>
    /// 取得平均輸出
    /// </summary>
    /// <param name="bMyDPS">true:myDPS  false:otherDPS</param>
    /// <returns></returns>
    private float GetDPS(bool bMyDPS)
    {
        return (bMyDPS == true) ? _myDPS : _otherDPS;
    }
    #endregion


    #region -- 亂寫區域 -- 
    // 直接從外部設定狀態 錯誤

    // 要改private 現在是提供給TestPanel使用
    public IBattleAIState GetBattleAIState()
    {
        return battleAIState;
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

    public void SetInvincible(bool value)
    {
        _bInvincible = value;
    }
    public void SetPropected(bool value)
    {
        _bPropected = value;
    }
    public void SetRefelcetion(bool value)
    {
        _bReflection = value;
    }

    /// <summary>
    /// 取得老鼠使用量 
    /// </summary>
    /// <returns></returns>
    public object GetMiceUseCount(short miceID)
    {
        return _dictMiceUseCount[miceID.ToString()]["UseCount"];
    }

    /// <summary>
    /// 取得道具使用量
    /// </summary>
    /// <returns></returns>
    public object GetItemUseCount(short miceID)
    {
        return _dictItemUseCount[miceID.ToString()]["UseCount"]; ;
    }

    /// <summary>
    /// 取得Battle數值
    /// </summary>
    /// <returns></returns>
    public BattleAttr GetBattleAttr()
    {
        return battleAttr;
    }
    #endregion

    #region -- Release --
    public override void Release()
    {
        base.Release();
        Global.isExitingRoom = true;        // 離開房間了
        Global.isMatching = false;          // 無配對狀態
        Global.BattleStatus = false;        // 不在戰鬥中

        _dictMiceUseCount.Clear();
        _dictItemUseCount.Clear();
        
        Global.photonService.MissionCompleteEvent -= OnMissionComplete;
        Global.photonService.ApplyMissionEvent -= OnApplyMission;
        Global.photonService.UpdateScoreEvent -= OnUpdateScore;
        Global.photonService.OtherScoreEvent -= OnUpdateOtherScore;
        Global.photonService.UpdateLifeEvent -= OnUpdateLife;
        Global.photonService.GetOpponentLifeEvent -= OnUpdateOpponentLife;
        Global.photonService.OtherMissionScoreEvent -= OnOtherMissionComplete;
        Global.photonService.GameOverEvent -= OnApplyGameOver;
        Global.photonService.ApplySkillItemEvent -= OnApplySkillItem;
        Global.photonService.BossSkillEvent -= OnApplyBossSkill;
        Global.photonService.ReLoginEvent -= OnReLogin;
        //Global.photonService.LoadSceneEvent -= OnLoadScene;
        //Global.photonService.ApplySkillMiceEvent -= OnApplySkillMice;
    }
    #endregion




    //public Dictionary<int, GameObject> GetSkillBossMice()
    //{
    //    return _dictSkillBossMice;
    //}

    //public void OnHighCombo()
    //{
    //    battleAIState.SetValue(0, 0, battleAIState.GetIntervalTime() - .1f, 0);
    //}

    //private void FindHole()
    //{
    //    hole = new List<GameObject>();
    //    for (int i = 1; i <= 12; i++)
    //        hole.Add(GameObject.Find("Hole" + i.ToString()).gameObject);
    //}

    //public SpawnAI GetSpawnAI()
    //{
    //    return spawnAI;
    //}
    // 外部取用 戰鬥資料
    //public Int16 combo { get { return battleAttr.combo; } }
    //public Int16 maxCombo { get { return _maxCombo; } }
    //public int tmpCombo { get { return _nowCombo; } }
    //public /*static */float gameTime { get { return battleAttr.gameTime; } }
    //public float score { get { return battleAttr.score; } }
    //public float gameScore { get { return _gameScore; } }
    //public float otherScore { get { return battleAttr.otherScore; } }
    //public float otherEnergy { get { return _otherEnergy; } }
    //public float maxScore { get { return _maxScore; } }
    //public int eggMiceUsage { get { return _eggMiceUsage; } }
    //public int energyUsage { get { return _energyUsage; } }
    //public Int16 lostMice { get { return _lostMice; } }
    //public int hitMice { get { return _killMice; } }
    //public Int16 spawnCount { get { return _spawnCount; } }
    //public float myDPS { get { return _myDPS; } }
    //public float otherDPS { get { return _otherDPS; } }
    //public static float Energy { get { return battleAttr.energy; } }
    //public bool isCombo { get { return battleAttr.bCombo; } }

    //public short life { get { return battleAttr.life; } }
    //public short OtherLife { get { return battleAttr.otherLife; } }
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
