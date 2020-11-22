﻿using MPProtocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUI : IMPPanelUI
{
    private AttachBtn_BattleUI UI;
    private BattleManager battleManager;
    private ObjectFactory objFactory;
    private Transform rewardPanel;
    private Dictionary<string, object> _dictItemReward;
    private Color _blueLifeColor, _redLifeColor;
    private float _beautyEnergy, _beautyOtherEnergy, _beautyFever, _beautyLife, _beautyOtherLife, energy, feverEnergy, blueLife, redLife, tmpBlueLifeBar, tmpRedLifeBar;
    private float _tmpLife, _tmpOhterLife;
    [Range(0.1f, 1.0f)]
    private float _beautyHP;                // 美化血條用
 //  private bool bLoadPrefab;
//private double _energy;

    public BattleUI(MPGame MPGame) : base(MPGame)
    {

    }

    public override void Initinal()
    {
        m_RootUI = GameObject.Find(Global.Scene.BattleAsset.ToString());
        _beautyHP = 0.5f;
        UI.WaitObject.transform.gameObject.SetActive(true);
        battleManager = m_RootUI.GetComponentInChildren<BattleManager>();
        assetLoader = MPGame.Instance.GetAssetLoader();
        objFactory = new ObjectFactory();

        assetLoader.LoadAssetFormManifest(Global.PanelUniquePath + Global.InvItemAssetName + Global.ext);

        //_energy = 0d;
        _beautyEnergy = _beautyFever = 0f;
        UI.playerName.text = Global.Nickname;
        UI.otherPlayerName.text = Global.OpponentData.Nickname;
        rewardPanel = UI.GGObject.transform.Find("Result").Find("Reward").GetChild(0).GetChild(0).GetChild(0);

        UI.avatarImage.spriteName = Global.PlayerImage;
        UI.otherAvatarImage.spriteName = Global.OpponentData.Image;

        _tmpLife = battleManager.life;
        _tmpOhterLife = battleManager.OtherLife;

        _blueLifeColor = UI.BlueLifeBar.GetComponent<UISprite>().color;
        _redLifeColor = UI.RedLifeBar.GetComponent<UISprite>().color;

        Global.photonService.WaitingPlayerEvent += OnWaitingPlayer;
        Global.photonService.LoadSceneEvent += OnLoadScene;
    }

    public override void Update()
    {
        base.Update();

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
            Debug.Log("Start 321................");
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
    public override void OnGUI()
    {
        BattleState();
        GUIVariables();
        ScoreBarAnim();

        BeautyBar(UI.EnergyBar, energy, _beautyEnergy);
        BeautyBar(UI.BlueEnergyBar, energy, _beautyEnergy);
        BeautyBar(UI.RedEnergyBar, battleManager.otherEnergy / 100f, _beautyOtherEnergy);
        BeautyBar(UI.FeverBar, feverEnergy, _beautyFever);
        BeautyBar(UI.BlueLifeBar, blueLife, _beautyLife);
        BeautyBar(UI.RedLifeBar, redLife, _beautyOtherLife);
    }

    #region  BattleState 顯示目前遊戲狀態
    // 顯示目前BattleState ICON
    private void BattleState()
    {
        switch (battleManager.GetBattleState())
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

    #region GUIVariables GUI數值顯示
    // GUI 顯示數值
    private void GUIVariables()
    {
        energy = BattleManager.Energy / 100f;
        feverEnergy = battleManager.feverEnergy / 100f;
        blueLife = battleManager.life / _tmpLife;
        redLife = battleManager.OtherLife / _tmpOhterLife;
        tmpBlueLifeBar = UI.BlueLifeBar.value;
        tmpRedLifeBar = UI.RedLifeBar.value;
        UI.GameTime.text = (Math.Max(0, Math.Floor(Global.GameTime - BattleManager.gameTime))).ToString();
        UI.BlueScoreLabel.text = battleManager.score.ToString();         // 畫出分數值
        UI.RedScoreLabel.text = battleManager.otherScore.ToString();     // 畫出分數值
        UI.BlueLifeText.text = battleManager.life.ToString();
        UI.RedLifeText.text = battleManager.OtherLife.ToString();
        UI.ComboLabel.text = battleManager.combo.ToString();        // 畫出 UI.Combo值

        //if (tmpBlueLifeBar > BlueLifeBar.value)   // 扣血變色 未完成
        //    BarTweenColor(BlueLifeBar, Color.green, _blueLifeColor);
        //else
        //    BarTweenColor(BlueLifeBar, Color.red, _blueLifeColor);
    }
    #endregion

    #region ScoreBarAnim 分數條動畫
    private void ScoreBarAnim()
    {
        float value = battleManager.score / (battleManager.score + battleManager.otherScore);                      // 得分百分比 兩邊都是0會 NaN

        if (_beautyHP == value)                                             // 如果HPBar值在中間 (0.5=0.5)
        {
            UI.HPBar.value = value;
        }
        else if (_beautyHP > value)                                         // 如果 舊值>目前值 (我的值比0.5小 分數比別人低)
        {
            UI.HPBar.value = _beautyHP;                                        // 先等於目前值，然後慢慢減少

            if (_beautyHP >= value)
                _beautyHP -= 0.01f;                                         // 每次執行就減少一些 直到數值相等 (可以造成平滑動畫)
        }
        else if (_beautyHP < value)                                         // 如果 舊值>目前值 (我的值比0.5大 分數比別人高)
        {
            UI.HPBar.value = _beautyHP;                                        // 先等於目前值，然後慢慢增加

            if (_beautyHP <= value)
                _beautyHP += 0.01f;                                         // 每次執行就增加一些 直到數值相等 (可以造成平滑動畫)
        }
        else if (battleManager.score == 0 && battleManager.otherScore == 0)
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
        UI.energyLabel.text = battleManager.GetEnergy().ToString();
    }
    #endregion

    #region BeautyBar 平滑Slider動畫
    /// <summary>
    /// 平滑Slider動畫
    /// </summary>
    /// <param name="bar"></param>
    /// <param name="value"></param>
    /// <param name="tmpValue"></param>
    private void BeautyBar(UISlider bar, float value, float tmpValue)
    {

        bar.value = Mathf.Lerp(bar.value, value, 0.1f);

        if (value == Math.Round(bar.value, 6)) bar.value = tmpValue = value;

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

    #region ShowBossHPBar 顯示BossHP Bar
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

    #region MissionMsg 任務訊息
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
                UI.MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "達成 " + value.ToString() + "  UI.Combo!";
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

    #region MissionCompletedMsg 任務完成訊息
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

    #region OtherScoreMsg 顯示對手分數 訊息
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

    #region MissionFailedMsg 任務失敗訊息
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

    #region ComboMsg 連擊訊息
    public void ComboMsg(int value)
    {
        if (Global.isGameStart)
        {
            UI.Combo.SetActive(true);
            if (value > 0)
            {
                UI.Combo.transform.GetChild(0).GetComponent<UILabel>().text = value.ToString();
                UI.Combo.transform.GetChild(1).GetComponent<UILabel>().text = " UI.Combo";
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

    #region GoodGameMsg 遊戲結束訊息
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
        float _exp = Global.Exp + exp;
        float value;
        _dictItemReward = new Dictionary<string, object>(MiniJSON.Json.Deserialize(jItemReward) as Dictionary<string, object>);

        // 實體化 對戰獎勵
        InstantiateRewardsBG(_dictItemReward, Global.InvItemAssetName, rewardPanel, new Vector2(400, 0));
        LoadItemICON();
        InstantiateItemReward();

        // EXP動畫還沒寫
        if (_exp > maxExp)
        {
            Debug.Log("LEVEL UP!");
            _exp -= maxExp;
        }
        value = _exp;
        value /= maxExp;
        Debug.Log("Exp Percent:" + value + " _exp:" + _exp);

        // 顯示對戰結束 資訊
        UI.GGObject.transform.Find(result == true ? "Win" : "Lose").gameObject.SetActive(true);
        UI.GGObject.transform.Find("Result").Find("Score").GetComponent<UILabel>().text = score.ToString();
        UI.GGObject.transform.Find("Result").Find(" UI.Combo").GetComponent<UILabel>().text = combo.ToString();
        UI.GGObject.transform.Find("Result").Find("Kill").GetComponent<UILabel>().text = killMice.ToString();
        UI.GGObject.transform.Find("Result").Find("Lost").GetComponent<UILabel>().text = lostMice.ToString();
        UI.GGObject.transform.Find("Result").Find("Rice").GetComponent<UILabel>().text = sliverReward.ToString();
        UI.GGObject.transform.Find("Result").Find("Evaluate").GetComponent<UILabel>().text = evaluate.ToString();
        UI.GGObject.transform.Find("Result").Find("Rank").GetChild(0).GetComponent<UISlider>().value = value;
        UI.GGObject.SetActive(true);
    }
    #endregion

    #region LoadItemICON 載入道具圖示
    private bool LoadItemICON()
    {
        if (_dictItemReward.Count != 0)
        {
            foreach (KeyValuePair<string, object> item in _dictItemReward)
            {
                string itemName = Convert.ToString(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.miceProperty, "ItemName", item.Key));
                if (assetLoader.GetAsset(Global.IconSuffix + itemName) == null)
                    assetLoader.LoadAssetFormManifest(Global.MiceIconUniquePath + Global.IconSuffix + itemName + Global.ext);
            }
            return false;
        }
        return true;    // 已載入 不須再載入
    }
    #endregion

    #region -- InstantiateRewardsBG 實體化背包物件背景--

    private Dictionary<string, GameObject> InstantiateRewardsBG(Dictionary<string, object> itemData, string itemName, Transform parent, Vector2 offset)
    {
        Vector2 pos = new Vector2();
        Dictionary<string, GameObject> dictItem = new Dictionary<string, GameObject>();

        foreach (KeyValuePair<string, object> item in itemData)
        {
            if (assetLoader.GetAsset(itemName) != null)                  // 已載入資產時
            {
                GameObject obj = objFactory.Instantiate(assetLoader.GetAsset(itemName), parent, item.Key, new Vector3(pos.x, pos.y), Vector3.one, Vector2.zero, -1);
                dictItem.Add(item.Key, obj);    // 存入道具資料索引
                pos.x += offset.x;
            }
        }
        return dictItem;
    }
    #endregion

    #region InstantiateItemReward 顯示道具獎勵
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
                GameObject bundle = assetLoader.GetAsset(Global.IconSuffix + itemName);

                if (bundle != null)
                    bundle = objFactory.Instantiate(bundle, rewardPanel.GetChild(i).Find("Image"), itemName, Vector3.zero, Vector3.one, new Vector2(100, 100), 310);

                rewardPanel.GetChild(i).Find("text").GetComponent<UILabel>().text = itemCount;
                i++;
            }
        }
        //    bLoadPrefab = true;
    }
    #endregion

    #region -- SortItemPos 排序道具位置  --
    /// <summary>
    /// 排序道具位置
    /// </summary>
    /// <param name="xCount">第一頁最大數量</param>
    /// <param name="yCount">每行道具數量</param>
    /// <param name="offset">目前物件位置</param>
    /// <param name="pos">初始位置</param>
    /// <param name="counter">計數</param>
    /// <returns>物件位置</returns>
    public Vector2 SortItemPos(int xCount, int yCount, Vector2 offset, Vector2 pos, int counter)
    {
        // 物件位置排序
        if (counter % xCount == 0 && counter != 0) // 3 % 9 =0
        {
            pos.x = offset.x * 3;
            pos.y = 0;
        }
        else if (counter % yCount == 0 && counter != 0)//3 3 =0
        {
            pos.y += offset.y;
            pos.x = 0;
        }
        return pos;
    }
    #endregion

    private void BarTweenColor(UISlider bar, Color toColor, Color defaultColor)
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

    /// <summary>
    /// HPBar 受傷 閃爍
    /// </summary>
    public void HPBar_Shing()
    {
        Debug.Log("FUCK HPBar_Shing !");
    }

    void OnWaitingPlayer()
    {
        if (!Global.isGameStart)
            UI.WaitObject.transform.gameObject.SetActive(true);
    }

    void OnLoadScene()
    {

    }



    public override void OnClosed(GameObject obj)
    {
        throw new System.NotImplementedException();
    }



    protected override void GetMustLoadAsset()
    {
        throw new System.NotImplementedException();
    }

    protected override int GetMustLoadedDataCount()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnLoading()
    {
        UI = m_RootUI.GetComponentInChildren<AttachBtn_BattleUI>();
    }

    protected override void OnLoadPanel()
    {
        throw new System.NotImplementedException();
    }
    public override void Release()
    {
        Global.photonService.WaitingPlayerEvent -= OnWaitingPlayer;
        Global.photonService.LoadSceneEvent -= OnLoadScene;
    }
}