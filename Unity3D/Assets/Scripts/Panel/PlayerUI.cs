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
 * 道具排序未照PlayerData排序(未寫)
 * ***************************************************************
 *                           ChangeLog
 * 20210111 v3.0.1 修正Equip背包
 * 20201027 v3.0.0  繼承重構
 * 20171119 v2.1.0   修正載入順序
 * 20170321 v2.0.0   1版完成       
 * 20160705 v1.0.0   0版完成，載入老鼠部分未來需要修改
 * 20160711 v1.0.1a  1次重構，獨立AssetLoader        
 * 20161102 v1.0.2   3次重構，改變繼承至 PanelManager>MPPanel
 * 20160914 v1.0.1b  2次重構，獨立實體化物件                          
 * ****************************************************************/
public class PlayerUI : IMPPanelUI
{
    #region Variables 變數
    AttachBtn_PlayerUI UI;

    public static Dictionary<string, GameObject> dictLoadedEquipedRefs { get; set; }        // 錯誤
    public static Dictionary<string, GameObject> dictLoadedItemRefs { get; set; }               // 錯誤

    private static Dictionary<string, object> _dictItemData /*,_dictEquipData*/;        // 道具資料、裝備資料
    private static GameObject _lastEmptyItemGroup;                                                          // 上一個實體的空群組

    private Vector2 _itemOffset;      // 道具位置間格
    private Vector2 _iconSize;           // Icon大小
    private int _itemCount;                // 道具數量
    private int _row;                            // 行數
    private int _iconDepth;                // Icon圖層深度
    private int _itemType;                  // 道具類型
    private int _dataLoadedCount; // 載入資料數量

    private bool _bLoadedPanel;                             // 是否載入Panel              
    private bool _bFirstLoad;                                    // 是否第一次載入                
    private bool _LoadedAsset;                                // 是否載入資產
    private bool _bLoadedAvatarIcon;                   // 是否載入角色Icon
    private bool _bInventoryActive;                       // 是否開啟背包
    private bool _bAvatarIconInventoryActive;  // 是否開啟角色頭像背包

    // private bool _bLoadPlayerData;                        // 是否載入玩家資料
    //  private bool _bLoadItem;                                    // 是否載入道具
    //  private bool _bLoadedPlayerItem;                   // 是否載入玩家道具資料
    //  private bool _bLoadedCurrency;                      // 是否載入金流資料
    //  private bool _bUpdateAvatarImage;               // 是否更新角色圖片
    //private bool _bInsEquip;
    #endregion

    public PlayerUI(MPGame MPGame) : base(MPGame)
    {
        Debug.Log("--------------- PlayerUI Create ----------------");
        dictLoadedEquipedRefs = new Dictionary<string, GameObject>();
        dictLoadedItemRefs = new Dictionary<string, GameObject>();
        _bFirstLoad = true;
    }

    public override void Initialize()
    {
        Debug.Log("--------------- PlayerUI Initialize ----------------");
        _dictItemData = Global.dictSortedItem;
        _itemType = (int)StoreType.Armor;
        _itemOffset = new Vector2(180, -180);
        _iconSize = new Vector2(100, 100);
        _itemCount = 8;
        _row = 2;
        _iconDepth = 310;
        _bLoadedPanel = false;
        //_bLoadedCurrency = _bLoadedPanel = _bAvatarIconInventoryActive = _bInventoryActive = false;

        // 監聽事件
        Global.photonService.LoadItemDataEvent += OnLoadItem;
        Global.photonService.LoadPlayerDataEvent += OnLoadPlayerData;
        Global.photonService.LoadCurrencyEvent += OnLoadCurrency;
        Global.photonService.LoadPlayerItemEvent += OnLoadPlayerItem;
        Global.photonService.UpdatePlayerImageEvent += OnUpdatePlayerImage;
    }

    public override void Update()
    {
        base.Update();

        #region // 資料庫資料載入完成時 載入Asset
        if (_dataLoadedCount == GetMustLoadedDataCount() && !_bLoadedPanel)
        {
            _bLoadedPanel = true;
            OnLoadPanel();
        }
        #endregion

        #region // Asset載入完成時 載入玩家頭像、實體化裝備圖示
        if (m_AssetLoaderSystem.IsLoadAllAseetCompleted && _LoadedAsset)
        {
            m_AssetLoaderSystem.Initialize();
            _LoadedAsset = false;
            InstantiateEquipIcon(Global.playerItem, UI.weaponEquip.transform, (int)StoreType.Armor);
            LoadPlayerAvatorIcon();
            LoadPlayerInfo();
            LoadPlayerRecord();
            ResumeToggleTarget();
        }
        #endregion
    }

    #region -- OnLoadPanel  載入Panel -- 
    /// <summary>
    /// 載入Panel時
    /// </summary>
    protected override void OnLoading()
    {
        UI = m_RootUI.GetComponentInChildren<AttachBtn_PlayerUI>();

        _dataLoadedCount = (int)ENUM_Data.None;

        if (Global.isMatching)
            Global.photonService.ExitWaitingRoom();
        // 按扭加入監聽事件
        UIEventListener.Get(UI.playerImageBtn).onClick = OnPlayerImageClick;
        UIEventListener.Get(UI.closeCollider).onClick = OnClosed;
        UIEventListener.Get(UI.weaponEquip).onClick = OnEquipClick;
        //UIEventListener.Get(UI.renameBtn).onClick = OnInvAvatorIamgeClick;
        //UIEventListener.Get(UI.saveNoteBtn).onClick = OnInvAvatorIamgeClick;

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
        //EventMaskSwitch.LastPanel = m_RootUI.transform.GetChild(0).gameObject; //儲存目前Panel
        GetMustLoadAsset();
    }
    #endregion

    #region -- GetMustLoadAsset 載入必要資產 --
    /// <summary>
    /// 載入必要資產
    /// </summary>
    protected override void GetMustLoadAsset()
    {
        List<string> notLoadedAssetNameList;

        // 如果是第一次載入 取得未載入物件。 否則 取得相異(新的)物件
        if (_bFirstLoad)
        {
            m_AssetLoaderSystem.LoadAssetFormManifest(Global.PanelUniquePath + Global.InvItemAssetName + Global.ext);  // 載入背包背景資產
            notLoadedAssetNameList = GetDontNotLoadAssetName(Global.playerItem, Global.itemProperty);
            _bFirstLoad = false;
        }
        else
        {
            // 排除過時資料
            LoadProperty.ExpectOutdataObject(Global.playerItem, _dictItemData, dictLoadedItemRefs);
            LoadProperty.ExpectOutdataObject(Global.playerItem, _dictItemData, dictLoadedEquipedRefs);

            // Where 找不存在的KEY 再轉換為Dictionary
            _dictItemData = Global.playerItem.Where(kvp => !_dictItemData.ContainsKey(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            notLoadedAssetNameList = GetDontNotLoadAssetName(_dictItemData, Global.itemProperty);
        }

        // 載入老鼠ICON
        foreach (KeyValuePair<string, object> item in Global.dictMiceAll)
        {
            if (!m_AssetLoaderSystem.GetAsset(item.Value.ToString()))
                m_AssetLoaderSystem.LoadAssetFormManifest(Global.MiceIconUniquePath + Global.IconSuffix + item.Value + Global.ext);
        }

        // 如果 有未載入物件 載入AB
        if (notLoadedAssetNameList.Count > 0)
            LoadIconObjectsAssetByName(notLoadedAssetNameList, Global.ItemIconUniquePath); // 載入道具ICON物件

        m_AssetLoaderSystem.SetLoadAllAseetCompleted();
        _dictItemData = Global.playerItem;
        _LoadedAsset = true;
    }
    #endregion

    #region -- LoadPlayer(Info、Equip、Record) 載入玩家資訊 --
    private void LoadPlayerInfo()
    {
        int exp = Clac.ClacExp(Mathf.Min(Global.Rank + 1, 100));

        UI.playerName.GetComponent<UILabel>().text = Global.Nickname.ToString();
        UI.level.GetComponent<UILabel>().text = Global.Rank.ToString();
        UI.rice.GetComponent<UILabel>().text = Global.Rice.ToString();
        UI.gold.GetComponent<UILabel>().text = Global.Gold.ToString();
        // UI.note.GetComponent<UILabel>().text = Global.MaxCombo.ToString();
        UI.exp.GetComponent<UILabel>().text = Global.Exp.ToString() + " / " + exp.ToString();
        UI.expSilder.GetComponent<UISlider>().value = System.Convert.ToSingle(Global.Exp) / System.Convert.ToSingle(exp);
    }

    private void LoadPlayerEquip()
    {

    }

    private void LoadPlayerRecord()
    {
        float winRate = (Global.SumWin / Global.SumBattle) * 100f;

        UI.maxScore.GetComponent<UILabel>().text = Global.MaxScore.ToString();
        UI.sumKill.GetComponent<UILabel>().text = Global.SumKill.ToString();
        UI.sumWin.GetComponent<UILabel>().text = Global.SumWin.ToString();
        UI.maxCombo.GetComponent<UILabel>().text = Global.MaxCombo.ToString();
        UI.winRate.GetComponent<UILabel>().text = winRate.ToString("F2") + " %";    //F2 = float 小數點第2位
        UI.strianghtWin.GetComponent<UILabel>().text = Global.SumWin.ToString(); // 還沒有寫連贏場數 錯誤FUCK
        UI.sumlost.GetComponent<UILabel>().text = Global.SumLost.ToString();
    }
    #endregion

    #region -- LoadPlayerAvatorIcon 載入 玩家頭像 -- 
    /// <summary>
    /// 載入 玩家頭像
    /// </summary>
    private void LoadPlayerAvatorIcon()
    {
        // 如果尚未載入玩家頭像 實體化
        if (UI.playerImageBtn.transform.childCount == 0)
            MPGFactory.GetObjFactory().Instantiate(m_AssetLoaderSystem.GetAsset(Global.PlayerImage), UI.playerImageBtn.transform, "AvatarImage", Vector3.one, Vector2.one, new Vector2(280, 280), -390);
        // 顯示
        UISprite playerImage = UI.playerImageBtn.transform.GetChild(0).GetComponent<UISprite>();
        playerImage.spriteName = Global.PlayerImage;
    }
    #endregion

    #region -- InstantiateAvatarIconBag  實體化角色頭像背包物件--
    /// <summary>
    /// 實體化角色頭像背包物件
    /// </summary>
    /// <param name="dictAvatarIconData">ICON道具資料</param>
    /// <param name="invBtnItemName"></param>
    /// <param name="itemPanel"></param>
    /// <param name="offset"></param>
    /// <param name="tableCount"></param>
    /// <param name="rowCount"></param>
    /// <returns></returns>
    public Dictionary<string, GameObject> InstantiateAvatarIconBag(Dictionary<string, object> dictAvatarIconData, string invBtnItemName, Transform itemPanel, Vector2 offset, int tableCount, int rowCount)
    {
        // 如果還沒建立 背包道具
        if (itemPanel.transform.childCount == 0)
        {
            _lastEmptyItemGroup = CreateEmptyObject(itemPanel, (int)StoreType.Mice);
            Vector2 pos = new Vector2();
            int i = 0;

            foreach (KeyValuePair<string, object> icon in dictAvatarIconData)
            {
                if (m_AssetLoaderSystem.GetAsset(invBtnItemName) != null)                  // 已載入按鈕資產時
                {
                    GameObject invBtn = m_AssetLoaderSystem.GetAsset(invBtnItemName);
                    pos = SortItemPos(tableCount, rowCount, offset, pos, i);
                    invBtn = MPGFactory.GetObjFactory().Instantiate(invBtn, _lastEmptyItemGroup.transform, icon.Key, pos, Vector3.one, Vector2.zero, -1);
                    string iconName = Global.IconSuffix + icon.Value;

                    if (m_AssetLoaderSystem.GetAsset(iconName) != null)                  // 已載入ICON資產時
                    {
                        GameObject iconBundle = m_AssetLoaderSystem.GetAsset(iconName);
                        MPGFactory.GetObjFactory().Instantiate(iconBundle, invBtn.transform.Find("Image"), icon.Key, Vector3.zero, Vector3.one, Vector2.zero, -1);
                        invBtn.tag = "InventoryICON";
                        GameObject.Destroy(invBtn.GetComponent<ButtonSwitcher>());
                        GameObject.Destroy(invBtn.GetComponent<UIDragObject>());
                        GameObject.Destroy(invBtn.GetComponent<Rigidbody>());
                        UIEventListener.Get(invBtn).onClick = OnPlayerImageInvBtnClick;
                    }
                    pos.x += offset.x;
                }
                i++;
            }
        }
        return null;
    }

    #endregion

    #region -- InstantiateEquipBagItem 實體化背包道具 --
    /// <summary>
    /// 實體化載入完成的遊戲物件，利用玩家JSON資料判斷必要實體物件
    /// </summary>
    /// <param name="dictEquipIconData">資料字典</param>
    /// <param name="itemPanel">實體化父系位置</param>
    /// <param name="itemType">道具類別</param>
    private void InstantiateEquipItemBag(Dictionary<string, object> dictEquipIconData, string invBtnItemName, Transform itemPanel, Vector2 offset, int tableCount, int rowCount)
    {
        // 如果道具形態正確 取得 該道具形態 的 道具資料
        if (_itemType != -1)
            dictEquipIconData = MPGFactory.GetObjFactory().GetItemDetailsInfoFromType(dictEquipIconData, _itemType);

        if (itemPanel.transform.childCount == 0)                // 如果還沒建立道具
        {
            _lastEmptyItemGroup = CreateEmptyObject(itemPanel, _itemType);
            Vector2 pos = new Vector2();
            int i = 0;

            foreach (KeyValuePair<string, object> icon in dictEquipIconData)
            {
                if (m_AssetLoaderSystem.GetAsset(invBtnItemName) != null)                  // 已載入按鈕資產時
                {
                    var dictEquipsData = icon.Value as Dictionary<string, object>;

                    GameObject invBtn = m_AssetLoaderSystem.GetAsset(invBtnItemName);
                    pos = SortItemPos(tableCount, rowCount, offset, pos, i);
                    invBtn = MPGFactory.GetObjFactory().Instantiate(invBtn, _lastEmptyItemGroup.transform, icon.Key, pos, Vector3.one, Vector2.zero, -1);

                    // 用ID取出ICON名稱
                    Global.dictSortedItem.TryGetValue(icon.Key, out object iconAssetName);
                    iconAssetName = Global.IconSuffix + iconAssetName;

                    // 已載入ICON資產時  實體化背包物件
                    if (m_AssetLoaderSystem.GetAsset(iconAssetName.ToString()) != null)
                    {
                        GameObject iconBundle = m_AssetLoaderSystem.GetAsset(iconAssetName.ToString());
                        MPGFactory.GetObjFactory().Instantiate(iconBundle, invBtn.transform.Find("Image"), icon.Key, Vector3.zero, Vector3.one, Vector2.zero, 500);
                        invBtn.tag = "EquipICON";

                        GameObject.Destroy(invBtn.GetComponent<ButtonSwitcher>());
                        GameObject.Destroy(invBtn.GetComponent<UIDragObject>());
                        GameObject.Destroy(invBtn.GetComponent<Rigidbody>());

                        UIEventListener.Get(invBtn).onClick = OnEquipInvBtnClick;
                        // bundle.transform.parent.parent.gameObject.AddComponent<ChangeICON>();

                        // 如果已經裝備道具 顯示 已裝備狀態
                        dictEquipsData.TryGetValue("IsEquip", out object bEquip);
                        //Debug.Log("bEquip:" + bEquip);
                        //if (System.Convert.ToBoolean(bEquip))
                        //{
                        //    invBtn.SendMessage("DisableBtn"); // imageParent.parent = button
                        //}
                    }
                    pos.x += offset.x;
                }
                i++;
            }
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
        Transform equipIconParent = myParent.GetChild(i).GetChild(0);

        foreach (KeyValuePair<string, object> item in itemData)
        {
            var nestedData = item.Value as Dictionary<string, object>;

            nestedData.TryGetValue("ItemID", out object itemID);
            string itemName = MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.itemProperty, "ItemName", itemID.ToString()).ToString();

            // 如果道具不在裝備欄位 
            if (!dictLoadedEquipedRefs.ContainsKey(itemName))
            {
                object isEquip, type;
                nestedData.TryGetValue("IsEquip", out isEquip);
                nestedData.TryGetValue("ItemType", out type);

                if (System.Convert.ToBoolean(isEquip) && System.Convert.ToInt32(type) == itemType)                                // 如果道具是裝備狀態
                {
                    string bundleName = Global.IconSuffix + itemName;
                    //  string bundleName = AssetBundleManager.GetAssetBundleNamePath(Global.IconSuffix + itemName);

                    // 已載入資產時
                    if (!string.IsNullOrEmpty(bundleName) && m_AssetLoaderSystem.GetAsset(bundleName) != null)
                    {
                        // 如果沒有ICON才實體化
                        if (equipIconParent.childCount == 0)
                        {
                            equipIconParent.parent.name = itemID.ToString();
                            equipIconParent.parent.tag = "Equip";

                            UIEventListener.Get(equipIconParent.parent.gameObject).onClick = OnEquipClick;
                            GameObject equipIconAsset = m_AssetLoaderSystem.GetAsset(bundleName);
                            equipIconAsset = MPGFactory.GetObjFactory().Instantiate(equipIconAsset, equipIconParent, bundleName, Vector3.zero, Vector3.one, new Vector2(_iconSize.x, _iconSize.y), _iconDepth);

                            // 刪除 weapon拖曳按鈕 錯誤 暫時移除的方法
                            GameObject.Destroy(equipIconAsset.GetComponentInParent<ButtonSwitcher>());
                            GameObject.Destroy(equipIconAsset.GetComponentInParent<UIDragObject>());
                            GameObject.Destroy(equipIconAsset.GetComponentInParent<Rigidbody>());

                            // 加入載入的裝備索引
                            if (!dictLoadedEquipedRefs.ContainsKey(itemID.ToString()))
                                dictLoadedEquipedRefs.Add(itemID.ToString(), equipIconParent.parent.gameObject);      // 參考至 老鼠所在的MiceBtn位置

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
    }
    #endregion

    #region -- OnClick 滑鼠事件 -- 
    /// <summary>
    /// 當點擊 背包頭像時
    /// </summary>
    /// <param name="go">頭像按扭</param>
    public void OnPlayerImageClick(GameObject go)
    {
        // 如果裝備背包開啟中 則 關閉 (避免擋到頭像背包)
        if (UI.playerEquipInvPanel.activeSelf)
            OnEquipClick(go);

        // 開啟或關閉角色頭像背包
        _bAvatarIconInventoryActive = !_bAvatarIconInventoryActive;
        UI.playerImageInvPanel.SetActive(_bAvatarIconInventoryActive);

        // 如果 頭像背包為空 實體化按鈕圖示
        if (UI.playerImageInvGrid.transform.childCount == 0)
        {
            InstantiateAvatarIconBag(Global.dictMiceAll, Global.InvItemAssetName, UI.playerImageInvGrid.transform, _itemOffset, _itemCount, _row);

            // 開關防止Item按鈕失效
            UI.playerImageInvGrid.SetActive(false);
            UI.playerImageInvGrid.SetActive(true);
        }
    }

    /// <summary>
    /// 開啟裝備背包
    /// </summary>
    /// <param name="go">裝備部位按鈕</param>
    public void OnEquipClick(GameObject go)
    {
        // 如果裝備背包開啟中 則 關閉 (避免擋到頭像背包)
        if (UI.playerImageInvPanel.activeSelf)
            OnPlayerImageClick(go);

        _bInventoryActive = !_bInventoryActive;
        UI.playerEquipInvPanel.SetActive(_bInventoryActive);

        // 如果 道具背包 還沒有載入
        if (UI.playerEquipInvGrid.transform.childCount == 0)
        {
            //取得開啟的裝備類別(武器、衣服、道具等)
            if (go.GetComponentInChildren<UISprite>())
                _itemType = System.Convert.ToInt32(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.playerItem, "ItemType", go.name));
            else
                _itemType = int.Parse(go.transform.parent.name);

            InstantiateEquipItemBag(_dictItemData, Global.InvItemAssetName, UI.playerEquipInvGrid.transform, _itemOffset, _itemCount, _row);

            // 開關防止Item按鈕失效
            UI.playerEquipInvGrid.SetActive(false);
            UI.playerEquipInvGrid.SetActive(true);
        }
    }


    private void OnPlayerImageInvBtnClick(GameObject go)
    {
        string imageName = go.transform.GetComponentInChildren<UISprite>().spriteName;
        UI.playerImageBtn.GetComponentInChildren<UISprite>().name = imageName;
        Global.photonService.UpdatePlayerData(imageName);
    }

    //暫時亂寫的 直接運GameObject名稱來更新資料庫 會導致嚴重錯誤
    private void OnEquipInvBtnClick(GameObject go)
    {
        if (go.name != UI.weaponBtn.name)
        {
            string imageName = go.transform.GetComponentInChildren<UISprite>().spriteName;

            Global.photonService.UpdatePlayerItem(short.Parse(go.name), true);
            Global.photonService.UpdatePlayerItem(short.Parse(UI.weaponBtn.name), false);
            UI.weaponBtn.GetComponentInChildren<UISprite>().spriteName = imageName;
            UI.weaponBtn.name = go.name;
        }
    }
    #endregion

    #region -- OnLoadData 載入資料區 -- 
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
        //_bUpdateAvatarImage = true;
        // 重新載入玩家頭像
        LoadPlayerAvatorIcon();
    }

    protected override int GetMustLoadedDataCount()
    {
        return (int)ENUM_Data.PlayerData * (int)ENUM_Data.PlayerItem * (int)ENUM_Data.ItemData * (int)ENUM_Data.CurrencyData;
    }
    #endregion

    #region -- ShowPanel  -- 
    public override void ShowPanel(string panelName)
    {
        m_RootUI = GameObject.Find(Global.Scene.MainGameAsset.ToString()).GetComponentInChildren<AttachBtn_MenuUI>().playerPanel;
        base.ShowPanel(panelName);
    }
    #endregion

    #region -- OnClosed  -- 
    /// <summary>
    /// 關閉Panel
    /// </summary>
    /// <param name="go">Panel</param>
    public override void OnClosed(GameObject go)
    {
        //   EventMaskSwitch.lastPanel = null;
        ShowPanel(m_RootUI.name);
        //  GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().LoadPanel(go.transform.parent.gameObject);
    }
    #endregion

    #region -- Release -- 
    // Panel 關閉時
    public override void Release()
    {
        // -event 移除事件監聽 (載入道具資料時、載入玩家資料時、載入玩家道具資料時)
        Global.photonService.LoadItemDataEvent -= OnLoadItem;
        Global.photonService.LoadPlayerDataEvent -= OnLoadPlayerData;
        Global.photonService.LoadCurrencyEvent -= OnLoadCurrency;
        Global.photonService.LoadPlayerItemEvent -= OnLoadPlayerItem;
        Global.photonService.UpdatePlayerImageEvent -= OnUpdatePlayerImage;
    }
    #endregion
}
