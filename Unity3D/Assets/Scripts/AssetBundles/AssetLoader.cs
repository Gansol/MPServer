using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
* 20200822 v2.0.0  更新為Unity2019
* 20160711 v1.0.0  新增載入資產           
* ****************************************************************/
public class AssetLoader : MonoBehaviour
{
    private int _objCount = 0, _loadedCount = 0;
    public bool bLoadedObj; // 外部呼叫用，確認物件已經載入完成
    private bool bPreLoad;
    public string ReturnMessage { get { return _returnMessage; } }
    private string _returnMessage;
    private GameLoop _gameLoop;
    private string[] dependenciesManifestAssetPath;
    private AssetBundleManifest manifest;
    private AssetBundle assetBundle;
    private string manipath;
    private List<string> atlasNamePath;

    public AssetLoader(/*GameLoop gameLoop*/)
    {

        init();
        // _gameLoop = gameLoop;
        //Debug.Log( GameObject.Find("GameLoop").name);
        // Debug.Log(gameLoop.name);
    }

    private void Update()
    {
        
        _returnMessage = AssetBundleManager.ReturnMessage;

        // 如果有載入物件
        if (_loadedCount + _objCount != 0)
        {
            if (AssetBundleManager.LoadedObjectCount == _objCount && _objCount != 0)  // 新載入的物件數量 = 需要載入的物件數量 ---> 完成全部物件第一次載入
            {
                Debug.Log("(1)全部物件第一次載入 AssetBundleManager.LoadedObjectCount == _objCount");
                init();
                bLoadedObj = true;
            }
            else if (AssetBundleManager.LoadedObjectCount == (_objCount - _loadedCount)) // 新載入的物件數量 = ( 需要載入的資產數量 - 已經存在載入的資源 ) ---> 完成部分物件第一次載入
            {
                Debug.Log("(2)部分物件已經載入 AssetBundleManager.LoadedObjectCount = _objCount - _loadedCount");
                init();
                bLoadedObj = true;
            }
            else if (bPreLoad && _objCount == 0 && _loadedCount > 0) // 已經存在載入的資源 且 需要載入的資產數量 > 0  ---> 全部資源已經存在 完成載入資源
            {
                Debug.Log("(3)所有物件已經載入過 AssetBundleManager.LoadedObjectCount == _loadedCount");
                init();
                bLoadedObj = true;
            }
        }
    }

    /// <summary>
    /// 從Manifest載入資產
    /// </summary>
    /// <param name="manifestAssetName">資產名稱路徑(含副檔名)</param>
    public void LoadAssetFormManifest(string manifestAssetName)
    {
        try
        {
            if (!string.IsNullOrEmpty(manifestAssetName))
            {
                // 載入Mainfest
                manipath = Application.persistentDataPath + "/AssetBundles/";
                manifestAssetName = manifestAssetName.ToLower();
                if (assetBundle == null)
                {
                    assetBundle = AssetBundle.LoadFromFile(Path.Combine(manipath, "AndroidBundles"));
                    manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                }

                dependenciesManifestAssetPath = manifest.GetAllDependencies(manifestAssetName);

                AssetBundleManager.Init();

                if (!AssetBundleManager.GetLoadedAssetbundle(manifestAssetName))
                {
                    // 載入Mainfest 中GameObject Dependencies資產物件
                    foreach (string dependencyManifestAssetPath in dependenciesManifestAssetPath)
                    {
                        if (!atlasNamePath.Contains(dependencyManifestAssetPath))
                        {
                            atlasNamePath.Add(dependencyManifestAssetPath);
                            LoadAsset(dependencyManifestAssetPath);
                        }
                    }
                    AssetBundleManager.LoadedAllAsset();    // 告知Manager已載入全部資產數量

                    // 載入遊戲物件
                    LoadPrefab(manifestAssetName);
                    _objCount++;
                    Debug.Log("_objCount:" + _objCount);
                }
                else
                {
                    // 物件已經載入
                    _loadedCount++;
                    Debug.Log("_loadedCount:" + _loadedCount);
                }
                bPreLoad = true;
            }
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// 載入資產
    /// </summary>
    /// <param name="manifestAssetName">資產名稱路徑(含附檔名)</param>
    private void LoadAsset(string manifestAssetName)
    {
        /*_gameLoop.*/
        AssetBundleManager.Init();
        StartCoroutine(AssetBundleManager.LoadAtlas(manifestAssetName.ToLower()/*, typeof(GameObject)*/));
    }

    /// <summary>
    /// 載入遊戲物件
    /// </summary>
    /// <param name="manifestAssetName">資產名稱路徑(含附檔名)</param>
    private void LoadPrefab(string manifestAssetName)
    {
        bLoadedObj = false;
        /* _gameLoop.*/
        StartCoroutine(AssetBundleManager.LoadGameObject(manifestAssetName.ToLower()));
    }

    /// <summary>
    /// 取得載入資產
    /// </summary>
    /// <param name="assetName">物件名稱(不含路徑、副檔名)</param>
    /// <returns>GameObject</returns>
    public GameObject GetAsset(string assetName)
    {
        AssetBundle ab;
        assetName = AssetBundleManager.GetAssetBundleNamePath(assetName.ToLower());

        if (AssetBundleManager.GetLoadedAssetbundle(assetName))
        {
            ab = AssetBundleManager.getAssetBundle(assetName);
            return ab.LoadAsset(ab.GetAllAssetNames()[0]) as GameObject;  // GetAllAssetNames()[0] 第一個資產prefeb  (因為現在可以有很多同名資產在一個Bundle)
        }
        return null;
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void init()
    {
        Debug.Log("--------------- Asset Loader Init ! ----------------");
        atlasNamePath = new List<string>();
        _objCount = _loadedCount = 0;
        bLoadedObj = bPreLoad = false;
        AssetBundleManager.Init();
    }
}
