using UnityEngine;
using System.Collections;
using MPProtocol;
using System;

public class BattleHUD : MonoBehaviour
{
    float ckeckTime;

    public GameObject HPBar;
    public GameObject ComboLabel;
    public GameObject BlueScore;
    public GameObject RedScore;
    public GameObject EnergyBar;
    public GameObject Combo;
    public GameObject MissionObject;
    public GameObject WaitObject;
    public GameObject StartObject;
    public GameObject ScorePlusObject;
    public GameObject OtherPlusObject;
    public GameObject GGObject;
    public GameObject BossHPBar;

    [Range(0.1f, 1.0f)]
    public float _beautyHP;                // 美化血條用

    private double _beautyEnergy;
    //private double _energy;
    private BattleManager battleManager;


    void Start()
    {
        battleManager = GetComponent<BattleManager>();

        Global.photonService.WaitingPlayerEvent += OnWaitingPlayer;
        Global.photonService.LoadSceneEvent += OnLoadScene;

        _beautyEnergy = 0d;
        //_energy = 0d;
    }

    void Update()
    {
        #region 動畫類判斷 DisActive
        if (WaitObject.activeSelf)
        {
            if (Global.isGameStart)
                WaitObject.GetComponent<Animator>().Play("Wait");

            Animator waitAnims = WaitObject.GetComponent("Animator") as Animator;
            AnimatorStateInfo waitState = waitAnims.GetCurrentAnimatorStateInfo(0);             // 取得目前動畫狀態 (0) = Layer1
            
            if (waitState.nameHash == Animator.StringToHash("Layer1.Wait"))                  // 如果 目前 動化狀態 是 Waiting
                if (waitState.normalizedTime > 0.1f) WaitObject.SetActive(false);               // 目前播放的動畫 "總"時間 = 動畫撥放完畢時
        }

        if (StartObject.activeSelf)
        {
            Animator startAnims = StartObject.GetComponent("Animator") as Animator;
            AnimatorStateInfo startState = startAnims.GetCurrentAnimatorStateInfo(0);             // 取得目前動畫狀態 (0) = Layer1

            if (startState.nameHash == Animator.StringToHash("Layer1.Start"))                  // 如果 目前 動化狀態 是 Start
                if (startState.normalizedTime > 3.0f) StartObject.SetActive(false);               // 目前播放的動畫 "總"時間 = 動畫撥放完畢時
        }

        if (MissionObject.activeSelf)
        {
            Animator missionAnims = MissionObject.GetComponent("Animator") as Animator;
            AnimatorStateInfo missionState = missionAnims.GetCurrentAnimatorStateInfo(0);          // 取得目前動畫狀態 (0) = Layer1

            if (missionState.nameHash == Animator.StringToHash("Layer1.FadeIn"))                   // 如果 目前 動化狀態 是 FadeIn
                if (missionState.normalizedTime > 2.0f) MissionObject.SetActive(false);               // 目前播放的動畫 "總"時間 = 動畫撥放完畢時
            if (missionState.nameHash == Animator.StringToHash("Layer1.Completed"))                // 如果 目前 動化狀態 是 Completed
                if (missionState.normalizedTime > 2.0f) MissionObject.SetActive(false);               // 目前播放的動畫 "總"時間 = 動畫撥放完畢時
        }

        if (ScorePlusObject.activeSelf)
        {
            Animator scoreAnims = ScorePlusObject.GetComponent("Animator") as Animator;
            AnimatorStateInfo scoreState = scoreAnims.GetCurrentAnimatorStateInfo(0);             // 取得目前動畫狀態 (0) = Layer1

            if (scoreState.nameHash == Animator.StringToHash("Layer1.ScorePlus"))                  // 如果 目前 動化狀態 是 Waiting
                if (scoreState.normalizedTime > 1.0f) ScorePlusObject.SetActive(false);               // 目前播放的動畫 "總"時間 = 動畫撥放完畢時
        }

        if (OtherPlusObject.activeSelf)
        {
            Animator otherAnims = OtherPlusObject.GetComponent("Animator") as Animator;
            AnimatorStateInfo otherState = otherAnims.GetCurrentAnimatorStateInfo(0);             // 取得目前動畫狀態 (0) = Layer1

            if (otherState.nameHash == Animator.StringToHash("Layer1.ScorePlus"))                  // 如果 目前 動化狀態 是 Waiting
                if (otherState.normalizedTime > 1.0f) OtherPlusObject.SetActive(false);               // 目前播放的動畫 "總"時間 = 動畫撥放完畢時
        }
        #endregion

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


        #region Score Bar動畫
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
        #endregion

        #region Energy Bar動畫

        if (BattleManager.energy == Math.Round(_beautyEnergy, 6))
        {
            EnergyBar.GetComponent<UISlider>().value = (float)(_beautyEnergy = BattleManager.energy);
        }

        if (BattleManager.energy > _beautyEnergy)                           // 如果 舊值>目前值 (我的值比0.5小 分數比別人低)
        {
            EnergyBar.GetComponent<UISlider>().value = (float)_beautyEnergy;           // 先等於目前值，然後慢慢減少
            _beautyEnergy = Mathf.Lerp((float)_beautyEnergy, (float)BattleManager.energy, 0.1f);                                        // 每次執行就減少一些 直到數值相等 (可以造成平滑動畫)
        }
        else if (BattleManager.energy < _beautyEnergy)                      // 如果 舊值>目前值 (我的值比0.5大 分數比別人高)
        {
            EnergyBar.GetComponent<UISlider>().value = (float)_beautyEnergy;           // 先等於目前值，然後慢慢增加
            _beautyEnergy = Mathf.Lerp((float)_beautyEnergy, (float)BattleManager.energy, 0.1f);                                        // 每次執行就增加一些 直到數值相等 (可以造成平滑動畫)
        }
        else
        {
            EnergyBar.GetComponent<UISlider>().value = (float)BattleManager.energy;
        }

        #endregion

        #region EXP動畫

        #endregion

        ComboLabel.GetComponent<UILabel>().text = battleManager.combo.ToString();        // 畫出Combo值

        //        Debug.Log("_beautyEnergy: " + _beautyEnergy);
        //        Debug.Log("battleManager.energy: " + battleManager.energy);
    }


    public void HPBar_Shing()
    {
        Debug.Log("HPBar_Shing !");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value">0~1顯示百分比</param>
    /// <param name="isDead">是否死亡</param>
    public void ShowBossHPBar(float value, bool isDead)
    {
        if (!isDead)
        {
            if (!BossHPBar.activeSelf)
                BossHPBar.SetActive(true);

            BossHPBar.transform.GetChild(0).GetComponent<UISlider>().value = value;
        }
        else
        {
            BossHPBar.SetActive(false);
        }
    }

    public void MissionMsg(Mission mission, float value)
    {
        MissionObject.SetActive(true);
        switch (mission)
        {
            case Mission.Harvest:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "收穫       糧食";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = value.ToString();
                Debug.Log("Mission : Harvest! 收穫:" + value + " 糧食");
                break;
            case Mission.HarvestRate:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "收穫增加       ";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = value.ToString();
                Debug.Log("Mission : HarvestRate UP+! 收穫倍率:" + value);
                break;
            case Mission.Exchange:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "交換收穫的糧食";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = "";
                Debug.Log("Mission : Exchange! 交換收穫的糧食");
                break;
            case Mission.Reduce:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "豐收祭典     糧食";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = "    "+value.ToString();
                Debug.Log("Mission : Reduce! 豐收祭典 花費: " + value + " 糧食");
                break;
            case Mission.DrivingMice:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "驅趕老鼠       隻";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = "    " + value.ToString();
                Debug.Log("Mission : DrivingMice! 驅趕老鼠 數量: " + value + " 隻");
                break;
            case Mission.WorldBoss:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "世界王出現!!";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = "";
                Debug.Log("Mission WARNING 世界王出現!!");
                break;
        }
        MissionObject.transform.GetChild(2).GetComponent<UILabel>().text = "Mission";
        MissionObject.GetComponent<Animator>().Play("FadeIn");
    }

    public void MissionCompletedMsg(Mission mission, float missionReward)
    {
        if (missionReward != 0)     // ScorePlus 動畫
        {
            ScorePlusObject.SetActive(true);

            if (missionReward > 0)
            {
                ScorePlusObject.transform.GetChild(0).GetComponent<UILabel>().text = "+" + missionReward.ToString();
            }
            else if (missionReward < 0)
            {
                ScorePlusObject.transform.GetChild(0).GetComponent<UILabel>().text = missionReward.ToString();
            }
            ScorePlusObject.GetComponent<Animator>().Play("ScorePlus");
        }

        // Mission Completed 動畫
        MissionObject.SetActive(true);
        switch (mission)
        {
            case Mission.Harvest:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "取得       糧食";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = missionReward.ToString();
                Debug.Log("Mission : Completed! 取得: " + missionReward + " 糧食");
                break;
            case Mission.HarvestRate:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "收穫倍率復原";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = "";
                Debug.Log("Mission 收穫倍率復原 = 1");
                break;
            case Mission.Exchange:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "任務結束:不再交換糧食";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = "";
                Debug.Log("Mission 任務結束:不再交換糧食");
                break;
            case Mission.Reduce:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "豐收祭典任務結束";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = "";
                Debug.Log("Mission : Reduce! 豐收祭典 花費: " + missionReward + " 糧食");
                break;
            case Mission.DrivingMice:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "取得       糧食";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = missionReward.ToString();
                Debug.Log("Mission : Completed!  取得: " + missionReward + " 糧食");
                break;
            case Mission.WorldBoss:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "取得       糧食";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = missionReward.ToString();
                Debug.Log("Mission : Completed!  取得: " + missionReward + " 糧食");
                break;
        }
        MissionObject.transform.GetChild(2).GetComponent<UILabel>().text = "Completed!";
        MissionObject.GetComponent<Animator>().Play("Completed");
    }

    public void OtherScoreMsg(float missionReward)
    {
        if (missionReward != 0) // ScorePlus 動畫
        {
            OtherPlusObject.SetActive(true);

            if (missionReward > 0)
            {
                OtherPlusObject.transform.GetChild(0).GetComponent<UILabel>().text = "+" + missionReward.ToString();
            }
            else if (missionReward < 0)
            {
                OtherPlusObject.transform.GetChild(0).GetComponent<UILabel>().text = missionReward.ToString();
            }
            OtherPlusObject.GetComponent<Animator>().Play("ScorePlus");
        }

        Debug.Log("Other MissionCompleted! + " + missionReward);
    }

    /// <summary>
    /// 任務失敗訊息
    /// </summary>
    /// <param name="mission">目前任務</param>
    /// <param name="value">值(不需要填0)</param>
    public void MissionFailedMsg(Mission mission,int value)
    {
        MissionObject.SetActive(true);
        if (mission == Mission.WorldBoss)
        {
            MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "任務失敗       糧食";
            MissionObject.transform.GetChild(1).GetComponent<UILabel>().text ="    "+value.ToString();
        }
        else
        {
            MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "任務失敗";
            MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = "";
        }
        MissionObject.transform.GetChild(2).GetComponent<UILabel>().text = "Mission Failed!";
        MissionObject.GetComponent<Animator>().Play("Completed");
        Debug.Log("Mission Failed!");
    }

    public void ComboMsg(int value)
    {
        if (Global.isGameStart)
        {
            Combo.SetActive(true);
            if (value > 0)
            {
                Combo.transform.GetChild(0).GetComponent<UILabel>().text = value.ToString();
                Combo.transform.GetChild(1).GetComponent<UILabel>().text = "Combo";
                Combo.GetComponent<Animator>().Play("ComboFadeIn", 0, 0);
            }
            else
            {
                Combo.transform.GetChild(0).GetComponent<UILabel>().text = value.ToString();
                Combo.transform.GetChild(1).GetComponent<UILabel>().text = "Break";
                Combo.GetComponent<Animator>().Play("ComboFadeOut");
            }
        }
        else
        {
            Combo.GetComponent<Animator>().Play("ComboFadeOut");
        }
    }

    public void EnergySilder(float value)
    {
        EnergyBar.GetComponent<UISlider>().value = (float)BattleManager.energy;
    }

/// <summary>
/// 遊戲結束
/// </summary>
/// <param name="score">自己的分數</param>
/// <param name="maxScore">遊戲中獲得總分</param>
/// <param name="combo"></param>
/// <param name="kill"></param>
/// <param name="lost"></param>
    public void GoodGameMsg(int score,bool result, int exp, int sliverReward, int combo, int killMice, int lostMice, bool isHighScore, bool isHighCombo)
    {
        int maxExp = 100;
        
        if (result)
        {
            GGObject.transform.GetChild(1).gameObject.SetActive(true);
        }
        else
        {
            GGObject.transform.GetChild(2).gameObject.SetActive(true);
        }

        GGObject.transform.Find("Result").GetChild(0).GetComponent<UILabel>().text = score.ToString();
        GGObject.transform.Find("Result").GetChild(1).GetComponent<UILabel>().text = combo.ToString();
        GGObject.transform.Find("Result").GetChild(2).GetComponent<UILabel>().text = killMice.ToString();
        GGObject.transform.Find("Result").GetChild(3).GetComponent<UILabel>().text = lostMice.ToString();
        GGObject.transform.Find("Result").GetChild(4).GetComponent<UILabel>().text = sliverReward.ToString();

       int _exp = Global.EXP + exp;


       // EXP動畫還沒寫
       if (_exp > 0 && _exp < maxExp)
        {
            GGObject.transform.Find("Result").GetChild(6).GetChild(0).GetComponent<UISlider>().value = (float)(_exp/100);
        }
       else if (_exp > maxExp)
       {
           Debug.Log("LEVEL UP!");
           _exp -= maxExp;
           GGObject.transform.Find("Result").GetChild(6).GetChild(0).GetComponent<UISlider>().value = (float)(_exp / 100);
       }

        GGObject.SetActive(true);
    }

    void OnWaitingPlayer()
    {
        if (!Global.isGameStart)
        {
            WaitObject.transform.gameObject.SetActive(true);
        }
    }

    void OnLoadScene()
    {
        Global.photonService.WaitingPlayerEvent -= OnWaitingPlayer;
        Global.photonService.LoadSceneEvent -= OnLoadScene;
    }
}
