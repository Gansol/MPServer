﻿using UnityEngine;
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
 * 20161102 v1.0.2   3次重構，改變繼承至 PanelManager>MPPanel
 * 20160914 v1.0.1b  2次重購，獨立實體化物件  
 * 20160711 v1.0.1a  1次重構，獨立AssetLoader                       
 * 20160705 v1.0.0   0版完成，載入老鼠部分未來需要修改                    
 * ****************************************************************/

public class MatchManager : MPPanel
{
    #region 欄位
    public GameObject[] infoGroupsArea;                                             // 物件群組位置
    /// <summary>
    /// MiceIcon名稱、Mice按鈕
    /// </summary>
    public Dictionary<string, GameObject> dictLoadedMice { get; set; }              // Icon名稱、Icon的按鈕
    /// <summary>
    /// TeamIcon名稱、Mice按鈕索引物件
    /// </summary>
    public Dictionary<string, GameObject> dictLoadedTeam { get; set; }              // Icon名稱、Icon的按鈕
    [Range(0.2f, 0.4f)]
    public float delayBetween2Clicks = 0.3f;                                        // 雙擊間隔時間
    public Vector3 actorScale;                                                      // 角色縮放
    public string iconPath = "MiceICON";                                            // 圖片資料夾位置
    public GameObject ok_btn;
    public GameObject[] Panel;
    public UILabel matchText, timeText;
    //private int _page;                                                              // 翻頁值(翻一頁+10)                                              
    private static bool _bFirstLoad;                                                // 是否第一次載入
    private bool _bLoadedIcon, _bLoadedActor, _checkFlag;                                       // 是否載入圖片、是否載入角色

    private List<EventDelegate> onClickEvent;
    private GameObject _btnClick, _doubleClickChk;                    // 角色、按下按鈕、雙擊檢查
    private static Dictionary<string, object> _dictMiceData/*, _dictTeamData*/;         // Json老鼠、隊伍資料
    private static int _miceCost, _maxCost = 100;
    float _time, _lastTime, _lastClickTime, clickInterval, escapeTime, _checkTime;    // 點擊間距時間
    #endregion


    public MatchManager(MPGame MPGame) : base(MPGame) { }

    void Awake()
    {
        _checkTime = 0;
        clickInterval = 2;
        dictLoadedMice = new Dictionary<string, GameObject>();
        dictLoadedTeam = new Dictionary<string, GameObject>();
        _dictMiceData = new Dictionary<string, object>();
        onClickEvent = new List<EventDelegate>();
        //_dictTeamData = new Dictionary<string, object>();

        //_page = 0;
        _bFirstLoad = true; // dontDestroyOnLoad 所以才使用非靜態

        UIEventListener.Get(ok_btn).onClick = OnMatchGame;
    }


    void OnEnable()
    {
        Global.photonService.LoadPlayerDataEvent += OnLoadPanel;
        Global.photonService.LoadPlayerItemEvent += None;
        Global.photonService.ExitWaitingEvent += OnExitWaiting;
        Global.photonService.UpdateMiceEvent += OnUpdateMice;
        _checkTime = 0;
    }



    void Update()
    {
        if (enabled)
        {
            if (_bLoadedActor || _bLoadedIcon)                                          // 除錯訊息
                if (!string.IsNullOrEmpty(assetLoader.ReturnMessage))
                    Debug.Log("訊息：" + assetLoader.ReturnMessage);


            // 顯示 配對等待時間文字
            if (Global.isMatching && Time.time > _lastTime + 1)
            {
                escapeTime++;
                MatchScrollText();
                _lastTime = Time.time;
            }

            if (assetLoader.loadedObj && _bLoadedIcon)
            {
                _bLoadedIcon = !_bLoadedIcon;
                assetLoader.init();
                InstantiateIcon(Global.dictMiceAll, infoGroupsArea[0].transform);
                LoadItemCount(Global.playerItem, infoGroupsArea[0].transform);
                InstantiateIcon(Global.dictTeam, infoGroupsArea[2].transform);
                // LoadItemCount(Global.playerItem, infoGroupsArea[2].transform);
                ActiveMice(Global.dictTeam);
                OnCostCheck();

                EventMaskSwitch.Resume();
                GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().Panel[5].SetActive(false);
                EventMaskSwitch.Switch(gameObject, false);
                EventMaskSwitch.lastPanel = gameObject;
            }
        }

        MatchStatusChk();
    }

    private void MatchStatusChk()
    {
        if (Global.isMatching)
        {
            int waitTime = Global.WaitTime;
            if (Global.isFriendMatching) waitTime = Global.WaitTime * 2;

            if ((_checkTime > waitTime) && _checkFlag)
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
                _checkTime += Time.deltaTime;
            }
        }
    }

    private void MatchScrollText()
    {
        try
        {
            timeText.text = "(" + escapeTime.ToString() + ")";
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
    private void LoadItemCount(Dictionary<string, object> data, Transform parent)
    {
        object miceData;

        for (int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i).GetComponentInChildren<UISprite>() != null)
                if (data.TryGetValue(parent.GetChild(i).GetComponentInChildren<UISprite>().name, out miceData))
                {
                    Dictionary<string, object> miceProp = miceData as Dictionary<string, object>;
                    parent.FindChild("Text").GetChild(i).GetComponentInChildren<UILabel>().text = miceProp["ItemCount"].ToString();
                }
        }
    }
    #endregion

    #region LoadPlayerMiceProp
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
    /// 實體化載入完成的遊戲物件，利用玩家JASON資料判斷必要實體物件
    /// </summary>
    /// <param name="dictServerData">ServerData</param>
    /// <param name="dictLoadedObject">Client Loaded Object</param>
    /// <param name="myParent">實體化父系位置</param>
    void InstantiateIcon(Dictionary<string, object> dictServerData, Transform myParent)
    {
        int i = 0;
        Dictionary<string, GameObject> tmp = new Dictionary<string, GameObject>();

        foreach (KeyValuePair<string, object> item in dictServerData)
        {
            string bundleName = item.Value.ToString() + "ICON";
            GameObject bundle = assetLoader.GetAsset(bundleName);

            if (bundle != null)                  // 已載入資產時
            {
                Transform miceBtn = myParent.Find(myParent.name + (i + 1).ToString());
                if (miceBtn.childCount == 0)
                {
                    MPGFactory.GetObjFactory().Instantiate(bundle, miceBtn, item.Key, Vector3.zero, Vector3.one, new Vector2(130, 130), -1);

                    Add2Refs(item.Key, miceBtn);     // 加入物件參考

                    miceBtn.GetComponent<MatchSwitcher>().enabled = true;           // 開啟老鼠隊伍交換功能
                    miceBtn.GetComponent<MatchSwitcher>().SendMessage("EnableBtn"); // 開啟按鈕功能
                }
                else
                {
                    //string imageName = miceBtn.GetComponentInChildren<UISprite>().gameObject.name;

                    tmp.Add(item.Key, miceBtn.gameObject);
                    miceBtn.GetComponentInChildren<UISprite>().gameObject.name = item.Key;
                    miceBtn.GetComponentInChildren<UISprite>().spriteName = bundleName;
                }
                i++;
            }
            else
            {
                Debug.LogError("Assetbundle reference not set to an instance. at InstantiateIcon (Line:154).");
            }
        }
        _bLoadedIcon = false; // 實體化完成 關閉載入
    }
    #endregion

    #region -- OnLoadPanel 載入面板--
    protected override void OnLoading()
    {
        Global.photonService.LoadPlayerData(Global.Account);
        Global.photonService.LoadPlayerItem(Global.Account);
    }

    protected override void OnLoadPanel()
    {
        if (transform.parent.gameObject.activeSelf)     // 如果Panel是啟動狀態 接收Event
        {
            Dictionary<string, object> dictNotLoadedAsset = new Dictionary<string, object>();

            if (_bFirstLoad)
            {
                dictNotLoadedAsset = Global.dictMiceAll;
                _bFirstLoad = false;
            }
            else
            {
                LoadProperty.ExpectOutdataObject(Global.dictMiceAll, _dictMiceData, dictLoadedMice);
                LoadProperty.ExpectOutdataObject(Global.dictTeam, _dictMiceData, dictLoadedTeam);
                _dictMiceData = LoadProperty.SelectNewData(Global.dictMiceAll, _dictMiceData);

                dictNotLoadedAsset = GetDontNotLoadAsset(_dictMiceData);

                ResumeToggleTarget();
            }

            if (dictNotLoadedAsset.Count != 0)  // 如果 有未載入物件 載入AB
            {
                assetLoader.init();
                assetLoader.LoadAsset(iconPath + "/", "MiceICON");
                _bLoadedIcon = LoadIconObject(dictNotLoadedAsset, iconPath);
            }                                   // 已載入物件 實體化
            else
            {
                InstantiateIcon(Global.dictMiceAll, infoGroupsArea[0].transform);
                InstantiateIcon(Global.dictTeam, infoGroupsArea[2].transform);
                ActiveMice(Global.dictTeam);
            }
            _dictMiceData = Global.dictMiceAll;
            //_dictTeamData = Global.Team;
            Global.isPlayerDataLoaded = false;
        }
        OnCostCheck();
    }
    #endregion


    private void OnUpdateMice()
    {
        OnCostCheck();
    }

    public bool OnCostCheck()
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

    public void OnClosed(GameObject obj)
    {
        EventMaskSwitch.lastPanel = null;
        Panel[0].SetActive(true);
        if (Global.isMatching)
            Global.photonService.ExitWaitingRoom();
        GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().LoadPanel(obj.transform.parent.gameObject);
        Panel[1].SetActive(false);
        // EventMaskSwitch.Prev();
    }

    #region -- ActiveMice 隱藏/顯示老鼠 --
    private void ActiveMice(Dictionary<string, object> dictTeamData) // 把按鈕變成無法使用 如果老鼠已Team中
    {
        foreach (KeyValuePair<string, object> item in dictTeamData)
        {
            if (dictLoadedMice.ContainsKey(item.Key.ToString()))
                dictLoadedMice[item.Key.ToString()].SendMessage("DisableBtn");
            else
                dictLoadedMice[item.Key.ToString()].SendMessage("EnableBtn");
        }
    }
    #endregion

    #region -- Add2Refs 加入老鼠參考 --
    /// <summary>
    /// 加入老鼠參考
    /// </summary>
    /// <param name="bundle">AssetBundle</param>
    /// <param name="myParent">參考按鈕</param>
    void Add2Refs(string bundle, Transform myParent)
    {
        string btnArea = myParent.parent.name;                          //按鈕存放區域名稱 Team / Mice 區域
        //string miceName = bundle.name.Remove(bundle.name.Length - 4);
        //string miceName = bundle.name.Remove(bundle.name.Length - 4);

        if (btnArea == "Mice")
            dictLoadedMice.Add(bundle, myParent.gameObject);          // 加入索引 老鼠所在的MiceBtn位置
        else
            dictLoadedTeam.Add(bundle, myParent.gameObject);      // 參考至 老鼠所在的MiceBtn位置
    }
    #endregion

    #region --字典 檢查/取值 片段程式碼 --

    public GameObject GetLoadedMice(string miceID)
    {
        GameObject obj;
        if (dictLoadedMice.TryGetValue(miceID, out obj))
            return obj;
        return null;
    }

    public GameObject GetLoadedTeam(string miceID)
    {
        GameObject obj;
        if (dictLoadedTeam.TryGetValue(miceID, out obj))
            return obj;
        return null;
    }
    #endregion


    private void None()
    {

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
                escapeTime = 0;
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
        EventMaskSwitch.Switch(gameObject, false);
        EventMaskSwitch.lastPanel = gameObject;

        UIEventListener.Get(ok_btn).onClick = MatchGameFriend;
    }


    private void MatchGameFriend(GameObject go)
    {
        if (!Global.isMatching && Global.LoginStatus && OnCostCheck())
        {
            matchText.text = "等待好友同意...";
            escapeTime = 0;
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

    void OnDisable()
    {
        Global.photonService.LoadPlayerDataEvent -= OnLoadPanel;
        Global.photonService.LoadPlayerItemEvent -= None;
        Global.photonService.ExitWaitingEvent -= OnExitWaiting;
        Global.photonService.UpdateMiceEvent -= OnUpdateMice;
        Panel[1].SetActive(false);
        Panel[1].transform.parent.parent.gameObject.SetActive(false);
    }

    //private void OnLoadScene()
    //{
    //    Global.photonService.LoadSceneEvent -= OnLoadScene;
    //    OnClosed(Panel[1].transform.parent.gameObject);
    //}
}