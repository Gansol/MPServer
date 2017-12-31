using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Sdkbox;

// 可以把PurchaseHandlder寫到這裡
public class PurchaseManager : MPPanel
{
    ObjectFactory objFactory;

    private Sdkbox.IAP _iap;
    public int offset = 275;
    public GameObject itemPanel;

    private Sdkbox.Product[] _product;
    private Dictionary<string, GameObject> _dictitemSlot;
    private Dictionary<string, object> _dictProductsFitCurrency;
    private bool _bLoadAsset, _bFirstLoad, _bLoadPanel, _bIABInit, _bLoadPurchase, _bLoadProduct, _bLoadCurrency;
    private string _purchaseName = "Purchase", _currencyCode = "TWD";
    private int _slotPosY;

    public PurchaseManager(MPGame MPGame) : base(MPGame) { }

    // Use this for initialization
    void Start()
    {
        _iap = FindObjectOfType<Sdkbox.IAP>();
        if (_iap == null)
        {
            Debug.Log("Failed to find IAP instance");
        }

        objFactory = new ObjectFactory();
        _dictitemSlot = new Dictionary<string, GameObject>();
        _dictProductsFitCurrency = new Dictionary<string, object>();
        _bFirstLoad = true;
        _bLoadProduct = false;

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
        if (!string.IsNullOrEmpty(assetLoader.ReturnMessage))
            Debug.Log("訊息：" + assetLoader.ReturnMessage);

        if (_bLoadCurrency && _bLoadPurchase && _bLoadProduct && !_bLoadPanel)
        {
            if (!_bFirstLoad)
                ResumeToggleTarget();

            _bLoadPanel = true;
            OnLoadPanel();
        }


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
        object value, dataTime;

        var newData = Global.purchaseItem.Where(kvp => !_dictitemSlot.ContainsKey(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);


        if (newData.Count > 0)
        {
            // ins slot
            foreach (KeyValuePair<string, object> item in newData)
            {
                Dictionary<string, object> values = item.Value as Dictionary<string, object>;
                values.TryGetValue(PurchaseProperty.OnSell, out value);
                values.TryGetValue(PurchaseProperty.PromotionsTime, out dataTime);

                // 如果商品在銷售期間內中才實體化
                if (bool.Parse(value.ToString()) && Convert.ToDateTime(dataTime.ToString()) > System.DateTime.Now)
                {
                    values.TryGetValue(PurchaseProperty.ItemName, out value);

                    itemSlot = objFactory.Instantiate(bundle, itemPanel.transform, value.ToString(), new Vector3(0, -_slotPosY, 0), Vector3.one, Vector2.zero, 100).transform;
                    UIEventListener.Get(itemSlot.gameObject).onClick = OnPurchase;
                    Add2Refs(item.Key, itemSlot.gameObject);
                    _slotPosY += offset;
                }
            }
        }
    }

    /// <summary>
    /// 載入道具資料
    /// </summary>
    private void LoadPurchaseProperty()
    {
        int i = 0;
        object value, purchaseTime, newArrivalsTime, buyCount, limitCount;
        bool bSoldOut = true;

        // 載入屬性
        foreach (KeyValuePair<string, object> item in Global.purchaseItem)
        {
            // 如果舊的數量較多
            if (_dictitemSlot.Count >= Global.purchaseItem.Count)
            {
                Dictionary<string, object> values = item.Value as Dictionary<string, object>;

                values.TryGetValue(PurchaseProperty.OnSell, out value);
                values.TryGetValue(PurchaseProperty.LimitCount, out limitCount);
                values.TryGetValue(PurchaseProperty.PromotionsTime, out newArrivalsTime);
                values.TryGetValue(PurchaseProperty.PromotionsTime, out purchaseTime);
                values.TryGetValue(PurchaseProperty.BuyCount, out buyCount);

                Debug.Log(Convert.ToDateTime(purchaseTime) + "  Now:" + System.DateTime.Now);



                bSoldOut = (int.Parse(limitCount.ToString()) == -1 || int.Parse(limitCount.ToString()) - int.Parse(buyCount.ToString()) > 0) ? false : true;


                // 如果商品在銷售中才載入值 //.ToString("yyyyMMddHHmmss")
                if (bool.Parse(value.ToString()) && !bSoldOut && Convert.ToDateTime(purchaseTime) > System.DateTime.Now && Convert.ToDateTime(newArrivalsTime) > System.DateTime.Now)
                {
                    //促銷時間
                    _dictitemSlot[item.Key].transform.Find(PurchaseProperty.PromotionsTime).GetComponent<UILabel>().text ="In: ~" Convert.ToDateTime(purchaseTime).ToString("yyyy/MM/dd/ HH:mm:ss");

                    // 價格
                    values.TryGetValue(PurchaseProperty.Price, out value);
                    _dictitemSlot[item.Key].transform.Find(PurchaseProperty.Price).GetComponent<UILabel>().text = "x" + value.ToString();

                    // 促銷數量
                    values.TryGetValue(PurchaseProperty.PromotionsCount, out value);
                    PromotionsCount_LabelChk(item.Key, int.Parse(value.ToString()));

                    // 限制數量
                    LimitCount_LabelChk(item.Key, int.Parse(limitCount.ToString()));

                    i++;
                }
                else
                {
                    if (Convert.ToDateTime(newArrivalsTime) < System.DateTime.Now)
                    {
                        Debug.LogError("趕快寫賣完了!");
                        // 顯示灰色 賣完了
                    }
                    else
                    {
                        Debug.LogError("趕快寫即將開賣!");
                        // 顯示即將開賣
                    }
                }


                _dictitemSlot[item.Key].transform.Find(PurchaseProperty.FitCurrency).GetComponent<UILabel>().text = _currencyCode +" "+ _dictProductsFitCurrency[item.Key].ToString();
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>
    /// 顯示數量限制
    /// </summary>
    /// <param name="key">字典資訊位置</param>
    /// <param name="limitCount">限制數量</param>
    private void PromotionsCount_LabelChk(string key, int promotionsCount)
    {
        _dictitemSlot[key].transform.Find(PurchaseProperty.PromotionsCount).GetComponent<UILabel>().text = "";
        _dictitemSlot[key].transform.Find(PurchaseProperty.PromotionsImage).gameObject.SetActive(false);

        if (promotionsCount > 0)
        {
            _dictitemSlot[key].transform.Find(PurchaseProperty.PromotionsCount).GetComponent<UILabel>().text = "+" + promotionsCount.ToString();
            _dictitemSlot[key].transform.Find(PurchaseProperty.PromotionsImage).gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 顯示數量限制
    /// </summary>
    /// <param name="key">字典資訊位置</param>
    /// <param name="limitCount">限制數量</param>
    private void LimitCount_LabelChk(string key, int limitCount)
    {
        if (limitCount > 0)
            _dictitemSlot[key].transform.Find(PurchaseProperty.LimitCount).GetComponent<UILabel>().text = limitCount.ToString();
        else if (limitCount == 0)
            _dictitemSlot[key].transform.Find(PurchaseProperty.LimitCount).GetComponent<UILabel>().text = "Sold Out!";
        else
            _dictitemSlot[key].transform.Find(PurchaseProperty.LimitCount).GetComponent<UILabel>().text = "";
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
        }
        _bLoadAsset = true;
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
