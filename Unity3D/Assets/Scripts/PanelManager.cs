using UnityEngine;
using System.Collections.Generic;
using System.IO;
using MiniJSON;

public class PanelManager : MonoBehaviour
{
    AssetBundleManager assetBundleManager;
    Dictionary<string, object> dictTeam = null;
    public string pathFile;
    public GameObject[] Panel;
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
    private int i = 0;

    void Awake()
    {
        assetBundleManager = new AssetBundleManager();
    }

    public void LoadPlayerPanel()
    {

    }

    public void LoadTeamPanel()
    {
        if (!_isLoadTeam)
        {
            LoadAsset(assetName[1], 1);
            _isLoadTeam = true;
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

    void LoadAsset(string assetName, int fileNum)
    {

        StartCoroutine(assetBundleManager.LoadAtlas(assetName, typeof(Texture)));
        StartCoroutine(assetBundleManager.LoadAtlas(assetName, typeof(Material)));
        StartCoroutine(assetBundleManager.LoadAtlas(assetName, typeof(GameObject)));

        this.fileNum = fileNum;
        this._name = assetName;

    }

    void Update()
    {

        if (assetBundleManager.isLoadPrefab && !_objetLoaded)
        {
            StartCoroutine(assetBundleManager.LoadGameObject(_name, typeof(GameObject)));
            _objetLoaded = true;
        }

        if (assetBundleManager.isLoadObject && !_bundleLoaded)
        {
            initTeam();
            LoadTeamData();
        }

        if (assetBundleManager.isStartLoadAsset)
        {
            Debug.Log("訊息：" + assetBundleManager.ReturnMessage);
        }
        
        if (assetBundleManager.loadedCount != 0 && assetBundleManager.loadedCount == 1)
        {
            clone = (GameObject)Instantiate(assetBundleManager.request.asset);
            clone.transform.parent = Panel[1].transform.GetChild(0).GetChild(0).GetChild(0).Find("Mice").GetChild(i);
            clone.name = assetBundleManager.request.asset.name.Remove(assetBundleManager.request.asset.name.Length - 4)+"Mice";
            clone.transform.localPosition = Vector3.zero;
            clone.transform.localScale = Vector3.one;
            AssetBundleManager.UnloadUnusedAssets();
            assetBundleManager.loadedCount = 0;
            _loadObjCount = 0;
            i++;
        }
    }

    void initTeam()
    {
        Panel[fileNum].SetActive(true);
        clone = (GameObject)Instantiate(assetBundleManager.request.asset);
        clone.transform.parent = Panel[fileNum].transform;
        clone.transform.localPosition = Vector3.zero;
        clone.transform.localScale = Vector3.one;
        AssetBundleManager.UnloadUnusedAssets();
        _bundleLoaded = true;
        assetBundleManager.loadedCount = 0;
    }

    void LoadTeamData()
    {
        _isLoadTeamData = true;
        dictTeam = Json.Deserialize(Global.Team) as Dictionary<string, object>;
        assetBundleManager.init();
        StartCoroutine(assetBundleManager.LoadAtlas("MiceICON/MiceICON", typeof(Texture)));
        StartCoroutine(assetBundleManager.LoadAtlas("MiceICON/MiceICON", typeof(Material)));
        StartCoroutine(assetBundleManager.LoadAtlas("MiceICON/MiceICON", typeof(GameObject)));
        //Remove(0,x) > 從第0個開始移除，到第X個字元
        //Reomve(x) >從X開始移除
        foreach (KeyValuePair<string, object> item in dictTeam)
        {
            Debug.Log("XXXXXXXXXX"+item.Value.ToString());
            StartCoroutine(assetBundleManager.LoadGameObject("MiceICON/"+item.Value.ToString().Remove(item.Value.ToString().Length - 4) + "ICON", typeof(GameObject)));
            _loadObjCount++;
        }
       
    }




    public bool _isLoadTeamData { get; set; }
}
