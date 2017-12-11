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
 * 20171211 v1.1.2   修正索引問題                  
 * 20171201 v1.1.1   修正隊伍交換問題         
 * 20171119 v1.1.0   修正載入流程 
 * 20161102 v1.0.2   3次重構，改變繼承至 PanelManager>MPPanel
 * 20160914 v1.0.1b  2次重購，獨立實體化物件  
 * 20160711 v1.0.1a  1次重構，獨立AssetLoader                       
 * 20160705 v1.0.0   0版完成，載入老鼠部分未來需要修改                    
 * ****************************************************************/

public class TeamManager : MPPanel
{
    #region 欄位
    public GameObject[] infoGroupsArea;                                             // 物件群組位置
    private Dictionary<string, GameObject> _dictLoadedMiceBtnRefs, _dictLoadedTeamBtnRefs;             // 已載入的按鈕(全部、隊伍)
    [Range(0.2f, 0.4f)]
    public float delayBetween2Clicks = 0.3f;                                        // 雙擊間隔時間
    public string iconPath = "MiceICON";                                            // 圖片資料夾位置
    public Vector3 actorScale;                                                      // 角色縮放
    public GameObject startShowActor;                                               // 起始顯示老鼠

    private static Dictionary<string, object> _dictMiceData, _dictTeamData;         // Json老鼠、隊伍資料

    private GameObject _actorParent, _btnClick, _doubleClickChk;                    // 角色、按下按鈕、雙擊檢查
    private bool _bFirstLoad, _bLoadedAsset, _bLoadedActor, _bLoadedEffect, _bLoadedPlayerData, _bLoadedPlayerItem, _bLoadedPanel; // 是否 第一次載入 載入圖片、是否載入角色
    private int _miceCost, _page;   // 翻頁值(翻一頁+10)
    private float _lastClickTime;                                                   // 點擊間距時間
    #endregion

    public TeamManager(MPGame MPGame) : base(MPGame) { }


    //infoGroupsArea[0].transform = mice
    //infoGroupsArea[2].transform = team
    void Awake()
    {
        _dictLoadedMiceBtnRefs = new Dictionary<string, GameObject>();
        _dictLoadedTeamBtnRefs = new Dictionary<string, GameObject>();
        _dictMiceData = new Dictionary<string, object>();
        _dictTeamData = new Dictionary<string, object>();

        //_page = 0;
        _bFirstLoad = true; // dontDestroyOnLoad 所以才使用非靜態
        actorScale = new Vector3(0.8f, 0.8f, 1);
        _actorParent = infoGroupsArea[1].transform.GetChild(0).gameObject;    // 方便程式辨認用 infoGroupsArea[1].transform.GetChild(0).gameObject = image
    }

    void OnEnable()
    {
        _bLoadedPanel = false;
        Global.photonService.LoadPlayerDataEvent += OnLoadPlayerData;
        Global.photonService.LoadPlayerItemEvent += OnLoadPlayerItem;
        Global.photonService.UpdateMiceEvent += OnCostCheck;
    }

    void Update()
    {
        // 除錯訊息
        if (_bLoadedActor || _bLoadedAsset)
            if (!string.IsNullOrEmpty(assetLoader.ReturnMessage))
                Debug.Log("訊息：" + assetLoader.ReturnMessage);

        // 資料庫資料載入完成時 載入Panel
        if (_bLoadedPlayerData && _bLoadedPlayerItem && !_bLoadedPanel)
        {
            if (!_bFirstLoad)
                ResumeToggleTarget();

            _bLoadedPanel = true;
            OnLoadPanel();
        }

        // 載入資產完成後 實體化 物件
        if (assetLoader.loadedObj && _bLoadedAsset  /*&& _bLoadedEffect*/)    // 可以使用了 只要畫SkillICON 並修改載入SkillICON
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
            ActiveMice(Global.dictTeam);
            // 顯示老鼠角色 Actor
            StartCoroutine(OnClickCoroutine(infoGroupsArea[0].transform.GetChild(0).gameObject));

            ResumeToggleTarget();
        }

        // 按下圖示按鈕後 載入角色完成時 實體化角色
        if (assetLoader.loadedObj && _bLoadedActor)
        {
            _bLoadedActor = !_bLoadedActor;
            assetLoader.init();
            string bundleName = _btnClick.gameObject.GetComponentInChildren<UISprite>().spriteName.Remove(_btnClick.gameObject.GetComponentInChildren<UISprite>().spriteName.Length - Global.extIconLength);
            InstantiateActor(bundleName, _actorParent.transform, actorScale);
        }
    }

    #region -- OnMiceClick 當按下老鼠時 --
    public void OnMiceClick(GameObject btn_mice)
    {
        if (Time.time - _lastClickTime < delayBetween2Clicks && _doubleClickChk == btn_mice)    // Double Click
            btn_mice.SendMessage("Mice2Click");
        else
            StartCoroutine(OnClickCoroutine(btn_mice));

        _lastClickTime = Time.time;
        _doubleClickChk = btn_mice;
    }

    IEnumerator OnClickCoroutine(GameObject miceBtn)   //Single Click
    {
        yield return new WaitForSeconds(delayBetween2Clicks);
        if (Time.time - _lastClickTime < delayBetween2Clicks)
            yield break;
        if (miceBtn.GetComponentInChildren<UISprite>())
        {
            _bLoadedActor = LoadActor(miceBtn, _actorParent.transform, actorScale);
            _btnClick = miceBtn;


            LoadProperty.LoadItemProperty(miceBtn.GetComponentInChildren<UISprite>().gameObject, infoGroupsArea[1], Global.miceProperty, (int)MPProtocol.StoreType.Mice);    // 載入老鼠屬性   
            LoadPlayerMiceProp(miceBtn.GetComponentInChildren<UISprite>().gameObject); // 載入玩家老鼠資料
            // InstantiateItem(infoGroupsArea[3].transform.GetChild(0), miceBtn.GetComponentInChildren<UISprite>().name);    // 可以使用了 只要畫SkillICON 並修改載入SkillICON
            // InstantiateSkill(infoGroupsArea[3].transform.GetChild(0), miceBtn.GetComponentInChildren<UISprite>().name);    // 可以使用了 只要畫SkillICON 並修改載入SkillICON
        }
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

        SortChildren.SortChildrenByID(parent.gameObject);

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
    /// 實體化載入完成的遊戲物件，利用玩家JASON資料判斷必要實體物件
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

                //Debug.Log("Btn: " + miceBtn.parent.parent.parent.name + "   " + miceBtn.parent.parent.parent.parent.name);

                Add2Refs(loadedBtnRefs, i, item.Key, miceBtn.gameObject);                   // 加入物件參考
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

    #region -- InstantiateSkill 實體化技能 --
    /// <summary>
    /// 實體化技能
    /// </summary>
    /// <param name="parent">物件位置</param>
    /// <param name="objectID">ID</param>
    private void InstantiateSkill(Transform parent, string objectID)
    {
        int skillID = Convert.ToInt16(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.miceProperty, "SkillID", objectID.ToString()));
        string bundleName = MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.dictSkills, "SkillName", objectID.ToString()).ToString();
        GameObject bundle = assetLoader.GetAsset(bundleName);

        if (bundle != null)                  // 已載入資產時
        {
            if (parent.childCount == 0)
            {
                MPGFactory.GetObjFactory().Instantiate(bundle, parent, objectID.ToString(), Vector3.zero, Vector3.one, new Vector2(65, 65), -1);
            }
            else
            {
                parent.GetComponentInChildren<UISprite>().gameObject.name = objectID.ToString();
                parent.GetComponentInChildren<UISprite>().spriteName = bundleName;
            }
        }
    }
    #endregion

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
            MemberChk(Global.dictMiceAll, _dictMiceData, _dictLoadedMiceBtnRefs, infoGroupsArea[0].transform);
            MemberChk(Global.dictTeam, _dictTeamData, _dictLoadedTeamBtnRefs, infoGroupsArea[2].transform);
            ActiveMice(Global.dictTeam);
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
    private void GetMustLoadAsset()
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
                Dictionary<string, object> newData;

                LoadItemCount(Global.playerItem, infoGroupsArea[0].transform);
                LoadProperty.ExpectOutdataObject(Global.dictMiceAll, _dictMiceData, _dictLoadedMiceBtnRefs);
                LoadProperty.ExpectOutdataObject(Global.dictTeam, _dictMiceData, _dictLoadedTeamBtnRefs);
                newData = LoadProperty.SelectNewData(Global.dictMiceAll, _dictMiceData);

                dictNotLoadedAsset = GetDontNotLoadAsset(newData);

                ResumeToggleTarget();
            }

            if (dictNotLoadedAsset.Count != 0)  // 如果 有未載入物件 載入AB
            {
                assetLoader.init();
                assetLoader.LoadAsset(iconPath + "/", "MiceICON");
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

    // 可以使用了 只要畫SkillICON 並修改載入SkillICON
    //private bool LoadEffectAsset(Dictionary<string, object> dictNotLoadedAsset)
    //{
    //    int itemID, skillID;
    //    string itemName, skillName;

    //    assetLoader.LoadAsset("ItemICON" + "/", "ItemICON");
    //    assetLoader.LoadAsset("Effects" + "/", "Effects");
    //    if (dictNotLoadedAsset != null)
    //    {
    //        foreach (KeyValuePair<string, object> item in dictNotLoadedAsset)
    //        {
    //            itemID = Convert.ToInt16(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.miceProperty, "ItemID", item.Key.ToString()));
    //            itemName = MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.itemProperty, "ItemName", itemID.ToString()).ToString();
    //            skillID = Convert.ToInt16(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.miceProperty, "SkillID", item.Key));
    //            skillName = MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.dictSkills, "SkillName", skillID.ToString()).ToString();

    //            assetLoader.LoadPrefab("ItemICON" + "/", itemName + "ICON");   // 載入 ICON
    //            assetLoader.LoadPrefab("Effects" + "/", skillName + "Effect");  // 載入 Effect
    //        }
    //        return true;
    //    }
    //    else
    //    {
    //        return false;
    //    } 
    //}

    public override void OnClosed(GameObject obj)
    {
        EventMaskSwitch.lastPanel = null;
        GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().LoadPanel(obj.transform.parent.gameObject);
        // EventMaskSwitch.Prev();
    }

    #region -- ActiveMice 隱藏/顯示老鼠 --
    /// <summary>
    /// 隱藏/顯示老鼠
    /// </summary>
    /// <param name="dictData">要被無效化的老鼠</param>
    private void ActiveMice(Dictionary<string, object> dictData) // 把按鈕變成無法使用 如果老鼠已Team中
    {
        var dictEnableMice = Global.dictMiceAll.Except(dictData);

        foreach (KeyValuePair<string, object> item in dictData)
        {
            if (_dictLoadedMiceBtnRefs.ContainsKey(item.Key.ToString()))
                _dictLoadedMiceBtnRefs[item.Key.ToString()].SendMessage("DisableBtn");
        }

        foreach (KeyValuePair<string, object> item in dictEnableMice)
        {
            _dictLoadedMiceBtnRefs[item.Key.ToString()].SendMessage("EnableBtn");
        }
    }
    #endregion

    #region -- Add2Refs 加入載入按鈕參考 --
    /// <summary>
    /// 加入載入按鈕參考
    /// </summary>
    /// <param name="loadedBtnRefs">按鈕參考字典</param>
    /// <param name="itemID">按鈕ID</param>
    /// <param name="myParent"></param>
    void Add2Refs(Dictionary<string, GameObject> loadedBtnRefs, int position, string itemID, GameObject myParent)
    {
        Transform btnArea = myParent.transform.parent;
        List<string> keys = loadedBtnRefs.Keys.ToList();

        // 檢查長度 防止溢位 position 初始值0
        if (position < loadedBtnRefs.Count && loadedBtnRefs.Count > 0)
        {
            // 如果已載入按鈕有重複Key
            if (loadedBtnRefs.ContainsKey(itemID))
            {
                // 如果Key值不同 移除舊資料
                if (keys[position] != itemID)
                {
                    loadedBtnRefs.Remove(itemID);

                    // 如果小於 載入按鈕的索引長度 直接修改索引 超過則新增
                    if (position < loadedBtnRefs.Count)
                    {
                        Global.RenameKey(loadedBtnRefs, keys[position], itemID);
                        loadedBtnRefs[itemID] = myParent;
                        loadedBtnRefs[itemID].GetComponent<BtnSwitch>().init(ref _dictLoadedMiceBtnRefs, ref _dictLoadedTeamBtnRefs, ref btnArea);
                    }
                    else
                    {
                        //Debug.Log("T new *******Ref ID:" + itemID + "  BtnName:" + myParent + "     Local:" + myParent.transform.parent.parent.parent.parent.name+"*************");
                        loadedBtnRefs.Add(itemID, myParent);
                        //loadedBtnRefs[itemID].GetComponent<BtnSwitch>().init(ref _dictLoadedMiceBtnRefs, ref _dictLoadedTeamBtnRefs, ref btnArea);
                    }
                }
                else
                {
                    Debug.Log("T re *******Ref ID:" + itemID + "  BtnName:" + myParent + "     Local:" + myParent.transform.parent.parent.parent.parent.name + "*************");
                    // 重新索引按鈕對應位置
                    loadedBtnRefs[itemID] = myParent;
                    loadedBtnRefs[itemID].GetComponent<BtnSwitch>().init(ref _dictLoadedMiceBtnRefs, ref _dictLoadedTeamBtnRefs, ref btnArea);
                }
            }
            else
            {
                // 如果小於 載入按鈕的索引長度 直接修改索引
                Global.RenameKey(loadedBtnRefs, keys[position], itemID);
                loadedBtnRefs[itemID] = myParent;
                loadedBtnRefs[itemID].GetComponent<BtnSwitch>().init(ref _dictLoadedMiceBtnRefs, ref _dictLoadedTeamBtnRefs, ref btnArea);
            }
        }
        else
        {
            // 大於 載入按鈕的索引長度 則新增索引
            loadedBtnRefs.Add(itemID, myParent.gameObject);
            //Debug.Log("T > *******Ref ID:" + itemID + "  BtnName:" + myParent + "     Local:" + myParent.transform.parent.parent.parent.parent.name + "*************");
        }

    }
    #endregion

    //#region -- SwitchMemeber 交換隊伍成員 --
    ///// <summary>
    ///// 交換隊伍成員
    ///// </summary>
    ///// <param name="key1">要交換的物件Key值</param>
    ///// <param name="key2">要交換的物件Key值</param>
    ///// <param name="dict">來源資料字典</param>
    //public void SwitchMemeber(string key1, string key2, Dictionary<string, object> dict)
    //{
    //    Dictionary<string, GameObject> tmpDict = _dictLoadedMiceBtnRefs;

    //    if (dict == Global.dictTeam)
    //        tmpDict = _dictLoadedTeamBtnRefs;

    //    // 交換 clientData Key and Value
    //    Global.SwapDictKey(key1, key2, "x", dict);
    //    Global.SwapDictValueByKey(key1, key2, dict);

    //    // 交換 btnRefs Key and Value
    //    Global.SwapDictKey(key1, key2, "x", tmpDict);
    //    Global.SwapDictValueByKey(key1, key2, tmpDict);
    //}
    //#endregion

    //#region -- TeamSequence 隊伍整理佇列 --
    ///// <summary>
    ///// 隊伍整理佇列 bDragOut=True : 拉出隊伍整理   bDragOut=False : 拉入隊伍整理
    ///// </summary>
    ///// <param name="btnRefs">受影響的按鈕</param>
    ///// <param name="bDragOut">拉入T 或 移出F</param>
    //public void TeamSequence(GameObject btnRefs, bool bDragOut)
    //{
    //    Dictionary<string, object> team = new Dictionary<string, object>(Global.dictTeam);
    //    int btnNo = int.Parse(btnRefs.name.Remove(0, btnRefs.name.Length - 1));
    //    // string miceName = teamRef.transform.GetChild(0).name;

    //    // 整理隊伍排序位置
    //    if (bDragOut)
    //    {
    //        // 拉出對隊伍時
    //        if (btnNo >= team.Count)
    //        {
    //            int offset = team.Count == 0 ? 0 : 1; ; // 當Btn=0時 防止溢位
    //            Transform teamBtn = infoGroupsArea[2].transform.GetChild(team.Count - offset);

    //            btnRefs.transform.GetChild(0).parent = infoGroupsArea[2].transform.GetChild(team.Count - offset);
    //            teamBtn.GetChild(0).localPosition = Vector3.zero;
    //            teamBtn.GetComponent<TeamSwitcher>().enabled = true;
    //            teamBtn.GetComponent<TeamSwitcher>().SendMessage("EnableBtn");
    //        }
    //    }
    //    else
    //    {
    //        // 拉入隊伍時
    //        if (btnNo < team.Count)                                           // 2<5
    //        {
    //            for (int i = 0; i < (team.Count - btnNo); i++)                  // 5-2=3  
    //            {
    //                GameObject outBtn;
    //                Transform teamIcon = infoGroupsArea[2].transform.GetChild(btnNo + i).GetChild(0);
    //                Transform pervTeamBtn = infoGroupsArea[2].transform.GetChild(btnNo + i - 1);

    //                if (_dictLoadedTeamBtnRefs.TryGetValue(teamIcon.name, out outBtn))
    //                {
    //                    _dictLoadedTeamBtnRefs[teamIcon.name] = pervTeamBtn.gameObject; //teamIcon.name  = ID
    //                }
    //                teamIcon.parent = pervTeamBtn; // team[2+i]=team[2+i-1] parent =>team[2]=team[1]

    //                if (i == 0)     // 因為第一個物件會有2個Child所以要GetChild(1) 下一個按鈕移過來的Mice
    //                    pervTeamBtn.GetChild(1).localPosition = Vector3.zero;
    //                else
    //                    pervTeamBtn.GetChild(0).localPosition = Vector3.zero;

    //                pervTeamBtn.GetChild(0).localPosition = Vector3.zero;
    //                pervTeamBtn.GetComponent<TeamSwitcher>().enabled = true;
    //                pervTeamBtn.GetComponent<TeamSwitcher>().SendMessage("EnableBtn");
    //            }
    //            Global.dictTeam = team;
    //        }
    //    }
    //}
    //#endregion

    //public void RemoveTeamMember(string teamName)
    //{
    //    // 移除隊伍成員
    //    Global.dictTeam.Remove(teamName);

    //    _dictLoadedMiceBtnRefs[teamName].GetComponent<TeamSwitcher>().enabled = true; // 要先Active 才能SendMessage
    //    _dictLoadedMiceBtnRefs[teamName].SendMessage("EnableBtn");                    // 啟動按鈕
    //    _dictLoadedTeamBtnRefs.Remove(teamName);                                                        // 移除隊伍參考

    //    Dictionary<string, GameObject> buffer = new Dictionary<string, GameObject>(_dictLoadedTeamBtnRefs);

    //    int i = 0;
    //    foreach (KeyValuePair<string, GameObject> item in buffer)
    //    {
    //        Global.RenameKey(_dictLoadedTeamBtnRefs, item.Key, i.ToString());
    //        i++;
    //    }

    //    i = 0;
    //    foreach (KeyValuePair<string, object> item in Global.dictTeam)
    //    {
    //        Global.RenameKey(_dictLoadedTeamBtnRefs, i.ToString(), item.Key);
    //        i++;
    //    }
    //}

    /// <summary>
    /// 檢查成員變動
    /// </summary>
    /// <param name="serverData">伺服器資料</param>
    /// <param name="clinetData">本資機料</param>
    /// <param name="loadedBtnRefs">已載入物件資料</param>
    /// <returns>true:修正資料完成 false:與伺服器資料相同</returns>
    private bool MemberChk(Dictionary<string, object> serverData, Dictionary<string, object> clinetData, Dictionary<string, GameObject> loadedBtnRefs, Transform parent)
    {
        string key = "";


        if (loadedBtnRefs.Count == serverData.Count)
        {//ok
            // 數量相同時 重新載入入圖檔資料
            if (InstantiateIcon(serverData, loadedBtnRefs, parent))
                return true;

            Debug.Log("MemberChk Same Count");
        }
        else if (serverData.Count > loadedBtnRefs.Count)
        {
            // 新增成員時
            List<string> keys = serverData.Keys.ToList();
            key = keys[serverData.Count - 1];
            Debug.Log("MemberChk Add");
            if (InstantiateIcon(serverData, loadedBtnRefs, parent))
                return true;
        }
        else if (serverData.Count < loadedBtnRefs.Count)
        {
            // 減少成員時
            List<string> keys = loadedBtnRefs.Keys.ToList();
            for (int i = 0; loadedBtnRefs.Count > serverData.Count; i++)
            {
                key = keys[loadedBtnRefs.Count - 1];
                if (loadedBtnRefs.ContainsKey(key) && loadedBtnRefs[key].transform.childCount > 0)
                    Destroy(loadedBtnRefs[key].transform.GetChild(0).gameObject);
                loadedBtnRefs.Remove(key);
            }

            Debug.Log("MemberChk Minus");

            if (loadedBtnRefs.Count != 0)
                if (InstantiateIcon(serverData, loadedBtnRefs, parent))
                    return true;
        }



        // 伺服器與本機 資料不同步時
        if (!DictionaryCompare(serverData, clinetData))
        {
            int i = 0, j = 0;

            Dictionary<string, GameObject> loadedBtnRefsBuffer = new Dictionary<string, GameObject>(loadedBtnRefs);
            Dictionary<string, GameObject> modifyIconBuffer = new Dictionary<string, GameObject>();
            List<string> loadedGameObjectKeys = loadedBtnRefsBuffer.Keys.ToList();
            List<string> serverDataKeys = serverData.Keys.ToList();

            Debug.Log("Server: " + serverData.Count + "    Client: " + loadedBtnRefs.Count);
            foreach (KeyValuePair<string, object> item in serverData)
            {
                key = loadedGameObjectKeys[i];
                if (item.Key.ToString() != key.ToString()) // child out 
                {
                    loadedBtnRefsBuffer[key].transform.GetChild(0).GetComponent<UISprite>().spriteName = item.Value.ToString() + Global.IconSuffix;
                    loadedBtnRefs[key].transform.GetChild(0).name = item.Key;
                    loadedBtnRefsBuffer[key].SendMessage("EnableBtn");
                    Global.RenameKey(loadedBtnRefs, key, "x" + i);
                    j++;
                }
                i++;

                Debug.Log("MemberChk Different");

                if (i == serverData.Count && j == 0)
                    return false;
            }

            loadedBtnRefsBuffer = new Dictionary<string, GameObject>(loadedBtnRefs);
            i = 0;

            foreach (KeyValuePair<string, GameObject> item in loadedBtnRefsBuffer)
            {
                Global.RenameKey(loadedBtnRefs, item.Key, serverDataKeys[i]);
                loadedBtnRefs[serverDataKeys[i]] = parent.GetChild(i).gameObject;
                i++;
            }
        }
        return true;
    }



    ///// <summary>
    ///// 修改物件參考 toID= null 、修改ID newRef= null
    ///// </summary>
    ///// <param name="id"></param>
    ///// <param name="toID"></param>
    ///// <param name="newRef"></param>
    //public void ModifyMiceRefs(string id, string toID, GameObject newRef)
    //{
    //    if (newRef != null)
    //        _dictLoadedMiceBtnRefs[id] = newRef;

    //    if (!string.IsNullOrEmpty(toID))
    //        Global.RenameKey(_dictLoadedMiceBtnRefs, id, toID);
    //}

    //public void ModifyTeamRefs(string id, string toID, GameObject newRef)
    //{
    //    if (newRef != null)
    //        _dictLoadedTeamBtnRefs[id] = newRef;

    //    if (!string.IsNullOrEmpty(toID))
    //        Global.RenameKey(_dictLoadedTeamBtnRefs, id, toID);
    //}


    //public bool AddMiceMemberRefs(string key, GameObject value)
    //{
    //    if (!_dictLoadedMiceBtnRefs.ContainsKey(key))
    //    {
    //        _dictLoadedMiceBtnRefs.Add(key, value);
    //        return true;
    //    }
    //    return false;
    //}

    //public bool AddTeamMemberRefs(string key, GameObject value)
    //{
    //    if (!_dictLoadedTeamBtnRefs.ContainsKey(key))
    //    {
    //        _dictLoadedTeamBtnRefs.Add(key, value);
    //        return true;
    //    }
    //    return false;
    //}

    //public bool RemoveMiceMemberRefs(string key)
    //{
    //    _dictLoadedMiceBtnRefs.Remove(key);
    //    return _dictLoadedMiceBtnRefs.ContainsKey(key) ? true : false;
    //}

    //public bool RemoveTeamMemberRefs(string key)
    //{
    //    _dictLoadedTeamBtnRefs.Remove(key);
    //    return _dictLoadedTeamBtnRefs.ContainsKey(key) ? true : false;
    //}

    private bool DictionaryCompare<TKey, TValue>(Dictionary<TKey, TValue> dict1, Dictionary<TKey, TValue> dict2)
    {
        // early-exit checks
        if (null == dict2)
            return null == dict1;
        if (null == dict1)
            return false;
        if (object.ReferenceEquals(dict1, dict2))
            return true;
        if (dict1.Count != dict2.Count)
            return false;

        int i = 0;
        foreach (KeyValuePair<TKey, TValue> dict1Item in dict1)
        {
            foreach (KeyValuePair<TKey, TValue> dict2Item in dict2)
            {
                int j = 0;
                if (i == j)
                {
                    if (!dict1Item.Key.Equals(dict2Item.Key) || !dict1Item.Value.Equals(dict2Item.Value))
                        return false;
                    break;
                }
                j++;
            }
            i++;
        }

        // check keys are the same
        foreach (TKey k in dict1.Keys)
            if (!dict2.ContainsKey(k))
                return false;

        // check values are the same
        foreach (TKey k in dict1.Keys)
            if (!dict1[k].Equals(dict2[k]))
                return false;

        return true;
    }

    /// <summary>
    /// 檢查Cost
    /// </summary>
    private void OnCostCheck()
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


        if (_miceCost > maxCost)
            infoGroupsArea[1].transform.FindChild("Cost").GetComponent<UILabel>().text = "[FF0000]" + _miceCost + "[-]" + "[14B5DE]/" + maxCost + "[-]";
        else
            infoGroupsArea[1].transform.FindChild("Cost").GetComponent<UILabel>().text = "[14B5DE]" + _miceCost + "/" + maxCost + "[-]";
    }

    void OnDisable()
    {
        Global.photonService.LoadPlayerDataEvent -= OnLoadPlayerData;
        Global.photonService.LoadPlayerItemEvent -= OnLoadPlayerItem;
        Global.photonService.UpdateMiceEvent -= OnCostCheck;
    }







    //private void CheckSolt(){
    //    if (Global.Rank < 5)
    //        infoGroupsArea[2].transform.GetChild(1).gameObject.SetActive(false);
    //    else
    //        infoGroupsArea[2].transform.GetChild(1).gameObject.SetActive(true);

    //    if (Global.Rank < 10)
    //        infoGroupsArea[2].transform.GetChild(2).gameObject.SetActive(false);
    //    else
    //        infoGroupsArea[2].transform.GetChild(2).gameObject.SetActive(true);

    //    if (Global.Rank < 15)
    //        infoGroupsArea[2].transform.GetChild(3).gameObject.SetActive(false);
    //    else
    //        infoGroupsArea[2].transform.GetChild(3).gameObject.SetActive(true);

    //    if (Global.Rank < 20)
    //        infoGroupsArea[2].transform.GetChild(4).gameObject.SetActive(false);
    //    else
    //        infoGroupsArea[2].transform.GetChild(4).gameObject.SetActive(true);
    //}
}