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
 * ***************************************************************
 *                           ChangeLog
 *                     
 * ****************************************************************/

// mice = item
// team = equip

public class PlayerManager : PanelManager
{
    /// <summary>
    /// MiceIcon名稱(非bundle)、Mice按鈕
    /// </summary>
    public static Dictionary<string, GameObject> dictLoadedItem { get; set; }       // <string, GameObject>Icon名稱、Icon的按鈕
    /// <summary>
    /// TeamIcon名稱、Mice按鈕索引物件
    /// </summary>
    public static Dictionary<string, GameObject> dictLoadedEquiped { get; set; }       // <string, GameObject>Icon名稱、Icon的按鈕
    private static Dictionary<string, object> _dictItemData, _dictEquipData;         // Json老鼠、隊伍資料

    public GameObject[] infoGroupArea;
    public GameObject _lastEmptyItemGroup;
    public int itemOffset;
    public string[] assetFolder;

    AssetLoader assetLoader;

    private int _itemType;
    private float delayBetween2Clicks, _lastClickTime;
    private bool _LoadedIcon, _LoadedItemData, _isLoadPlayerData, _isLoadPlayerItem, _isLoadEquip;
    private GameObject _tmpTab, _doubleClickChk;
    private string folderString;
    string[,] item, equip;


    private InstantiateObject insObj;
    // Use this for initialization
    void Awake()
    {
        Global.photonService.LoadPlayerItemEvent += OnLoadPanel;
        dictLoadedItem = new Dictionary<string, GameObject>();
        dictLoadedEquiped = new Dictionary<string, GameObject>();
        _dictItemData = Global.SortedItem;
        //_dictEquipData = Global.Team;
        insObj = new InstantiateObject();
        assetLoader = gameObject.AddComponent<AssetLoader>();
        _itemType = (int)StoreType.Item;
        folderString = assetFolder[_itemType];
    }

    // Update is called once per frame
    void Update()
    {
        if (!string.IsNullOrEmpty(assetLoader.ReturnMessage))
            Debug.Log("訊息：" + assetLoader.ReturnMessage);

        if (Global.isPlayerDataLoaded && !_isLoadPlayerData)
        {
            _isLoadPlayerData = !_isLoadPlayerData;
            Global.isPlayerDataLoaded = false;
            LoadPlayerData();
        }

        if (assetLoader.loadedObj && _LoadedIcon)
        {
            _LoadedIcon = !_LoadedIcon;
            assetLoader.init();
            _tmpTab = infoGroupArea[_itemType];

            InstantiateItem(_dictItemData, infoGroupArea[2].transform, _itemType);
            InstantiateEquipIcon(Global.playerItem);
            InstantiateItemIcon(_dictItemData, _lastEmptyItemGroup.transform, _itemType);
        }

    }

    #region -- LoadIconObject 載入載入ICON物件 --
    /// <summary>
    /// 載入ICON物件
    /// </summary>
    /// <param name="itemData">物件陣列</param>
    /// <param name="folder">資料夾</param>
    public void LoadIconObject(Dictionary<string, object> itemData, string folder)    // 載入遊戲物件
    {
        if (itemData != null)
        {
            foreach (KeyValuePair<string, object> item in itemData)
            {
                if (!string.IsNullOrEmpty(item.Key)) assetLoader.LoadPrefab(folder + "/", item.Key);
            }
            _LoadedIcon = true;
        }
        else
        {
            Debug.Log("LoadIconObject:itemData is Null !!");
        }
    }
    #endregion

    #region -- InstantiateItem 實體化商店物件--
    /// <summary>
    /// 實體化載入完成的遊戲物件，利用玩家JSON資料判斷必要實體物件
    /// </summary>
    /// <param name="dictionary">資料字典</param>
    /// <param name="myParent">實體化父系位置</param>
    void InstantiateItem(Dictionary<string, object> itemData, Transform itemPanel, int itemType)
    {
        if (itemPanel.transform.childCount == 0)
        {
            _lastEmptyItemGroup = CreateEmptyGroup(itemPanel, itemType);
            InstantiateItem2(itemData, _lastEmptyItemGroup.transform, itemData.Count);
        }
        else
        {
            if (itemPanel.FindChild(itemType.ToString()))
            {
                _lastEmptyItemGroup.SetActive(false);
                _lastEmptyItemGroup = itemPanel.FindChild(itemType.ToString()).gameObject;
                _lastEmptyItemGroup.SetActive(true);
            }
            else if (_lastEmptyItemGroup != itemPanel.FindChild(itemType.ToString()))
            {
                _lastEmptyItemGroup.SetActive(false);
                _lastEmptyItemGroup = CreateEmptyGroup(itemPanel, itemType);
                InstantiateItem2(itemData, _lastEmptyItemGroup.transform, itemData.Count);
            }
        }
        infoGroupArea[2].GetComponent<BoxCollider>().enabled = false;   // 開關防止Item按鈕失效
        infoGroupArea[2].GetComponent<BoxCollider>().enabled = true;
    }

    void InstantiateItem2(Dictionary<string, object> itemData, Transform parent, int itemCount)
    {
        Vector2 pos = new Vector2();
        string itemName = "InvItem";
        int count = parent.childCount, i = 0;

        itemData = GetItemInfoFromType(itemData, _itemType);

        foreach (KeyValuePair<string, object> item in itemData)
        {
            var nestedData = item.Value as Dictionary<string, object>;
            object itemID;
            nestedData.TryGetValue("ItemID", out itemID);

            if (assetLoader.GetAsset(itemName) != null)                  // 已載入資產時
            {
                pos = sortItemPos(9, 3, new Vector2(itemOffset, itemOffset), pos, count + i);
                GameObject bundle = assetLoader.GetAsset(itemName);
                string bundleName = GetItemNameFromID(itemID.ToString(), Global.itemProperty);
                _clone = insObj.Instantiate(bundle, parent, bundleName, new Vector3(pos.x, pos.y), Vector3.one, Vector2.zero, -1);
                pos.x += itemOffset;
            }
            i++;
        }
    }
    #endregion

    #region -- InstantiateItemIcon 實體化背包道具物件--
    /// <summary>
    /// 實體化載入完成的遊戲物件，利用玩家JASON資料判斷必要實體物件
    /// </summary>
    /// <param name="dictionary">資料字典</param>
    /// <param name="myParent">實體化父系位置</param>
    void InstantiateItemIcon(Dictionary<string, object> itemData, Transform myParent, int itemType)
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
                    Transform imageParent = myParent.GetChild(i).GetChild(0);

                    if (imageParent.childCount == 0)   // 如果沒有ICON才實體化
                    {
                        imageParent.parent.name = itemID.ToString();
                        _clone = insObj.Instantiate(bundle, imageParent, bundleName, Vector3.zero, Vector3.one, new Vector2(100, 100), 310);
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
    /// <param name="dictionary">資料字典</param>
    /// <param name="myParent">實體化父系位置</param>
    void InstantiateEquipIcon(Dictionary<string, object> itemData)
    {
        int itemCount = 0, armorCount = 0;
        bool itemOrEquip;    // true = item false = equip

        foreach (KeyValuePair<string, object> item in itemData)
        {
            var nestedData = item.Value as Dictionary<string, object>;
            object itemID;
            string itemName = "";

            nestedData.TryGetValue("ItemID", out itemID);
            itemName = GetItemNameFromID(itemID.ToString(), Global.itemProperty);

            if (!bLoadedEquip(itemName.ToString()))
            {
                object value;
                nestedData.TryGetValue("IsEquip", out value);
                if (System.Convert.ToBoolean(value))
                {
                    string bundleName = itemName + "ICON";
                    if (assetLoader.GetAsset(bundleName) != null)                  // 已載入資產時
                    {
                        // 起始值是EquipItem的位置
                        Transform myParent = infoGroupArea[0].transform;
                        Transform imageParent = myParent.GetChild(itemCount).GetChild(0);
                        itemOrEquip = true;

                        // 如果是裝備位置
                        nestedData.TryGetValue("ItemType", out value);
                        if (int.Parse(value.ToString()) == (int)StoreType.Armor)
                        {
                            myParent = infoGroupArea[1].transform;
                            imageParent = myParent.GetChild(armorCount).GetChild(0);
                            itemOrEquip = false;
                        }

                        if (imageParent.childCount == 0)   // 如果沒有ICON才實體化
                        {

                            imageParent.parent.name = itemID.ToString();
                            GameObject bundle = assetLoader.GetAsset(bundleName);
                            _clone = insObj.Instantiate(bundle, imageParent, bundleName, Vector3.zero, Vector3.one, new Vector2(100, 100), 310);
                            _clone.GetComponentInParent<ButtonSwitcher>().enabled = true;
                            _clone.GetComponentInParent<ButtonSwitcher>().SendMessage("EnableBtn");
                            if (!dictLoadedEquiped.ContainsKey(itemID.ToString())) dictLoadedEquiped.Add(itemID.ToString(), imageParent.parent.gameObject);      // 參考至 老鼠所在的MiceBtn位置
                            //dictLoadedItem[imageName].SendMessage("DisableBtn");

                            if (itemOrEquip) itemCount++; else armorCount++; // 判斷載入裝備物品類別遞增值
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region -- Add2Refs 加入老鼠參考 --
    /// <summary>
    /// 加入老鼠參考
    /// </summary>
    /// <param name="bundle">AssetBundle</param>
    /// <param name="myParent">參考按鈕</param>
    void Add2Refs(GameObject bundle, Transform myParent, bool isEquip)
    {
        string btnArea = myParent.parent.name;                          //按鈕存放區域名稱 Team / Mice 區域
        string itemName = bundle.name.Remove(bundle.name.Length - 4);




    }
    #endregion

    public bool bLoadedEquip(string itemName)
    {
        GameObject obj;
        if (!string.IsNullOrEmpty(itemName))
            return dictLoadedEquiped.TryGetValue(itemName, out obj);

        return false;
    }

    public bool bLoadedItem(string itemName)
    {
        GameObject obj;
        if (!string.IsNullOrEmpty(itemName))
            return dictLoadedItem.TryGetValue(itemName, out obj);
        return false;
    }

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

    #region -- OnMiceClick 當按下老鼠時 --
    public void OnItemClick(GameObject item)
    {
        if (Time.time - _lastClickTime < delayBetween2Clicks && _doubleClickChk == item)    // Double Click
            item.SendMessage("Item2Click");
        else
            StartCoroutine(OnClickCoroutine(item));

        _lastClickTime = Time.time;
        _doubleClickChk = item;
    }

    IEnumerator OnClickCoroutine(GameObject item)
    {
        yield return new WaitForSeconds(delayBetween2Clicks);

        if (Time.time - _lastClickTime < delayBetween2Clicks)
            yield break;

        Debug.Log(item.transform.GetChild(0).name);
    }
    #endregion

    #region OnTabClick
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
        OnLoadPanel();
    }
    #endregion

    void OnLoading()
    {
        Global.photonService.LoadItemData();
        Global.photonService.LoadPlayerData(Global.Account);
        Global.photonService.LoadPlayerItem(Global.Account);

    }

    void OnLoadPanel()
    {
        if (transform.parent.gameObject.activeSelf)
        {
            _dictItemData = Global.playerItem;

            ExpectOutdataObjectByKey(dictLoadedItem, _dictItemData);
            ExpectOutdataEquip(_dictItemData);

            _dictItemData = ExpectDuplicateObject(_dictItemData, dictLoadedItem);

            Dictionary<string, object> dictNotLoadedObject = new Dictionary<string, object>();

            foreach (KeyValuePair<string, object> item in _dictItemData)
            {
                string imageName = GetItemNameFromID(item.Key, Global.itemProperty);
                if (!string.IsNullOrEmpty(imageName))
                {
                    imageName += "ICON";
                    if (assetLoader.GetAsset(imageName) == null)
                        dictNotLoadedObject.Add(imageName, item.Value);
                }
            }

            if (dictNotLoadedObject.Count != 0)
            {
                assetLoader.init();
                assetLoader.LoadAsset(folderString + "/", folderString);
                assetLoader.LoadPrefab("Panel/", "InvItem");
                LoadIconObject(dictNotLoadedObject, folderString);
            }
            else
            {
                InstantiateItem(_dictItemData, infoGroupArea[2].transform, _itemType);
                InstantiateEquipIcon(Global.playerItem);
                InstantiateItemIcon(_dictItemData, _lastEmptyItemGroup.transform, _itemType);
            }

            _tmpTab = infoGroupArea[_itemType];
        }
    }


    private void ExpectOutdataEquip(Dictionary<string, object> dictServerData)
    {
        foreach (KeyValuePair<string, GameObject> equipObject in dictLoadedEquiped)
        {
            foreach (KeyValuePair<string, object> item in dictServerData)
            {
                var serverItem = item.Value as Dictionary<string, object>;
                object value;
                serverItem.TryGetValue("ItemID", out value);
                if (value.ToString() == equipObject.Key)
                {
                    serverItem.TryGetValue("IsEquip", out value);
                    if (!System.Convert.ToBoolean(value))
                    {
                        equipObject.Value.GetComponentInChildren<UISprite>().spriteName = null;
                        break;
                    }
                }
            }
        }
    }

    void selectData()
    {
        switch (_itemType)
        {
            case (int)StoreType.Item:
            case (int)StoreType.Armor:
                folderString = assetFolder[_itemType];
                _dictItemData = Global.playerItem;
                break;
        }
    }

    void LoadPlayerData()
    {
        // to do load property
        _isLoadPlayerData = !_isLoadPlayerData;
        Transform parent = infoGroupArea[3].transform;
        float winRate = ((float)Global.SumWin / (float)Global.SumBattle) * 100f;

        parent.GetChild(1).GetComponent<UILabel>().text = Global.Nickname;
        parent.GetChild(2).GetComponent<UILabel>().text = Global.Rank.ToString();
        parent.GetChild(3).GetComponent<UILabel>().text = Global.Rice.ToString();
        parent.GetChild(4).GetComponent<UILabel>().text = Global.Gold.ToString();
        parent.GetChild(5).GetComponent<UILabel>().text = (winRate).ToString() + " %";
    }

    void LoadItemStorage()
    {
        // to do load Storage
    }

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
