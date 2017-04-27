using UnityEngine;
using System.Collections;
using MPProtocol;
using System;
using System.Collections.Generic;

public class BattleHUD : MonoBehaviour
{
    public UILabel myName;
    public UILabel otherName;
    public UISprite myImage,otherImage;
    public GameObject HPBar;
    public GameObject ComboLabel;
    public GameObject BlueScore;
    public GameObject RedScore;
    public GameObject BlueEnergyBar;
    public GameObject RedEnergyBar;
    public GameObject EnergyBar;
    public GameObject Combo;
    public GameObject MissionObject;
    public GameObject WaitObject;
    public GameObject StartObject;
    public GameObject ScorePlusObject;
    public GameObject OtherPlusObject;
    public GameObject GGObject;
    public GameObject BossHPBar;
    public UILabel GameTime;
    public GameObject[] StateICON;

    [Range(0.1f, 1.0f)]
    public float _beautyHP;                // 美化血條用

    private double _beautyEnergy;
    //private double _energy;
    private BattleManager battleManager;
    private AssetLoader assetLoader;
    private bool bLoadPrefab;
    private Dictionary<string, object> _dictItemReward;
    Transform rewardPanel;
    ObjectFactory objFactory;
    void Start()
    {
        battleManager = GetComponent<BattleManager>();
        assetLoader = GetComponent<AssetLoader>();
        objFactory = new ObjectFactory();
        Global.photonService.WaitingPlayerEvent += OnWaitingPlayer;
        Global.photonService.LoadSceneEvent += OnLoadScene;

        assetLoader.LoadPrefab("Panel/", "InvItem");
        _beautyEnergy = 0d;
        //_energy = 0d;
        bLoadPrefab = true;
        myName.text = Global.Nickname;
        otherName.text = Global.OtherData.Nickname;
        rewardPanel = GGObject.transform.Find("Result").Find("Reward").GetChild(0).GetChild(0).GetChild(0);

        myImage.spriteName = Global.PlayerImage;
        otherImage.spriteName = Global.OtherData.Image;
    }

    void Update()
    {
        if (assetLoader.loadedObj && !bLoadPrefab)
        {
            bLoadPrefab = true;
            ShowItemReward();
            assetLoader.init();
        }

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

        //if (StartObject.activeSelf)
        //{
        //    Animator startAnims = StartObject.GetComponent("Animator") as Animator;
        //    AnimatorStateInfo startState = startAnims.GetCurrentAnimatorStateInfo(0);             // 取得目前動畫狀態 (0) = Layer1

        //    if (startState.nameHash == Animator.StringToHash("Layer1.Start"))                  // 如果 目前 動化狀態 是 Start
        //        if (startState.normalizedTime > 1) StartObject.SetActive(false);               // 目前播放的動畫 "總"時間 = 動畫撥放完畢時
        //}

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
    }

    private void ShowItemReward()
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
                string itemName = Convert.ToString(ObjectFactory.GetColumnsDataFromID(Global.miceProperty, "ItemName", item.Key));
                GameObject bundle = assetLoader.GetAsset(itemName + "ICON");
                Debug.Log("Bundle:" + bundle);
                if (bundle != null)
                    bundle = objFactory.Instantiate(bundle, rewardPanel.GetChild(i).FindChild("Image"), itemName, Vector3.zero, Vector3.one, new Vector2(100, 100), 310);


                rewardPanel.GetChild(i).FindChild("text").GetComponent<UILabel>().text = itemCount;
                i++;
            }
        }
        bLoadPrefab = true;
    }


    void OnGUI()
    {
        GameTime.text = (Math.Max(0,Math.Floor(Global.GameTime - BattleManager.gameTime))).ToString();
        BlueScore.GetComponent<UILabel>().text = battleManager.score.ToString();         // 畫出分數值
        RedScore.GetComponent<UILabel>().text = battleManager.otherScore.ToString();     // 畫出分數值

        #region -- HUD Energy --
        BlueEnergyBar.GetComponent<UISlider>().value = battleManager.Energy;
        RedEnergyBar.GetComponent<UISlider>().value = battleManager.otherEnergy;
        #endregion

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

            BossHPBar.GetComponentInChildren<UISlider>().value = value;
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
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "收穫 "+value.ToString()+" 糧食";
                Debug.Log("Mission : Harvest! 收穫:" + value + " 糧食");
                break;
            case Mission.HarvestRate:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "收穫增加 "+value.ToString();
                Debug.Log("Mission : HarvestRate UP+! 收穫倍率:" + value);
                break;
            case Mission.Exchange:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "交換收穫的糧食";
                Debug.Log("Mission : Exchange! 交換收穫的糧食");
                break;
            case Mission.Reduce:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "老鼠們偷吃了 " + value.ToString() + " 糧食..";
                Debug.Log("Mission : Reduce! 豐收祭典 花費: " + value + " 糧食");
                break;
            case Mission.DrivingMice:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "達成 " + value.ToString() + " Combo!";
                Debug.Log("Mission : DrivingMice! 驅趕老鼠 數量: " + value + " 隻");
                break;
            case Mission.WorldBoss:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "超 級 老 鼠 出 沒 !!";
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
                //                Debug.Log("Mission : Completed! 取得: " + missionReward + " 糧食");
                break;
            case Mission.HarvestRate:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "收穫倍率復原";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = "";
                //                Debug.Log("Mission 收穫倍率復原 = 1");
                break;
            case Mission.Exchange:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "任務結束:不再交換糧食";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = "";
                //                Debug.Log("Mission 任務結束:不再交換糧食");
                break;
            case Mission.Reduce:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "豐收祭典任務結束";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = "";
                //               Debug.Log("Mission : Reduce! 豐收祭典 花費: " + missionReward + " 糧食");
                break;
            case Mission.DrivingMice:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "取得       糧食";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = missionReward.ToString();
                //                Debug.Log("Mission : Completed!  取得: " + missionReward + " 糧食");
                break;
            case Mission.WorldBoss:
                MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "取得       糧食";
                MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = missionReward.ToString();
                //                Debug.Log("Mission : Completed!  取得: " + missionReward + " 糧食");
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
    public void MissionFailedMsg(Mission mission, int value)
    {
        MissionObject.SetActive(true);
        if (mission == Mission.WorldBoss)
        {
            MissionObject.transform.GetChild(0).GetComponent<UILabel>().text = "任務失敗       糧食";
            MissionObject.transform.GetChild(1).GetComponent<UILabel>().text = "    " + value.ToString();
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
    public void GoodGameMsg(int score, bool result, int exp, int sliverReward, int goldReward, string jItemReward, int combo, int killMice, int lostMice, string evaluate, bool isHighScore, bool isHighCombo)
    {
        ClacExp clacExp = new ClacExp(Global.Rank + 1);
        _dictItemReward = new Dictionary<string, object>(MiniJSON.Json.Deserialize(jItemReward) as Dictionary<string, object>);


        int maxExp = clacExp.Exp;
        int _exp = Global.Exp + exp;



        // todo show itemReward goldReward
        InstantiateItem(_dictItemReward, "InvItem", rewardPanel, new Vector2(400, 0));
        bLoadPrefab = LoadItemICON();

        if (result)
        {
            GGObject.transform.GetChild(1).gameObject.SetActive(true);
        }
        else
        {
            GGObject.transform.GetChild(2).gameObject.SetActive(true);
        }

        GGObject.transform.Find("Result").Find("Score").GetComponent<UILabel>().text = score.ToString();
        GGObject.transform.Find("Result").Find("Combo").GetComponent<UILabel>().text = combo.ToString();
        GGObject.transform.Find("Result").Find("Kill").GetComponent<UILabel>().text = killMice.ToString();
        GGObject.transform.Find("Result").Find("Lost").GetComponent<UILabel>().text = lostMice.ToString();
        GGObject.transform.Find("Result").Find("Rice").GetComponent<UILabel>().text = sliverReward.ToString();
        GGObject.transform.Find("Result").Find("Evaluate").GetComponent<UILabel>().text = evaluate.ToString();




        // EXP動畫還沒寫
        if (_exp > maxExp)
        {
            Debug.Log("LEVEL UP!");
            _exp -= maxExp;
        }
        float value = _exp;
        value = value / 100f;
        GGObject.transform.Find("Result").Find("Rank").GetChild(0).GetComponent<UISlider>().value = value;
        GGObject.SetActive(true);
    }

    private bool LoadItemICON()
    {
        if (_dictItemReward.Count != 0)
        {
            assetLoader.LoadAsset("MiceICON/", "MiceICON");
            // dictItemReward {"10001":{"ItemCount":"0"}}
            foreach (KeyValuePair<string, object> item in _dictItemReward)
            {
                string itemName = Convert.ToString(ObjectFactory.GetColumnsDataFromID(Global.miceProperty, "ItemName", item.Key));
                if (assetLoader.GetAsset(itemName + "ICON") == null)
                    assetLoader.LoadPrefab("MiceICON/", itemName + "ICON");
            }
            return false;
        }
        return true;    // 已載入 不須再載入
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


    #region -- InstantiateItem 實體化背包物件背景--

    private Dictionary<string, GameObject> InstantiateItem(Dictionary<string, object> itemData, string itemName, Transform parent, Vector2 offset)
    {
        Vector2 pos = new Vector2();
        Dictionary<string, GameObject> dictItem = new Dictionary<string, GameObject>();
        int count = parent.childCount, i = 0;

        foreach (KeyValuePair<string, object> item in itemData)
        {
            // id to name 
            if (assetLoader.GetAsset(itemName) != null)                  // 已載入資產時
            {
                GameObject bundle = assetLoader.GetAsset(itemName);

                bundle = objFactory.Instantiate(bundle, parent, item.Key, new Vector3(pos.x, pos.y), Vector3.one, Vector2.zero, -1);
                if (bundle != null) dictItem.Add(item.Key, bundle);    // 存入道具資料索引
                pos.x += offset.x;
            }
            i++;
        }

        return dictItem;
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
    public Vector2 sortItemPos(int xCount, int yCount, Vector2 offset, Vector2 pos, int counter)
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
}
