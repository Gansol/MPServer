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
 * 20161102 v1.0.2  改變繼承至MPPanel
 * 20160711 v1.0.1  1次重構，獨立AssetLoader                            
 * 20160701 v1.0.0  0版完成 FUCK
 * ****************************************************************/
public class PanelManager : MPPanel
{
    #region 欄位
    public static GameObject _lastEmptyItemGroup;
    public GameObject[] Panel;                                      // 存放Panel位置

    private static Dictionary<string, PanelState> dictPanelRefs;    // Panel參考
    private static GameObject _lastPanel;                           // 暫存Panel
    private string _panelName;                                      // panel名稱
    private bool _loadedPanel;                                      // 載入的Panel
    private int _panelNo;                                           // Panel編號

    ObjectFactory insObj;

    public class PanelState                                         // 存放Pnael物件 狀態
    {
        public bool onOff;                                          // Panel開關
        public GameObject obj;                                      // Panel物件
    }
    #endregion

    void Awake()
    {
        assetLoader = gameObject.AddMissingComponent<AssetLoader>();
        insObj = new ObjectFactory();
        dictPanelRefs = new Dictionary<string, PanelState>();
    }

    void Start()
    {

    }

    void Update()
    {
        //if (!_loadedPanel)                                          // 除錯訊息
        //    if (!string.IsNullOrEmpty(assetLoader.ReturnMessage))
        //        Debug.Log("訊息：" + assetLoader.ReturnMessage);

        if (assetLoader.loadedObj && _loadedPanel)                 // 載入Panel完成時
        {
            _loadedPanel = !_loadedPanel;
            InstantiatePanel();
            PanelSwitch();
        }
    }


    #region -- InstantiatePanel 實體化Panel--
    private void InstantiatePanel() //實體化Panel現在是正確的，有時間可以重新啟用 很多用編輯器拉進去的Panel都要修改到陣列
    {
        PanelState panelState = new PanelState();
        GameObject bundle = assetLoader.GetAsset(_panelName);
        panelState.obj = insObj.Instantiate(bundle, Panel[_panelNo].transform, _panelName, Vector3.zero, Vector3.one, Vector3.zero, -1);
        panelState.obj = panelState.obj.transform.parent.gameObject;
        panelState.obj.layer = Panel[_panelNo].layer;
        dictPanelRefs.Add(_panelName, panelState);
    }
    #endregion

    #region -- LoadPanel 載入Panel(外部呼叫用) --
    /// <summary>
    /// 載入Panel
    /// </summary>
    /// <param name="panel">Panel</param>
    /// <param name="obj">物件自己</param>
    public void LoadPanel(GameObject panel)
    {
        _panelNo = 0;
        foreach (GameObject item in Panel)  // 取得Panel編號
        {
            if (item == panel)
                break;
            _panelNo++;
        }

        _panelName = Panel[_panelNo].name.Remove(Panel[_panelNo].name.Length - 7);   // 7 = xxx(Panel) > xxx

        if (!dictPanelRefs.ContainsKey(_panelName))         // 如果還沒載入Panel AB 載入AB
        {
            assetLoader.init();
            assetLoader.LoadAsset("Panel/", "PanelUI");
            assetLoader.LoadPrefab("Panel/", _panelName);
            _loadedPanel = true;
        }                                                   // 已載入AB 顯示Panel
        else
        {
            PanelSwitch();
        }
    }
    #endregion

    #region -- InstantiateItem 實體化背包物件背景--
    /// <summary>
    /// 實體化商店物件背景
    /// </summary>
    /// <param name="itemData">資料字典</param>
    /// <param name="itemPanel">實體化父系位置</param>
    /// <param name="itemType">道具類別</param>
    public Dictionary<string, GameObject> InstantiateItem(Dictionary<string, object> itemData, string itemName, int itemType, Transform itemPanel, Vector2 offset, int tableCount, int rowCount)
    {
        itemData = ObjectFactory.GetItemInfoFromType(itemData, itemType);     // 取得對應道具類別資料

        if (itemPanel.transform.childCount == 0)                // 如果還沒建立道具
        {
            _lastEmptyItemGroup = CreateEmptyObject(itemPanel, itemType);
            return InstantiateItem2(itemData, itemName, itemType, _lastEmptyItemGroup.transform, itemData.Count, offset, tableCount, rowCount);
        }                                                       // 已建立道具時
        else
        {
            if (itemPanel.FindChild(itemType.ToString()))       // 如果有對應道具類別
            {
                _lastEmptyItemGroup.SetActive(false);
                _lastEmptyItemGroup = itemPanel.FindChild(itemType.ToString()).gameObject;
                _lastEmptyItemGroup.SetActive(true);
            }                                                   // 如果沒有對應道具類別資料 建立道具
            else if (_lastEmptyItemGroup != itemPanel.FindChild(itemType.ToString()))
            {
                _lastEmptyItemGroup.SetActive(false);
                _lastEmptyItemGroup = CreateEmptyObject(itemPanel, itemType);
                return InstantiateItem2(itemData, itemName, itemType, _lastEmptyItemGroup.transform, itemData.Count, offset, tableCount, rowCount);
            }
        }

        return null;
    }

    private Dictionary<string, GameObject> InstantiateItem2(Dictionary<string, object> itemData, string itemName, int itemType, Transform parent, int itemCount, Vector2 offset, int tableCount, int rowCount)
    {
        Vector2 pos = new Vector2();
        Dictionary<string, GameObject> dictItem = new Dictionary<string, GameObject>();
        int count = parent.childCount, i = 0;

        ObjectFactory insObj = new ObjectFactory();

        foreach (KeyValuePair<string, object> item in itemData)
        {
            var nestedData = item.Value as Dictionary<string, object>;
            object itemID;
            nestedData.TryGetValue("ItemID", out itemID);

            if (assetLoader.GetAsset(itemName) != null)                  // 已載入資產時
            {
                pos = sortItemPos(tableCount, rowCount, offset, pos, count + i);
                GameObject bundle = assetLoader.GetAsset(itemName);

                bundle = insObj.Instantiate(bundle, parent, itemID.ToString(), new Vector3(pos.x, pos.y), Vector3.one, Vector2.zero, -1);
                if (bundle != null) dictItem.Add(itemID.ToString(), bundle);    // 存入道具資料索引
                pos.x += offset.x;
            }
            i++;
        }

        return dictItem;
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


    #region -- PanelSwitch Panel 開/關 --
    /// <summary>
    /// Panel 開/關狀態
    /// </summary>
    /// <param name="panelNo">Panel編號</param>
    private void PanelSwitch()
    {
        PanelState panelState = dictPanelRefs[_panelName];      // 取得目前Panel狀態

        if (panelState.obj != null)
        {
            if (!panelState.obj.activeSelf)                     // 如果Panel是關閉狀態
            {
                if (_lastPanel != null) _lastPanel.SetActive(false);
                panelState.obj.SetActive(true);
                panelState.obj.transform.GetChild(0).SendMessage("OnLoading");
                panelState.onOff = !panelState.onOff;
                _lastPanel = panelState.obj;

                EventMaskSwitch.Switch(panelState.obj, false);
                EventMaskSwitch.lastPanel = panelState.obj;
            }                                                   // 如果Panel已開啟
            else
            {
                EventMaskSwitch.Resume();
                panelState.obj.SetActive(false);
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

    #region -- 字典 檢查/取值 片段 --
    public bool bLoadedPanel(string panelName)
    {
        return dictPanelRefs.ContainsKey(panelName) ? true : false;
    }
    #endregion

    #region -- ExpectOutdataObject 移除非同步物件 --
    /// <summary>
    /// 移除非同步物件
    /// </summary>
    /// <param name="dicServerData">Server Data</param>
    /// <param name="dicClinetData">Client Data</param>
    /// <param name="dicLoadedObject">已載入物件</param>
    public void ExpectOutdataObject(Dictionary<string, object> dicServerData, Dictionary<string, object> dicClinetData, Dictionary<string, GameObject> dicLoadedObject)
    {
        // var delObject = new Dictionary<string, object>();

        if (dicClinetData.Count != 0)
        {
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
    }
    #endregion

    #region -- SelectNewData 獲得伺服器新增資料 --
    /// <summary>
    /// 獲得伺服器新增資料，排除重複
    /// </summary>
    /// <param name="dicServerData">Server Data</param>
    /// <param name="dicClinetData">Client Data</param>
    /// <returns></returns>
    public Dictionary<string, object> SelectNewData(Dictionary<string, object> dicServerData, Dictionary<string, object> dicClinetData)
    {
        var newObject = new Dictionary<string, object>();           // 新資料
        //var buffer = new Dictionary<string, object>();              // buffer

        foreach (KeyValuePair<string, object> item in dicServerData)
        {
            if (!dicClinetData.ContainsValue(item.Value))           // 如果在Clinet找不到Server資料 = 新資料           
            {
                newObject.Add(item.Key, item.Value);
            }                                                       // 如果在Clinet找不到Server資料 且 如果Clinet和Server Key值不相等
            else if (dicClinetData.ContainsValue(item.Value) && !dicClinetData.ContainsKey(item.Key))
            {
                //buffer = dicServerData;
                Debug.LogError("SelectNewData 還沒寫好 Error!");
            }
        }
        return newObject;
    }
    #endregion

    public override void OnLoading()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnLoadPanel()
    {
        throw new System.NotImplementedException();
    }
}
