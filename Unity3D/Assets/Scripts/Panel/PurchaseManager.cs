using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PurchaseManager : MPPanel
{
    ObjectFactory objFactory;
    
    public int offset = 275;
    public GameObject itemPanel;

    private Sdkbox.Product[] _product;
    private Dictionary<string,GameObject> dictitemSlot;
    private bool _bLoadAsset, _bFirstLoad, _bLoadPanel, _bIABInit, _bLoadPurchase,_bLoadProduct, _bLoadCurrency;
    private string purchaseName = "Purchase";
    private int _slotPosY;

    public PurchaseManager(MPGame MPGame) : base(MPGame) { }

    // Use this for initialization
    void Start()
    {
        objFactory = new ObjectFactory();
        dictitemSlot = new Dictionary<string, GameObject>();
        _bLoadProduct = _bFirstLoad = true;
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
        public const string PromotionsCount = "PromotionsCount";
        public const string PromotionsTime = "PromotionsTime";
        public const string PromotionsLimit = "PromotionsTime";
        public const string Promotions = "Promotions";
        public const string LimitCount = "LimitCount";
        public const string OnSell = "OnSell";
    }

    private void InstantiateItem()
    {
        GameObject bundle = m_MPGame.GetAssetLoader().GetAsset("PurchaseItem");
        Transform itemSlot = null;
        object value;

        var newData  = Global.purchaseItem.Where(kvp => !dictitemSlot.ContainsKey(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);


        if (newData.Count>0 )
        {
            // ins slot
            foreach (KeyValuePair<string, object> item in newData)
            {
                Dictionary<string, object> values = item.Value as Dictionary<string, object>;
                values.TryGetValue(PurchaseProperty.OnSell,out value);

                // 如果商品在銷售中才實體化
                if (bool.Parse(value.ToString()))
                {
                    values.TryGetValue(PurchaseProperty.ItemName, out value);

                    itemSlot = objFactory.Instantiate(bundle, itemPanel.transform, value.ToString(), new Vector3(0, -_slotPosY, 0), Vector3.one, Vector2.zero, 100).transform;
                    UIEventListener.Get(itemSlot.gameObject).onClick = OnPurchase;
                    Add2Refs(item.Key,itemSlot.gameObject);
                    _slotPosY += offset;
                }
            }
        } 
    }


    private void LoadPurchaseProperty()
    {
         int i = 0;
            object value;

            // assign value
            foreach (KeyValuePair<string, object> item in Global.purchaseItem)
            {
                if (dictitemSlot.Count > Global.purchaseItem.Count)
                {
                    Dictionary<string, object> values = item.Value as Dictionary<string, object>;
                    values.TryGetValue(PurchaseProperty.OnSell, out value);

                    // 如果商品在銷售中才載入值
                    if (bool.Parse(value.ToString()))
                    {
                       // values.TryGetValue(PurchaseProperty.ItemName, out value);
                       // itemList[i].transform.Find(PurchaseProperty.ItemName).GetComponent<UISprite>().spriteName = value.ToString();
                        values.TryGetValue(PurchaseProperty.Price, out value);
                        dictitemSlot[item.Key].transform.Find(PurchaseProperty.Price).GetComponent<UILabel>().text = "x" + value.ToString();
                        values.TryGetValue(PurchaseProperty.PromotionsCount, out value);
                        dictitemSlot[item.Key].transform.Find(PurchaseProperty.PromotionsCount).GetComponent<UILabel>().text = value.ToString();
                        values.TryGetValue(PurchaseProperty.PromotionsTime, out value);
                        dictitemSlot[item.Key].transform.Find(PurchaseProperty.PromotionsTime).GetComponent<UILabel>().text = value.ToString();
                        values.TryGetValue(PurchaseProperty.LimitCount, out value);
                        dictitemSlot[item.Key].transform.Find(PurchaseProperty.LimitCount).GetComponent<UILabel>().text = value.ToString();

                        i++;
                    }

                }
                else
                {
                    break;
                }
            }
        }
    

    private void Add2Refs(string id,GameObject go)
    {
        if (dictitemSlot.ContainsKey(id))
            dictitemSlot[id] = go;
        dictitemSlot.Add(id,go);
    }

    private void OnPurchase(GameObject go)
    {
        GetComponent<PurchaseHandler>().Purchase(go.name);
    }

    public void OnProductRequest(Sdkbox.Product[] products)
    {
        _product = new Sdkbox.Product[products.Length];
        _product = products;
        _bLoadProduct = true;  
    }

    protected override void OnLoading()
    {
        Global.photonService.LoadCurrency(Global.Account);
        Global.photonService.LoadPurchase();
    }

    protected override void OnLoadPanel()
    {
        GetMustLoadAsset();
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
