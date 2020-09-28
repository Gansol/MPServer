using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
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
 * 20160701 v1.0.0  0版完成 
 * ****************************************************************/
public class PanelManager /*: IMPPanelUI*/
{
    //#region 欄位
    //public static GameObject[] PanelRefs;
    //private ScrollView scrollView;
    //public GameObject[] Panel;                                      // 存放Panel位置
    //public GameObject loginPanel;
    //private static Dictionary<string, PanelState> dictPanelRefs;    // Panel參考
    //private static GameObject _lastPanel;                           // 暫存Panel
    //private string _panelName;                                      // panel名稱
    //private bool _loadedPanel, _loginStatus, _bApplyMatchGameFriend;             // 載入的Panel 登入狀態
    //private int _panelNo;                                           // Panel編號

    //public class PanelState                                         // 存放Pnael物件 狀態
    //{
    //    public bool onOff;                                          // Panel開關
    //    public GameObject obj;                                      // Panel物件
    //}
    //#endregion

    //public PanelManager(MPGame MPGame)
    //    : base(MPGame)
    //{
    //    m_RootUI = GameObject.Find("MenuUI");
    //}

    //void Awake()
    //{
    //    EventMaskSwitch.Init();
    //    if (dictPanelRefs == null) dictPanelRefs = new Dictionary<string, PanelState>();


    //    scrollView = GameObject.FindGameObjectWithTag("GM").GetComponent<ScrollView>();
    //}

    //void OnEnable()
    //{
    //    EventMaskSwitch.Resume();
    //    //Global.photonService.LoginEvent += OnLogin;
    //    Global.photonService.ApplyMatchGameFriendEvent += OnApplyMatchGameFriend;
    //}

    //void OnDisable()
    //{
    //   // Global.photonService.LoginEvent -= OnLogin;
    //    Global.photonService.ApplyMatchGameFriendEvent -= OnApplyMatchGameFriend;
    //}

    //void Start()
    //{
    //}

    //public override void Update()
    //{
    //    if (m_MPGame.GetAssetLoader().bLoadedObj && _loadedPanel)                 // 載入Panel完成時
    //    {
    //        _loadedPanel = !_loadedPanel;

    //        InstantiatePanel();
    //        PanelSwitch();

    //        // 如果收到 好友配對事件 傳送配對事件
    //        if (_bApplyMatchGameFriend)
    //        {
    //            _lastPanel.transform.GetChild(0).SendMessage("OnApplyMatchGameFriend");
    //            _bApplyMatchGameFriend = false;
    //        }
    //    }
    //}

    //#region -- InstantiatePanel 實體化Panel--
    //protected override void InstantiatePanel() //實體化Panel現在是正確的，有時間可以重新啟用 很多用編輯器拉進去的Panel都要修改到陣列
    //{
    //    PanelState panelState = new PanelState();

    //    if (_bApplyMatchGameFriend)
    //    {
    //        int no = 0;
    //        foreach (GameObject go in Panel)
    //        {
    //            if (go.name == _panelName + "(Panel)")
    //                break;
    //            no++;
    //        }
    //        this._panelNo = no;
    //    }

    //    GameObject bundle = assetLoader.GetAsset(_panelName.ToLower());
    //    panelState.obj = MPGFactory.GetObjFactory().Instantiate(bundle, Panel[_panelNo].transform, _panelName, Vector3.zero, Vector3.one, Vector3.zero, -1);
    //    panelState.obj = panelState.obj.transform.parent.gameObject;
    //    panelState.obj.layer = Panel[_panelNo].layer;

    //    if (!dictPanelRefs.ContainsKey(_panelName))
    //        dictPanelRefs.Add(_panelName, panelState);
    //}
    //#endregion

    ////#region -- LoadPanel 載入Panel(外部呼叫用) --
    /////// <summary>
    /////// 載入Panel
    /////// </summary>
    /////// <param name="panel">Panel</param>
    /////// <param name="obj">物件自己</param>
    ////public void LoadPanel(GameObject panel)
    ////{
    ////    _panelNo = 0;

    ////    foreach (GameObject item in Panel)  // 取得Panel編號
    ////    {
    ////        if (item == panel)
    ////            break;
    ////        _panelNo++;
    ////    }
    ////    _panelName = Panel[_panelNo].name.Remove(Panel[_panelNo].name.Length - 7);   // 7 = xxx(Panel) > xxx

    ////    if (!dictPanelRefs.ContainsKey(_panelName))         // 如果還沒載入Panel AB 載入AB
    ////    {
            
    ////        assetLoader.LoadAssetFormManifest(Global.PanelUniquePath + _panelName.ToLower() + Global.ext);
    ////        _loadedPanel = true;
    ////    }
    ////    else
    ////    {
    ////        PanelSwitch();// 已載入AB 顯示Panel
    ////    }
    ////}
    ////#endregion


    //#region LoadPanel 亂寫的 給 叫出未實體的Panel使用
    //private void LoadPanelByName(string panelName)
    //{
    //    _panelName = panelName;

    //    if (!dictPanelRefs.ContainsKey(_panelName))         // 如果還沒載入Panel AB 載入AB
    //    {
            
    //        assetLoader.LoadAssetFormManifest(Global.PanelUniquePath + _panelName.ToLower() + Global.ext);
    //        _loadedPanel = true;
    //    }
    //    else
    //    {
    //        PanelSwitch();// 已載入AB 顯示Panel
    //    }
    //}
    //#endregion

    //#region -- PanelSwitch Panel 開/關 --
    ///// <summary>
    ///// Panel 開/關狀態
    ///// </summary>
    ///// <param name="panelNo">Panel編號</param>
    //private GameObject PanelSwitch()
    //{

    //    PanelState panelState = dictPanelRefs[_panelName];      // 取得目前Panel狀態

    //    if (panelState.obj != null && (_loginStatus || !Global.connStatus))
    //    {
    //        if (!panelState.obj.activeSelf)                     // 如果Panel是關閉狀態
    //        {
    //            scrollView.scroll = false;
    //            if (_lastPanel != null) _lastPanel.SetActive(false);
    //            Panel[5].SetActive(true);
    //            panelState.obj.SetActive(true);
    //            panelState.obj.transform.GetChild(0).SendMessage("OnLoading");
    //            panelState.onOff = !panelState.onOff;
    //            _lastPanel = panelState.obj;

    //            EventMaskSwitch.Switch(Panel[5]);
    //            EventMaskSwitch.lastPanel = Panel[5];

    //            return _lastPanel;
    //        }                                                   // 如果Panel已開啟
    //        else
    //        {
    //            scrollView.scroll = true;
    //            EventMaskSwitch.Resume();
    //            panelState.obj.SetActive(false);
    //            _lastPanel = panelState.obj;
    //            panelState.onOff = !panelState.onOff;
    //            Camera.main.GetComponent<UICamera>().eventReceiverMask = (int)Global.UILayer.Default;
    //            return _lastPanel;
    //        }
    //    }
    //    else
    //    {
    //        Debug.LogError("PanelNo unknow or not login !");
    //        return null;

    //    }
    //}
    //#endregion

    ////#region -- 字典 檢查/取值 片段 --
    ////private bool bLoadedPanel(string panelName)
    ////{
    ////    return dictPanelRefs.ContainsKey(panelName) ? true : false;
    ////}
    ////#endregion



    ////private void OnConnect(bool status)
    ////{
    ////    if(status)
    ////    {
    ////        loginPanel.SetActive(true);
    ////    }
    ////}

    ////private void OnLogin(bool loginStatus, string message, string returnCode)
    ////{
    ////    _loginStatus = loginStatus;
    ////}

    //protected override void OnLoading()
    //{
    //    throw new System.NotImplementedException();
    //}

    //protected override void OnLoadPanel()
    //{
    //    throw new System.NotImplementedException();
    //}

    //protected override void GetMustLoadAsset()
    //{
    //    throw new System.NotImplementedException();
    //}
    //// 同意配對 並開起配對視窗
    //private void OnApplyMatchGameFriend()
    //{
       
    //}

    //public override void OnClosed(GameObject obj)
    //{
    //    throw new System.NotImplementedException();
    //}

    //protected override int GetMustLoadedDataCount()
    //{
    //    throw new System.NotImplementedException();
    //}

    //public override void Initinal()
    //{
    //    throw new System.NotImplementedException();
    //}

    //public override void Release()
    //{
    //    throw new System.NotImplementedException();
    //}
}
