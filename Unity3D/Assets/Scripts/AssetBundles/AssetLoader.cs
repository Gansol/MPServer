using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/* ***************************************************************
 * -----Copyright © 2015 Gansol Studio.  All Rights Reserved.-----
 * -----------            CC BY-NC-SA 4.0            -------------
 * -----------  @Website:  EasyUnity@blogspot.com    -------------
 * -----------  @Email:    GansolTW@gmail.com        -------------
 * -----------  @Author:   Krola.                    -------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * 負責載入資產、取得載入的資產 
 * AssetBundleManager <-- AssetLoader
 * ***************************************************************
 *                           ChangeLog
 * 20160711 v1.0.0  新增載入資產                        
 * ****************************************************************/
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
        {
            _returnMessage = assetBundleManager.ReturnMessage;

            if (assetBundleManager.loadedObjectCount == _objCount && _objCount != 0)
            {
                assetBundleManager.loadedObjectCount = _objCount = 0;
                loadedObj = true;
            }

            if (assetBundleManager.isLoadPrefab)
                assetBundleManager.init();
        }
    }

    public void LoadAsset(string folderPath, string assetName)
    {
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
