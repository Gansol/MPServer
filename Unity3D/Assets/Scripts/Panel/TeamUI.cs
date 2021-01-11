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
 * 20210109 v3.0.1  移除多餘程式碼  
 * 20201027 v3.0.0  繼承重構
 * 20171213 v1.1.3   交換按鈕功能合併修改至SwitchBtnComponent      
 * 20171211 v1.1.2   修正索引問題                  
 * 20171201 v1.1.1   修正隊伍交換問題         
 * 20171119 v1.1.0   修正載入流程 
 * 20161102 v1.0.3   3次重構，改變繼承至 PanelManager>MPPanel
 * 20160914 v1.0.2   2次重構，獨立實體化物件  
 * 20160711 v1.0.1   1次重構，獨立AssetLoader                       
 * 20160705 v1.0.0   0版完成，載入老鼠部分未來需要修改                    
 * ****************************************************************/

public class TeamUI : IMPPanelUI
{
    #region Variables 變數
    AttachBtn_TeamUI UI;
    SwitchBtnComponent switchBtnMethod;

    private static Dictionary<string, object> _dictMiceData;                        // Json老鼠資料(檢查老鼠變動使用)
    private static Dictionary<string, object> _dictTeamData;                       // Json隊伍資料(檢查老鼠變動使用)

    private Dictionary<string, GameObject> _dictLoadedMiceBtnRefs;  // 已載入的按鈕(全部老鼠)
    private Dictionary<string, GameObject> _dictLoadedTeamBtnRefs; // 已載入的按鈕(隊伍老鼠)
    private GameObject _btnClick;                    // 按下按鈕
    private GameObject _doubleClickChk;      // 雙擊檢查
    private Vector2 _actorScale;                         // 角色縮放

    private int _miceCost;                                    // 老鼠Cost
    private int _dataLoadedCount;                    // 資料載入數量
    private int _page;                                             // 翻頁值(翻一頁+10)
    private float _lastClickTime;                          // 點擊間距時間
    private float _delayBetween2Clicks;          // 雙擊間隔時間
    private bool _bFirstLoad;                              // 是否 第一次載入 
    private bool _bLoadedPanel;                       // 是否 載入Panel
    private bool _bLoadedAsset;                       // 是否 載入資產
    private bool _bLoadedActor;                       // 是否 載入角色
    private bool _bLoadedEffect;                       // 是否 載入特效
    #endregion

    public TeamUI(MPGame MPGame) : base(MPGame)
    {
        Debug.Log("--------------- TeamUI Create ----------------");

        switchBtnMethod = new SwitchBtnComponent();
        _dictLoadedMiceBtnRefs = new Dictionary<string, GameObject>();
        _dictLoadedTeamBtnRefs = new Dictionary<string, GameObject>();

        _delayBetween2Clicks = 0.3f;
        _bFirstLoad = true;
        //_page = 0;
    }

    public override void Initialize()
    {
        Debug.Log("--------------- TeamUI Initialize ----------------");

        _actorScale = new Vector3(0.8f, 0.8f, 1);
        _bLoadedPanel = false;
        Global.photonService.LoadPlayerDataEvent += OnLoadPlayerData;
        Global.photonService.LoadPlayerItemEvent += OnLoadPlayerItem;
        Global.photonService.UpdateMiceEvent += OnCostCheck;
    }

    public override void Update()
    {
        base.Update();
        // 除錯訊息
        if (_bLoadedActor || _bLoadedAsset)
            if (!string.IsNullOrEmpty(m_AssetLoaderSystem.ReturnMessage))
                Debug.Log("訊息：" + m_AssetLoaderSystem.ReturnMessage);

        #region // 資料庫資料載入完成時 載入資產
        if (_dataLoadedCount == GetMustLoadedDataCount() && !_bLoadedPanel)
        {
            _bLoadedPanel = true;
            GetMustLoadAsset();
        }
        #endregion

        #region // 載入資產完成後 實體化 物件
        if (m_AssetLoaderSystem.IsLoadAllAseetCompleted && _bLoadedAsset /*&& _bLoadedEffect*/)    // 可以使用了 只要畫SkillICON 並修改載入SkillICON
        {
            _bLoadedAsset = false;
            _bLoadedEffect = false;
            OnLoadPanel();
            OnCostCheck();
            LoadItemCount(Global.playerItem, UI.miceGroup.transform);
            ResumeToggleTarget();
        }
        #endregion

        #region  // 載入角色完成時 實體化角色
        if (m_AssetLoaderSystem.IsLoadAllAseetCompleted && _bLoadedActor)
        {
            m_AssetLoaderSystem.Initialize();
            _bLoadedActor = false;
            Debug.Log("CC");
            string actorName = _btnClick.gameObject.GetComponentInChildren<UISprite>().spriteName.Replace(Global.IconSuffix, "");
            InstantiateActor(actorName, UI.miceImage.transform, _actorScale);
        }
        #endregion
    }

    #region -- OnMiceClick 當按下老鼠時 --
    public void OnMiceClick(GameObject btn_mice)
    {
        // 當點擊2下時 交換老鼠
        if (Time.time - _lastClickTime < _delayBetween2Clicks && _doubleClickChk == btn_mice)    // Double Click
            btn_mice.SendMessage("Mice2Click");
        // 點擊1下時 顯示老鼠角色
        else
            MPGame.Instance.StartCoroutine(OnClickActorCoroutine(btn_mice));

        _lastClickTime = Time.time;
        _doubleClickChk = btn_mice; // 儲存上次點擊的按扭
    }

    IEnumerator OnClickActorCoroutine(GameObject miceBtn)   //Single Click
    {
        yield return new WaitForSeconds(_delayBetween2Clicks);
        if (Time.time - _lastClickTime < _delayBetween2Clicks)
            yield break;
        if (miceBtn.GetComponentInChildren<UISprite>())
        {
            _bLoadedActor = LoadActor(miceBtn, UI.miceImage.transform, _actorScale);
            _btnClick = miceBtn;

            LoadProperty.LoadItemProperty(miceBtn.GetComponentInChildren<UISprite>().gameObject, UI.infoGroup, Global.miceProperty, (int)StoreType.Mice);    // 載入老鼠屬性   
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

        SortChildren.SortChildrenByID(parent.gameObject, Global.extIconLength);

        for (int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i).GetComponentInChildren<UISprite>() != null)
                if (data.TryGetValue(parent.GetChild(i).GetComponentInChildren<UISprite>().name, out miceData))
                {
                    Dictionary<string, object> miceProp = miceData as Dictionary<string, object>;
                    parent.parent.Find("ItemCount").GetChild(i).GetComponentInChildren<UILabel>().text = miceProp["ItemCount"].ToString();
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

        UI.rank.text = rank.ToString();
        UI.expSlider.value = Convert.ToSingle(exp) / miceMaxExp;
        UI.exp.text = exp.ToString() + " / " + miceMaxExp.ToString();
    }
    #endregion

    #region -- InstantiateIcon 實體化老鼠圖示物件--
    /// <summary>
    /// 實體化載入完成的遊戲物件，利用玩家JSON資料判斷必要實體物件
    /// </summary>
    /// <param name="dictServerData">ServerData</param>
    /// <param name="dictLoadedObject">Client Loaded Object</param>
    /// <param name="myParent">實體化父系位置</param>
    /// <returns>true=ok false=有重複Key沒有實體化完成</returns>
    bool InstantiateIcon(Dictionary<string, object> dictServerData, Dictionary<string, GameObject> loadedBtnRefs, Transform myParent)
    {
        Dictionary<string, GameObject> tmp = new Dictionary<string, GameObject>();
        List<string> keys = loadedBtnRefs.Keys.ToList();
        int i = 0;

        foreach (KeyValuePair<string, object> item in dictServerData)
        {
            string bundleName = Global.IconSuffix + item.Value.ToString();

            if (m_AssetLoaderSystem.GetAsset(bundleName) != null)                                   // 已載入資產時
            {
                GameObject bundle = m_AssetLoaderSystem.GetAsset(bundleName);
                Transform miceBtn = myParent.Find(myParent.name + (i + 1).ToString());
                // 如果 按鈕下 沒有物件 實體化物件
                if (miceBtn.childCount == 0)
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
                UIEventListener.Get(miceBtn.gameObject).onClick = OnMiceClick;
                //Debug.Log("Btn: " + miceBtn.parent.parent.parent.name + "   " + miceBtn.parent.parent.parent.parent.name);

                switchBtnMethod.Add2Refs(loadedBtnRefs, _dictLoadedMiceBtnRefs, _dictLoadedTeamBtnRefs, i, item.Key, miceBtn.gameObject);                   // 加入物件參考
                i++;
            }
            else
            {
                Debug.LogError("Assetbundle reference not set to an instance. at InstantiateIcon (Line:154).");
                return false;
            }
        }
        _bLoadedAsset = false; // 實體化完成 關閉載入
        return true;
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
        GameObject bundle = m_AssetLoaderSystem.GetAsset(bundleName);

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
        UI = m_RootUI.GetComponentInChildren<AttachBtn_TeamUI>();
        UIEventListener.Get(UI.closeCollider).onClick = OnClosed;

        _dataLoadedCount = (int)ENUM_Data.None;

        if (Global.isMatching)
            Global.photonService.ExitWaitingRoom();

        Global.photonService.LoadPlayerData(Global.Account);
        Global.photonService.LoadPlayerItem(Global.Account);
        EventMaskSwitch.LastPanel = m_RootUI.transform.GetChild(0).gameObject;
    }

    protected override void OnLoadPanel()
    {
        if (m_RootUI.activeSelf)
        {
            // 實體化按鈕  如果第一載入，不檢查老鼠變動
            if (!switchBtnMethod.MemberChk(Global.dictMiceAll, _dictMiceData, _dictLoadedMiceBtnRefs, UI.miceGroup) || _bFirstLoad)
                InstantiateIcon(Global.dictMiceAll, _dictLoadedMiceBtnRefs, UI.miceGroup);
            // 實體化按鈕  如果第一載入，不檢查隊伍變動
            if (!switchBtnMethod.MemberChk(Global.dictTeam, _dictTeamData, _dictLoadedTeamBtnRefs, UI.teamGroup) || _bFirstLoad)
                InstantiateIcon(Global.dictTeam, _dictLoadedTeamBtnRefs, UI.teamGroup);

            switchBtnMethod.ActiveMice(Global.dictTeam, _dictLoadedMiceBtnRefs);                // Enable按鈕
            MPGame.Instance.StartCoroutine(OnClickActorCoroutine(UI.startShowActor));    // 顯示老鼠角色 Actor

            _bLoadedPanel = true;
            _bFirstLoad = false;
            _dictMiceData = Global.dictMiceAll; // 保存老鼠資料
            _dictTeamData = Global.dictTeam;    // 保存隊伍資料
        }
    }

    void OnLoadPlayerData()
    {
        _dataLoadedCount *= (int)ENUM_Data.PlayerData;
    }
    void OnLoadPlayerItem()
    {
        _dataLoadedCount *= (int)ENUM_Data.PlayerItem;
    }
    #endregion

    #region -- GetMustLoadAsset 載入必要資產 --
    // 載入必要資產
    protected override void GetMustLoadAsset()
    {

        List<string> dictNotLoadedAsset;

        // 如果是第一次載入 載入全部資產 否則 載入必要資產
        if (_bFirstLoad)
        {
            dictNotLoadedAsset = GetDontNotLoadAssetName(Global.dictMiceAll);
            _dictMiceData = Global.dictMiceAll;
            _dictTeamData = Global.dictTeam;
        }
        else
        {
            // 移除過舊資料
            LoadProperty.ExpectOutdataObject(Global.dictMiceAll, _dictMiceData, _dictLoadedMiceBtnRefs);
            LoadProperty.ExpectOutdataObject(Global.dictTeam, _dictMiceData, _dictLoadedTeamBtnRefs);
            // Where 找不存在的KEY 再轉換為Dictionary
            Dictionary<string, object> newAssetData = Global.dictMiceAll.Where(kvp => !_dictMiceData.ContainsKey(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            dictNotLoadedAsset = GetDontNotLoadAssetName(newAssetData);
        }
        // 如果 有未載入物件 載入AB
        if (dictNotLoadedAsset.Count > 0)
        {
            // _bLoadedEffect = LoadEffectAsset(dictNotLoadedAsset);    // 可以使用了 只要畫SkillICON 並修改載入SkillICON
            _bLoadedAsset = LoadIconObjectsAssetByName(dictNotLoadedAsset, Global.MiceIconUniquePath);
        }
        else
        {
            // 已載入物件 實體化
            _bLoadedAsset = true;
        }
    }
    #endregion

    #region -- OnCostCheck 檢查Cost --
    /// <summary>
    /// 檢查Cost
    /// </summary>
    private void OnCostCheck()
    {
        int maxCost = Clac.ClacCost(Global.Rank);
        _miceCost = 0;
        Dictionary<string, object> data;
        foreach (KeyValuePair<string, object> mice in Global.dictTeam)
        {
            data = Global.miceProperty[mice.Key] as Dictionary<string, object>;
            data.TryGetValue("MiceCost", out object value);
            _miceCost += Convert.ToInt32(value);
        }

        if (_miceCost > maxCost)
            UI.cost.text = "[FF0000]" + _miceCost + "[-]" + "[14B5DE]/" + maxCost + "[-]";
        else
            UI.cost.text = "[14B5DE]" + _miceCost + "/" + maxCost + "[-]";
    }
    #endregion

    #region --  ShowPane 顯示Panel -- 
    public override void ShowPanel(string panelName)
    {
        m_RootUI = GameObject.Find(Global.Scene.MainGameAsset.ToString()).GetComponentInChildren<AttachBtn_MenuUI>().teamPanel;
        base.ShowPanel(panelName);
    }
    #endregion

    #region -- GetMustLoadedDataCount 計算載入資料數量 -- 
    protected override int GetMustLoadedDataCount()
    {
        return (int)ENUM_Data.PlayerItem * (int)ENUM_Data.PlayerData;
    }
    #endregion

    #region --  OnClosed   --
    public override void OnClosed(GameObject go)
    {
        ShowPanel(m_RootUI.name);
        // EventMaskSwitch.LastPanel = null;
        //EventMaskSwitch.Prev(1);
    }
    #endregion

    #region --  Release -- 
    public override void Release()
    {
        Global.photonService.LoadPlayerDataEvent -= OnLoadPlayerData;
        Global.photonService.LoadPlayerItemEvent -= OnLoadPlayerItem;
        Global.photonService.UpdateMiceEvent -= OnCostCheck;
    }
    #endregion





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
    //#region -- InstantiateItem 實體化道具(按鈕) --
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
}