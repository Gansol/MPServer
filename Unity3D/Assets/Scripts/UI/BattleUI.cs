using MPProtocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUI : IMPPanelUI
{
    private BattleSystem m_BattleSystem;
    private AttachBtn_BattleUI UI;

    #region -- Variables 變數 -- #endregion
    private Dictionary<string, object> _dictItemReward;
    private Transform rewardPanel;
    private Color _blueLifeColor;
    private Color _redLifeColor;

    private int _dataLoadedCount;           // 資料載入量
    private float _energy;                            // 能量
    private float _myLife;                            // 生命
    private float _otherLife;                       // 對手生命
    private float _myMaxLife;                   // 最大生命
    private float _feverEnergy;                 // fever能量
    private float _ohterMaxLife;              // 對手最大生命
    private float _tmpMyLifeBar;            // 生命條數值
    private float _tmpOtherLifeBar;       // 對手生命條數值

    private float _beautyLife;                     // 美化生命條數值
    private float _beautyFever;                 // 美化FerverTime條數值
    private float _beautyEnergy;               // 美化能量條數值
    private float _beautyOtherLife;          // 美化對手生命條數值
    private float _beautyOtherEnergy;    // 美化對手能量條數值

    [Range(0.1f, 1.0f)]
    private float _beautyHP;                 // 美化血條用
    private bool _bLoadedPanel;         // 是否載入Panel
    private bool _bLoadedAsset;         // 是否載入資產

    //  private bool bLoadPrefab;
    //private double _energy;
    #endregion


    public BattleUI(MPGame MPGame) : base(MPGame)
    {
        Debug.Log("--------------- BattleUI Create ----------------");
        m_BattleSystem = MPGame.GetBattleSystem();
    }

    public override void Initialize()
    {
        Debug.Log("--------------- BattleUI Initialize ----------------");

        m_AssetLoaderSystem = MPGame.Instance.GetAssetLoaderSystem();
        m_RootUI = GameObject.Find(Global.Scene.BattleAsset.ToString());
        UI = m_RootUI.GetComponentInChildren<AttachBtn_BattleUI>();

        _dataLoadedCount = (int)ENUM_Data.None;
        _beautyHP = 0.5f;
        _beautyEnergy = _beautyFever = 0f;
        UI.playerName.text = Global.Nickname;
        UI.otherPlayerName.text = Global.OpponentData.Nickname;
        UI.avatarImage.spriteName = Global.PlayerImage;
        UI.otherAvatarImage.spriteName = Global.OpponentData.Image;
        rewardPanel = UI.GoodGamePanel.transform.Find("Result").Find("Reward").GetChild(0).GetChild(0).GetChild(0);

        _myMaxLife = m_BattleSystem.GetBattleAttr().life;
        _ohterMaxLife = m_BattleSystem.GetBattleAttr().otherLife;

        _blueLifeColor = UI.BlueLifeBar.GetComponent<UISprite>().color;
        _redLifeColor = UI.RedLifeBar.GetComponent<UISprite>().color;

        UIEventListener.Get(UI.GoodGamePanel.transform.Find("Exit_Btn").gameObject).onClick = OnClosed;
        UIEventListener.Get(UI.exitBtn).onClick = OnClosed;
        Global.photonService.WaitingPlayerEvent += OnWaitingPlayer;
     //   Global.photonService.LoadSceneEvent += OnLoadScene;
        Global.photonService.GameStartEvent += OnGameStart;
      //  Global.photonService.LoadPlayerItemEvent += OnLoadPlayerItem;
      //  Global.photonService.LoadPlayerDataEvent += OnLoadPlayerData;
    }

    public override void Update()
    {
        base.Update();

        //#region // 資料庫資料載入完成時 載入資產
        //if (_dataLoadedCount == GetMustLoadedDataCount() && !_bLoadedPanel)
        //{
        //    OnLoadPanel();
        //}
        //#endregion

        //#region // 載入資產完成後 實體化 物件
        //if (m_AssetLoaderSystem.IsLoadAllAseetCompleted && _bLoadedAsset /*&& _bLoadedEffect*/)
        //{
        //    Debug.Log("UI OK");
        //    m_AssetLoaderSystem.Initialize();
        //    _bLoadedAsset = false;
        //    ResumeToggleTarget();
        //}
        //#endregion

        if (Global.isGameStart)
        {
            #region 動畫類判斷 DisActive
            if (UI.WaitObject.activeSelf)
            {
                Debug.Log("Waiting ....................");
                if (Global.isGameStart)
                    UI.WaitObject.GetComponent<Animator>().Play("Layer1.Wait");

                Animator waitAnims = UI.WaitObject.GetComponent("Animator") as Animator;
                AnimatorStateInfo waitState = waitAnims.GetCurrentAnimatorStateInfo(0);             // 取得目前動畫狀態 (0) = Layer1

                if (waitState.fullPathHash == Animator.StringToHash("Layer1.Wait"))                  // 如果 目前 動化狀態 是 Waiting
                    if (waitState.normalizedTime > .1f)
                    {
                        UI.WaitObject.SetActive(false);               // 目前播放的動畫 "總"時間 = 動畫撥放完畢時
                        UI.StartObject.SetActive(true);
                    }

            }

            if (UI.StartObject.activeSelf && !UI.WaitObject.activeSelf && Global.isGameStart)
            {
                //  Debug.Log("Start 321................");
                Animator startAnims = UI.StartObject.GetComponent<Animator>();
                AnimatorStateInfo startState = startAnims.GetCurrentAnimatorStateInfo(0);             // 取得目前動畫狀態 (0) = Layer1

                if (startState.fullPathHash == Animator.StringToHash("Layer1.Start"))                  // 如果 目前 動化狀態 是 Start
                    if (startState.normalizedTime > .9f) UI.StartObject.SetActive(false);               // 目前播放的動畫 "總"時間 = 動畫撥放完畢時
            }

            if (UI.MissionObject.activeSelf)
            {
                Animator missionAnims = UI.MissionObject.GetComponent<Animator>();
                AnimatorStateInfo missionState = missionAnims.GetCurrentAnimatorStateInfo(0);          // 取得目前動畫狀態 (0) = Layer1

                if (missionState.fullPathHash == Animator.StringToHash("Layer1.FadeIn"))                   // 如果 目前 動化狀態 是 FadeIn
                    if (missionState.normalizedTime > 2.0f)
                    {
                        UI.MissionObject.SetActive(false);               // 目前播放的動畫 "總"時間 = 動畫撥放完畢時
                        Debug.Log(" --------------FadeIn  UI.MissionObject.SetActive(false)------------");

                    }

                if (missionState.fullPathHash == Animator.StringToHash("Layer1.Completed"))                // 如果 目前 動化狀態 是 Completed
                    if (missionState.normalizedTime > 2.0f)
                    {
                        UI.MissionObject.SetActive(false);               // 目前播放的動畫 "總"時間 = 動畫撥放完畢時
                        Debug.Log(" --------------Completed  UI.MissionObject.SetActive(false)------------");
                    }
            }

            if (UI.ScorePlusObject.activeSelf)
            {
                Animator scoreAnims = UI.ScorePlusObject.GetComponent<Animator>();
                AnimatorStateInfo scoreState = scoreAnims.GetCurrentAnimatorStateInfo(0);             // 取得目前動畫狀態 (0) = Layer1

                if (scoreState.fullPathHash == Animator.StringToHash("Layer1.ScorePlus"))                  // 如果 目前 動化狀態 是 Waiting
                    if (scoreState.normalizedTime > 1.0f) UI.ScorePlusObject.SetActive(false);               // 目前播放的動畫 "總"時間 = 動畫撥放完畢時
            }

            if (UI.OtherPlusObject.activeSelf)
            {
                Animator otherAnims = UI.OtherPlusObject.GetComponent<Animator>();
                AnimatorStateInfo otherState = otherAnims.GetCurrentAnimatorStateInfo(0);             // 取得目前動畫狀態 (0) = Layer1

                if (otherState.shortNameHash == Animator.StringToHash("Layer1.ScorePlus"))                  // 如果 目前 動化狀態 是 Waiting
                    if (otherState.normalizedTime > 1.0f) UI.OtherPlusObject.SetActive(false);               // 目前播放的動畫 "總"時間 = 動畫撥放完畢時
            }
            #endregion
        }
    }

    #region -- OnGUI --
    public override void OnGUI()
    {

        if (Global.isGameStart)
        {
            BattleState();
            GUIShowVariables();
            ScoreBarAnim();

            BeautyBar(UI.EnergyBar, _energy, _beautyEnergy);
            BeautyBar(UI.BlueEnergyBar, _energy, _beautyEnergy);
            BeautyBar(UI.RedEnergyBar, m_BattleSystem.GetBattleAttr().otherEnergy / 100f, _beautyOtherEnergy);
            BeautyBar(UI.FeverBar, _feverEnergy, _beautyFever);
            BeautyBar(UI.BlueLifeBar, _myLife, _beautyLife);
            BeautyBar(UI.RedLifeBar, _otherLife, _beautyOtherLife);
        }

    }
    #endregion

    #region  -- BattleState 顯示目前遊戲狀態 --
    // 顯示目前BattleState ICON
    private void BattleState()
    {
        switch (m_BattleSystem.GetBattleState())
        {
            case ENUM_BattleAIState.EasyMode:
                UI.gameMode.spriteName = "Easy Mode";
                break;
            case ENUM_BattleAIState.NormalMode:
                UI.gameMode.spriteName = "NormalMode";
                break;
            case ENUM_BattleAIState.HardMode:
                UI.gameMode.spriteName = "HardMode";
                break;
            case ENUM_BattleAIState.CarzyMode:
                UI.gameMode.spriteName = "CarzyMode";
                break;
            case ENUM_BattleAIState.EndTimeMode:
                UI.gameMode.spriteName = "EndMode";
                break;
        }
    }
    #endregion

    #region -- GUIShowVariables GUI數值顯示 --
    // GUI 顯示數值
    private void GUIShowVariables()
    {
        _myLife = m_BattleSystem.GetBattleAttr().life / _myMaxLife;
        _otherLife = m_BattleSystem.GetBattleAttr().otherLife / _ohterMaxLife;
        _energy = m_BattleSystem.GetBattleAttr().energy / 100f;
        _feverEnergy = m_BattleSystem.GetBattleAttr().feverEnergy / 100f;

        _tmpMyLifeBar = UI.BlueLifeBar.value;
        _tmpOtherLifeBar = UI.RedLifeBar.value;

        UI.GameTime.text = (Math.Max(0, Math.Floor(Global.GameTime - m_BattleSystem.GetBattleAttr().gameTime))).ToString();
        UI.BlueScoreLabel.text = m_BattleSystem.GetBattleAttr().score.ToString();         // 畫出分數值
        UI.RedScoreLabel.text = m_BattleSystem.GetBattleAttr().otherScore.ToString();     // 畫出分數值
        UI.BlueLifeText.text = m_BattleSystem.GetBattleAttr().life.ToString();
        UI.RedLifeText.text = m_BattleSystem.GetBattleAttr().otherLife.ToString();
        UI.ComboLabel.text = m_BattleSystem.GetBattleAttr().combo.ToString();        // 畫出 UI.Combo值

        //if (tmpBlueLifeBar > BlueLifeBar.value)   // 扣血變色 未完成
        //    BarTweenColor(BlueLifeBar, Color.green, _blueLifeColor);
        //else
        //    BarTweenColor(BlueLifeBar, Color.red, _blueLifeColor);
    }
    #endregion

    #region -- ScoreBarAnim 分數條動畫 --
    private void ScoreBarAnim()
    {
        // 得分百分比 兩邊都是0會 NaN
        float scorePrecent = m_BattleSystem.GetBattleAttr().score / (m_BattleSystem.GetBattleAttr().score + m_BattleSystem.GetBattleAttr().otherScore);

        if (_beautyHP == scorePrecent)                                             // 如果HPBar值在中間 (0.5=0.5)
        {
            UI.HPBar.value = scorePrecent;
        }
        else if (_beautyHP > scorePrecent)                                         // 如果 舊值>目前值 (我的值比0.5小 分數比別人低)
        {
            UI.HPBar.value = _beautyHP;                                        // 先等於目前值，然後慢慢減少

            if (_beautyHP >= scorePrecent)
                _beautyHP -= 0.01f;                                         // 每次執行就減少一些 直到數值相等 (可以造成平滑動畫)
        }
        else if (_beautyHP < scorePrecent)                                         // 如果 舊值>目前值 (我的值比0.5大 分數比別人高)
        {
            UI.HPBar.value = _beautyHP;                                        // 先等於目前值，然後慢慢增加

            if (_beautyHP <= scorePrecent)
                _beautyHP += 0.01f;                                         // 每次執行就增加一些 直到數值相等 (可以造成平滑動畫)
        }
        else if (m_BattleSystem.GetBattleAttr().score == 0 && m_BattleSystem.GetBattleAttr().otherScore == 0)
        {
            UI.HPBar.value = _beautyHP;
            if (_beautyHP <= UI.HPBar.value && UI.HPBar.value > 0.5f)
            {
                _beautyHP -= 0.01f;
            }

            if (_beautyHP >= UI.HPBar.value && UI.HPBar.value < 0.5f)
                _beautyHP += 0.01f;
        }
    }
    #endregion

    #region EnergyTextAnim 能量數值(數字) 動畫 ***還沒寫***
    private void EnergyTextAnim()
    {
        UI.energyLabel.text = m_BattleSystem.GetBattleAttr().energy.ToString();
    }
    #endregion

    #region -- BeautyBar 平滑Slider動畫 --
    /// <summary>
    /// 平滑Slider動畫
    /// </summary>
    /// <param name="bar"></param>
    /// <param name="value"></param>
    /// <param name="tmpValue"></param>
    private void BeautyBar(UISlider bar, float value, float tmpValue)
    {
        bar.value = Mathf.Lerp(bar.value, value, 0.1f);

        if (value == Math.Round(bar.value, 6)) bar.value /*= tmpValue */= value;

        //if (value > tmpValue)                           // 如果 舊值>目前值 (我的值比0.5小 分數比別人低)
        //{
        //    bar.value = tmpValue;           // 先等於目前值，然後慢慢減少
        //    tmpValue = Mathf.Lerp(tmpValue, value, 0.1f);                                        // 每次執行就減少一些 直到數值相等 (可以造成平滑動畫)
        //}
        //else if (value < tmpValue)                      // 如果 舊值>目前值 (我的值比0.5大 分數比別人高)
        //{
        //    bar.value = (float)tmpValue;           // 先等於目前值，然後慢慢增加
        //    tmpValue = Mathf.Lerp(tmpValue, value, 0.1f);                                        // 每次執行就增加一些 直到數值相等 (可以造成平滑動畫)
        //}
        //else
        //{
        //    bar.value = value;
        //}
    }
    #endregion

    #region -- ShowBossHPBar 顯示BossHP Bar --
    /// <summary>
    /// 顯示BossHP Bar
    /// </summary>
    /// <param name="value">0~1顯示百分比</param>
    /// <param name="isDead">是否死亡</param>
    public void ShowBossHPBar(float value, bool isDead)
    {
        if (!isDead)
        {
            if (!UI.BossHPBar.gameObject.activeSelf)
                UI.BossHPBar.gameObject.SetActive(true);

            UI.BossHPBar.value = value;
        }
        else
        {
            UI.BossHPBar.gameObject.SetActive(false);
        }
    }
    #endregion

    #region -- MissionMsg 任務訊息 --
    public void MissionMsg(Mission mission, float value)
    {
        UI.MissionObject.SetActive(true);
        switch (mission)
        {
            case Mission.Harvest:
                UI.MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "收穫 " + value.ToString() + " 糧食";
                Debug.Log("Mission : Harvest! 收穫:" + value + " 糧食");
                break;
            case Mission.HarvestRate:
                UI.MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "收穫增加 " + value.ToString();
                Debug.Log("Mission : HarvestRate UP+! 收穫倍率:" + value);
                break;
            case Mission.Exchange:
                UI.MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "交換收穫的糧食";
                Debug.Log("Mission : Exchange! 交換收穫的糧食");
                break;
            case Mission.Reduce:
                UI.MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "老鼠們偷吃了 " + value.ToString() + " 糧食..";
                Debug.Log("Mission : Reduce! 豐收祭典 花費: " + value + " 糧食");
                break;
            case Mission.DrivingMice:
                UI.MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "達成 " + value.ToString() + "  Combo!";
                Debug.Log("Mission : DrivingMice! 驅趕老鼠 數量: " + value + " 隻");
                break;
            case Mission.WorldBoss:
                UI.MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "超 級 老 鼠 出 沒 !!";
                Debug.Log("Mission WARNING 世界王出現!!");
                break;
        }
        UI.MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = "Mission";
        UI.MissionObject.GetComponent<Animator>().Play("Layer1.FadeIn");
    }
    #endregion

    #region -- MissionCompletedMsg 任務完成訊息 --
    public void MissionCompletedMsg(Mission mission, float missionReward)
    {
        if (missionReward != 0)     // ScorePlus 動畫
        {
            UI.ScorePlusObject.SetActive(true);
            UI.ScorePlusObject.transform.GetChild(0).GetComponent<UILabel>().text = missionReward > 0 ? "+" : "" + missionReward.ToString();
            UI.ScorePlusObject.GetComponent<Animator>().Play("Layer1.ScorePlus");
        }

        // Mission Completed 動畫
        UI.MissionObject.SetActive(true);
        switch (mission)
        {
            case Mission.Harvest:
                UI.MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "取得 " + missionReward.ToString() + " 糧食";
                break;
            case Mission.HarvestRate:
                UI.MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "收穫倍率復原";
                break;
            case Mission.Exchange:
                UI.MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "任務結束:不再交換糧食";
                break;
            case Mission.Reduce:
                UI.MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "豐收祭典任務結束";
                break;
            case Mission.DrivingMice:
                UI.MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "取得 " + missionReward.ToString() + " 糧食";
                break;
            case Mission.WorldBoss:
                UI.MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "取得 " + missionReward.ToString() + " 糧食";
                break;
        }
        UI.MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = "Completed!";
        UI.MissionObject.GetComponent<Animator>().Play("Layer1.Completed", -1, 0f);
    }
    #endregion

    #region -- OtherScoreMsg 顯示對手分數 訊息 --
    public void OtherScoreMsg(float missionReward)
    {
        if (missionReward != 0) // ScorePlus 動畫
        {
            UI.OtherPlusObject.SetActive(true);

            if (missionReward > 0)
            {
                UI.OtherPlusObject.transform.GetChild(0).GetComponent<UILabel>().text = "+" + missionReward.ToString();
            }
            else if (missionReward < 0)
            {
                UI.OtherPlusObject.transform.GetChild(0).GetComponent<UILabel>().text = missionReward.ToString();
            }
            UI.OtherPlusObject.GetComponent<Animator>().Play("Layer1.ScorePlus", -1, 0f);
        }

        Debug.Log("Other MissionCompleted! + " + missionReward);
    }
    #endregion

    #region -- MissionFailedMsg 任務失敗訊息 --
    /// <summary>
    /// 任務失敗訊息
    /// </summary>
    /// <param name="mission">目前任務</param>
    /// <param name="value">值(不需要填0)</param>
    public void MissionFailedMsg(Mission mission, int value)
    {
        UI.MissionObject.SetActive(true);
        if (mission == Mission.WorldBoss)
        {
            UI.MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "任務失敗 " + value.ToString() + " 糧食";
        }
        else
        {
            UI.MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "任務失敗";
        }
        UI.MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = "Mission Failed!";
        UI.MissionObject.GetComponent<Animator>().Play("Layer1.Completed", -1, 0f);
        Debug.Log("Mission Failed!");
    }
    #endregion

    #region -- ComboMsg 連擊訊息 --
    public void ComboMsg(int value)
    {
        if (Global.isGameStart)
        {
            UI.Combo.SetActive(true);
            if (value > 0)
            {
                UI.Combo.transform.GetChild(0).GetComponent<UILabel>().text = value.ToString();
                UI.Combo.transform.GetChild(1).GetComponent<UILabel>().text = " Combo";
                UI.Combo.GetComponent<Animator>().Play("Layer1.ComboFadeIn", -1, 0f);
            }
            else
            {
                UI.Combo.transform.GetChild(0).GetComponent<UILabel>().text = value.ToString();
                UI.Combo.transform.GetChild(1).GetComponent<UILabel>().text = "Break";
                UI.Combo.GetComponent<Animator>().Play("Layer1.ComboFadeOut", -1, 0f);
            }
        }
        else
        {
            UI.Combo.GetComponent<Animator>().Play("Layer1.ComboFadeOut", -1, 0f);
        }
    }
    #endregion

    #region -- GoodGameMsg 遊戲結束訊息 --
    /// <summary>
    /// 遊戲結束
    /// </summary>
    /// <param name="score">自己的分數</param>
    /// <param name="maxScore">遊戲中獲得總分</param>
    /// <param name="combo"></param>
    /// <param name="kill"></param>
    /// <param name="lost"></param>
    public void GoodGameMsg(int score, bool result, int exp, int sliverReward, int goldReward, string jItemReward, int combo, int killMice, int lostMice, string evaluate, bool isHighScore, bool isHighCombo)
    {
        float maxExp = Clac.ClacExp(Global.Rank + 1);
        float sumExp = Global.Exp + exp;
        float value;
        _dictItemReward = new Dictionary<string, object>(MiniJSON.Json.Deserialize(jItemReward) as Dictionary<string, object>);

        // 實體化 對戰獎勵
        InstantiateRewardsBG(_dictItemReward, Global.InvItemAssetName, rewardPanel, new Vector2(400, 0));
        LoadItemICON();
        InstantiateItemReward();

        // EXP動畫還沒寫
        if (sumExp > maxExp)
        {
            Debug.Log("LEVEL UP!");
            sumExp -= maxExp;
        }
        value = sumExp;
        value /= maxExp;
        Debug.Log("Exp Percent:" + value + " _exp:" + sumExp +" Result: "+result);

        // 顯示對戰結束 資訊
        UI.GoodGamePanel.SetActive(true);
        UI.GoodGamePanel.transform.Find(result == true ? "Win" : "Lose").gameObject.SetActive(true);
        UI.GoodGamePanel.transform.Find("Result").Find("Score").GetComponent<UILabel>().text = score.ToString();
        UI.GoodGamePanel.transform.Find("Result").Find("Combo").GetComponent<UILabel>().text = combo.ToString();
        UI.GoodGamePanel.transform.Find("Result").Find("Kill").GetComponent<UILabel>().text = killMice.ToString();
        UI.GoodGamePanel.transform.Find("Result").Find("Lost").GetComponent<UILabel>().text = lostMice.ToString();
        UI.GoodGamePanel.transform.Find("Result").Find("Rice").GetComponent<UILabel>().text = sliverReward.ToString();
        UI.GoodGamePanel.transform.Find("Result").Find("Evaluate").GetComponent<UILabel>().text = evaluate.ToString();
        UI.GoodGamePanel.transform.Find("Result").Find("Rank").GetChild(0).GetComponent<UISlider>().value = value;
        UI.GoodGamePanel.transform.Find("Result").Find("Rank").GetChild(0).GetComponent<UISlider>().enabled = false;
        UI.GoodGamePanel.transform.Find("Result").Find("Rank").GetChild(0).GetComponent<UISlider>().enabled = true;
    }
    #endregion

    #region -- LoadItemICON 載入道具圖示 --
    private bool LoadItemICON()
    {
        if (_dictItemReward.Count != 0)
        {
            foreach (KeyValuePair<string, object> item in _dictItemReward)
            {
                string itemName = Convert.ToString(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.miceProperty, "ItemName", item.Key));
                if (m_AssetLoaderSystem.GetAsset(Global.IconSuffix + itemName) == null)
                    m_AssetLoaderSystem.LoadAssetFormManifest(Global.MiceIconUniquePath + Global.IconSuffix + itemName + Global.ext);
            }
            m_AssetLoaderSystem.SetLoadAllAseetCompleted();
            return false;
        }
        return true;    // 已載入 不須再載入
    }
    #endregion

    #region -- InstantiateRewardsBG 實體化背包物件背景--
    /// <summary>
    /// 實體化背包物件背景
    /// </summary>
    /// <param name="itemData">道具資料</param>
    /// <param name="bgAssetName">道具名稱</param>
    /// <param name="parent"></param>
    /// <param name="offset">物件間格</param>
    private void InstantiateRewardsBG(Dictionary<string, object> itemData, string bgAssetName, Transform parent, Vector2 offset)
    {
        Vector2 pos = new Vector2();
        //      Dictionary<string, GameObject> dictItem = new Dictionary<string, GameObject>();

        if (m_AssetLoaderSystem.GetAsset(bgAssetName) != null)                  // 已載入資產時
        {
        foreach (KeyValuePair<string, object> item in itemData)
            {
                GameObject go = MPGFactory.GetObjFactory().Instantiate(m_AssetLoaderSystem.GetAsset(bgAssetName), parent, item.Key, pos, Vector3.one, Vector2.zero, -1);
                //     dictItem.Add(item.Key, go);    // 存入道具資料索引
                pos.x += offset.x;
            }
        }
        // return dictItem;
    }
    #endregion

    #region -- InstantiateItemReward 顯示道具獎勵 --
    private void InstantiateItemReward()
    {
        // todo show itemReward goldReward
        if (_dictItemReward.Count != 0)
        {
            int i = 0;

            // dictItemReward {"10001":{"ItemCount":"0"}}
            foreach (KeyValuePair<string, object> item in _dictItemReward)
            {
                Dictionary<string, object> itemData = item.Value as Dictionary<string, object>;   // ItemCount:1

                string itemCount = Convert.ToString(itemData[PlayerItem.ItemCount.ToString()]);
                string itemName = Convert.ToString(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.miceProperty, "ItemName", item.Key));
                GameObject bundle = m_AssetLoaderSystem.GetAsset(Global.IconSuffix + itemName);

                if (bundle != null)
                    bundle = MPGFactory.GetObjFactory().Instantiate(bundle, rewardPanel.GetChild(i).Find("Image"), itemName, Vector3.zero, Vector3.one, new Vector2(100, 100), 310);

                rewardPanel.GetChild(i).Find("text").GetComponent<UILabel>().text = itemCount;
                i++;
            }
        }
    }
    #endregion

    #region -- BarTweenColor Sprite底色變換 -- 
    /// <summary>
    /// Sprite底色變換
    /// </summary>
    /// <param name="bar">Sprite</param>
    /// <param name="defaultColor">目前的顏色</param>
    /// <param name="toColor">變換的顏色</param>
    private void BarTweenColor(UISlider bar, Color defaultColor, Color toColor)
    {
        Color color = bar.GetComponent<UISprite>().color;
        // 0.137255   // green -35
        // 0.6471     // blue -165
        if (toColor == Color.red)
            bar.GetComponent<UISprite>().color = new Color(Mathf.Max(color.r + ((1 - defaultColor.r) * (1 - bar.value)), 1), Mathf.Min(color.g - (defaultColor.g * (1 - bar.value)), 0), Mathf.Min(color.b - ((defaultColor.b) * (1 - bar.value)), 0));
        if (toColor == Color.green)
            bar.GetComponent<UISprite>().color = new Color(Mathf.Min(color.r - (defaultColor.r * (1 - bar.value)), 0), Mathf.Max(color.g + ((1 - defaultColor.g) * (1 - bar.value)), 1), Mathf.Min(color.b - ((defaultColor.b) * (1 - bar.value)), 0));
        //  color = new Color(1 - bar.value, color.g - color.g * (1 - bar.value), Math.Max(color.b - color.b * (1 - bar.value), 0));
    }
    #endregion

    #region -- HPBar_Shing HP減少閃爍動畫 (沒寫) -- 
    /// <summary>
    /// HPBar 受傷 閃爍
    /// </summary>
    public void HPBar_Shing()
    {
        Debug.Log("FUCK HPBar_Shing !");
    }
    #endregion

    #region -- OnWaitingPlayer 收到等待玩家開始 -- 
    void OnWaitingPlayer()
    {
        // 顯示等待文字
        if (!Global.isGameStart)
            UI.WaitObject.transform.gameObject.SetActive(true);
    }
    #endregion

    #region -- OnLoadData 載入資料區 -- 
    protected override void OnLoading()
    {
        throw new NotImplementedException();
    }

    //private void OnLoadScene()
    //{
    //    Debug.Log("OnLoadScene NotImplementedException");
    //}

    protected override void OnLoadPanel()
    {
        Debug.Log("OnLoadPanel NotImplementedException");
    }

    //void OnLoadPlayerData()
    //{
    //    Debug.Log("OnLoadPlayerData UI");
    //    _dataLoadedCount *= (int)ENUM_Data.PlayerData;
    //}
    //void OnLoadPlayerItem()
    //{
    //    Debug.Log("OnLoadPlayerItem UI");
    //    _dataLoadedCount *= (int)ENUM_Data.PlayerItem;
    //}

    protected override int GetMustLoadedDataCount()
    {
        Debug.Log("GetMustLoadedDataCount NotImplementedException");
        return 0;
    }
    #endregion


    #region -- OnGameStart 收到遊戲開始 -- 
    void OnGameStart()
    {
        // 關閉等待文字 顯示開始文字
        Global.isGameStart = true;
        UI.WaitObject.transform.gameObject.SetActive(false);
        UI.StartObject.SetActive(true);
        Debug.Log(" ----  Game Start!  ---- ");
    }
    #endregion

    #region -- OnExit 按下離開按扭時 -- 
    public override void OnClosed(GameObject go)
    {
        m_BattleSystem.Release();
        MPGame.Instance.GetCreatureSystem().Release();
        MPGame.Instance.GetPoolSystem().Release();
        MPGame.Instance.GetBattleUI().Release();
        MPGame.Instance.GetMissionSystem().Release();
        // 剔除對手 並離開房間
        Global.photonService.KickOther();
        Global.photonService.ExitRoom();
    }
    #endregion

    #region -- GetMustLoadAsset 載入必要資料  -- 
    protected override void GetMustLoadAsset()
    {
       
    }
    #endregion

    #region -- Release  -- 
    public override void Release()
    {
        Global.photonService.WaitingPlayerEvent -= OnWaitingPlayer;
        //Global.photonService.LoadSceneEvent -= OnLoadScene;
        Global.photonService.GameStartEvent -= OnGameStart;
       // Global.photonService.LoadPlayerItemEvent -= OnLoadPlayerItem;
      //  Global.photonService.LoadPlayerDataEvent -= OnLoadPlayerData;
    }
    #endregion
}
