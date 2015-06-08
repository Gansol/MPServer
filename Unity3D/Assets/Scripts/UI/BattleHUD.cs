using UnityEngine;
using System.Collections;
using MPProtocol;

public class BattleHUD : MonoBehaviour
{
    float ckeckTime;

    public GameObject HPBar;
    public GameObject ComboLabel;
    public GameObject BlueScore;
    public GameObject RedScore;

    [Range(0.1f,1.0f)]
    public float _beautyHP;                // 美化血條用

    private BattleManager battleManager;

    void Start()
    {
        battleManager = GetComponent<BattleManager>();

        Global.photonService.WaitingPlayerEvent += OnWaitingPlayer;
        Global.photonService.ExitRoomEvent += OnExitRoom;
    }

    void Update()
    {  
        if (ckeckTime > 15)
        {
            Global.photonService.CheckStatus();
            ckeckTime = 0;
        }
        else
        {
            ckeckTime += Time.deltaTime;
        }  
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 100), "ExitRoom"))
        {
            Global.photonService.KickOther();
            Global.photonService.ExitRoom();
        }

        BlueScore.GetComponent<UILabel>().text = battleManager.score.ToString();         // 畫出分數值
        RedScore.GetComponent<UILabel>().text = battleManager.otherScore.ToString();     // 畫出分數值

        float value = battleManager.score / (battleManager.score + battleManager.otherScore);                      // 得分百分比 兩邊都是0會 NaN

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
        else if (battleManager.score == 0 && battleManager.otherScore == 0)
        {
            HPBar.GetComponent<UISlider>().value = _beautyHP;
            if (_beautyHP <= HPBar.GetComponent<UISlider>().value && HPBar.GetComponent<UISlider>().value > 0.5f)
            {
                _beautyHP -= 0.01f;
            }

            if (_beautyHP >= HPBar.GetComponent<UISlider>().value && HPBar.GetComponent<UISlider>().value < 0.5f)
                _beautyHP += 0.01f;
        }

        ComboLabel.GetComponent<UILabel>().text = battleManager.combo.ToString();        // 畫出Combo值

    }


    public void HPBar_Shing()
    {
        Debug.Log("HPBar_Shing !");
    }

    public void MissionMsg(Mission mission,float value)
    {
        switch (mission)
        {
            case Mission.Harvest:
                Debug.Log("Mission : Harvest! 豐收時刻 取得: " + value + " 糧食");
                break;
            case Mission.HarvestRate:
                Debug.Log("Mission : HarvestRate UP+! 收穫倍率:"+value);
                break;
            case Mission.Exchange:
                Debug.Log("Mission : Exchange! 交換收穫的糧食");
                break;
            case Mission.Reduce:
                Debug.Log("Mission : Reduce! 豐收祭典 花費: " + value + " 糧食");
                break;
            case Mission.DrivingMice:
                Debug.Log("Mission : DrivingMice! 驅趕老鼠 數量: " + value + " 隻");
                break;
            case Mission.WorldBoss:
                Debug.Log("Mission WARNING 世界王出現!!");
                break;
        }
        
    }

    public void MissionCompletedMsg(Mission mission , float missionReward){
        switch (mission)
        {
            case Mission.Harvest:
                Debug.Log("Mission : Completed! 取得: " + missionReward + " 糧食");
                break;
            case Mission.HarvestRate:
                Debug.Log("Mission 收穫倍率復原 = 1");
                break;
            case Mission.Exchange:
                Debug.Log("Mission 任務結束:不再交換糧食");
                break;
            case Mission.Reduce:
                Debug.Log("Mission : Reduce! 豐收祭典 花費: " + missionReward + " 糧食");
                break;
            case Mission.DrivingMice:
                Debug.Log("Mission : Completed!  取得: " + missionReward + " 糧食");
                break;
            case Mission.WorldBoss:
                Debug.Log("Mission : Completed!  取得: " + missionReward + " 糧食");
                break;
        }
    }

    public void OtherScoreMsg(float missionReward)
    {
        Debug.Log("Other MissionCompleted! + "+missionReward);
    }

    public void MissionFailedMsg()
    {
        Debug.Log("Mission Failed !");
    }



    void OnWaitingPlayer()
    {
        Debug.Log("Waiting Other Player . . .");
    }

    void OnExitRoom()
    {
        Global.photonService.WaitingPlayerEvent -= OnWaitingPlayer;
        Global.photonService.ExitRoomEvent -= OnExitRoom;
    }
}
