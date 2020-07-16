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
* 20160711 v1.0.0  新增載入資產                        
* ****************************************************************/
public class AssetLoader : MonoBehaviour
{
    private int _objCount = 0, _loadedCount = 0;
    public bool loadedObj;
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

        if (_objCount != 0)
        {
            if (AssetBundleManager.LoadedObjectCount == _objCount) // 全部沒有載入的情況  如果AB載入完成數量=載入數量
            {
                AssetBundleManager.LoadedObjectCount = _objCount = 0;
                loadedObj = true;
                if (AssetBundleManager.IsLoadPrefab) AssetBundleManager.init();
            }
            else if ((_objCount - _loadedCount) == AssetBundleManager.LoadedObjectCount && AssetBundleManager.LoadedObjectCount != 0) // 部分AB已載入的情況  載入數量-已載入數量 = AB載入完成數量
            {
                AssetBundleManager.LoadedObjectCount = _objCount = 0;
                loadedObj = true;
                if (AssetBundleManager.IsLoadPrefab) AssetBundleManager.init();
            }
            else if (_objCount == _loadedCount)  // 全部已載入
            {
                AssetBundleManager.LoadedObjectCount = _objCount = 0;
                loadedObj = true;
                if (AssetBundleManager.IsLoadPrefab) AssetBundleManager.init();
            }
            //else
            //{
            //    Debug.Log("(Else) _objCount:" + _objCount + " AssetBundleManager.loadedObjectCount:" + AssetBundleManager.loadedObjectCount + "  _loadedCount:" + _loadedCount);
            //}
        }
    }

    public void LoadAssetFormManifest(string manifestAssetName)
    {
        // 載入Mainfest
        manipath = Application.persistentDataPath + "/AssetBundles/";
        if (assetBundle == null)
        {
            assetBundle = AssetBundle.LoadFromFile(Path.Combine(manipath, "AndroidBundles"));
            manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }

        dependenciesManifestAssetPath = manifest.GetAllDependencies(manifestAssetName);

        AssetBundleManager.init();

        if (!AssetBundleManager.bLoadedAssetbundle(manifestAssetName))
        {
            // 載入Mainfest 中GameObject Dependencies物件
            foreach (string dependencyManifestAssetPath in dependenciesManifestAssetPath)
            {
                if (!atlasNamePath.Contains(dependencyManifestAssetPath))
                {
                    atlasNamePath.Add(dependencyManifestAssetPath);
                    LoadAsset(dependencyManifestAssetPath);
                }
            }

            // 載入遊戲物件
            LoadPrefab(manifestAssetName);
        }
        else
        {
            _loadedCount++;
        }
        _objCount++;
        Debug.Log("_objCount:"+ _objCount);
    }


    private void LoadAsset(string manifestAssetName)
    {
        AssetBundleManager.init();
        try
        {
            /*_gameLoop.*/
            StartCoroutine(AssetBundleManager.LoadAtlas(manifestAssetName, typeof(GameObject)));
        }
        catch
        {
            throw;
        }

    }

    private  void LoadPrefab(string manifestAssetName)    // 載入遊戲物件
    {
        loadedObj = false;
        //if (!AssetBundleManager.bLoadedAssetbundle(manifestAssetName))
        //{
            /* _gameLoop.*/
            StartCoroutine(AssetBundleManager.LoadGameObject(manifestAssetName, typeof(GameObject)));
        //}
        //else
        //{
        //    _loadedCount++;
        //}
        //_objCount++;
    }

    /// <summary>
    /// 取得載入資產
    /// </summary>
    /// <param name="assetName"></param>
    /// <returns>GameObject</returns>
    public GameObject GetAsset(string assetName)
    {
        AssetBundle ab;
        if (AssetBundleManager.bLoadedAssetbundle(assetName))
        {
            ab = AssetBundleManager.getAssetBundle(assetName);
            return ab.LoadAsset(ab.GetAllAssetNames()[0]) as GameObject;  // 2019新版寫法
        }
        return null;
    }


    public void init()
    {
        atlasNamePath = new List<string>();
        _objCount = _loadedCount = 0;
        loadedObj = false;
        AssetBundleManager.init();


    }






}
