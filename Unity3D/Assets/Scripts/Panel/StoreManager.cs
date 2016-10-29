using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
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
 * 
 * ***************************************************************
 *                           ChangeLog
 * 20160914 v1.0.1b  2次重購，獨立實體化物件                          
 * 20160711 v1.0.1a  1次重構，獨立AssetLoader                       
 * 20160705 v1.0.0   0版完成，載入老鼠部分未來需要修改                     
 * ****************************************************************/
public class StoreManager : PanelManager
{
    public GameObject[] infoGroupsArea;
    public string[] assetFolder;
    public int itemOffsetX, itemOffsetY;
    public Vector3 actorScale;

    private AssetLoader assetLoader;
    private GameObject _btnClick, _tmpActor, _lastItem, _lastStoreItemGroup;
    private static GameObject _tmpTab;
    private bool _LoadedGashapon, _LoadedMice, _LoadedActor;
    private Dictionary<string, GameObject> _dictActor;
    private int _itemType = -1;
    private Dictionary<string, object> itemData;
    private string folderString;
    /// <summary>
    /// 儲存購買商品資料 0:商品ID、1:商品名稱、2:道具類型、3:貨幣類型、4:數量
    /// </summary>
    private string[] buyingGoodsData;

    void Awake()
    {
        Global.photonService.LoadStoreDataEvent += OnLoadPanel;
        actorScale = new Vector3(1.5f, 1.5f, 1);
        assetLoader = gameObject.AddComponent<AssetLoader>();
        _dictActor = new Dictionary<string, GameObject>();
        buyingGoodsData = new string[5];
        Global.photonService.UpdateCurrencyEvent += LoadPlayerInfo;
    }

    void Update()
    {
        if (!string.IsNullOrEmpty(assetLoader.ReturnMessage))
            Debug.Log("訊息：" + assetLoader.ReturnMessage);

        // ins fisrt load panelScene : Gashapon
        if (assetLoader.loadedObj && !_LoadedGashapon)
        {
            _LoadedGashapon = !_LoadedGashapon;
            assetLoader.init();
            _tmpTab = infoGroupsArea[2];
            InstantiateGashapon(infoGroupsArea[2].transform, assetFolder[0]);
        }

        if (assetLoader.loadedObj && !_LoadedMice)
        {
            _LoadedMice = !_LoadedMice;
            assetLoader.init();
            //itemData = Global.storeItem;
            InstantiateItem(itemData, infoGroupsArea[3].transform, _itemType);
            Transform parent = infoGroupsArea[3].transform.FindChild(_itemType.ToString());
            selectItemProperty(_itemType);
            LoadItemData(itemData, parent, _itemType);
            InstantiateItemIcon(itemData, parent);
            selectItemData(_itemType);

            LoadPrice(itemData, parent, _itemType);          
        }

        if (assetLoader.loadedObj && !_LoadedActor)
        {
            _LoadedActor = !_LoadedActor;
            assetLoader.init();
            InstantiateActor(infoGroupsArea[4].transform.GetChild(0));
        }

    }

    void selectItemData(int _itemType)
    {
        switch (_itemType)
        {
            case (int)StoreType.Mice:
                folderString = assetFolder[_itemType];
                itemData = Global.storeItem;
                break;
            case (int)StoreType.Item:
            case (int)StoreType.Armor:
                folderString = assetFolder[_itemType];
                itemData = Global.storeItem;
                break;
        }
    }

    void selectItemProperty(int _itemType)
    {
        switch (_itemType)
        {
            case (int)StoreType.Mice:
                folderString = assetFolder[_itemType];
                itemData = Global.miceProperty;
                break;
            case (int)StoreType.Item:
            case (int)StoreType.Armor:
                folderString = assetFolder[_itemType];
                itemData = Global.itemProperty;
                break;
        }
    }

    void OnLoading()
    {
        assetLoader.LoadAsset(assetFolder[0] + "/", assetFolder[0]);
        LoadGashapon(assetFolder[0]);
        LoadPlayerInfo();
        Global.photonService.LoadStoreData();
        Global.photonService.LoadItemData();
        EventMaskSwitch.lastPanel = gameObject;
    }

    public void OnLoadPanel()
    {
        itemData = Global.storeItem;

        Global.isStoreLoaded = false;
    }

    private void LoadPlayerInfo()
    {
        infoGroupsArea[0].transform.GetChild(0).GetComponent<UILabel>().text = "20";
        infoGroupsArea[0].transform.GetChild(1).GetComponent<UILabel>().text = Global.Rice.ToString();
        infoGroupsArea[0].transform.GetChild(2).GetComponent<UILabel>().text = Global.Gold.ToString();
    }

    #region OnClcikEvents
    #region -- OnClick 按下事件 --
    public void OnGashaponClick(GameObject obj)
    {
        Debug.Log(obj.name);
    }

    public void OnItemClick(GameObject obj)
    {
        buyingGoodsData[0] = obj.GetComponent<Item>().itemInfo[(int)StoreProperty.ItemID];
        buyingGoodsData[1] = obj.GetComponent<Item>().itemInfo[(int)StoreProperty.ItemName];
        buyingGoodsData[2] = obj.GetComponent<Item>().itemInfo[(int)StoreProperty.ItemType];
        Debug.Log(obj.name);
        _lastItem = obj;
        infoGroupsArea[4].SetActive(true);
        int itemType = int.Parse(obj.GetComponent<Item>().itemInfo[(int)StoreProperty.ItemType]);
        LoadProperty loadProperty = new LoadProperty();
        loadProperty.LoadMiceProperty(obj, infoGroupsArea[4].transform.GetChild(0).gameObject, 0);
        loadProperty.LoadPrice(obj, infoGroupsArea[4].transform.GetChild(0).gameObject, itemType);
        LoadActor(obj, itemType);   // 錯誤 暫時道具沒有動畫物件
        EventMaskSwitch.Switch(infoGroupsArea[4]);
    }

    public void OnBuyClick(GameObject myPanel)
    {

        myPanel.SetActive(false);
        infoGroupsArea[5].SetActive(true);
        BuyWindowsInit();
        LoadProperty loadProperty = new LoadProperty();
        //loadProperty.LoadMiceProperty(_lastItem, infoGroupsArea[5], 0);
        LoadBuyCountInfo(_lastItem, infoGroupsArea[5].transform);
        EventMaskSwitch.Switch(infoGroupsArea[5]);
    }

    private void BuyWindowsInit()
    {
        buyingGoodsData[0] = _lastItem.GetComponent<Item>().itemInfo[(int)StoreProperty.ItemID];
        buyingGoodsData[1] = _lastItem.GetComponent<Item>().itemInfo[(int)StoreProperty.ItemName];
        buyingGoodsData[2] = _lastItem.GetComponent<Item>().itemInfo[(int)StoreProperty.ItemType];
        buyingGoodsData[3] = _lastItem.GetComponent<Item>().itemInfo[(int)StoreProperty.CurrencyType];
        buyingGoodsData[4] = "1";
        infoGroupsArea[5].transform.GetChild(0).GetChild(4).GetComponent<UILabel>().text = "1";  // count = 1
        infoGroupsArea[5].transform.GetChild(0).GetChild(3).GetComponent<UILabel>().text = _lastItem.GetComponent<Item>().itemInfo[(int)StoreProperty.Price]; // price
    }

    public void OnQuantity(GameObject obj)
    {
        int price = int.Parse(infoGroupsArea[5].transform.GetChild(0).GetChild(2).GetComponent<UILabel>().text);
        int sum = int.Parse(infoGroupsArea[5].transform.GetChild(0).GetChild(3).GetComponent<UILabel>().text);
        int count = int.Parse(infoGroupsArea[5].transform.GetChild(0).GetChild(4).GetComponent<UILabel>().text);

        count += (obj.name == "Add") ? 1 : -1;
        count = (count < 0) ? 0 : count;
        buyingGoodsData[4] = infoGroupsArea[5].transform.GetChild(0).GetChild(4).GetComponent<UILabel>().text = count.ToString();
        infoGroupsArea[5].transform.GetChild(0).GetChild(3).GetComponent<UILabel>().text = (price * count).ToString();
    }

    public void OnComfirm(GameObject myPanel)
    {
        myPanel.SetActive(false);
        //buyingGoodsData[2] = _itemType.ToString();
        Global.photonService.BuyItem(Global.Account, buyingGoodsData);
    }

    public void OnClosed(GameObject obj)
    {
        EventMaskSwitch.lastPanel = null;
        GameObject root = obj.transform.parent.parent.gameObject;

        _tmpTab.SetActive(false);
        infoGroupsArea[2].SetActive(true);
        _tmpTab = infoGroupsArea[2];

        GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().LoadPanel(obj.transform.parent.gameObject);
        EventMaskSwitch.Switch(root);
    }

    public void OnReturn(GameObject obj)
    {
        EventMaskSwitch.openedPanel.SetActive(false);
        EventMaskSwitch.Switch(obj);
    }

    public void OnTabClick(GameObject obj)
    {
        Debug.Log(obj.name);

        int value = int.Parse(obj.name.Remove(0, 3));

        switch (value)
        {
            case 1:
                {
                    _itemType = (int)StoreType.Gashapon;
                    if (_tmpTab != infoGroupsArea[2]) _tmpTab.SetActive(false);
                    infoGroupsArea[2].SetActive(true);
                    _tmpTab = infoGroupsArea[2];
                    break;
                }
            case 2:
                {
                    _itemType = (int)StoreType.Mice;
                    selectItemData(_itemType);
                    itemData = GetItemInfoFromType(itemData, _itemType);
                    if (_tmpTab != infoGroupsArea[3]) _tmpTab.SetActive(false);
                    assetLoader.init();
                    assetLoader.LoadAsset(folderString + "/", folderString);
                    LoadIconObject(itemData, folderString);
                    assetLoader.LoadPrefab("Panel/", "Item");
                    _LoadedMice = false;
                    infoGroupsArea[3].SetActive(true);
                    _tmpTab = infoGroupsArea[3];
                    break;
                }
            case 3:
                {
                    _itemType = (int)StoreType.Item;
                    selectItemData(_itemType);
                    itemData = GetItemInfoFromType(itemData, _itemType);
                    if (_tmpTab != infoGroupsArea[3]) _tmpTab.SetActive(false);
                    assetLoader.init();
                    assetLoader.LoadAsset(folderString + "/", folderString);
                    LoadIconObject(itemData, folderString);
                    assetLoader.LoadPrefab("Panel/", "Item");
                    _LoadedMice = false;
                    infoGroupsArea[3].SetActive(true);
                    _tmpTab = infoGroupsArea[3];
                    break;
                }
            case 4:
                {
                    _itemType = (int)StoreType.Armor;
                    selectItemData(_itemType);
                    itemData = GetItemInfoFromType(itemData, _itemType);
                    if (_tmpTab != infoGroupsArea[3]) _tmpTab.SetActive(false);
                    assetLoader.init();
                    assetLoader.LoadAsset(folderString + "/", folderString);
                    LoadIconObject(itemData, folderString);
                    assetLoader.LoadPrefab("Panel/", "Item");
                    _LoadedMice = false;
                    infoGroupsArea[3].SetActive(true);
                    _tmpTab = infoGroupsArea[3];
                    break;
                }
            case 5:
                {
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
    #endregion


    #region -- LoadGashapon 載入轉蛋物件 --
    void LoadGashapon(string folder)
    {
        for (int i = 1; i <= 3; i++)
            assetLoader.LoadPrefab(folder + "/", folder + i);
    }
    #endregion


    #region -- LoadPrice 載入物件價格 --
    void LoadPrice(Dictionary<string, object> itemData, Transform parent, int itemType)
    {
        itemData = GetItemInfoFromType(itemData, itemType);
        for (int i = 0; i < itemData.Count; i++)
        {
            parent.GetChild(i).GetComponentInChildren<UILabel>().text = parent.GetChild(i).GetComponent<Item>().itemInfo[(int)StoreProperty.Price];
        }
    }
    #endregion

    #region -- LoadBuyCountInfo 載入物件價格 --
    void LoadBuyCountInfo(GameObject item, Transform parent)
    {
        parent.GetChild(0).GetChild(1).GetComponent<UILabel>().text = item.GetComponent<Item>().itemProperty[(int)ItemProperty.ItemID];
        parent.GetChild(0).GetChild(2).GetComponent<UILabel>().text = item.GetComponent<Item>().itemInfo[(int)StoreProperty.Price];
    }
    #endregion

    #region -- LoadActor 載入老鼠角色 --
    private void LoadActor(GameObject btn_mice, int itemType)
    {
        if (itemType == (int)StoreType.Mice)
        {
            GameObject _miceImage;
            string miceName = btn_mice.transform.GetComponentInChildren<UISprite>().name;

            _btnClick = btn_mice;

            if (_tmpActor != null) _tmpActor.SetActive(false);          // 如果暫存老鼠圖片不是空的(防止第一次點擊出錯)，將上一個老鼠圖片隱藏

            if (_dictActor.TryGetValue(miceName, out _miceImage))       // 假如已經載入老鼠圖片了 直接顯示
            {
                _miceImage.SetActive(true);
                _tmpActor = _miceImage;
            }
            else
            {
                if (assetLoader.GetAsset(miceName) != null)
                {
                    InstantiateActor(infoGroupsArea[4].transform.GetChild(0));
                }
                else
                {
                    _LoadedActor = false;
                    assetLoader.LoadAsset(miceName + "/", miceName);
                    assetLoader.LoadPrefab(miceName + "/", miceName);
                }

                //LoadMiceAsset(btn_mice);
            }
        }
    }
    #endregion

    #region -- LoadIconObject 載入載入ICON物件 --
    /// <summary>
    /// 載入ICON物件
    /// </summary>
    /// <param name="itemData">物件陣列</param>
    /// <param name="folder">資料夾</param>
    public void LoadIconObject(Dictionary<string, object> itemData, string folder)    // 載入遊戲物件
    {
        foreach (KeyValuePair<string, object> item in itemData)
        {
            var nestedData = item.Value as Dictionary<string, object>;
            object itemName;
            nestedData.TryGetValue("ItemName", out itemName);
            assetLoader.LoadPrefab(folder + "/", itemName.ToString() + "ICON");
        }
    }
    #endregion

    #region -- InstantiateActor 實體化老鼠角色 --
    private void InstantiateActor(Transform parent)
    {
        GameObject _tmp;
        InstantiateObject insObj = new InstantiateObject();
        string miceName = _btnClick.transform.GetComponentInChildren<UISprite>().name;
        GameObject bundle = (GameObject)assetLoader.GetAsset(miceName);

        if (bundle != null)                  // 已載入資產時
        {
            _clone = insObj.InstantiateActor(bundle, parent.GetChild(0), miceName, actorScale);
        }
        else
        {
            Debug.LogError("Assetbundle reference not set to an instance. at InstantiateActor.");
        }

        if (_dictActor.TryGetValue(_clone.name, out _tmp) != false)
            _dictActor.Add(_clone.name, _clone);

        _tmpActor = _clone;
        _LoadedActor = true;
    }
    #endregion

    #region -- InstantiateGashapon 實體化轉蛋物件--
    /// <summary>
    /// 實體化載入完成的轉蛋物件，利用資料夾名稱判斷必要實體物件
    /// </summary>
    /// <param name="myParent">實體化父系位置</param>
    /// <param name="folder">資料夾名稱</param>
    void InstantiateGashapon(Transform myParent, string folder)
    {
        for (int i = 0; i < 3; i++)
        {
            if (assetLoader.GetAsset(folder + (i + 1).ToString())!=null)                  // 已載入資產時
            {
                GameObject bundle = assetLoader.GetAsset(folder + (i + 1).ToString());
                Transform parent = myParent.GetChild(1).GetChild(i).GetChild(0);
                InstantiateObject insObj = new InstantiateObject();

                _clone = insObj.Instantiate(bundle, parent, folder + (i + 1).ToString(), Vector3.zero, Vector3.one, Vector2.zero, -1);
            }
            else
            {
                Debug.LogError("Assetbundle reference not set to an instance.");
            }
        }
        _LoadedGashapon = true;
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
            _lastStoreItemGroup = CreateEmptyGroup(itemPanel, itemType);
            InstantiateItem2(itemData, _lastStoreItemGroup.transform, itemData.Count);
        }
        else
        {
            if (itemPanel.FindChild(itemType.ToString()))
            {
                _lastStoreItemGroup.SetActive(false);
                _lastStoreItemGroup = itemPanel.FindChild(itemType.ToString()).gameObject;
                _lastStoreItemGroup.SetActive(true);
            }
            else if (_lastStoreItemGroup != itemPanel.FindChild(itemType.ToString()))
            {
                _lastStoreItemGroup.SetActive(false);
                _lastStoreItemGroup = CreateEmptyGroup(itemPanel, itemType);
                InstantiateItem2(itemData, _lastStoreItemGroup.transform, itemData.Count);
            }
        }
    }




    void InstantiateItem2(Dictionary<string, object> itemData, Transform parent, int itemCount)
    {
        Vector2 pos = new Vector2();
        string itemName = "Item", folderPath = "Panel/";
        int count = parent.childCount, i = 0;

        foreach (KeyValuePair<string, object> item in itemData)
        {
            var nestedData = item.Value as Dictionary<string, object>;
            if (assetLoader.GetAsset(itemName)!=null)                  // 已載入資產時
            {
                pos = sortItemPos(9, 3, new Vector2(itemOffsetX, itemOffsetY), pos, count + i);
                GameObject bundle = assetLoader.GetAsset(itemName);
                InstantiateObject insObj = new InstantiateObject();
                object bundleName;
                nestedData.TryGetValue("ItemName", out bundleName);
                assetLoader.LoadPrefab(folderPath, bundleName + "ICON");
                _clone = insObj.Instantiate(bundle, parent, bundleName.ToString(), new Vector3(pos.x, pos.y), Vector3.one, Vector2.zero, -1);
                pos.x += itemOffsetX;
            }
            i++;
        }
    }
    #endregion


    #region LoadItemData
    private void LoadItemData(Dictionary<string, object> itemData, Transform parent, int itemType)
    {
        int i = 0, j = 0;
        foreach (KeyValuePair<string, object> item in itemData)
        {
            var nestedData = item.Value as Dictionary<string, object>;
            j = 0;
            foreach (KeyValuePair<string, object> nested in nestedData)
            {
                parent.GetChild(i).GetComponent<Item>().itemProperty[j] = nested.Value.ToString();
                j++;
            }
            i++;
        }

        itemData = GetItemInfoFromType(Global.storeItem, _itemType);
        i = j = 0;

        foreach (KeyValuePair<string, object> item in itemData)
        {
            var nestedData = item.Value as Dictionary<string, object>;
            j = 0;
            foreach (KeyValuePair<string, object> nested in nestedData)
            {
                parent.GetChild(i).GetComponent<Item>().itemInfo[j] = nested.Value.ToString();
                j++;
            }
            i++;
        }

        //for (int i = 0; i < itemData.GetLength(0); i++) // 載入商品資料
        //{
        //    for (int j = 0; j < itemData.GetLength(1); j++) // 載入商品資料
        //    {
        //        parent.GetChild(i).GetComponent<Item>().itemProperty[j] = itemData[i, j];
        //        //Debug.Log("ITEM DATA " + i.ToString() + " :  " + itemData[i, j]);
        //    }
        //}
        //itemData = GetItemInfoFromType(Global.storeItem, _itemType);
        //Debug.Log("FFF" + itemData.GetLength(0));
        //for (int i = 0; i < itemData.GetLength(0); i++) // 載入商品資料
        //{
        //    for (int j = 0; j < itemData.GetLength(1); j++) // 載入商品資料
        //    {
        //        parent.GetChild(i).GetComponent<Item>().itemInfo[j] = itemData[i, j];
        //        //Debug.Log("ITEM DATA " + i.ToString() + " :  " + itemData[i, j]);
        //    }
        //}
    }
    #endregion


    #region -- InstantiateItemIcon 實體化老鼠物件--
    /// <summary>
    /// 實體化載入完成的遊戲物件，利用玩家JASON資料判斷必要實體物件
    /// </summary>
    /// <param name="dictionary">資料字典</param>
    /// <param name="myParent">實體化父系位置</param>
    void InstantiateItemIcon(Dictionary<string, object> itemData, Transform myParent)
    {
        int i = 0;
        selectItemData(_itemType);
        // to do check has icon object
        foreach (KeyValuePair<string, object> item in itemData)
        {
            object itemName;
            var nestedData = item.Value as Dictionary<string, object>;
            nestedData.TryGetValue("ItemName", out itemName);
            string bundleName = itemName.ToString() + "ICON";

            if (assetLoader.GetAsset(bundleName)!=null)                  // 已載入資產時
            {
                GameObject bundle = assetLoader.GetAsset(bundleName);
                Transform imageParent = myParent.GetChild(i).GetChild(0);
                if (imageParent.childCount == 0)   // 如果沒有ICON才實體化
                {
                    InstantiateObject insObj = new InstantiateObject();
                    insObj.Instantiate(bundle, imageParent, itemName.ToString(), Vector3.zero, Vector3.one, new Vector2(150, 150), 310);
                }
            }
            else
            {
                Debug.LogError("Assetbundle reference not set to an instance.");
            }
            i++;
        }
        _LoadedMice = true;
    }
    #endregion


}
