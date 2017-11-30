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
    /// <summary>
    /// MiceIcon名稱、Mice按鈕
    /// </summary>
    //public Dictionary<string, GameObject> dictLoadedMiceBtnRefs { get { return dictLoadedMiceBtnRefs.Count; } }
    private Dictionary<string, GameObject> _dictLoadedMiceBtnRefs;             // Icon名稱、Icon的按鈕
    /// <summary>
    /// TeamIcon名稱、Mice按鈕索引物件
    /// </summary>
    //public Dictionary<string, GameObject> dictLoadedTeamBtnRefs { get { return _dictLoadedTeamBtnRefs; } }
    private Dictionary<string, GameObject> _dictLoadedTeamBtnRefs;            // Icon名稱、Icon的按鈕
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
        //_dictTeamData = new Dictionary<string, object>();

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
        if (_bLoadedActor || _bLoadedAsset)                                          // 除錯訊息
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

            InstantiateIcon(Global.dictMiceAll, _dictLoadedMiceBtnRefs, infoGroupsArea[0].transform);
            InstantiateIcon(Global.dictTeam, _dictLoadedTeamBtnRefs, infoGroupsArea[2].transform);

            LoadItemCount(Global.playerItem, infoGroupsArea[0].transform);
            // LoadItemCount(Global.playerItem, infoGroupsArea[2].transform);

            ActiveMice(Global.dictTeam);
            StartCoroutine(OnClickCoroutine(infoGroupsArea[0].transform.GetChild(0).gameObject));   // 顯示老鼠角色 Actor

            ResumeToggleTarget();
            // OnMiceClick(startShowActor);    // 顯示第一隻老鼠
        }

        // 按下圖是按鈕後 載入角色完成時 實體化角色
        if (assetLoader.loadedObj && _bLoadedActor)
        {
            _bLoadedActor = !_bLoadedActor;
            assetLoader.init();
            string bundleName = _btnClick.gameObject.GetComponentInChildren<UISprite>().spriteName.Remove(_btnClick.gameObject.GetComponentInChildren<UISprite>().spriteName.Length - 4);
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
    void InstantiateIcon(Dictionary<string, object> dictServerData, Dictionary<string, GameObject> loadedBtnRefs, Transform myParent)
    {
        int i = 0;
        Dictionary<string, GameObject> tmp = new Dictionary<string, GameObject>();
        List<string> keys = loadedBtnRefs.Keys.ToList();
        foreach (KeyValuePair<string, object> item in dictServerData)
        {
            string bundleName = item.Value.ToString() + "ICON";
            if (assetLoader.GetAsset(bundleName) != null)                  // 已載入資產時
            {
                GameObject bundle = assetLoader.GetAsset(bundleName);

                Transform miceBtn = myParent.Find(myParent.name + (i + 1).ToString());
                if (miceBtn.childCount == 0)
                {
                    MPGFactory.GetObjFactory().Instantiate(bundle, miceBtn, item.Key, Vector3.zero, Vector3.one, new Vector2(65, 65), -1);

                    Add2Refs(loadedBtnRefs, i, item.Key, miceBtn);     // 加入物件參考

                    miceBtn.GetComponent<TeamSwitcher>().enabled = true;           // 開啟老鼠隊伍交換功能
                    miceBtn.GetComponent<TeamSwitcher>().SendMessage("EnableBtn"); // 開啟按鈕功能
                }
                else if (item.Key.ToString() != keys[i])
                {
                    //string imageName = miceBtn.GetComponentInChildren<UISprite>().gameObject.name;

                    tmp.Add(item.Key, miceBtn.gameObject);
                    // 檢查 重複Key 如果有先改X 在改回
                    if (!loadedBtnRefs.ContainsKey(item.Key.ToString()))
                    {
                        Global.RenameKey(loadedBtnRefs, keys[i], item.Key);
                        miceBtn.GetComponentInChildren<UISprite>().gameObject.name = item.Key;
                        miceBtn.GetComponentInChildren<UISprite>().spriteName = bundleName;
                    }
                    else
                    {
                        Debug.LogError("Same Key!");
                    }
                }
                i++;
            }
            else
            {
                Debug.LogError("Assetbundle reference not set to an instance. at InstantiateIcon (Line:154).");
            }
        }
        _bLoadedAsset = false; // 實體化完成 關閉載入
    }
    #endregion

    #region -- InstantiateItem 實體化道具 --
    /// <summary>
    /// 實體化道具
    /// </summary>
    /// <param name="parent">物件位置</param>
    /// <param name="objectID">ID</param>
    private void InstantiateItem(Transform parent, string objectID)
    {
        int itemID = Convert.ToInt16(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.miceProperty, "ItemID", objectID.ToString()));
        string bundleName = MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.itemProperty, "ItemName", itemID.ToString()).ToString() + "ICON";
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

    public void OnClosed(GameObject obj)
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
    void Add2Refs(Dictionary<string, GameObject> loadedBtnRefs, int position, string itemID, Transform myParent)
    {

        List<string> keys = loadedBtnRefs.Keys.ToList();
        Dictionary<string, string> itemPositionRefs = new Dictionary<string, string>();
        int i = 0;
        string key = "";

        foreach (var item in loadedBtnRefs)
        {
            itemPositionRefs.Add(i.ToString(), item.Key.ToString());
            i++;
        }

        if (!loadedBtnRefs.ContainsKey(itemID))
        {
            loadedBtnRefs.Add(itemID, myParent.gameObject);          // 加入索引 老鼠所在的MiceBtn位置
        }
        else
        {
            // 如果存在索引 移除已存在索引 加入新索引
            if (itemPositionRefs.ContainsValue(itemID))
            {
                loadedBtnRefs.Remove(itemID);
                if (position > loadedBtnRefs.Count)
                {
                    loadedBtnRefs.Add(itemID, myParent.gameObject);
                }
                else
                {
                    key = itemPositionRefs[(position - 1).ToString()];
                    loadedBtnRefs[key] = myParent.gameObject;
                    Global.RenameKey(loadedBtnRefs, key, itemID);
                }
            }
            else
            {
                loadedBtnRefs.Add(itemID, myParent.gameObject);
            }



            //Debug.Log("----------------------------------------------------------");
            //Debug.Log("   Error LoadedBtnRefs has Key:" + itemID);
            //Debug.Log("----------------------------------------------------------");
            //foreach (KeyValuePair<string, GameObject> item in dictLoadedTeamBtnRefs)
            //{
            //    Debug.Log("Key:" + item.Key + "  Value:" + item.Value);
            //}
        }
    }
    #endregion

    #region -- SwitchMemeber 交換隊伍成員 --
    /// <summary>
    /// 交換隊伍成員
    /// </summary>
    /// <param name="key1">要交換的物件Key值</param>
    /// <param name="key2">要交換的物件Key值</param>
    /// <param name="data">來源資料字典</param>
    public void SwitchMemeber(string key1, string key2, Dictionary<string, object> dict)
    {
        Dictionary<string, GameObject> tmpDict = _dictLoadedMiceBtnRefs;

        if (dict == Global.dictTeam)
            tmpDict = _dictLoadedTeamBtnRefs;
        Global.SwapDictValueByKey(key1, key2, dict);
        Global.SwapDictKey(key1, key2, "x", dict);


        GameObject tmpBtn = tmpDict[key1];
        tmpDict[key1] = tmpDict[key2];

        Global.SwapDictKey(key1, key2, "x", tmpDict);
        //Global.RenameKey(tmpDict, key1, "x");
        //Global.RenameKey(tmpDict, key2, key1);
        //Global.RenameKey(tmpDict, "x", key2);
    }
    #endregion

    #region -- TeamSequence 隊伍整理佇列 --
    /// <summary>
    /// 隊伍整理佇列 bDragOut=True : 拉出隊伍整理   bDragOut=False : 拉入隊伍整理
    /// </summary>
    /// <param name="btnRefs">受影響的按鈕</param>
    /// <param name="bDragOut">拉入T 或 移出F</param>
    public void TeamSequence(GameObject btnRefs, bool bDragOut)
    {
        Dictionary<string, object> team = new Dictionary<string, object>(Global.dictTeam);
        int btnNo = int.Parse(btnRefs.name.Remove(0, btnRefs.name.Length - 1));
        // string miceName = teamRef.transform.GetChild(0).name;

        if (bDragOut)
        {
            if (btnNo >= team.Count)
            {
                int offset;
                offset = team.Count == 0 ? 0 : 1;
                btnRefs.transform.GetChild(0).parent = infoGroupsArea[2].transform.GetChild(team.Count - offset);
                infoGroupsArea[2].transform.GetChild(team.Count - offset).GetChild(0).localPosition = Vector3.zero;
                infoGroupsArea[2].transform.GetChild(team.Count - offset).GetComponent<TeamSwitcher>().enabled = true;
                infoGroupsArea[2].transform.GetChild(team.Count - offset).GetComponent<TeamSwitcher>().SendMessage("EnableBtn");
            }
        }
        else
        {
            if (btnNo < team.Count)                                           // 2<5
            {
                for (int i = 0; i < (team.Count - btnNo); i++)                  // 5-2=3  
                {
                    GameObject teamBtn;
                    if (_dictLoadedTeamBtnRefs.TryGetValue(infoGroupsArea[2].transform.GetChild(btnNo + i).GetChild(0).name, out teamBtn))
                    {
                        _dictLoadedTeamBtnRefs[infoGroupsArea[2].transform.GetChild(btnNo + i).GetChild(0).name] = infoGroupsArea[2].transform.GetChild(btnNo + i - 1).gameObject;
                    }
                    infoGroupsArea[2].transform.GetChild(btnNo + i).GetChild(0).parent = infoGroupsArea[2].transform.GetChild(btnNo + i - 1); // team[2+i]=team[2+i-1] parent =>team[2]=team[1]



                    if (i == 0)     // 因為第一個物件會有2個Child所以要GetChild(1) 下一個按鈕移過來的Mice
                        infoGroupsArea[2].transform.GetChild(btnNo + i - 1).GetChild(1).localPosition = Vector3.zero;
                    else
                        infoGroupsArea[2].transform.GetChild(btnNo + i - 1).GetChild(0).localPosition = Vector3.zero;
                    infoGroupsArea[2].transform.GetChild(btnNo + i - 1).GetChild(0).localPosition = Vector3.zero;
                    infoGroupsArea[2].transform.GetChild(btnNo + i - 1).GetComponent<TeamSwitcher>().enabled = true;
                    infoGroupsArea[2].transform.GetChild(btnNo + i - 1).GetComponent<TeamSwitcher>().SendMessage("EnableBtn");
                }
                Global.dictTeam = team;
            }
        }
    }
    #endregion

    public void RemoveTeamMember(string teamName)
    {
        // 移除隊伍成員
        Global.dictTeam.Remove(teamName);

        _dictLoadedMiceBtnRefs[teamName].GetComponent<TeamSwitcher>().enabled = true; // 要先Active 才能SendMessage
        _dictLoadedMiceBtnRefs[teamName].SendMessage("EnableBtn");                    // 啟動按鈕
        _dictLoadedTeamBtnRefs.Remove(teamName);                                                        // 移除隊伍參考

        Dictionary<string, GameObject> buffer = new Dictionary<string, GameObject>(_dictLoadedTeamBtnRefs);

        int i = 0;
        foreach (KeyValuePair<string, GameObject> item in buffer)
        {
            Global.RenameKey(_dictLoadedTeamBtnRefs, item.Key, i.ToString());
            i++;
        }

        i = 0;
        foreach (KeyValuePair<string, object> item in Global.dictTeam)
        {
            Global.RenameKey(_dictLoadedTeamBtnRefs, i.ToString(), item.Key);
            i++;
        }
    }

    #region --字典 檢查/取值 片段程式碼 --

    public GameObject GetLoadedMice(string miceID)
    {
        GameObject obj;
        if (_dictLoadedMiceBtnRefs.TryGetValue(miceID, out obj))
            return obj;
        return null;
    }

    public GameObject GetLoadedTeam(string miceID)
    {
        GameObject obj;
        if (_dictLoadedTeamBtnRefs.TryGetValue(miceID, out obj))
            return obj;
        return null;
    }
    #endregion

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
            InstantiateIcon(serverData, loadedBtnRefs, parent);
            clinetData = serverData;
            Debug.Log("MemberChk Same");
            return true;
        }
        else if (serverData.Count > loadedBtnRefs.Count)
        {
            // 新增成員時
            List<string> keys = serverData.Keys.ToList();
            key = keys[serverData.Count - 1];
            clinetData.Add("x" + key, serverData[key]);
            InstantiateIcon(serverData, loadedBtnRefs, parent);

            clinetData = serverData;
            Debug.Log("MemberChk Add");
            return true;
        }
        else if (serverData.Count < loadedBtnRefs.Count)
        {
            // 減少成員時
            List<string> keys = loadedBtnRefs.Keys.ToList();
            for (int i = 0; loadedBtnRefs.Count > serverData.Count; i++)
            {
                key = keys[clinetData.Count - 1];
                clinetData.Remove(key);
                Destroy(loadedBtnRefs[key].transform.GetChild(0).gameObject);
                loadedBtnRefs.Remove(key);
            }
            Debug.Log("MemberChk Minus");
            if (loadedBtnRefs.Count == 0)
                return true;
            else
                InstantiateIcon(serverData, loadedBtnRefs, parent);
            clinetData = serverData;
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
                if (item.Key != loadedBtnRefsBuffer[key].transform.GetChild(0).name) // child out 
                {
                    loadedBtnRefsBuffer[key].transform.GetChild(0).GetComponent<UISprite>().spriteName = item.Value.ToString() + "ICON";
                    loadedBtnRefs[key].transform.GetChild(0).name = item.Key;
                    loadedBtnRefsBuffer[key].transform.GetComponent<TeamSwitcher>().EnableBtn();
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
                i++;
            }
            clinetData = serverData;
        }
        return true;
    }

    public int GetMiceMemberCount()
    {
        return _dictLoadedMiceBtnRefs.Count;
    }

    public int GetTeamMemberCount()
    {
        return _dictLoadedTeamBtnRefs.Count;
    }

    /// <summary>
    /// 修改物件參考 toID= null 、修改ID newRef= null
    /// </summary>
    /// <param name="id"></param>
    /// <param name="toID"></param>
    /// <param name="newRef"></param>
    public void ModifyMiceRefs(string id, string toID, GameObject newRef)
    {
        if (newRef != null)
            _dictLoadedMiceBtnRefs[id] = newRef;

        if (!string.IsNullOrEmpty(toID))
            Global.RenameKey(_dictLoadedMiceBtnRefs, id, toID);
    }

    public void ModifyTeamRefs(string id, string toID, GameObject newRef)
    {
        if (newRef != null)
            _dictLoadedTeamBtnRefs[id] = newRef;

        if (!string.IsNullOrEmpty(toID))
            Global.RenameKey(_dictLoadedTeamBtnRefs, id, toID);
    }


    public bool AddMiceMemberRefs(string key, GameObject value)
    {
        if (!_dictLoadedMiceBtnRefs.ContainsKey(key))
        {
            _dictLoadedMiceBtnRefs.Add(key, value);
            return true;
        }
        return false;
    }

    public bool AddTeamMemberRefs(string key, GameObject value)
    {
        if (!_dictLoadedTeamBtnRefs.ContainsKey(key))
        {
            _dictLoadedTeamBtnRefs.Add(key, value);
            return true;
        }
        return false;
    }

    public bool RemoveMiceMemberRefs(string key)
    {
        _dictLoadedMiceBtnRefs.Remove(key);
        return _dictLoadedMiceBtnRefs.ContainsKey(key) ? true : false;
    }

    public bool RemoveTeamMemberRefs(string key)
    {
        _dictLoadedTeamBtnRefs.Remove(key);
        return _dictLoadedTeamBtnRefs.ContainsKey(key) ? true : false;
    }

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

    void OnDisable()
    {
        Global.photonService.LoadPlayerDataEvent -= OnLoadPlayerData;
        Global.photonService.LoadPlayerItemEvent -= OnLoadPlayerItem;
        Global.photonService.UpdateMiceEvent += OnCostCheck;
    }
}