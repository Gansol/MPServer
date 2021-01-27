using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
/* ***************************************************************
 * ------------  Copyright © 2021 Gansol Studio.  All Rights Reserved.  ------------
 * ----------------                                CC BY-NC-SA 4.0                                ----------------
 * ----------------                @Website:  EasyUnity@blogspot.com      ----------------
 * ----------------                @Email:    GansolTW@gmail.com               ----------------
 * ----------------                @Author:   Krola.                                             ----------------
 * ***************************************************************
 *                                                       Description
 * ***************************************************************
 *                                                  載入Assetbundle
 *   1.設定所有載入的資產後 需呼叫 SetLoadAllAseetCompleted()
 *   2.當所有資產載入完成後 需呼叫 Initialize()
 * ***************************************************************
 *                                                       ChangeLog
 * 20210109 v1.0.1    修正載入問題                                                 
 * 20210108 v1.0.0    資產載入完成
 * ****************************************************************/
public class AssetLoaderSystem : IGameSystem
{
    #region -- Variables 變數 --
    public string Ret { get; private set; }                               // Debug代碼
    public string ReturnMessage { get; private set; }       // Debug資訊
    public static int Progress { get; private set; }               // 載入進度
    public bool IsLoadAllAseetCompleted { get; set; }     // 是否全部物件載入完成

    private static AssetBundle _assetBundle;                                                                       // 依賴資源包
    private static Dictionary<string, AssetBundleRef> _dictAssetBundleRefs;           // 資產索引字典
    private static Dictionary<string, string> _dictAssetBundleNameRefs;                   // 資產名稱索引字典
    private static List<string> _loadedDependenciesNamePath;                                    // 儲存已經載入的 資產路徑   (可以被取代 現在移除會有問題)

    private int _loadAssetCounter;                                                                                          // 載入資產 計數
    private int _dependenciesAssetCunter;                                                                          // 載入依賴資產 計數
    private int _alreadyLoadDependenciesCount;                                                              // 已經載入的依賴資產數量
    private int _loadedDependenciesAssetCount;                                                             // 目前載入完成的依賴資產數量
    private int _loadedAssetCount;                                                                                         // 載入的資產數量
    private bool _bLoadAllDependenciesAsset;                                                                   // 是否載入全部依賴資產
    private bool _bPreloadAllAseetCompleted;                                                                   // 是否 資產 已經全部載入 (才可以進行下一步載入物件，否則會破圖)
    #endregion

    // 資產索引類別
    public class AssetBundleRef
    {
        public AssetBundle assetBundle = null;
    }

    public AssetLoaderSystem(MPGame MPGame) : base(MPGame)
    {
        Debug.Log("--------------- AssetLoaderSystem Create ----------------");
        _dictAssetBundleRefs = new Dictionary<string, AssetBundleRef>();
        _dictAssetBundleNameRefs = new Dictionary<string, string>();
        _loadedDependenciesNamePath = new List<string>();
        Initialize();
    }

    /// <summary>
    /// ※當所有需要載入的資產完成後呼叫(只能一次)※
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
        Debug.Log("--------------- AssetLoaderSystem Initialize ----------------");
        //Debug.Log("alreadyLoadDependenciesCount :  " + _alreadyLoadDependenciesCount + "  _loadedDependenciesAssetCount:   " + _loadedDependenciesAssetCount + "  _allDependenciesAssetCount:   " + _dependenciesAssetCunter + " _bLoadAllDependenciesAsset: " + _bLoadAllDependenciesAsset);
        //Debug.Log("_loadAssetCounter:   " + _loadAssetCounter + " _loadedObjCount:  " + _loadedAssetCount + " LoadAllAseetCompleted: " + IsLoadAllAseetCompleted);
        Ret = "C000";
        ReturnMessage = "";

        Progress = 0;
        _loadAssetCounter = 0;
        _dependenciesAssetCunter = 0;
        _alreadyLoadDependenciesCount = 0;
        _loadedDependenciesAssetCount = 0;
        _loadedAssetCount = 0;

        IsLoadAllAseetCompleted = false;
        _bLoadAllDependenciesAsset = false;
        _bPreloadAllAseetCompleted = false;
    }

    public override void Update()
    {
        base.Update();


        // 如果所有依賴數量 = 已經載入依賴資源 + 目前載入完成的依賴資源    
        if ((_dependenciesAssetCunter == _alreadyLoadDependenciesCount + _loadedDependenciesAssetCount) && _dependenciesAssetCunter > 0)
            _bLoadAllDependenciesAsset = true;  // 全部依賴資產已經載入
        // 全部依賴資產已經載入完成 不須載入
        if ((_dependenciesAssetCunter == _alreadyLoadDependenciesCount) && _dependenciesAssetCunter > 0)
            _bLoadAllDependenciesAsset = true;  // 全部依賴資產已經載入

        // 如果所有物件數量 = 已經載入物件 + 目前載入完成的物件
        if ((_loadAssetCounter == _loadedAssetCount) && _bPreloadAllAseetCompleted && _loadedAssetCount > 0)
            IsLoadAllAseetCompleted = true;         // 全部物件載入完成
    }

    #region -- LoadAssetFormManifest 從Manifest載入資產  -- 
    /// <summary>
    /// 從Manifest載入資產
    /// ※當預先要載入Aseet 全部載入完成後呼叫SetLoadAllAseetCompleted(一定要)※
    /// </summary>
    /// <param name="manifestAssetName">資產名稱路徑(含副檔名)</param>
    public void LoadAssetFormManifest(string manifestAssetName)
    {
        try
        {
            if (!string.IsNullOrEmpty(manifestAssetName))
            {
                _loadAssetCounter++;                                                                // 資產計數器會一直累計 直到全部資產載入完成 (需Initialize初始化)
                IsLoadAllAseetCompleted = false;                                           // 只要還在呼叫方法，持續設定尚未載入完成 避免還沒載入完成就true
                manifestAssetName = manifestAssetName.ToLower();


                string manipath = Application.persistentDataPath + "/AssetBundles/";                                            // AssetBundle 路徑

                // 如果找不到 AndroidBundles 載入 依賴資源包(AndroidBundles 路徑)
                if (_assetBundle == null)
                    _assetBundle = AssetBundle.LoadFromFile(Path.Combine(manipath, "AndroidBundles"));  // AndroidBundles 路徑

                // 依賴資源包 遺失
                if (_assetBundle == null)
                {
                    Debug.LogError("Load AssetBundle (AndroidBundles) is Null!");
                    return;
                }

                // 載入AssetBundleManifest
                AssetBundleManifest manifest = _assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                // 取得所有依賴資源路徑
                string[] dependenciesManifestAssetPath = manifest.GetAllDependencies(manifestAssetName);
                // 依賴資產計數器會一直累計 直到全部依賴資產載入完成(需Initialize初始化)
                _dependenciesAssetCunter += dependenciesManifestAssetPath.Length;

                //Debug.Log(manifestAssetName);
                //foreach (string dependencyManifestAssetPath in dependenciesManifestAssetPath)
                //    Debug.Log(dependencyManifestAssetPath);

                // 如果資源不存在
                if (!GetIsLoadedAssetbundle(manifestAssetName))
                {
                    // 載入Mainfest 中GameObject Dependencies資產物件
                    foreach (string dependencyManifestAssetPath in dependenciesManifestAssetPath)
                    {
                        // 如果 依賴資源尚未載入 載入資源並加入已載入列表
                        if (!GetIsLoadedAssetbundle(dependencyManifestAssetPath))
                        {
                            _loadedDependenciesNamePath.Add(dependencyManifestAssetPath);
                            m_MPGame.StartCoroutine(LoadAsset(dependencyManifestAssetPath.ToLower()/*, typeof(GameObject)*/));
                        }
                        else
                        {
                            _alreadyLoadDependenciesCount++;
                            Ret = "C001";
                            ReturnMessage = "(已載入依賴物件): " + dependencyManifestAssetPath;
                            // Debug.Log("(已載入依賴物件): " + dependencyManifestAssetPath+ "  _alreadyLoadDependenciesCount:" + _alreadyLoadDependenciesCount);
                        }
                    }
                    // 載入遊戲物件
                    m_MPGame.StartCoroutine(LoadPrefab(manifestAssetName.ToLower()));
                }
                else
                {
                    // 物件已經載入
                    Ret = "C001";
                    ReturnMessage = "(已載入物件): " + manifestAssetName;
                    _loadedAssetCount++;
                    _alreadyLoadDependenciesCount += dependenciesManifestAssetPath.Length;
                    // Debug.Log("(已載入遊戲物件): " + manifestAssetName + "  _loadedAssetCount: " + _loadedAssetCount);
                }
            }
        }
        catch
        {
            throw;
        }
    }
    #endregion

    #region -- LoadAsset 從manifest載入依賴資產-- 
    /// <summary>
    /// 從manifest載入Assetbundle
    /// </summary>
    /// <param name="manifestPathName">資產名稱路徑(含副檔名)</param>
    /// <returns></returns>
    private IEnumerator LoadAsset(string manifestPathName/*, System.Type type*/)
    {
        manifestPathName = manifestPathName.ToLower();
        string assetFullPath = @"file:///" + Application.persistentDataPath + "/AssetBundles/" + manifestPathName;  // 完整資產路徑

        // 如果資產尚未載入
        if (!GetIsLoadedAssetbundle(manifestPathName))
        {
            // 等待依賴資產載入完成
            while (_bPreloadAllAseetCompleted == false)
                yield return null;

            // 等待 上個www下載完成
            while (!Caching.ready)
                yield return null;

            using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(assetFullPath))
            {
                Progress = (int)www.downloadProgress * 100;         // 載入進度
                ReturnMessage = "正在載入資產... " + Progress + " %  ( " + manifestPathName + " )";
                //Debug.Log("LoadGameObject... " + Progress + " %");
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    Ret = "C002";
                    ReturnMessage = "載入資源失敗！ : \n" + manifestPathName + "\n" + "   assetPath:" + assetFullPath + "   bGetAsset: " + GetIsLoadedAssetbundle(manifestPathName);
                    throw new Exception(www.error);
                }
                else if (www.isDone)
                {
                    // 如果資產索引不存在 加入索引
                    if (!GetIsLoadedAssetbundle(manifestPathName))
                    {
                        Ret = "C001";
                        ReturnMessage = "載入資源完成" + manifestPathName;
                        //Debug.Log(Ret + " " + ReturnMessage);
                        AssetBundleRef abRef = new AssetBundleRef();
                        abRef.assetBundle = DownloadHandlerAssetBundle.GetContent(www);                                                 // 儲存已載入的資產
                        string bundleName = Path.GetFileName(manifestPathName).Replace(Global.ext, "").ToLower();    // 取得檔名
                        _dictAssetBundleRefs.Add(manifestPathName, abRef);                                                                                 // 儲存至資產索引
                        _dictAssetBundleNameRefs.Add(bundleName, manifestPathName);                                                         // 儲存至資產名稱索引
                    }
                    _loadedDependenciesAssetCount++;
                }
            }
        }
        else
        {
            Ret = "C001";
            ReturnMessage = "(Already)載入資源完成" + "( " + manifestPathName + " )";
            //Debug.Log(Ret + " " + ReturnMessage);
            _alreadyLoadDependenciesCount++;
        }
    }
    #endregion

    #region --  LoadPrefab 從manifest載入遊戲物件 -- 
    /// <summary>
    /// 從manifest載入遊戲物件
    /// </summary>
    /// <param name="manifestPathName">資產名稱路徑(含副檔名)</param>
    /// <returns></returns>
    private IEnumerator LoadPrefab(string manifestPathName)
    {
        string assetFullPath = @"file:///" + Application.persistentDataPath + "/AssetBundles/" + manifestPathName;

        if (!GetIsLoadedAssetbundle(manifestPathName))
        {
            // 等待依賴資產載入完成
            while (_bLoadAllDependenciesAsset == false)
                yield return null;
            // 等待 上個www下載完成
            while (!Caching.ready)
                yield return null;

            using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(assetFullPath))
            {
                Progress = (int)www.downloadProgress * 100;         // 載入進度
                ReturnMessage = "正在載入遊戲物件... " + Progress + " %  ( " + manifestPathName + " )";
                //Debug.Log("LoadGameObject... " + Progress + " %");
                yield return www.SendWebRequest();


                if (www.isNetworkError || www.isHttpError)
                {
                    Ret = "C002";
                    ReturnMessage = "載入遊戲物件失敗！ :" + " assetName:" + manifestPathName + "\n assetPath:" + assetFullPath + "\n" + www.error;
                    Debug.Log("( 3 ) :" + ReturnMessage);
                    throw new Exception(www.error);
                }
                else if (www.isDone)
                {
                    if (!GetIsLoadedAssetbundle(manifestPathName))
                    {
                        Ret = "C001";
                        ReturnMessage = "載入遊戲物件完成" + "( " + manifestPathName + " )";
                        //Debug.Log(Ret + " " + ReturnMessage);
                        AssetBundleRef abRef = new AssetBundleRef();
                        abRef.assetBundle = DownloadHandlerAssetBundle.GetContent(www);                                                 // 儲存已載入的資產
                        string bundleName = Path.GetFileName(manifestPathName).Replace(Global.ext, "").ToLower();    // 取得檔名
                        _dictAssetBundleRefs.Add(manifestPathName, abRef);                                                                                 // 儲存至資產索引
                        _dictAssetBundleNameRefs.Add(bundleName, manifestPathName);                                                         // 儲存至資產名稱索引
                    }
                    _loadedAssetCount++;
                    //Debug.Log(ReturnMessage + "  loadedObjectCount:" + _loadedAssetCount);
                }
            }
        }
        else // 已經載入了 不須載入
        {
            Ret = "C001";
            _loadedAssetCount++;
            ReturnMessage = "(Already)載入遊戲物件完成" + "( " + manifestPathName + " )";
            //Debug.Log(Ret + " " + ReturnMessage);
        }
    }
    #endregion

    #region --  GetAssetBundle 取得AssetBundle -- 
    /// <summary>
    /// 取得載入資產
    /// </summary>
    /// <param name="assetName">物件名稱(不含路徑、副檔名)</param>
    /// <returns>GameObject</returns>
    public GameObject GetAsset(string assetName)
    {
        assetName = GetAssetBundleNamePath(assetName.ToLower());

        if (GetIsLoadedAssetbundle(assetName))
        {
            AssetBundle ab = GetAssetBundle(assetName);
            return ab.LoadAsset(ab.GetAllAssetNames()[0]) as GameObject;  // GetAllAssetNames()[0] 第一個資產prefeb  (因為現在可以有很多同名資產在一個Bundle)
        }
        return null;
    }

    /// <summary>
    /// 取得是否已載入資產 (Path + AssetName)
    /// </summary>
    /// <param name="assetName">資產名稱(不含路徑、副檔名)</param>
    /// <returns></returns>
    public bool GetIsLoadedAssetbundle(string assetName)
    {
        if (!string.IsNullOrEmpty(assetName.Replace(" ", "")))
            return _dictAssetBundleRefs.ContainsKey(assetName) ? true : false;
        return false;
    }

    /// <summary>
    /// 取得AssetBundle名稱
    /// </summary>
    /// <param name="assetName">使用物件名稱 取得 物件名稱路徑名稱(不含附檔名)</param>
    /// <returns></returns>
    public string GetAssetBundleNamePath(string assetName)
    {
        if (!string.IsNullOrEmpty(assetName.Replace(" ", "")))
            return _dictAssetBundleNameRefs.TryGetValue(assetName, out assetName) ? assetName : "";
        return "";
    }

    /// <summary>
    /// 取得已載入AssetBundle
    /// </summary>
    /// <param name="assetName">"資產名稱(不含附檔名)"</param>
    /// <returns></returns>
    public AssetBundle GetAssetBundle(string assetName)
    {
        if (_dictAssetBundleRefs.TryGetValue(assetName.Replace(" ", ""), out AssetBundleRef abRef))
            return abRef.assetBundle;
        return null;
    }
    #endregion

    #region -- GetDontNotLoadAsset 取得未載入Asset --
    /// <summary>
    /// 取得未載入Asset
    /// </summary>
    /// <param name="dictAssetData"></param>
    /// <returns></returns>
    public List<string> GetDontNotLoadAssetName(Dictionary<string, object> dictAssetData)
    {
        List<string> notLoadedAssetNameList = new List<string>();

        // 取得未載入物件
        foreach (KeyValuePair<string, object> item in dictAssetData)
        {
            string serverBundleName = item.Value.ToString();
            if (serverBundleName != null)
                if (GetAsset(serverBundleName.ToString()) == null)
                    notLoadedAssetNameList.Add(serverBundleName);
        }
        return notLoadedAssetNameList;
    }

    ///// <summary>
    ///// 取得未載入Asset (nestedDict)
    ///// </summary>
    ///// <param name="dictServer">NewAssetData</param>
    ///// <param name="itemNameData">name Data(OldAssetData)</param>
    ///// <returns></returns>
    //public List<string> GetDontNotLoadAssetName(Dictionary<string, object> dictServer, Dictionary<string, object> itemNameData)
    //{
    //    List<string> notLoadedAssetNameList = new List<string>();
    //    // 取得未載入物件
    //    foreach (KeyValuePair<string, object> item in dictServer)
    //    {
    //        object serverBundleName = MPGFactory.GetObjFactory().GetColumnsDataFromID(itemNameData, "ItemName", item.Key);

    //        if (serverBundleName!=null)
    //            if (GetAsset(serverBundleName.ToString()) == null)
    //            notLoadedAssetNameList.Add(serverBundleName.ToString());
    //    }
    //    return notLoadedAssetNameList;
    //}
    #endregion

    #region -- SetLoadAllAseetCompleted 通知AssetLoader已經準備好全部要載入的Asset -- 
    /// <summary>
    /// 通知AssetLoader已經準備好全部要載入的Asset
    /// </summary>
    public void SetLoadAllAseetCompleted()
    {
        _bPreloadAllAseetCompleted = true;
    }
    #endregion

    #region -- ShowLoadedAsset 查看已載入的AssetBundle -- 
    /// <summary>
    /// 查看已載入的AssetBundle
    /// </summary>
    public void ShowLoadedAsset()
    {
        string msg = "";

        foreach (KeyValuePair<string, AssetBundleRef> item in _dictAssetBundleRefs) // 查看載入物件
            msg += "\n" + item.Key.ToString();
        Debug.Log("AssetBundles in Dictinary: " + msg + "\n");
    }
    #endregion

    #region -- Unload  -- 
    /// <summary>
    /// 移除載入的AssetBundle
    /// </summary>
    /// <param name="assetNamePath">物件名稱路徑</param>
    /// <param name="type"></param>
    /// <param name="unloadAllObject">移除所有載入的物件</param>
    public void Unload(string assetNamePath, System.Type type, bool unloadAllObject)  // 錯誤 assetName 應該是URL
    {
        assetNamePath = assetNamePath.Replace(" ", "").ToLower();

        if (_dictAssetBundleRefs.TryGetValue(assetNamePath, out AssetBundleRef abRef))
        {
            abRef.assetBundle.Unload(unloadAllObject);
            abRef.assetBundle = null;
            _dictAssetBundleRefs.Remove(assetNamePath);
            GC.Collect();
        }
    }

    public void UnloadUnusedAssets()
    {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }
    #endregion

    #region -- Release  -- 
    public override void Release()
    {
        base.Release();
        _dictAssetBundleRefs.Clear();
        _dictAssetBundleNameRefs.Clear();
        _loadedDependenciesNamePath.Clear();
        UnloadUnusedAssets();
    }
    #endregion
}

