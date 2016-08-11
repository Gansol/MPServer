using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

public class StoreManager : MonoBehaviour
{
    AssetLoader assetLoader;
    public GameObject[] infoGroupsArea;
    public string[] assetFolder;
    public int itemOffsetX, itemOffsetY;
    private GameObject _clone;
    private static GameObject _tmpTab;
    private bool _LoadedGashapon, _LoadedMice;
    private Dictionary<string, object> _miceData;

    void Awake()
    {
        assetLoader = gameObject.AddComponent<AssetLoader>();
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
            LoadProperty(Global.miceProperty, infoGroupsArea[3].transform);
        }

    }

    void OnMessage()
    {
        assetLoader.LoadAsset(assetFolder[0] + "/", assetFolder[0]);
        LoadGashapon(assetFolder[0]);
    }

    #region -- OnClick 按下事件 --
    public void OnGashaponClick(GameObject obj)
    {
        Debug.Log(obj.name);
    }

    public void OnItemClick(GameObject obj)
    {
        Debug.Log(obj.name);
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

    #region -- LoadIconObject 載入老鼠物件 --
    void LoadIconObject(Dictionary<string, object> dictionary, string folder)    // 載入遊戲物件
    {
        foreach (KeyValuePair<string, object> item in dictionary)
        {
            var miceData = item.Value as Dictionary<string, object>;
            foreach (KeyValuePair<string, object> inner in miceData)
            {
                assetLoader.LoadPrefab(folder, item.Value.ToString().Remove(item.Value.ToString().Length - 4) + "ICON");
                break;
            }
        }
    }
    #endregion

    #region -- LoadProperty 載入物件屬性 --
    void LoadProperty(Dictionary<string, object> dictionary, Transform parent)
    {
        int i, j;
        i = 0;
        foreach (KeyValuePair<string, object> item in dictionary)
        {
            var innerDict = item.Value as Dictionary<string, object>;
            j = 0;
            foreach (KeyValuePair<string, object> inner in innerDict)
            {
                if (j == 0)
                    parent.GetChild(i).GetComponent<Item>().itemName = inner.Value.ToString();
                else
                    parent.GetChild(i).GetComponent<Item>().property[j - 1] = float.Parse(inner.Value.ToString());
                j++;
            }
            i++;
        }
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
    void InstantiateItem(Dictionary<string, object> dictionary, Transform myParent)
    {
        int i, posX, posY;
        i = posX = posY = 0;
        string itemName = "Item", folderPath = "Panel/";

        foreach (KeyValuePair<string, object> item in dictionary)
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
                _clone.name = itemName + i;
                _clone.transform.localPosition = new Vector3(posX, posY);
                _clone.transform.localScale = Vector3.one;
                posX += itemOffsetX;
                i++;
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
    void InstantiateMice(Dictionary<string, object> dictionary, Transform myParent, string folder)
    {
        int i = 0;
        foreach (KeyValuePair<string, object> item in dictionary)
        {
            var miceData = item.Value as Dictionary<string, object>;
            foreach (KeyValuePair<string, object> inner in miceData)
            {
                string bundleName = inner.Value.ToString().Remove(inner.Value.ToString().Length - 4) + "ICON";
                Debug.Log(bundleName);
                if (assetLoader.GetAsset(folder + "/", bundleName))                  // 已載入資產時
                {
                    GameObject bundle = assetLoader.GetAsset(folder + "/", bundleName);
                    Transform parent = myParent.GetChild(i).GetChild(0);

                    //Add2Refs(bundle, miceBtn);     // 加入物件參考

                    _clone = (GameObject)Instantiate(bundle);             // 實體化
                    _clone.layer = myParent.gameObject.layer;
                    _clone.transform.parent = parent;
                    _clone.name = item.Value.ToString();
                    _clone.transform.localPosition = Vector3.zero;
                    _clone.transform.localScale = Vector3.one;
                    _clone.GetComponent<UISprite>().width = 150;
                    break;
                }
                else
                {
                    Debug.LogError("Assetbundle reference not set to an instance.");
                }
            }
            i++;
        }
        _LoadedMice = true;
    }
    #endregion
}
