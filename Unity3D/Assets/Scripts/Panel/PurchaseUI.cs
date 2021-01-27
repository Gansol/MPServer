using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using MPProtocol;
//using Sdkbox;
/* ***************************************************************
 * -----Copyright © 2015 Gansol Studio.  All Rights Reserved.-----
 * -----------            CC BY-NC-SA 4.0            -------------
 * -----------  @Website:  EasyUnity@blogspot.com    -------------
 * -----------  @Email:    GansolTW@gmail.com        -------------
 * -----------  @Author:   Krola.                    -------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * 負責 負責所有法幣商品交易的 所有處理
 * 
 * 
 * ***************************************************************
 *                           ChangeLog
 * 20201027 v3.0.0  繼承重構
 * 20180101 v1.0.0   完成功能、註解             
 * ****************************************************************/
// 可以把PurchaseHandlder寫到這裡
public class PurchaseUI : IMPPanelUI
{
    #region Variables 變數
    private AttachBtn_PurchaseUI UI;
    private Dictionary<string, GameObject> _dictitemSlot;               // 已實體化的商品
    private Dictionary<string, object> _dictProductsFitCurrency;    // 法幣道具價格
    private string _currencyCode;     // 貨幣代號
    private int _offsetY;                         // 道具Y軸偏移量
    private int _slotPosY;                      // 道具Y軸位置
    private int _dataLoadedCount;   //  資料載入量                                       
    private bool _bFirstLoad;              // 是否第一次載入
    private bool _bLoadPanel;             // 是否載入Panel
    private bool _bLoadAsset;            // 是否載入資產

    // private bool  _bIABInit /*,_bLoadProduct */;
    //private Sdkbox.IAP _iap;                                        // IAP資料
    //private Sdkbox.Product[] _product;                              // 商品資料
    #endregion

    public class PurchaseProperty
    {
        public const string ItemName = "ItemName";
        public const string Price = "Price";
        public const string FitCurrency = "FitPrice";
        public const string PromotionsCount = "PromotionsCount";
        public const string NewArrivalsTime = "NewArrivalsTime";
        public const string PromotionsTime = "PromotionsTime";
        public const string PromotionsLimit = "PromotionsTime";
        public const string PromotionsImage = "PromotionsImage";
        public const string Promotions = "Promotions";
        public const string LimitCount = "LimitCount";
        public const string BuyCount = "BuyCount";
        public const string OnSell = "OnSell";
    }

    public PurchaseUI(MPGame MPGame) : base(MPGame)
    {
        Debug.Log("--------------- PurchaseUI Create ----------------");
        _dictitemSlot = new Dictionary<string, GameObject>();
        _dictProductsFitCurrency = new Dictionary<string, object>();
        _bFirstLoad = true;
        _currencyCode = "TWD";
        _offsetY = 275;

        //_iap = FindObjectOfType<Sdkbox.IAP>();
        //  _bLoadProduct = true;
        //_iap.getProducts();
    }

    public override void Initialize()
    {
        Debug.Log("--------------- PurchaseUI Initialize ----------------");

        _slotPosY = 0;
        _dataLoadedCount = 0;

        _bLoadPanel = false;
        _bLoadAsset = false;

        Global.photonService.LoadCurrencyEvent += OnLoadCurrency;
        Global.photonService.LoadPurchaseEvent += OnLoadPurchase;
        Global.photonService.UpdateCurrencyEvent += OnUpdateCurrency;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        //if (!string.IsNullOrEmpty(assetLoader.ReturnMessage))
        //    Debug.Log("訊息：" + assetLoader.ReturnMessage);

        #region // 資料載入完成後 載入Panel
        if (_dataLoadedCount == GetMustLoadedDataCount() /*&& _bLoadProduct */&& !_bLoadPanel)
        {
            _bLoadPanel = true;
            OnLoadPanel();
        }
        #endregion

        #region   // Panel載入完成後 實體化道具 載入屬性
        if (m_AssetLoaderSystem.IsLoadAllAseetCompleted && _bLoadAsset && _bLoadPanel)
        {
            _bLoadAsset = false;
            m_AssetLoaderSystem.Initialize();
            InstantiateItem();
            LoadPurchaseProperty();
            ResumeToggleTarget();
        }
        #endregion
    }


    #region -- InstantiateItem 實體化 物品欄位 --
    /// <summary>
    /// 實體化 物品欄位
    ///   (伺服器道具不會刪除 只會下架)
    /// </summary>
    private void InstantiateItem()
    {
        GameObject bundle = m_AssetLoaderSystem.GetAsset(Global.PurchaseItemAssetName);
        List<string> serverPurchasekeys = Global.purchaseItem.Keys.ToList();
        var newPurchaseData = Global.purchaseItem.Where(kvp => !_dictitemSlot.ContainsKey(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        int i = 0;

        // 如果有新的道具資料
        if (newPurchaseData.Count > 0 && Global.purchaseItem.Count > _dictitemSlot.Count)
        {
            // 實體化道具欄位
            foreach (KeyValuePair<string, object> item in newPurchaseData)
            {
                Dictionary<string, object> values = item.Value as Dictionary<string, object>;
                values.TryGetValue(PurchaseProperty.OnSell, out object value);
                values.TryGetValue(PurchaseProperty.PromotionsTime, out object promotionsTime);

                // 如果商品在銷售期間內中才實體化
                if (bool.Parse(value.ToString()) && Convert.ToDateTime(promotionsTime.ToString()) > System.DateTime.Now)
                {
                    values.TryGetValue(PurchaseProperty.ItemName, out value);

                    Transform itemSlot = MPGFactory.GetObjFactory().Instantiate(bundle, UI.itemPanel.transform, value.ToString(), new Vector3(0, -_slotPosY, 0), Vector3.one, Vector2.zero, 100).transform;
                    UIEventListener.Get(itemSlot.gameObject).onClick = OnPurchase;
                    AddPurchaseItemRefs(item.Key, itemSlot.gameObject);
                    _slotPosY += _offsetY;
                }
            }
        }
        else if (Global.purchaseItem.Count < _dictitemSlot.Count)
        {
            // 移除多餘的 Slot
            while ((Global.purchaseItem.Count < _dictitemSlot.Count) && Global.purchaseItem.Count > 0)
            {
                string lastKey = _dictitemSlot.Keys.Last();
                _dictitemSlot.Remove(lastKey);
            }
        }

        // 重新命名物件名稱
        foreach (var item in _dictitemSlot)
        {
            if (i < serverPurchasekeys.Count)
                item.Value.name = serverPurchasekeys[i];
            i++;
        }
    }
    #endregion

    #region -- LoadPurchaseProperty  載入道具資料 --
    /// <summary>
    /// 載入道具資料
    /// </summary>
    private void LoadPurchaseProperty()
    {
        List<string> keys = _dictitemSlot.Keys.ToList();

        // 載入屬性
        foreach (KeyValuePair<string, object> item in Global.purchaseItem)
        {
            // 取出新物件值，並賦值目前物件值 
            if (keys.Contains(item.Key))
            {
                Dictionary<string, object> values = item.Value as Dictionary<string, object>;

                // 取值
                values.TryGetValue(PurchaseProperty.OnSell, out object bSell);
                values.TryGetValue(PurchaseProperty.Price, out object price);
                values.TryGetValue(PurchaseProperty.LimitCount, out object limitCount);
                values.TryGetValue(PurchaseProperty.NewArrivalsTime, out object newArrivalsTime);
                values.TryGetValue(PurchaseProperty.PromotionsTime, out object promotionsTime);
                values.TryGetValue(PurchaseProperty.BuyCount, out object buyCount);
                values.TryGetValue(PurchaseProperty.PromotionsCount, out object promotionsCount);

                // 價格
                _dictitemSlot[item.Key].transform.Find("Info").transform.Find(PurchaseProperty.Price).GetComponent<UILabel>().text = "x" + price.ToString();
                // 法幣價格
                //   _dictitemSlot[item.Key].transform.Find("Info").Find(PurchaseProperty.FitCurrency).GetComponent<UILabel>().text = _currencyCode +" "+ _dictProductsFitCurrency[item.Key].ToString();
                // 上市日期 檢查 附值
                NewArrivalsTime_LabelChk(item.Key, Convert.ToDateTime(newArrivalsTime));
                // 促銷日期 檢查 附值
                PromotionsTime_LabelChk(item.Key, Convert.ToDateTime(promotionsTime));
                // 促銷數量 檢查 附值
                PromotionsCount_LabelChk(item.Key, int.Parse(promotionsCount.ToString()));
                // 限制數量 檢查 附值
                LimitCount_LabelChk(item.Key, int.Parse(limitCount.ToString()), buyCount.ToString());
            }
        }
    }
    #endregion

    #region -- PromotionsTime_LabelChk 促銷時間檢查 --
    /// <summary>
    /// 促銷時間 改變道具欄位Active狀態
    /// </summary>
    /// <param name="key">字典資訊位置</param>
    /// <param name="promotionsTime">限制時間</param>
    private void PromotionsTime_LabelChk(string key, DateTime promotionsTime)
    {
        if (promotionsTime > DateTime.Now)
        {
            _dictitemSlot[key].GetComponent<UIButton>().enabled = true;
            _dictitemSlot[key].transform.Find("BG").Find("ActiveBG").gameObject.SetActive(true);
            _dictitemSlot[key].transform.Find("Info").Find(PurchaseProperty.PromotionsTime).GetComponent<UILabel>().text = "~" + promotionsTime.ToString("yyyy/MM/dd");
        }
        else
        {
            _dictitemSlot[key].GetComponent<UIButton>().enabled = false;
            _dictitemSlot[key].transform.Find("BG").Find("ActiveBG").gameObject.SetActive(false);
            _dictitemSlot[key].transform.Find("Info").Find(PurchaseProperty.PromotionsTime).GetComponent<UILabel>().text = "";
            _dictitemSlot[key].transform.Find("Info").Find(PurchaseProperty.NewArrivalsTime).GetComponent<UILabel>().text = "overtime";
        }
    }
    #endregion

    #region -- NewArrivalsTime_LabelChk 開始販售檢查 --
    /// <summary>
    /// 是否開始販售中 改變道具欄位Active狀態
    /// </summary>
    /// <param name="key">字典資訊位置</param>
    /// <param name="limitCount">限制數量</param>
    private void NewArrivalsTime_LabelChk(string key, DateTime newArrivalsTime)
    {
        if (newArrivalsTime < DateTime.Now)
        {
            _dictitemSlot[key].GetComponent<UIButton>().enabled = true;
            _dictitemSlot[key].transform.Find("BG").Find("ActiveBG").gameObject.SetActive(true);
            _dictitemSlot[key].transform.Find("Info").Find(PurchaseProperty.LimitCount).GetComponent<UILabel>().text = newArrivalsTime.ToString("yyyy/MM/dd");
        }
        else
        {
            _dictitemSlot[key].GetComponent<UIButton>().enabled = false;
            _dictitemSlot[key].transform.Find("BG").Find("ActiveBG").gameObject.SetActive(false);
            _dictitemSlot[key].transform.Find("Info").Find(PurchaseProperty.LimitCount).GetComponent<UILabel>().text = "Coming Soon..";
        }
    }
    #endregion

    #region -- PromotionsCount_LabelChk 促銷數量檢查 --
    /// <summary>
    /// 顯示數量限制
    /// </summary>
    /// <param name="key">字典資訊位置</param>
    /// <param name="limitCount">限制數量</param>
    private void PromotionsCount_LabelChk(string key, int promotionsCount)
    {
        if (promotionsCount > 0)
        {
            _dictitemSlot[key].transform.Find("Info").Find(PurchaseProperty.PromotionsCount).GetComponent<UILabel>().text = "+" + promotionsCount.ToString();
            _dictitemSlot[key].transform.Find("Info").Find(PurchaseProperty.PromotionsImage).gameObject.SetActive(true);
        }
        else
        {
            _dictitemSlot[key].transform.Find("Info").Find(PurchaseProperty.PromotionsCount).GetComponent<UILabel>().text = "";
            _dictitemSlot[key].transform.Find("Info").Find(PurchaseProperty.PromotionsImage).gameObject.SetActive(false);
        }
    }
    #endregion

    #region -- LimitCount_LabelChk 購買數量限制檢查 --
    /// <summary>
    /// 顯示數量限制
    /// </summary>
    /// <param name="key">字典資訊位置</param>
    /// <param name="limitCount">限制數量</param>
    private void LimitCount_LabelChk(string key, int limitCount, string buyCount)
    {
        _dictitemSlot[key].transform.Find("Info").Find(PurchaseProperty.LimitCount).GetComponent<UILabel>().text = "";
        _dictitemSlot[key].transform.Find("BG").Find("ActiveBG").gameObject.SetActive(true);

        // 剩餘購買量 limit -buycount
        int purcahseRemainingCount = (int.Parse(limitCount.ToString()) > 0) ? int.Parse(limitCount.ToString()) - int.Parse(buyCount.ToString()) : -1;

        bool bSoldOut = (int.Parse(limitCount.ToString()) == -1 || purcahseRemainingCount > 0) ? false : true;

        // 如果 商品有庫存 顯示物品按扭
        if (purcahseRemainingCount > 0)
        {
            _dictitemSlot[key].transform.Find("Info").Find(PurchaseProperty.LimitCount).GetComponent<UILabel>().text = "Limit:" + buyCount + "/" + limitCount.ToString();
            UIEventListener.Get(_dictitemSlot[key]).onClick = OnPurchase;
        }
        else if (limitCount == 0 || bSoldOut)
        {
            _dictitemSlot[key].transform.Find("Info").Find(PurchaseProperty.LimitCount).GetComponent<UILabel>().text = "Sold Out!";
            _dictitemSlot[key].transform.Find("BG").Find("ActiveBG").gameObject.SetActive(false);
            UIEventListener.Get(_dictitemSlot[key]).onClick = null;
        }
    }
    #endregion

    #region -- AddPurchaseItemRefs 加入購物索引 --
    private void AddPurchaseItemRefs(string id, GameObject go)
    {
        if (_dictitemSlot.ContainsKey(id))
            _dictitemSlot[id] = go;
        _dictitemSlot.Add(id, go);
    }
    #endregion

    #region -- OnPurchase 當按下夠買  --
    private void OnPurchase(GameObject go)
    {
        Debug.Log("OnPurchase Need New version Scripts!");
        //GetComponentInParent<PurchaseHandler>().Purchase(go.name);
    }
    #endregion

    #region -- OnLoadPanel   --
    /// <summary>
    /// 當載入Panel時，載入資料
    /// </summary>
    protected override void OnLoading()
    {
        UI = m_RootUI.GetComponentInChildren<AttachBtn_PurchaseUI>();
        UIEventListener.Get(UI.closeCollider).onClick = OnClosed;
        _dataLoadedCount = (int)ENUM_Data.None;

        if (Global.isMatching)
            Global.photonService.ExitWaitingRoom();

        Global.photonService.LoadCurrency(Global.Account);
        Global.photonService.LoadPurchase();
    }

    /// <summary>
    /// 當Panel載入完成時
    /// </summary>
    protected override void OnLoadPanel()
    {
        GetMustLoadAsset();
    }
    #endregion

    #region -- GetMustLoadAsset 載入幣要資產 --
    protected override void GetMustLoadAsset()
    {
        if (m_RootUI.activeSelf && !Global.isGameStart)
        {
            if (_bFirstLoad)
            {
                _bFirstLoad = false;
                m_AssetLoaderSystem.LoadAssetFormManifest(Global.PanelUniquePath + Global.PurchaseItemAssetName + Global.ext);
                m_AssetLoaderSystem.SetLoadAllAseetCompleted();
            }
            _bLoadAsset = true;
        }
    }
    #endregion

    #region -- ShowPanel  --
    public override void ShowPanel(string panelName)
    {
        m_RootUI = GameObject.Find(Global.Scene.MainGameAsset.ToString()).GetComponentInChildren<AttachBtn_MenuUI>().purchasePanel;
        base.ShowPanel(panelName);
    }
    #endregion

    #region -- OnLoadData 載入資料區  --
    private void OnLoadCurrency()
    {
        _dataLoadedCount *= (int)ENUM_Data.CurrencyData;
    }

    private void OnLoadPurchase()
    {
        _dataLoadedCount *= (int)ENUM_Data.Purchase;
    }
    protected override int GetMustLoadedDataCount()
    {
        return (int)ENUM_Data.Purchase * (int)ENUM_Data.CurrencyData; ;
    }
    #endregion

    #region -- OnUpdateCurrency 當收到更新貨幣時 --
    /// <summary>
    /// 當收到更新貨幣時
    /// </summary>
    private void OnUpdateCurrency()
    {
        Debug.Log("NOT ME!!!");
        throw new NotImplementedException();
    }
    #endregion

    #region -- OnClosed  --
    public override void OnClosed(GameObject go)
    {
        ShowPanel(m_RootUI.name);
    }
    #endregion

    #region -- Release --
    public override void Release()
    {
        Global.photonService.LoadCurrencyEvent -= OnLoadCurrency;
        Global.photonService.LoadPurchaseEvent -= OnLoadPurchase;
        Global.photonService.UpdateCurrencyEvent -= OnUpdateCurrency;
    }
    #endregion




    //public void OnProductRequest(Dictionary<string, object> dictProducts, Product[] product, string currencyCode)
    //{
    //    _dictProductsFitCurrency = dictProducts;
    //    _product = product;
    //    _bLoadProduct = true;
    //    _currencyCode = currencyCode;
    //    Debug.Log("Manager OnProductRequest");
    //}

    //public void OnIABInit(bool status)
    //{
    //    _bIABInit = status;
    //}
}
