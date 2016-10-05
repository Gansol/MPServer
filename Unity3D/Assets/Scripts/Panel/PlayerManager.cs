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
 * 
 * ***************************************************************
 *                           ChangeLog
 *                     
 * ****************************************************************/

// mice = item
// team = equip

public class PlayerManager : PanelManager
{
    /// <summary>
    /// MiceIcon名稱、Mice按鈕
    /// </summary>
    public static Dictionary<string, GameObject> dictLoadedItem { get; set; }       // <string, GameObject>Icon名稱、Icon的按鈕
    /// <summary>
    /// TeamIcon名稱、Mice按鈕索引物件
    /// </summary>
    public static Dictionary<string, GameObject> dictLoadedEquiped { get; set; }       // <string, GameObject>Icon名稱、Icon的按鈕
    private static Dictionary<string, object> _dictMiceData, _dictTeamData;         // Json老鼠、隊伍資料
    private Dictionary<string, GameObject> _dictActor;                              // 已載入角色參考

    public GameObject[] infoGroupArea;
    public int itemOffset;
    public string[] assetFolder;

    AssetLoader assetLoader;

    private int _itemType;
    private float delayBetween2Clicks, _lastClickTime;
    private bool _LoadedIcon, _LoadedItemData, _isLoadPlayerData, _isLoadPlayerItem, _isLoadEquip;
    private GameObject _tmpTab, _lastEmptyItemGroup, _doubleClickChk;
    private string folderString;
    string[,] itemData, item, equip;
    // Use this for initialization
    void Awake()
    {
        dictLoadedItem = new Dictionary<string, GameObject>();
        dictLoadedEquiped = new Dictionary<string, GameObject>();
        _dictMiceData = Json.Deserialize(Global.MiceAll) as Dictionary<string, object>;
        _dictTeamData = Json.Deserialize(Global.Team) as Dictionary<string, object>;

        assetLoader = gameObject.AddComponent<AssetLoader>();
        _itemType = (int)StoreType.Item;
        folderString = assetFolder[_itemType];
    }

    // Update is called once per frame
    void Update()
    {
        if (Global.isPlayerDataLoaded && !_isLoadPlayerData)
        {
            _isLoadEquip = !_isLoadEquip;
            LoadPlayerData();
        }

        if (!string.IsNullOrEmpty(assetLoader.ReturnMessage))
            Debug.Log("訊息：" + assetLoader.ReturnMessage);

        if (Global.isPlayerItemLoaded && !_LoadedItemData)
        {
            assetLoader.LoadPrefab("Panel/", "InvItem");
            _LoadedItemData = !_LoadedItemData;
            itemData = GetItemInfoFromType(Global.playerItem, _itemType);
            LoadIconObject(itemData, assetFolder[_itemType]);
        }

        if (assetLoader.loadedObj && _LoadedIcon)
        {
            _LoadedIcon = !_LoadedIcon;
            assetLoader.init();
            _tmpTab = infoGroupArea[_itemType];

            InstantiateItem(itemData, infoGroupArea[2].transform, _itemType);
            InstantiateItemIcon(itemData, _lastEmptyItemGroup.transform);
        }

        if (assetLoader.loadedObj && _LoadedIcon && !_isLoadPlayerItem)
        {
            _isLoadPlayerItem = true;

        }

    }

    void OnMessage()
    {
        Debug.Log("Player Recive Message!");
        Global.photonService.LoadItemData();
        Global.photonService.LoadPlayerData(Global.Account);
        Global.photonService.LoadPlayerItem(Global.Account);
        //assetLoader.LoadAsset(assetFolder[0] + "/", assetFolder[0]); // 載入人物頭像
        assetLoader.LoadAsset(assetFolder[_itemType] + "/", assetFolder[_itemType]);    // 載入道具圖像
    }

    #region -- LoadIconObject 載入載入ICON物件 --
    /// <summary>
    /// 載入ICON物件
    /// </summary>
    /// <param name="itemData">物件陣列</param>
    /// <param name="folder">資料夾</param>
    public void LoadIconObject(string[,] itemData, string folder)    // 載入遊戲物件
    {
        if (itemData != null)
        {
            string itemName = null;

            for (int i = 0; i < itemData.GetLength(0); i++)
            {
                itemName = GetItemNameFromID(itemData[i, 0], Global.itemProperty);
                if (!string.IsNullOrEmpty(itemName)) assetLoader.LoadPrefab(folder + "/", itemName + "ICON");


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
    void InstantiateItem(string[,] itemData, Transform itemPanel, int itemType)
    {
        if (itemPanel.transform.childCount == 0)
        {
            _lastEmptyItemGroup = CreateEmptyGroup(itemPanel, itemType);
            InstantiateItem2(itemData, _lastEmptyItemGroup.transform, itemData.GetLength(0));
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
                InstantiateItem2(itemData, _lastEmptyItemGroup.transform, itemData.GetLength(0));
            }
        }
        infoGroupArea[2].GetComponent<BoxCollider>().enabled = false;   // 開關防止Item按鈕失效
        infoGroupArea[2].GetComponent<BoxCollider>().enabled = true;
    }

    void InstantiateItem2(string[,] itemData, Transform parent, int itemCount)
    {
        Vector2 pos = new Vector2();
        string itemName = "InvItem", folderPath = "Panel/";
        int count = parent.childCount;

        for (int i = 0; i < itemCount; i++)
        {
            if (assetLoader.GetAsset(folderPath, itemName))                  // 已載入資產時
            {
                pos = sortItemPos(9, 3, new Vector2(itemOffset, itemOffset), pos, count + i);
                GameObject bundle = assetLoader.GetAsset(folderPath, itemName);
                InstantiateObject insObj = new InstantiateObject();
                string bundleName = GetItemNameFromID(itemData[i, 0], Global.itemProperty);
                _clone = insObj.Instantiate(bundle, parent, bundleName, new Vector3(pos.x, pos.y), Vector3.one, Vector2.zero, -1);
                pos.x += itemOffset;
            }
        }
    }
    #endregion

    #region InsEquip
    void SelectEquipedObject()
    {
        selectData();
        itemData = GetItemInfoFromType(itemData, (int)StoreType.Item);
        item = new string[itemData.GetLength(0), itemData.GetLength(1)];
        equip = new string[itemData.GetLength(0), itemData.GetLength(1)];
        bool isEquip;

        for (int i = 0; i < itemData.GetLength(0); i++)
        {
            isEquip = System.Convert.ToBoolean(itemData[i, 3]);
            if (isEquip == true)
            {
                _itemType = System.Convert.ToInt32(itemData[i, 2]);
                if (_itemType == (int)StoreType.Item)
                {
                    item[i, 0] = itemData[i, 0];
                }
                else if (_itemType == (int)StoreType.Armor)
                {
                    equip[i, 0] = itemData[i, 0];
                }
            }
        }
    }
    #endregion

    #region -- InstantiateItemIcon 實體化老鼠物件--
    /// <summary>
    /// 實體化載入完成的遊戲物件，利用玩家JASON資料判斷必要實體物件
    /// </summary>
    /// <param name="dictionary">資料字典</param>
    /// <param name="myParent">實體化父系位置</param>
    void InstantiateItemIcon(string[,] itemData, Transform myParent)
    {

        // to do check has icon object
        for (int i = 0; i < itemData.GetLength(0); i++)
        {
            if (!string.IsNullOrEmpty(itemData[i, 0]))
            {
                string itemName = GetItemNameFromID(itemData[i, 0], Global.itemProperty);
                string bundleName = itemName + "ICON";
                if (assetLoader.GetAsset(folderString + "/", bundleName))                  // 已載入資產時
                {
                    GameObject bundle = assetLoader.GetAsset(folderString + "/", bundleName);
                    Transform imageParent = myParent.GetChild(i).GetChild(0);
                    if (imageParent.childCount == 0)   // 如果沒有ICON才實體化
                    {
                        InstantiateObject insObj = new InstantiateObject();
                        _clone = insObj.Instantiate(bundle, imageParent, bundleName, Vector3.zero, Vector3.one, new Vector2(100, 100), 310);
                        _clone.GetComponentInParent<ButtonSwitcher>()._activeBtn = true;
                        Add2Refs(bundle, myParent.GetChild(i), System.Convert.ToBoolean(itemData[i, 3])); // itemData[i, 3] = bool is equip ?

                        // instantiate equip
                        if (System.Convert.ToBoolean(itemData[i, 3]))
                        {
                            if (itemData[i, 2] == ((int)StoreType.Item).ToString())
                                InstantiateEquipIcon(itemName, infoGroupArea[0].transform);
                            else if (itemData[i, 2] == ((int)StoreType.Armor).ToString())
                                InstantiateEquipIcon(itemName, infoGroupArea[1].transform);
                        }
                    }
                }
                else
                {
                    Debug.LogError("Assetbundle reference not set to an instance.");
                }
            }
        }
        _LoadedIcon = true;
    }
    #endregion

    #region -- InstantiateItemIcon 實體化老鼠物件--
    /// <summary>
    /// 實體化載入完成的遊戲物件，利用玩家JASON資料判斷必要實體物件
    /// </summary>
    /// <param name="dictionary">資料字典</param>
    /// <param name="myParent">實體化父系位置</param>
    void InstantiateEquipIcon(string itemName, Transform myParent)
    {
        string bundleName = itemName + "ICON";
        for (int i = 0; i < myParent.childCount; i++)
        {
            if (assetLoader.GetAsset(folderString + "/", bundleName))                  // 已載入資產時
            {
                Transform imageParent = myParent.GetChild(i).GetChild(0);

                if (imageParent.childCount == 0)   // 如果沒有ICON才實體化
                {
                    myParent.GetChild(i).name = itemName;
                    GameObject bundle = assetLoader.GetAsset(folderString + "/", bundleName);
                    InstantiateObject insObj = new InstantiateObject();
                    _clone = insObj.Instantiate(bundle, imageParent, bundleName, Vector3.zero, Vector3.one, new Vector2(100, 100), 310);
                    _clone.GetComponentInParent<ButtonSwitcher>().enabled = true;
                    _clone.GetComponentInParent<ButtonSwitcher>().SendMessage("EnableBtn");
                    dictLoadedItem[itemName].SendMessage("DisableBtn");
                    break;
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

        if (isEquip && !dictLoadedEquiped.ContainsKey(itemName)) dictLoadedEquiped.Add(itemName, GetLoadedMice(itemName));      // 參考至 老鼠所在的MiceBtn位置
        if (!dictLoadedItem.ContainsKey(itemName)) dictLoadedItem.Add(itemName, myParent.gameObject);          // 加入索引 老鼠所在的MiceBtn位置
    }
    #endregion

    public bool bLoadedEquip(string itemName)
    {
        GameObject obj;
        return dictLoadedEquiped.TryGetValue(itemName, out obj);
    }

    public bool bLoadedItem(string itemName)
    {
        GameObject obj;
        return dictLoadedItem.TryGetValue(itemName, out obj);
    }

    public GameObject GetLoadedMice(string itemName)
    {
        GameObject obj;
        if (dictLoadedItem.TryGetValue(itemName, out obj))
            return obj;
        return null;
    }

    public GameObject GetLoadedTeam(string itemName)
    {
        GameObject obj;
        if (dictLoadedEquiped.TryGetValue(itemName, out obj))
            return obj;
        return null;
    }



    void LoadPanel()
    {

    }

    void OnReLoad()
    {

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
                {
                    _LoadedIcon = !_LoadedIcon;
                    _itemType = (int)StoreType.Item;
                    selectData();
                    itemData = GetItemInfoFromType(itemData, _itemType);
                    assetLoader.init();
                    assetLoader.LoadAsset(folderString + "/", folderString);
                    LoadIconObject(itemData, folderString);
                    _tmpTab = infoGroupArea[_itemType];
                    break;
                }
            case 2:
                {
                    _LoadedIcon = !_LoadedIcon;
                    _itemType = (int)StoreType.Armor;
                    selectData();
                    itemData = GetItemInfoFromType(itemData, _itemType);
                    assetLoader.init();
                    assetLoader.LoadAsset(folderString + "/", folderString);
                    LoadIconObject(itemData, folderString);
                    _tmpTab = infoGroupArea[_itemType];
                    break;
                }
            default:
                {
                    Debug.LogError("Unknow Tab!");
                    break;
                }
        }


    }
    #endregion

    void selectData()
    {
        switch (_itemType)
        {
            case (int)StoreType.Item:
            case (int)StoreType.Armor:
                folderString = assetFolder[_itemType];
                itemData = Global.playerItem;
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
