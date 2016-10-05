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
        loadingPanel.transform.GetChild(0).SendMessage("OnMessage");
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
                Global.photonService.LoadPlayerData(Global.Account);    // 錯誤 應該只載入TEAM
                Panel[_panelNo].SetActive(true);                        // 錯誤 如改為載入AB 要等載入後顯示
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
    public string[,] GetItemInfoFromType(string[,] itemData, int type)
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
                    a.Add(itemData[i, j]);
                }
                b.Add(a);
            }
        }

        itemData = new string[b.Count, b[0].Count];

        for (int i = 0; i < b.Count; i++)
        {
            List<string> c = b[i];

            for (int j = 0; j < c.Count; j++)
            {
                itemData[i, j] = c[j];
            }
        }
        return itemData;
    }
    #endregion

    #region -- GetItemNameFromID --
    public string GetItemNameFromID(string itemID, string[,] nameData)
    {
        for (int j = 0; j < nameData.GetLength(0); j++)
        {
            if (itemID == nameData[j, 0])
            {
                itemID = nameData[j, 1];
                break;
            }
        }
        return itemID;
    } 
    #endregion

    #region -- GetItemNameFromID --
    public string GetItemIDFromName(string itemName, string[,] itemData)
    {
        for (int j = 0; j < itemData.GetLength(0); j++)
        {
            if (itemName == itemData[j, 1])
            {
                itemName = itemData[j, 0];
                break;
            }
        }
        return itemName;
    }
    #endregion
    #region -- InstantiatePanel --
    void InstantiatePanel() //實體化Panel現在是正確的，有時間可以重新啟用 很多用編輯器拉進去的Panel都要修改到陣列
    {
        PanelState panelState = new PanelState();
        panelState.obj = (GameObject)Instantiate(AssetBundleManager.getAssetBundle("Panel/" + _panelName).mainAsset);
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
