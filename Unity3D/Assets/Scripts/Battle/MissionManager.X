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
    BattleManager battleManager;
    public MissionMode missionMode = MissionMode.Closed;        // 顯示目前任務模式狀態
    public Mission mission = Mission.Harvest;                   // 顯是目前執行任務
    public int missionInterval = 10;                            // 任務再次啟動間隔時間
    public int lowerPercent = 25;                               // 較低的一方分數百分比
    public int lowestPercent = 10;                               // 較低的一方分數百分比
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

    // Use this for initialization
    void Start()
    {
        Global.photonService.ApplyMissionEvent += OnApplyMissionEvent;
        Global.photonService.ShowMissionScoreEvent += OnShowMissionScoreEvent;
        Global.photonService.MissionCompleteEvent += MissionCompleteEvent;
        battleManager = GetComponent<BattleManager>();

        activeScore = 1000;
        activeTime = 15;
        missionTime = 60;
        missionRate = 1.0f;
        lastGameTime = 0;
        missionFlag = false;
    }

    // Update is called once per frame
    void Update()
    {
        // 順序 Closed > Completed > Completing > Opeing > Open  倒著寫防止發生Update 2 次以上
        if (Global.isGameStart)
        {
            gameTime = Time.timeSinceLevelLoad;   // 遊戲時間

            if (missionMode == MissionMode.Closed)
            {
                // 任務結束 並判斷下次會觸發的任務
                MissionTrigger();
            }
            else if (missionMode == MissionMode.Completed)
            {
                missionMode = MissionMode.Closed;
                mission = Mission.None;
                avgMissionTime = (lastGameTime + (gameTime - lastGameTime)) / 2;     // 平均任務完成時間
                lastGameTime = gameTime;                            // 任務完成時時間

            }
            else if (missionMode == MissionMode.Completing)
            {
                if (Global.isMissionCompleted)  // 去掉看看
                {
                    // clac
                    Global.photonService.MissionComplete((byte)mission, missionRate);
                    Global.isMissionCompleted = false;
                }
            }
            else if (missionMode == MissionMode.Opening)
            {
                // 任務執行時
                ShowMissionLabel(mission, missionScore);
                MissionExecutor(mission);
            }
            else if (missionMode == MissionMode.Open)
            {
                // 任務開始時
                if (missionFlag)
                {
                    missionFlag = false;
                    lastScore = battleManager.score;        // 儲存任務開始前的分數
                    lastGameTime = gameTime;                // 任務開始時時間
                    Global.photonService.SendMission((byte)mission, missionRate);        // 會一直傳
                }
            }
        }
    }


    // 任務事件處理者
    void MissionExecutor(Mission mission)
    {

        switch (mission)
        {
            case Mission.None:
                {
                    break;
                }
            case Mission.Harvest:
                {
                    if ((battleManager.score - lastScore) >= missionScore)      // success
                    {
                        missionMode = MissionMode.Completing;
                        Global.isMissionCompleted = true;
                    }
                    else if (gameTime > missionTime)                            // failed
                    {
                        missionMode = MissionMode.Completed;
                        ShowFailedLabel();
                    }
                    break;
                }
            case Mission.Reduce:
                {
                    if ((gameTime - lastGameTime - missionTime) > -5) // 減少糧食 這比較特殊 需要顯示閃爍血調 還沒寫
                    {
                        ShingHPBar();
                    }
                    else if (gameTime - lastGameTime > missionTime)      
                    {
                        missionMode = MissionMode.Completing;
                    }
                    break;
                }
            case Mission.DrivingMice:
                {
                    break;
                }
            case Mission.HarvestRate:
                {
                    break;
                }
            case Mission.BadMice:
                {
                    break;
                }
            case Mission.Exchange:
                {
                    break;
                }
            case Mission.WorldBoss:
                break;
        }
    }


    // 任務事件處發者
    void MissionTrigger()
    {
        // UnityEngine.Random.seed = System.Guid.NewGuid().GetHashCode();
        if (Global.OtherData.RoomPlace != "Host")       // 如果我是主機才會當任務事件判斷者
        {
            float scorePercent = (battleManager.otherScore / battleManager.score + battleManager.otherScore) * 100;

            if ((gameTime - lastGameTime) > missionInterval)    // 任務間隔時間
            {
                // 如果對方分數<10%之間 起動高平衡機制
                if (scorePercent < lowestPercent && missionMode == MissionMode.Closed)   // 錯誤 觸發1次
                {
                    Mission[] missionSelect = { Mission.Exchange, Mission.BadMice };
                    mission = missionSelect[UnityEngine.Random.Range(0, 2)];
                    missionMode = MissionMode.Open;
                    missionFlag = true;
                }// 如果對方分數再10~25%之間 起動低平衡機制
                else if (scorePercent < lowerPercent && scorePercent > lowestPercent && missionMode == MissionMode.Closed)   // **錯誤 觸發1次
                {
                    Mission[] missionSelect = { Mission.Exchange, Mission.BadMice };
                    mission = missionSelect[UnityEngine.Random.Range(0, 2)];
                    missionMode = MissionMode.Open;
                    missionFlag = true;
                }
                else if ((gameTime > (lastGameTime + activeTime)) && missionMode == MissionMode.Closed)  // 一直觸發
                {
                    Mission[] missionSelect = { Mission.Harvest, Mission.DrivingMice };
                    mission = missionSelect[UnityEngine.Random.Range(0, 3)];
                    activeTime += UnityEngine.Random.Range(0, (int)(activeTime / 2));
                    missionMode = MissionMode.Open;
                    missionFlag = true;
                }
                else if ((battleManager.score > activeScore) && missionMode == MissionMode.Closed)   // 一直觸發
                {
                    Mission[] missionSelect = { Mission.Harvest, Mission.BadMice, Mission.DrivingMice };
                    mission = missionSelect[UnityEngine.Random.Range(0, 2)];
                    activeScore += UnityEngine.Random.Range(0, (int)(activeScore / 2));
                    missionMode = MissionMode.Open;
                    missionFlag = true;
                }
            }

        }
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

    void OnApplyMissionEvent(Mission mission, Int16 missionScore)
    {
        // recive server send event message
        Global.missionFlag = true;
        this.missionScore = missionScore;
        this.mission = mission;
        missionMode = MissionMode.Opening;
    }

    void MissionCompleteEvent(Int16 missionScore)
    {
        // to show message box
        Debug.Log(" Mission Completed !   Get +" + missionScore);
    }

    void OnShowMissionScoreEvent(Int16 missionScore)
    {
        // to show message box
        Debug.Log("Other Player Completed Mission !   Get +" + missionScore);
    }
}
