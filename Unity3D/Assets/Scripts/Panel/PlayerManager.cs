using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
 * 負責 開啟/關閉/載入 PlayerPanel的的所有處理
 * 125行 目前還沒載入載人圖片檔案 如要要載入需要移除 且須改寫部分程式
 * _dictEquipData 還沒寫 裝備排序
 * 道具排序未照PlayerData排序(未寫)
 * ***************************************************************
 *                           ChangeLog
 * 20161102 v1.0.2   3次重構，改變繼承至 PanelManager>MPPanel
 * 20160914 v1.0.1b  2次重構，獨立實體化物件                          
 * 20160711 v1.0.1a  1次重構，獨立AssetLoader                       
 * 20160705 v1.0.0   0版完成，載入老鼠部分未來需要修改
 * 20170321 v2.0.0   1版完成           
 * 20171119 v2.1.0   修正載入順序
 * ****************************************************************/

public class PlayerManager : MPPanel
{
    public static Dictionary<string, GameObject> dictLoadedEquiped { get; set; }
    public static Dictionary<string, GameObject> dictLoadedItem { get; set; }
    public static UISprite playerImage;

    public GameObject playerInfoArea, equipArea, playerRecordArea, playerAvatarIconGrid, inventoryArea, iconBtn, equipBtn;
    public Vector2 itemOffset, iconSize;
    public Vector3 actorScale;
    public int itemCount = 8, row = 2, _itemType, iconDepth = 310;

    private static Dictionary<string, object> _dictItemData /*,_dictEquipData*/;        // 道具資料、裝備資料
    private GameObject _playerIconInventoryPanel, _playerEquipInventoryPanel;
    private static string _MiceIconPath = "miceicon", _ItemIconPath = "itemicon", _PanelPath = "panel", _InvItem = "invitem";
    private bool _bLoadedPlayerItem, _bLoadItem, _bLoadPlayerData, _bLoadedCurrency, _bLoadedPanel, _bFirstLoad, _LoadedAsset, _bImgActive, _bInvActive, _bPanelFirstLoad, _bLoadedPlayerAvatarIcon, _bInsEquip;


    public PlayerManager(MPGame MPGame) : base(MPGame) { }

    //init
    void Awake()
    {
        dictLoadedEquiped = new Dictionary<string, GameObject>();
        dictLoadedItem = new Dictionary<string, GameObject>();

        _playerIconInventoryPanel = playerAvatarIconGrid.transform.parent.parent.parent.gameObject;
        _playerEquipInventoryPanel = inventoryArea.transform.parent.parent.parent.gameObject;
        _dictItemData = Global.dictSortedItem;
        _itemType = (int)StoreType.Armor;
        _bLoadedCurrency = _bLoadedPanel = _bImgActive = _bInvActive = false;
        _bPanelFirstLoad = _bFirstLoad = true;
    }

    void OnEnable()
    {
        _bLoadedPanel = false;
        // 監聽事件
        Global.photonService.LoadItemDataEvent += OnLoadItem;
        Global.photonService.LoadPlayerDataEvent += OnLoadPlayerData;
        Global.photonService.LoadCurrencyEvent += OnLoadCurrency;
        Global.photonService.LoadPlayerItemEvent += OnLoadPlayerItem;
    }

    void Update()
    {

        // 資料庫資料載入完成時 載入Asset
        if (_bLoadedPlayerItem && _bLoadPlayerData && _bLoadedCurrency && _bLoadItem && !_bLoadedPanel)
        {
            _bLoadedPanel = true;
            OnLoadPanel();
        }

        // Asset載入完成時 載入玩家頭像、實體化裝備圖示
        if (m_MPGame.GetAssetLoader().loadedObj && _LoadedAsset)
        {
            _LoadedAsset = false;
            LoadPlayerAvatorIcon();
            InstantiateEquipIcon(Global.playerItem, equipArea.transform, (int)StoreType.Armor);
        }

        // 裝備實體化完成 載入 資料
        if (_bInsEquip)
        {
            //會發生還沒載入物件就載入資料
            _bInsEquip = Global.isPlayerDataLoaded = Global.isPlayerItemLoaded = Global.isItemLoaded = false;

            // 載入資料
            LoadPlayerInfo();
            LoadPlayerEquip();
            LoadPlayerRecord();

            // 載入畫面焦點 復原 至Panel
            ResumeToggleTarget();
        }


    }

    /// <summary>
    /// 載入Panel時
    /// </summary>
    protected override void OnLoading()
    {
        // 載入 資料庫資料
        Global.photonService.LoadItemData();
        Global.photonService.LoadPlayerData(Global.Account);
        Global.photonService.LoadCurrency(Global.Account);
        Global.photonService.LoadPlayerItem(Global.Account);
    }

    void OnLoadCurrency() {

        _bLoadedCurrency = true;
    }

    /// <summary>
    /// 載入資料庫 道具資料完成時
    /// </summary>
    void OnLoadItem()
    {
        _bLoadItem = true;
    }

    /// <summary>
    /// 載入資料庫 玩家資料完成時
    /// </summary>
    void OnLoadPlayerData()
    {
        _bLoadPlayerData = true;
    }

    /// <summary>
    /// 載入資料庫 玩家道具資料完成時
    /// </summary>
    void OnLoadPlayerItem()
    {
        _bLoadedPlayerItem = true;
    }


    /// <summary>
    /// 載入 Panel 完成時
    /// </summary>
    protected override void OnLoadPanel()
    {
        // load asset
        GetMustLoadAsset();
        EventMaskSwitch.lastPanel = gameObject;
    }

    /// <summary>
    /// 取得必須載入的Asset
    /// </summary>
    protected override void GetMustLoadAsset()
    {
        Dictionary<string, object> dictNotLoadedAsset = new Dictionary<string, object>();

        // 如果是第一次載入 取得未載入物件。 否則 取得相異(新的)物件
        if (_bFirstLoad)
        {
            assetLoader.LoadAsset(_MiceIconPath + "/", _MiceIconPath);
            dictNotLoadedAsset = GetDontNotLoadAsset(Global.playerItem, Global.itemProperty);
            _bFirstLoad = false;
        }
        else
        {
            LoadProperty.ExpectOutdataObject(Global.playerItem, _dictItemData, dictLoadedItem);
            LoadProperty.ExpectOutdataObject(Global.playerItem, _dictItemData, dictLoadedEquiped);

            // Where 找不存在的KEY 再轉換為Dictionary
            _dictItemData = Global.playerItem.Where(kvp => !_dictItemData.ContainsKey(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            dictNotLoadedAsset = GetDontNotLoadAsset(_dictItemData, Global.itemProperty);

            ResumeToggleTarget();
        }

        // 如果 有未載入物件 載入AB
        if (dictNotLoadedAsset.Count != 0)
        {
            assetLoader.init();
            //assetLoader.LoadAsset(assetFolder[_itemType] + "/", assetFolder[_itemType]);
            assetLoader.LoadAsset(_ItemIconPath + "/", _ItemIconPath);
            assetLoader.LoadPrefab(_PanelPath + "/", _InvItem);

            //_LoadedIcon = LoadIconObject(dictNotLoadedAsset, assetFolder[_itemType]);
            _LoadedAsset = LoadIconObject(dictNotLoadedAsset, _ItemIconPath);
        }
        else
        {
            m_MPGame.GetAssetLoader().loadedObj = _LoadedAsset = true;
        }

        foreach (KeyValuePair<string, object> item in Global.dictMiceAll)
            assetLoader.LoadPrefab(_MiceIconPath + "/", Global.IconSuffix+ item.Value);
        _dictItemData = Global.playerItem;
    }

    /// <summary>
    /// 載入 玩家頭像
    /// </summary>
    private void LoadPlayerAvatorIcon()
    {
        Transform imageParent = playerInfoArea.transform.Find("Image").GetChild(0).GetComponent<UISprite>().transform;
        imageParent.GetComponent<UISprite>().spriteName = Global.PlayerImage;
        playerImage = imageParent.GetComponent<UISprite>();
        imageParent.parent.tag = "EquipICON";
    }


    // ins go
    #region -- InstantiateItemIcon 實體化背包道具 --
    /// <summary>
    /// 實體化載入完成的遊戲物件，利用玩家JSON資料判斷必要實體物件
    /// </summary>
    /// <param name="itemData">資料字典</param>
    /// <param name="itemPanel">實體化父系位置</param>
    /// <param name="itemType">道具類別</param>
    private void InstantiateItemIcon(Dictionary<string, object> itemData, Transform itemPanel, int itemType)
    {
        // 如果道具形態正確 取得 該道具形態 的 道具資料
        if (itemType != -1)
            itemData = MPGFactory.GetObjFactory().GetItemDetailsInfoFromType(itemData, itemType);

        // 如果取得資料
        if (itemData.Count != 0)
        {
            int i = 0;

            // 實體化
            foreach (KeyValuePair<string, object> item in itemData)
            {
                var nestedData = item.Value as Dictionary<string, object>;
                object itemID;
                string itemName = "", bundleName = "";

                nestedData.TryGetValue("ItemID", out itemID);
                itemName = MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.itemProperty, "ItemName", itemID.ToString()).ToString();
                bundleName = Global.IconSuffix+ itemName;

                // 已載入資產時
                if (assetLoader.GetAsset(bundleName) != null)
                {
                    GameObject bundle = assetLoader.GetAsset(bundleName);
                    Transform imageParent = itemPanel.GetChild(i).GetChild(0);

                    // 如果沒有ICON才實體化
                    if (imageParent.childCount == 0)
                    {
                        imageParent.parent.name = itemID.ToString();
                        imageParent.parent.tag = "Inventory";
                        GameObject _clone = MPGFactory.GetObjFactory().Instantiate(bundle, imageParent, bundleName, Vector3.zero, Vector3.one, new Vector2(iconSize.x, iconSize.y), iconDepth);
                        _clone.GetComponentInParent<ButtonSwitcher>()._activeBtn = true;

                        object value;
                        nestedData.TryGetValue("ItemType", out value);

                        // 加入索引 老鼠所在的MiceBtn位置
                        if (itemType.ToString() == value.ToString())
                            if (!dictLoadedItem.ContainsKey(itemID.ToString()))
                                dictLoadedItem.Add(itemID.ToString(), imageParent.parent.gameObject);

                        // 如果已經裝備道具 顯示 已裝備狀態
                        nestedData.TryGetValue("IsEquip", out value);

                        if (System.Convert.ToBoolean(value))
                        {
                            imageParent.parent.SendMessage("DisableBtn"); // imageParent.parent = button
                        }
                    }
                }
                i++;
            }
        }
        else
        {
            Debug.LogError("Assetbundle reference not set to an instance.");
        }
    }
    #endregion

    #region -- InstantiateEquipIcon 實體化裝備中物件--
    /// <summary>
    /// 實體化載入完成的遊戲物件，利用玩家JSON資料判斷必要實體物件
    /// </summary>
    /// <param name="itemData">資料字典</param>
    /// <param name="parent">圖片位置</param>
    private void InstantiateEquipIcon(Dictionary<string, object> itemData, Transform myParent, int itemType)
    {
        int i = 0;
        object itemID;
        string itemName = "";
        Transform imageParent = myParent.GetChild(i).GetChild(0);

        foreach (KeyValuePair<string, object> item in itemData)
        {
            var nestedData = item.Value as Dictionary<string, object>;

            nestedData.TryGetValue("ItemID", out itemID);
            itemName = MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.itemProperty, "ItemName", itemID.ToString()).ToString();

            // 如果道具不在裝備欄位 
            if (!dictLoadedEquiped.ContainsKey(itemName))
            {
                object isEquip, type;
                nestedData.TryGetValue("IsEquip", out isEquip);
                nestedData.TryGetValue("ItemType", out type);

                if (System.Convert.ToBoolean(isEquip) && System.Convert.ToInt32(type) == itemType)                                // 如果道具是裝備狀態
                {
                    string bundleName = Global.IconSuffix+ itemName ;
                    // 已載入資產時
                    if (assetLoader.GetAsset(bundleName) != null)
                    {
                        // 如果沒有ICON才實體化
                        if (imageParent.childCount == 0)
                        {
                            imageParent.parent.name = itemID.ToString();
                            imageParent.parent.tag = "Equip";
                            UIEventListener.Get(imageParent.parent.gameObject).onClick += OnEquipClick;
                            GameObject _clone, bundle = assetLoader.GetAsset(bundleName);
                            _clone = MPGFactory.GetObjFactory().Instantiate(bundle, imageParent, bundleName, Vector3.zero, Vector3.one, new Vector2(iconSize.x, iconSize.y), iconDepth);
                            _clone.GetComponentInParent<ButtonSwitcher>().enabled = true;
                            _clone.GetComponentInParent<ButtonSwitcher>().SendMessage("EnableBtn");

                            if (!dictLoadedEquiped.ContainsKey(itemID.ToString()))
                                dictLoadedEquiped.Add(itemID.ToString(), imageParent.parent.gameObject);      // 參考至 老鼠所在的MiceBtn位置

                            i++;
                        }
                    }
                }
            }
        }

        // 加入按鈕監聽事件
        if (imageParent.childCount == 0)
        {
            UIEventListener.Get(imageParent.parent.gameObject).onClick += OnEquipClick;
        }

        _bInsEquip = true;
    }
    #endregion

    #region -- LoadPlayer(Info、Equip、Record) 載入玩家資訊 --
    private void LoadPlayerInfo()
    {
        Transform parent = playerInfoArea.transform;
        int exp = Clac.ClacExp(Mathf.Min(Global.Rank + 1, 100));

        parent.Find("Lv").GetComponent<UILabel>().text = Global.Rank.ToString();
        parent.Find("Rice").GetComponent<UILabel>().text = Global.Rice.ToString();
        parent.Find("Gold").GetComponent<UILabel>().text = Global.Gold.ToString();
        // parent.FindChild("Note").GetComponent<UILabel>().text = Global.MaxCombo.ToString();
        parent.Find("Exp").GetComponent<UILabel>().text = Global.Exp.ToString() + " / " + exp.ToString();
        parent.Find("Exp").Find("ExpBar").GetComponent<UISlider>().value = System.Convert.ToSingle(Global.Exp) / System.Convert.ToSingle(exp);
        parent.Find("Name").GetComponent<UILabel>().text = Global.Nickname.ToString();
    }

    private void LoadPlayerEquip()
    {

    }

    private void LoadPlayerRecord()
    {
        Transform parent = playerRecordArea.transform;
        float winRate = ((float)Global.SumWin / (float)Global.SumBattle) * 100f;

        parent.Find("HighScore").GetComponent<UILabel>().text = Global.MaxScore.ToString();
        parent.Find("Hit").GetComponent<UILabel>().text = Global.SumKill.ToString();
        parent.Find("Win").GetComponent<UILabel>().text = Global.SumWin.ToString();
        parent.Find("Combo").GetComponent<UILabel>().text = Global.MaxCombo.ToString();
        parent.Find("WinRate").GetComponent<UILabel>().text = winRate.ToString("F2") + " %";
        parent.Find("StrianghtWin").GetComponent<UILabel>().text = Global.SumWin.ToString();
        parent.Find("Lose").GetComponent<UILabel>().text = Global.SumLost.ToString();
    }
    #endregion

    // 當點擊 背包頭像時
    public void OnPlayerAvatarIconClick()
    {
        _bImgActive = !_bImgActive;
        _playerIconInventoryPanel.SetActive(_bImgActive);

        // 如果裝備背包開啟 則 關閉
        if (_playerEquipInventoryPanel.activeSelf)
            _playerEquipInventoryPanel.SetActive(false);

        // 如果 頭像背包為空 實體化按鈕圖示
        if (playerAvatarIconGrid.transform.childCount == 0)
            InstantiateICON(Global.dictMiceAll, "invitem", playerAvatarIconGrid.transform, itemOffset, itemCount, row);

        // 開關防止Item按鈕失效
        playerAvatarIconGrid.SetActive(false);
        playerAvatarIconGrid.SetActive(true);
    }

    #region -- InstantiateICON 實體化背包物件背景--
    /// <summary>
    /// 實體化商店物件背景
    /// </summary>
    /// <param name="itemData">資料字典</param>
    /// <param name="itemPanel">實體化父系位置</param>
    /// <param name="itemType">道具類別</param>
    public Dictionary<string, GameObject> InstantiateICON(Dictionary<string, object> itemData, string itemName, Transform itemPanel, Vector2 offset, int tableCount, int rowCount)
    {
        if (itemPanel.transform.childCount == 0)                // 如果還沒建立道具
        {
            _lastEmptyItemGroup = CreateEmptyObject(itemPanel, 1);
            Vector2 pos = new Vector2();
            Dictionary<string, GameObject> dictItem = new Dictionary<string, GameObject>();

            int i = 0;
            foreach (KeyValuePair<string, object> item in itemData)
            {
                if (assetLoader.GetAsset(itemName) != null)                  // 已載入資產時
                {

                    GameObject bundle = assetLoader.GetAsset(itemName);
                    pos = sortItemPos(12, 5, offset, pos, i);
                    bundle = MPGFactory.GetObjFactory().Instantiate(bundle, _lastEmptyItemGroup.transform, item.Key, new Vector3(pos.x, pos.y), Vector3.one, Vector2.zero, -1);

                    string iconName = Global.IconSuffix + item.Value ;
                    if (assetLoader.GetAsset(iconName) != null)                  // 已載入資產時
                    {
                        GameObject icon = assetLoader.GetAsset(iconName);
                        bundle = MPGFactory.GetObjFactory().Instantiate(icon, bundle.transform.Find("Image"), item.Key, Vector3.zero, Vector3.one, Vector2.zero, -1);
                        bundle.transform.parent.parent.gameObject.tag = "InventoryICON";
                        GameObject.Destroy(bundle.transform.parent.parent.GetComponent<ButtonSwitcher>());
                        GameObject.Destroy(bundle.transform.parent.parent.GetComponent<UIDragScrollView>());
                        GameObject.Destroy(bundle.transform.parent.parent.GetComponent<Rigidbody>());
                        GameObject.Destroy(bundle.transform.parent.parent.GetComponent<UIDragObject>());
                        bundle.transform.parent.parent.gameObject.AddComponent<ChangeICON>();
                    }
                    pos.x += offset.x;
                }

                i++;
            }
        }
        return null;
    }

    #endregion

    /// <summary>
    /// 開啟裝備背包
    /// </summary>
    /// <param name="obj">裝備部位按鈕</param>
    public void OnEquipClick(GameObject obj)
    {
        _bInvActive = !_bInvActive;

        //如果 背包為開啟 關閉
        if (_playerIconInventoryPanel.activeSelf)
            _playerIconInventoryPanel.SetActive(false);

        _playerEquipInventoryPanel.SetActive(_bInvActive);

        // 如果 玩家頭像背包 還沒有載入
        if (playerAvatarIconGrid.transform.childCount == 0)
        {
            // 如果裝備欄位是開啟的 關閉
            if (_playerIconInventoryPanel.activeSelf) OnPlayerAvatarIconClick();

            //取得開啟的裝備類別(武器、衣服、道具等)
            if (obj.GetComponentInChildren<UISprite>())
                _itemType = System.Convert.ToInt32(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.playerItem, "ItemType", obj.name));
            else
                _itemType = int.Parse(obj.transform.parent.name);

            //實體化道具格、圖示
            Dictionary<string, GameObject> bag = InstantiateItemBG(_dictItemData, _InvItem, _itemType, inventoryArea.transform, itemOffset, itemCount, row);



            InstantiateItemIcon(_dictItemData, _lastEmptyItemGroup.transform, _itemType);
        }

        // 開關防止Item按鈕失效
        inventoryArea.SetActive(false);
        inventoryArea.SetActive(true);
    }

    /// <summary>
    /// 關閉Panel
    /// </summary>
    /// <param name="obj">Panel</param>
    public override void OnClosed(GameObject obj)
    {
        EventMaskSwitch.lastPanel = null;
        GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().LoadPanel(obj.transform.parent.gameObject);
    }

    // Panel 關閉時
    void OnDisable()
    {
        // -event 移除事件監聽 (載入道具資料時、載入玩家資料時、載入玩家道具資料時)
        Global.photonService.LoadItemDataEvent -= OnLoadItem;
        Global.photonService.LoadPlayerDataEvent -= OnLoadPlayerData;
        Global.photonService.LoadCurrencyEvent -= OnLoadCurrency;
        Global.photonService.LoadPlayerItemEvent -= OnLoadPlayerItem;
    }
}
