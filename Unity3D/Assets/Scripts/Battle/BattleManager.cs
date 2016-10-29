using UnityEngine;
using System.Collections;
using System;
using MPProtocol;

/*
 * UpadateScore亂打的!!! 直接更新自己分數沒到Server驗證
 * 
 * 
 * 
 */

public class BattleManager : MonoBehaviour
{
    MissionManager missionManager;          // 任務管理員
    BattleHUD battleHUD;                    // HUD

    private static Int16 _maxCombo = 0;       // 最大連擊數
    private static int _tmpCombo = 0;       // 達到多少連擊 用來判斷連擊中
    private double _gameTime = 0;            // 遊戲時間


    private float _maxScore = 0;            // 最高得分
    private float _gameScore = 0;           // 遊戲進行時所得到的分數
    private static int _eggMiceUsage = 0;   // 老鼠使用量
    private static int _energyUsage = 0;    // 能量使用量
    private static Int16 _lostMice = 0;       // 失誤數           統計用
    private int _killMice = 0;              // 計算打了幾隻老鼠 統計用
    private static int _spawnCount = 0;     // 產生了多少老鼠   統計用

    private float _myDPS = 0;               // 我的平均輸出
    private float _otherDPS = 0;            // 對手的平均輸出
    private float _scoreRate = 1;           // 分數倍率
    private float _otherRate = 1;           // 對手分數倍率
    private float _lastTime;                // 我的平均輸出
    private float _score = 0;               // 分數
    private float _otherScore = 0;          // 對手分數
    private double _energy = 0;             // 能量

    private Int16 _combo = 0;               // 連擊數



    private bool _isCombo;                  // 是否連擊
    private bool _isHighScore;              // 是否破分數紀錄
    private bool _isHighCombo;              // 是否破Combo紀錄

    // 外部取用 戰鬥資料

    public Int16 combo { get { return _combo; } }
    public Int16 maxCombo { get { return _maxCombo; } }
    public int tmpCombo { get { return _tmpCombo; } }
    public double gameTime { get { return _gameTime; } }
    public float score { get { return _score; } }
    public float gameScore { get { return _gameScore; } }
    public float otherScore { get { return _otherScore; } }
    public float maxScore { get { return _maxScore; } }
    public int eggMiceUsage { get { return _eggMiceUsage; } }
    public int energyUsage { get { return _energyUsage; } }
    public Int16 lostMice { get { return _lostMice; } }
    public int hitMice { get { return _killMice; } }
    public int spawnCount { get { return _spawnCount; } }
    public float myDPS { get { return _myDPS; } }
    public float otherDPS { get { return _myDPS; } }
    public double energy { get { return _energy; } }

    // Use this for initialization
    void Start()
    {
        Debug.Log("Battle Start");
        missionManager = GetComponent<MissionManager>();
        battleHUD = GetComponent<BattleHUD>();
        Global.isExitingRoom = false;

        Global.photonService.ExitRoomEvent += OnExitRoom;
        Global.photonService.OtherScoreEvent += OnOtherScore;
        Global.photonService.MissionCompleteEvent += OnMissionComplete;
        Global.photonService.ApplyMissionEvent += OnApplyRate;
        Global.photonService.UpdateScoreEvent += OnUpdateScore;
        Global.photonService.OtherMissionScoreEvent += OnOtherMissionComplete;
        Global.photonService.GameStartEvent += OnGameStart;
        Global.photonService.GameOverEvent += OnGameOver;

        _isCombo = false;
        _combo = 0;
        _maxCombo = 0;
        _tmpCombo = 0;
        _gameTime = 0;
        _score = 0;
        _gameScore = 0;
        _otherScore = 0;
        _maxScore = 0;
        _eggMiceUsage = 0;
        _energyUsage = 0;
        _lostMice = 0;
        _killMice = 0;
        _spawnCount = 0;
    }

    void Update()
    {
        if (Global.isGameStart)
        {
            if (_combo > _maxCombo) _maxCombo = _combo;     // 假如目前連擊數 大於 _maxCombo  更新 _maxCombo
            if (_score > _maxScore)
            {
                _maxScore = _score;     // 假如目前分數 大於 _maxScore  更新 _maxScore
                _isHighScore = true;
            }

            if (_gameTime > 5 && (_score == 0 || _otherScore == 0))  // ＊＊＊＊＊＊＊＊＊這裡還是亂寫的 需要回傳Server遊戲玩成的資料才完成＊＊＊＊＊＊＊＊＊
            {
                Global.isGameStart = false;
                Global.photonService.GameOver((short)_gameScore, (short)_otherScore, (short)_gameTime, _maxCombo, _killMice, _lostMice);
                Debug.Log("GameOver!" + _gameTime);
            }
        }
        else
        {
            _gameTime = 0;
        }
    }

    void FixedUpdate()
    {
        // Debug.Log("(Update)_score"+_score);
        if (!Global.isGameStart)
            _lastTime = Time.fixedTime;

        if (Global.isGameStart)
        {
            _gameTime += Time.fixedDeltaTime;

            if (_gameTime % 5==0)
            {
                _myDPS = _score / Time.timeSinceLevelLoad;
                _otherDPS = _otherScore / Time.timeSinceLevelLoad;
                //Debug.Log("_myDPS: " + _myDPS + "\n  _otherDPS: " + _otherDPS);
            }
        }
    }


    #region UpadateScore 更新分數
    public void UpadateScore(string name, float aliveTime)
    {
        if (name != null)
        {
            UpadateCombo();
            _spawnCount++;
            _killMice++;
            Global.MiceCount--;
            Global.photonService.UpdateScore(name, aliveTime);
        }
    }
    #endregion

    #region LostScore 失去分數
    public void LostScore(string name, float aliveTime)
    {
        //計分公式 存活時間 / 食量 / 吃東西速度 ex:4 / 1 / 0.5 = 8
        if (name != null)
        {
            BreakCombo();
            Global.photonService.UpdateScore(name, aliveTime);
            _spawnCount++;
            _lostMice++;
            Global.MiceCount--;
        }
    }
    #endregion

    #region UpadateEnergy 更新能量
    public void UpadateEnergy(double value)
    {
        if (_energy + value > 1)
        {
            _energy = 1.0d;
        }
        else if (_energy + value < 0)
        {
            _energy = 0.0d;
        }
        else
        {
            _energy = Math.Round(_energy += Math.Round(value, 2), 3);
        }
    }
    #endregion

    private void UpadateCombo()
    {
        _tmpCombo++;
        //如果還在連擊 增加連擊數

        if (_isCombo)
            _combo++;

        if (_tmpCombo == 5 && !_isCombo)
        {
            _isCombo = true;
            _combo = 5;
            _tmpCombo = 0;
        }

        if (_isCombo)
            battleHUD.ComboMsg(_combo);
    }

    private void BreakCombo()
    {
        _isCombo = false;           // 結束 連擊
        _combo = 0;                 // 恢復0
        _tmpCombo = 0;
        battleHUD.ComboMsg(_combo);
    }

    void OnUpdateScore(Int16 value)    // 更新分數時
    {
        //        Debug.Log("(Update)OnUpdateScore" + value);
        if (Global.isGameStart)
        {
            Int16 _tmpScore = (Int16)(value * _scoreRate);  // 真實分數 = 獲得的分數 * 倍率(＊＊＊＊＊＊＊有可能被記憶體修改＊＊＊＊＊＊＊)
            // 如果再交換分數任務下，則不取得自己增加的分數
            if (missionManager.missionMode == MissionMode.Opening)
            {
                if (missionManager.mission == Mission.Exchange && value > 0)
                {
                    _otherScore += _tmpScore;
                    if (_combo >= 5) UpadateEnergy(0.01d);
                }

                if (missionManager.mission == Mission.Exchange && value < 0)    // 如果再交換分數任務下，則取得自己減少的分數
                    _score = (this.score + _tmpScore > 0) ? (_score += _tmpScore) : 0;
            }

            if (missionManager.mission != Mission.Exchange)
            {
                _score = (this.score + _tmpScore < 0) ? 0 : _score += _tmpScore;
                if (_tmpScore > 0) _gameScore += _tmpScore;
            }

            if (_combo >= 5) UpadateEnergy(0.01d);
        }
    }



    void OnOtherScore(Int16 value)     // 接收對手分數 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ＢＵＧ　接收對手分數要ｘ對方的倍率＊＊＊＊＊＊＊＊＊＊＊＊
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

            Debug.Log("(Battle)_score:" + _score);
        }
    }

    void OnOtherMissionComplete(Int16 otherMissionReward)
    {
        if (Global.isGameStart)
        {
            if (missionManager.mission == Mission.HarvestRate)
            {
                _scoreRate = 1;
            }
            else
            {
                _otherScore = (this.otherScore + otherMissionReward < 0) ? 0 : _otherScore += otherMissionReward;
            }
            Debug.Log("(Battle)_otherScore:" + _otherScore);
        }
    }


    void OnApplyRate(Mission mission, Int16 scoreRate)
    {
        if (Global.isGameStart)
        {
            if (mission == Mission.HarvestRate)
            {
                Debug.Log("Rate:" + scoreRate);
                if (_score > _otherScore)
                {
                    _scoreRate -= ((float)scoreRate / 100); //scoreRate 在伺服器以整數百分比儲存 這裡是0~1 所以要/100
                }
                else
                {
                    _scoreRate += ((float)scoreRate / 100); //scoreRate 在伺服器以整數0~100 儲存 這裡是0~1 所以要/100
                }

                battleHUD.MissionMsg(mission, _scoreRate);
                _otherRate = 1 - _scoreRate;
                _otherRate = (_otherRate == 0) ? 1 : _otherRate = 1 + _otherRate;                       // 如果 1(原始) - 1(我) = 0 代表倍率相同不調整倍率 ; 如果1 - 0.9(我) = 0.1 代表他多出0.1 ;  如果1 - 1.1(我) = -0.1 代表他少0.1 。最後 1+(+-0.1)就是答案
            }
        }
    }

    void OnGameOver(Int16 score, byte exp, Int16 sliverReward,byte battleResult)
    {
        bool result;
        result = (battleResult > 0) ? true : false;
        battleHUD.GoodGameMsg(score, result, exp, sliverReward, _combo, _killMice, _lostMice, _isHighScore, _isHighCombo);
    }

    void OnGameStart()
    {
        Global.isGameStart = true;
        Debug.Log("Game Start!");
    }

    void OnExitRoom()                       // 離開房間時
    {
        Global.isExitingRoom = true;        // 離開房間了
        Global.isMatching = false;          // 無配對狀態
        Global.BattleStatus = false;        // 不在戰鬥中

        // 取消委派
        Global.photonService.ExitRoomEvent -= OnExitRoom;
        Global.photonService.OtherScoreEvent -= OnOtherScore;
        Global.photonService.MissionCompleteEvent -= OnMissionComplete;
        Global.photonService.ApplyMissionEvent -= OnApplyRate;
        Global.photonService.UpdateScoreEvent -= OnUpdateScore;
        Global.photonService.OtherMissionScoreEvent -= OnOtherMissionComplete;
        Global.photonService.GameStartEvent -= OnGameStart;
        Global.photonService.GameOverEvent -= OnGameOver;
    }
}
