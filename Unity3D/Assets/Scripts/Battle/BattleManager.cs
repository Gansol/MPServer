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
    public GameObject BlueScore;            // 藍隊分數Label
    public GameObject ComboLabel;           // COMBO   Label
    public GameObject RedScore;             // 紅隊分數Label
    public GameObject HPBar;                // 藍色HPBar

    private static int _combo = 0;          // 連擊數
    private static int _maxCombo = 0;       // 最大連擊數
    private static int _tmpCombo = 0;       // 達到多少連擊 用來判斷連擊中
    private static int _gameTime = 0;       // 遊戲時間
    private static float _score = 0;        // 分數
    private static float _otherScore = 0;   // 對手分數
    private static int _maxScore = 0;       // 最高得分
    private static int _eggMiceUsage = 0;   // 老鼠使用量
    private static int _energyUsage = 0;    // 能量使用量
    private static int _missMice = 0;       // 失誤數           統計用
    private static int _hitMice = 0;        // 計算打了幾隻老鼠 統計用
    private static int _spawnCount = 0;     // 產生了多少老鼠   統計用
    private static float _myDPS = 0;        // 我的平均輸出
    private static float _otherDPS = 0;     // 對手的平均輸出
    private static float _scoreRate = 1;    // 分數倍率
    private float _lastTime;                // 我的平均輸出


    private float _beautyHP;                // 美化血條用

    private static bool _isCombo = false;   // 是否連擊

    // 外部取用 戰鬥資料

    public int combo { get { return _combo; } }
    public int maxCombo { get { return _maxCombo; } }
    public int tmpCombo { get { return _tmpCombo; } }
    public int gameTime { get { return _gameTime; } }
    public float score { get { return _score; } }
    public float otherScore { get { return _otherScore; } }
    public int maxScore { get { return _maxScore; } }
    public int eggMiceUsage { get { return _eggMiceUsage; } }
    public int energyUsage { get { return _energyUsage; } }
    public int missMice { get { return _missMice; } }
    public int hitMice { get { return _hitMice; } }
    public int spawnCount { get { return _spawnCount; } }
    public float myDPS { get { return _myDPS; } }
    public float otherDPS { get { return _myDPS; } }

    // Use this for initialization
    void Start()
    {
        missionManager = GetComponent<MissionManager>();
        Global.isExitingRoom = false;

        Global.photonService.ExitRoomEvent += OnExitRoomEvent;
        Global.photonService.OtherScoreEvent += OnOtherScoreEvent;
        Global.photonService.MissionCompleteEvent += OnMissionCompleteEvent;
        Global.photonService.ApplyMissionEvent += OnApplyRateEvent;

        _isCombo = false;
        _combo = 0;
        _maxCombo = 0;
        _tmpCombo = 0;
        _gameTime = 0;
        _score = 0;
        _otherScore = 0;
        _maxScore = 0;
        _eggMiceUsage = 0;
        _energyUsage = 0;
        _missMice = 0;
        _hitMice = 0;
        _spawnCount = 0;

        _beautyHP = 0.5f; // 0.5 是血調中間
    }

    void FixedUpdate()
    {
        float _time = Time.time;
        if (Global.isGameStart)
        {
            if (_time > _lastTime + 5)
            {
                _myDPS = _score / Time.timeSinceLevelLoad;
                _otherDPS = _otherScore / Time.timeSinceLevelLoad;
                _lastTime = _time;
                //Debug.Log("_myDPS: " + _myDPS + "\n  _otherDPS: " + _otherDPS);
            }
        }
    }

    #region UpadateScore 更新分數 這裡根本亂打的
    /// <summary>
    /// 更新分數 這裡根本亂打的
    /// </summary>
    /// <param name="name"></param>
    public void UpadateScore(string name)
    {
        switch (name)
        {
            case "EggMice":
                {
                    if (MissionManager.missionMode == MissionMode.Opening && MissionManager.mission == Mission.Exchange)
                    {
                        _score += (int)(2 * _scoreRate);              // ＊＊＊＊＊＊分數這裡亂打的要改!!!＊＊＊＊＊＊＊＊＊
                    }
                    else
                    {
                        _score += (int)(2 * _scoreRate);              // ＊＊＊＊＊＊分數這裡亂打的要改!!!＊＊＊＊＊＊＊＊＊
                    }

                    UpadateCombo();
                    _spawnCount++;
                    _hitMice++;
                    Global.MiceCount--;
                    //Global.photonService.SendDamage(1);  //＊＊＊＊ 攻擊值亂打的要改
                    Global.photonService.UpdateScore(10, 1, 2);//＊＊＊＊ 分數亂打的要改
                    break;
                }
        }
    }
    #endregion


    #region LostScore 失去分數 這裡根本亂打的
    /// <summary>
    /// 失去分數 這裡根本亂打的
    /// </summary>
    /// <param name="name"></param>
    /// <param name="aliveTime"></param>
    public void LostScore(string name, float aliveTime)
    {
        //計分公式 存活時間 / 食量 / 吃東西速度 ex:4 / 1 / 0.5 = 8
        switch (name)
        {
            case "EggMice":
                {
                    BreakCombo();
                    //_score = (int)(Math.Round(aliveTime, 2) / (micePty.eggMiceAppetite / micePty.eggMiceBiteSpeed));
                    _score -= 2;
                    Global.photonService.UpdateScore(10, 1, -2);//＊＊＊＊ 分數亂打的要改
                    _spawnCount++;
                    _missMice++;
                    Global.MiceCount--;
                    break;
                }
        }
    }
    #endregion

    private void UpadateCombo()
    {
        _tmpCombo++;

        if (_tmpCombo == 5 && !_isCombo)
        {
            _isCombo = true;
            _combo = 4; //下面會多+1=5
            _tmpCombo = 0;
        }

        if (_isCombo)   //如果還在連擊
        {
            _combo++;   //增加連擊數
        }
    }

    private void BreakCombo()
    {
        if (_isCombo)
        {
            if (_combo > _maxCombo)     // 假如目前連擊數 大於 maxCombo 
            {
                _maxCombo = _combo;     // 更新 maxCombo
            }
            _isCombo = false;           // 結束 連擊
            _combo = 0;                 // 恢復0
            _tmpCombo = 0;
        }
    }

    void OnGUI()
    {
        //Debug.Log(_score);
        BlueScore.GetComponent<UILabel>().text = _score.ToString();         // 畫出分數值
        RedScore.GetComponent<UILabel>().text = _otherScore.ToString();     // 畫出分數值

        if (_score < 0) _score = 0;                                         // 假如-分 顯示0分
        if (_otherScore < 0) _otherScore = 0;                               // 假如-分 顯示0分

        float value = _score / (_score + _otherScore);                      // 得分百分比 兩邊都是0會 NaN

        if (_beautyHP == value)                                             // 如果HPBar值在中間 (0.5=0.5)
        {
            HPBar.GetComponent<UISlider>().value = value;
        }
        else if (_beautyHP > value)                                         // 如果 舊值>目前值 (我的值比0.5小 分數比別人低)
        {
            HPBar.GetComponent<UISlider>().value = _beautyHP;               // 先等於目前值，然後慢慢減少

            if (_beautyHP >= value)
                _beautyHP -= 0.01f;                                         // 每次執行就減少一些 直到數值相等 (可以造成平滑動畫)
        }
        else if (_beautyHP < value)                                         // 如果 舊值>目前值 (我的值比0.5大 分數比別人高)
        {
            HPBar.GetComponent<UISlider>().value = _beautyHP;               // 先等於目前值，然後慢慢增加

            if (_beautyHP <= value)
                _beautyHP += 0.01f;                                         // 每次執行就增加一些 直到數值相等 (可以造成平滑動畫)
        }
        else if (_score == 0 && _otherScore == 0)
        {
            HPBar.GetComponent<UISlider>().value = _beautyHP;
            //Debug.Log("KKKCCCCC" + _beautyHP);
            if (_beautyHP <= HPBar.GetComponent<UISlider>().value && HPBar.GetComponent<UISlider>().value > 0.5f)
            {
                _beautyHP -= 0.01f;
            }

            if (_beautyHP >= HPBar.GetComponent<UISlider>().value && HPBar.GetComponent<UISlider>().value < 0.5f)
                _beautyHP += 0.01f;
        }

        ComboLabel.GetComponent<UILabel>().text = _combo.ToString();        // 畫出Combo值

    }

    void OnExitRoomEvent()                  // 離開房間時
    {
        Global.isExitingRoom = true;        // 離開房間了
        Global.isMatching = false;          // 無配對狀態
        Global.BattleStatus = false;        // 不在戰鬥中
    }

    void OnOtherScoreEvent(Int16 score)     // 接收對手分數
    {
        _otherScore += score;

        if (MissionManager.missionMode == MissionMode.Opening && MissionManager.mission == Mission.Exchange)
        {
            _score += score;
        }
    }

    void OnMissionCompleteEvent(Int16 missionScore)
    {
        if (MissionManager.mission == Mission.HarvestRate)
        {
            _scoreRate = 1;
        }
        else
        {
            _score += missionScore;
        }
    }

    void OnApplyRateEvent(Mission mission,Int16 scoreRate)
    {
        if (mission == Mission.HarvestRate)
            _scoreRate = scoreRate;
    }
}
