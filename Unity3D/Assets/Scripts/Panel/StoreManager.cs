using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using MPProtocol;

public class StoreManager : MonoBehaviour
{

    public GameObject[] infoGroupsArea;
    public string[] assetFolder;
    public int itemOffsetX, itemOffsetY;
    public float actorScale = 0.8f;

    private AssetLoader assetLoader;

    private GameObject _clone, _btnClick, _tmpActor, _lastPanel, _lastItem;
    private static GameObject _tmpTab;
    private bool _LoadedGashapon, _LoadedMice, _LoadedActor;
    private Dictionary<string, object> _miceData;
    private Dictionary<string, GameObject> _dictActor;
    /// <summary>
    /// 儲存購買商品資料 0:商品ID、1:類型、2:數量
    /// </summary>
    private string[] goods;

    void Awake()
    {
        assetLoader = gameObject.AddComponent<AssetLoader>();
        _dictActor = new Dictionary<string, GameObject>();
        goods = new string[3];
        Global.photonService.UpdateCurrencyEvent += LoadPlayerInfo;

    }

    void Update()
    {
        if (!string.IsNullOrEmpty(assetLoader.ReturnMessage))
            Debug.Log("訊息：" + assetLoader.ReturnMessage);

        if (assetLoader.loadedObj && !_LoadedGashapon)
        {
            _LoadedGashapon = !_LoadedGashapon;
            assetLoader.init();
            _tmpTab = infoGroupsArea[2];
            InstantiateGashapon(infoGroupsArea[2].transform, assetFolder[0]);
        }

        if (assetLoader.loadedObj && !_LoadedMice)
        {
            _LoadedMice = !_LoadedMice;
            assetLoader.init();
            InstantiateItem(Global.miceProperty, infoGroupsArea[3].transform);
            InstantiateMice(Global.miceProperty, infoGroupsArea[3].transform, assetFolder[1]);
            LoadPrice(Global.miceProperty, infoGroupsArea[3].transform);
        }

        if (assetLoader.loadedObj && !_LoadedActor)
        {
            _LoadedActor = !_LoadedActor;
            assetLoader.init();
            InstantiateActor(infoGroupsArea[4].transform.GetChild(0).gameObject);
        }

    }

    void OnMessage()
    {
        assetLoader.LoadAsset(assetFolder[0] + "/", assetFolder[0]);
        LoadGashapon(assetFolder[0]);
        LoadPlayerInfo();
        EventMaskSwitch.lastPanel = gameObject;
    }

    private void LoadPlayerInfo()
    {
        infoGroupsArea[0].transform.GetChild(0).GetComponent<UILabel>().text = "20";
        infoGroupsArea[0].transform.GetChild(1).GetComponent<UILabel>().text = Global.Rice.ToString();
        infoGroupsArea[0].transform.GetChild(2).GetComponent<UILabel>().text = Global.Gold.ToString();
    }

    #region -- OnClick 按下事件 --
    public void OnGashaponClick(GameObject obj)
    {
        Debug.Log(obj.name);
    }

    public void OnItemClick(GameObject obj)
    {
        goods[0] = obj.name;
        goods[1] = obj.GetComponent<Item>().property[(int)ItemProperty.ItemType];
        Debug.Log(obj.name);
        _lastItem = obj;
        infoGroupsArea[4].SetActive(true);
        LoadMiceProperty loadProperty = new LoadMiceProperty();
        loadProperty.LoadProperty(obj, infoGroupsArea[4], 0);
        LoadActor(obj);
        EventMaskSwitch.Switch(infoGroupsArea[4]);
    }

    public void OnBuyClick(GameObject myPanel)
    {
        myPanel.SetActive(false);
        infoGroupsArea[5].SetActive(true);
        BuyWindowsInit();
        LoadMiceProperty loadProperty = new LoadMiceProperty();
        loadProperty.LoadProperty(_lastItem, infoGroupsArea[5], 2);
        EventMaskSwitch.Switch(infoGroupsArea[5]);
    }

    private void BuyWindowsInit()
    {
        goods[0] = _lastItem.GetComponent<Item>().property[(int)ItemProperty.MiceName];
        goods[1] = _lastItem.GetComponent<Item>().property[(int)ItemProperty.ItemType];
        goods[2] ="1";
        infoGroupsArea[5].transform.GetChild(0).GetChild(4).GetComponent<UILabel>().text = "1";  // count = 1
        infoGroupsArea[5].transform.GetChild(0).GetChild(3).GetComponent<UILabel>().text = _lastItem.GetComponent<Item>().property[(int)ItemProperty.Price]; // price
    }

    public void OnQuantity(GameObject obj)
    {
        int price = int.Parse(infoGroupsArea[5].transform.GetChild(0).GetChild(2).GetComponent<UILabel>().text);
        int sum = int.Parse(infoGroupsArea[5].transform.GetChild(0).GetChild(3).GetComponent<UILabel>().text);
        int count = int.Parse(infoGroupsArea[5].transform.GetChild(0).GetChild(4).GetComponent<UILabel>().text);

        count += (obj.name == "Add") ? 1 : -1;
        count = (count < 0) ? 0 : count;
        goods[2] = infoGroupsArea[5].transform.GetChild(0).GetChild(4).GetComponent<UILabel>().text = count.ToString();
        infoGroupsArea[5].transform.GetChild(0).GetChild(3).GetComponent<UILabel>().text = (price * count).ToString();
    }

    public void OnComfirm(GameObject myPanel)
    {
        myPanel.SetActive(false);
        Global.photonService.BuyItem(Global.Account, goods);
    }

    public void OnClosed(GameObject obj)
    {
        EventMaskSwitch.lastPanel = null;
        GameObject root = obj.transform.parent.parent.gameObject;

        _tmpTab.SetActive(false);
        infoGroupsArea[2].SetActive(true);
        _tmpTab = infoGroupsArea[2];

        GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().LoadPanel(obj.transform.parent.gameObject);
        EventMaskSwitch.Switch(root);
    }

    public void OnReturn(GameObject obj)
    {
        EventMaskSwitch.openedPanel.SetActive(false);
        EventMaskSwitch.Switch(obj);
    }

    public void OnTabClick(GameObject obj)
    {
        Debug.Log(obj.name);

        int value = int.Parse(obj.name.Remove(0, 3));

        switch (value)
        {
            case 1:
                {
                    if (_tmpTab != infoGroupsArea[2]) _tmpTab.SetActive(false);
                    infoGroupsArea[2].SetActive(true);
                    _tmpTab = infoGroupsArea[2];
                    break;
                }
            case 2:
                {
                    if (_tmpTab != infoGroupsArea[3]) _tmpTab.SetActive(false);
                    assetLoader.init();
                    assetLoader.LoadAsset(assetFolder[1] + "/", assetFolder[1]);
                    LoadIconObject(Global.miceProperty, assetFolder[1]);
                    assetLoader.LoadPrefab("Panel/", "Item");
                    _LoadedMice = false;
                    infoGroupsArea[3].SetActive(true);
                    _tmpTab = infoGroupsArea[3];
                    break;
                }
            case 3:
                {
                    break;
                }
            case 4:
                {
                    break;
                }
            case 5:
                {
                    break;
                }
            default:
                {
                    Debug.LogError("Unknow Tab!");
                    break;
                }
        }

    }
    #endregion


    #region -- LoadGashapon 載入轉蛋物件 --
    void LoadGashapon(string folder)
    {
        for (int i = 1; i <= 3; i++)
            assetLoader.LoadPrefab(folder + "/", folder + i);
    }
    #endregion

    #region -- LoadIconObject 載入老鼠物件 -- //修改
    void LoadIconObject(string[,] miceData, string folder)    // 載入遊戲物件
    {
        for (int i = 0; i < miceData.GetLength(0); i++)
        {
            assetLoader.LoadPrefab(folder + "/", miceData[i, 0].ToString().Remove(miceData[i, 0].ToString().Length - 4) + "ICON");
        }
    }
    #endregion

    #region -- LoadPrice 載入物件價格 --
    void LoadPrice(string[,] miceData, Transform parent)
    {
        for (int i = 0; i < miceData.GetLength(0); i++)
        {
            parent.GetChild(i).GetComponentInChildren<UILabel>().text = miceData[i, (int)ItemProperty.Price].ToString();
        }
    }
    #endregion

    #region -- LoadActor 載入老鼠角色 --
    private void LoadActor(GameObject btn_mice)
    {
        GameObject _miceImage;
        string miceName = btn_mice.transform.GetComponentInChildren<UISprite>().name;

        _btnClick = btn_mice;

        if (_tmpActor != null) _tmpActor.SetActive(false);          // 如果暫存老鼠圖片不是空的(防止第一次點擊出錯)，將上一個老鼠圖片隱藏

        if (_dictActor.TryGetValue(miceName, out _miceImage))       // 假如已經載入老鼠圖片了 直接顯示
        {
            _miceImage.SetActive(true);
            _tmpActor = _miceImage;
        }
        else
        {

            if (assetLoader.GetAsset(miceName + "/", miceName) != null)
            {
                InstantiateActor(infoGroupsArea[4].transform.GetChild(0).gameObject);
            }
            else
            {
                _LoadedActor = false;
                assetLoader.LoadAsset(miceName + "/", miceName);
                assetLoader.LoadPrefab(miceName + "/", miceName);
            }

            //LoadMiceAsset(btn_mice);
        }
    }
    #endregion

    #region -- InstantiateActor 實體化老鼠角色 --
    private void InstantiateActor(GameObject parent)
    {

        string miceName = _btnClick.transform.GetComponentInChildren<UISprite>().name;

        _clone = (GameObject)Instantiate(assetLoader.GetAsset(miceName + "/", miceName));
        ObjectManager.SwitchDepthLayer(_clone, parent, Global.MeunObjetDepth);

        _clone.transform.parent = parent.transform.GetChild(0);
        _clone.SetActive(false);
        _clone.name = miceName;
        _clone.transform.localPosition = Vector3.zero;
        _clone.transform.localScale = Vector3.one;
        _clone.layer = parent.transform.gameObject.layer;
        _clone.transform.localScale = new Vector3(actorScale, actorScale, 1);

        Destroy(_clone.transform.GetChild(0).GetComponent(_clone.name));    // 刪除Battle用腳本

        GameObject _tmp;
        if (_dictActor.TryGetValue(_clone.name, out _tmp) != null)
            _dictActor.Add(_clone.name, _clone);

        _tmpActor = _clone;
        _clone.SetActive(true);
        _LoadedActor = true;

        AssetBundleManager.UnloadUnusedAssets();

    }
    #endregion
    #region -- InstantiateGashapon 實體化轉蛋物件--
    /// <summary>
    /// 實體化載入完成的轉蛋物件，利用資料夾名稱判斷必要實體物件
    /// </summary>
    /// <param name="myParent">實體化父系位置</param>
    /// <param name="folder">資料夾名稱</param>
    void InstantiateGashapon(Transform myParent, string folder)
    {
        GameObject _clone;
        for (int i = 0; i < 3; i++)
        {
            if (assetLoader.GetAsset(folder + "/", folder + (i + 1).ToString()))                  // 已載入資產時
            {
                GameObject bundle = assetLoader.GetAsset(folder + "/", folder + (i + 1).ToString());
                Transform parent = myParent.GetChild(1).GetChild(i).GetChild(0);

                _clone = (GameObject)Instantiate(bundle);             // 實體化
                _clone.layer = myParent.gameObject.layer;
                _clone.transform.parent = parent;
                _clone.name = folder + (i + 1).ToString();
                _clone.transform.localPosition = Vector3.zero;
                _clone.transform.localScale = Vector3.one;
            }
            else
            {
                Debug.LogError("Assetbundle reference not set to an instance.");
            }
        }
        _LoadedGashapon = true;
    }
    #endregion

    #region -- InstantiateItem 實體化老鼠物件--
    /// <summary>
    /// 實體化載入完成的遊戲物件，利用玩家JASON資料判斷必要實體物件
    /// </summary>
    /// <param name="dictionary">資料字典</param>
    /// <param name="myParent">實體化父系位置</param>
    void InstantiateItem(string[,] miceData, Transform myParent)
    {
        if (myParent.transform.childCount == 0)
        {
            int posX, posY;
            posX = posY = 0;
            string itemName = "Item", folderPath = "Panel/";

            for (int i = 0; i < miceData.GetLength(0); i++)
            {

                if (assetLoader.GetAsset(folderPath, itemName))                  // 已載入資產時
                {
                    // 物件位置排序
                    if (i % 9 == 0 && i != 0)
                    {
                        posX = itemOffsetX * 3;
                        posY = 0;
                    }
                    else if (i % 3 == 0 && i != 0)
                    {
                        posY += itemOffsetY;
                        posX = 0;
                    }

                    GameObject bundle = assetLoader.GetAsset(folderPath, itemName);
                    Transform parent = myParent;

                    _clone = (GameObject)Instantiate(bundle);             // 實體化
                    _clone.layer = myParent.gameObject.layer;
                    _clone.transform.parent = parent;
                    _clone.name = miceData[i, 0];
                    _clone.transform.localPosition = new Vector3(posX, posY);
                    _clone.transform.localScale = Vector3.one;
                    posX += itemOffsetX;

                    for (int j = 0; j < miceData.GetLength(1) - 1; j++)
                    {
                        _clone.GetComponent<Item>().property[j] = miceData[i, j];
                    }

                }
            }
        }
    }
    #endregion

    #region -- InstantiateMice 實體化老鼠物件--
    /// <summary>
    /// 實體化載入完成的遊戲物件，利用玩家JASON資料判斷必要實體物件
    /// </summary>
    /// <param name="dictionary">資料字典</param>
    /// <param name="myParent">實體化父系位置</param>
    void InstantiateMice(string[,] miceData, Transform myParent, string folder)
    {

        for (int i = 0; i < miceData.GetLength(0); i++)
        {
            string bundleName = miceData[i, 0].Remove(miceData[i, 0].Length - 4) + "ICON";
            Debug.Log(i + bundleName);
            if (assetLoader.GetAsset(folder + "/", bundleName))                  // 已載入資產時
            {
                GameObject bundle = assetLoader.GetAsset(folder + "/", bundleName);
                Transform parent = myParent.GetChild(i).GetChild(0);

                //Add2Refs(bundle, miceBtn);     // 加入物件參考

                _clone = (GameObject)Instantiate(bundle);             // 實體化
                _clone.layer = myParent.gameObject.layer;
                _clone.transform.parent = parent;
                _clone.name = miceData[i, 0];
                _clone.GetComponent<UISprite>().depth = 310;
                _clone.transform.localPosition = Vector3.zero;
                _clone.transform.localScale = Vector3.one;
                _clone.GetComponent<UISprite>().width = 150;
            }
            else
            {
                Debug.LogError("Assetbundle reference not set to an instance.");
            }
        }
        _LoadedMice = true;
    }
    #endregion
}
