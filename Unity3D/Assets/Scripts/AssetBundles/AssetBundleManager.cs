﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class AssetBundleManager
{
    public AssetBundleRequest request { get { return _request; } }
    public bool isLoadAtlas { get { return _isLoadAtlas; } }
    public bool isLoadMat { get { return _isLoadMat; } }
    public bool isLoadPrefab { get { return _isLoadPrefab; } }
    public bool isLoadObject { get { return _isLoadObject; } set { _isLoadObject = value;} }
    public bool isStartLoadAsset { get { return _isStartLoadAsset; } }
    public string ReturnMessage { get { return _ReturnMessage; } }
    public string Ret { get { return _Ret; } }
    public int progress { get { return _progress; } }
    public int loadedCount { get { return _loadedCount; } set { _loadedCount = value; } }

    private AssetBundleRequest _request = null;
    private static WWW www = null;
    private bool _isLoadAtlas = false;
    private bool _isLoadMat = false;
    private bool _isLoadPrefab = false;
    private bool _isLoadObject = false;
    private bool _isStartLoadAsset = false;
    private string _ReturnMessage = "";
    private string _Ret = "C000";
    private int _progress = 0;
    private int _loadedCount = 0;

    public static Dictionary<string, AssetBundleRef> dictAssetBundleRefs;

    static AssetBundleManager()
    {
        dictAssetBundleRefs = new Dictionary<string, AssetBundleRef>();
    }

    public void init()
    {
        _request = null;
        www = null;
        _isLoadAtlas = false;
        _isLoadMat = false;
        _isLoadPrefab = false;
        _isLoadObject = false;
        _isStartLoadAsset = false;
        _ReturnMessage = "";
        _Ret = "C000";
        _progress = 0;
        _loadedCount = 0;
    }

    public class AssetBundleRef
    {
        public AssetBundle assetBundle = null;
    };

    public IEnumerator LoadAtlas(string assetName, System.Type type)
    {
        AssetBundleRef abRef;
        if (!dictAssetBundleRefs.TryGetValue(assetName, out abRef))
        {
            string fileName = "";

            if (type == typeof(Texture)) fileName = assetName + "Atlas";

            if (type == typeof(Material))
            {
                fileName = assetName + "Mat";
                while (_isLoadAtlas == false)
                    yield return null;
            }
            if (type == typeof(GameObject))
            {
                fileName = assetName + "Prefab";
                while (_isLoadMat == false)
                    yield return null;
            }

            _isStartLoadAsset = true;
            Debug.Log("New Path:" + Application.persistentDataPath + "/AssetBundles/" + fileName + Global.ext);
            www = WWW.LoadFromCacheOrDownload("file:///" + Application.persistentDataPath + "/AssetBundles/" + fileName + Global.ext, 1);
            yield return www;

            _ReturnMessage = "正再載入資源... ( " + fileName + Global.ext + " )";
            if (www.error != null)
            {
                _Ret = "C002";
                _ReturnMessage = "載入資源失敗！ : \n" + www.error;
                throw new Exception(www.error);
            }
            else if (www.isDone)
            {
                _Ret = "C001";
                _ReturnMessage = "載入資源完成" + fileName;
                abRef = new AssetBundleRef();
                abRef.assetBundle = www.assetBundle;
                dictAssetBundleRefs.Add(fileName, abRef);
                string[] asset = fileName.Split('/');
                AssetBundleRequest request = abRef.assetBundle.LoadAsync(asset[1], type);
                if (type == typeof(Texture)) _isLoadAtlas = true;
                else if (type == typeof(Material)) _isLoadMat = true;
                else if (type == typeof(GameObject)) _isLoadPrefab = true;
                www.Dispose();
            }
        }
        else
        {
            if (type == typeof(Texture)) _isLoadAtlas = true;
            else if (type == typeof(Material)) _isLoadMat = true;
            else if (type == typeof(GameObject)) _isLoadPrefab = true;
        }
    }


    public IEnumerator LoadGameObject(string assetName, System.Type type)
    {
        //Debug.Log("( 1 ) :" + assetName);
        AssetBundleRef abRef;
        if (!dictAssetBundleRefs.TryGetValue(assetName, out abRef))
        {
            while (_isLoadPrefab == false)
                yield return null;
            _isStartLoadAsset = true;
            //Debug.Log("(2)New Path:" + Application.persistentDataPath + "/AssetBundles/" + assetName + Global.ext);
            WWW www = WWW.LoadFromCacheOrDownload("file:///" + Application.persistentDataPath + "/AssetBundles/" + assetName + Global.ext, 1);
            _progress = (int)www.progress * 100;
            yield return www;
            try
            {

            
            _ReturnMessage = "正再載入遊戲物件... ( " + assetName + Global.ext + " )";
            //Debug.Log("( 2 ) :" + assetName);
            if (www.error != null)
            {
                _Ret = "C002";
                _ReturnMessage = "載入遊戲物件失敗！ : \n" + www.error;
                //Debug.Log("( 3 ) :" + assetName);
            }
            else if (www.isDone)
            {
                _Ret = "C001";
                _ReturnMessage = "載入遊戲物件完成" + "( " + assetName + " )";
                abRef = new AssetBundleRef();
                abRef.assetBundle = www.assetBundle;
                dictAssetBundleRefs.Add(assetName, abRef);
                string[] asset = assetName.Split('/');
                _request = abRef.assetBundle.LoadAsync(asset[1], type);
                _isLoadObject = true;
                _loadedCount++;
                www.Dispose();
                //Debug.Log("( 4 ) :" + assetName);
            }
            }
            catch(Exception e)
            {
                throw e;
            }
        }
        else // 已經載入了 不須載入
        {
            _isLoadObject = true;
            _loadedCount++;
        }
    }

    public static void Unload(string assetName, System.Type type, bool allObjects)
    {
        string fileName = "";

        if (type == typeof(Texture)) fileName = assetName + "Atlas";
        else if (type == typeof(Material)) fileName = assetName + "Mat";
        else if (type == typeof(GameObject)) fileName = assetName + "Prefab";

        AssetBundleRef abRef;

        if (dictAssetBundleRefs.TryGetValue(fileName, out abRef))
        {
            abRef.assetBundle.Unload(allObjects);
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