using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using System.Linq;
using MPProtocol;
using System;
/* ***************************************************************
 * ------------  Copyright © 2021 Gansol Studio.  All Rights Reserved.  ------------
 * ----------------                                CC BY-NC-SA 4.0                                ----------------
 * ----------------                @Website:  EasyUnity@blogspot.com      ----------------
 * ----------------                @Email:    GansolTW@gmail.com               ----------------
 * ----------------                @Author:   Krola.                                             ----------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * 負責 開啟/關閉/載入 Team的的所有處理
 * NGUI BUG : Team交換時Tween會卡色
 * + pageVaule 還沒加入翻頁值
 * ***************************************************************
 *                           ChangeLog
 * 20201027 v3.0.0  繼承重構
 * 20171213 v1.1.1   交換按鈕功能合併修改至SwitchBtnComponent                        
 * 20171211 v1.1.0   重製、修正索引問題   
 * 20161102 v1.0.2   3次重構，改變繼承至 PanelManager>MPPanel
 * 20160914 v1.0.1b  2次重購，獨立實體化物件  
 * 20160711 v1.0.1a  1次重構，獨立AssetLoader                       
 * 20160705 v1.0.0   0版完成，載入老鼠部分未來需要修改                    
 * ****************************************************************/

public class MatchUI : IMPPanelUI
{
    #region 欄位
    SwitchBtnComponent SwitchBtnMethod;
    AttachBtn_MatchUI UI;

    private static Dictionary<string, object> _dictMiceData;                        // Json全部老鼠資料
    private static Dictionary<string, object> _dictTeamData;                       // Json老鼠隊伍資料

    private Dictionary<string, GameObject> _dictLoadedMiceBtnRefs;   // 已載入的按鈕(全部老鼠)
    private Dictionary<string, GameObject> _dictLoadedTeamBtnRefs;  // 已載入的按鈕(隊伍老鼠)

    private GameObject _btnClick;                   // 按下按鈕
    private GameObject _doubleClickChk;     // 雙擊檢查

    private int _page;                                            // 翻頁值(翻一頁+10)
    private int _miceCost;                                   // 隊伍老鼠Cost
    private int _dataLoadedCount;                   // 資料載入數量
    [Range(0.2f, 0.4f)]
    private float _delayBetween2Clicks;         // 雙擊間隔時間
    private float _lastTime;                                 // 儲存起始配對時間
    private float _escapeTime;                          // 配對等待經過時間
    private float _lastClickTime;                        // 點擊間距時間

    private bool _bFirstLoad;                             // 是否 第一次載入
    private bool _bLoadedPanel;                      // 是否載入Panel
    private bool _bLoadedAsset;                      // 是否載入資產
    private bool _bLoadedActor;                      // 是否載入角色資產
    private bool _bLoadedEffect;                      // 是否載入特效資產
    private bool _bMatchingCheckFlag;         // 是否在配對等待

    //private GameObject _actorParent;            // 角色
    #endregion

    public MatchUI(MPGame MPGame) : base(MPGame)
    {
        Debug.Log("--------------- MatchUI Create ----------------");
        SwitchBtnMethod = new SwitchBtnComponent();
        _dictMiceData = new Dictionary<string, object>();
        _dictTeamData = new Dictionary<string, object>();
        _dictLoadedMiceBtnRefs = new Dictionary<string, GameObject>();
        _dictLoadedTeamBtnRefs = new Dictionary<string, GameObject>();

        _delayBetween2Clicks = 0.3f;
        _bFirstLoad = true;
    }

    public override void Initialize()
    {
        Debug.Log("--------------- MatchUI Initialize ----------------");

        //_page = 0;
        //_actorParent = UI.info_group.transform.GetChild(0).gameObject;    // 方便程式辨認用 UI.info_group.transform.GetChild(0).gameObject = image

        Global.photonService.LoadPlayerDataEvent += OnLoadPlayerData;
        Global.photonService.LoadPlayerItemEvent += OnLoadPlayerItem;
        Global.photonService.ExitWaitingEvent += OnExitWaiting;
        Global.photonService.UpdateMiceEvent += OnUpdateMice;
        Global.photonService.ApplyMatchGameFriendEvent += OnApplyMatchGameFriend;
    }

    public override void Update()
    {
        base.Update();
        if (m_RootUI.gameObject.activeSelf)
        {
            //// 除錯訊息
            //if (_bLoadedActor || _bLoadedAsset)
            //    if (!string.IsNullOrEmpty(assetLoader.ReturnMessage))
            //        Debug.Log("訊息：" + assetLoader.ReturnMessage);

            #region //  顯示 配對等待時間文字
            if (Global.isMatching && Time.time > _lastTime + 1)
            {
                _escapeTime++;
                _lastTime = Time.time;
                MatchScrollText();
                MatchStatusChk();
            }
            #endregion

            #region // 資料庫資料載入完成時 載入Panel
            if (_dataLoadedCount == GetMustLoadedDataCount() && !_bLoadedPanel)
            {
                _bLoadedPanel = true;
                GetMustLoadAsset();
            }
            #endregion

            #region // 載入資產完成後 實體化 物件
            if (m_AssetLoaderSystem.IsLoadAllAseetCompleted && _bLoadedAsset  /*&& _bLoadedEffect*/)    // 可以使用了 只要畫SkillICON 並修改載入SkillICON
            {
                m_AssetLoaderSystem.Initialize();
                _bLoadedAsset = false;
                _bLoadedEffect = false;

                OnLoadPanel();
                OnCostCheck();
                LoadItemCount(Global.playerItem, UI.mice_group.transform);
                ResumeToggleTarget();
            }
            #endregion

            #region  // 按下圖示按鈕後 載入角色完成時 實體化角色
            //if (assetLoader.loadedObj && _bLoadedActor)
            //{
            //    _bLoadedActor = !_bLoadedActor;
            //    
            //    string bundleName = _btnClick.gameObject.GetComponentInChildren<UISprite>().spriteName.Remove(_btnClick.gameObject.GetComponentInChildren<UISprite>().spriteName.Length - Global.extIconLength);
            //    InstantiateActor(bundleName, _actorParent.transform, actorScale);
            //}
            #endregion
        }
    }

    #region -- LoadItemCount 載入道具數量 --
    /// <summary>
    /// 載入道具數量
    /// </summary>
    /// <param name="data">資料</param>
    /// <param name="parent">物件</param>
    private void LoadItemCount(Dictionary<string, object> data, Transform parent)
    {
        // 排序Gameobject
        SortChildren.SortChildrenByID(parent.gameObject, Global.extIconLength);

        for (int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i).GetComponentInChildren<UISprite>() != null)
                if (data.TryGetValue(parent.GetChild(i).GetComponentInChildren<UISprite>().name, out object miceData))
                {
                    Dictionary<string, object> miceProp = miceData as Dictionary<string, object>;
                    parent.parent.Find("ItemCount").GetChild(i).GetComponentInChildren<UILabel>().text = miceProp["ItemCount"].ToString();
                }
        }
    }
    #endregion

    #region -- LoadPlayerMiceProp 載入老鼠資料 --
    /// <summary>
    /// 載入老鼠資料
    /// </summary>
    /// <param name="miceBtn">老鼠按鈕</param>
    private void LoadPlayerMiceProp(GameObject miceBtn)
    {
        Global.playerItem.TryGetValue(miceBtn.name, out object data);

        Dictionary<string, object> miceProp = data as Dictionary<string, object>;
        miceProp.TryGetValue(PlayerItem.Rank.ToString(), out object rank);
        miceProp.TryGetValue(PlayerItem.Exp.ToString(), out object exp);
        float miceMaxExp = Clac.ClacMiceExp(Convert.ToInt32(rank) + 1);

        UI.info_group.transform.Find("Rank").GetComponentInChildren<UILabel>().text = rank.ToString();
        UI.info_group.transform.Find("Exp").GetComponentInChildren<UISlider>().value = Convert.ToSingle(exp) / miceMaxExp;
        UI.info_group.transform.Find("Exp").GetComponentInChildren<UILabel>().text = exp.ToString() + " / " + miceMaxExp.ToString();
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
            string bundleName = Global.IconSuffix + item.Value.ToString();

            if (m_AssetLoaderSystem.GetAsset(bundleName) != null)                                   // 已載入資產時
            {
                GameObject bundle = m_AssetLoaderSystem.GetAsset(bundleName);
                Transform miceBtn = myParent.Find(myParent.name + (i + 1).ToString());

                if (miceBtn.childCount == 0)                                                // 如果 按鈕下 沒有物件 實體化物件
                {
                    MPGFactory.GetObjFactory().Instantiate(bundle, miceBtn, item.Key, Vector3.zero, Vector3.one, new Vector2(65, 65), -1);
                    miceBtn.gameObject.AddMissingComponent<BtnSwitch>().Initialize(ref _dictLoadedMiceBtnRefs, ref _dictLoadedTeamBtnRefs, ref myParent);
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

    #region -- GetMustLoadAsset 載入必要資產 --
    /// <summary>
    /// 載入必要資產
    /// </summary>
    protected override void GetMustLoadAsset()
    {
        List<string> notLoadedAssetList;

        // 如果是第一次載入 載入全部資產 否則 載入必要資產
        if (_bFirstLoad)
        {
            notLoadedAssetList = GetDontNotLoadAssetName(Global.dictMiceAll);
        }
        else
        {
            LoadItemCount(Global.playerItem, UI.mice_group.transform);
            LoadProperty.ExpectOutdataObject(Global.dictMiceAll, _dictMiceData, _dictLoadedMiceBtnRefs);
            LoadProperty.ExpectOutdataObject(Global.dictTeam, _dictMiceData, _dictLoadedTeamBtnRefs);

            // Where 找不存在的KEY 再轉換為Dictionary
            Dictionary<string, object> newAssetData = Global.dictMiceAll.Where(kvp => !_dictMiceData.ContainsKey(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            notLoadedAssetList = GetDontNotLoadAssetName(newAssetData);
        }

        // 如果 有未載入物件 載入AB
        if (notLoadedAssetList.Count > 0)
        {
            // _bLoadedEffect = LoadEffectAsset(dictNotLoadedAsset);    // 可以使用了 只要畫SkillICON 並修改載入SkillICON
            _bLoadedAsset = LoadIconObjectsAssetByName(notLoadedAssetList, Global.MiceIconUniquePath);
        }
        else
        {
            _bLoadedAsset = true;
        }

        _dictMiceData = Global.dictMiceAll;
        _dictTeamData = Global.dictTeam;
    }
    #endregion

    #region -- MatchStatusChk 檢查配對狀態 --
    /// <summary>
    /// 檢查配對狀態
    /// </summary>
    private void MatchStatusChk()
    {
        if (Global.isMatching)
        {
            int waitTime = Global.WaitTime;
            if (Global.isFriendMatching) waitTime = Global.WaitTime * 2;

            if ((_escapeTime > waitTime) && _bMatchingCheckFlag)
            {
                // 如果朋友不再配對中 配對BOT  (Global.isFriendMatching 錯誤)
                if (!Global.isFriendMatching)
                {
                    Global.photonService.MatchGameBot(Global.PrimaryID, Global.dictTeam);
                }
                else
                {
                    // 離開等待房間
                    Global.photonService.ExitWaitingRoom();
                    Global.isFriendMatching = false;
                }
                _bMatchingCheckFlag = false;
            }
            else
            {
                _bMatchingCheckFlag = true;
            }
        }
    }
    #endregion

    #region -- MatchScrollText 等待文字動畫 --
    /// <summary>
    /// 還沒寫完 簡易文字 錯誤
    /// </summary>
    /// <returns></returns>
    private void MatchScrollText()
    {
            UI.time_label.text = "(" + _escapeTime.ToString() + ")";
    }
    #endregion

    #region -- TeamCountChk 判斷隊伍數量 --
    /// <summary>
    /// 還沒寫完 簡易判斷 錯誤
    /// </summary>
    /// <returns></returns>
    private bool TeamCountChk()
    {
        return (Global.dictTeam.Count > 0 && Global.dictTeam.Count <= 5) ? true : false;
    }
    #endregion

    #region -- MatchGameFriend 與好友對戰 --
    /// <summary>
    /// 與好友對戰
    /// </summary>
    /// <param name="go">對戰按扭</param>
    private void OnMatchGameFriend(GameObject go)
    {
        if (!Global.isMatching && Global.LoginStatus && OnCostCheck())
        {
            UI.matching_label.text = "等待好友同意...";
            _escapeTime = 0;
            Global.isMatching = true;
            UIEventListener.Get(UI.okBtn).onClick = OnMatchGame;
            UI.matchingPanel.SetActive(true);
            UI.beforeMatchPanel.SetActive(false);
            UI.time_label = UI.matching_label.transform.GetChild(0).GetComponent<UILabel>();
            Global.isFriendMatching = true;
            Global.photonService.MatchGameFriend();
        }
    }
    #endregion

    #region -- OnLoadPanel 載入面板 --
    protected override void OnLoading()
    {
        _bLoadedPanel = false;
        _dataLoadedCount = (int)ENUM_Data.None;

        UI = m_RootUI.GetComponentInChildren<AttachBtn_MatchUI>();

        UIEventListener.Get(UI.okBtn).onClick = OnMatchGame;
        UIEventListener.Get(UI.closeCollider).onClick = OnClosed;
        UIEventListener.Get(UI.exitBtn).onClick = OnClosed;

        Global.photonService.LoadPlayerData(Global.Account);
        Global.photonService.LoadPlayerItem(Global.Account);
    }

    /// <summary>
    /// 當載入Panel完成時
    /// </summary>
    protected override void OnLoadPanel()
    {
        if (!SwitchBtnMethod.MemberChk(Global.dictMiceAll, _dictMiceData, _dictLoadedMiceBtnRefs, UI.mice_group.transform) || _bFirstLoad)
            InstantiateIcon(Global.dictMiceAll, _dictLoadedMiceBtnRefs, UI.mice_group.transform);

        if (!SwitchBtnMethod.MemberChk(Global.dictTeam, _dictTeamData, _dictLoadedTeamBtnRefs, UI.team_group.transform) || _bFirstLoad)
            InstantiateIcon(Global.dictTeam, _dictLoadedTeamBtnRefs, UI.team_group.transform);

        SwitchBtnMethod.ActiveMice(Global.dictTeam, _dictLoadedMiceBtnRefs);

        _bFirstLoad = false;
        _dictMiceData = Global.dictMiceAll;
        _dictTeamData = Global.dictTeam;

    }
    #endregion

    #region -- OnLoadData 當收到伺服器資料時 --
    /// <summary>
    /// 當收到伺服器玩家資料時，累計資料數量
    /// </summary>
    void OnLoadPlayerData()
    {
        _dataLoadedCount *= (int)ENUM_Data.PlayerData;
    }

    /// <summary>
    /// 當收到伺服器玩家道具時，累計資料數量
    /// </summary>
    void OnLoadPlayerItem()
    {
        _dataLoadedCount *= (int)ENUM_Data.PlayerItem;
    }

    protected override int GetMustLoadedDataCount()
    {
        return (int)ENUM_Data.PlayerData * (int)ENUM_Data.PlayerItem;
    }
    #endregion

    #region  -- OnUpdateMice  當隊伍更新時 --
    /// <summary>
    /// 當隊伍更新變動時 檢查老鼠費用
    /// </summary>
    private void OnUpdateMice()
    {
        OnCostCheck();
    }
    #endregion

    #region -- OnCostCheck 檢查老鼠Cost --
    /// <summary>
    /// 檢查老鼠Cost
    /// </summary>
    /// <returns></returns>
    private bool OnCostCheck()
    {
        int maxCost = Clac.ClacCost(Global.Rank);
        _miceCost = 0;

        // 加總Cost
        foreach (KeyValuePair<string, object> mice in Global.dictTeam)
        {
            Dictionary<string, object> data = Global.miceProperty[mice.Key] as Dictionary<string, object>;
            data.TryGetValue("MiceCost", out object value);
            _miceCost += Convert.ToInt32(value);
        }
        // 顯示超過Cost紅字
        if (_miceCost > maxCost)
        {
            UI.info_group.transform.Find("Cost").GetComponent<UILabel>().text = "[FF0000]" + _miceCost + "[-]" + "[14B5DE]/" + maxCost + "[-]";
            UI.okBtn.SetActive(false);
            return false;
        }

        UI.info_group.transform.Find("Cost").GetComponent<UILabel>().text = "[14B5DE]" + _miceCost + "/" + maxCost + "[-]";
        UI.okBtn.SetActive(true);

        return true;

    }
    #endregion

    #region -- OnMiceClick 當按下老鼠時 --
    public void OnMiceClick(GameObject btn_mice)
    {
        // 雙擊事件(移動老鼠)
        if (Time.time - _lastClickTime < _delayBetween2Clicks && _doubleClickChk == btn_mice)
            btn_mice.SendMessage("Mice2Click");

        _lastClickTime = Time.time;
        _doubleClickChk = btn_mice;
    }
    #endregion

    #region -- OnMatchGame 開始配對 --
    /// <summary>
    /// Match按鈕使用 開始配對
    /// </summary>
    /// <param name="go"></param>
    public void OnMatchGame(GameObject go)
    {
        if (!Global.isMatching && Global.LoginStatus)
        {
            if (TeamCountChk())
            {
                UI.matching_label.text = "尋找玩家...";
                _escapeTime = 0;
                Global.isMatching = true;
                UI.matchingPanel.SetActive(true);
                UI.beforeMatchPanel.SetActive(false);
                UI.time_label = UI.matching_label.transform.GetChild(0).GetComponent<UILabel>();
                Global.photonService.MatchGame(Global.PrimaryID, Global.dictTeam);
            }
        }
    }
    #endregion

    #region -- OnApplyMatchGameFriend 接收 同意好友對戰 --
    /// <summary>
    /// 接收 同意好友對戰
    /// </summary>
    private void OnApplyMatchGameFriend()
    {
        if (Global.LoginStatus && !Global.isMatching)
        {
            //if (dictPanelRefs.ContainsKey(panelName))
            ShowPanel(m_RootUI.transform.GetChild(0).name);
        }

        UI.beforeMatchPanel.SetActive(true);
        UI.matchingPanel.SetActive(false);

        EventMaskSwitch.Resume();
        if (EventMaskSwitch.LastPanel != m_RootUI)  //m_RootUI = gameobject
            EventMaskSwitch.LastPanel.SetActive(false);
        EventMaskSwitch.Switch(m_RootUI/*, false*/); //m_RootUI = gameobject
        EventMaskSwitch.LastPanel = m_RootUI; //m_RootUI = gameobject

        UIEventListener.Get(UI.okBtn).onClick = OnMatchGameFriend;
    }
    #endregion

    #region -- OnExitWaiting 當收到配對超時 --
    /// <summary>
    /// 當收到配對超時
    /// </summary>
    private void OnExitWaiting()
    {
        //matching_label.text = "等待超時，請重新配對！";
        UI.beforeMatchPanel.SetActive(true);
        UI.matchingPanel.SetActive(false);
        ShowPanel(m_RootUI.transform.GetChild(0).name);
    }
    #endregion

    #region -- OnClosed --
    public override void OnClosed(GameObject go)
    {
        EventMaskSwitch.LastPanel = null;
        UI.beforeMatchPanel.SetActive(true);
        if (Global.isMatching)
            Global.photonService.ExitWaitingRoom();
        ShowPanel(m_RootUI.transform.GetChild(0).name);
        UI.matchingPanel.SetActive(false);

    }
    #endregion

    public override void ShowPanel(string panelName)
    {
        m_RootUI = GameObject.Find(Global.Scene.MainGameAsset.ToString()).GetComponentInChildren<AttachBtn_MenuUI>().matchPanel;
        base.ShowPanel(panelName);
    }

    #region -- Release --
    public override void Release()
    {
        Global.photonService.LoadPlayerDataEvent -= OnLoadPlayerData;
        Global.photonService.LoadPlayerItemEvent -= OnLoadPlayerItem;
        Global.photonService.ExitWaitingEvent -= OnExitWaiting;
        Global.photonService.UpdateMiceEvent -= OnUpdateMice;
        Global.photonService.ApplyMatchGameFriendEvent -= OnApplyMatchGameFriend;

        UI.beforeMatchPanel.SetActive(true);
        UI.matchingPanel.SetActive(false);
    }
    #endregion
}