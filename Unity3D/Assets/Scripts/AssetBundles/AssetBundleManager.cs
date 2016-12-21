using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
/* ***************************************************************
 * -----Copyright © 2015 Gansol Studio.  All Rights Reserved.-----
 * -----------            CC BY-NC-SA 4.0            -------------
 * -----------  @Website:  EasyUnity@blogspot.com    -------------
 * -----------  @Email:    GansolTW@gmail.com        -------------
 * -----------  @Author:   Krola.                    -------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * 負責資產管理
 * ***************************************************************
 *                          ChangeLog
 * v0.0.2 20160629  fixbug: loaded bundle check.
 * v0.0.1 20150000  AssetbundleManager publish.    
 * ***************************************************************/
public static class AssetBundleManager
{
    public static AssetBundleRequest request { get { return _request; } }
    public static bool isLoadAtlas { get { return _isLoadAtlas; } }
    public static bool isLoadMat { get { return _isLoadMat; } }
    public static bool isLoadPrefab { get { return _isLoadPrefab; } }
    public static bool isLoadObject { get { return _isLoadObject; } set { _isLoadObject = value; } }
    public static bool isStartLoadAsset { get { return _isStartLoadAsset; } }
    public static string ReturnMessage { get { return _ReturnMessage; } }
    public static string Ret { get { return _Ret; } }
    public static int progress { get { return _progress; } }
    public static int loadedABCount { get { return _loadedABCount; } set { _loadedABCount = value; } }
    public static int loadedObjectCount { get { return _loadedObjectCount; } set { _loadedObjectCount = value; } }

    private static AssetBundleRequest _request;
    private static bool _isLoadAtlas = false;
    private static bool _isLoadMat = false;
    private static bool _isLoadPrefab = false;
    private static bool _isLoadObject = false;
    private static bool _isStartLoadAsset = false;
    private static string _ReturnMessage = "";
    private static string _Ret = "C000";
    private static int _progress = 0;
    private static int _loadedABCount = 0;
    private static int _loadedObjectCount = 0;

    public static Dictionary<string, AssetBundleRef> dictAssetBundleRefs;

    static AssetBundleManager()
    {
        dictAssetBundleRefs = new Dictionary<string, AssetBundleRef>();
    }

    public static void init()
    {
        _request = null;
        _isLoadAtlas = false;
        _isLoadMat = false;
        _isLoadPrefab = false;
        _isLoadObject = false;
        _isStartLoadAsset = false;
        _ReturnMessage = "";
        _Ret = "C000";
        _progress = 0;
        _loadedABCount = 0;
        loadedObjectCount = 0;
    }

    public class AssetBundleRef
    {
        public AssetBundle assetBundle = null;
    };

    public static IEnumerator LoadAtlas(string folder, string assetName, System.Type type)
    {
        AssetBundleRef abRef;
        string assetPath = folder + assetName;
        bool chkAtlas, chkMat, chkPrefab;
        chkAtlas = dictAssetBundleRefs.TryGetValue(assetName + "Atlas", out abRef);
        chkMat = dictAssetBundleRefs.TryGetValue(assetName + "Mat", out abRef);
        chkPrefab = dictAssetBundleRefs.TryGetValue(assetName + "Prefab", out abRef);

        if (!chkAtlas && !chkMat && !chkPrefab)
        {
            string fileName = "";
            /*
            if (type == typeof(Texture)) fileName = assetName + "Atlas";

            if (type == typeof(Material))
            {
                fileName = assetName + "Mat";
                while (_isLoadAtlas == false)
                    yield return null;
            }*/
            if (type == typeof(GameObject))
            {
                fileName = assetName + "Prefab";
                //while (_isLoadMat == false)
                //yield return null;
            }
            if (!bLoadedAssetbundle(fileName))
            {
                assetPath = folder + fileName;
                _isStartLoadAsset = true;
                while (!Caching.ready)
                    yield return null;
                //Debug.Log("LoadAtlas Path:" + Application.persistentDataPath + "/AssetBundles/" + fileName + Global.ext);
                WWW www = WWW.LoadFromCacheOrDownload("file:///" + Application.persistentDataPath + "/AssetBundles/" + assetPath + Global.ext,Global.bundleVersion);
                yield return www;

                _ReturnMessage = "正再載入資源... ( " + assetPath + Global.ext + " )";
                if (www.error != null)
                {
                    _Ret = "C002";
                    _ReturnMessage = "載入資源失敗！ : \n" + assetPath + "\n";
                    Debug.Log(_ReturnMessage);
                    foreach (KeyValuePair<string, AssetBundleRef> item in dictAssetBundleRefs) // 查看載入物件
                    {
                        Debug.LogError("AB DICT: " + item.Key.ToString());
                    }

                    Debug.LogError("assetName:" + assetPath + "   Get AB: " + bLoadedAssetbundle(assetName));
                    throw new Exception(www.error);
                }
                else if (www.isDone)
                {
                    _Ret = "C001";
                    _ReturnMessage = "載入資源完成" + assetPath;
                    abRef = new AssetBundleRef();
                    abRef.assetBundle = www.assetBundle;
                    dictAssetBundleRefs.Add(fileName, abRef);
                    //AssetBundleRequest request = abRef.assetBundle.LoadAsync(fileName, type);
                    if (type == typeof(Texture)) _isLoadAtlas = true;
                    else if (type == typeof(Material)) _isLoadMat = true;
                    else if (type == typeof(GameObject)) _isLoadPrefab = true;
                    // www.Dispose(); 如果發現網路吃很大要補回修改新方法
                }
            }
            else
            {
                if (type == typeof(Texture)) _isLoadAtlas = true;
                else if (type == typeof(Material)) _isLoadMat = true;
                else if (type == typeof(GameObject)) _isLoadPrefab = true;
            }
        }
        else
        {
            if (type == typeof(Texture)) _isLoadAtlas = true;
            else if (type == typeof(Material)) _isLoadMat = true;
            else if (type == typeof(GameObject)) _isLoadPrefab = true;
        }
        _loadedABCount++;   // 這非常可能導致錯誤 應放在www.Done可是他不會計算多次的IEnumerator累計直 3+3=6 會變成只有3
    }


    public static IEnumerator LoadGameObject(string folder, string assetName, System.Type type)   // 錯誤 要加一個 floder
    {
        //Debug.Log("( 1 ) :" + assetName);
        AssetBundleRef abRef;
        string assetPath = folder + assetName;
        if (!bLoadedAssetbundle(assetName))
        {
            while (_isLoadPrefab == false)
                yield return null;

            while (!Caching.ready)
                yield return null;
            _isStartLoadAsset = true;
            //Debug.Log("(2)New Path:" + Application.persistentDataPath + "/AssetBundles/" + assetName + Global.ext);
            WWW www = WWW.LoadFromCacheOrDownload("file:///" + Application.persistentDataPath + "/AssetBundles/" + assetPath + Global.ext, Global.bundleVersion);
            _progress = (int)www.progress * 100;
            yield return www;

            _ReturnMessage = "正再載入遊戲物件... ( " + assetPath + Global.ext + " )";
            //Debug.Log("( 2 ) :" + assetName);
            if (www.error != null)
            {
                _Ret = "C002";
                _ReturnMessage = "載入遊戲物件失敗！ :" + assetPath + "\n" + www.error;
                Debug.Log("( 3 ) :" + _ReturnMessage);
                throw new Exception(www.error);
            }
            else if (www.isDone)
            {
                _Ret = "C001";
                _ReturnMessage = "載入遊戲物件完成" + "( " + assetPath + " )";
                abRef = new AssetBundleRef();
                abRef.assetBundle = www.assetBundle;
                dictAssetBundleRefs.Add(assetName, abRef);
                _request = abRef.assetBundle.LoadAsync(assetName, type);
                _isLoadObject = true;

                www.Dispose();
                //Debug.Log("( 4 ) :" + assetName);
            }
        }
        else // 已經載入了 不須載入
        {
            _isLoadObject = true;
        }

        _loadedObjectCount++;// 這非常可能導致錯誤 應放在www.Done可是他不會計算多次的IEnumerator累計直 3+3=6 會變成只有3
    }

    /// <summary>
    /// 取得已載入資產
    /// </summary>
    /// <param name="assetName"></param>
    /// <returns></returns>
    public static bool bLoadedAssetbundle(string assetName)
    {
        if (!string.IsNullOrEmpty(assetName))
        {
            AssetBundleRef abRef;
            return dictAssetBundleRefs.TryGetValue(assetName, out abRef) ? true : false;
        }

        return false;
    }

    /// <summary>
    /// 取得已載入AssetBundle
    /// </summary>
    /// <param name="assetName">"資產名稱"</param>
    /// <returns></returns>
    public static AssetBundle getAssetBundle(string assetName)
    {
        AssetBundleRef abRef;
        if (dictAssetBundleRefs.TryGetValue(assetName, out abRef))
            return abRef.assetBundle;
        else
            return null;
    }

    /// <summary>
    /// 查看已載入的AssetBundle
    /// </summary>
    public static void LoadedBundle()
    {
        string msg = "";
        foreach (KeyValuePair<string, AssetBundleRef> item in dictAssetBundleRefs) // 查看載入物件
        {
            msg += "\n" + item.Key.ToString();
        }
        Debug.Log("AssetBundles in Dictinary: " + msg + "\n");
    }

    /// <summary>
    /// 移除載入的AssetBundle
    /// </summary>
    /// <param name="assetName">物件名稱</param>
    /// <param name="type"></param>
    /// <param name="unloadAllObject">移除所有載入的物件</param>
    public static void Unload(string assetName, System.Type type, bool unloadAllObject)  // 錯誤 assetName 應該是URL
    {
        string fileName = "";

        if (type == typeof(Texture)) fileName = assetName + "Atlas";
        else if (type == typeof(Material)) fileName = assetName + "Mat";
        else if (type == typeof(GameObject)) fileName = assetName + "Prefab";

        AssetBundleRef abRef;

        if (dictAssetBundleRefs.TryGetValue(fileName, out abRef))
        {
            abRef.assetBundle.Unload(unloadAllObject);
            abRef.assetBundle = null;
            dictAssetBundleRefs.Remove(fileName);
        }
    }

    public static void UnloadUnusedAssets()
    {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }
}
