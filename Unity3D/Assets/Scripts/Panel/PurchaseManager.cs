using UnityEngine;
using System.Collections;

public class PurchaseManager : MPPanel
{

    private bool _bLoadObj;
    public int purchaseCount;
    public GameObject itemPanel;
    private string purchaseName = "Purchase";

        public PurchaseManager(MPGame MPGame) : base(MPGame) { }

    // Use this for initialization
    void Start()
    {
        purchaseCount = 9;
    }

    // Update is called once per frame
    void Update()
    {
        if (assetLoader.loadedObj && _bLoadObj)
        {
            _bLoadObj = !_bLoadObj;
         InstantiateItem();

        }
    }

    private void InstantiateItem()
    {
        GameObject bundle ;

         for (int i = 0; i < purchaseCount; i++){
             bundle = assetLoader.GetAsset(purchaseName+i);
             MPGFactory.GetObjFactory().Instantiate(bundle, itemPanel.transform, i.ToString(), Vector3.zero, Vector3.one, new Vector2(200, 200), -1);
         }
    }



    protected override void OnLoading()
    {
        if (!assetLoader.GetAsset("ItemICON"))
        {

            assetLoader.LoadAsset("ItemICON/", "ItemICON");

            for (int i = 0; i < purchaseCount; i++)
            {
                assetLoader.LoadPrefab("ItemICON/", purchaseName + i);
            }

            assetLoader.LoadPrefab("Panel/", "Item");
            _bLoadObj = true;
        }
    }

    protected override void OnLoadPanel()
    {
        throw new System.NotImplementedException();
    }

    protected override void GetMustLoadAsset()
    {
        throw new System.NotImplementedException();
    }

    public override void OnClosed(GameObject obj)
    {
        throw new System.NotImplementedException();
    }
}
