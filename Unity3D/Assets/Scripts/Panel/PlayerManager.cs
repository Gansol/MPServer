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

public class PlayerManager : PanelManager
{

    public static Dictionary<string, GameObject> dictLoadedEquiped { get; set; }
    public static Dictionary<string, GameObject> dictLoadedItem { get; set; }
    private static Dictionary<string, object> _dictItemData /*,_dictEquipData*/;        // 道具資料、裝備資料
    public GameObject playerInfoArea, equipArea, playerRecordArea, inventoryArea;
    public Vector3 actorScale;
    public Vector2 itemOffset, iconSize;
    public int itemCount = 8, row = 2, _itemType, iconDepth = 310;
    private bool _bFirstLoad, _LoadedIcon, _bInvActive, _bPanelFirstLoad;

    ObjectFactory objFactory;
    void Awake()
    {
        dictLoadedEquiped = new Dictionary<string, GameObject>();
        dictLoadedItem = new Dictionary<string, GameObject>();
        assetLoader = gameObject.AddMissingComponent<AssetLoader>();
        objFactory = new ObjectFactory();
        _dictItemData = Global.dictSortedItem;
        _itemType = (int)StoreType.Armor;
        _bInvActive = false;
        _bPanelFirstLoad =_bFirstLoad = true;
        Global.photonService.LoadPlayerItemEvent += OnLoadPanel;
        
    }

    void Update()
    {
        if (Global.isPlayerDataLoaded)
        {
            Global.isPlayerDataLoaded = false;
            LoadPlayerInfo();
            LoadPlayerEquip();
            LoadPlayerRecord();
        }

        if (assetLoader.loadedObj && _LoadedIcon)
        {
            _LoadedIcon = !_LoadedIcon;
            assetLoader.init();
            
            InstantiateEquipIcon(Global.playerItem, equipArea.transform, (int)StoreType.Armor);
            
        }
    }

    #region -- LoadPlayer(Info、Equip、Record) 載入玩家資訊 --
    private void LoadPlayerInfo()
    {
        ClacExp clacExp = new ClacExp(Global.Rank);
        Transform parent = playerInfoArea.transform;

        parent.FindChild("Lv").GetComponent<UILabel>().text = Global.Rank.ToString();
        parent.FindChild("Rice").GetComponent<UILabel>().text = Global.Rice.ToString();
        parent.FindChild("Gold").GetComponent<UILabel>().text = Global.Gold.ToString();
        // parent.FindChild("Note").GetComponent<UILabel>().text = Global.MaxCombo.ToString();
        parent.FindChild("Exp").GetComponent<UILabel>().text = Global.EXP.ToString() + " / " + clacExp.exp.ToString();
        parent.FindChild("Name").GetComponent<UILabel>().text = Global.Nickname.ToString();
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

        foreach (KeyValuePair<string, object> item in itemData)
        {
            var nestedData = item.Value as Dictionary<string, object>;
            object itemID;
            string itemName = "";

            nestedData.TryGetValue("ItemID", out itemID);
            itemName = ObjectFactory.GetColumnsDataFromID(Global.itemProperty, "ItemName", itemID.ToString()).ToString();

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
                        Transform imageParent = myParent.GetChild(i).GetChild(0);
                        if (imageParent.childCount == 0)   // 如果沒有ICON才實體化
                        {
                            imageParent.parent.name = itemID.ToString();
                            imageParent.parent.tag = "Equip";
                            UIEventListener.Get(imageParent.parent.gameObject).onClick += OnEquipClick;
                            GameObject _clone, bundle = assetLoader.GetAsset(bundleName);
                            _clone = objFactory.Instantiate(bundle, imageParent, bundleName, Vector3.zero, Vector3.one, new Vector2(iconSize.x, iconSize.y), iconDepth);
                            _clone.GetComponentInParent<ButtonSwitcher>().enabled = true;
                            _clone.GetComponentInParent<ButtonSwitcher>().SendMessage("EnableBtn");
                            if (!dictLoadedEquiped.ContainsKey(itemID.ToString())) dictLoadedEquiped.Add(itemID.ToString(), imageParent.parent.gameObject);      // 參考至 老鼠所在的MiceBtn位置
                            i++;
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region -- InstantiateItemIcon 實體化背包道具 --
    /// <summary>
    /// 實體化載入完成的遊戲物件，利用玩家JASON資料判斷必要實體物件
    /// </summary>
    /// <param name="itemData">資料字典</param>
    /// <param name="itemPanel">實體化父系位置</param>
    /// <param name="itemType">道具類別</param>
    private void InstantiateItemIcon(Dictionary<string, object> itemData, Transform itemPanel, int itemType)
    {
        itemData = ObjectFactory.GetItemInfoFromType(itemData, itemType);

        if (itemData.Count != 0)
        {
            int i = 0;
            foreach (KeyValuePair<string, object> item in itemData)
            {
                var nestedData = item.Value as Dictionary<string, object>;
                object itemID;
                string itemName = "", bundleName = "";

                nestedData.TryGetValue("ItemID", out itemID);
                itemName = ObjectFactory.GetColumnsDataFromID(Global.itemProperty, "ItemName", itemID.ToString()).ToString();
                bundleName = itemName + "ICON";

                if (assetLoader.GetAsset(bundleName) != null)                  // 已載入資產時
                {
                    GameObject bundle = assetLoader.GetAsset(bundleName);
                    Transform imageParent = itemPanel.GetChild(i).GetChild(0);

                    if (imageParent.childCount == 0)   // 如果沒有ICON才實體化
                    {
                        imageParent.parent.name = itemID.ToString();
                        imageParent.parent.tag = "Inventory";
                        GameObject _clone = objFactory.Instantiate(bundle, imageParent, bundleName, Vector3.zero, Vector3.one, new Vector2(iconSize.x, iconSize.y), iconDepth);
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

    public void OnEquipClick(GameObject obj)
    {
        _bInvActive = !_bInvActive;
       // inventoryArea.transform.parent.gameObject.SetActive(_bInvActive);
        inventoryArea.transform.parent.parent.parent.gameObject.SetActive(_bInvActive);
        _itemType = System.Convert.ToInt32(ObjectFactory.GetColumnsDataFromID(Global.playerItem, "ItemType", obj.name));
        InstantiateItem(_dictItemData, "InvItem", _itemType, inventoryArea.transform, itemOffset, itemCount, row);
        InstantiateItemIcon(_dictItemData, _lastEmptyItemGroup.transform, _itemType);

        inventoryArea.SetActive(false);   // 開關防止Item按鈕失效
        inventoryArea.SetActive(true);
    }

    private void GetMustLoadAsset()
    {
        Dictionary<string, object> dictNotLoadedAsset = new Dictionary<string, object>();
        if (_bFirstLoad)                        // 取得未載入物件
        {

            dictNotLoadedAsset = GetDontNotLoadAsset(Global.playerItem, Global.itemProperty);
            _bFirstLoad = false;
        }
        else
        {
            ExpectOutdataObject(Global.playerItem, _dictItemData, dictLoadedItem);
            ExpectOutdataObject(Global.playerItem, _dictItemData, dictLoadedEquiped);
            _dictItemData = SelectNewData(Global.playerItem, _dictItemData);

            dictNotLoadedAsset = GetDontNotLoadAsset(_dictItemData, Global.itemProperty);
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

    public override void OnLoading()
    {
        Global.photonService.LoadItemData();
        Global.photonService.LoadPlayerData(Global.Account);
        Global.photonService.LoadPlayerItem(Global.Account);
    }

    private void OnLoadPanel()
    {
        GetMustLoadAsset();
    }
}
