using UnityEngine;
using System.Linq;
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
 * 20161102 v1.0.2   3次重構，改變繼承至 PanelManager>MPPanel
 * 20160914 v1.0.1b  2次重構，獨立實體化物件                          
 * 20160711 v1.0.1a  1次重構，獨立AssetLoader                       
 * 20160705 v1.0.0   0版完成，載入老鼠部分未來需要修改                     
 * ****************************************************************/
public class StoreManager : PanelManager
{
    #region 欄位
    public GameObject[] infoGroupsArea;                         // 物件存放區
    public string[] assetFolder;                                // 資產資料夾名稱
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

    /// <summary>
    /// 儲存購買商品資料 0:商品ID、1:商品名稱、2:道具類型、3:貨幣類型、4:數量
    /// </summary>
    private string[] buyingGoodsData;
    private string _folderString;                               // 資料夾名稱
    private int _itemType;                                      // 道具形態
    private bool _bFirstLoad,_bLoadedGashapon, _bLoadedIcon, _bLoadedActor; // 是否載入轉蛋、是否載入圖片、是否載入角色
    private static GameObject _tmpTab, _lastItem;               // 暫存分頁、暫存按下
    private Dictionary<string, object> _itemData;               // 道具資料

    private ObjectFactory insObj;

    private Dictionary<string, GameObject> dictItemRefs;
    #endregion

    private void Awake()
    {
        _bFirstLoad = true;
        insObj = new ObjectFactory();
        assetLoader = gameObject.AddMissingComponent<AssetLoader>();
        dictItemRefs = new Dictionary<string, GameObject>();

        _itemType = -1;
        tablePageCount = 9;
        tableRowCount = 3;
        actorScale = new Vector3(1.5f, 1.5f, 1f);
        buyingGoodsData = new string[5];
    }

    void OnEnable()
    {
        Global.photonService.LoadStoreDataEvent += OnLoadPanel;
        Global.photonService.UpdateCurrencyEvent += LoadPlayerInfo;
    }

    private void Update()
    {
        if (!string.IsNullOrEmpty(assetLoader.ReturnMessage))
            Debug.Log("訊息：" + assetLoader.ReturnMessage);

        // ins fisrt load panelScene : Gashapon
        if (assetLoader.loadedObj && !_bLoadedGashapon)                 // 載入轉蛋完成後 實體化 轉蛋
        {
            _bLoadedGashapon = !_bLoadedGashapon;
            assetLoader.init();
            //_tmpTab = infoGroupsArea[2];
            //  InstantiateGashapon(infoGroupsArea[2].transform, assetFolder[0]);
            GameObject go = new GameObject();
            go.name = "Tab2";
            OnTabClick(go);
        }

        if (assetLoader.loadedObj && _bLoadedIcon)                      // 載入道具完成後 實體化 道具
        {
            _bLoadedIcon = !_bLoadedIcon;
            assetLoader.init();
            //itemData = Global.storeItem;

            Dictionary<string, GameObject> tmpDict;
            tmpDict = InstantiateItem(_itemData, "Item", _itemType, infoGroupsArea[3].transform, itemOffset, tablePageCount, tableRowCount);

            if (tmpDict != null) dictItemRefs = dictItemRefs.Concat(tmpDict).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value);
            Transform parent = infoGroupsArea[3].transform.FindChild(_itemType.ToString());

            SelectItemProperty(_itemType);
            //LoadItemData(_itemData, parent, _itemType);                 // 載入道具資訊資料

            SelectStoreItemData(_itemType);                                  // 選擇商店資料
            InstantiateItemIcon(_itemData, parent);
            LoadPrice(_itemData, _itemType);
            EventMaskSwitch.Resume();
            GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().Panel[5].SetActive(false);
            EventMaskSwitch.Switch(gameObject, false);
            EventMaskSwitch.lastPanel = gameObject;
        }

        if (assetLoader.loadedObj && _bLoadedActor)                     // 載入角色完成後 實體化 角色
        {
            _bLoadedActor = !_bLoadedActor;
            assetLoader.init();
            _bLoadedActor = InstantiateActor(_lastItem.GetComponentInChildren<UISprite>().name, infoGroupsArea[4].transform.Find("Mice").GetChild(0).Find("Image"), actorScale);
        }
    }

    #region -- OnLoadPanel 載入面板 --
    public override void OnLoading()
    {
        Global.photonService.LoadStoreData();
        Global.photonService.LoadItemData();
        Global.photonService.LoadCurrency(Global.Account);
        Global.photonService.LoadPlayerItem(Global.Account);
    }

    protected override void OnLoadPanel()
    {
        if (transform.parent.gameObject.activeSelf)
        {
            if (_bFirstLoad)
            {
                _bFirstLoad = false;
            }
            else
            {
                EventMaskSwitch.Resume();
                GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().Panel[5].SetActive(false);
                EventMaskSwitch.Switch(gameObject, false);
                EventMaskSwitch.lastPanel = gameObject;
            }

            _itemData = Global.storeItem;
            assetLoader.LoadAsset(assetFolder[0] + "/", assetFolder[0]);
            LoadGashapon(assetFolder[0]);
            LoadPlayerInfo();
            EventMaskSwitch.lastPanel = gameObject;
        }
        Global.isStoreLoaded = false;
    }
    #endregion

    #region -- LoadPlayerInfo 載入玩家資訊 --
    private void LoadPlayerInfo()
    {
        infoGroupsArea[0].transform.GetChild(0).GetComponent<UILabel>().text = Global.Rank.ToString();
        infoGroupsArea[0].transform.GetChild(1).GetComponent<UILabel>().text = Global.Rice.ToString();
        infoGroupsArea[0].transform.GetChild(2).GetComponent<UILabel>().text = Global.Gold.ToString();
    }
    #endregion

    #region -- OnClick 按下事件 --
    public void OnGashaponClick(GameObject obj)
    {
        Debug.Log(obj.name);
    }

    public void OnItemClick(GameObject obj)
    {

        _lastItem = obj;
        Dictionary<string, object> dictItemProperty = Global.storeItem[_lastItem.name] as Dictionary<string, object>;
        Dictionary<string, object> playerItemData = new Dictionary<string,object>();
        if (Global.playerItem.ContainsKey(_lastItem.name))
            playerItemData = Global.playerItem[_lastItem.name] as Dictionary<string, object>;
        object value;

        SelectItemProperty(_itemType);

        if ((StoreType)_itemType == StoreType.Mice)
        {
            LoadProperty loadProperty = new LoadProperty();
            infoGroupsArea[4].SetActive(true);
            infoGroupsArea[4].transform.Find("Item").gameObject.SetActive(false);
            infoGroupsArea[4].transform.Find("Mice").gameObject.SetActive(true);

            loadProperty.LoadItemProperty(obj, infoGroupsArea[4].transform.Find("Mice").GetChild(0).gameObject, Global.miceProperty, _itemType);
            loadProperty.LoadItemProperty(obj, infoGroupsArea[4].transform.Find("Mice").GetChild(0).gameObject, _itemData, _itemType);
            loadProperty.LoadPrice(obj, infoGroupsArea[4].transform.Find("Mice").GetChild(0).gameObject, _itemType);

            if (playerItemData.Count != 0)
            {
                playerItemData.TryGetValue("ItemCount", out value);
                infoGroupsArea[4].transform.Find("Mice").GetChild(0).Find("Count").GetComponent<UILabel>().text = value.ToString();
            }
            else
            {
                infoGroupsArea[4].transform.Find("Mice").GetChild(0).Find("Count").GetComponent<UILabel>().text = "0";
            }

            _bLoadedActor = LoadActor(obj, infoGroupsArea[4].transform.Find("Mice").GetChild(0).Find("Image"), actorScale);

            infoGroupsArea[4].transform.parent.gameObject.layer = LayerMask.NameToLayer("ItemInfo");
            infoGroupsArea[4].layer = LayerMask.NameToLayer("ItemInfo");

            dictItemProperty.TryGetValue("Description", out value);
            infoGroupsArea[4].transform.Find("Mice").GetChild(0).Find("Description").GetComponent<UILabel>().text = value.ToString(); 

            EventMaskSwitch.Switch(infoGroupsArea[4], true);
        }
        else if ((StoreType)_itemType == StoreType.Item || (StoreType)_itemType == StoreType.Armor)
        {
            LoadProperty loadProperty = new LoadProperty();
            infoGroupsArea[4].SetActive(true);
            infoGroupsArea[4].transform.Find("Item").gameObject.SetActive(true);
            infoGroupsArea[4].transform.Find("Mice").gameObject.SetActive(false);

            loadProperty.LoadItemProperty(obj, infoGroupsArea[4].transform.Find("Item").GetChild(0).gameObject, _itemData, _itemType);
            loadProperty.LoadPrice(obj, infoGroupsArea[4].transform.Find("Item").GetChild(0).gameObject, _itemType);


            

            
            dictItemProperty.TryGetValue("ItemName", out value);
            infoGroupsArea[4].transform.Find("Item").GetChild(0).Find("Image").GetComponent<UISprite>().atlas = _lastItem.GetComponentInChildren<UISprite>().atlas;
            infoGroupsArea[4].transform.Find("Item").GetChild(0).Find("Image").GetComponent<UISprite>().spriteName = value.ToString().Replace(" ", "") + "ICON";

            if (playerItemData.Count != 0)
            {
                playerItemData.TryGetValue("ItemCount", out value);
                infoGroupsArea[4].transform.Find("Item").GetChild(0).Find("Count").GetComponent<UILabel>().text = value.ToString();

            }
            else
            {
                infoGroupsArea[4].transform.Find("Item").GetChild(0).Find("Count").GetComponent<UILabel>().text = "0";
            }

            dictItemProperty.TryGetValue("Description", out value);
            infoGroupsArea[4].transform.Find("Item").GetChild(0).Find("Description").GetComponent<UILabel>().text = value.ToString(); 


            infoGroupsArea[4].transform.parent.gameObject.layer = LayerMask.NameToLayer("ItemInfo");
            infoGroupsArea[4].layer = LayerMask.NameToLayer("ItemInfo");
            EventMaskSwitch.Switch(infoGroupsArea[4], true);
        }
    }

    public void OnBuyClick(GameObject myPanel)
    {
        myPanel.SetActive(false);
        infoGroupsArea[5].SetActive(true);
        BuyWindowsInit();
        LoadBuyCountInfo(_lastItem, infoGroupsArea[5].transform);
        infoGroupsArea[5].layer = LayerMask.NameToLayer("ItemInfo");
        EventMaskSwitch.Switch(infoGroupsArea[5], true);
    }

    private void BuyWindowsInit()
    {
        Dictionary<string, object> dictItemProperty = Global.storeItem[_lastItem.name] as Dictionary<string, object>;
        object value;
        //string colunmsName = (_itemType == (int)MPProtocol.StoreType.Mice) ? "MiceID" : "ItemID";
        dictItemProperty.TryGetValue("CurrencyType", out value);


        dictItemProperty.TryGetValue("ItemID", out value);
        buyingGoodsData[0] = value.ToString();
        dictItemProperty.TryGetValue("ItemName", out value);
        buyingGoodsData[1] = value.ToString();
        dictItemProperty.TryGetValue("ItemType", out value);
        buyingGoodsData[2] = value.ToString();
        dictItemProperty.TryGetValue("CurrencyType", out value);
        buyingGoodsData[3] = value.ToString();
        buyingGoodsData[4] = "1";


        dictItemProperty.TryGetValue("ItemName", out value);
        infoGroupsArea[5].transform.GetChild(0).Find("Image").GetComponent<UISprite>().atlas = _lastItem.GetComponentInChildren<UISprite>().atlas;
        infoGroupsArea[5].transform.GetChild(0).Find("Image").GetComponent<UISprite>().spriteName = value.ToString().Replace(" ", "") + "ICON";


        infoGroupsArea[5].transform.GetChild(0).FindChild("Count").GetComponent<UILabel>().text = "1";  // count = 1
        dictItemProperty.TryGetValue("Price", out value);
        infoGroupsArea[5].transform.GetChild(0).FindChild("Sum").GetComponent<UILabel>().text = value.ToString(); // price
        infoGroupsArea[5].transform.GetChild(0).FindChild("Price").GetComponent<UILabel>().text = value.ToString(); // price


    }

    /// <summary>
    /// 改變道具數量
    /// </summary>
    /// <param name="obj"></param>
    public void OnQuantity(GameObject obj)
    {
        int price = int.Parse(infoGroupsArea[5].transform.GetChild(0).FindChild("Price").GetComponent<UILabel>().text);
        int sum = int.Parse(infoGroupsArea[5].transform.GetChild(0).FindChild("Sum").GetComponent<UILabel>().text);
        int count = int.Parse(infoGroupsArea[5].transform.GetChild(0).FindChild("Count").GetComponent<UILabel>().text);

        count += (obj.name == "Add") ? 1 : -1;
        count = (count < 0) ? 0 : count;
        buyingGoodsData[4] = infoGroupsArea[5].transform.GetChild(0).FindChild("Count").GetComponent<UILabel>().text = count.ToString();
        infoGroupsArea[5].transform.GetChild(0).FindChild("Sum").GetComponent<UILabel>().text = (price * count).ToString();
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

    public void OnClosed(GameObject obj)
    {
        EventMaskSwitch.lastPanel = null;
        GameObject root = obj.transform.parent.gameObject;

        //_tmpTab.SetActive(false);
        //infoGroupsArea[2].SetActive(true);
        //_tmpTab = infoGroupsArea[2];

        GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().LoadPanel(obj.transform.parent.gameObject);
        EventMaskSwitch.Resume();
    }

    public void OnReturn(GameObject obj)
    {
        int level = int.Parse(obj.name);
        EventMaskSwitch.openedPanel.SetActive(false);
        EventMaskSwitch.Prev(level);

    }

    public void OnTabClick(GameObject obj)
    {
        int value = int.Parse(obj.name.Remove(0, 3));

        switch (value)
        {
            //case 1:
            //    {
            //        _itemType = (int)StoreType.Gashapon;
            //        if (_tmpTab != infoGroupsArea[2]) _tmpTab.SetActive(false);
            //        infoGroupsArea[2].SetActive(true);
            //        _tmpTab = infoGroupsArea[2];
            //        break;
            //    }
            case 2:
                _itemType = (int)StoreType.Mice;
                break;
            case 3:
                _itemType = (int)StoreType.Item;
                break;
            case 4:
                _itemType = (int)StoreType.Armor;
                break;
            case 5:
                break;
            default:
                _itemType = (int)StoreType.Mice;
                Debug.Log("Show Default Panel");
                break;
        }


        if (_itemType != (int)StoreType.Gashapon)
        {
            SelectStoreItemData(_itemType);
            _itemData = ObjectFactory.GetItemInfoFromType(_itemData, _itemType);
//            if (_tmpTab != infoGroupsArea[3]) _tmpTab.SetActive(false);
            assetLoader.init();
            assetLoader.LoadAsset(_folderString + "/", _folderString);
            _bLoadedIcon = LoadIconObject(SelectIconData(_itemData), _folderString);
            assetLoader.LoadPrefab("Panel/", "Item");
            infoGroupsArea[3].SetActive(true);
            _tmpTab = infoGroupsArea[3];
        }



    }
    #endregion

    #region -- LoadGashapon 載入轉蛋物件(亂寫) --
    private void LoadGashapon(string folder)
    {
        for (int i = 1; i <= 3; i++)
            assetLoader.LoadPrefab(folder + "/", folder + i);
    }
    #endregion

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
        itemData = ObjectFactory.GetItemInfoFromType(itemData, itemType);

        // 載入資料
        foreach (KeyValuePair<string, object> item in itemData)
        {
            diceItemProperty = Global.storeItem[item.Key] as Dictionary<string, object>;
            diceItemProperty.TryGetValue("Price", out price);
            dictItemRefs[item.Key].transform.FindChild("Price").GetComponent<UILabel>().text = price.ToString();
        }
    }
    #endregion

    #region -- LoadBuyCountInfo 載入物件價格 --
    private void LoadBuyCountInfo(GameObject item, Transform parent)
    {
        //parent.GetChild(0).GetChild(1).GetComponent<UILabel>().text = item.GetComponent<Item>().itemProperty[(int)StoreProperty.ItemName];
        //parent.GetChild(0).GetChild(2).GetComponent<UILabel>().text = item.GetComponent<Item>().storeInfo[(int)StoreProperty.Price];
    }
    #endregion

    #region -- SelectIconData 選擇ICON物件資料 --
    /// <summary>
    /// 選擇ICON物件資料
    /// </summary>
    /// <param name="itemData">物件陣列</param>
    private Dictionary<string, object> SelectIconData(Dictionary<string, object> itemData)    // 載入遊戲物件
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

    #region -- InstantiateGashapon 實體化轉蛋物件(亂寫)--
    /// <summary>
    /// 實體化載入完成的轉蛋物件，利用資料夾名稱判斷必要實體物件
    /// </summary>
    /// <param name="myParent">實體化父系位置</param>
    /// <param name="folder">資料夾名稱</param>
    private void InstantiateGashapon(Transform myParent, string folder)
    {
        for (int i = 0; i < 3; i++)
        {
            if (assetLoader.GetAsset(folder + (i + 1).ToString()) != null)                  // 已載入資產時
            {
                GameObject bundle = assetLoader.GetAsset(folder + (i + 1).ToString());
                Transform parent = myParent.GetChild(1).GetChild(i).GetChild(0);

                insObj.Instantiate(bundle, parent, folder + (i + 1).ToString(), new Vector3(75, 100), Vector3.one, new Vector2(180, 180), -1);
            }
            else
            {
                Debug.LogError("Assetbundle reference not set to an instance.");
            }
        }
        _bLoadedGashapon = true;
    }
    #endregion

    //#region -- LoadItemData 載入道具資訊 --
    //private void LoadItemData(Dictionary<string, object> itemData, Transform parent, int itemType)
    //{
    //    int i = 0, j = 0;
    //    itemData = ObjectFactory.GetItemInfoFromID(itemData, "ItemID", _itemType); /// 這一定要有 但是 道具類別(有兩類)沒有ItemType可以分辨資料
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

    //    itemData = ObjectFactory.GetItemInfoFromType(Global.storeItem, _itemType);
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

    #region -- InstantiateItemIcon 實體化道具圖片--
    /// <summary>
    /// 實體化載入完成的遊戲物件，利用玩家JASON資料判斷必要實體物件
    /// </summary>
    /// <param name="dictionary">資料字典</param>
    /// <param name="myParent">實體化父系位置</param>
    private void InstantiateItemIcon(Dictionary<string, object> itemData, Transform myParent)
    {
        int i = 0;
        itemData = ObjectFactory.GetItemInfoFromType(itemData, _itemType);
        // to do check has icon object
        foreach (KeyValuePair<string, object> item in itemData)
        {
            object itemName;
            var nestedData = item.Value as Dictionary<string, object>;
            nestedData.TryGetValue("ItemName", out itemName);
            string bundleName = itemName.ToString() + "ICON";

            if (assetLoader.GetAsset(bundleName) != null)                  // 已載入資產時
            {
                GameObject bundle = assetLoader.GetAsset(bundleName);
                Transform imageParent = myParent.GetChild(i).GetChild(0);
                if (imageParent.childCount == 0)   // 如果沒有ICON才實體化
                {
                    insObj.Instantiate(bundle, imageParent, itemName.ToString(), new Vector3(0, -30), Vector3.one, new Vector2(140, 140), 400);
                }
            }
            else
            {
                Debug.LogError("Assetbundle reference not set to an instance.");
            }
            i++;
        }
        _bLoadedIcon = false;
    }
    #endregion

    private void SelectStoreItemData(int _itemType)
    {
        switch (_itemType)
        {
            case (int)StoreType.Mice:
                _folderString = assetFolder[_itemType];
                _itemData = Global.storeItem;
                break;
            case (int)StoreType.Item:
            case (int)StoreType.Armor:
                _folderString = assetFolder[_itemType];
                _itemData = Global.storeItem;
                break;
        }
    }

    private void SelectItemProperty(int _itemType)
    {
        switch (_itemType)
        {
            case (int)StoreType.Mice:
                _folderString = assetFolder[_itemType];
                _itemData = Global.miceProperty;
                break;
            case (int)StoreType.Item:
            case (int)StoreType.Armor:
                _folderString = assetFolder[_itemType];
                _itemData = Global.itemProperty;
                break;
        }
    }

    void OnDisable()
    {
        Global.photonService.LoadStoreDataEvent -= OnLoadPanel;
        Global.photonService.UpdateCurrencyEvent -= LoadPlayerInfo;
    }
}
