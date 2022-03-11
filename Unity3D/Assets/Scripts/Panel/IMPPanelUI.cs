using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/* ***************************************************************
 * -----Copyright © 2015 Gansol Studio.  All Rights Reserved.-----
 * -----------            CC BY-NC-SA 4.0            -------------
 * -----------  @Website:  EasyUnity@blogspot.com    -------------
 * -----------  @Email:    GansolTW@gmail.com        -------------
 * -----------  @Author:   Krola.                    -------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * IMPPanelUI 為Panel Base類
 * ***************************************************************
 *                           ChangeLog
*  20210109 v1.0.3  修正錯誤
 * 20200926 v1.0.2  scrollView未啟用停止滑動
 * 20161102 v1.0.0  建立    MPPanel  Base類 
 * 20171016 v1.0.1  加入    MPGame
 * ****************************************************************/

public abstract class IMPPanelUI
{
    protected AssetLoaderSystem m_AssetLoaderSystem;                // AssetLoaderSystem索引
    protected GameObject m_RootUI = null;                                          // UIRoot位置

    private static Dictionary<string, PanelState> _dictPanelRefs;     // Panel參考
    private static Dictionary<string, GameObject> _dictActorRefs;// 已載入角色參考
    private static GameObject _lastEmptyItemGroup;                       // 儲存上次開啟的 道具群組 (錯誤，不應該存在繼承類別)

    private ScrollView m_ScrollView;                                                       // 拖曳滑動 應該獨立出來 目前功能關閉了(錯誤)
    private GameObject _activeActor;                                                    // 目前顯示的腳色 (錯誤，不應該存在繼承類別)
    private string _activePanelName;                                                      // 目前開啟的Panel名稱
    private bool _loadedPanel;                                                                  // 是否載入完成Panel

    // 存放Pnael物件狀態類別
    protected class PanelState
    {
        public bool onOff;                             // Panel開關
        public GameObject go;                   // Panel物件
    }

    public IMPPanelUI(MPGame MPGame)
    {
        if (_dictPanelRefs == null)
        {
            _dictPanelRefs = new Dictionary<string, PanelState>();
            _dictActorRefs = new Dictionary<string, GameObject>();
        }
        m_AssetLoaderSystem = MPGame.GetAssetLoaderSystem();
        //scrollView = GameObject.FindGameObjectWithTag("GM").GetComponent<ScrollView>();
    }

    public virtual void Update()
    {
        // 載入Panel完成時
        if (m_AssetLoaderSystem.IsLoadAllAseetCompleted && _loadedPanel)
        {
            _loadedPanel = false;
            InstantiatePanel();                             // 實體化Panel
            PanelSwitch(_activePanelName);  // 切換至目前Panel
        }
    }

    #region -- LoadActor 載入老鼠角色 --
    /// <summary>
    /// 載入老鼠角色
    /// </summary>
    /// <param name="actorBtn_click"></param>
    /// <param name="parent"></param>
    /// <param name="actorScale"></param>
    /// <returns>true:開始載入Asset false:已載入Asset</returns>
    public bool LoadActor(GameObject actorBtn_click, Transform parent, Vector3 actorScale)
    {
        UISprite sprite = actorBtn_click.transform.GetComponentInChildren<UISprite>();     // 取得按扭2DSprite
        string assetName = sprite.spriteName.Replace(Global.IconSuffix, "");                            // 移除icon_前綴詞

        // 如果 未載入老鼠角色了 載入資產
        if (!IsLoadedActor(assetName))
        {
            m_AssetLoaderSystem.LoadAssetFormManifest(Global.MicePath + assetName + "/unique/" + assetName + Global.ext);
            m_AssetLoaderSystem.SetLoadAllAseetCompleted();
            return true;
        }
        // 如果已經載入老鼠角色了 直接顯示 
        ActiveLoadedActor(GetLoadedActor(assetName), parent, actorScale);
        return false;
    }
    #endregion

    #region -- InstantiateActor 實體化老鼠角色 --
    /// <summary>
    /// 實體化老鼠角色
    /// </summary>
    /// <param name="actorName"></param>
    /// <param name="parent"></param>
    /// <param name="actorScale"></param>
    /// <returns></returns>
    public bool InstantiateActor(string actorName, Transform parent, Vector3 actorScale)
    {
        GameObject bundle = m_AssetLoaderSystem.GetAsset(actorName);

        // 如果角色還沒載入 實體化角色
        if (!IsLoadedActor(bundle.name))
        {
            bundle = MPGFactory.GetObjFactory().InstantiateActor(bundle, parent.transform, actorName, actorScale, 500);   // 老鼠Depth是手動輸入的!! 錯誤
            AddLoadedActorRefs(bundle); // 存入已載入角色索引
        }
        // 顯示角色
        ActiveLoadedActor(bundle, parent, actorScale);
        return false;
    }
    #endregion

    #region -- InstantiateItemBG 實體化背包物件背景--
    /// <summary>
    /// 實體化道具物件背景
    /// </summary>
    /// <param name="itemData">資料字典</param>
    /// <param name="itemPanel">實體化父系位置</param>
    /// <param name="itemType">道具類別</param>
    public virtual Dictionary<string, GameObject> InstantiateBagItemBG(Dictionary<string, object> itemData, string itemName, int itemType, Transform itemPanel, Vector2 offset, int tableCount, int rowCount)
    {
        // 取得對應道具類別資料
        if (itemType != -1)
            itemData = MPGFactory.GetObjFactory().GetItemDetailsInfoFromType(itemData, itemType);

        // 如果還沒建立道具 實體化
        if (itemPanel.transform.childCount == 0)                                                            
        {
            _lastEmptyItemGroup =MPGFactory.GetObjFactory(). CreateEmptyObject(itemPanel, itemType);
            return InstantiateItemBGSub(itemData, itemName, itemType, _lastEmptyItemGroup.transform, itemData.Count, offset, tableCount, rowCount);
        }
        else
        {
            // 已建立道具時 如果有對應道具類別
            if (itemPanel.Find(itemType.ToString())) 
            {
                _lastEmptyItemGroup.SetActive(false);
                _lastEmptyItemGroup = itemPanel.Find(itemType.ToString()).gameObject;
                _lastEmptyItemGroup.SetActive(true);
            }
            else if ((_lastEmptyItemGroup != itemPanel.Find(itemType.ToString())))   // 如果沒有對應道具類別資料 建立道具
            {
                _lastEmptyItemGroup.SetActive(false);
                _lastEmptyItemGroup = MPGFactory.GetObjFactory().CreateEmptyObject(itemPanel, itemType);
                return InstantiateItemBGSub(itemData, itemName, itemType, _lastEmptyItemGroup.transform, itemData.Count, offset, tableCount, rowCount);
            }
        }
        return null;
    }

    private Dictionary<string, GameObject> InstantiateItemBGSub(Dictionary<string, object> itemData, string itemName, int itemType, Transform parent, int itemCount, Vector2 offset, int tableCount, int rowCount)
    {
        Vector2 pos = new Vector2();
        Dictionary<string, GameObject> dictItem = new Dictionary<string, GameObject>();
        int count = parent.childCount, i = 0;

        foreach (KeyValuePair<string, object> item in itemData)
        {
            var nestedData = item.Value as Dictionary<string, object>;
            nestedData.TryGetValue("ItemID", out object itemID);

            if (m_AssetLoaderSystem.GetAsset(itemName) != null)                  // 已載入資產時
            {
                pos = SortItemPos(tableCount, rowCount, offset, pos, count + i);
                GameObject itemBundle = m_AssetLoaderSystem.GetAsset(itemName);

                itemBundle = MPGFactory.GetObjFactory().Instantiate(itemBundle, parent, itemID.ToString(), new Vector3(pos.x, pos.y), Vector3.one, Vector2.zero, -1);

                if (itemBundle != null)
                    dictItem.Add(itemID.ToString(), itemBundle);    // 存入道具資料索引
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
    /// <param name="itemCount">全部物件數量</param>
    /// <returns>物件位置</returns>
    protected Vector2 SortItemPos(int xCount, int yCount, Vector2 offset, Vector2 pos, int itemCount)
    {
        // 物件位置排序
        if (itemCount % xCount == 0 && itemCount != 0) // 3 % 9 =0
        {
            pos.x = offset.x * 3;
            pos.y = 0;
        }
        else if (itemCount % yCount == 0 && itemCount != 0)//3 3 =0
        {
            pos.y += offset.y;
            pos.x = 0;
        }
        return pos;
    }
    #endregion

    #region -- LoadIconObjectsAssetByName 載入載入ICON物件 --
    /// <summary>
    /// 載入ICON物件 by BundleName
    /// </summary>
    /// <param name="assetNameData">物件陣列</param>
    /// <param name="folder">資料夾名稱(不含/)</param>
    /// <param name="bKeyOrValue">0=keyName;1=valueName</param>
    public bool LoadIconObjectsAssetByName(List<string> assetNameData, string folder)
    {
        // 載入資產
        if (assetNameData != null)
        {
            foreach (string assetName in assetNameData)
            {
                if (!string.IsNullOrEmpty(assetName))
                    m_AssetLoaderSystem.LoadAssetFormManifest(folder + Global.IconSuffix + assetName.ToLower() + Global.ext);
            }
            m_AssetLoaderSystem.SetLoadAllAseetCompleted();
            return true;
        }
        else
        {
            Debug.Log("LoadIconObject:itemData is Null !!");
            return false;
        }
    }
    #endregion

    #region -- ResumeToggleTarget 復原視窗焦點 --
    /// <summary>
    /// 復原視窗焦點(錯誤，目前只在Menu作用
    /// </summary>
    protected void ResumeToggleTarget()
    {
        EventMaskSwitch.Resume();
        m_RootUI.transform.parent.GetComponentInChildren<AttachBtn_MenuUI>().loadingPanel.SetActive(false); //錯誤 目前只在Menu作用

        EventMaskSwitch.Switch(m_RootUI);
        EventMaskSwitch.LastPanel = m_RootUI;
    }
    #endregion

    #region -- ShowPanel 開啟Panel --
    /// <summary>
    ///  開啟Panel
    /// </summary>
    /// <param name="panelName">Panel名稱，不含(Panel)</param>
    /// <returns>Panel</returns>
    public virtual void ShowPanel(string panelName)
    {
      //  EventMaskSwitch.LastPanel = m_RootUI;
        panelName = panelName.Replace("(Panel)", "");
        _activePanelName = panelName;

        // 開啟LoadingPanel
        GameObject loadingPanel = GameObject.Find(Global.Scene.MainGameAsset.ToString()).GetComponentInChildren<AttachBtn_MenuUI>().loadingPanel;
        loadingPanel.SetActive(true);
        EventMaskSwitch.Switch(loadingPanel);
        EventMaskSwitch.LastPanel = loadingPanel;
        //_lastPanel = loadingPanel;

        // 如果還沒載入Panel AB 載入AB
        if (!_dictPanelRefs.ContainsKey(panelName))
        {
            m_AssetLoaderSystem.LoadAssetFormManifest(Global.PanelUniquePath + panelName.ToLower() + Global.ext);
            m_AssetLoaderSystem.SetLoadAllAseetCompleted();
            _loadedPanel = true;
        }
        else
        {
            PanelSwitch(panelName);     // 已載入AB 顯示Panel
        }
    }
    #endregion

    #region -- PanelSwitch Panel 開/關 --
    /// <summary>
    /// Panel 開/關狀態
    /// </summary>
    /// <param name="panelNo">Panel編號</param>
    private GameObject PanelSwitch(string panelName)
    {
        PanelState panelState = _dictPanelRefs[panelName];      // 取得目前Panel狀態

        if (panelState.go != null && (MPGame.Instance.GetLoginStatus() || !Global.connStatus))
        {
            // 如果Panel是關閉狀態 開啟Panel
            if (!panelState.go.activeSelf)
            {
                Initialize();
                // Debug.Log("Open Panel : " + panelName);
                //scrollView.scroll = false;
                if (EventMaskSwitch.LastPanel != null)
                    EventMaskSwitch.LastPanel.SetActive(false);

                panelState.go.SetActive(true);
                OnLoading();
                panelState.onOff = true; ;
                // _lastPanel = panelState.go;
                EventMaskSwitch.Switch(panelState.go);
                EventMaskSwitch.LastPanel = panelState.go;

                return panelState.go;
            }
            else
            {
                // 如果Panel已開啟 關閉Panel並Realse
                Release();
                //  scrollView.scroll = true;
                EventMaskSwitch.LastPanel.SetActive(false);
                EventMaskSwitch.Resume();
                panelState.go.SetActive(false);
                //_lastPanel = panelState.go;
                panelState.onOff = false;
                //Camera.main.GetComponent<UICamera>().eventReceiverMask = (int)Global.UILayer.Default;
                return panelState.go;
            }
        }
        else
        {
            Debug.LogError("PanelNo unknow or not login !");
            return null;
        }
    }
    #endregion

    #region -- InstantiatePanel 實體化Panel--
    /// <summary>
    /// 實體化Panel現在是正確的，有時間可以重新啟用 很多用編輯器拉進去的Panel都要修改到陣列
    /// </summary>
    protected virtual void InstantiatePanel()
    {
        GameObject bundle = m_AssetLoaderSystem.GetAsset(_activePanelName.ToLower());
        PanelState panelState = new PanelState();

        panelState.go = MPGFactory.GetObjFactory().Instantiate(bundle, m_RootUI.transform, _activePanelName, Vector3.zero, Vector3.one, Vector3.zero, -1);
        panelState.go = panelState.go.transform.parent.gameObject;
        panelState.go.layer = m_RootUI.layer;

        // 加入Panel索引
        if (!IsLoadedPanel(_activePanelName))
            _dictPanelRefs.Add(_activePanelName, panelState);
    }
    #endregion

    #region -- IsLoadedPanel是否載入Panel --
    protected bool IsLoadedPanel(string panelName)
    {
        return _dictPanelRefs.ContainsKey(panelName) ? true : false;
    }
    #endregion

    #region  -- ActiveLoadedActor 顯示已載入的老鼠角色 -- 
    /// <summary>
    /// 顯示 已載入的老鼠角色
    /// </summary>
    /// <param name="actor"></param>
    private void ActiveLoadedActor(GameObject actor, Transform parent, Vector3 scale)
    {
        // 如果有已開啟的 角色 則關閉
        if (_activeActor != null)
            _activeActor.SetActive(false);

        // 如果 有載入的角色 則顯示
        if (IsLoadedActor(actor.name))
            _activeActor = GetLoadedActor(actor.name);

        actor.transform.parent = parent;
        actor.gameObject.layer = parent.gameObject.layer;
        actor.transform.localPosition = Vector3.zero;
        actor.transform.localScale = scale;
        actor.SetActive(true);
        _activeActor = actor;
        _activeActor.SetActive(true);
    }
    #endregion

    #region   --  AddLoadedActorRefs 加入已載入的角色索引 --
    /// <summary>
    /// 加入 已載入的角色索引
    /// </summary>
    /// <param name="actor"></param>
    private void AddLoadedActorRefs(GameObject actor)
    {
        if (!_dictActorRefs.ContainsKey(actor.name))
            _dictActorRefs.Add(actor.name, actor);
    }
    #endregion

    #region   --   GetLoadedActor  取得載入的角色 -- 
    /// <summary>
    /// 取得載入的角色
    /// </summary>
    /// <param name="actorName"></param>
    /// <returns></returns>
    private GameObject GetLoadedActor(string actorName)
    {
        GameObject actor = null;
        if (IsLoadedActor(actorName))
            _dictActorRefs.TryGetValue(actorName, out actor);
        return actor;
    }
    #endregion

    #region   --   IsLoadedActor 是否載入 老鼠角色 --
    /// <summary>
    /// 是否載入 老鼠角色
    /// </summary>
    /// <param name="actorName"></param>
    /// <returns></returns>
    private bool IsLoadedActor(string actorName)
    {
        if (_dictActorRefs.ContainsKey(actorName))
            return true;
        return false;
    }
    #endregion

    #region      -- GetPanelState取得Panel狀態 --
    protected PanelState GetPanelState(GameObject m_RootUI)
    {
        string panelName = m_RootUI.name.Replace("(Panel)", "");
        return _dictPanelRefs[panelName];
    }
    #endregion

    public abstract void Initialize();
    public virtual void OnGUI() { }
    protected abstract void OnLoading();
    protected abstract void OnLoadPanel();
    protected abstract void GetMustLoadAsset();
    protected abstract int GetMustLoadedDataCount();    // 取得需要載入的資料總和
    public abstract void OnClosed(GameObject go);
    public abstract void Release();
}
