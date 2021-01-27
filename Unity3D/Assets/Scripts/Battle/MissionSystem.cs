using UnityEngine;
using System.Collections;
using MPProtocol;
using System;

/* 如果發現任務不同步 重啟伺服器
 * seesawFlag 這裡怪怪的 會同時開啟
 * 1.當任意一方達成XXX收穫時
 * 2.老鼠們將吃掉XX糧食 / XX %
 * 3.完美的趕走XX老鼠，增加XX糧食
 * 4.收穫倍率增加 XX / 減少 XX (豐收時刻，旱象出現)
 * 5.交換所獲得的糧食(不含損失)
 * 6.禁止打XX老鼠，減少XX糧食
 * 7.區域王出沒，先消滅者獲得XX糧食。(血量共用?? 會增加伺服器負擔)
 */

public class MissionSystem : IGameSystem
{
    BattleSystem m_BattleSystem;
    BattleUI m_BattleUI;

    #region Variables 變數
    //public GameObject[] missionICON;                            // 任務圖示



    public MissionMode MissionMode { get { return _missionMode; } }
    public Mission Mission { get { return _mission; } }

    private MissionMode _missionMode;// 顯示目前任務模式狀態  之後要改private
    private Mission _mission;                       // 顯示目前執行任務

    private short _missionScore;          // 任務所需分數
    private int _balanceTimes;              // 平衡次數    [Range(1, 4)]
    private int _lowerPercent;               // 較低的一方分數百分比    [Range(10, 25)]
    private int _lowestPercent;             // 最低的一方分數百分比    [Range(5, 10)]
    private int _missionInterval;           // 任務再次啟動間隔時間
    private int _bossActiveTime;          // Boss會出現的 時間條件
    private int _bossActiveScore;         // Boss會出現的 分數條件
    private int _activeScore;                  // 任務觸發分數
    private int _activeTime;                   // 遊戲開始後 啟動任務時間
    private int _endlessTime;                // 強制結束時間
    private int _missionTime;                // 任務時間限制

    private float _lastScore;                   // 任務開始前分數
    private float _missionRate;             // 任務倍率
    private double _avgMissionTime; // 平均任務完成時間
    private double _lastMissionCompletedTime;     // 上一次完成任務的時間
    private bool _seesawFlag;               // 任務蹺蹺板 (A啟動B不啟動..etc)
    private bool _bMissionCompleted;
    //    private bool _isBadMice;                                    // 是否打到壞老鼠
    #endregion

    public MissionSystem(MPGame MPGame) : base(MPGame)
    {
        Debug.Log("--------------- MissionSystem Create ----------------");
    }

    public override void Initialize()
    {
        Debug.Log("--------------- MissionSystem Initialize ----------------");
        m_BattleUI = m_MPGame.GetBattleUI();
        m_BattleSystem = m_MPGame.GetBattleSystem();

        _missionInterval = 10;
        _lowerPercent = 25;
        _lowestPercent = 10;
        _balanceTimes = 2;
        _bossActiveTime = 100;
        _bossActiveScore = 1000;
        _endlessTime = 1000;
        _activeScore = 1000;
        _activeTime = 15;
        _missionTime = 10;
        _missionRate = 1.0f;
        _lastMissionCompletedTime = 0;

        _seesawFlag = true;
        _bMissionCompleted = false;
        _mission = Mission.None;
        _missionMode = MissionMode.Closed;

        Global.photonService.LoadSceneEvent += OnLoadScene;                                          // 移除 離開房間 監聽事件
        Global.photonService.ApplyMissionEvent += OnApplyMission;                               // 加入 接受任務 監聽事件
        Global.photonService.MissionCompleteEvent += OnMissionComplete;                 // 加入 完成任務 監聽事件
        Global.photonService.OtherMissionScoreEvent += OnOtherMissionComplete;  // 加入 顯示對方 完成任務 監聽事件
    }

    public override void Update()
    {
        base.Update();
        // 順序 Closed > Completed > Completing > Opeing > Open  倒著寫防止發生Update 2 次以上
        if (Global.isGameStart)
        {
            if (MissionMode == MissionMode.Closed)           // 任務關閉時，持續判斷是否觸發任務              
                MissionTrigger();
            if (MissionMode == MissionMode.Opening)        // 任務開啟中，持續判斷 完成任務/任務失敗
                MissionExecutor(_mission);
            if (MissionMode == MissionMode.Completing)  // 任務完成中，發送完成訊息並等待Server完成資料判斷。
                MissionCompleting();
            if (MissionMode == MissionMode.Completed)    // 任務完成時，關閉任務並儲存資訊，回到初始狀態
                MissionCompleted();
        }
    }

    #region -- SendMission2Server 發送任務到伺服器 --
    /// <summary>
    /// 發送任務到伺服器
    /// </summary>
    /// <param name="mission">任務</param>
    private void SendMission2Server(Mission mission)
    {
        // 任務開始時，發送任務訊息並等待Server回傳任務
        if (MissionMode == MissionMode.Open)
        {
            _lastScore = m_BattleSystem.GetBattleAttr().score;        // 儲存任務開始前的分數
            Global.photonService.SendMission((byte)mission, _missionRate);
        }
    }
    #endregion

    #region -- MissionTrigger 任務事件處發者 --
    /// <summary>
    /// 任務事件處發者 (錯誤，應該伺服器判斷)
    /// </summary>
    void MissionTrigger()
    {
        if (Global.OpponentData.RoomPlace != "Host")       // 如果我是主機才會當任務事件判斷者
        {
            float _otherScore = m_BattleSystem.GetBattleAttr().otherScore;
            float _score = m_BattleSystem.GetBattleAttr().score;
            float otherPercent = (_otherScore / (_score + _otherScore)) * 100;
            float myPercent = 100 - otherPercent;

            if ((m_BattleSystem.GetBattleAttr().gameTime - _lastMissionCompletedTime) > _missionInterval)                                // 任務間隔時間
            {
                Debug.Log("----------MissionTrigger----------");
                // 如果 我方或對方 分數<10%之間 啟動高平衡機制，只觸發限制次數
                if ((otherPercent < _lowestPercent || myPercent < _lowestPercent) && _balanceTimes > 0 && MissionMode == MissionMode.Closed && _otherScore != 0 && _score != 0)
                {
                    Mission[] missionSelect = { Mission.HarvestRate };
                    _mission = missionSelect[UnityEngine.Random.Range(0, 0)];
                    _missionMode = MissionMode.Open;
                    SendMission2Server(_mission);
                    _balanceTimes--;
                    Debug.Log("我方或對方 分數<10%之間 啟動高平衡機制");

                }// 如果 我方或對方 分數再10~25%之間 啟動低平衡機制，只觸發限制次數
                else if ((myPercent < _lowerPercent && myPercent > _lowestPercent) || (myPercent < _lowerPercent && myPercent > _lowestPercent && _otherScore != 0 && _score != 0)
                        && _balanceTimes > 0 && MissionMode == MissionMode.Closed)
                {
                    Mission[] missionSelect = { Mission.HarvestRate };
                    _mission = missionSelect[UnityEngine.Random.Range(0, 0)];
                    _missionMode = MissionMode.Open;
                    SendMission2Server(_mission);
                    _balanceTimes--;
                    Debug.Log("我方或對方 分數再10~25%之間 啟動低平衡機制");
                }

                // 如果遊戲時間 > 觸發時間 啟動任務(收穫、趕老鼠) (如果分數觸發 則 時間不觸發)
                if (m_BattleSystem.GetBattleAttr().gameTime > (_lastMissionCompletedTime + _activeTime) && _seesawFlag && MissionMode == MissionMode.Closed)
                {
                    Mission[] missionSelect = { Mission.Exchange, Mission.DrivingMice, Mission.Harvest, Mission.WorldBoss };
                    //Mission[] missionSelect = { Mission.WorldBoss, Mission.WorldBoss, Mission.WorldBoss, Mission.WorldBoss };
                    _mission = missionSelect[UnityEngine.Random.Range(0, 4)];
                    _activeTime += _activeTime + UnityEngine.Random.Range(0, (int)(_activeTime / 2));
                    _missionMode = MissionMode.Open;
                    SendMission2Server(_mission);
                    _seesawFlag = false;
                    Debug.Log("如果遊戲時間 > 觸發時間 啟動任務(收穫、趕老鼠) (如果分數觸發 則 時間不觸發)");

                    // Debug.Log("gameTime" + gameTime + "lastGameTime" + lastGameTime + "activeTime" + activeTime);
                }

                // 如果 任意玩家遊戲分數 > 觸發分數 啟動任務 (如果時間觸發 則 分數不觸發)
                if ((_score > _activeScore || _otherScore > _activeScore) && !_seesawFlag && MissionMode == MissionMode.Closed)
                {
                    Mission[] missionSelect = { Mission.Exchange, Mission.DrivingMice, Mission.Harvest, Mission.WorldBoss };
                    // Mission[] missionSelect = { Mission.WorldBoss, Mission.WorldBoss, Mission.WorldBoss, Mission.WorldBoss };
                    _mission = missionSelect[UnityEngine.Random.Range(0, 4)];
                    _activeScore += _activeScore + UnityEngine.Random.Range(0, (int)(_activeScore / 2));
                    _missionMode = MissionMode.Open;
                    SendMission2Server(_mission);
                    _seesawFlag = true;
                    Debug.Log("// 如果 任意玩家遊戲分數 > 觸發分數 啟動任務 (如果時間觸發 則 分數不觸發)");
                }

                // 如果雙方遊戲分數、遊戲時間 > 觸發條件 出現BOSS
                if (_score > _bossActiveScore && _otherScore > _bossActiveScore && m_BattleSystem.GetBattleAttr().gameTime > _bossActiveTime && MissionMode == MissionMode.Closed)
                {
                    _mission = Mission.WorldBoss;
                    _missionMode = MissionMode.Open;
                    SendMission2Server(_mission);
                    Debug.Log("// 如果雙方遊戲分數、遊戲時間 > 觸發條件 出現BOSS");
                }
            }

        }
    }
    #endregion

    #region -- MissionExecutor 任務事件處理者 --
    /// <summary>
    /// 任務事件處理者 (錯誤)
    /// </summary>
    /// <param name="mission">任務</param>
    private void MissionExecutor(Mission mission)
    {
        switch (mission)
        {
            case Mission.Harvest:   // 達成XX收穫
                {
                    if ((m_BattleSystem.GetBattleAttr().score - _lastScore) >= _missionScore)      // success
                    {
                        _missionMode = MissionMode.Completing;
                        _bMissionCompleted = true;
                    }
                    else if (m_BattleSystem.GetBattleAttr().gameTime - _lastMissionCompletedTime > _missionTime)                            // failed
                    {
                        _missionMode = MissionMode.Completed;
                        m_BattleUI.MissionFailedMsg(mission, 0);
                    }
                    break;
                }
            case Mission.Reduce:        // 完成後 activeScore要減少Reduce的量
                {
                    double endTime = m_BattleSystem.GetBattleAttr().gameTime - _lastMissionCompletedTime - _missionTime;
                    if (endTime > -5 && endTime < 0) // 減少糧食 這比較特殊 需要顯示閃爍血調 還沒寫
                    {
                        m_BattleUI.HPBar_Shing();
                    }
                    else if (m_BattleSystem.GetBattleAttr().gameTime - _lastMissionCompletedTime > _missionTime)
                    {
                        _missionMode = MissionMode.Completing;
                        _bMissionCompleted = true;
                    }
                    break;
                }
            case Mission.DrivingMice:
                {
                    if (m_BattleSystem.GetBattleAttr().gameTime - _lastMissionCompletedTime > _missionTime)       //missionScore 這裡是 Combo任務目標
                    {
                        if (m_BattleSystem.MissionCombo >= _missionScore)   // success 20200817 改動過 修正顯示任務失敗錯誤
                        {
                            _missionMode = MissionMode.Completing;
                            m_BattleUI.MissionCompletedMsg(mission, 0);
                            _bMissionCompleted = true;

                        }
                        else
                        {
                            _missionMode = MissionMode.Completed;           // failed
                            m_BattleUI.MissionFailedMsg(mission, 0);
                        }
                    }
                    break;
                }
            case Mission.HarvestRate:
                {
                    if (m_BattleSystem.GetBattleAttr().gameTime - _lastMissionCompletedTime > _missionTime)
                    {
                        _missionMode = MissionMode.Completing;
                        _bMissionCompleted = true;
                    }
                    break;
                }
            case Mission.Exchange:
                {
                    if (m_BattleSystem.GetBattleAttr().gameTime - _lastMissionCompletedTime > _missionTime)
                    {
                        _missionMode = MissionMode.Completing;
                        _bMissionCompleted = true;
                    }
                    break;
                }
            case Mission.WorldBoss: // 要計算網路延遲... ＊＊＊＊＊＊＊＊＊＊還沒寫＊＊＊＊＊＊＊＊＊＊＊＊＊
                                    // 在 BossPorperty在邏輯判斷
                break;
        }
    }
    #endregion

    #region -- MissionCompleted 任務完成 --
    private void MissionCompleted()
    {
        if (_mission == Mission.Reduce) _activeScore -= _missionScore;

        _missionScore = 0;
        _missionMode = MissionMode.Closed;
        _mission = Mission.None;
        _avgMissionTime = (_lastMissionCompletedTime + (m_BattleSystem.GetBattleAttr().gameTime - _lastMissionCompletedTime)) / 2;            // 平均任務完成時間
        _lastMissionCompletedTime = m_BattleSystem.GetBattleAttr().gameTime;                                                    // 任務完成時時間
    }
    #endregion

    #region -- MissionCompleting 任務完成時 --
    /// <summary> 
    /// 任務完成時 發送任務完成給伺服器
    /// </summary>
    private void MissionCompleting()
    {
        _missionMode = MissionMode.Closed;
        if (_mission == Mission.DrivingMice)
        {
            Global.photonService.MissionCompleted((byte)_mission, _missionRate, m_BattleSystem.GetBattleAttr().combo, "");
            return;
        }

        Global.photonService.MissionCompleted((byte)_mission, _missionRate, 0, "");
    }
    #endregion

    #region -- OnApplyMission 當收到任務 --
    /// <summary>
    /// 當收到任務
    /// </summary>
    /// <param name="mission"></param>
    /// <param name="missionScore"></param>
    void OnApplyMission(Mission mission, short missionScore)
    {
        if (Global.isGameStart)
        {
            if (mission != Mission.HarvestRate) m_BattleUI.MissionMsg(mission, missionScore);
            _missionScore = missionScore;
            _mission = mission;
            _lastMissionCompletedTime = m_BattleSystem.GetBattleAttr().gameTime;                // 任務開始時時間
            _missionMode = MissionMode.Opening;
        }

    }
    #endregion

    #region -- OnMissionComplete 任務完成 --
    /// <summary>
    /// 收到伺服器任務完成資料
    /// </summary>
    /// <param name="missionReward"></param>
    void OnMissionComplete(short missionReward)
    {
        if (Global.isGameStart)
        {
            Debug.Log("OnMissionManager:" + missionReward);
            _bMissionCompleted = false;
            _missionMode = MissionMode.Completed;

            if (/*Mission == Mission.WorldBoss &&*/ missionReward < 0)
            {
                m_BattleUI.MissionFailedMsg(Mission, 0);
                return;
            }

            m_BattleUI.MissionCompletedMsg(Mission, missionReward);

        }
    }
    #endregion

    #region -- OnOtherMissionComplete 收到對手任務完成 --
    /// <summary>
    /// 收到對手任務完成
    /// </summary>
    /// <param name="otherMissioReward"></param>
    void OnOtherMissionComplete(short otherMissioReward)
    {
        // 顯示敵方得分
        if (Global.isGameStart)
            m_BattleUI.OtherScoreMsg(otherMissioReward);
    }
    #endregion

    #region -- OnLoadScene --
    void OnLoadScene()
    {
        Global.photonService.LoadSceneEvent -= OnLoadScene;                       // 移除 離開房間 監聽事件
        Global.photonService.ApplyMissionEvent -= OnApplyMission;               // 移除 接受任務 監聽事件
        Global.photonService.MissionCompleteEvent -= OnMissionComplete;         // 移除 完成任務 監聽事件
        Global.photonService.OtherMissionScoreEvent -= OnOtherMissionComplete;  // 移除 顯示對方 完成任務 監聽事件
    }
    #endregion

    #region -- Release --
    public override void Release()
    {
        Global.photonService.LoadSceneEvent -= OnLoadScene;                       // 移除 離開房間 監聽事件
        Global.photonService.ApplyMissionEvent -= OnApplyMission;               // 移除 接受任務 監聽事件
        Global.photonService.MissionCompleteEvent -= OnMissionComplete;         // 移除 完成任務 監聽事件
        Global.photonService.OtherMissionScoreEvent -= OnOtherMissionComplete;  // 移除 顯示對方 完成任務 監聽事件


    }
    #endregion

    /// <summary>
    /// 同步時間 (還沒寫)
    /// </summary>
    void OnAnsycTime()
    {
        // to recive Host Last GameTime;
    }
}
