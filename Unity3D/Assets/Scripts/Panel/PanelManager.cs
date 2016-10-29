using UnityEngine;
using System.Collections.Generic;
using System.Collections;
/* ***************************************************************
 * -----Copyright © 2015 Gansol Studio.  All Rights Reserved.-----
 * -----------            CC BY-NC-SA 4.0            -------------
 * -----------  @Website:  EasyUnity@blogspot.com    -------------
 * -----------  @Email:    GansolTW@gmail.com        -------------
 * -----------  @Author:   Krola.                    -------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * 負責Panel切換
 * 實體化Panel現在是正確的，有時間可以重新啟用 很多用編輯器拉進去的Panel都要修改到陣列，並加入已載入dictPanelRefs字典
 * 目前載入字體是測試
 * ***************************************************************
 *                          ChangeLog
 * 20160711 v1.0.1  1次重構，獨立AssetLoader                            
 * 20160701 v1.0.0  0版完成 
 * ****************************************************************/
public class PanelManager : MonoBehaviour
{
    #region 欄位
    AssetLoader _assetLoader;                                // 資源載入組件
    public string folder;                                   // 資料夾路徑
    public GameObject[] Panel;                              // 存放Panel位置

    public GameObject _clone;                  // 存放克隆物件、已開起的Pnael
    private static Dictionary<string, PanelState> dictPanelRefs;   // Panel參考

    private string _panelName;                              // panel名稱
    private bool _loadedPanel;                              // 載入的Panel
    private int _panelNo;                                   // Panel編號

    class PanelState                                        // 存放Pnael
    {
        public bool onOff;                                  // Panel開關
        public GameObject obj;                              // Panel物件
    }
    #endregion

    void Awake()
    {
        _assetLoader = gameObject.AddComponent<AssetLoader>();
        dictPanelRefs = new Dictionary<string, PanelState>();
        StartCoroutine(Test());
    }

    IEnumerator Test()
    {
        _assetLoader.LoadAsset("Panel/", "ComicFont");
        yield return new WaitForSeconds(1f);
        _assetLoader.LoadAsset("Panel/", "LiHeiProFont");
    }

    void Update()
    {
        if (!_loadedPanel)                                          // 除錯訊息
            if (!string.IsNullOrEmpty(_assetLoader.ReturnMessage))
                Debug.Log("訊息：" + _assetLoader.ReturnMessage);

        if (_assetLoader.loadedObj && !_loadedPanel)                 // 載入Panel完成時
        {
            _loadedPanel = !_loadedPanel;
            InstantiatePanel();
            PanelSwitch();
            InitPanel(Panel[_panelNo]);
        }
    }

    #region -- LoadPanel 載入Panel(外部呼叫用) --
    /// <summary>
    /// 載入Panel
    /// </summary>
    /// <param name="panel">Panel</param>
    /// <param name="obj">物件自己</param>
    public void LoadPanel(GameObject panel)
    {
        _panelNo = 0;
        foreach (GameObject item in Panel)
        {
            if (item == panel)
                break;
            _panelNo++;
        }

        _panelName = Panel[_panelNo].name.Remove(Panel[_panelNo].name.Length - 7);   // 7 = xxx(Panel) > xxx

        if (!dictPanelRefs.ContainsKey(_panelName))
        {
            _loadedPanel = false;
            _assetLoader.init();

            _assetLoader.LoadAsset("Panel/", "Panel");
            _assetLoader.LoadPrefab("Panel/", _panelName);

        }
        else
        {
            PanelSwitch();
        }
    }
    #endregion

    #region -- InitPanel 開始載入Panel內容 --
    void InitPanel(GameObject loadingPanel)
    {
        //loadingPanel.transform.GetChild(0).SendMessage("OnLoadPanel");
        AssetBundleManager.UnloadUnusedAssets();
        // InstantiatePanel(); // 實體化Panel現在是正確的，有時間可以重新啟用 很多用編輯器拉進去的Panel都要修改到陣列
    }
    #endregion

    #region -- PanelSwitch Panel 開/關 --
    /// <summary>
    /// Panel 開/關狀態
    /// </summary>
    /// <param name="panelNo">Panel編號</param>
    void PanelSwitch()
    {
        if (Panel[_panelNo] != null)
        {
            PanelState panelState = dictPanelRefs[_panelName];
            if (!Panel[_panelNo].activeSelf)  // if closed
            {
                Panel[_panelNo].SetActive(true);
                Panel[_panelNo].transform.GetChild(0).SendMessage("OnLoading");
                panelState.onOff = !panelState.onOff;
                EventMaskSwitch.Switch(Panel[_panelNo]);
                EventMaskSwitch.lastPanel = Panel[_panelNo];
            }
            else
            {
                Panel[_panelNo].SetActive(false);
                panelState.onOff = !panelState.onOff;
                Camera.main.GetComponent<UICamera>().eventReceiverMask = (int)Global.UILayer.Default;
            }
        }
        else
        {
            Debug.LogError("PanelNo is unknow!");
        }
    }
    #endregion

    #region ExpectOutdataObjectByValue 移除已載入且不存在資料庫中物件
    /// <summary>
    /// 移除已載入且不存在資料庫中物件
    /// </summary>
    /// <param name="dictLoadedObject">舊的已載入物件資料</param>
    /// <param name="dictServerData">新的Server資料</param>
    public void ExpectOutdataObjectByValue(Dictionary<string, GameObject> dictLoadedObject, Dictionary<string, object> dictServerData)
    {
        var buffer = new Dictionary<string, GameObject>(dictLoadedObject);
        var bufferRefs = new Dictionary<int, string>();

        int i = 0;

        foreach (KeyValuePair<string, GameObject> item in buffer)
        {
            bufferRefs.Add(i, item.Key);
            i++;
        }

        i = 0;
        foreach (KeyValuePair<string, GameObject> item in buffer)
        {
            if (!dictServerData.ContainsValue(item.Key))
            {
                for (int j = 0; j < buffer.Count - i; j++)
                {
                    if (j + 1 == buffer.Count - i) // j+1 == next value
                    {
                        dictLoadedObject[bufferRefs[j + i]].GetComponentInChildren<UISprite>().spriteName = null;
                        dictLoadedObject[bufferRefs[j + i]].SendMessage("DisableBtn");
                    }
                    else
                    {
                        dictLoadedObject[bufferRefs[j + i]].GetComponentInChildren<UISprite>().spriteName = dictLoadedObject[bufferRefs[j + i + 1]].GetComponentInChildren<UISprite>().spriteName;
                    }
                }
                dictLoadedObject.Remove(item.Key);
            }
            i++;
        }
    }
    #endregion


    #region ExpectOutdataObject 移除已載入且不存在資料庫中物件
    /// <summary>
    /// 移除已載入且不存在資料庫中物件
    /// </summary>
    /// <param name="dictLoadedObject">舊的已載入物件資料</param>
    /// <param name="dictServerData">新的Server資料</param>
    public void ExpectOutdataObjectByKey(Dictionary<string, GameObject> dictLoadedObject, Dictionary<string, object> dictServerData)
    {
        var buffer = new Dictionary<string, GameObject>(dictLoadedObject);
        var bufferRefs = new Dictionary<int, string>();

        int i = 0;

        foreach (KeyValuePair<string, GameObject> item in buffer)
        {
            bufferRefs.Add(i, item.Key);
            i++;
        }

        i = 0;
        foreach (KeyValuePair<string, GameObject> item in buffer)
        {
            if (!dictServerData.ContainsKey(item.Key))
            {
                for (int j = 0; j < buffer.Count - i; j++)
                {
                    if (j + 1 == buffer.Count - i) // j+1 == next value
                    {
                        dictLoadedObject[bufferRefs[j + i]].GetComponentInChildren<UISprite>().spriteName = null;
                        dictLoadedObject[bufferRefs[j + i]].SendMessage("DisableBtn");
                    }
                    else
                    {
                        dictLoadedObject[bufferRefs[j + i]].GetComponentInChildren<UISprite>().spriteName = dictLoadedObject[bufferRefs[j + i + 1]].GetComponentInChildren<UISprite>().spriteName;
                    }
                }
                dictLoadedObject.Remove(item.Key);
            }
            i++;
        }
    }
    #endregion

    #region ExpectDuplicateObject
    /// <summary>
    /// 移除重複的物件值
    /// </summary>
    /// <param name="dictServerData">新的Server資料</param>
    /// <param name="dictLoadedObject">舊的已載入物件資料</param>
    /// <returns>不重複的字典資料</returns>
    public Dictionary<string, object> ExpectDuplicateObject(Dictionary<string, object> dictServerData, Dictionary<string, GameObject> dictLoadedObject)
    {
        foreach (KeyValuePair<string, GameObject> item in dictLoadedObject)
        {
            var buffer = new Dictionary<string, object>(dictServerData);

            if (buffer.ContainsValue(item.Key))
            {
                //var key = dictServerData.FirstOrDefault(x => x.Value == objName).Key;
                foreach (KeyValuePair<string, object> serverItem in buffer)
                {
                    if (serverItem.Value.ToString() == item.Key)
                        dictServerData.Remove(serverItem.Key);
                }
            }
        }
        return dictServerData;
    }
    #endregion



    public void fuckingDel(Dictionary<string, object> dicServerData, Dictionary<string, object> dicClinetData, Dictionary<string, GameObject> dicLoadedObject)
    {
        var delObject = new Dictionary<string, object>();
        foreach (KeyValuePair<string, object> item in dicClinetData)
        {
            if (dicServerData.ContainsValue(item.Value)) // 如果Server有Client的物件
            {
                if (!dicServerData.ContainsKey(item.Key)) // 如果Server的KEY 和 Client的KEY 不同 移除舊的物件
                {
                    dicLoadedObject[item.Value.ToString()].GetComponentInChildren<UISprite>().spriteName = null;
                    dicLoadedObject.Remove(item.Value.ToString());
                }
            }
            else if (dicLoadedObject.ContainsKey(item.Value.ToString())) // 如果載入的物件
            {
                Debug.Log("BUG");
                dicLoadedObject[item.Value.ToString()].GetComponentInChildren<UISprite>().spriteName = null;
                dicLoadedObject.Remove(item.Value.ToString());
            }
        }
    }


    public Dictionary<string, object> fuckingNew(Dictionary<string, object> dicServerData, Dictionary<string, object> dicClinetData)
    {
        var newObject = new Dictionary<string, object>();
        var buffer = new Dictionary<string, object>();

        foreach (KeyValuePair<string, object> item in dicServerData)
        {
            if (!dicClinetData.ContainsValue(item.Value))
                newObject.Add(item.Key, item.Value);
            else if (dicClinetData.ContainsValue(item.Value) && !dicClinetData.ContainsKey(item.Key))
            {
                buffer = dicServerData;

            }
        }

        return newObject;
    }

    #region -- 字典 檢查/取值 片段 --
    public bool bLoadedPanel(string panelName)
    {
        return dictPanelRefs.ContainsKey(panelName) ? true : false;
    }
    #endregion

    #region -- CreateEmptyGroup 建立空物件群組 --
    /// <summary>
    /// 建立空物件群組
    /// </summary>
    /// <param name="parent">上層物件</param>
    /// <param name="itemType">群組類型(名稱)</param>
    /// <returns></returns>
    public GameObject CreateEmptyGroup(Transform parent, int itemType)
    {
        GameObject emptyGroup = new GameObject(itemType.ToString());   // 商品物件空群組
        emptyGroup.transform.parent = parent;
        emptyGroup.layer = parent.gameObject.layer;
        emptyGroup.transform.localPosition = Vector3.zero;
        emptyGroup.transform.localScale = Vector3.one;
        return emptyGroup;
    }
    #endregion

    #region -- SortItemPos 排序道具位置  --
    /// <summary>
    /// 排序道具位置
    /// </summary>
    /// <param name="xCount">第一頁最大數量</param>
    /// <param name="yCount">每行道具數量</param>
    /// <param name="offset">目前物件位置</param>
    /// <param name="pos">初始位置</param>
    /// <param name="counter">計數</param>
    /// <returns>物件位置</returns>
    public Vector2 sortItemPos(int xCount, int yCount, Vector2 offset, Vector2 pos, int counter)
    {
        // 物件位置排序
        if (counter % xCount == 0 && counter != 0) // 3 % 9 =0
        {
            pos.x = offset.x * 3;
            pos.y = 0;
        }
        else if (counter % yCount == 0 && counter != 0)//3 3 =0
        {
            pos.y += offset.y;
            pos.x = 0;
        }
        return pos;
    }
    #endregion

    #region -- GetItemInfoFromType --
    public Dictionary<string, object> GetItemInfoFromType(Dictionary<string, object> itemData, int type)
    {
        Dictionary<string, object> data = new Dictionary<string, object>();

        foreach (KeyValuePair<string, object> item in itemData)
        {
            var nestedData = item.Value as Dictionary<string, object>;
            object itemType;
            nestedData.TryGetValue("ItemType", out itemType);
            if (itemType != null)
                if (itemType.ToString() == type.ToString())
                {
                    data.Add(nestedData["ItemID"].ToString(), nestedData);
                }
        }
        return data;







        //List<List<string>> b = new List<List<string>>();
        //List<string> a;
        ////        Debug.Log(itemData[0, 0]);
        //for (int i = 0; i < itemData.GetLength(0); i++)
        //{
        //    string itemType = itemData[i, 0].Remove(1, itemData[i, 0].Length - 1); // 商品ID第一個字元為類別
        //    //            Debug.Log(itemType);
        //    if (itemType == type.ToString())
        //    {
        //        a = new List<string>();
        //        for (int j = 0; j < itemData.GetLength(1); j++)
        //        {
        //            a.Add(itemData[i, j]);
        //        }
        //        b.Add(a);
        //    }
        //}
        ////        Debug.Log(itemData.GetLength(0) + "  " + b.Count);
        //itemData = new string[b.Count, b[0].Count];

        //for (int i = 0; i < b.Count; i++)
        //{
        //    List<string> c = b[i];

        //    for (int j = 0; j < c.Count; j++)
        //    {
        //        itemData[i, j] = c[j];
        //    }
        //}
        //return itemData;
    }
    #endregion

    /// <summary>
    /// 從道具ID取得道具名稱
    /// </summary>
    /// <param name="itemName">道具名稱</param>
    /// <param name="itemData">2d Dictionary</param>
    /// <returns>itemName</returns>
    #region -- GetItemNameFromID --
    public string GetItemNameFromID(string itemID, Dictionary<string, object> itemData)
    {
        object objNested;

        itemData.TryGetValue(itemID.ToString(), out objNested);
        if (objNested != null)
        {
            var dictNested = objNested as Dictionary<string, object>;
            return dictNested["ItemName"].ToString();
        }
        return "";
    }
    #endregion

    /// <summary>
    /// 從道具名稱取得道具ID
    /// </summary>
    /// <param name="itemName">道具名稱</param>
    /// <param name="itemData">2d Dictionary</param>
    /// <returns>itemName</returns>
    #region -- GetItemNameFromID --
    public string GetItemIDFromName(string itemName, Dictionary<string, object> itemData)
    {
        object value;
        foreach (KeyValuePair<string, object> item in itemData)
        {
            var nestedData = item.Value as Dictionary<string, object>;
            nestedData.TryGetValue("ItemName", out value);
            if (itemName == value.ToString())
            {
                nestedData.TryGetValue("ItemID", out value);
                return value.ToString();
            }
        }
        return "";
    }
    #endregion

    #region -- InstantiatePanel --
    void InstantiatePanel() //實體化Panel現在是正確的，有時間可以重新啟用 很多用編輯器拉進去的Panel都要修改到陣列
    {
        PanelState panelState = new PanelState();
        panelState.obj = (GameObject)Instantiate(_assetLoader.GetAsset(_panelName));
        panelState.obj.transform.parent = Panel[_panelNo].transform;
        panelState.obj.transform.localPosition = Vector3.zero;
        panelState.obj.transform.localScale = Vector3.one;
        panelState.obj.name = _panelName;
        panelState.obj.layer = Panel[_panelNo].layer;
        dictPanelRefs.Add(_panelName, new PanelState());

        EventMaskSwitch.Switch(panelState.obj);
    }
    #endregion
}
