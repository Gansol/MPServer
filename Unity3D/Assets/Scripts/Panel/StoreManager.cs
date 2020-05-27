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
 * 部分程式碼 在MPPanel
 * ***************************************************************
 *                           ChangeLog
 * 20171226 v1.1.4   簡化載入流程 註解                          
 * 20171119 v1.1.3   修正載入流程 
 * 20161102 v1.0.2   3次重構，改變繼承至 PanelManager>MPPanel
 * 20160914 v1.0.1b  2次重構，獨立實體化物件                          
 * 20160711 v1.0.1a  1次重構，獨立AssetLoader                       
 * 20160705 v1.0.0   0版完成，載入老鼠部分未來需要修改     
 * ****************************************************************/
public class StoreManager : MPPanel
{
    #region 欄位
    /// <summary>
    /// 物件存放區
    /// </summary>
    public GameObject[] infoGroupsArea;
    /// <summary>
    /// 資產資料夾名稱
    /// </summary>
    public string[] assetFolder;
    /// <summary>
    /// 偏移量
    /// </summary>
    public Vector2 itemOffset;
    /// <summary>
    /// 每頁物件矩總量
    /// </summary>
    public int tablePageCount;
    /// <summary>
    /// 物件矩 每行物件數量
    /// </summary>
    public int tableRowCount;
    /// <summary>
    /// 角色縮放
    /// </summary>     
    public Vector3 actorScale;

    private Dictionary<string, string> buyingGoodsData;              // 儲存購買商品資料 
    private string _folderString;                                  // 資料夾名稱
    private int _itemType;                                        // 道具形態
    private bool _bFirstLoad, _bLoadedGashapon, _bLoadedAsset, _bLoadedItem, _bLoadedActor, _bLoadedStoreData, _bLoadedItemData, _bLoadedCurrency, _bLoadedPlayerItem, _bLoadedPanel; // 是否載入轉蛋、是否載入圖片、是否載入角色 等
    private static GameObject _lastItemBtn;                   // 暫存分頁、暫存按鈕
    private Dictionary<string, object> _itemData;                   // 道具資料
    private Dictionary<string, GameObject> dictItemRefs;
    #endregion

    public StoreManager(MPGame MPGame) : base(MPGame) { }


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

    private void Awake()
    {
        _bFirstLoad = true;
        dictItemRefs = new Dictionary<string, GameObject>();
        buyingGoodsData = new Dictionary<string, string> { 
        { StoreParameterCode.ItemID.ToString(), "" }, { StoreParameterCode.ItemName.ToString(), "" }, 
        { StoreParameterCode.ItemType.ToString(), "" }, { StoreParameterCode.CurrencyType.ToString(), "" }, { StoreParameterCode.BuyCount.ToString(), "" } };

        _itemType = (int)StoreType.Mice;
        tablePageCount = 9;
        tableRowCount = 3;
        actorScale = new Vector3(1.5f, 1.5f, 1f);
    }

    void OnEnable()
    {
        _bLoadedPanel = false;
        Global.photonService.LoadStoreDataEvent += OnLoadStoreData;
        Global.photonService.LoadPlayerItemEvent += OnLoadPlayerItem;
        Global.photonService.LoadCurrencyEvent += OnLoadCurrency;
        Global.photonService.LoadItemDataEvent += OnLoadItemData;
        Global.photonService.UpdateCurrencyEvent += LoadPlayerInfo;
        Global.photonService.GetGashaponEvent += OnGetGashapon;
    }

    private void Update()
    {
        if (!string.IsNullOrEmpty(assetLoader.ReturnMessage))
            Debug.Log("訊息：" + assetLoader.ReturnMessage);


        //// ins fisrt load panelScene : Gashapon
        if (m_MPGame.GetAssetLoader().loadedObj && !_bLoadedGashapon)                 // 載入轉蛋完成後 實體化 轉蛋
        {
            m_MPGame.GetAssetLoader().init();
            //_tmpTab = infoGroupsArea[2];
            InstantiateGashapon(infoGroupsArea[(int)ENUM_Area.Tab].transform);
        }

        // 載入Panel完成後 
        if (_bLoadedStoreData && _bLoadedPlayerItem && _bLoadedItemData && _bLoadedCurrency && !_bLoadedPanel)
        {
            _bLoadedPanel = true;

            // 顯示 Tab下 第一個商品類別
            if (!_bFirstLoad)
                OnTabClick(infoGroupsArea[(int)ENUM_Area.Tab].transform.GetChild(0).gameObject);

            ResumeToggleTarget();
            OnLoadPanel();
        }

        // 載入道具完成後 實體化 道具
        if (m_MPGame.GetAssetLoader().loadedObj && _bLoadedAsset)
        {
            _bLoadedAsset = !_bLoadedAsset;
            InstantiateStoreItem();
            LoadPrice(_itemData, _itemType);
            ResumeToggleTarget();
        }

        // 載入角色完成後 實體化 角色
        if (m_MPGame.GetAssetLoader().loadedObj && _bLoadedActor)
        {
            _bLoadedActor = !_bLoadedActor;
            _bLoadedActor = InstantiateActor(_lastItemBtn.GetComponentInChildren<UISprite>().name, infoGroupsArea[(int)ENUM_Area.ItemInfoBox].transform.Find(StoreType.Mice.ToString()).GetChild(0).Find("Image"), actorScale);
        }
    }

    /// <summary>
    /// 實體化商店物品
    /// </summary>
    private void InstantiateStoreItem()
    {
        // 實體化的商品背景(按鈕)
        Dictionary<string, GameObject> itemBtnDict = InstantiateItemBG(_itemData, StoreType.Item.ToString(), _itemType, infoGroupsArea[(int)ENUM_Area.ItemPanel].transform, itemOffset, tablePageCount, tableRowCount);
        Transform parent = infoGroupsArea[(int)ENUM_Area.ItemPanel].transform.FindChild(_itemType.ToString());

        // 如果有新商品 加入索引 並 加入按鈕事件
        if (itemBtnDict != null)
        {
            dictItemRefs = dictItemRefs.Concat(itemBtnDict).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value);
            AddItemBtnEvent_OnClick(itemBtnDict);
        }

        //實體化 商品圖示
        InstantiateItemIcon(GetStoreItemDataAndFolderPath(_itemType), parent);// 選擇商店資料後 實體化物件
    }

    /// <summary>
    /// 增加商品按下時 顯示資訊 按鈕事件
    /// </summary>
    /// <param name="itemBtnDict"></param>
    private void AddItemBtnEvent_OnClick(Dictionary<string, GameObject> itemBtnDict)
    {
        foreach (KeyValuePair<string, GameObject> item in itemBtnDict)
            UIEventListener.Get(item.Value).onClick += OnItemClick;
    }

    #region -- OnLoadPanel 載入面板 --
    protected override void OnLoading()
    {
        LoadGashaponAsset();
        Global.photonService.LoadStoreData();
        Global.photonService.LoadItemData();
        Global.photonService.LoadCurrency(Global.Account);
        Global.photonService.LoadPlayerItem(Global.Account);
    }

    private void OnLoadStoreData()
    {
        _bLoadedStoreData = true;
    }

    private void OnLoadItemData()
    {
        _bLoadedItemData = true;
    }
    private void OnLoadCurrency()
    {
        _bLoadedCurrency = true;
    }

    private void OnLoadPlayerItem()
    {
        _bLoadedPlayerItem = true;
    }

    protected override void OnLoadPanel()
    {
        if (transform.parent.gameObject.activeSelf)
        {
            if (_bFirstLoad)
                _bFirstLoad = false;

            _itemData = Global.storeItem;

            //   LoadGashaponAsset(assetFolder[0]);
            GetMustLoadAsset();
            LoadPlayerInfo();
            EventMaskSwitch.lastPanel = gameObject;
        }
        Global.isStoreLoaded = false;
    }
    #endregion

    #region -- LoadPlayerInfo 載入玩家資訊 --
    private void LoadPlayerInfo()
    {
        infoGroupsArea[(int)ENUM_Area.PlayerInfo].transform.GetChild(0).GetComponent<UILabel>().text = Global.Rank.ToString();
        infoGroupsArea[(int)ENUM_Area.PlayerInfo].transform.GetChild(1).GetComponent<UILabel>().text = Global.Rice.ToString();
        infoGroupsArea[(int)ENUM_Area.PlayerInfo].transform.GetChild(2).GetComponent<UILabel>().text = Global.Gold.ToString();
    }
    #endregion

    #region -- OnClick 按下事件 --
    public void OnGashaponClick(GameObject obj)
    {
        Debug.Log(obj.name);
    }

    /// <summary>
    /// 商品按鈕事件(顯示商品資訊)
    /// </summary>
    /// <param name="obj">商品按鈕</param>
    public void OnItemClick(GameObject obj)
    {
        _lastItemBtn = obj;

        Transform itemInfoBox = infoGroupsArea[(int)ENUM_Area.ItemInfoBox].transform;
        Dictionary<string, object> dictItemProperty = Global.storeItem[_lastItemBtn.name] as Dictionary<string, object>;
        Dictionary<string, object> playerItemData = new Dictionary<string, object>();

        // 取得玩家道具資詳細資料
        if (Global.playerItem.ContainsKey(_lastItemBtn.name)) playerItemData = Global.playerItem[_lastItemBtn.name] as Dictionary<string, object>;

        // 取得商品類別 資產資料夾位子
        _folderString = assetFolder[_itemType];

        // 因為子物件不能有不同的Layer強制改變
        itemInfoBox.gameObject.SetActive(true);
        itemInfoBox.gameObject.layer = itemInfoBox.parent.gameObject.layer = LayerMask.NameToLayer("ItemInfo");

        // 載入選擇的資料
        switch (_itemType)
        {
            default:
            case (int)StoreType.Mice:
                _itemData = Global.miceProperty;
                LoadMicePorperty(obj, itemInfoBox, playerItemData, dictItemProperty);
                break;
            case (int)StoreType.Item:
            case (int)StoreType.Armor:
                _itemData = Global.itemProperty;
                LoadItemPorperty(obj, itemInfoBox, playerItemData, dictItemProperty);
                break;
        }

        EventMaskSwitch.Switch(itemInfoBox.gameObject);
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
        itemInfoBox.Find(StoreType.Item.ToString()).gameObject.SetActive(false);
        itemInfoBox.Find(StoreType.Mice.ToString()).gameObject.SetActive(true);

        // 載入商品資訊
        LoadProperty.LoadItemProperty(item_Click, itemInfoBox.Find(StoreType.Mice.ToString()).GetChild(0).gameObject, Global.miceProperty, _itemType);
        LoadProperty.LoadItemProperty(item_Click, itemInfoBox.Find(StoreType.Mice.ToString()).GetChild(0).gameObject, _itemData, _itemType);
        LoadProperty.LoadPrice(item_Click, itemInfoBox.transform.Find(StoreType.Mice.ToString()).GetChild(0).gameObject, _itemType);

        // 異步載入角色
        _bLoadedActor = LoadActor(item_Click, itemInfoBox.Find(StoreType.Mice.ToString()).GetChild(0).Find("Image"), actorScale);

        // 載入 對應商品 的 玩家道具數量
        playerItemData.TryGetValue(PlayerItem.ItemCount.ToString(), out value);
        itemInfoBox.Find(StoreType.Mice.ToString()).GetChild(0).Find("Count").GetComponent<UILabel>().text = value.ToString();

        // 載入商品資訊
        dictItemProperty.TryGetValue(StoreProperty.Description.ToString(), out value);
        itemInfoBox.Find(StoreType.Mice.ToString()).GetChild(0).Find(StoreProperty.Description.ToString()).GetComponent<UILabel>().text = value.ToString();
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
        itemInfoBox.Find(StoreType.Item.ToString()).gameObject.SetActive(true);
        itemInfoBox.Find(StoreType.Mice.ToString()).gameObject.SetActive(false);

        // 載入商品資訊
        LoadProperty.LoadItemProperty(item_Click, itemInfoBox.Find(StoreType.Item.ToString()).GetChild(0).gameObject, _itemData, _itemType);
        LoadProperty.LoadPrice(item_Click, itemInfoBox.Find(StoreType.Item.ToString()).GetChild(0).gameObject, _itemType);

        itemInfoBox.Find(StoreType.Item.ToString()).GetChild(0).Find("Image").GetComponent<UISprite>().atlas = _lastItemBtn.GetComponentInChildren<UISprite>().atlas;

        // 載入商品資訊
        dictItemProperty.TryGetValue(StoreProperty.ItemName.ToString(), out value);
        itemInfoBox.Find(StoreType.Item.ToString()).GetChild(0).Find("Image").GetComponent<UISprite>().spriteName = value.ToString().Replace(" ", "") + Global.IconSuffix;
        dictItemProperty.TryGetValue(StoreProperty.Description.ToString(), out value);
        itemInfoBox.Find(StoreType.Item.ToString()).GetChild(0).Find(StoreProperty.Description.ToString()).GetComponent<UILabel>().text = value.ToString();

        // 載入 對應商品 的 玩家道具數量
        playerItemData.TryGetValue(PlayerItem.ItemCount.ToString(), out value);
        itemInfoBox.Find(StoreType.Item.ToString()).GetChild(0).Find("Count").GetComponent<UILabel>().text = value.ToString();
    }

    /// <summary>
    /// 按下購買時
    /// </summary>
    /// <param name="myPanel"></param>
    public void OnBuyClick(GameObject myPanel)
    {
        // 關閉 商品詳情視窗 並顯示 購買數量視窗
        myPanel.SetActive(false);
        infoGroupsArea[(int)ENUM_Area.CheckoutBox].SetActive(true);

        // 初始化視窗資訊
        BuyWindowsInit();

        // 因為子物件不能有不同的Layer強制改變
        infoGroupsArea[(int)ENUM_Area.CheckoutBox].layer = LayerMask.NameToLayer("BuyInfo");
        EventMaskSwitch.Switch(infoGroupsArea[(int)ENUM_Area.CheckoutBox]);
    }

    /// <summary>
    /// 初始化視窗資訊
    /// </summary>
    private void BuyWindowsInit()
    {
        object value;
        Transform checkoutBox = infoGroupsArea[(int)ENUM_Area.CheckoutBox].transform;
        Dictionary<string, object> dictItemProperty = Global.storeItem[_lastItemBtn.name] as Dictionary<string, object>;

        //string colunmsName = (_itemType == (int)MPProtocol.StoreType.Mice) ? "MiceID" : "ItemID";

        // 基礎購買量
        buyingGoodsData[StoreProperty.BuyCount.ToString()] = "1";

        // 暫存 將購買的商品資訊
        dictItemProperty.TryGetValue(StoreProperty.ItemID.ToString(), out value);
        buyingGoodsData[StoreProperty.ItemID.ToString()] = value.ToString();

        dictItemProperty.TryGetValue(StoreProperty.ItemType.ToString(), out value);
        buyingGoodsData[StoreProperty.ItemType.ToString()] = value.ToString();

        dictItemProperty.TryGetValue(StoreProperty.CurrencyType.ToString(), out value);
        buyingGoodsData[StoreProperty.CurrencyType.ToString()] = value.ToString();

        dictItemProperty.TryGetValue(StoreProperty.ItemName.ToString(), out value);
        buyingGoodsData[StoreProperty.ItemName.ToString()] = value.ToString();

        // 顯示圖示
        checkoutBox.GetChild(0).Find("Image").GetComponent<UISprite>().atlas = _lastItemBtn.GetComponentInChildren<UISprite>().atlas;
        checkoutBox.GetChild(0).Find("Image").GetComponent<UISprite>().spriteName = value.ToString().Replace(" ", "") + Global.IconSuffix;

        // 顯示商品資訊
        checkoutBox.GetChild(0).FindChild("Count").GetComponent<UILabel>().text = "1";  // count = 1
        dictItemProperty.TryGetValue("Price", out value);
        checkoutBox.GetChild(0).FindChild("Sum").GetComponent<UILabel>().text = value.ToString(); // price
        checkoutBox.GetChild(0).FindChild("Price").GetComponent<UILabel>().text = value.ToString(); // price
    }

    /// <summary>
    /// 改變道具數量時 計算總價格 數量
    /// </summary>
    /// <param name="obj"></param>
    public void OnQuantity(GameObject obj)
    {
        Transform checkoutBox = infoGroupsArea[(int)ENUM_Area.CheckoutBox].transform;
        int price = int.Parse(checkoutBox.GetChild(0).FindChild("Price").GetComponent<UILabel>().text);
        int sum = int.Parse(checkoutBox.GetChild(0).FindChild("Sum").GetComponent<UILabel>().text);
        int count = int.Parse(checkoutBox.GetChild(0).FindChild("Count").GetComponent<UILabel>().text);

        count += (obj.name == "Add") ? 1 : -1;
        count = (count < 0) ? 0 : count;
        buyingGoodsData[StoreProperty.BuyCount.ToString()] = checkoutBox.GetChild(0).FindChild("Count").GetComponent<UILabel>().text = count.ToString();
        checkoutBox.GetChild(0).FindChild("Sum").GetComponent<UILabel>().text = (price * count).ToString();
    }

    /// <summary>
    /// 確認購買
    /// </summary>
    /// <param name="myPanel"></param>
    public void OnComfirm(GameObject myPanel)
    {
        myPanel.SetActive(false);
        Global.photonService.BuyItem(Global.Account, buyingGoodsData);
    }

    public override void OnClosed(GameObject obj)
    {
        EventMaskSwitch.lastPanel = null;
        GameObject root = obj.transform.parent.gameObject;

        GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().LoadPanel(obj.transform.parent.gameObject);
        EventMaskSwitch.Resume();
    }

    /// <summary>
    /// 返回事件遮罩
    /// </summary>
    /// <param name="obj"></param>
    public void OnReturn(GameObject obj)
    {
        int level = int.Parse(obj.name);
        EventMaskSwitch.openedPanel.SetActive(false);
        EventMaskSwitch.Prev(level);
    }

    /// <summary>
    /// 點選商品分頁時 載入商品
    /// </summary>
    /// <param name="obj"></param>
    public void OnTabClick(GameObject obj)
    {
        _itemType = (obj == null) ? (int)StoreType.Mice : int.Parse(obj.name.Remove(0, 3));
        GetMustLoadAsset();
        infoGroupsArea[(int)ENUM_Area.ItemPanel].SetActive(true);
    }
    #endregion

    /// <summary>
    /// 載入必要資產
    /// </summary>
    protected override void GetMustLoadAsset()
    {
        Dictionary<string, object> itemDetailData;

        // 取得道具資料
        _itemData = GetStoreItemDataAndFolderPath(_itemType);

        // 道具詳細資料
        itemDetailData = MPGFactory.GetObjFactory().GetItemDetailsInfoFromType(_itemData, _itemType);

        // 載入資產
        if (!assetLoader.GetAsset(_folderString))
        {
            assetLoader.init();
            assetLoader.LoadAsset(_folderString + "/", _folderString);
            assetLoader.LoadPrefab("Panel/", "Item");   // 道具Slot
            _bLoadedAsset = LoadIconObject(GetItemNameData(itemDetailData), _folderString);
        }
        else if (GetDontNotLoadAsset(itemDetailData).Count > 0)
        {
            _bLoadedAsset = LoadIconObject(GetItemNameData(itemDetailData), _folderString);
        }
    }


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
            dictItemRefs[item.Key].transform.FindChild("Price").GetComponent<UILabel>().text = price.ToString();
        }
    }
    #endregion

    #region -- GetItemNameData 取得道具名稱資料 --


    /// <summary>
    /// 取得道具名稱資料字典(name,name)
    /// </summary>
    /// <param name="itemData">物件陣列</param>
    /// <returns>Dictionary(ItemName,ItemName)</returns>
    private Dictionary<string, object> GetItemNameData(Dictionary<string, object> itemData)    // 載入遊戲物件
    {
        Dictionary<string, object> data = new Dictionary<string, object>();

        foreach (KeyValuePair<string, object> item in itemData)
        {
            var nestedData = item.Value as Dictionary<string, object>;
            object itemName;
            nestedData.TryGetValue("ItemName", out itemName);
            data.Add(itemName.ToString(), itemName);
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
            string bundleName = itemName.ToString() + Global.IconSuffix;

            // 已載入資產時
            if (assetLoader.GetAsset(bundleName) != null)
            {
                GameObject bundle = assetLoader.GetAsset(bundleName);
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
        _bLoadedAsset = false;
    }
    #endregion

    /// <summary>
    /// 選擇商店資料
    /// </summary>
    /// <param name="_itemType">道具類別</param>
    private Dictionary<string, object> GetStoreItemDataAndFolderPath(int _itemType)
    {
        _folderString = assetFolder[_itemType];

        switch ((StoreType)_itemType)
        {
            default:
            case StoreType.Mice:
            case StoreType.Item:
                _itemData = Global.storeItem;
                break;
            case StoreType.Gashapon:
                _itemData = Global.gashaponItem;
                break;
        }

        return _itemData;
    }

    void OnDisable()
    {
        Global.photonService.LoadStoreDataEvent -= OnLoadStoreData;
        Global.photonService.LoadPlayerItemEvent -= OnLoadPlayerItem;
        Global.photonService.LoadCurrencyEvent -= OnLoadCurrency;
        Global.photonService.LoadItemDataEvent -= OnLoadItemData;
        Global.photonService.UpdateCurrencyEvent -= LoadPlayerInfo;
    }

    #region -- LoadGashapon 載入轉蛋物件(亂寫) --
    private void LoadGashaponAsset()
    {
        GetStoreItemDataAndFolderPath((int)StoreType.Gashapon);
        assetLoader.init();
        assetLoader.LoadAsset(_folderString + "/", _folderString);
        for (int i = 1; i <= 3; i++)
            assetLoader.LoadPrefab(_folderString + "/", _folderString + i);
        _bLoadedGashapon = true;
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
        GetStoreItemDataAndFolderPath((int)StoreType.Gashapon);

        Dictionary<string, object> dictGashaponData = new Dictionary<string, object>();
        List<string> itemNameList = new List<string>();
        object itemType, itemName;

        foreach (var item in Global.storeItem)
        {
            Dictionary<string, object> values = item.Value as Dictionary<string, object>;
            values.TryGetValue(StoreProperty.ItemType.ToString(), out itemType);
            values.TryGetValue(StoreProperty.ItemName.ToString(), out itemName);
            itemNameList.Add(itemName.ToString());
            if ((int)itemType == (int)StoreType.Gashapon)
                dictGashaponData.Add(item.Key, item.Value);
        }

        int i = 0;

        foreach (KeyValuePair<string, object> gashapon in dictGashaponData)
        {
            if (assetLoader.GetAsset(itemNameList[i]) != null)                  // 已載入資產時
            {
                GameObject bundle = assetLoader.GetAsset(itemNameList[i]);
                Transform parent = infoGroupsArea[(int)ENUM_Area.Gashapon].transform.Find("Promotions");
                MPGFactory.GetObjFactory().Instantiate(bundle, parent, itemNameList[i], new Vector3(75, 100), Vector3.one, new Vector2(180, 180), -1);
                parent.GetChild(i).name = itemNameList[i];
            }
            else
            {
                Debug.LogError("Assetbundle reference not set to an instance.");
            }
            i++;
        }
        _bLoadedGashapon = true;
    }
    #endregion



    public void OnBuyGashapon(GameObject go)
    {
        _lastItemBtn = go;
        infoGroupsArea[(int)ENUM_Area.GashaponBox].SetActive(true);

        // 初始化視窗資訊
        GashaponWindowsInit();

        // 因為子物件不能有不同的Layer強制改變
        infoGroupsArea[(int)ENUM_Area.GashaponBox].layer = LayerMask.NameToLayer("BuyInfo");
        EventMaskSwitch.Switch(infoGroupsArea[(int)ENUM_Area.GashaponBox]);
    }

    private void GashaponWindowsInit()
    {
        object value;
        Transform checkoutBox = infoGroupsArea[(int)ENUM_Area.GashaponBox].transform;
        Dictionary<string, object> dictItemProperty = Global.storeItem[_lastItemBtn.name] as Dictionary<string, object>;

        //string colunmsName = (_itemType == (int)MPProtocol.StoreType.Mice) ? "MiceID" : "ItemID";

        //// 基礎購買量
        //buyingGoodsData[StoreProperty.BuyCount.ToString()] = "1";

        // 暫存 將購買的商品資訊
        dictItemProperty.TryGetValue(StoreProperty.ItemID.ToString(), out value);
        buyingGoodsData[StoreProperty.ItemID.ToString()] = value.ToString();

        dictItemProperty.TryGetValue(StoreProperty.ItemType.ToString(), out value);
        buyingGoodsData[StoreProperty.ItemType.ToString()] = value.ToString();

        dictItemProperty.TryGetValue(StoreProperty.CurrencyType.ToString(), out value);
        buyingGoodsData[StoreProperty.CurrencyType.ToString()] = value.ToString();

        dictItemProperty.TryGetValue(StoreProperty.ItemName.ToString(), out value);
        buyingGoodsData[StoreProperty.ItemName.ToString()] = value.ToString();

        // 顯示圖示
        //  checkoutBox.GetChild(0).Find("Image").GetComponent<UISprite>().atlas = _lastItemBtn.GetComponentInChildren<UISprite>().atlas;
        checkoutBox.GetChild(0).Find("Image").GetComponent<UISprite>().spriteName = value.ToString()/*.Replace(" ", "") + Global.IconSuffix*/;

        // 顯示商品資訊
        // checkoutBox.GetChild(0).FindChild("Count").GetComponent<UILabel>().text = "1";  // count = 1
        dictItemProperty.TryGetValue("Price", out value);
        //  checkoutBox.GetChild(0).FindChild("Sum").GetComponent<UILabel>().text = value.ToString(); // price
        checkoutBox.GetChild(0).FindChild("Price").GetComponent<UILabel>().text = value.ToString(); // price
    }

    /// <summary>
    /// 確認購買轉蛋
    /// </summary>
    /// <param name="go">Series </param>
    public void OnConfirmGashapon(GameObject go)
    {
        Global.photonService.BuyGashapon(buyingGoodsData[StoreProperty.ItemID.ToString()], buyingGoodsData[StoreProperty.ItemType.ToString()], go.name);
    }


    private void OnGetGashapon(List<string> itemList)
    {
        // show panel
        // show animation
        // skip
    }

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
    //#endregion
}
