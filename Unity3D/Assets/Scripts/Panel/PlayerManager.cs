using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MPProtocol;
using MiniJSON;
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
 * ****************************************************************/

public class PlayerManager : PanelManager
{
    #region 欄位
    /// <summary>
    /// MiceIcon名稱(非bundle)、Mice按鈕
    /// </summary>
    public static Dictionary<string, GameObject> dictLoadedItem { get; set; }
    /// <summary>
    /// TeamIcon名稱、Mice按鈕索引物件
    /// </summary>
    public static Dictionary<string, GameObject> dictLoadedEquiped { get; set; }
    private static Dictionary<string, object> _dictItemData /*,_dictEquipData*/;        // 道具資料、裝備資料

    public GameObject[] infoGroupArea;      // 道具存放區
    public string[] assetFolder;            // 資料夾
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

    private int _itemType;
    private float _delayBetween2Clicks, _lastClickTime;
    private static bool _bFirstLoad;
    private bool _LoadedIcon, _isLoadPlayerData;
    private GameObject _tmpTab, _doubleClickChk;

    private ObjectFactory insObj;
    #endregion

    #region -- Init --
    private void Awake()
    {
        assetLoader = gameObject.AddMissingComponent<AssetLoader>();
        dictLoadedItem = new Dictionary<string, GameObject>();
        dictLoadedEquiped = new Dictionary<string, GameObject>();
        insObj = new ObjectFactory();

        _bFirstLoad = true;
        _dictItemData = Global.SortedItem;
        _itemType = (int)StoreType.Item;

        Global.photonService.LoadPlayerItemEvent += OnLoadPanel;
    }
    #endregion

    #region -- Update --
    private void Update()
    {
        if (!string.IsNullOrEmpty(assetLoader.ReturnMessage))
            Debug.Log("訊息：" + assetLoader.ReturnMessage);

        if (Global.isPlayerDataLoaded && !_isLoadPlayerData)
        {
            _isLoadPlayerData = !_isLoadPlayerData;
            Global.isPlayerDataLoaded = false;
            LoadPlayerInfo();
        }

        if (assetLoader.loadedObj && _LoadedIcon)
        {
            _LoadedIcon = !_LoadedIcon;
            assetLoader.init();
            _tmpTab = infoGroupArea[_itemType];

            InstantiateItem(_dictItemData, "InvItem", _itemType, infoGroupArea[2].transform, itemOffset, tablePageCount, tableRowCount);
            InstantiateEquipIcon(Global.playerItem, infoGroupArea[0].transform, (int)StoreType.Item);
            InstantiateEquipIcon(Global.playerItem, infoGroupArea[1].transform, (int)StoreType.Armor);
            InstantiateIcon(_dictItemData, PanelManager._lastEmptyItemGroup.transform, _itemType);

            infoGroupArea[2].GetComponent<BoxCollider>().enabled = false;   // 開關防止Item按鈕失效
            infoGroupArea[2].GetComponent<BoxCollider>().enabled = true;
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
    private void InstantiateIcon(Dictionary<string, object> itemData, Transform itemPanel, int itemType)
    {
        itemData = GetItemInfoFromType(itemData, itemType);

        if (itemData.Count != 0)
        {
            int i = 0;
            foreach (KeyValuePair<string, object> item in itemData)
            {
                var nestedData = item.Value as Dictionary<string, object>;
                object itemID;
                string itemName = "", bundleName = "";

                nestedData.TryGetValue("ItemID", out itemID);
                itemName = GetItemNameFromID(itemID.ToString(), Global.itemProperty);
                bundleName = itemName + "ICON";

                if (assetLoader.GetAsset(bundleName) != null)                  // 已載入資產時
                {
                    GameObject bundle = assetLoader.GetAsset(bundleName);
                    Transform imageParent = itemPanel.GetChild(i).GetChild(0);

                    if (imageParent.childCount == 0)   // 如果沒有ICON才實體化
                    {
                        imageParent.parent.name = itemID.ToString();
                        GameObject _clone = insObj.Instantiate(bundle, imageParent, bundleName, Vector3.zero, Vector3.one, new Vector2(100, 100), 310);
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
            itemName = GetItemNameFromID(itemID.ToString(), Global.itemProperty);

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
                            GameObject _clone, bundle = assetLoader.GetAsset(bundleName);
                            _clone = insObj.Instantiate(bundle, imageParent, bundleName, Vector3.zero, Vector3.one, new Vector2(100, 100), 310);
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

    #region -- 取值程式碼片段 --
    public GameObject GetLoadedItem(string itemName)
    {
        GameObject obj;
        if (!string.IsNullOrEmpty(itemName) && dictLoadedItem.TryGetValue(itemName, out obj))
            return obj;
        return null;
    }

    public GameObject GetLoadedTeam(string itemName)
    {
        GameObject obj;
        if (!string.IsNullOrEmpty(itemName) && dictLoadedEquiped.TryGetValue(itemName, out obj))
            return obj;
        return null;
    }
    #endregion

    #region -- OnMiceClick 當按下老鼠時 --
    public void OnItemClick(GameObject item)
    {
        if (Time.time - _lastClickTime < _delayBetween2Clicks && _doubleClickChk == item)    // Double Click
            item.SendMessage("Item2Click");
        else
            StartCoroutine(OnClickCoroutine(item));

        _lastClickTime = Time.time;
        _doubleClickChk = item;
    }

    IEnumerator OnClickCoroutine(GameObject item)
    {
        yield return new WaitForSeconds(_delayBetween2Clicks);

        if (Time.time - _lastClickTime < _delayBetween2Clicks)
            yield break;

        Debug.Log(item.transform.GetChild(0).name);
    }
    #endregion

    #region -- OnTabClick 當按下Tab時--
    public void OnTabClick(GameObject obj)
    {
        int value = int.Parse(obj.name.Remove(0, 3));

        switch (value)
        {
            case 1:
                _itemType = (int)StoreType.Item;
                break;
            case 2:
                _itemType = (int)StoreType.Armor;
                break;
            default:
                Debug.LogError("Unknow Tab!");
                break;
        }
        InstantiateItem(_dictItemData, "InvItem", _itemType, infoGroupArea[2].transform, itemOffset, tablePageCount, tableRowCount);
        InstantiateIcon(_dictItemData, PanelManager._lastEmptyItemGroup.transform, _itemType);
        infoGroupArea[2].GetComponent<BoxCollider>().enabled = false;   // 開關防止Item按鈕失效
        infoGroupArea[2].GetComponent<BoxCollider>().enabled = true;
    }
    #endregion

    #region -- OnLoadPanel 載入面板時--
    public override void OnLoading()
    {
        Global.photonService.LoadItemData();
        Global.photonService.LoadPlayerData(Global.Account);
        Global.photonService.LoadPlayerItem(Global.Account);
    }

    protected override void OnLoadPanel()
    {
        if (transform.parent.gameObject.activeSelf) // 如果Panel是啟動狀態 接收Event
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
                assetLoader.LoadAsset(assetFolder[_itemType] + "/", assetFolder[_itemType]);
                assetLoader.LoadPrefab("Panel/", "InvItem");
                _LoadedIcon = LoadIconObject(dictNotLoadedAsset, assetFolder[_itemType]);
            }                                   // 已載入物件 實體化
            else
            {
                InstantiateItem(_dictItemData, "InvItem", _itemType, infoGroupArea[2].transform, itemOffset, tablePageCount, tableRowCount);
                InstantiateEquipIcon(Global.playerItem, infoGroupArea[0].transform, (int)StoreType.Item);
                InstantiateEquipIcon(Global.playerItem, infoGroupArea[1].transform, (int)StoreType.Armor);
                InstantiateIcon(_dictItemData, PanelManager._lastEmptyItemGroup.transform, _itemType);

                infoGroupArea[2].GetComponent<BoxCollider>().enabled = false;   // 開關防止Item按鈕失效
                infoGroupArea[2].GetComponent<BoxCollider>().enabled = true;
            }
            _dictItemData = Global.playerItem;
            _tmpTab = infoGroupArea[_itemType];
        }
    }
    #endregion

    #region -- LoadPlayerInfo 載入玩家資訊 --
    private void LoadPlayerInfo()
    {
        _isLoadPlayerData = !_isLoadPlayerData;
        Transform parent = infoGroupArea[3].transform;
        float winRate = ((float)Global.SumWin / (float)Global.SumBattle) * 100f;

        parent.GetChild(1).GetComponent<UILabel>().text = Global.Nickname;
        parent.GetChild(2).GetComponent<UILabel>().text = Global.Rank.ToString();
        parent.GetChild(3).GetComponent<UILabel>().text = Global.Rice.ToString();
        parent.GetChild(4).GetComponent<UILabel>().text = Global.Gold.ToString();
        parent.GetChild(5).GetComponent<UILabel>().text = (winRate).ToString() + " %";
    }
    #endregion

    public void OnClosed()
    {
        // to do click out close
        Debug.Log("Close");
    }

    void ExpBar()
    {
        // to show exp bar
    }

}
