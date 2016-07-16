using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AssetLoader : MonoBehaviour
{
    AssetBundleManager assetBundleManager;
    GameObject abm;
    private int _matCount, _objCount;
    public bool loadedObj;
    public string ReturnMessage { get { return _returnMessage; } }
    private string _returnMessage;

    void Awake()
    {
        assetBundleManager = new AssetBundleManager();
        init();
    }

    void Update()
    {
        if (assetBundleManager != null)
            _returnMessage = assetBundleManager.ReturnMessage;

        if (assetBundleManager.loadedObjectCount == _objCount && _objCount != 0)
        {
            assetBundleManager.loadedObjectCount = _objCount = 0;
            loadedObj = true;
        }

        if(assetBundleManager.isLoadPrefab)
            assetBundleManager.init();
    }

    public void LoadAsset(string folderPath, string assetName)
    {
        assetBundleManager = new AssetBundleManager();
        assetBundleManager.init();

        StartCoroutine(assetBundleManager.LoadAtlas(folderPath + assetName, typeof(Texture)));
        StartCoroutine(assetBundleManager.LoadAtlas(folderPath + assetName, typeof(Material)));
        StartCoroutine(assetBundleManager.LoadAtlas(folderPath + assetName, typeof(GameObject)));
    }

    public void LoadPrefab(string folderPath, string assetName)    // 載入遊戲物件
    {
        loadedObj = false;
        _objCount++;
        StartCoroutine(assetBundleManager.LoadGameObject(folderPath + assetName, typeof(GameObject)));
    }


    public GameObject GetAsset(string folderPath, string assetName)
    {
        if (assetBundleManager.bLoadedAssetbundle(folderPath+assetName))
            return AssetBundleManager.getAssetBundle(folderPath + assetName).mainAsset as GameObject;
        return null;
    }

    public void init()
    {
        _matCount = _objCount = 0;
        loadedObj = false;
    }






}
