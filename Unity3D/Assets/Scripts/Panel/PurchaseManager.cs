using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Sdkbox;
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
 * 20180101 v1.0.0   完成功能、註解             
 * ****************************************************************/
// 可以把PurchaseHandlder寫到這裡
public class PurchaseManager : MPPanel
{
    ObjectFactory objFactory;

    public int offset = 275;
    public GameObject itemPanel;

    private Sdkbox.IAP _iap;                                        // IAP資料
    private Sdkbox.Product[] _product;                              // 商品資料
    private Dictionary<string, GameObject> _dictitemSlot;           // 已實體化的商品
    private Dictionary<string, object> _dictProductsFitCurrency;    // 法幣道具價格
    private bool _bLoadAsset, _bFirstLoad, _bLoadPanel, _bIABInit, _bLoadPurchase, _bLoadProduct, _bLoadCurrency;
    private string _currencyCode = "TWD";                           // 貨幣代號
    private int _slotPosY;                                          // 道具位子偏移量

    public PurchaseManager(MPGame MPGame) : base(MPGame) { }

    // Use this for initialization
    void Start()
    {
        _iap = FindObjectOfType<Sdkbox.IAP>();
        objFactory = new ObjectFactory();
        _dictitemSlot = new Dictionary<string, GameObject>();
        _dictProductsFitCurrency = new Dictionary<string, object>();
        _bFirstLoad = true;
        _bLoadProduct = true;

        _iap.getProducts();
    }

    void OnEnable()
    {
        _bLoadPanel = false;

        Global.photonService.LoadCurrencyEvent += OnLoadCurrency;
        Global.photonService.LoadPurchaseEvent += OnLoadPurchase;
    }


    // Update is called once per frame
    void Update()
    {
        //if (!string.IsNullOrEmpty(assetLoader.ReturnMessage))
        //    Debug.Log("訊息：" + assetLoader.ReturnMessage);

        // 資料載入完成後 載入Panel
        if (_bLoadCurrency && _bLoadPurchase && _bLoadProduct && !_bLoadPanel)
        {
            if (!_bFirstLoad)
                ResumeToggleTarget();

            _bLoadPanel = true;
            OnLoadPanel();
        }

        // Panel載入完成後 實體化道具 載入屬性
        if (m_MPGame.GetAssetLoader().loadedObj && _bLoadAsset && _bLoadPanel)
        {
            _bLoadAsset = !_bLoadAsset;
            InstantiateItem();
            LoadPurchaseProperty();
            ResumeToggleTarget();
        }
    }

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

    // 目前沒寫 數量變少時的刪除物件處理(伺服器道具不會刪除 只會下架)
    /// <summary>
    /// 實體化 物品欄位
    /// </summary>
    private void InstantiateItem()
    {
        GameObject bundle = m_MPGame.GetAssetLoader().GetAsset("PurchaseItem");
        Transform itemSlot = null;
        object value, promotionsTime;
        bool reload = false;

        var newData = Global.purchaseItem.Where(kvp => !_dictitemSlot.ContainsKey(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        List<string> serverPurchasekeys = Global.purchaseItem.Keys.ToList();

        // 如果有新的道具資料
        if (newData.Count > 0 && Global.purchaseItem.Count > _dictitemSlot.Count)
        {
            // 實體化道具欄位
            foreach (KeyValuePair<string, object> item in newData)
            {
                Dictionary<string, object> values = item.Value as Dictionary<string, object>;
                values.TryGetValue(PurchaseProperty.OnSell, out value);
                values.TryGetValue(PurchaseProperty.PromotionsTime, out promotionsTime);

                // 如果商品在銷售期間內中才實體化
                if (bool.Parse(value.ToString()) && Convert.ToDateTime(promotionsTime.ToString()) > System.DateTime.Now)
                {
                    values.TryGetValue(PurchaseProperty.ItemName, out value);

                    itemSlot = objFactory.Instantiate(bundle, itemPanel.transform, value.ToString(), new Vector3(0, -_slotPosY, 0), Vector3.one, Vector2.zero, 100).transform;
                   // UIEventListener.Get(itemSlot.gameObject).onClick = OnPurchase;
                    Add2Refs(item.Key, itemSlot.gameObject);
                    _slotPosY += offset;
                }
            }
            reload = true;
        }
        else if (Global.purchaseItem.Count < _dictitemSlot.Count)
        {
            string lastKey = _dictitemSlot.Keys.Last();
            _dictitemSlot.Remove(lastKey);
            reload = true;
        }

        int i = 0;

        if (reload)
            foreach (var item in _dictitemSlot)
            {
                if (i < serverPurchasekeys.Count)
                    item.Value.name = serverPurchasekeys[i];
                i++;
            }
    }

    /// <summary>
    /// 載入道具資料
    /// </summary>
    private void LoadPurchaseProperty()
    {
        object bSell,price, promotionsTime, newArrivalsTime, promotionsCount, buyCount, limitCount;
        List<string> keys = _dictitemSlot.Keys.ToList();

        // 載入屬性
        foreach (KeyValuePair<string, object> item in Global.purchaseItem)
        {
            // 如果舊的數量較多
            if (keys.Contains(item.Key))
            {
                Dictionary<string, object> values = item.Value as Dictionary<string, object>;

                // 取值
                values.TryGetValue(PurchaseProperty.OnSell, out bSell);
                values.TryGetValue(PurchaseProperty.Price, out price);
                values.TryGetValue(PurchaseProperty.LimitCount, out limitCount);
                values.TryGetValue(PurchaseProperty.NewArrivalsTime, out newArrivalsTime);
                values.TryGetValue(PurchaseProperty.PromotionsTime, out promotionsTime);
                values.TryGetValue(PurchaseProperty.BuyCount, out buyCount);
                values.TryGetValue(PurchaseProperty.PromotionsCount, out promotionsCount);

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
            _dictitemSlot[key].transform.Find("Info").Find(PurchaseProperty.PromotionsTime).GetComponent<UILabel>().text ="~"+ promotionsTime.ToString("yyyy/MM/dd");
        }
        else
        {
            _dictitemSlot[key].GetComponent<UIButton>().enabled = false;
            _dictitemSlot[key].transform.Find("BG").Find("ActiveBG").gameObject.SetActive(false);
            _dictitemSlot[key].transform.Find("Info").Find(PurchaseProperty.PromotionsTime).GetComponent<UILabel>().text = "";
            _dictitemSlot[key].transform.Find("Info").Find(PurchaseProperty.NewArrivalsTime).GetComponent<UILabel>().text = "overtime";
        }
    }

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

    private void Add2Refs(string id, GameObject go)
    {
        if (_dictitemSlot.ContainsKey(id))
            _dictitemSlot[id] = go;
        _dictitemSlot.Add(id, go);
    }

    private void OnPurchase(GameObject go)
    {
        GetComponentInParent<PurchaseHandler>().Purchase(go.name);
    }

    protected override void OnLoading()
    {
        Global.photonService.LoadCurrency(Global.Account);
        Global.photonService.LoadPurchase();
    }

    protected override void OnLoadPanel()
    {
        GetMustLoadAsset();
        EventMaskSwitch.lastPanel = gameObject;
    }

    protected override void GetMustLoadAsset()
    {
        if (enabled && !Global.isGameStart)
        {
            if (_bFirstLoad)
            {
                _bFirstLoad = false;

                //  m_MPGame.GetAssetLoader().LoadAsset("Purchase" + "/", "Purchase" + Global.IconSuffix);
                m_MPGame.GetAssetLoader().LoadAsset("Panel" + "/", "PanelUI");
                m_MPGame.GetAssetLoader().LoadPrefab("Panel" + "/", "PurchaseItem");
            }
            _bLoadAsset = true;
        }
       
    }

    private void OnLoadCurrency()
    {
        _bLoadCurrency = true;
    }

    private void OnLoadPurchase()
    {
        _bLoadPurchase = true;
    }

    public void OnProductRequest(Dictionary<string, object> dictProducts, Product[] product, string currencyCode)
    {
        _dictProductsFitCurrency = dictProducts;
        _product = product;
        _bLoadProduct = true;
        _currencyCode = currencyCode;
        Debug.Log("Manager OnProductRequest");
    }



    public void OnIABInit(bool status)
    {
        _bIABInit = status;
    }

    public override void OnClosed(GameObject obj)
    {
        EventMaskSwitch.lastPanel = null;
        GameObject root = obj.transform.parent.gameObject;
        GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().LoadPanel(root);
        EventMaskSwitch.Resume();
    }

    void OnDisEnable()
    {
        Global.photonService.LoadCurrencyEvent -= OnLoadCurrency;
        Global.photonService.LoadPurchaseEvent -= OnLoadPurchase;
    }
}
