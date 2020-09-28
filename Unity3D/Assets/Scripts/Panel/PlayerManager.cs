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

public class PlayerManager : IMPPanelUI
{
    public static Dictionary<string, GameObject> dictLoadedEquiped { get; set; }
    public static Dictionary<string, GameObject> dictLoadedItem { get; set; }
    public static UISprite playerImage;

    public GameObject playerInfoArea, equipArea, playerRecordArea, playerAvatarIconGrid, inventoryArea, iconBtn, equipBtn;
    public Vector2 itemOffset, iconSize;
    public Vector3 actorScale;
    public int itemCount = 12, row = 5, _itemType, iconDepth = 310;

    private static Dictionary<string, object> _dictItemData /*,_dictEquipData*/;        // 道具資料、裝備資料
    private GameObject _playerIconInventoryPanel, _playerEquipInventoryPanel;
    private bool _bLoadedPlayerItem, _bLoadItem, _bLoadPlayerData, _bUpdatePlayerImage, _bLoadedCurrency, _bLoadedPanel, _bFirstLoad, _LoadedAsset, _bImgActive, _bInvActive, _bPanelFirstLoad, _bLoadedPlayerAvatarIcon, _bInsEquip;
    private int _dataLoadedCount;

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
        m_RootUI = GameObject.Find("Player(Panel)");
    }

    void OnEnable()
    {
        _bLoadedPanel = false;
        // 監聽事件
        Global.photonService.LoadItemDataEvent += OnLoadItem;
        Global.photonService.LoadPlayerDataEvent += OnLoadPlayerData;
        Global.photonService.LoadCurrencyEvent += OnLoadCurrency;
        Global.photonService.LoadPlayerItemEvent += OnLoadPlayerItem;
        Global.photonService.UpdatePlayerImageEvent += OnUpdatePlayerImage;
    }

    public override void Update()
    {
        // 資料庫資料載入完成時 載入Asset
        if (_dataLoadedCount == GetMustLoadedDataCount() && !_bLoadedPanel)
        {
            _bLoadedPanel = true;
            OnLoadPanel();
        }

        // Asset載入完成時 載入玩家頭像、實體化裝備圖示
        if (m_MPGame.GetAssetLoader().bLoadedObj && _LoadedAsset)
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

        // 重新載入玩家頭像
        if (_bUpdatePlayerImage)
            LoadPlayerAvatorIcon();

    }

    /// <summary>
    /// 載入Panel時
    /// </summary>
    protected override void OnLoading()
    {
        _dataLoadedCount = (int)ENUM_Data.None;

        if (Global.isMatching)
            Global.photonService.ExitWaitingRoom();

        // 載入 資料庫資料
        Global.photonService.LoadItemData();
        Global.photonService.LoadPlayerData(Global.Account);
        Global.photonService.LoadCurrency(Global.Account);
        Global.photonService.LoadPlayerItem(Global.Account);
    }

    /// <summary>
    /// 載入 Panel 完成時
    /// </summary>
    protected override void OnLoadPanel()
    {
        EventMaskSwitch.lastPanel = m_RootUI.transform.GetChild(0).gameObject; //儲存目前Panel
        GetMustLoadAsset();
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
            assetLoader.LoadAssetFormManifest(Global.PanelUniquePath + Global.InvItemAssetName + Global.ext);  // 載入背包背景資產
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
        }

        // 如果 有未載入物件 載入AB
        if (dictNotLoadedAsset.Count != 0)
            LoadIconObjects(dictNotLoadedAsset, Global.ItemIconUniquePath); // 載入道具ICON物件

        // 載入老鼠ICON
        foreach (KeyValuePair<string, object> item in Global.dictMiceAll)
            if (!assetLoader.GetAsset(item.Value.ToString()))
                assetLoader.LoadAssetFormManifest(Global.MiceIconUniquePath + Global.IconSuffix + item.Value + Global.ext);

        _dictItemData = Global.playerItem;
        _LoadedAsset = true;
        AssetBundleManager.LoadedAllAsset();
        ResumeToggleTarget();
    }

    /// <summary>
    /// 載入 玩家頭像
    /// </summary>
    private void LoadPlayerAvatorIcon()
    {
        _bUpdatePlayerImage = false;
        Transform imageParent = playerInfoArea.transform.Find("Image");

        if(imageParent.childCount==0)
            MPGFactory.GetObjFactory().Instantiate(assetLoader.GetAsset(Global.PlayerImage), imageParent, "AvatarImage", Vector3.one, Vector2.one, new Vector2(280, 280), -390);

        playerImage = imageParent.GetChild(0).GetComponent<UISprite>();
        playerImage.spriteName = Global.PlayerImage;
     //   imageParent.parent.tag = "EquipICON";
    }


    // ins go
    #region -- InstantiateBagItemIcon 實體化背包道具 --
    /// <summary>
    /// 實體化載入完成的遊戲物件，利用玩家JSON資料判斷必要實體物件
    /// </summary>
    /// <param name="itemData">資料字典</param>
    /// <param name="itemPanel">實體化父系位置</param>
    /// <param name="itemType">道具類別</param>
    private void InstantiateBagItemIcon(Dictionary<string, object> itemData, Transform itemPanel, int itemType)
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
                bundleName = Global.IconSuffix + itemName;
                //bundleName = AssetBundleManager.GetAssetBundleNamePath(Global.IconSuffix + itemName);

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
                        GameObject _clone = MPGFactory.GetObjFactory().Instantiate(bundle, imageParent, itemName, Vector3.zero, Vector3.one, new Vector2(iconSize.x, iconSize.y), iconDepth);
                        // _clone.GetComponentInParent<ButtonSwitcher>()._activeBtn = true;

                        object value;
                        nestedData.TryGetValue("ItemType", out value);

                        // 加入索引 老鼠所在的MiceBtn位置
                        if (itemType.ToString() == value.ToString())
                            if (!dictLoadedItem.ContainsKey(itemID.ToString()))
                                dictLoadedItem.Add(itemID.ToString(), imageParent.parent.gameObject);

                        // 如果已經裝備道具 顯示 已裝備狀態
                        nestedData.TryGetValue("IsEquip", out value);

                        //if (System.Convert.ToBoolean(value))
                        //{
                        //    imageParent.parent.SendMessage("DisableBtn"); // imageParent.parent = button
                        //}
                    }
                }
                else
                {
                    Debug.LogError("AssetBundle is null !");
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
    /// <param name="myParent">圖片位置</param>
    ///     /// <param name="itemType">道具型態</param>
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
                    string bundleName = Global.IconSuffix + itemName;
                    //  string bundleName = AssetBundleManager.GetAssetBundleNamePath(Global.IconSuffix + itemName);

                    // 已載入資產時
                    if (!string.IsNullOrEmpty(bundleName) && assetLoader.GetAsset(bundleName) != null)
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
                    else
                    {
                        Debug.LogError("Can't get assetbundle or assetbundle name is null.");
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
        float winRate = (Global.SumWin / Global.SumBattle) * 100f;

        parent.Find("HighScore").GetComponent<UILabel>().text = Global.MaxScore.ToString();
        parent.Find("Hit").GetComponent<UILabel>().text = Global.SumKill.ToString();
        parent.Find("Win").GetComponent<UILabel>().text = Global.SumWin.ToString();
        parent.Find("Combo").GetComponent<UILabel>().text = Global.MaxCombo.ToString();
        parent.Find("WinRate").GetComponent<UILabel>().text = winRate.ToString("F2") + " %";    //F2 = float 小數點第2位
        parent.Find("StrianghtWin").GetComponent<UILabel>().text = Global.SumWin.ToString();
        parent.Find("Lose").GetComponent<UILabel>().text = Global.SumLost.ToString();
    }
    #endregion

    // 當點擊 背包頭像時
    public void OnPlayerAvatarIconClick(GameObject obj)
    {
        // 如果裝備背包開啟中 則 關閉 (避免擋到頭像背包)
        if (_playerEquipInventoryPanel.activeSelf)
            OnEquipClick(obj);

        _bImgActive = !_bImgActive;
        _playerIconInventoryPanel.SetActive(_bImgActive);



        // 如果 頭像背包為空 實體化按鈕圖示
        if (playerAvatarIconGrid.transform.childCount == 0)
            InstantiateAvatarBagBtn(Global.dictMiceAll, Global.InvItemAssetName, playerAvatarIconGrid.transform, itemOffset, itemCount, row);

        // 開關防止Item按鈕失效
        playerAvatarIconGrid.SetActive(false);
        playerAvatarIconGrid.SetActive(true);
    }

    #region -- InstantiateAvatarBagIcon  實體化角色頭像背包物件--
    /// <summary>
    /// 實體化角色頭像背包物件
    /// </summary>
    /// <param name="itemData">ICON道具資料</param>
    /// <param name="invBtnItemName"></param>
    /// <param name="itemPanel"></param>
    /// <param name="offset"></param>
    /// <param name="tableCount"></param>
    /// <param name="rowCount"></param>
    /// <returns></returns>
    public Dictionary<string, GameObject> InstantiateAvatarBagBtn(Dictionary<string, object> itemData, string invBtnItemName, Transform itemPanel, Vector2 offset, int tableCount, int rowCount)
    {
        if (itemPanel.transform.childCount == 0)                // 如果還沒建立道具
        {
            _lastEmptyItemGroup = CreateEmptyObject(itemPanel, (int)StoreType.Mice);
            Vector2 pos = new Vector2();
            int i = 0;

            foreach (KeyValuePair<string, object> item in itemData)
            {
                if (assetLoader.GetAsset(invBtnItemName) != null)                  // 已載入按鈕資產時
                {

                    GameObject bundle = assetLoader.GetAsset(invBtnItemName);
                    pos = sortItemPos(tableCount, rowCount, offset, pos, i);
                    bundle = MPGFactory.GetObjFactory().Instantiate(bundle, _lastEmptyItemGroup.transform, item.Key, new Vector3(pos.x, pos.y), Vector3.one, Vector2.zero, -1);

                    string iconName = Global.IconSuffix + item.Value;
                    //string iconName = AssetBundleManager.GetAssetBundleNamePath(Global.IconSuffix + item.Value);
                    if (assetLoader.GetAsset(iconName) != null)                  // 已載入ICON資產時
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
        // 如果裝備背包開啟中 則 關閉 (避免擋到頭像背包)
        if (_playerIconInventoryPanel.activeSelf)
            OnPlayerAvatarIconClick(obj);

        _bInvActive = !_bInvActive;
        _playerEquipInventoryPanel.SetActive(_bInvActive);

        // 如果 玩家頭像背包 還沒有載入
        if (inventoryArea.transform.childCount == 0)
        {
            //取得開啟的裝備類別(武器、衣服、道具等)
            if (obj.GetComponentInChildren<UISprite>())
                _itemType = System.Convert.ToInt32(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.playerItem, "ItemType", obj.name));
            else
                _itemType = int.Parse(obj.transform.parent.name);

            //實體化道具格、圖示
            Dictionary<string, GameObject> bag = InstantiateBagItemBG(_dictItemData, Global.InvItemAssetName, _itemType, inventoryArea.transform, itemOffset, itemCount, row);
            InstantiateBagItemIcon(_dictItemData, _lastEmptyItemGroup.transform, _itemType);
        }

        // 開關防止Item按鈕失效
        inventoryArea.SetActive(false);
        inventoryArea.SetActive(true);
    }

    /// <summary>
    /// 載入資料庫 玩家資料完成時
    /// </summary>
    void OnLoadPlayerData()
    {
        _dataLoadedCount *= (int)ENUM_Data.PlayerData;
    }

    /// <summary>
    /// 載入資料庫 玩家道具資料完成時
    /// </summary>
    void OnLoadPlayerItem()
    {
        _dataLoadedCount *= (int)ENUM_Data.PlayerItem;
    }

    /// <summary>
    /// 載入資料庫 道具資料完成時
    /// </summary>
    void OnLoadItem()
    {
        _dataLoadedCount *= (int)ENUM_Data.ItemData;
    }

    /// <summary>
    /// 載入資料庫 金流資料完成時
    /// </summary>
    void OnLoadCurrency()
    {
        _dataLoadedCount *= (int)ENUM_Data.CurrencyData;
    }

    /// <summary>
    /// 載入資料庫 玩家資料完成時
    /// </summary>
    void OnUpdatePlayerImage()
    {
        _bUpdatePlayerImage = true;
    }

    /// <summary>
    /// 關閉Panel
    /// </summary>
    /// <param name="obj">Panel</param>
    public override void OnClosed(GameObject obj)
    {
        EventMaskSwitch.lastPanel = null;
        ShowPanel(obj.transform.parent.name);
        //  GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().LoadPanel(obj.transform.parent.gameObject);
    }

    // Panel 關閉時
    void OnDisable()
    {
        // -event 移除事件監聽 (載入道具資料時、載入玩家資料時、載入玩家道具資料時)
        Global.photonService.LoadItemDataEvent -= OnLoadItem;
        Global.photonService.LoadPlayerDataEvent -= OnLoadPlayerData;
        Global.photonService.LoadCurrencyEvent -= OnLoadCurrency;
        Global.photonService.LoadPlayerItemEvent -= OnLoadPlayerItem;
        Global.photonService.UpdatePlayerImageEvent -= OnUpdatePlayerImage;
    }

    protected override int GetMustLoadedDataCount()
    {
        return (int)ENUM_Data.PlayerData * (int)ENUM_Data.PlayerItem * (int)ENUM_Data.ItemData * (int)ENUM_Data.CurrencyData;
    }

    public override void Initinal()
    {
        throw new System.NotImplementedException();
    }

    public override void Release()
    {
        throw new System.NotImplementedException();
    }
}
