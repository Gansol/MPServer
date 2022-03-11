using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using MPProtocol;

/* ***************************************************************
 * -----Copyright © 2018 Gansol Studio.  All Rights Reserved.-----
 * -----------            CC BY-NC-SA 4.0            -------------
 * -----------  @Website:  EasyUnity@blogspot.com    -------------
 * -----------  @Email:    GansolTW@gmail.com        -------------
 * -----------  @Author:   Krola.                    -------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * 負責 商店 所有處理
 * 現在會錯誤是正常的 因為還沒有Gashpon 暫時會少載入一下ICON
 * 目前載入第一個Tab是Mice，如果要第一個載入Gashapon需要換到第一個  infoGroupsArea
 * 部分程式碼 在MPPanel
 * ***************************************************************
 *                           ChangeLog
 * 20210111 v3.0.1  修正載入問題
 * 20201027 v3.0.0  繼承重構
 * 20200813 Store需要全部重寫
 * 20171226 v1.1.4   簡化載入流程 註解                          
 * 20171119 v1.1.3   修正載入流程 
 * 20161102 v1.0.2   3次重構，改變繼承至 PanelManager>MPPanel
 * 20160914 v1.0.1b  2次重構，獨立實體化物件                          
 * 20160711 v1.0.1a  1次重構，獨立AssetLoader                       
 * 20160705 v1.0.0   0版完成，載入老鼠部分未來需要修改     

 * ****************************************************************/
public class StoreUI : IMPPanelUI
{
    #region 欄位
    //assetFolder;// 1 miceicon 2 itemicon 3 itemicon 9 gashapon
    private static GameObject _lastPanel, _lastItemBtn;                 // 暫存分頁、暫存按鈕
    private AttachBtn_StoreUI UI;                                                          // UI按扭
    private Dictionary<string, string> _dictBuyingGoodsData;      // 儲存購買商品資料 
    private Dictionary<string, object> _dictItemData;                      // 道具資料
    private Dictionary<string, GameObject> _dictItemRefs;           // 道具索引

    private Vector2 itemPosOffset;      // 道具位置偏移量
    private Vector3 actorScale;             // 角色縮放
    private string _folderString;            // 資料夾名稱
 /// <summary>
/// 使用質數判斷資料載入完成  (無法整除的數 = 少載入的資料)
/// 2=StoreData  | 3=ItemData  |5=Currency  |7=PlayerItem  |9=Gashapon  |11=  |13=  |17=  |19=  |
/// </summary>
    private int _dataLoadedCount;      // 資料載入量
    private int _tablePageCount;          // 每頁物件矩總量
    private int _tableRowCount;           // 物件矩 每行物件數量
    private int _itemType;                       // 道具類別
    private bool _bFirstLoad;                 // 是否第一次載入Panel
    private bool  _bLoadedAsset;         // 是否載入資產
    private bool _bLoadedActor;          // 是否載入角色
    private bool _bLoadedPanel;          // 是否載入Panel
    private bool _bLoadedGashapon; // 是否載入轉蛋
    #endregion

    public StoreUI(MPGame MPGame) : base(MPGame)
    {
        Debug.Log("--------------- StoreUI Create ----------------");
        _bFirstLoad = true;
        _dictItemRefs = new Dictionary<string, GameObject>();
        actorScale = new Vector3(1.5f, 1.5f, 1f);
        itemPosOffset = new Vector2(295, -350);

        _dictBuyingGoodsData = new Dictionary<string, string> {
        { StoreParameterCode.ItemID.ToString(), "" }, { StoreParameterCode.ItemName.ToString(), "" },
        { StoreParameterCode.ItemType.ToString(), "" }, { StoreParameterCode.CurrencyType.ToString(), "" }, { StoreParameterCode.BuyCount.ToString(), "" } };
    }

    enum ENUM_Area : int
    {
        PlayerInfo = 0,
        Tab,
        Gashapon,
        ItemPanel,
        ItemInfoBox,
        CheckoutBox,
        GashaponBox
    }


    public override void Initialize()
    {
        Debug.Log("--------------- StoreUI Initialize ----------------");
        _itemType = (int)StoreType.Mice;
        _tablePageCount = 9;
        _tableRowCount = 3;
        _bLoadedPanel = false;

        Global.photonService.LoadStoreDataEvent += OnLoadStoreData;
        Global.photonService.LoadPlayerItemEvent += OnLoadPlayerItem;
        Global.photonService.LoadCurrencyEvent += OnLoadCurrency;
        Global.photonService.LoadItemDataEvent += OnLoadItemData;
        Global.photonService.UpdateCurrencyEvent += LoadPlayerInfo;
        Global.photonService.GetGashaponEvent += OnGetGashapon;
    }

    public override void Update()
    {
        base.Update();

        #region 載入轉蛋完成後 實體化 轉蛋
        //// ins fisrt load panelScene : Gashapon
        //if (m_MPGame.GetAssetLoader().bLoadedObj && _bLoadedGashapon)                 // 載入轉蛋完成後 實體化 轉蛋
        //{
        //    Debug.Log("Gashapon!!!!!");
        //    InstantiateGashapon(infoGroupsArea[(int)ENUM_Area.Gashapon].transform);
        //}
        #endregion

        #region -- 載入Panel完成後 --
        if (_dataLoadedCount == GetMustLoadedDataCount() && !_bLoadedPanel)
        {
            _bLoadedPanel = true;

            //// 顯示 Tab下 第一個商品類別
            //if (!_bFirstLoad)
            //    OnTabClick(UI.miceTab);

            OnLoadPanel();
        }
        #endregion

        #region 載入道具完成後 實體化 道具
        if (m_AssetLoaderSystem.IsLoadAllAseetCompleted && _bLoadedAsset)
        {
            m_AssetLoaderSystem.Initialize();
            _bLoadedAsset = false;

            InstantiateStoreItem();
            LoadPrice(_dictItemData, _itemType);
            SwitchInStorePanel(UI.itemPanel.transform.Find(_itemType.ToString()).gameObject);
            ResumeToggleTarget();
        }
        #endregion

        #region 載入角色完成後 實體化 角色
        if (m_AssetLoaderSystem.IsLoadAllAseetCompleted && _bLoadedActor)
        {
            m_AssetLoaderSystem.Initialize();
            _bLoadedActor = false;

            string actorName = _lastItemBtn.GetComponentInChildren<UISprite>().name;
            InstantiateActor(actorName, UI.miceInfo_msgBox.transform.GetChild(0).Find("Image"), actorScale);
        }
        #endregion
    }


    #region  --  InstantiateStoreItem 實體化商店物品   --   
    /// <summary>
    /// 實體化商店物品
    /// </summary>
    private void InstantiateStoreItem()
    {
        // 實體化的商品背景(按鈕)
        Dictionary<string, GameObject> itemBtnDict = InstantiateBagItemBG(_dictItemData, StoreType.Item.ToString(), _itemType, UI.itemPanel.transform, itemPosOffset, _tablePageCount, _tableRowCount);
        Transform parent = UI.itemPanel.transform.Find(_itemType.ToString());

        // 如果有新商品 加入索引 並 加入按鈕事件
        if (itemBtnDict != null)
        {
            _dictItemRefs = _dictItemRefs.Concat(itemBtnDict).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value);
            AddItemBtnEvent_OnClick(itemBtnDict);
        }

        //實體化 商品圖示
        InstantiateItemIcon(GetStoreItemDataAndFolderPath(_itemType), parent);// 選擇商店資料後 實體化物件
    }
    #endregion

    #region  --  AddItemBtnEvent_OnClick  增加按鈕事件 --   
    /// <summary>
    /// 增加商品按下時 顯示資訊 按鈕事件
    /// </summary>
    /// <param name="dictItemBtn"></param>
    private void AddItemBtnEvent_OnClick(Dictionary<string, GameObject> dictItemBtn)
    {
        foreach (KeyValuePair<string, GameObject> btn in dictItemBtn)
            UIEventListener.Get(btn.Value).onClick = OnItemClick;
    }
    #endregion

    #region -- OnLoadPanel 載入面板 --
    protected override void OnLoading()
    {
        UI = m_RootUI.GetComponentInChildren<AttachBtn_StoreUI>();
        _dataLoadedCount = (int)ENUM_Data.None;

        if (Global.isMatching)
            Global.photonService.ExitWaitingRoom();

        UIEventListener.Get(UI.gashaponTab).onClick = OnTabClick;
        UIEventListener.Get(UI.miceTab).onClick = OnTabClick;
        UIEventListener.Get(UI.ItemTab).onClick = OnTabClick;
        UIEventListener.Get(UI.skillTab).onClick = OnTabClick;

        UIEventListener.Get(UI.miceBuyBtn).onClick = OnBuyClick;
        UIEventListener.Get(UI.itemBuyBtn).onClick = OnBuyClick;
        UIEventListener.Get(UI.minusBtn).onClick = OnQuantity;
        UIEventListener.Get(UI.minusBtn).onClick = OnQuantity;
        UIEventListener.Get(UI.addBtn).onClick = OnQuantity;
        UIEventListener.Get(UI.chkoutCancelBtn).onClick = OnReturn;
        UIEventListener.Get(UI.gashaponCancelBtn).onClick = OnReturn;
        UIEventListener.Get(UI.chkoutConfirmBtn).onClick = OnComfirm;
        UIEventListener.Get(UI.gashaponConfirmBtn).onClick = OnConfirmGashapon;

        UIEventListener.Get(UI.chcekoutBoxCollider).onClick = OnReturn;
        UIEventListener.Get(UI.gashaponBoxCollider).onClick = OnReturn;
        UIEventListener.Get(UI.itemCollider).onClick = OnReturn;
        UIEventListener.Get(UI.miceCollider).onClick = OnReturn;
        UIEventListener.Get(UI.closeCollider).onClick = OnClosed;

        // LoadGashaponAsset(); //暫時關閉 Gashapon功能
        Global.photonService.LoadStoreData();
        Global.photonService.LoadItemData();
        Global.photonService.LoadCurrency(Global.Account);
        Global.photonService.LoadPlayerItem(Global.Account);
    }
    #endregion

    #region  --  OnLoadData 載入資料集合  --   
    private void OnLoadStoreData()
    {
        _dataLoadedCount *= (int)ENUM_Data.StoreData;
    }

    private void OnLoadItemData()
    {
        _dataLoadedCount *= (int)ENUM_Data.ItemData;
    }

    private void OnLoadCurrency()
    {
        _dataLoadedCount *= (int)ENUM_Data.CurrencyData;
    }

    private void OnLoadPlayerItem()
    {
        _dataLoadedCount *= (int)ENUM_Data.PlayerItem;
    }
    #endregion

    #region -- OnLoadPanel  --
    /// <summary>
    /// Panel載入完成後
    /// </summary>
    protected override void OnLoadPanel()
    {
        if (m_RootUI.activeSelf)
        {
            if (_bFirstLoad)
            {
                _bFirstLoad = false;
            //   LoadGashaponAsset(assetFolder[0]);isStoreLoaded
            }

            GetMustLoadAsset();
            _dictItemData = Global.storeItem;
            LoadPlayerInfo();
        }
      //  Global.isStoreLoaded = false;
    }
    #endregion

    #region -- LoadPlayerInfo 載入玩家資訊 --
    /// <summary>
    /// 載入玩家資訊
    /// </summary>
    private void LoadPlayerInfo()
    {
        Debug.Log("Rice: " + Global.Rice.ToString());
        Debug.Log("Gold: " + Global.Gold.ToString());
        UI.level.text = Global.Rank.ToString();
        UI.rice.text = Global.Rice.ToString();
        UI.gold.text = Global.Gold.ToString();
    }
    #endregion

    #region -- OnClick 按下事件 --
    public void OnGashaponClick(GameObject go)
    {
        Debug.Log(go.name);
    }

    /// <summary>
    /// 商品按鈕事件(顯示商品資訊)
    /// </summary>
    /// <param name="go">商品按鈕</param>
    public void OnItemClick(GameObject go)
    {
        _lastItemBtn = go;

        Dictionary<string, object> dictItemProperty = Global.storeItem[_lastItemBtn.name] as Dictionary<string, object>;
        Dictionary<string, object> playerItemData = new Dictionary<string, object>();

        // 取得玩家道具資詳細資料
        if (Global.playerItem.ContainsKey(_lastItemBtn.name)) playerItemData = Global.playerItem[_lastItemBtn.name] as Dictionary<string, object>;

        // 取得商品類別 資產資料夾位子
        //GetStoreItemDataAndFolderPath(_itemType);

        // 因為子物件不能有不同的Layer強制改變



        // 載入選擇的資料
        switch (_itemType)
        {
            default:
            case (int)StoreType.Mice:
                UI.miceInfo_msgBox.SetActive(true);
                // UI.miceInfo_msgBox.layer = UI.miceInfo_msgBox.transform.parent.gameObject.layer = LayerMask.NameToLayer("ItemInfo");
                _dictItemData = Global.miceProperty;
                LoadMicePorperty(go, UI.miceInfo_msgBox.transform, playerItemData, dictItemProperty);
                EventMaskSwitch.Switch(UI.miceInfo_msgBox.gameObject);
                break;
            case (int)StoreType.Item:
            case (int)StoreType.Armor:
                UI.itemInfo_msgBox.SetActive(true);
                // UI.itemInfo_msgBox.layer = UI.itemInfo_msgBox.transform.parent.gameObject.layer = LayerMask.NameToLayer("ItemInfo");
                _dictItemData = Global.itemProperty;
                LoadItemPorperty(go, UI.itemInfo_msgBox.transform, playerItemData, dictItemProperty);
                EventMaskSwitch.Switch(UI.itemInfo_msgBox.gameObject);
                break;
        }
    }

    /// <summary>
    /// 載入老鼠詳細資料
    /// </summary>
    /// <param name="item_Click">按下的道具</param>
    /// <param name="itemInfoBox">訊息視窗</param>
    /// <param name="playerItemData">玩家道具資料</param>
    /// <param name="dictItemProperty">道具詳細資料</param>
    private void LoadMicePorperty(GameObject item_Click, Transform itemInfoBox, Dictionary<string, object> playerItemData, Dictionary<string, object> dictItemProperty)
    {
        object value;

        // 關閉上個Panel 顯示Panel 
        // switchInStorePanel(itemInfoBox.Find(StoreType.Item.ToString()).gameObject);

        //itemInfoBox.Find(StoreType.Item.ToString()).gameObject.SetActive(false);
        //itemInfoBox.Find(StoreType.Mice.ToString()).gameObject.SetActive(true);

        // 載入商品資訊
        LoadProperty.LoadItemProperty(item_Click, itemInfoBox.GetChild(0), Global.miceProperty, _itemType);
        LoadProperty.LoadItemProperty(item_Click, itemInfoBox.GetChild(0), _dictItemData, _itemType);
        LoadProperty.LoadPrice(item_Click, itemInfoBox.transform.GetChild(0).gameObject, _itemType);

        // 異步載入角色
        _bLoadedActor = LoadActor(item_Click, itemInfoBox.GetChild(0).Find("Image"), actorScale);

        // 載入 對應商品 的 玩家道具數量
        playerItemData.TryGetValue(PlayerItem.ItemCount.ToString(), out value);
        itemInfoBox.GetChild(0).Find("Count").GetComponent<UILabel>().text = value.ToString();

        // 載入商品資訊
        dictItemProperty.TryGetValue(StoreProperty.Description.ToString(), out value);
        itemInfoBox.GetChild(0).Find(StoreProperty.Description.ToString()).GetComponent<UILabel>().text = value.ToString();
    }

    /// <summary>
    /// 載入道具詳細資料
    /// </summary>
    /// <param name="item_Click">按下的道具</param>
    /// <param name="itemInfoBox">訊息視窗</param>
    /// <param name="playerItemData">玩家道具資料</param>
    /// <param name="dictItemProperty">道具詳細資料</param>
    private void LoadItemPorperty(GameObject item_Click, Transform itemInfoBox, Dictionary<string, object> playerItemData, Dictionary<string, object> dictItemProperty)
    {
        object value;

        // 關閉上個Panel 顯示Panel 
        //switchInStorePanel(itemInfoBox.Find(StoreType.Item.ToString()).gameObject);
        //itemInfoBox.Find(StoreType.Item.ToString()).gameObject.SetActive(true);
        //itemInfoBox.Find(StoreType.Mice.ToString()).gameObject.SetActive(false);

        // 載入商品資訊
        LoadProperty.LoadItemProperty(item_Click, itemInfoBox.GetChild(0), _dictItemData, _itemType);
        LoadProperty.LoadPrice(item_Click, itemInfoBox.GetChild(0).gameObject, _itemType);

        itemInfoBox.GetChild(0).Find("Image").GetComponent<UISprite>().atlas = _lastItemBtn.GetComponentInChildren<UISprite>().atlas;

        // 載入商品資訊
        dictItemProperty.TryGetValue(StoreProperty.ItemName.ToString(), out value);
        itemInfoBox.GetChild(0).Find("Image").GetComponent<UISprite>().spriteName = Global.IconSuffix + value.ToString().ToLower().Replace(" ", "");
        dictItemProperty.TryGetValue(StoreProperty.Description.ToString(), out value);
        itemInfoBox.GetChild(0).Find(StoreProperty.Description.ToString()).GetComponent<UILabel>().text = value.ToString();

        // 載入 對應商品 的 玩家道具數量
        playerItemData.TryGetValue(PlayerItem.ItemCount.ToString(), out value);
        itemInfoBox.GetChild(0).Find("Count").GetComponent<UILabel>().text = value.ToString();
    }

    /// <summary>
    /// 按下購買時
    /// </summary>
    /// <param name="myPanel"></param>
    public void OnBuyClick(GameObject myPanel)
    {
        Debug.Log("ItemType: " + _itemType);
        // 關閉 商品詳情視窗 並顯示 購買數量視窗


        // 初始化視窗資訊
        BuyWindowsInit();

        // // 因為子物件不能有不同的Layer強制改變
        //UI.checkout_msgBox.layer = LayerMask.NameToLayer("BuyInfo");
        UI.checkout_msgBox.SetActive(true);
        EventMaskSwitch.OpenedPanel.SetActive(false);
        EventMaskSwitch.Switch(UI.checkout_msgBox);
    }

    /// <summary>
    /// 初始化視窗資訊
    /// </summary>
    private void BuyWindowsInit()
    {
        Dictionary<string, object> dictItemProperty = Global.storeItem[_lastItemBtn.name] as Dictionary<string, object>;

        //string colunmsName = (_itemType == (int)MPProtocol.StoreType.Mice) ? "MiceID" : "ItemID";

        // 基礎購買量
        _dictBuyingGoodsData[StoreProperty.BuyCount.ToString()] = "1";

        // 暫存 將購買的商品資訊
        dictItemProperty.TryGetValue(StoreProperty.ItemID.ToString(), out object value);
        _dictBuyingGoodsData[StoreProperty.ItemID.ToString()] = value.ToString();

        dictItemProperty.TryGetValue(StoreProperty.ItemType.ToString(), out value);
        _dictBuyingGoodsData[StoreProperty.ItemType.ToString()] = value.ToString();

        dictItemProperty.TryGetValue(StoreProperty.CurrencyType.ToString(), out value);
        _dictBuyingGoodsData[StoreProperty.CurrencyType.ToString()] = value.ToString(); Debug.Log(value.ToString());

        dictItemProperty.TryGetValue(StoreProperty.ItemName.ToString(), out value);
        _dictBuyingGoodsData[StoreProperty.ItemName.ToString()] = value.ToString();

        // 顯示圖示
        UI.checkout_msgBox.transform.GetChild(0).Find("Image").GetComponent<UISprite>().atlas = _lastItemBtn.GetComponentInChildren<UISprite>().atlas;
        UI.checkout_msgBox.transform.GetChild(0).Find("Image").GetComponent<UISprite>().spriteName = Global.IconSuffix + value.ToString().ToLower().Replace(" ", "");

        // 顯示商品資訊
        UI.checkout_msgBox.transform.GetChild(0).Find("Count").GetComponent<UILabel>().text = "1";  // count = 1
        dictItemProperty.TryGetValue("Price", out value);
        UI.checkout_msgBox.transform.GetChild(0).Find("Sum").GetComponent<UILabel>().text = value.ToString(); // price
        UI.checkout_msgBox.transform.GetChild(0).Find("Price").GetComponent<UILabel>().text = value.ToString(); // price
    }

    /// <summary>
    /// 改變道具數量時 計算總價格 數量
    /// </summary>
    /// <param name="go"></param>
    public void OnQuantity(GameObject go)
    {
        int price = int.Parse(UI.checkout_msgBox.transform.GetChild(0).Find("Price").GetComponent<UILabel>().text);
        int sum = int.Parse(UI.checkout_msgBox.transform.GetChild(0).Find("Sum").GetComponent<UILabel>().text);
        int count = int.Parse(UI.checkout_msgBox.transform.GetChild(0).Find("Count").GetComponent<UILabel>().text);

        count += (go.name == "Add") ? 1 : -1;
        count = (count < 0) ? 0 : count;
        _dictBuyingGoodsData[StoreProperty.BuyCount.ToString()] = UI.checkout_msgBox.transform.GetChild(0).Find("Count").GetComponent<UILabel>().text = count.ToString();
        UI.checkout_msgBox.transform.GetChild(0).Find("Sum").GetComponent<UILabel>().text = (price * count).ToString();
    }

    /// <summary>
    /// 確認購買
    /// </summary>
    /// <param name="myPanel"></param>
    public void OnComfirm(GameObject go)
    {
        UI.checkout_msgBox.SetActive(false);
        Global.photonService.BuyItem(Global.Account, _dictBuyingGoodsData);
    }

    public override void OnClosed(GameObject go)
    {
        EventMaskSwitch.LastPanel = null;
        //GameObject root = go.transform.parent.gameObject;
        ShowPanel(m_RootUI.name);
        // GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().LoadPanel(go.transform.parent.gameObject);
        EventMaskSwitch.Resume();
    }

    /// <summary>
    /// 返回事件遮罩
    /// </summary>
    /// <param name="go"></param>
    public void OnReturn(GameObject go)
    {
        int level = int.Parse(go.name); // 會回遮罩層級 1=上一層 2=上兩層
        EventMaskSwitch.OpenedPanel.SetActive(false);
        EventMaskSwitch.Prev(level);
    }

    /// <summary>
    /// 點選商品分頁時 載入商品
    /// </summary>
    /// <param name="go"></param>
    public void OnTabClick(GameObject go)
    {
        _itemType = (go == null) ? (int)StoreType.Mice : int.Parse(go.name.Remove(0, 3));
        Debug.Log("go.name " + go.name);
        Debug.Log("_itemType " + _itemType);
        Debug.Log("infoGroupsArea[(int)ENUM_Area.ItemPanel] " + UI.itemPanel.name);

        OnLoadPanel();
    }
    #endregion

    #region  --   GetMustLoadAsset 載入必要資產 --   
    /// <summary>
    /// 載入必要資產
    /// </summary>
    protected override void GetMustLoadAsset()
    {
        Dictionary<string, object> itemDetailData;

        // 取得道具資料
        _dictItemData = GetStoreItemDataAndFolderPath(_itemType);

        // 道具詳細資料
        itemDetailData = MPGFactory.GetObjFactory().GetItemDetailsInfoFromType(_dictItemData, _itemType);

        // 載入資產
        if (!m_AssetLoaderSystem.GetAsset(_folderString))
        {
            m_AssetLoaderSystem.LoadAssetFormManifest(Global.PanelUniquePath + Global.StoreItemAssetName + Global.ext);  // 道具Slot
            _bLoadedAsset = LoadIconObjectsAssetByName(GetItemNameList(itemDetailData), _folderString);
        }
        else if (m_AssetLoaderSystem.GetDontNotLoadAssetName(itemDetailData).Count > 0)
        {
            _bLoadedAsset = LoadIconObjectsAssetByName(GetItemNameList(itemDetailData), _folderString);
        }
    }
    #endregion

    #region -- LoadPrice 載入物件價格 --
    /// <summary>
    /// 載入物件價格
    /// </summary>
    /// <param name="itemData">道具資料</param>
    /// <param name="itemType">道具類別</param>
    private void LoadPrice(Dictionary<string, object> itemData, int itemType)
    {
        Dictionary<string, object> diceItemProperty;
        object price;

        // 取得分類好的道具資料
        itemData = MPGFactory.GetObjFactory().GetItemDetailsInfoFromType(itemData, itemType);

        // 載入資料
        foreach (KeyValuePair<string, object> item in itemData)
        {
            diceItemProperty = Global.storeItem[item.Key] as Dictionary<string, object>;
            diceItemProperty.TryGetValue("Price", out price);
            _dictItemRefs[item.Key].transform.Find("Price").GetComponent<UILabel>().text = price.ToString();
        }
    }
    #endregion

    #region -- GetItemNameData 取得道具名稱資料 --


    /// <summary>
    /// 取得道具名稱資料字典(name,name)
    /// </summary>
    /// <param name="itemData">物件陣列</param>
    /// <returns>Dictionary(ItemName,ItemName)</returns>
    private List<string> GetItemNameList(Dictionary<string, object> itemData)    // 載入遊戲物件
    {
        List<string>  data = new List<string>();

        foreach (KeyValuePair<string, object> item in itemData)
        {
            var nestedData = item.Value as Dictionary<string, object>;
            nestedData.TryGetValue("ItemName", out object itemName);
            data.Add(itemName.ToString());
        }

        return data;
    }
    #endregion

    #region -- InstantiateItemIcon 實體化道具圖片--
    /// <summary>
    /// 實體化載入完成的遊戲物件，利用玩家JSON資料判斷必要實體物件
    /// </summary>
    /// <param name="dictionary">資料字典</param>
    /// <param name="myParent">實體化父系位置</param>
    private void InstantiateItemIcon(Dictionary<string, object> itemData, Transform myParent)
    {
        int i = 0;
        itemData = MPGFactory.GetObjFactory().GetItemDetailsInfoFromType(itemData, _itemType);

        // 實體化
        foreach (KeyValuePair<string, object> item in itemData)
        {
            object itemName;
            var nestedData = item.Value as Dictionary<string, object>;
            nestedData.TryGetValue(StoreProperty.ItemName.ToString(), out itemName);
            string bundleName = Global.IconSuffix + itemName.ToString();

            // 已載入資產時
            if (m_AssetLoaderSystem.GetAsset(bundleName) != null)
            {
                GameObject bundle = m_AssetLoaderSystem.GetAsset(bundleName);
                Transform imageParent = myParent.GetChild(i).GetChild(0);

                // 如果沒有ICON 實體化
                if (imageParent.childCount == 0)
                    MPGFactory.GetObjFactory().Instantiate(bundle, imageParent, itemName.ToString(), new Vector3(0, -30), Vector3.one, new Vector2(140, 140), 400);
            }
            else
            {
                Debug.LogError("Assetbundle reference not set to an instance. BundleName:" + bundleName);
            }
            i++;
        }

        //switchInStorePanel(infoGroupsArea[(int)ENUM_Area.ItemPanel].transform.Find(_itemType.ToString()).gameObject);
        _bLoadedAsset = false;
    }
    #endregion

    #region  --  GetStoreItemDataAndFolderPath 選擇商店資料  --   
    /// <summary>
    /// 選擇商店資料
    /// </summary>
    /// <param name="_itemType">道具類別</param>
    private Dictionary<string, object> GetStoreItemDataAndFolderPath(int _itemType)
    {
        switch ((StoreType)_itemType)
        {
            case StoreType.Mice:
                //     _itemData = Global.storeItem;
                _folderString = Global.MiceIconUniquePath;
                break;
            case StoreType.Item:
            case StoreType.Armor:
                //   _itemData = Global.storeItem;
                _folderString = Global.ItemIconUniquePath;
                break;
            case StoreType.Gashapon:
                //   _itemData = Global.storeItem;
                _folderString = Global.GashaponUniquePath;
                break;
        }
        _dictItemData = Global.storeItem;
        return _dictItemData;
    }
    #endregion

    #region -- LoadGashapon 載入轉蛋物件(亂寫) --
    private void LoadGashaponAsset()
    {
        GetStoreItemDataAndFolderPath((int)StoreType.Gashapon);
      //  m_AssetLoaderSystem.Initialize();
        for (int i = 1; i <= 3; i++)
            m_AssetLoaderSystem.LoadAssetFormManifest("gashapon/unique/gashapon" + i.ToString() + Global.ext);
        m_AssetLoaderSystem.SetLoadAllAseetCompleted();
        _bLoadedGashapon = true; // 來用暫時停掉方便測試  並需要配合修改起始TAB頁面 
    }
    #endregion

    #region -- InstantiateGashapon 實體化轉蛋物件(亂寫)--
    /// <summary>
    /// 實體化載入完成的轉蛋物件，利用資料夾名稱判斷必要實體物件
    /// </summary>
    /// <param name="myParent">實體化父系位置</param>
    /// <param name="folder">資料夾名稱</param>
    private void InstantiateGashapon(Transform myParent)
    {
        // _bLoadedGashapon = true;

        GetStoreItemDataAndFolderPath((int)StoreType.Gashapon);

        Dictionary<string, object> dictGashaponData = new Dictionary<string, object>();
        List<string> itemNameList = new List<string>();
        List<string> gashaponNameList = new List<string>();
        object itemType, itemName;

        foreach (var item in Global.storeItem)
        {
            Dictionary<string, object> values = item.Value as Dictionary<string, object>;
            values.TryGetValue(StoreProperty.ItemType.ToString(), out itemType);
            values.TryGetValue(StoreProperty.ItemName.ToString(), out itemName);
            itemNameList.Add(itemName.ToString());

            if (int.Parse(itemType.ToString()) == (int)StoreType.Gashapon)
            {
                gashaponNameList.Add(itemName.ToString());
                dictGashaponData.Add(item.Key, item.Value);
            }
        }

        int i = 0;

        foreach (KeyValuePair<string, object> gashapon in dictGashaponData)
        {
            if (m_AssetLoaderSystem.GetAsset(gashaponNameList[i]) != null)                  // 已載入資產時
            {
                GameObject bundle = m_AssetLoaderSystem.GetAsset(gashaponNameList[i]);
                Transform parent = myParent.Find("Promotions").GetChild(i);

                Debug.Log("parent name: " + parent.name);
                Debug.Log("parent.Find()" + parent.Find("Imgae").name);
                Debug.Log("gashaponNameList[i] " + gashaponNameList[i]);
                parent.Find("Imgae").GetComponent<UISprite>().name = gashaponNameList[i];
                parent.name = gashaponNameList[i];
            }
            else
            {
                Debug.LogError("Assetbundle reference not set to an instance.");
            }
            i++;
        }
        _bLoadedGashapon = false;
        //  switchInStorePanel(infoGroupsArea[(int)ENUM_Area.Gashapon]);
    }
    #endregion

    #region  --  OnBuyGashapon 按下購買轉蛋   --   
    public void OnBuyGashapon(GameObject go)
    {
        _lastItemBtn = go;
        UI.gashapon_msgBox.SetActive(true);

        // 初始化視窗資訊
        GashaponWindowsInit();

        //// 因為子物件不能有不同的Layer強制改變
        //UI.gashapon_msgBox.layer = LayerMask.NameToLayer("BuyInfo");
        EventMaskSwitch.OpenedPanel.SetActive(false);
        EventMaskSwitch.Switch(UI.gashapon_msgBox);
    }
    #endregion

    #region  --  GashaponWindowsInit 轉蛋視窗初始化   --  
    private void GashaponWindowsInit()
    {
        object value;
        Transform checkoutBox = UI.gashapon_msgBox.transform;
        Dictionary<string, object> dictItemProperty = Global.storeItem[_lastItemBtn.name] as Dictionary<string, object>;

        //string colunmsName = (_itemType == (int)MPProtocol.StoreType.Mice) ? "MiceID" : "ItemID";

        //// 基礎購買量
        //buyingGoodsData[StoreProperty.BuyCount.ToString()] = "1";

        // 暫存 將購買的商品資訊
        dictItemProperty.TryGetValue(StoreProperty.ItemID.ToString(), out value);
        _dictBuyingGoodsData[StoreProperty.ItemID.ToString()] = value.ToString();

        dictItemProperty.TryGetValue(StoreProperty.ItemType.ToString(), out value);
        _dictBuyingGoodsData[StoreProperty.ItemType.ToString()] = value.ToString();

        dictItemProperty.TryGetValue(StoreProperty.CurrencyType.ToString(), out value);
        _dictBuyingGoodsData[StoreProperty.CurrencyType.ToString()] = value.ToString();

        dictItemProperty.TryGetValue(StoreProperty.ItemName.ToString(), out value);
        _dictBuyingGoodsData[StoreProperty.ItemName.ToString()] = value.ToString();

        // 顯示圖示
        //  checkoutBox.GetChild(0).Find("Image").GetComponent<UISprite>().atlas = _lastItemBtn.GetComponentInChildren<UISprite>().atlas;
        checkoutBox.GetChild(0).Find("Image").GetComponent<UISprite>().spriteName = value.ToString()/*.Replace(" ", "") + Global.IconSuffix*/;

        // 顯示商品資訊
        // checkoutBox.GetChild(0).FindChild("Count").GetComponent<UILabel>().text = "1";  // count = 1
        dictItemProperty.TryGetValue("Price", out value);
        //  checkoutBox.GetChild(0).FindChild("Sum").GetComponent<UILabel>().text = value.ToString(); // price
        checkoutBox.GetChild(0).Find("Price").GetComponent<UILabel>().text = value.ToString(); // price
    }
    #endregion

    #region  --  OnConfirmGashapon 確認購買轉蛋  --   
    /// <summary>
    /// 確認購買轉蛋
    /// </summary>
    /// <param name="go">Series </param>
    public void OnConfirmGashapon(GameObject go)
    {
        Global.photonService.BuyGashapon(_dictBuyingGoodsData[StoreProperty.ItemID.ToString()], _dictBuyingGoodsData[StoreProperty.ItemType.ToString()], go.name);
    }
    #endregion

    #region  --  OnGetGashapon (還沒寫)   --   
    private void OnGetGashapon(List<string> itemList)
    {
        // show panel
        // show animation
        // skip
    }
    #endregion

    #region  --  ShowPanel 顯示Panel   --   
    public override void ShowPanel(string panelName)
    {
        m_RootUI = GameObject.Find(Global.Scene.MainGameAsset.ToString()).GetComponentInChildren<AttachBtn_MenuUI>().storePanel;
        base.ShowPanel(panelName);
    }
    #endregion

    #region  --  GetMustLoadedDataCount 取得需要載入的資料總和   --   
    /// <summary>
    /// 取得需要載入的資料總和
    /// </summary>
    /// <returns></returns>
    protected override int GetMustLoadedDataCount()
    {
        return (int)ENUM_Data.CurrencyData * (int)ENUM_Data.ItemData * (int)ENUM_Data.PlayerItem * (int)ENUM_Data.StoreData;
    }
    #endregion

    #region  -- SwitchInStorePanel 關閉前一個Panel 並儲存目前Panel等待下次關閉   --  
    /// <summary>
    /// 關閉前一個Panel 並儲存目前Panel等待下次關閉
    /// </summary>
    /// <param name="tab"></param>
    private void SwitchInStorePanel(GameObject tab)
    {
        if (_lastPanel != null && _lastPanel != tab)
            _lastPanel.SetActive(false);

        _lastPanel = tab;
    }
    #endregion

    #region  --  Release   --   
    public override void Release()
    {
        Global.photonService.LoadStoreDataEvent -= OnLoadStoreData;
        Global.photonService.LoadPlayerItemEvent -= OnLoadPlayerItem;
        Global.photonService.LoadCurrencyEvent -= OnLoadCurrency;
        Global.photonService.LoadItemDataEvent -= OnLoadItemData;
        Global.photonService.UpdateCurrencyEvent -= LoadPlayerInfo;
        Global.photonService.GetGashaponEvent -= OnGetGashapon;
    }
    #endregion







    //#region -- LoadItemData 載入道具資訊 --
    //private void LoadItemData(Dictionary<string, object> itemData, Transform parent, int itemType)
    //{
    //    int i = 0, j = 0;
    //    itemData = MPGFactory.GetObjFactory().GetItemInfoFromID(itemData, "ItemID", _itemType); /// 這一定要有 但是 道具類別(有兩類)沒有ItemType可以分辨資料
    //    foreach (KeyValuePair<string, object> item in itemData)
    //    {
    //        var nestedData = item.Value as Dictionary<string, object>;
    //        j = 0;
    //        parent.GetChild(i).GetComponent<Item>().itemProperty = new string[nestedData.Count];
    //        foreach (KeyValuePair<string, object> nested in nestedData)
    //        {
    //            parent.GetChild(i).GetComponent<Item>().itemProperty[j] = nested.Value.ToString();
    //            j++;
    //        }
    //        i++;
    //    }

    //    itemData = MPGFactory.GetObjFactory().GetItemInfoFromType(Global.storeItem, _itemType);
    //    i = j = 0;

    //    foreach (KeyValuePair<string, object> item in itemData)
    //    {
    //        var nestedData = item.Value as Dictionary<string, object>;
    //        j = 0;
    //        parent.GetChild(i).GetComponent<Item>().storeInfo = new string[nestedData.Count];
    //        foreach (KeyValuePair<string, object> nested in nestedData)
    //        {
    //            parent.GetChild(i).GetComponent<Item>().storeInfo[j] = nested.Value.ToString();
    //            j++;
    //        }
    //        i++;
    //    }
    //}
}
