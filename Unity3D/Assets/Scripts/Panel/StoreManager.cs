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
public class StoreManager : MonoBehaviour
{
    public GameObject[] infoGroupsArea;
    public string[] assetFolder;
    public int itemOffsetX, itemOffsetY;
    public Vector3 actorScale;

    private AssetLoader assetLoader;

    private GameObject _clone, _btnClick, _tmpActor, _lastPanel, _lastItem;
    private static GameObject _tmpTab;
    private bool _LoadedGashapon, _LoadedMice, _LoadedActor;
    private Dictionary<string, object> _miceData;
    private Dictionary<string, GameObject> _dictActor;
    private int _itemType = -1;
    private string[,] itemData;
    /// <summary>
    /// 儲存購買商品資料 0:商品ID、1:類型、2:數量
    /// </summary>
    private string[] goods;

    void Awake()
    {
        actorScale = new Vector3(1.5f, 1.5f, 1);
        assetLoader = gameObject.AddComponent<AssetLoader>();
        _dictActor = new Dictionary<string, GameObject>();
        goods = new string[3];
        Global.photonService.UpdateCurrencyEvent += LoadPlayerInfo;

    }

    void Update()
    {
        if (!string.IsNullOrEmpty(assetLoader.ReturnMessage))
            Debug.Log("訊息：" + assetLoader.ReturnMessage);

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
            selectData();
            InstantiateItem(itemData, infoGroupsArea[3].transform, _itemType);
            LoadItemData(itemData, infoGroupsArea[3].transform);
            InstantiateMice(itemData, infoGroupsArea[3].transform, assetFolder[1]);
            LoadPrice(itemData, infoGroupsArea[3].transform);
        }

        if (assetLoader.loadedObj && !_LoadedActor)
        {
            _LoadedActor = !_LoadedActor;
            assetLoader.init();
            InstantiateActor(infoGroupsArea[4].transform.GetChild(0));
        }

    }

    void selectData()
    {
        switch (_itemType)
        {
            case (int)StoreType.Mice:
                itemData = Global.miceProperty;
                    break;
            case (int)StoreType.Item :
            case (int)StoreType.Armor:
                    itemData = Global.storeItem;
                    break;
        }
    }

    void OnMessage()
    {
        assetLoader.LoadAsset(assetFolder[0] + "/", assetFolder[0]);
        LoadGashapon(assetFolder[0]);
        LoadPlayerInfo();
        Global.photonService.LoadStoreData();
        EventMaskSwitch.lastPanel = gameObject;
    }

    private void LoadPlayerInfo()
    {
        infoGroupsArea[0].transform.GetChild(0).GetComponent<UILabel>().text = "20";
        infoGroupsArea[0].transform.GetChild(1).GetComponent<UILabel>().text = Global.Rice.ToString();
        infoGroupsArea[0].transform.GetChild(2).GetComponent<UILabel>().text = Global.Gold.ToString();
    }

    #region -- OnClick 按下事件 --
    public void OnGashaponClick(GameObject obj)
    {
        Debug.Log(obj.name);
    }

    public void OnItemClick(GameObject obj)
    {
        goods[0] = obj.name;
        goods[1] = obj.GetComponent<Item>().itemInfo[(int)StoreProperty.CurrencyType];
        Debug.Log(obj.name);
        _lastItem = obj;
        infoGroupsArea[4].SetActive(true);
        LoadProperty loadProperty = new LoadProperty();
        loadProperty.LoadMiceProperty(obj, infoGroupsArea[4].transform.GetChild(0).gameObject, 0);
        loadProperty.LoadPrice(obj, infoGroupsArea[4].transform.GetChild(0).gameObject, Global.miceProperty.GetLength(1));
        LoadActor(obj);
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
        goods[0] = _lastItem.GetComponent<Item>().itemProperty[(int)ItemProperty.MiceName];
        goods[1] = _lastItem.GetComponent<Item>().itemInfo[(int)StoreProperty.CurrencyType];
        goods[2] = "1";
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
        goods[2] = infoGroupsArea[5].transform.GetChild(0).GetChild(4).GetComponent<UILabel>().text = count.ToString();
        infoGroupsArea[5].transform.GetChild(0).GetChild(3).GetComponent<UILabel>().text = (price * count).ToString();
    }

    public void OnComfirm(GameObject myPanel)
    {
        myPanel.SetActive(false);
        Global.photonService.BuyItem(Global.Account, goods);
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
                    if (_tmpTab != infoGroupsArea[3]) _tmpTab.SetActive(false);
                    assetLoader.init();
                    assetLoader.LoadAsset(assetFolder[1] + "/", assetFolder[1]);
                    LoadIconObject(Global.miceProperty, assetFolder[1]);
                    assetLoader.LoadPrefab("Panel/", "Item");
                    _LoadedMice = false;
                    infoGroupsArea[3].SetActive(true);
                    _tmpTab = infoGroupsArea[3];
                    break;
                }
            case 3:
                {
                    _itemType = (int)StoreType.Item;
                    if (_tmpTab != infoGroupsArea[3]) _tmpTab.SetActive(false);
                    assetLoader.init();
                    assetLoader.LoadAsset(assetFolder[1] + "/", assetFolder[1]);
                    LoadIconObject(Global.miceProperty, assetFolder[1]);
                    assetLoader.LoadPrefab("Panel/", "Item");
                    _LoadedMice = false;
                    infoGroupsArea[3].SetActive(true);
                    _tmpTab = infoGroupsArea[3];
                    break;
                }
            case 4:
                {
                    _itemType = (int)StoreType.Armor;
                    if (_tmpTab != infoGroupsArea[3]) _tmpTab.SetActive(false);
                    assetLoader.init();
                    assetLoader.LoadAsset(assetFolder[1] + "/", assetFolder[1]);
                    LoadIconObject(Global.miceProperty, assetFolder[1]);
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


    #region -- LoadGashapon 載入轉蛋物件 --
    void LoadGashapon(string folder)
    {
        for (int i = 1; i <= 3; i++)
            assetLoader.LoadPrefab(folder + "/", folder + i);
    }
    #endregion

    #region -- LoadIconObject 載入老鼠物件 -- //修改
    void LoadIconObject(string[,] miceData, string folder)    // 載入遊戲物件
    {
        for (int i = 0; i < miceData.GetLength(0); i++)
        {
            assetLoader.LoadPrefab(folder + "/", miceData[i, 1].ToString().Remove(miceData[i, 1].ToString().Length - 4) + "ICON");
        }
    }
    #endregion

    #region -- LoadPrice 載入物件價格 --
    void LoadPrice(string[,] miceData, Transform parent)
    {
        for (int i = 0; i < miceData.GetLength(0); i++)
        {
            parent.GetChild(i).GetComponentInChildren<UILabel>().text = parent.GetChild(i).GetComponent<Item>().itemInfo[(int)StoreProperty.Price];
        }
    }
    #endregion

    #region -- LoadBuyCountInfo 載入物件價格 --
    void LoadBuyCountInfo(GameObject item, Transform parent)
    {
        parent.GetChild(0).GetChild(1).GetComponent<UILabel>().text = item.GetComponent<Item>().itemInfo[(int)StoreProperty.ItemName];
        parent.GetChild(0).GetChild(2).GetComponent<UILabel>().text = item.GetComponent<Item>().itemInfo[(int)StoreProperty.Price];
    }
    #endregion

    #region -- LoadActor 載入老鼠角色 --
    private void LoadActor(GameObject btn_mice)
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
            if (assetLoader.GetAsset(miceName + "/", miceName) != null)
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
    #endregion

    #region -- InstantiateActor 實體化老鼠角色 --
    private void InstantiateActor(Transform parent)
    {
        GameObject _tmp;
        InstantiateObject insObj = new InstantiateObject();
        string miceName = _btnClick.transform.GetComponentInChildren<UISprite>().name;
        GameObject bundle = (GameObject)assetLoader.GetAsset(miceName + "/", miceName);

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
            if (assetLoader.GetAsset(folder + "/", folder + (i + 1).ToString()))                  // 已載入資產時
            {
                GameObject bundle = assetLoader.GetAsset(folder + "/", folder + (i + 1).ToString());
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
    /// 實體化載入完成的遊戲物件，利用玩家JASON資料判斷必要實體物件
    /// </summary>
    /// <param name="dictionary">資料字典</param>
    /// <param name="myParent">實體化父系位置</param>
    void InstantiateItem(string[,] itemData, Transform parent, int itemType)
    {
        if (parent.transform.childCount == 0) InstantiateItem2(itemData, parent, itemData.GetLength(0), itemType);

        if (parent.transform.childCount < itemData.GetLength(0))
        {
            // 如果 Item數量小於道具資料數量 增加Item  6 < 9
            InstantiateItem2(itemData, parent, itemData.GetLength(0) - parent.transform.childCount, itemType);
        }
        else if (parent.transform.childCount > itemData.GetLength(0))
        {
            // 如果 Item數量大於道具資料數量 刪除多餘Item  9-3 > 6
            for (int i = parent.transform.childCount; i > itemData.GetLength(0); i--) // 刪除至數量相等
            {
                Destroy(parent.GetChild(i));
            }
        }
        else
        {
            // 如果數量相等
        }
    }




    void InstantiateItem2(string[,] itemData, Transform parent, int itemCount, int itemType)
    {
        Vector2 pos = new Vector2();
        string itemName = "Item", folderPath = "Panel/";

        for (int i = 0; i < itemCount; i++)
        {
            if (assetLoader.GetAsset(folderPath, itemName))                  // 已載入資產時
            {
                pos = sortItemPos(9, 3, pos, i);
                GameObject bundle = assetLoader.GetAsset(folderPath, itemName);
                InstantiateObject insObj = new InstantiateObject();

                _clone = insObj.Instantiate(bundle, parent, itemData[i, 1], new Vector3(pos.x, pos.y), Vector3.one, Vector2.zero, -1);
                pos.x += itemOffsetX;
            }
        }
    }
    #endregion


    private void LoadItemData(string[,] itemData, Transform parent)
    {
        for (int i = 0; i < itemData.GetLength(0); i++) // 載入商品資料
        {
            for (int j = 0; j < itemData.GetLength(1); j++) // 載入商品資料
            {
                parent.GetChild(i).GetComponent<Item>().itemProperty[j] = itemData[i, j];
                //Debug.Log("ITEM DATA " + i.ToString() + " :  " + itemData[i, j]);
            }
        }

        itemData = GetItemInfoFromType(Global.storeItem, _itemType);
        Debug.Log("FFF" + itemData.GetLength(0));
        for (int i = 0; i < itemData.GetLength(0); i++) // 載入商品資料
        {
            for (int j = 0; j < itemData.GetLength(1); j++) // 載入商品資料
            {
                parent.GetChild(i).GetComponent<Item>().itemInfo[j] = itemData[i, j];
                //Debug.Log("ITEM DATA " + i.ToString() + " :  " + itemData[i, j]);
            }
        }

        /*
        for (int i = 0; i < itemData.GetLength(1); i++) // 載入商品資料
        {
            for (int j = 0; j < itemData.GetLength(1); j++) // 載入商品資料
            {
                _clone.GetComponent<Item>().itemProperty[j] = itemData[i, j];
                //Debug.Log("ITEM DATA " + i.ToString() + " :  " + itemData[i, j]);
            }
        }
         * */
    }


    #region -- InstantiateMice 實體化老鼠物件--
    /// <summary>
    /// 實體化載入完成的遊戲物件，利用玩家JASON資料判斷必要實體物件
    /// </summary>
    /// <param name="dictionary">資料字典</param>
    /// <param name="myParent">實體化父系位置</param>
    void InstantiateMice(string[,] miceData, Transform myParent, string folder)
    {
        // to do check has icon object
        for (int i = 0; i < miceData.GetLength(0); i++)
        {
            string bundleName = miceData[i, 1].Remove(miceData[i, 1].Length - 4) + "ICON";

            if (assetLoader.GetAsset(folder + "/", bundleName))                  // 已載入資產時
            {
                GameObject bundle = assetLoader.GetAsset(folder + "/", bundleName);
                Transform parent = myParent.GetChild(i).GetChild(0);

                InstantiateObject insObj = new InstantiateObject();
                insObj.Instantiate(bundle, parent, miceData[i, 1], Vector3.zero, Vector3.one, new Vector2(150, 150), 310);
            }
            else
            {
                Debug.LogError("Assetbundle reference not set to an instance.");
            }
        }
        _LoadedMice = true;
    }
    #endregion



    #region -- sortItemPos 排序道具位置  --
    /// <summary>
    /// 排序道具位置
    /// </summary>
    /// <param name="xCount">第一頁最大數量</param>
    /// <param name="yCount">每行道具數量</param>
    /// <param name="pos">目前物件位置</param>
    /// <param name="counter">計數</param>
    /// <returns></returns>
    private Vector2 sortItemPos(int xCount, int yCount, Vector2 pos, int counter)
    {
        // 物件位置排序
        if (counter % xCount == 0 && counter != 0) // 3 % 9 =0
        {
            pos.x = itemOffsetX * 3;
            pos.y = 0;
        }
        else if (counter % yCount == 0 && counter != 0)//3 3 =0
        {
            pos.y += itemOffsetY;
            pos.x = 0;
        }
        return pos;
    }
    #endregion


    string[,] GetItemInfoFromType(string[,] itemData, int type)
    {
        List<List<string>> b = new List<List<string>>();
        List<string> a;

        for (int i = 0; i < itemData.GetLength(0); i++)
        {
            string itemType = itemData[i, 0].Remove(1, itemData[i, 0].Length - 1); // 商品ID第一個字元為類別
            if (itemType == type.ToString())
            {
                a = new List<string>();
                for (int j = 0; j < itemData.GetLength(1); j++)
                {
                    a.Add(Global.storeItem[i, j]);
                }
                b.Add(a);
            }
        }

        string[,] data = new string[b.Count, b[0].Count];

        for (int i = 0; i < b.Count; i++)
        {
            List<string> c = b[i];

            for (int j = 0; j < c.Count; j++)
            {
                data[i, j] = c[j];
            }
        }
        return data;
    }
}
