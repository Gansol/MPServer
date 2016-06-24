using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MiniJSON;

public class PanelManager : MonoBehaviour
{
    AssetBundleManager assetBundleManager;
    private Dictionary<string, object> _dictObject = null;

    public bool _isLoadTeamData { get; set; }
    public string pathFile;
    public GameObject[] Panel;
    public GameObject[] TeamParent;
    public string[] assetName;

    private GameObject clone;
    private GameObject myParent;

    private bool _bundleLoaded = false;
    private int fileNum;
    private string _name;
    private bool _objetLoaded = false;
    private static bool _isLoadTeam = false;
    private static bool _teamON = false;
    private int _loadObjCount = 0;
    private int _miceCount = 0;
    private int _teamCount = 0;
    private int _matCount = 0;
    private int _objCount = 0;
    private int _panelNo = -1;
    void Awake()
    {
        assetBundleManager = new AssetBundleManager();
    }

    void Update()
    {
        if (assetBundleManager.isStartLoadAsset)
        {
            Debug.Log("訊息：" + assetBundleManager.ReturnMessage);
        }

        Debug.Log(assetBundleManager.loadedABCount);
        if (assetBundleManager.loadedABCount == _matCount && _matCount != 0 && _isLoadTeam != true)   // 2
        {
            Debug.Log("(Update) _matCount: " + _matCount);
            _isLoadTeam = true;
            assetBundleManager.loadedABCount = _matCount = 0;
            LoadObject(Global.MiceAll);
        }

        if (assetBundleManager.loadedObjectCount == _objCount && _objCount != 0 && _isLoadTeam == true)
        {
            assetBundleManager.loadedObjectCount = _matCount = _objCount = 0;
            InstantiateObject(Global.MiceAll, TeamParent[0].transform);
            InstantiateObject(Global.Team, TeamParent[1].transform);
            InitTeam();
        }
    }


    void LoadAsset(string assetName, int fileNum) //1
    {
        _matCount += fileNum * 3;
        Debug.Log("(LoadAsset) _matCount: " + _matCount);
        StartCoroutine(assetBundleManager.LoadAtlas(assetName, typeof(Texture)));
        StartCoroutine(assetBundleManager.LoadAtlas(assetName, typeof(Material)));
        StartCoroutine(assetBundleManager.LoadAtlas(assetName, typeof(GameObject)));
    }

    void LoadObject(string data)    // 載入遊戲物件
    {
        _dictObject = Json.Deserialize(data) as Dictionary<string, object>;
        _objCount += _dictObject.Count;
        Debug.Log("(LoadObject) _objCount: " + _objCount);
        foreach (KeyValuePair<string, object> item in _dictObject)
        {
            Debug.Log("LoadObject : " + item.Value.ToString());
            StartCoroutine(assetBundleManager.LoadGameObject("MiceICON/" + item.Value.ToString().Remove(item.Value.ToString().Length - 4) + "ICON", typeof(GameObject)));
        }
    }

    /// <summary>
    /// 實體化載入完成的遊戲物件，利用玩家JASON資料判斷必要實體物件
    /// </summary>
    /// <param name="data">JSON資料</param>
    /// <param name="parent">實體化父系位置</param>
    void InstantiateObject(string data, Transform parent)
    {
        _dictObject = Json.Deserialize(data) as Dictionary<string, object>;
        _objCount = 0;
        foreach (KeyValuePair<string, object> item in _dictObject)
        {
            string bundleName = "MiceICON/" + item.Value.ToString().Remove(item.Value.ToString().Length - 4) + "ICON";
            if (assetBundleManager.bLoadedAssetbundle(bundleName))
            {
                AssetBundle bundle = AssetBundleManager.getAssetBundle(bundleName);
                if (bundle != null)
                {
                    Debug.Log("LoadMice : " + item.Value.ToString());
                    clone = (GameObject)Instantiate(bundle.mainAsset);
                    clone.transform.parent = parent.GetChild(_objCount);
                    clone.name = item.Value.ToString();
                    clone.transform.localPosition = Vector3.zero;
                    clone.transform.localScale = Vector3.one;
                    _objCount++;
                }
                else
                {
                    Debug.LogError("Assetbundle reference not set to an instance.");
                }
            }
        }

        assetBundleManager.LoadedBundle();
        Debug.Log(assetBundleManager.loadedObjectCount);

        _objCount = 0;
    }

    void InitTeam()
    {
        Panel[_panelNo].SetActive(true);
        AssetBundleManager.UnloadUnusedAssets();
        _bundleLoaded = true;
        //assetBundleManager.loadedCount = 0;
    }

    public void LoadPlayerPanel()
    {

    }

    public void LoadTeamPanel(GameObject obj)
    {
        _panelNo = 1;
        _name = assetName[1];

        if (!_isLoadTeam)
        {
            LoadAsset("MiceICON/MiceICON", 1);
            LoadAsset(assetName[_panelNo], 1);
        }

        if (!_teamON)
        {
            Panel[1].SetActive(true);
            _teamON = !_teamON;
        }
        else
        {
            Panel[1].SetActive(false);
            _teamON = !_teamON;
        }
    }
}
