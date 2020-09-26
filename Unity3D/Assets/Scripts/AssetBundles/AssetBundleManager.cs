using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.Networking;
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
* 負責資產管理
* ***************************************************************
*                          ChangeLog
* v0.0.2 20160629  fixbug: loaded bundle check.
* v0.0.1 20150000  AssetbundleManager publish.    
* ***************************************************************/
public static class AssetBundleManager /*: GameSystem*/
{
    public static AssetBundleRequest Request { get; private set; }
    public static bool IsbLoadAtlas { get; private set; } = false;
    public static int Progress { get; private set; } = 0;
    // public static int LoadedABCount { get; private set; } = 0;    // 目前不使用Aseet來確認載入，只使用Gameobject數量確認，如發現錯誤再改回
    public static int LoadedObjectCount { get; private set; } = 0;
    public static string ReturnMessage { get; private set; } = "";
    public static string Ret { get; private set; } = "C000";

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
    private static Dictionary<string, string> dictAssetBundleNameRefs;

    static  AssetBundleManager()/*(MPGame MPGame) : base(MPGame)*/
    {
        if (dictAssetBundleRefs == null)
        {
            dictAssetBundleRefs = new Dictionary<string, AssetBundleRef>();
            dictAssetBundleNameRefs = new Dictionary<string, string>();
        }
    }

    public static void Init()
    {
        Request = null;
        IsbLoadAtlas = false;
        ReturnMessage = "";
        Ret = "C000";
        Progress = 0;
        // LoadedABCount = 0;
        LoadedObjectCount = 0;
    }

    public class AssetBundleRef
    {
        public AssetBundle assetBundle = null;
    }

    /// <summary>
    /// 從manifest載入Assetbundle
    /// </summary>
    /// <param name="manifestPathName">資產名稱路徑(含副檔名)</param>
    /// <returns></returns>
    public static IEnumerator LoadAtlas(string manifestPathName/*, System.Type type*/)
    {
        AssetBundleRef abRef;
        manifestPathName = manifestPathName.ToLower();
        string assetFullPath = @"file:///" + Application.persistentDataPath + "/AssetBundles/" + manifestPathName;

        if (!GetLoadedAssetbundle(manifestPathName))
        {
            while (!Caching.ready)
                yield return null;
            //Debug.Log("LoadAtlas Path:" + manifestPathName);
            using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(assetFullPath))
            {
                yield return www.SendWebRequest();

                ReturnMessage = "正再載入資源... ( " + manifestPathName + Global.ext + " )";
                if (www.isNetworkError || www.isHttpError)
                {
                    Ret = "C002";
                    ReturnMessage = "載入資源失敗！ : \n" + manifestPathName + "\n" + "   assetPath:" + assetFullPath + "   bGetAsset: " + GetLoadedAssetbundle(manifestPathName);
                    //Debug.Log(ReturnMessage);

                    throw new Exception(www.error);
                }
                else if (www.isDone)
                {
                    Ret = "C001";
                    ReturnMessage = "載入資源完成" + manifestPathName;
                    //Debug.Log(ReturnMessage);
                    abRef = new AssetBundleRef();
                    abRef.assetBundle = DownloadHandlerAssetBundle.GetContent(www);

                    // 如果資產索引不存在 加入索引
                    if (!dictAssetBundleRefs.ContainsKey(manifestPathName))
                    {
                        string bundleName = Path.GetFileName(manifestPathName).Replace(Global.ext, "");
                        dictAssetBundleRefs.Add(manifestPathName, abRef);
                        dictAssetBundleNameRefs.Add(bundleName.ToLower(), manifestPathName);
                    }
                }
            }
        }
        //   LoadedABCount++;   // 這非常可能導致錯誤 應放在www.Done可是他不會計算多次的IEnumerator累計直 3+3=6 會變成只有3
    }

    /// <summary>
    /// 從manifest載入遊戲物件
    /// </summary>
    /// <param name="manifestPathName">資產名稱路徑(含副檔名)</param>
    /// <returns></returns>
    public static IEnumerator LoadGameObject(string manifestPathName)
    {
        string assetFullPath = @"file:///" + Application.persistentDataPath + "/AssetBundles/" + manifestPathName;
        AssetBundleRef abRef;

        if (!GetLoadedAssetbundle(manifestPathName))
        {
            while (IsbLoadAtlas == false)
                yield return null;

            while (!Caching.ready)
                yield return null;

            using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(assetFullPath))
            {
                Progress = (int)www.downloadProgress * 100;
                yield return www.SendWebRequest();
                Debug.Log("LoadGameObject... " + Progress + " %");
                ReturnMessage = "正再載入遊戲物件... ( " + manifestPathName + " )";
                //Debug.Log("( 2 ) :" + assetName);
                if (www.isNetworkError || www.isHttpError)
                {
                    Ret = "C002";
                    ReturnMessage = "載入遊戲物件失敗！ :" + " assetName:" + manifestPathName + "\n assetPath:" + assetFullPath + "\n" + www.error;
                    Debug.Log("( 3 ) :" + ReturnMessage);
                    throw new Exception(www.error);
                }
                else if (www.isDone)
                {
                    Ret = "C001";
                    ReturnMessage = "載入遊戲物件完成" + "( " + manifestPathName + " )";
                    Debug.Log(ReturnMessage);
                    abRef = new AssetBundleRef();
                    abRef.assetBundle = DownloadHandlerAssetBundle.GetContent(www);

                    if (!dictAssetBundleRefs.ContainsKey(manifestPathName))
                    {
                        string bundleName = Path.GetFileName(manifestPathName).Replace(Global.ext, "");
                        dictAssetBundleRefs.Add(manifestPathName, abRef);
                        dictAssetBundleNameRefs.Add(bundleName, manifestPathName);

                        LoadedObjectCount++;
                        Debug.Log("(AB)_loadedObjectCount:" + LoadedObjectCount);
                    }
                    //  Request = abRef.assetBundle.LoadAssetAsync(manifestPathName);
                    www.Dispose();
                    //Debug.Log("( 4 ) :" + assetName);

                }
            }
        }
        else // 已經載入了 不須載入
        {
            Debug.Log("(AB_Already)_loadedObjectCount:" + LoadedObjectCount);
        }

    }

    /// <summary>
    /// 取得已載入資產 (Path + AssetName)
    /// </summary>
    /// <param name="assetName">資產名稱(不含路徑、副檔名)</param>
    /// <returns></returns>
    public static bool GetLoadedAssetbundle(string assetName)
    {
        assetName = assetName.Replace(" ", "");

        if (!string.IsNullOrEmpty(assetName))
        {
            return dictAssetBundleRefs.ContainsKey(assetName) ? true : false;
        }
        return false;
    }


    /// <summary>
    /// 使用物件名稱 取得 物件名稱路徑
    /// </summary>
    /// <param name="assetName">物件名稱(不含附檔名)</param>
    /// <returns></returns>
    public static string GetAssetBundleNamePath(string assetName)
    {
        if (!string.IsNullOrEmpty(assetName))
        {
            return dictAssetBundleNameRefs.TryGetValue(assetName, out assetName) ? assetName : "";
        }
        return "";
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
            msg += "\n" + item.Key.ToString();

        Debug.Log("AssetBundles in Dictinary: " + msg + "\n");
    }

    /// <summary>
    /// 移除載入的AssetBundle
    /// </summary>
    /// <param name="assetNamePath">物件名稱路徑</param>
    /// <param name="type"></param>
    /// <param name="unloadAllObject">移除所有載入的物件</param>
    public static void Unload(string assetNamePath, System.Type type, bool unloadAllObject)  // 錯誤 assetName 應該是URL
    {
        assetNamePath = assetNamePath.Replace(" ", "").ToLower();

        if (dictAssetBundleRefs.TryGetValue(assetNamePath, out AssetBundleRef abRef))
        {
            abRef.assetBundle.Unload(unloadAllObject);
            abRef.assetBundle = null;
            dictAssetBundleRefs.Remove(assetNamePath);
            GC.Collect();
        }
    }

    ///// <summary>
    ///// Split Path and Name 分開assetbundle和Path(沒用到可以刪除)
    ///// string[0]=Path
    ///// string[1]=assetName
    ///// </summary>
    ///// <param name="manifestAssetName"></param>
    ///// <returns></returns>
    //private static string[] SplitPathName(string manifestAssetName)
    //{
    //    manifestAssetName = manifestAssetName.Replace(" ", "");
    //    string[] path = manifestAssetName.Split(new char[] { '/' });
    //    string assetName = path[path.Length - 1];
    //    string folderPath = "";
    //    string[] assetPathName = new string[2];

    //    for (int i = 0; i < path.Length - 1; i++)
    //    {
    //        folderPath += path[i] + "/";
    //    }
    //    assetPathName[0] = folderPath;
    //    assetPathName[1] = assetName;
    //    return assetPathName;
    //}

    /// <summary>
    /// AssetLoader告知Manager已完成全部Asset預先載入動作
    /// </summary>
    /// 
    public static void LoadedAllAsset()
    {
        IsbLoadAtlas = true;
    }

    //public string GetReturnMessage()
    //{
    //    return ReturnMessage;
    //}

    public static void UnloadUnusedAssets()
    {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }
}
