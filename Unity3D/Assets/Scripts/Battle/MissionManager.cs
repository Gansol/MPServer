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

    public MissionMode missionMode = MissionMode.Closed;        // 顯示目前任務模式狀態
    public Mission mission = Mission.Harvest;                   // 顯是目前執行任務
    BattleManager battleManager;

    public int missionInterval = 10;    // 目前還沒用到

    private int activeScore;                                    // grandmother know it!
    private int activeTime;                                     // 遊戲開始後 啟動任務時間
    private int missionTime;                                  // 任務時間限制

    private float gameTime;                                     // 遊戲時間
    private float lastGameTime;                                 // 上一次完成任務的時間
    private Int16 missionScore;                                 // 任務所需分數
    private float lastScore;                                    // 任務開始前分數
    private float missionRate;                                    // 任務倍率

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
    }

    // Update is called once per frame
    void Update()
    {
        // 順序 Closed > Completed > Completing > Opeing > Open  倒著寫防止發生Update 2 次以上
        if (Global.isGameStart)
        {
            gameTime = Time.timeSinceLevelLoad;   // 遊戲時間
            //Debug.Log(gameTime);
            //if (Global.missionFlag)
            //{
                if (missionMode == MissionMode.Closed)
                {
                    // 任務結束 並判斷下次會觸發的任務
                    if (Global.OtherData.RoomPlace != "Host")
                    {
                        if (gameTime > lastGameTime + activeTime)
                        {
                            mission = Mission.Harvest;
                            missionMode = MissionMode.Open;
                            activeTime += 900;
                        }
                    }
                }
                else if (missionMode == MissionMode.Completed)
                {
                    missionMode = MissionMode.Closed;
                    mission = Mission.None;
                    lastGameTime = gameTime;
                    //Global.missionFlag = false;
                }
                else if (missionMode == MissionMode.Completing)
                {
                    if (Global.isMissionCompleted)
                    {
                        // clac
                        Global.photonService.MissionComplete((byte)mission, missionRate);
                        Global.isMissionCompleted = false;
                    }
                }
                else if (missionMode == MissionMode.Opening)
                {
                    // 任務執行時
                    switch (mission)
                    {
                        case Mission.None:
                            {
                                break;
                            }
                        case Mission.Harvest:
                            {
                                ShowMissionLabel(mission, missionScore);

                                if ((battleManager.score - lastScore) >= (missionScore -190))      // success
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
                    }
                }
                else if (missionMode == MissionMode.Open)
                {
                    // 任務開始時
                    lastScore = battleManager.score;        // 儲存任務開始前的分數
                    Global.photonService.SendMission((byte)mission,missionRate);        // 會一直傳
                }

            //}
        }
    }

    void ShowMissionLabel(Mission mission, Int16 missionScore)
    {
        // show message box
        //if (flag)
        //{

        //}

        Debug.Log("MISSION STARTING...");
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
