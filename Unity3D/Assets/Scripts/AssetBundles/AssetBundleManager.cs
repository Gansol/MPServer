using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.Networking;
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
    public static bool IsLoadAtlas { get; private set; } = false;
    public static bool IsLoadMat { get { return _isLoadMat; } private set { _isLoadMat = value; } }
    public static bool IsLoadPrefab { get { return _isLoadPrefab; } private set { _isLoadPrefab = value; } }
    public static bool IsLoadObject { get { return _isLoadObject; } set { _isLoadObject = value; } }
    public static bool IsStartLoadAsset { get { return _isStartLoadAsset; } }
    public static string ReturnMessage { get { return _ReturnMessage; } }
    public static string Ret { get { return _Ret; } }
    public static int Progress { get { return _progress; } }
    public static int LoadedABCount { get { return _loadedABCount; } set { _loadedABCount = value; } }
    public static int LoadedObjectCount { get { return _loadedObjectCount; } set { _loadedObjectCount = value; } }

    private static AssetBundleRequest _request;
    private static bool _isLoadMat = false;
    private static bool _isLoadPrefab = false;
    private static bool _isLoadObject = false;
    private static bool _isStartLoadAsset = false;
    private static string _ReturnMessage = "";
    private static string _Ret = "C000";
    private static int _progress = 0;
    private static int _loadedABCount = 0;
    private static int _loadedObjectCount = 0;
    //    private static char[] trimString = new char[] {
    //    // SpaceSeparator category
    //    '\u0020', '\u1680', '\u180E', '\u2000', '\u2001', '\u2002', '\u2003', 
    //    '\u2004', '\u2005', '\u2006', '\u2007', '\u2008', '\u2009', '\u200A', 
    //    '\u202F', '\u205F', '\u3000',
    //    // LineSeparator category
    //    '\u2028',
    //    // ParagraphSeparator category
    //    '\u2029',
    //    // Latin1 characters
    //    '\u0009', '\u000A', '\u000B', '\u000C', '\u000D', '\u0085', '\u00A0',
    //    // ZERO WIDTH SPACE (U+200B) & ZERO WIDTH NO-BREAK SPACE (U+FEFF)
    //    '\u200B', '\uFEFF'
    //};

    public static Dictionary<string, AssetBundleRef> dictAssetBundleRefs;

    static AssetBundleManager()
    {
        if (dictAssetBundleRefs == null)
            dictAssetBundleRefs = new Dictionary<string, AssetBundleRef>();
    }

    public static void init()
    {
        _request = null;
        IsLoadAtlas = false;
        _isLoadMat = false;
        _isLoadPrefab = false;
        _isLoadObject = false;
        _isStartLoadAsset = false;
        _ReturnMessage = "";
        _Ret = "C000";
        _progress = 0;
        _loadedABCount = 0;
        _loadedObjectCount = 0;
    }

    public class AssetBundleRef
    {
        public AssetBundle assetBundle = null;
    }

    public static IEnumerator LoadAtlas(string manifestPathName, System.Type type)
    {

        AssetBundleRef abRef;
        bool chkAtlas;
        string assetFullPath = @"file:///" + Application.persistentDataPath + "/AssetBundles/" + manifestPathName;

        chkAtlas = dictAssetBundleRefs.TryGetValue(manifestPathName, out abRef);

        if (!chkAtlas)
        {
            if (!bLoadedAssetbundle(manifestPathName))
            {
                _isStartLoadAsset = true;
                while (!Caching.ready)
                    yield return null;
                //Debug.Log("LoadAtlas Path:" + Application.persistentDataPath + "/AssetBundles/" + fileName + Global.ext);
                using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(assetFullPath))
                {
                    yield return www.SendWebRequest();

                    _ReturnMessage = "正再載入資源... ( " + manifestPathName + Global.ext + " )";
                    if (www.isNetworkError || www.isHttpError)
                    {
                        _Ret = "C002";
                        _ReturnMessage = "載入資源失敗！ : \n" + manifestPathName + "\n";
                        Debug.Log(_ReturnMessage);
                        //foreach (KeyValuePair<string, AssetBundleRef> item in dictAssetBundleRefs) // 查看載入物件
                        //{
                        //    Debug.LogError("AB DICT: " + item.Key.ToString());
                        //}

                        Debug.LogError("assetName:" + manifestPathName + "   assetPath:" + assetFullPath + "   bGetAsset: " + bLoadedAssetbundle(manifestPathName));
                        throw new Exception(www.error);
                    }
                    else if (www.isDone)
                    {

                        _Ret = "C001";
                        _ReturnMessage = "載入資源完成" + manifestPathName;
                        abRef = new AssetBundleRef();
                        abRef.assetBundle = DownloadHandlerAssetBundle.GetContent(www);
                        dictAssetBundleRefs.Add(manifestPathName, abRef);
                        //AssetBundleRequest request = abRef.assetBundle.LoadAsync(fileName, type);
                        if (type == typeof(GameObject)) _isLoadPrefab = true;
                        // www.Dispose(); 如果發現網路吃很大要補回修改新方法
                    }
                    else if (type == typeof(GameObject))
                    {
                        _isLoadPrefab = true;
                    }
                }
            }
        }
        else if (type == typeof(GameObject))
        {
            _isLoadPrefab = true;
        }
        _loadedABCount++;   // 這非常可能導致錯誤 應放在www.Done可是他不會計算多次的IEnumerator累計直 3+3=6 會變成只有3
    }


    public static IEnumerator LoadGameObject(string manifestPathName, System.Type type)   // 載入遊戲物件
    {
        string assetFullPath = @"file:///" + Application.persistentDataPath + "/AssetBundles/" + manifestPathName;
        AssetBundleRef abRef;

        if (!bLoadedAssetbundle(manifestPathName))
        {
            while (_isLoadPrefab == false)
                yield return null;

            while (!Caching.ready)
                yield return null;
            _isStartLoadAsset = true;
            //Debug.Log("(2)New Path:" + Application.persistentDataPath + "/AssetBundles/" + assetName + Global.ext);

            using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(assetFullPath))
            {
                _progress = (int)www.uploadProgress * 100;
                yield return www.SendWebRequest();
                Debug.Log(_progress);
                _ReturnMessage = "正再載入遊戲物件... ( " + manifestPathName + " )";
                //Debug.Log("( 2 ) :" + assetName);
                if (www.isNetworkError || www.isHttpError)
                {
                    _Ret = "C002";
                    _ReturnMessage = "載入遊戲物件失敗！ :" + " assetName:" + manifestPathName + "\n assetPath:" + assetFullPath + "\n" + www.error;
                    Debug.Log("( 3 ) :" + _ReturnMessage);
                    throw new Exception(www.error);
                }
                else if (www.isDone)
                {
                    _Ret = "C001";
                    _ReturnMessage = "載入遊戲物件完成" + "( " + manifestPathName + " )";
                    abRef = new AssetBundleRef();
                    abRef.assetBundle = DownloadHandlerAssetBundle.GetContent(www);
                    dictAssetBundleRefs.Add(manifestPathName, abRef);
                    _request = abRef.assetBundle.LoadAssetAsync(manifestPathName, type);
                    _isLoadObject = true;

                    www.Dispose();
                    //Debug.Log("( 4 ) :" + assetName);
                }
                else // 已經載入了 不須載入
                {
                    _isLoadObject = true;
                }
            }
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
        assetName = assetName.Replace(" ", "");
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
        assetName = assetName.Replace(" ", "");
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
        assetName = assetName.Replace(" ", "");
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

    /// <summary>
    /// Split Path and Name 分開assetbundle和Path(沒用到可以刪除)
    /// string[0]=Path
    /// string[1]=assetName
    /// </summary>
    /// <param name="manifestAssetName"></param>
    /// <returns></returns>
    private static string[] SplitPathName(string manifestAssetName)
    {
        manifestAssetName = manifestAssetName.Replace(" ","");
        string[] path = manifestAssetName.Split(new char[] { '/' });
        string assetName = path[path.Length - 1];
        string folderPath = "";
        string[] assetPathName =new string[2];

        for (int i = 0; i < path.Length - 1; i++)
        {
            folderPath += path[i] + "/";
        }
        assetPathName[0] = folderPath;
        assetPathName[1] = assetName;
        return assetPathName;
    }

    public static void UnloadUnusedAssets()
    {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }
}
