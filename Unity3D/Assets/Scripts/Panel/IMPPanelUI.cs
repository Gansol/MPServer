using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using MPProtocol;

/* ***************************************************************
 * -----Copyright © 2015 Gansol Studio.  All Rights Reserved.-----
 * -----------            CC BY-NC-SA 4.0            -------------
 * -----------  @Website:  EasyUnity@blogspot.com    -------------
 * -----------  @Email:    GansolTW@gmail.com        -------------
 * -----------  @Author:   Krola.                    -------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * MPPanel 為Panel Base類
 * ***************************************************************
 *                           ChangeLog
 * 20200926 v1.0.2  scrollView未啟用停止滑動
 * 20161102 v1.0.0  建立    MPPanel  Base類 
 * 20171016 v1.0.1  加入    MPGame
 * ****************************************************************/

public abstract class IMPPanelUI
{
    //  private AttachBtn_MenuUI UI;
    private ScrollView scrollView;
    protected static AssetLoader assetLoader;
    protected MPGame m_MPGame = null;

    protected static Dictionary<string, PanelState> dictPanelRefs;    // Panel參考
    private static GameObject[] PanelRefs;
    public static GameObject _lastEmptyItemGroup;
    private static Dictionary<string, GameObject> _dictActor = new Dictionary<string, GameObject>(); // 已載入角色參考
    private GameObject _tmpActor, _clone;
    // private static GameObject _lastPanel;
    protected GameObject m_RootUI = null;

    protected string activePanelName;
    private bool _loadedPanel, m_bActive = true;
    private int _panelNo;

    public IMPPanelUI(MPGame MPGame)
    {
        m_MPGame = MPGame;
        //UI = GameObject.Find(Global.Scene.MainGameAsset.ToString()).GetComponent<AttachBtn_MenuUI>();
        if (dictPanelRefs == null)
            dictPanelRefs = new Dictionary<string, PanelState>();
        assetLoader = m_MPGame.GetAssetLoader();
        assetLoader.init();

        //scrollView = GameObject.FindGameObjectWithTag("GM").GetComponent<ScrollView>();
        Debug.Log("MPPanel init!");
    }

    #region
    protected class PanelState                                         // 存放Pnael物件 狀態
    {
        public bool onOff;                                          // Panel開關
        public GameObject obj;                                      // Panel物件
    }
    #endregion

    public abstract void Initinal();

    public virtual void OnGUI() { }

    public virtual void Update()
    {
        if (m_MPGame.GetAssetLoader().bLoadedObj && _loadedPanel)                 // 載入Panel完成時
        {
            _loadedPanel = !_loadedPanel;
            InstantiatePanel();
            PanelSwitch(activePanelName);
        }
    }
    public abstract void Release();

    #region -- LoadActor 載入老鼠角色 --
    /// <summary>
    /// 
    /// </summary>
    /// <param name="btn_click"></param>
    /// <param name="parent"></param>
    /// <param name="scale"></param>
    /// <returns>true:開始載入Asset false:已載入Asset</returns>
    public bool LoadActor(GameObject btn_click, Transform parent, Vector3 scale)
    {
        UISprite sprite = btn_click.transform.GetComponentInChildren<UISprite>();
        string assetName = sprite.spriteName.Replace(Global.IconSuffix, ""); // 移除icon_前綴詞
        //        Debug.Log(_dictActor.Count);
        //if (tmpActor != null) tmpActor.SetActive(false);          // 如果暫存老鼠圖片不是空的(防止第一次點擊出錯)，將上一個老鼠圖片隱藏

        if (_dictActor.TryGetValue(assetName, out GameObject _miceImage))       // 假如已經載入老鼠圖片了 直接顯示
        {
            if (_tmpActor != null) _tmpActor.SetActive(false);
            _miceImage.transform.parent = parent;
            _miceImage.gameObject.layer = parent.gameObject.layer;
            _miceImage.transform.localPosition = Vector3.zero;
            _miceImage.transform.localScale = scale;
            _miceImage.SetActive(true);
            _tmpActor = _miceImage;
            return false;
        }
        else
        {

            assetLoader.LoadAssetFormManifest(Global.MicePath + assetName + "/unique/" + assetName + Global.ext);
            //assetLoader.LoadAsset(assetName + "/", assetName);
            //assetLoader.LoadPrefab(assetName + "/", assetName);
            return true;
        }
    }
    #endregion

    #region -- InstantiateActor 實體化老鼠角色 --
    public bool InstantiateActor(string actorName, Transform parent, Vector3 scale)
    {
        GameObject bundle = (GameObject)assetLoader.GetAsset(actorName);

        if (bundle != null)                  // 已載入資產時
        {
            if (!IsLoadedActor(bundle.name))
            {
                SetLoadedActor(MPGFactory.GetObjFactory().InstantiateActor(bundle, parent.transform, actorName, scale, 500)); // 老鼠Depth是手動輸入的!! 錯誤
            }
            else
            {
                bundle.name = actorName;
                SetLoadedActor(bundle);
            }
            //else
            //    _clone = GetLoadedActor(bundle.name);
            return false;
        }
        else
        {
            Debug.LogError("Assetbundle reference not set to an instance. at InstantiateActor.");
            return true;
        }
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
        if (itemType != -1)
            itemData = MPGFactory.GetObjFactory().GetItemDetailsInfoFromType(itemData, itemType);     // 取得對應道具類別資料

        //   itemName = AssetBundleManager.GetAssetBundleNamePath(itemName);

        if (itemPanel.transform.childCount == 0)                // 如果還沒建立道具
        {
            _lastEmptyItemGroup = CreateEmptyObject(itemPanel, itemType);
            return InstantiateItemBGSub(itemData, itemName, itemType, _lastEmptyItemGroup.transform, itemData.Count, offset, tableCount, rowCount);
        }
        else
        {// 已建立道具時

            if (itemPanel.Find(itemType.ToString()))       // 如果有對應道具類別
            {
                _lastEmptyItemGroup.SetActive(false);
                _lastEmptyItemGroup = itemPanel.Find(itemType.ToString()).gameObject;
                _lastEmptyItemGroup.SetActive(true);                                                  // 如果沒有對應道具類別資料 建立道具
            }
            else if ((_lastEmptyItemGroup != itemPanel.Find(itemType.ToString())))
            {

                _lastEmptyItemGroup.SetActive(false);
                _lastEmptyItemGroup = CreateEmptyObject(itemPanel, itemType);
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
            object itemID;
            nestedData.TryGetValue("ItemID", out itemID);

            if (assetLoader.GetAsset(itemName) != null)                  // 已載入資產時
            {
                pos = sortItemPos(tableCount, rowCount, offset, pos, count + i);
                GameObject itemBundle = assetLoader.GetAsset(itemName);

                itemBundle = MPGFactory.GetObjFactory().Instantiate(itemBundle, parent, itemID.ToString(), new Vector3(pos.x, pos.y), Vector3.one, Vector2.zero, -1);

                if (itemBundle != null) dictItem.Add(itemID.ToString(), itemBundle);    // 存入道具資料索引
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

    #region -- LoadIconObject 載入載入ICON物件 --
    /// <summary>
    /// 載入ICON物件 by BundleName
    /// </summary>
    /// <param name="itemData">物件陣列</param>
    /// <param name="folder">資料夾名稱(不含/)</param>
    /// <param name="bKeyOrValue">0=keyName;1=valueName</param>
    public bool LoadIconObjects(Dictionary<string, object> itemData, string folder)    // 載入遊戲物件
    {

        if (itemData != null)
        {
            foreach (KeyValuePair<string, object> item in itemData)
            {
                if (!string.IsNullOrEmpty(item.Value.ToString()))
                    assetLoader.LoadAssetFormManifest(folder + Global.IconSuffix + item.Value + Global.ext);
                Debug.Log("LoadIconObjects: " + item.Value);
                //assetLoader.LoadPrefab(folder + "/", Global.IconSuffix + item.Value.ToString() );
            }
            return true;
        }
        else
        {
            Debug.Log("LoadIconObject:itemData is Null !!");
            return false;
        }
    }
    #endregion

    #region -- CreateEmptyObject 建立空物件 --
    /// <summary>
    /// 建立空物件群組
    /// </summary>
    /// <param name="parent">上層物件</param>
    /// <param name="itemType">群組類型(名稱)</param>
    /// <returns></returns>
    public GameObject CreateEmptyObject(Transform parent, int itemType)
    {
        GameObject emptyGroup = new GameObject(itemType.ToString());   // 商品物件空群組
        emptyGroup.transform.parent = parent;
        emptyGroup.layer = parent.gameObject.layer;
        emptyGroup.transform.localPosition = Vector3.zero;
        emptyGroup.transform.localScale = Vector3.one;
        return emptyGroup;
    }
    #endregion

    #region -- GetDontNotLoadAsset 取得未載入Asset --
    /// <summary>
    /// 取得未載入Asset
    /// </summary>
    /// <param name="ServerDict"></param>
    /// <param name="itemNameData">name Data</param>
    /// <returns></returns>
    public Dictionary<string, object> GetDontNotLoadAsset(Dictionary<string, object> ServerDict)
    {
        Dictionary<string, object> dictNotLoadedAsset = new Dictionary<string, object>();

        foreach (KeyValuePair<string, object> item in ServerDict)       // 取得未載入物件
        {
            string imageName = item.Value.ToString();

            if (!string.IsNullOrEmpty(imageName) && assetLoader.GetAsset(imageName) == null)
            {
                dictNotLoadedAsset.Add(imageName, imageName);
            }
        }
        return dictNotLoadedAsset;
    }

    /// <summary>
    /// 取得未載入Asset (nestedDict)
    /// </summary>
    /// <param name="ServerDict"></param>
    /// <param name="itemNameData">name Data</param>
    /// <returns></returns>
    public Dictionary<string, object> GetDontNotLoadAsset(Dictionary<string, object> ServerDict, Dictionary<string, object> itemNameData)
    {
        Dictionary<string, object> dictNotLoadedAsset = new Dictionary<string, object>();

        foreach (KeyValuePair<string, object> item in ServerDict)       // 取得未載入物件
        {
            string imageName = MPGFactory.GetObjFactory().GetColumnsDataFromID(itemNameData, "ItemName", item.Key.ToString()).ToString();

            if (!string.IsNullOrEmpty(imageName) && imageName != "-1" && assetLoader.GetAsset(imageName) == null)
            {
                dictNotLoadedAsset.Add(item.Key.ToString(), imageName);
            }
        }
        return dictNotLoadedAsset;
    }
    #endregion

    #region -- ResumeToggleTarget --
    /// <summary>
    /// 復原視窗焦點
    /// </summary>
    protected void ResumeToggleTarget()
    {
        EventMaskSwitch.Resume();
        //  UI.loadingPanel.SetActive(false);
        m_RootUI.transform.parent.GetComponentInChildren<AttachBtn_MenuUI>().loadingPanel.SetActive(false);

        //  m_RootUI.transform.Find("Loading(Panel)").gameObject.SetActive(false);
        //   GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().Panel[5].SetActive(false);
        EventMaskSwitch.Switch(m_RootUI);
        EventMaskSwitch.lastPanel = m_RootUI;
        //   assetLoader.init();  // 錯誤 有可以會導致部分載入還沒完成就重製
    }
    #endregion

    public void SetLoadedActor(GameObject actor)
    {
        if (_tmpActor != null)
            _tmpActor.SetActive(false);

        if (!_dictActor.ContainsKey(actor.name))
        {
            _dictActor.Add(actor.name, actor);
            _tmpActor = actor;
        }
        else if (_dictActor.ContainsKey(actor.name))
        {
            _dictActor.TryGetValue(actor.name, out _tmpActor);
        }
        else
        {
            Debug.LogError("Can't find Actor!");
        }
        _tmpActor.SetActive(true);

    }

    public GameObject GetLoadedActor(string actorName)
    {
        if (_dictActor.ContainsKey(actorName))
        {
            _dictActor.TryGetValue(actorName, out GameObject actor);
            return actor;
        }
        Debug.LogError("Can't Get Actor!");
        return null;
    }

    public bool IsLoadedActor(string actorName)
    {
        if (_dictActor.ContainsKey(actorName))
            return true;
        else
            return false;
    }


    protected abstract void OnLoading();

    protected abstract void OnLoadPanel();

    protected abstract void GetMustLoadAsset();

    /// 取得需要載入的資料總和
    protected abstract int GetMustLoadedDataCount();

    public abstract void OnClosed(GameObject obj);

    #region -- ShowPanel 開啟Panel --
    /// <summary>
    ///  開啟Panel
    /// </summary>
    /// <param name="panelName">Panel名稱，不含(Panel)</param>
    /// <returns>Panel</returns>
    public virtual void ShowPanel(string panelName)
    {
        panelName = panelName.Replace("(Panel)", "");
        this.activePanelName = panelName;

        // 開啟LoadingPanel
        GameObject loadingPanel = GameObject.Find(Global.Scene.MainGameAsset.ToString()).GetComponentInChildren<AttachBtn_MenuUI>().loadingPanel;
        loadingPanel.SetActive(true);
        EventMaskSwitch.Switch(loadingPanel);
        EventMaskSwitch.lastPanel = loadingPanel;
        //_lastPanel = loadingPanel;

        if (!dictPanelRefs.ContainsKey(panelName))         // 如果還沒載入Panel AB 載入AB
        {
            assetLoader.LoadAssetFormManifest(Global.PanelUniquePath + panelName.ToLower() + Global.ext);
            _loadedPanel = true;
        }
        else
        {
            PanelSwitch(panelName);// 已載入AB 顯示Panel
            //m_RootUI.SetActive(true);
        }

        Debug.Log(panelName);
        Debug.Log(m_RootUI);
        //   Transform panel = m_RootUI.transform.Find(panelName + "(Panel)");
        //  


    }
    #endregion

    #region -- PanelSwitch Panel 開/關 --
    /// <summary>
    /// Panel 開/關狀態
    /// </summary>
    /// <param name="panelNo">Panel編號</param>
    private GameObject PanelSwitch(string panelName)
    {

        PanelState panelState = dictPanelRefs[panelName];      // 取得目前Panel狀態

        if (panelState.obj != null && (m_MPGame.GetLoginStatus() || !Global.connStatus))
        {
            if (!panelState.obj.activeSelf)                     // 如果Panel是關閉狀態
            {
                Initinal();
                Debug.Log("Open Panel : " + panelName);
                //scrollView.scroll = false;
                if (EventMaskSwitch.lastPanel != null)
                    EventMaskSwitch.lastPanel.SetActive(false);

                panelState.obj.SetActive(true);
                OnLoading();
                panelState.onOff = true; ;
                // _lastPanel = panelState.obj;
                EventMaskSwitch.Switch(panelState.obj);
                EventMaskSwitch.lastPanel = panelState.obj;

                return panelState.obj;
            }                                                   // 如果Panel已開啟
            else
            {
                Debug.Log("Closed Panel : " + panelName);
                Release();
                //  scrollView.scroll = true;
                EventMaskSwitch.lastPanel.SetActive(false);
                EventMaskSwitch.Resume();
                panelState.obj.SetActive(false);
                //_lastPanel = panelState.obj;
                panelState.onOff = false;
                //Camera.main.GetComponent<UICamera>().eventReceiverMask = (int)Global.UILayer.Default;
                return panelState.obj;
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
    protected virtual void InstantiatePanel() //實體化Panel現在是正確的，有時間可以重新啟用 很多用編輯器拉進去的Panel都要修改到陣列
    {
        GameObject bundle = assetLoader.GetAsset(activePanelName.ToLower());
        PanelState panelState = new PanelState();

        panelState.obj = MPGFactory.GetObjFactory().Instantiate(bundle, m_RootUI.transform, activePanelName, Vector3.zero, Vector3.one, Vector3.zero, -1);
        panelState.obj = panelState.obj.transform.parent.gameObject;
        panelState.obj.layer = m_RootUI.layer;

        if (!dictPanelRefs.ContainsKey(activePanelName))
            dictPanelRefs.Add(activePanelName, panelState);
    }
    #endregion

    #region -- 是否載入Panel --
    protected bool bLoadedPanel(string panelName)
    {
        return dictPanelRefs.ContainsKey(panelName) ? true : false;
    }
    #endregion
}
