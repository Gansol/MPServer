using UnityEngine;
using System.Collections;
using MPProtocol;
using System;

/*
 * 1.當任意一方達成XXX收穫時
 * 2.老鼠們將吃掉XX糧食 / XX %
 * 3.完美的趕走XX老鼠，增加XX糧食
 * 4.收穫倍率增加 XX / 減少 XX (豐收時刻，旱象出現)
 * 5.交換所獲得的糧食(不含損失)
 * 6.禁止打XX老鼠，減少XX糧食
 * 7.區域王出沒，先消滅者獲得XX糧食。(血量共用?? 會增加伺服器負擔)
 */

public class MissionManager : MonoBehaviour
{
    #region variables
    BattleManager battleManager;

    public static MissionMode missionMode { get { return _missionMode; } }
    public static Mission mission { get { return _mission; } }

    public static MissionMode _missionMode = MissionMode.Closed;// 顯示目前任務模式狀態  之後要改private
    public static Mission _mission = Mission.Harvest;           // 顯示目前執行任務
    public int missionInterval = 10;                            // 任務再次啟動間隔時間
    [Range(10, 25)]
    public int lowerPercent = 25;                               // 較低的一方分數百分比
    [Range(5, 10)]
    public int lowestPercent = 10;                              // 較低的一方分數百分比
    [Range(1, 4)]
    public int balanceTimes = 2;                                // 平衡次數
    public int bossActiveTime = 100;                            // Boss會出現的 時間條件
    public int bossActiveScore = 1000;                          // Boss會出現的 分數條件
    public int endlessTime = 1000;                              // 強制結束時間

    private int activeScore;                                    // grandmother know it!
    private int activeTime;                                     // 遊戲開始後 啟動任務時間
    private int missionTime;                                    // 任務時間限制

    private float avgMissionTime;                               // 平均任務完成時間
    private float gameTime;                                     // 遊戲時間
    private float lastGameTime;                                 // 上一次完成任務的時間
    private Int16 missionScore;                                 // 任務所需分數
    private float lastScore;                                    // 任務開始前分數
    private float missionRate;                                  // 任務倍率

    private bool missionFlag;                                   // 任務是否開啟
    private bool seesawFlag;                                    // 任務蹺蹺板 (A啟動B不啟動..etc)
    private bool _isBadMice;                                    // 是否打到壞老鼠

    #endregion

    void Start()
    {
        Global.photonService.ApplyMissionEvent += OnApplyMission;               // 加入 接受任務 監聽事件
        Global.photonService.OtherMissionScoreEvent += OnOtherMissionComplete;  // 加入 顯示對方 完成任務 監聽事件
        Global.photonService.MissionCompleteEvent += OnMissionComplete;         // 加入 完成任務 監聽事件
        battleManager = GetComponent<BattleManager>();

        activeScore = 1000;
        activeTime = 15;
        missionTime = 60;
        missionRate = 1.0f;
        lastGameTime = 0;

        missionFlag = false;
        seesawFlag = true;
        _isBadMice = false;

        _mission = Mission.None;
        _missionMode = MissionMode.Closed;
    }


    void Update()
    {
        gameTime = Time.timeSinceLevelLoad;
        // 順序 Closed > Completed > Completing > Opeing > Open  倒著寫防止發生Update 2 次以上
        if (Global.isGameStart)
        {
            if (missionMode == MissionMode.Closed)                                  // 任務關閉時，持續判斷是否觸發任務
                MissionTrigger();

            if (missionMode == MissionMode.Completed)                               // 任務完成時，關閉任務並儲存資訊，回到初始狀態
            {
                if (_mission == Mission.Reduce) activeScore -= missionScore;

                missionScore = 0;
                _missionMode = MissionMode.Closed;
                _mission = Mission.None;
                avgMissionTime = (lastGameTime + (gameTime - lastGameTime)) / 2;            // 平均任務完成時間
                lastGameTime = gameTime;                                                    // 任務完成時時間
            }

            if (missionMode == MissionMode.Completing && Global.isMissionCompleted) // 任務完成中，發送完成訊息並等待Server完成資料判斷。
            {
                if (_mission == Mission.DrivingMice)
                {
                    Global.photonService.MissionCompleted((byte)_mission, missionRate, (Int16)battleManager.combo,"");
                }
                else
                {
                    Global.photonService.MissionCompleted((byte)_mission, missionRate, 0,"");
                }

                Global.isMissionCompleted = false;
            }

            if (missionMode == MissionMode.Opening)                                 // 任務開啟中，持續判斷 完成任務/任務失敗
                MissionExecutor(_mission);

            if (missionMode == MissionMode.Open && missionFlag)                     // 任務開始時，發送任務訊息並等待Server回傳任務
            {
                missionFlag = false;
                lastScore = battleManager.score;        // 儲存任務開始前的分數
                lastGameTime = gameTime;                // 任務開始時時間
                Global.photonService.SendMission((byte)_mission, missionRate);
            }
        }
    }

    // 任務事件處發者
    void MissionTrigger()
    {
        if (Global.OtherData.RoomPlace != "Host")       // 如果我是主機才會當任務事件判斷者
        {
            float otherPercent = (battleManager.otherScore / (battleManager.score + battleManager.otherScore)) * 100;
            float myPercent = 100 - otherPercent;

            if ((gameTime - lastGameTime) > missionInterval)                                // 任務間隔時間
            {
                // 如果 我方或對方 分數<10%之間 啟動高平衡機制，只觸發限制次數
                if ((otherPercent < lowestPercent || myPercent < lowestPercent) && balanceTimes > 0 && missionMode == MissionMode.Closed)
                {
                    Mission[] missionSelect = { Mission.Exchange };
                    _mission = missionSelect[UnityEngine.Random.Range(0, 0)];
                    _missionMode = MissionMode.Open;
                    missionFlag = true;
                    balanceTimes--;
                    Debug.Log("我方或對方 分數<10%之間 啟動高平衡機制");

                }// 如果 我方或對方 分數再10~25%之間 啟動低平衡機制，只觸發限制次數
                else if ((myPercent < lowerPercent && myPercent > lowestPercent) || (myPercent < lowerPercent && myPercent > lowestPercent)
                        && balanceTimes > 0 && missionMode == MissionMode.Closed)
                {
                    Mission[] missionSelect = { Mission.Exchange};
                    _mission = missionSelect[UnityEngine.Random.Range(0, 2)];
                    _missionMode = MissionMode.Open;
                    missionFlag = true;
                    balanceTimes--;
                    Debug.Log("我方或對方 分數再10~25%之間 啟動低平衡機制");
                }

                // 如果遊戲時間 > 觸發時間 啟動任務(收穫、趕老鼠) (如果分數觸發 則 時間不觸發)
                if (gameTime > (lastGameTime + activeTime) && seesawFlag && missionMode == MissionMode.Closed)
                {
                    Mission[] missionSelect = { Mission.Exchange, Mission.Harvest, Mission.DrivingMice, Mission.Reduce };
                    _mission = missionSelect[UnityEngine.Random.Range(0, 5)];
                    activeTime += activeTime + UnityEngine.Random.Range(0, (int)(activeTime / 2));
                    _missionMode = MissionMode.Open;
                    missionFlag = true;
                }

                // 如果 任意玩家遊戲分數 > 觸發分數 啟動任務 (如果時間觸發 則 分數不觸發)
                if ((battleManager.score > activeScore || battleManager.otherScore > activeScore) && !seesawFlag && missionMode == MissionMode.Closed)
                {
                    Mission[] missionSelect = { Mission.Exchange,Mission.Harvest, Mission.DrivingMice };
                    _mission = missionSelect[UnityEngine.Random.Range(0, 4)];
                    activeScore += activeScore + UnityEngine.Random.Range(0, (int)(activeScore / 2));
                    _missionMode = MissionMode.Open;
                    missionFlag = true;
                }

                // 如果雙方遊戲分數、遊戲時間 > 觸發條件 出現BOSS
                if (battleManager.score > bossActiveScore && battleManager.otherScore > bossActiveScore && gameTime > bossActiveTime && missionMode == MissionMode.Closed)
                {
                    _mission = Mission.WorldBoss;
                    _missionMode = MissionMode.Open;
                    missionFlag = true;
                }
            }

        }
    }

    // 任務事件處理者
    void MissionExecutor(Mission mission)
    {
        ShowMissionLabel(mission, missionScore);
        switch (mission)
        {
            case Mission.None:
                {
                    break;
                }
            case Mission.Harvest:   // 達成XX收穫
                {
                    if ((battleManager.score - lastScore) >= missionScore)      // success
                    {
                        _missionMode = MissionMode.Completing;
                        Global.isMissionCompleted = true;
                    }
                    else if (gameTime - lastGameTime > missionTime)                            // failed
                    {
                        _missionMode = MissionMode.Completed;
                        ShowFailedLabel();
                    }
                    break;
                }
            case Mission.Reduce:        // 完成後 activeScore要減少Reduce的量
                {
                    float endTime = gameTime - lastGameTime - missionTime;
                    if (endTime > -5 && endTime < 0) // 減少糧食 這比較特殊 需要顯示閃爍血調 還沒寫
                    {
                        ShingHPBar();
                    }
                    else if (gameTime - lastGameTime > missionTime)
                    {
                        _missionMode = MissionMode.Completing;
                        Global.isMissionCompleted = true;
                    }
                    break;
                }
            case Mission.DrivingMice:
                {
                    if (gameTime - lastGameTime > missionTime)
                    {
                        if (battleManager.combo == 0)
                        {
                            _missionMode = MissionMode.Completed;
                            ShowFailedLabel();
                        }
                    }
                    else
                    {
                        _missionMode = MissionMode.Completing;
                        Global.isMissionCompleted = true;
                    }
                    break;
                }
            case Mission.HarvestRate:
                {
                    if (gameTime - lastGameTime > missionTime)
                    {
                        HarvestRate();

                        _missionMode = MissionMode.Completing;
                        Global.isMissionCompleted = true;
                    }
                    break;
                }
            case Mission.Exchange:
                {
                    if (gameTime - lastGameTime > missionTime)
                    {
                        _missionMode = MissionMode.Completing;
                        Global.isMissionCompleted = true;
                    }
                    break;
                }
            case Mission.WorldBoss: // 要計算網路延遲... 還沒寫
                if (gameTime - lastGameTime > missionTime)
                {
                    _missionMode = MissionMode.Completing;
                    Global.isMissionCompleted = true;
                }
                break;
        }
    }

    void HarvestRate()
    {
        //if ((gameTime - lastGameTime - missionTime) > -5)
        //{
        //    // 慢慢變淡
        //}
        // 顯示 倍率圖樣 if flag
        Debug.Log("HarvestRate : ");
    }

    void ReduceHPBar()
    {
        Debug.Log("Shing....");
        //gui amins;
        //xxx.Play();
    }

    void ShingHPBar()
    {
        Debug.Log("Shing....");
        //gui amins;
        //xxx.Play();
    }

    void ShowMissionLabel(Mission mission, Int16 missionScore)
    {
        // show message box
        //if (flag)
        //{

        //}

        Debug.Log(mission + "MISSION STARTING...");
    }

    void ShowFailedLabel()
    {
        // show message box
        //if (flag)
        //{

        //}
        Debug.Log("MISSION Failed...");
    }

    void OnApplyMission(Mission mission, Int16 missionScore)
    {
        // recive server send event message
        Global.missionFlag = true;
        this.missionScore = missionScore;
        MissionManager._mission = mission;
        _missionMode = MissionMode.Opening;
    }

    void OnMissionComplete(Int16 missionReward)
    {
        // to show message box
        Debug.Log(" Mission Completed !   Get +" + missionReward);
        _missionMode = MissionMode.Completed;
    }

    void OnOtherMissionComplete(Int16 otherMissioReward)
    {
        // to show message box
        Debug.Log("Other Player Mission Completed ! +" + otherMissioReward + "Score.");
    }

    void OnBossDied()
    {
        // to show message box
        // 接收對方打死訊息
       // Debug.Log("Other Player Completed Mission !   Get +" + missionReward);
    }

    void OnAnsycTime()
    {
        // to recive Host Last GameTime;
    }
}
