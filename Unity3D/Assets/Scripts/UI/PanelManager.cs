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
    AssetLoader assetLoader;                                // 資源載入組件
    public GameObject[] Panel;                              // 存放Panel位置
    private GameObject _clone;                              // 存放克隆物件
    private Dictionary<string, PanelState> dictPanelRefs;   // Panel參考
    
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
        assetLoader = gameObject.AddComponent<AssetLoader>();
        dictPanelRefs = new Dictionary<string, PanelState>();
        StartCoroutine(Test());
    }


    IEnumerator Test()
    {
        assetLoader.LoadAsset("TeamPanel/", "comic");
        yield return new WaitForSeconds(.5f);
        assetLoader.LoadAsset("TeamPanel/", "LiHeiPro");
        
    }

    void Update()
    {
        if (!_loadedPanel)                                          // 除錯訊息
            if (!string.IsNullOrEmpty(assetLoader.ReturnMessage))
                Debug.Log("訊息：" + assetLoader.ReturnMessage);

        if (assetLoader.loadedObj && !_loadedPanel)                 // 載入Panel完成時
        {
            _loadedPanel = !_loadedPanel;               
            InstantiatePanel();
            PanelSwitch(_panelNo);
            InitPanel(Panel[_panelNo]);
        }
    }

    #region -- LoadPanel 載入Panel(外部呼叫用) --
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
            assetLoader.init();
            assetLoader.LoadAsset(_panelName + "Panel/", _panelName);
            assetLoader.LoadPrefab(_panelName + "Panel/", _panelName);
        }
        else
        {
            PanelSwitch(_panelNo);
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
    void PanelSwitch(int panelNo)
    {
        PanelState panelState = dictPanelRefs[_panelName];
        switch (panelNo)
        {
            case 0:
                break;
            case 1:
                {
                    if (!panelState.onOff)
                    {
                        Global.photonService.LoadPlayerData(Global.Account);    // 錯誤 應該只載入TEAM
                        Panel[_panelNo].SetActive(true);                        // 錯誤 如改為載入AB 要等載入後顯示
                        panelState.onOff = !panelState.onOff;
                    }
                    else
                    {
                        Panel[_panelNo].SetActive(false);
                        panelState.onOff = !panelState.onOff;
                    }
                    break;
                }
            default:
                Debug.LogError("PanelNo is unknow!");
                break;
        }
    }
    #endregion

    #region -- 字典 檢查/取值 片段 --
    public bool bLoadedPanel(string panelName)
    {
        return dictPanelRefs.ContainsKey(panelName) ? true : false;
    }
    #endregion

    #region -- InstantiatePanel --
    void InstantiatePanel() //實體化Panel現在是正確的，有時間可以重新啟用 很多用編輯器拉進去的Panel都要修改到陣列
    {
        PanelState panelState = new PanelState();
        panelState.obj = (GameObject)Instantiate(AssetBundleManager.getAssetBundle(_panelName + "Panel/" + _panelName).mainAsset);
        panelState.obj.transform.parent = Panel[_panelNo].transform;
        panelState.obj.transform.localPosition = Vector3.zero;
        panelState.obj.transform.localScale = new Vector3(2, 2, 1);
        panelState.obj.name = _panelName;

        dictPanelRefs.Add(_panelName, new PanelState());
    }
    #endregion



}
