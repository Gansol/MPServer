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
    private int  _objCount = 0, _loadedCount = 0;
    public bool loadedObj;
    public string ReturnMessage { get { return _returnMessage; } }
    private string _returnMessage;

    private void Awake()
    {
        init();
    }

    private void Update()
    {
        _returnMessage = AssetBundleManager.ReturnMessage;

        if (_objCount != 0)
        {
            if (AssetBundleManager.loadedObjectCount == _objCount) // 全部沒有載入的情況  如果AB載入完成數量=載入數量
            {
                AssetBundleManager.loadedObjectCount = _objCount = 0;
                loadedObj = true;
            }
            else if ((_objCount - _loadedCount) == AssetBundleManager.loadedObjectCount && AssetBundleManager.loadedObjectCount != 0) // 部分AB已載入的情況  載入數量-已載入數量 = AB載入完成數量
            {
                AssetBundleManager.loadedObjectCount = _objCount = 0;
                loadedObj = true;
            }
            else if (_objCount == _loadedCount)  // 全部已載入
            {
                AssetBundleManager.loadedObjectCount = _objCount = 0;
                loadedObj = true;
            }
        }

        if (AssetBundleManager.isLoadPrefab)
            AssetBundleManager.init();

    }

    public void LoadAsset(string folderPath, string assetName)
    {
        AssetBundleManager.init();
        try
        {
            StartCoroutine(AssetBundleManager.LoadAtlas(folderPath, assetName, typeof(GameObject)));
        }
        catch
        {
            throw;
        }

    }

    public void LoadPrefab(string folderPath, string assetName)    // 載入遊戲物件
    {
        loadedObj = false;
        if (!AssetBundleManager.bLoadedAssetbundle(assetName))
        {
            StartCoroutine(AssetBundleManager.LoadGameObject(folderPath, assetName, typeof(GameObject)));
        }
        else
        {
            _loadedCount++;
        }
        _objCount++;
    }

    /// <summary>
    /// 取得載入資產
    /// </summary>
    /// <param name="assetName"></param>
    /// <returns>GameObject</returns>
    public GameObject GetAsset(string assetName)
    {
        if (AssetBundleManager.bLoadedAssetbundle(assetName))
            return AssetBundleManager.getAssetBundle(assetName).mainAsset as GameObject;
        return null;
    }

    public void init()
    {
        _objCount = _loadedCount = 0;
        loadedObj = false;
        AssetBundleManager.init();
    }






}
