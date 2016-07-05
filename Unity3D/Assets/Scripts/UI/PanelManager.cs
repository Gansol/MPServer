using UnityEngine;
using System.Collections.Generic;
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
 * 之後有空改回載入Team.unity現在並沒有載入是直接放在場景 
 * 實體化Panel現在是正確的，有時間可以重新啟用 很多用編輯器拉進去的Panel都要修改到陣列
 * 用拉進來的Panel要砍掉，並加入已載入dictPanelRefs字典
 * ***************************************************************
 *                          ChangeLog
 * 20160701 v1.0.0  0版完成 
 * ****************************************************************/
public class PanelManager : MonoBehaviour
{
    #region 欄位
    AssetBundleManager assetBundleManager;

    public GameObject[] Panel;
    public string[] assetName;
    public bool _isLoadTeamData { get; set; }

    private Dictionary<string, GameObject> dictPanelRefs;
    private Dictionary<string, object> _dictObject = null;
    private GameObject _clone, _myParent, _loadingPanel;

    private string _name;
    private bool _LoadedPanel, _teamON, _bundleLoaded, _objetLoaded, _isCompleted, _isSent2Panel;
    private int _loadObjCount, _miceCount, _teamCount, _matCount, _objCount, _panelNo;
    #endregion

    void Awake()
    {
        assetBundleManager = new AssetBundleManager();
        dictPanelRefs = new Dictionary<string, GameObject>();
        _panelNo = -1;
    }

    void Update()
    {
        if (assetBundleManager != null)
            if (assetBundleManager.isStartLoadAsset && !_LoadedPanel)
                if (!string.IsNullOrEmpty(assetBundleManager.ReturnMessage))
                    Debug.Log("訊息：" + assetBundleManager.ReturnMessage);

        if (assetBundleManager.loadedABCount == _matCount && _matCount != 0 && !_LoadedPanel)
        {
            _LoadedPanel = true;
            assetBundleManager.loadedABCount = _matCount = 0;
            LoadObject(assetName[_panelNo], 1);
        }

        if (assetBundleManager.loadedObjectCount == _objCount && _objCount != 0 && _LoadedPanel)
        {
            // dictPanelRefs.Add(name, gameObject); 載入AB時啟用
            assetBundleManager.loadedObjectCount = _objCount = 0;
            InitPanel(Panel[_panelNo]);
        }
    }

    #region -- 載入資產片斷程式 --
    void LoadAsset(string assetName, int fileNum)
    {
        _matCount += fileNum * 3;
        assetBundleManager.init();
        StartCoroutine(assetBundleManager.LoadAtlas(assetName, typeof(Texture)));
        StartCoroutine(assetBundleManager.LoadAtlas(assetName, typeof(Material)));
        StartCoroutine(assetBundleManager.LoadAtlas(assetName, typeof(GameObject)));
    }

    void LoadObject(string assetName, int fileNum)
    {
        _objCount = fileNum;
        if (!assetBundleManager.bLoadedAssetbundle(assetName + "Panel/" + assetName))
            StartCoroutine(assetBundleManager.LoadGameObject(assetName, typeof(GameObject)));
    }
    #endregion

    #region -- InitPanel 開始載入Panel內容 --
    void InitPanel(GameObject loadingPanel)
    {
        loadingPanel.transform.GetChild(0).SendMessage("OnMessage");
        AssetBundleManager.UnloadUnusedAssets();
        _bundleLoaded = true;
        // InstantiatePanel(); // 實體化Panel現在是正確的，有時間可以重新啟用 很多用編輯器拉進去的Panel都要修改到陣列
    }
    #endregion
    
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

        _name = assetName[_panelNo];

        if (!_LoadedPanel)
            LoadAsset(assetName[_panelNo], 1);

        PanelSwitch(_panelNo);
    }
    #endregion

    #region -- PanelSwitch Panel 開/關 --
    /// <summary>
    /// Panel 開/關狀態
    /// </summary>
    /// <param name="panelNo">Panel編號</param>
    void PanelSwitch(int panelNo)
    {
        switch (panelNo)
        {
            case 0:
                break;
            case 1:
                {
                    if (!_teamON)
                    {
                        Global.photonService.LoadPlayerData(Global.Account);    // 錯誤 應該只載入TEAM
                        Panel[_panelNo].SetActive(true);                        // 錯誤 如改為載入AB 要等載入後顯示
                        _teamON = !_teamON;
                    }
                    else
                    {
                        Panel[_panelNo].SetActive(false);
                        _teamON = !_teamON;
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
    
    /*
    void InstantiatePanel() //實體化Panel現在是正確的，有時間可以重新啟用 很多用編輯器拉進去的Panel都要修改到陣列
    {
        GameObject go;
        go= (GameObject)Instantiate(AssetBundleManager.getAssetBundle("TeamPanel/Team").mainAsset);
        go.transform.parent = Panel[_panelNo].transform;
        go.transform.localPosition = new Vector3(-25, -54, 0);
        go.transform.localScale = new Vector3(2, 2, 1);
        go.name = "Team";
        Debug.Log("TEST");
    }
    */
}
