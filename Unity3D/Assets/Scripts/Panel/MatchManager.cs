using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using System.Linq;
using MPProtocol;
using System;
/* ***************************************************************
 * -----Copyright © 2015 Gansol Studio.  All Rights Reserved.-----
 * -----------            CC BY-NC-SA 4.0            -------------
 * -----------  @Website:  EasyUnity@blogspot.com    -------------
 * -----------  @Email:    GansolTW@gmail.com        -------------
 * -----------  @Author:   Krola.                    -------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * 負責 開啟/關閉/載入 Team的的所有處理
 * NGUI BUG : Team交換時Tween會卡色
 * + pageVaule 還沒加入翻頁值
 * ***************************************************************
 *                           ChangeLog
 * 20171213 v1.1.1   交換按鈕功能合併修改至SwitchBtnComponent                        
 * 20171211 v1.1.0   重製、修正索引問題   
 * 20161102 v1.0.2   3次重構，改變繼承至 PanelManager>MPPanel
 * 20160914 v1.0.1b  2次重購，獨立實體化物件  
 * 20160711 v1.0.1a  1次重構，獨立AssetLoader                       
 * 20160705 v1.0.0   0版完成，載入老鼠部分未來需要修改                    
 * ****************************************************************/

public class MatchManager : MPPanel
{
    SwitchBtnComponent SwitchBtnMethod;

    #region 欄位
    public GameObject[] Panel;
    public GameObject[] infoGroupsArea;                                             // 物件群組位置
    public UILabel matchText, timeText;
    public GameObject ok_btn;

    [Range(0.2f, 0.4f)]
    public float delayBetween2Clicks = 0.3f;                                        // 雙擊間隔時間
    public Vector3 actorScale;                                                      // 角色縮放
    public string iconPath = "MiceICON";                                            // 圖片資料夾位置

    private static Dictionary<string, object> _dictMiceData, _dictTeamData;         // Json老鼠、隊伍資料
    private Dictionary<string, GameObject> _dictLoadedMiceBtnRefs, _dictLoadedTeamBtnRefs;             // 已載入的按鈕(全部、隊伍)
    private GameObject _actorParent, _btnClick, _doubleClickChk;                    // 角色、按下按鈕、雙擊檢查
    private bool _bFirstLoad, _bLoadedAsset, _bLoadedActor, _bLoadedEffect, _bLoadedPlayerData, _bLoadedPlayerItem, _bLoadedPanel, _checkFlag; // 是否 第一次載入 載入圖片、是否載入角色
    private int _miceCost, _page;   // 翻頁值(翻一頁+10)
    private float _time, _lastTime, _escapeTime, _checkTime, _lastClickTime;    // 點擊間距時間
    #endregion

    public MatchManager(MPGame MPGame) : base(MPGame) { }


    //infoGroupsArea[0].transform = mice
    //infoGroupsArea[1].transform = info
    //infoGroupsArea[2].transform = team
    void Awake()
    {
        SwitchBtnMethod = new SwitchBtnComponent();
        _checkTime = 0;

        _dictLoadedMiceBtnRefs = new Dictionary<string, GameObject>();
        _dictLoadedTeamBtnRefs = new Dictionary<string, GameObject>();
        _dictMiceData = new Dictionary<string, object>();
        _dictTeamData = new Dictionary<string, object>();

        //_page = 0;
        _bFirstLoad = true; // dontDestroyOnLoad 所以才使用非靜態
        actorScale = new Vector3(0.8f, 0.8f, 1);
        _actorParent = infoGroupsArea[1].transform.GetChild(0).gameObject;    // 方便程式辨認用 infoGroupsArea[1].transform.GetChild(0).gameObject = image

        UIEventListener.Get(ok_btn).onClick = OnMatchGame;
    }

    void OnEnable()
    {
        _bLoadedPanel = false;
        Global.photonService.LoadPlayerDataEvent += OnLoadPlayerData;
        Global.photonService.LoadPlayerItemEvent += OnLoadPlayerItem;
        Global.photonService.ExitWaitingEvent += OnExitWaiting;
        Global.photonService.UpdateMiceEvent += OnUpdateMice;

        _checkTime = 0;
    }

    void Update()
    {
        if (enabled)
        {
            //// 除錯訊息
            //if (_bLoadedActor || _bLoadedAsset)
            //    if (!string.IsNullOrEmpty(assetLoader.ReturnMessage))
            //        Debug.Log("訊息：" + assetLoader.ReturnMessage);

            // 顯示 配對等待時間文字
            if (Global.isMatching && Time.time > _lastTime + 1)
            {
                _escapeTime++;
                MatchScrollText();
                _lastTime = Time.time;
                MatchStatusChk();
            }

            // 資料庫資料載入完成時 載入Panel
            if (_bLoadedPlayerData && _bLoadedPlayerItem && !_bLoadedPanel)
            {
                if (!_bFirstLoad)
                    ResumeToggleTarget();

                _bLoadedPanel = true;
                OnLoadPanel();
            }

            // 載入資產完成後 實體化 物件
            if (m_MPGame.GetAssetLoader().loadedObj && _bLoadedAsset  /*&& _bLoadedEffect*/)    // 可以使用了 只要畫SkillICON 並修改載入SkillICON
            {
                _bLoadedAsset = !_bLoadedAsset;
                _bLoadedEffect = !_bLoadedEffect;

                // 實體化按鈕
                InstantiateIcon(Global.dictMiceAll, _dictLoadedMiceBtnRefs, infoGroupsArea[0].transform);
                InstantiateIcon(Global.dictTeam, _dictLoadedTeamBtnRefs, infoGroupsArea[2].transform);

                // 載入道具數量資訊
                LoadItemCount(Global.playerItem, infoGroupsArea[0].transform);
                // LoadItemCount(Global.playerItem, infoGroupsArea[2].transform);

                // Enable按鈕
                SwitchBtnMethod.ActiveMice(Global.dictTeam, _dictLoadedMiceBtnRefs);
                // 顯示老鼠角色 Actor
                // StartCoroutine(OnClickCoroutine(infoGroupsArea[0].transform.GetChild(0).gameObject));

                ResumeToggleTarget();
            }

            //// 按下圖示按鈕後 載入角色完成時 實體化角色
            //if (assetLoader.loadedObj && _bLoadedActor)
            //{
            //    _bLoadedActor = !_bLoadedActor;
            //    assetLoader.init();
            //    string bundleName = _btnClick.gameObject.GetComponentInChildren<UISprite>().spriteName.Remove(_btnClick.gameObject.GetComponentInChildren<UISprite>().spriteName.Length - Global.extIconLength);
            //    InstantiateActor(bundleName, _actorParent.transform, actorScale);
            //}
        }
    }

    #region -- OnMiceClick 當按下老鼠時 --
    public void OnMiceClick(GameObject btn_mice)
    {
        if (Time.time - _lastClickTime < delayBetween2Clicks && _doubleClickChk == btn_mice)    // Double Click
            btn_mice.SendMessage("Mice2Click");

        _lastClickTime = Time.time;
        _doubleClickChk = btn_mice;
    }
    #endregion

    #region LoadItemCount
    /// <summary>
    /// 載入道具數量
    /// </summary>
    /// <param name="data">資料</param>
    /// <param name="parent">物件</param>
    private void LoadItemCount(Dictionary<string, object> data, Transform parent)
    {
        object miceData;

        SortChildren.SortChildrenByID(parent.gameObject, Global.extIconLength);

        for (int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i).GetComponentInChildren<UISprite>() != null)
                if (data.TryGetValue(parent.GetChild(i).GetComponentInChildren<UISprite>().name, out miceData))
                {
                    Dictionary<string, object> miceProp = miceData as Dictionary<string, object>;
                    parent.parent.FindChild("ItemCount").GetChild(i).GetComponentInChildren<UILabel>().text = miceProp["ItemCount"].ToString();
                }
        }
    }
    #endregion

    #region LoadPlayerMiceProp
    /// <summary>
    /// 載入老鼠資料
    /// </summary>
    /// <param name="miceBtn">老鼠按鈕</param>
    private void LoadPlayerMiceProp(GameObject miceBtn)
    {
        object rank, exp, data;
        float miceMaxExp;

        Global.playerItem.TryGetValue(miceBtn.name, out data);

        Dictionary<string, object> miceProp = data as Dictionary<string, object>;
        miceProp.TryGetValue(PlayerItem.Rank.ToString(), out rank);
        miceProp.TryGetValue(PlayerItem.Exp.ToString(), out exp);
        miceMaxExp = Clac.ClacMiceExp(Convert.ToInt32(rank) + 1);

        infoGroupsArea[1].transform.FindChild("Rank").GetComponentInChildren<UILabel>().text = rank.ToString();
        infoGroupsArea[1].transform.FindChild("Exp").GetComponentInChildren<UISlider>().value = Convert.ToSingle(exp) / miceMaxExp;
        infoGroupsArea[1].transform.FindChild("Exp").GetComponentInChildren<UILabel>().text = exp.ToString() + " / " + miceMaxExp.ToString();
    }
    #endregion


    #region -- InstantiateIcon 實體化老鼠物件--
    /// <summary>
    /// 實體化載入完成的遊戲物件，利用玩家JSON資料判斷必要實體物件
    /// </summary>
    /// <param name="dictServerData">ServerData</param>
    /// <param name="dictLoadedObject">Client Loaded Object</param>
    /// <param name="myParent">實體化父系位置</param>
    /// <returns>true=ok false=有重複Key沒有實體化完成</returns>
    bool InstantiateIcon(Dictionary<string, object> dictServerData, Dictionary<string, GameObject> loadedBtnRefs, Transform myParent)
    {
        int i = 0;
        bool bComplete = true; //是否完成實體化

        Dictionary<string, GameObject> tmp = new Dictionary<string, GameObject>();
        List<string> keys = loadedBtnRefs.Keys.ToList();
        foreach (KeyValuePair<string, object> item in dictServerData)
        {
            string bundleName = item.Value.ToString() + Global.IconSuffix;

            if (assetLoader.GetAsset(bundleName) != null)                                   // 已載入資產時
            {
                GameObject bundle = assetLoader.GetAsset(bundleName);
                Transform miceBtn = myParent.Find(myParent.name + (i + 1).ToString());

                if (miceBtn.childCount == 0)                                                // 如果 按鈕下 沒有物件 實體化物件
                {
                    MPGFactory.GetObjFactory().Instantiate(bundle, miceBtn, item.Key, Vector3.zero, Vector3.one, new Vector2(65, 65), -1);
                    miceBtn.gameObject.AddMissingComponent<BtnSwitch>().init(ref _dictLoadedMiceBtnRefs, ref _dictLoadedTeamBtnRefs, ref myParent);
                    miceBtn.GetComponent<BtnSwitch>().SendMessage("EnableBtn");          // 開啟按鈕功能
                }
                else if (item.Key.ToString() != keys[i])                                    // 如果 按鈕下 有物件 且資料不同步時 修正按鈕
                {
                    miceBtn.GetComponentInChildren<UISprite>().gameObject.name = item.Key;  // 如果Key不相同 重新載入ICON 修正ICON ID
                    miceBtn.GetComponentInChildren<UISprite>().spriteName = bundleName;     // 如果Key不相同 重新載入ICON 修正ICON SPRITE
                }
                SwitchBtnMethod.Add2Refs(loadedBtnRefs, _dictLoadedMiceBtnRefs, _dictLoadedTeamBtnRefs, i, item.Key, miceBtn.gameObject);                   // 加入物件參考
                i++;
            }
            else
            {
                bComplete = false;
                Debug.LogError("Assetbundle reference not set to an instance. at InstantiateIcon (Line:154).");
            }
        }

        _bLoadedAsset = false; // 實體化完成 關閉載入
        return bComplete ? true : false;
    }
    #endregion

    //#region -- InstantiateItem 實體化道具(按鈕) --
    ///// <summary>
    ///// 實體化道具(按鈕)
    ///// </summary>
    ///// <param name="parent">物件位置</param>
    ///// <param name="objectID">ID</param>
    //private void InstantiateItem(Transform parent, string objectID)
    //{
    //    int itemID = Convert.ToInt16(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.miceProperty, "ItemID", objectID.ToString()));
    //    string bundleName = MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.itemProperty, "ItemName", itemID.ToString()).ToString() + Global.IconSuffix;
    //    GameObject bundle = assetLoader.GetAsset(bundleName);

    //    // 已載入資產時
    //    if (bundle != null)
    //    {
    //        if (parent.childCount == 0)
    //        {
    //            MPGFactory.GetObjFactory().Instantiate(bundle, parent, objectID.ToString(), Vector3.zero, Vector3.one, new Vector2(65, 65), -1);
    //        }
    //        else
    //        {
    //            parent.GetComponentInChildren<UISprite>().gameObject.name = objectID.ToString();
    //            parent.GetComponentInChildren<UISprite>().spriteName = bundleName;
    //        }
    //    }
    //}
    //#endregion

    #region -- OnLoadPanel 載入面板--
    protected override void OnLoading()
    {
        Global.photonService.LoadPlayerData(Global.Account);
        Global.photonService.LoadPlayerItem(Global.Account);
    }

    void OnLoadPlayerData()
    {
        _bLoadedPlayerData = true;
    }

    void OnLoadPlayerItem()
    {
        _bLoadedPlayerItem = true;
    }

    protected override void OnLoadPanel()
    {
        GetMustLoadAsset();

        if (!_bFirstLoad)
        {
            if (!SwitchBtnMethod.MemberChk(Global.dictMiceAll, _dictMiceData, _dictLoadedMiceBtnRefs, infoGroupsArea[0].transform))
                InstantiateIcon(Global.dictMiceAll, _dictLoadedMiceBtnRefs, infoGroupsArea[0].transform);

            if (!SwitchBtnMethod.MemberChk(Global.dictTeam, _dictTeamData, _dictLoadedTeamBtnRefs, infoGroupsArea[2].transform))
                InstantiateIcon(Global.dictTeam, _dictLoadedTeamBtnRefs, infoGroupsArea[2].transform);

            SwitchBtnMethod.ActiveMice(Global.dictTeam, _dictLoadedMiceBtnRefs);
        }
        else
        {
            _bFirstLoad = false;
        }

        _dictMiceData = Global.dictMiceAll;
        _dictTeamData = Global.dictTeam;

        OnCostCheck();
    }
    #endregion

    // 載入必要資產
    protected override void GetMustLoadAsset()
    {
        if (transform.parent.gameObject.activeSelf)     // 如果Panel是啟動狀態 接收Event
        {
            Dictionary<string, object> dictNotLoadedAsset = new Dictionary<string, object>();

            // 如果是第一次載入 載入全部資產 否則 載入必要資產
            if (_bFirstLoad)
            {
                dictNotLoadedAsset = _dictMiceData = Global.dictMiceAll;
                _dictTeamData = Global.dictTeam;
            }
            else
            {
                Dictionary<string, object> newAssetData;

                LoadItemCount(Global.playerItem, infoGroupsArea[0].transform);
                LoadProperty.ExpectOutdataObject(Global.dictMiceAll, _dictMiceData, _dictLoadedMiceBtnRefs);
                LoadProperty.ExpectOutdataObject(Global.dictTeam, _dictMiceData, _dictLoadedTeamBtnRefs);

                // Where 找不存在的KEY 再轉換為Dictionary
                newAssetData = Global.dictMiceAll.Where(kvp => !_dictMiceData.ContainsKey(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                dictNotLoadedAsset = GetDontNotLoadAsset(newAssetData);

                ResumeToggleTarget();
            }

            if (dictNotLoadedAsset.Count != 0)  // 如果 有未載入物件 載入AB
            {
                assetLoader.init();
                assetLoader.LoadAsset(iconPath + "/", iconPath);
                // _bLoadedEffect = LoadEffectAsset(dictNotLoadedAsset);    // 可以使用了 只要畫SkillICON 並修改載入SkillICON
                _bLoadedAsset = LoadIconObject(dictNotLoadedAsset, iconPath);
            }                                   // 已載入物件 實體化
            else
            {
                _bLoadedAsset = true;
            }

            Global.isPlayerDataLoaded = false;

            OnCostCheck();
        }
    }



    public override void OnClosed(GameObject obj)
    {
        EventMaskSwitch.lastPanel = null;
        Panel[0].SetActive(true);
        if (Global.isMatching)
            Global.photonService.ExitWaitingRoom();
        GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().LoadPanel(obj.transform.parent.gameObject);
        Panel[1].SetActive(false);
        // EventMaskSwitch.Prev();
    }

    private void OnUpdateMice()
    {
        OnCostCheck();
    }

    private bool OnCostCheck()
    {
        object value;
        int maxCost = Clac.ClacCost(Global.Rank);
        _miceCost = 0;
        Dictionary<string, object> data;
        foreach (KeyValuePair<string, object> mice in Global.dictTeam)
        {
            data = Global.miceProperty[mice.Key] as Dictionary<string, object>;
            data.TryGetValue("MiceCost", out value);
            _miceCost += Convert.ToInt32(value);
        }

        infoGroupsArea[1].transform.FindChild("Cost").GetComponent<UILabel>().text = "[14B5DE]" + _miceCost + "/" + maxCost + "[-]";
        ok_btn.SetActive(true);
        if (_miceCost > maxCost)
        {
            infoGroupsArea[1].transform.FindChild("Cost").GetComponent<UILabel>().text = "[FF0000]" + _miceCost + "[-]" + "[14B5DE]/" + maxCost + "[-]";
            ok_btn.SetActive(false);
            return false;
        }
        else
        {
            return true;
        }
    }


    //----------------------------------------------------------------------- diff

    private void MatchStatusChk()
    {
        if (Global.isMatching)
        {
            int waitTime = Global.WaitTime;
            if (Global.isFriendMatching) waitTime = Global.WaitTime * 2;

            if ((_escapeTime > waitTime) && _checkFlag)
            {
                //  Global.photonService.ExitWaitingRoom();
                if (!Global.isFriendMatching)
                {
                    Global.photonService.MatchGameBot(Global.PrimaryID, Global.dictTeam);
                }
                else
                {
                    Global.photonService.ExitWaitingRoom();
                    Global.isFriendMatching = false;
                }
                _checkTime = 0;
                _checkFlag = false;
            }
            else
            {
                _checkFlag = true;

                _checkTime = Time.fixedTime;
            }
        }
    }

    private void MatchScrollText()
    {
        try
        {
            timeText.text = "(" + _escapeTime.ToString() + ")";
        }
        catch
        {
            throw;
        }
        //if (_lastTime > _time)
        //{
        //    matchText.text = "尋找玩家...";
        //}
    }

    private bool TeamCountChk()
    {
        return (Global.dictTeam.Count > 0 && Global.dictTeam.Count <= 5) ? true : false;
    }

    // Match按鈕使用 開始配對
    public void OnMatchGame(GameObject go)
    {
        if (!Global.isMatching && Global.LoginStatus)
        {
            if (TeamCountChk())
            {
                matchText.text = "尋找玩家...";
                _escapeTime = 0;
                Global.isMatching = true;
                Panel[1].SetActive(true);
                Panel[0].SetActive(false);
                timeText = matchText.transform.GetChild(0).GetComponent<UILabel>();
                Global.photonService.MatchGame(Global.PrimaryID, Global.dictTeam);
            }
        }
    }

    // 接收 同意好友對戰
    private void OnApplyMatchGameFriend()
    {
        Panel[0].SetActive(true);
        Panel[1].SetActive(false);

        EventMaskSwitch.Resume();
        if (EventMaskSwitch.lastPanel != gameObject)
            EventMaskSwitch.lastPanel.SetActive(false);
        EventMaskSwitch.Switch(gameObject/*, false*/);
        EventMaskSwitch.lastPanel = gameObject;

        UIEventListener.Get(ok_btn).onClick = MatchGameFriend;
    }


    private void MatchGameFriend(GameObject go)
    {
        if (!Global.isMatching && Global.LoginStatus && OnCostCheck())
        {
            matchText.text = "等待好友同意...";
            _escapeTime = 0;
            Global.isMatching = true;
            UIEventListener.Get(ok_btn).onClick = OnMatchGame;
            Panel[1].SetActive(true);
            Panel[0].SetActive(false);
            timeText = matchText.transform.GetChild(0).GetComponent<UILabel>();
            Global.isFriendMatching = true;
            Global.photonService.MatchGameFriend();
        }
    }

    private void OnExitWaiting()
    {
        //matchText.text = "等待超時，請重新配對！";
        EventMaskSwitch.lastPanel = null;
        Panel[0].SetActive(true);
        Panel[1].SetActive(false);
        GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().LoadPanel(transform.parent.gameObject);

    }
    //-----------------------------------------------

    void OnDisable()
    {
        Global.photonService.LoadPlayerDataEvent -= OnLoadPlayerData;
        Global.photonService.LoadPlayerItemEvent -= OnLoadPlayerItem;
        Global.photonService.UpdateMiceEvent -= OnUpdateMice;
        Global.photonService.ExitWaitingEvent -= OnExitWaiting;

        Panel[1].SetActive(false);
        Panel[1].transform.parent.parent.gameObject.SetActive(false);
    }
}