using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
 * ****************************************************************/

public class PlayerManagerOld : MPPanel
{

    public static Dictionary<string, GameObject> dictLoadedEquiped { get; set; }
    public static Dictionary<string, GameObject> dictLoadedItem { get; set; }

    private static Dictionary<string, object> _dictItemData /*,_dictEquipData*/;        // 道具資料、裝備資料
    public GameObject playerInfoArea, equipArea, playerRecordArea, imageArea, inventoryArea;
    public Vector3 actorScale;
    public Vector2 itemOffset, iconSize;
    public int itemCount = 8, row = 2, _itemType, iconDepth = 310;
    private bool _bFirstLoad, _LoadedIcon, _bImgActive, _bInvActive, _bPanelFirstLoad;

    public static UISprite playerImage;
    public GameObject iconBtn, equipBtn;

    public PlayerManagerOld(MPGame MPGame) : base(MPGame) { }

    void Awake()
    {
        dictLoadedEquiped = new Dictionary<string, GameObject>();
        dictLoadedItem = new Dictionary<string, GameObject>();

        _dictItemData = Global.dictSortedItem;
        _itemType = (int)StoreType.Armor;
        _bImgActive = _bInvActive = false;
        _bPanelFirstLoad = _bFirstLoad = true;
    }

    void OnEnable()
    {
        Global.photonService.LoadPlayerItemEvent += OnLoadPanel;
    }

    void Update()
    {
        if (Global.isPlayerDataLoaded && Global.isPlayerItemLoaded && Global.isItemLoaded && _LoadedIcon)
        {
            //會發生還沒載入物件就載入資料
            Global.isPlayerDataLoaded = false;
            Global.isPlayerItemLoaded = false; 
            Global.isItemLoaded = false;

            LoadPlayerInfo();
            LoadPlayerEquip();
            LoadPlayerRecord();
        }

        if (assetLoader.loadedObj && _LoadedIcon)
        {
            _LoadedIcon = !_LoadedIcon;
            assetLoader.init();

            Transform imageParent = playerInfoArea.transform.Find("Image").GetChild(0).GetComponent<UISprite>().transform;
            imageParent.GetComponent<UISprite>().spriteName = Global.PlayerImage;
            playerImage = imageParent.GetComponent<UISprite>();
            imageParent.parent.tag = "EquipICON";
            // imageParent.GetComponent<ButtonSwitcher>().SendMessage("EnableBtn");

            //      string key =  Global.dictMiceAll.FirstOrDefault(x => x.Value == imageParent.GetComponent<UISprite>().spriteName).Key;

            //  if (!dictLoadedICON.ContainsKey(key)) dictLoadedICON.Add(key, imageParent.parent.gameObject);      // 參考至 老鼠所在的MiceBtn位置
            InstantiateEquipIcon(Global.playerItem, equipArea.transform, (int)StoreType.Armor);
            EventMaskSwitch.Resume();
            GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().Panel[5].SetActive(false);
            EventMaskSwitch.Switch(gameObject, false);
            EventMaskSwitch.lastPanel = gameObject;
        }
    }

    #region -- LoadPlayer(Info、Equip、Record) 載入玩家資訊 --
    private void LoadPlayerInfo()
    {
        Transform parent = playerInfoArea.transform;
        int exp = Clac.ClacExp(Mathf.Min(Global.Rank + 1, 100));

        parent.FindChild("Lv").GetComponent<UILabel>().text = Global.Rank.ToString();
        parent.FindChild("Rice").GetComponent<UILabel>().text = Global.Rice.ToString();
        parent.FindChild("Gold").GetComponent<UILabel>().text = Global.Gold.ToString();
        // parent.FindChild("Note").GetComponent<UILabel>().text = Global.MaxCombo.ToString();
        parent.FindChild("Exp").GetComponent<UILabel>().text = Global.Exp.ToString() + " / " + exp.ToString();
        parent.FindChild("Exp").Find("ExpBar").GetComponent<UISlider>().value = System.Convert.ToSingle(Global.Exp) / System.Convert.ToSingle(exp);
        parent.FindChild("Name").GetComponent<UILabel>().text = Global.Nickname.ToString();
    }


    public void OnClosed(GameObject obj)
    {
        EventMaskSwitch.lastPanel = null;
        GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().LoadPanel(obj.transform.parent.gameObject);
    }


    private void LoadPlayerEquip()
    {

    }

    private void LoadPlayerRecord()
    {
        Transform parent = playerRecordArea.transform;
        float winRate = ((float)Global.SumWin / (float)Global.SumBattle) * 100f;

        parent.FindChild("HighScore").GetComponent<UILabel>().text = Global.MaxScore.ToString();
        parent.FindChild("Hit").GetComponent<UILabel>().text = Global.SumKill.ToString();
        parent.FindChild("Win").GetComponent<UILabel>().text = Global.SumWin.ToString();
        parent.FindChild("Combo").GetComponent<UILabel>().text = Global.MaxCombo.ToString();
        parent.FindChild("WinRate").GetComponent<UILabel>().text = winRate.ToString("F2") + " %";
        parent.FindChild("StrianghtWin").GetComponent<UILabel>().text = Global.SumWin.ToString();
        parent.FindChild("Lose").GetComponent<UILabel>().text = Global.SumLost.ToString();
    }
    #endregion

    #region -- InstantiateEquipIcon 實體化裝備中物件--
    /// <summary>
    /// 實體化載入完成的遊戲物件，利用玩家JASON資料判斷必要實體物件
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

            if (!dictLoadedEquiped.ContainsKey(itemName))                                 // 如果道具不在裝備欄位 
            {
                object isEquip, type;
                nestedData.TryGetValue("IsEquip", out isEquip);
                nestedData.TryGetValue("ItemType", out type);

                if (System.Convert.ToBoolean(isEquip) && System.Convert.ToInt32(type) == itemType)                                // 如果道具是裝備狀態
                {
                    string bundleName = itemName + "ICON";
                    if (assetLoader.GetAsset(bundleName) != null)                   // 已載入資產時
                    {

                        if (imageParent.childCount == 0)   // 如果沒有ICON才實體化
                        {
                            imageParent.parent.name = itemID.ToString();
                            imageParent.parent.tag = "Equip";
                            UIEventListener.Get(imageParent.parent.gameObject).onClick += OnEquipClick;
                            GameObject _clone, bundle = assetLoader.GetAsset(bundleName);
                            _clone = MPGFactory.GetObjFactory().Instantiate(bundle, imageParent, bundleName, Vector3.zero, Vector3.one, new Vector2(iconSize.x, iconSize.y), iconDepth);
                            _clone.GetComponentInParent<ButtonSwitcher>().enabled = true;
                            _clone.GetComponentInParent<ButtonSwitcher>().SendMessage("EnableBtn");
                            if (!dictLoadedEquiped.ContainsKey(itemID.ToString())) dictLoadedEquiped.Add(itemID.ToString(), imageParent.parent.gameObject);      // 參考至 老鼠所在的MiceBtn位置
                            i++;
                        }
                    }
                }
            }
        }

        if (imageParent.childCount == 0)
        {
            UIEventListener.Get(imageParent.parent.gameObject).onClick += OnEquipClick;
        }
    }
    #endregion

    #region -- InstantiateItemIcon 實體化背包道具 --
    /// <summary>
    /// 實體化載入完成的遊戲物件，利用玩家JSON資料判斷必要實體物件
    /// </summary>
    /// <param name="itemData">資料字典</param>
    /// <param name="itemPanel">實體化父系位置</param>
    /// <param name="itemType">道具類別</param>
    private void InstantiateItemIcon(Dictionary<string, object> itemData, Transform itemPanel, int itemType)
    {
        if (itemType != -1)
            itemData = MPGFactory.GetObjFactory().GetItemInfoFromType(itemData, itemType);

        if (itemData.Count != 0)
        {
            int i = 0;
            foreach (KeyValuePair<string, object> item in itemData)
            {
                var nestedData = item.Value as Dictionary<string, object>;
                object itemID;
                string itemName = "", bundleName = "";

                nestedData.TryGetValue("ItemID", out itemID);
                itemName = MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.itemProperty, "ItemName", itemID.ToString()).ToString();
                bundleName = itemName + "ICON";

                if (assetLoader.GetAsset(bundleName) != null)                  // 已載入資產時
                {
                    GameObject bundle = assetLoader.GetAsset(bundleName);
                    Transform imageParent = itemPanel.GetChild(i).GetChild(0);

                    if (imageParent.childCount == 0)   // 如果沒有ICON才實體化
                    {
                        imageParent.parent.name = itemID.ToString();
                        imageParent.parent.tag = "Inventory";
                        GameObject _clone = MPGFactory.GetObjFactory().Instantiate(bundle, imageParent, bundleName, Vector3.zero, Vector3.one, new Vector2(iconSize.x, iconSize.y), iconDepth);
                        _clone.GetComponentInParent<ButtonSwitcher>()._activeBtn = true;

                        object value;
                        nestedData.TryGetValue("ItemType", out value);
                        if (itemType.ToString() == value.ToString())
                            if (!dictLoadedItem.ContainsKey(itemID.ToString())) dictLoadedItem.Add(itemID.ToString(), imageParent.parent.gameObject);          // 加入索引 老鼠所在的MiceBtn位置

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


    public void OnImageClick()
    {
        _bImgActive = !_bImgActive;
        imageArea.transform.parent.parent.parent.gameObject.SetActive(_bImgActive);

        if (inventoryArea.transform.parent.parent.parent.gameObject.activeSelf) inventoryArea.transform.parent.parent.parent.gameObject.SetActive(false);

        if (imageArea.transform.childCount == 0)
        {
            InstantiateICON(Global.dictMiceAll, "InvItem", imageArea.transform, itemOffset, itemCount, row);
        }


        imageArea.SetActive(false);   // 開關防止Item按鈕失效
        imageArea.SetActive(true);
    }


    public void OnEquipClick(GameObject obj)
    {
        _bInvActive = !_bInvActive;
        if (imageArea.transform.parent.parent.parent.gameObject.activeSelf) imageArea.transform.parent.parent.parent.gameObject.SetActive(false);

        inventoryArea.transform.parent.parent.parent.gameObject.SetActive(_bInvActive);
        if (imageArea.transform.childCount == 0)
        {
            if (imageArea.transform.parent.parent.parent.gameObject.activeSelf) 
                OnImageClick();

            if (obj.GetComponentInChildren<UISprite>())
                _itemType = System.Convert.ToInt32(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.playerItem, "ItemType", obj.name));
            else
                _itemType = int.Parse(obj.transform.parent.name);

            InstantiateItem(_dictItemData, "InvItem", _itemType, inventoryArea.transform, itemOffset, itemCount, row);
            InstantiateItemIcon(_dictItemData, _lastEmptyItemGroup.transform, _itemType);
        }

        inventoryArea.SetActive(false);   // 開關防止Item按鈕失效
        inventoryArea.SetActive(true);
    }

    private void GetMustLoadAsset()
    {
        Dictionary<string, object> dictNotLoadedAsset = new Dictionary<string, object>();
        if (_bFirstLoad)                        // 取得未載入物件
        {
            assetLoader.LoadAsset("MiceICON" + "/", "MiceICON");
            dictNotLoadedAsset = GetDontNotLoadAsset(Global.playerItem, Global.itemProperty);
            _bFirstLoad = false;
        }
        else
        {
            LoadProperty.ExpectOutdataObject(Global.playerItem, _dictItemData, dictLoadedItem);
            LoadProperty.ExpectOutdataObject(Global.playerItem, _dictItemData, dictLoadedEquiped);
            _dictItemData = LoadProperty.SelectNewData(Global.playerItem, _dictItemData);

            dictNotLoadedAsset = GetDontNotLoadAsset(_dictItemData, Global.itemProperty);

            EventMaskSwitch.Resume();
            GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().Panel[5].SetActive(false);
            EventMaskSwitch.Switch(gameObject, false);
            EventMaskSwitch.lastPanel = gameObject;
        }

        if (dictNotLoadedAsset.Count != 0) // 如果 有未載入物件 載入AB
        {
            assetLoader.init();
            //assetLoader.LoadAsset(assetFolder[_itemType] + "/", assetFolder[_itemType]);
            assetLoader.LoadAsset("ItemICON" + "/", "ItemICON");
            assetLoader.LoadPrefab("Panel/", "InvItem");

            //_LoadedIcon = LoadIconObject(dictNotLoadedAsset, assetFolder[_itemType]);
            _LoadedIcon = LoadIconObject(dictNotLoadedAsset, "ItemICON");
        }
        else
        {
            assetLoader.loadedObj = _LoadedIcon = true;
        }


        foreach (KeyValuePair<string, object> item in Global.dictMiceAll)
            assetLoader.LoadPrefab("MiceICON" + "/", item.Value + "ICON");
        _dictItemData = Global.playerItem;
    }

    //public void ItemPanelFix(GameObject obj)
    //{
    //    if (_bPanelFirstLoad)
    //    {
    //        _bPanelFirstLoad = false;
    //        obj.SetActive(false);
    //        obj.SetActive(true);
    //    }
    //}

    protected override void OnLoading()
    {
        _bImgActive = _bInvActive = false;

        imageArea.transform.parent.parent.parent.gameObject.SetActive(_bImgActive);
        inventoryArea.transform.parent.parent.parent.gameObject.SetActive(_bInvActive);
        Global.photonService.LoadItemData();
        Global.photonService.LoadPlayerData(Global.Account);
        Global.photonService.LoadPlayerItem(Global.Account);
    }

    protected override void OnLoadPanel()
    {
        GetMustLoadAsset();


    }

    private void OnDisable()
    {
        Global.photonService.LoadPlayerItemEvent -= OnLoadPanel;
    }
}
