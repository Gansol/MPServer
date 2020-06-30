using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
/* ***************************************************************
* -----Copyright © 2020 Gansol Studio.  All Rights Reserved.-----
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
* v0.0.1 20200625  test ver.
* ***************************************************************/
public class NewAssetBundleManager : MonoBehaviour
{
    //Android or iOS or Win 伺服器中的 檔案路徑
    public static readonly string targetPlanform =
#if UNITY_ANDROID
 "/AndroidBundles/"; // "/AndroidBundles/";
#elif UNITY_IPHONE
    "/iOSBundles/";
#else
 string.Empty;
#endif

    string folderPath;
    string manifestBundleName;
    public static Dictionary<string, AssetBundleRef> dictAssetBundleRefs;

    public void Awake()
    {
        folderPath = Application.dataPath + "/_Assetbundles/" + targetPlanform;
        manifestBundleName = "panel/unique/menuui.unity3d";

        LoadAssetFormManifest(folderPath, manifestBundleName);
    }

    private void OnEnable()
    {

    }
    public class AssetBundleRef
    {
        public AssetBundle assetBundle = null;
    }

    public void LoadAssetFormManifest(string folderPath, string manifestAssetName)
    {
        // 載入Mainfest
        AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(folderPath, "AndroidBundles"));
        AssetBundleManifest manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

        string[] dependencies = manifest.GetAllDependencies(manifestAssetName);


        // 載入Mainfest 中GameObject Dependencies物件
        foreach (string dependency in dependencies)
            StartCoroutine(LoadAsset(folderPath, dependency));

        // 載入遊戲物件
        StartCoroutine(LoadGameObject(folderPath + manifestAssetName, "menuui"));
    }

    /// <summary>
    /// 載入AB資源
    /// </summary>
    /// <param name="folderPath"></param>
    /// <param name="mainfestAssetName"></param>
    /// <returns></returns>
    IEnumerator LoadAsset(string folderPath, string mainfestAssetName)
    {
        string filePath = folderPath + mainfestAssetName;
        AssetBundleRef abRef;

        Debug.Log("Asset Path:" + filePath);

        AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(filePath);
        yield return assetBundleCreateRequest;

        if (assetBundleCreateRequest.isDone)
        {
            AssetBundle asseBundle = assetBundleCreateRequest.assetBundle;

            Debug.Log("Asset name:" + asseBundle.name);
            AssetBundleRequest asset = asseBundle.LoadAssetAsync<GameObject>(mainfestAssetName);
            yield return asset;
            if (asset.isDone)
            {
                //abRef = new AssetBundleRef();
                //abRef.assetBundle = DownloadHandlerAssetBundle.GetContent(www);
                //dictAssetBundleRefs.Add(fileName, abRef);

            }
            Debug.Log("Asset is done: " + asset.asset.name);
        }
    }

    /// <summary>
    /// 載入遊戲物件
    /// </summary>
    /// <param name="assetPath"></param>
    /// <param name="assetName"></param>
    /// <returns></returns>
    IEnumerator LoadGameObject(string assetPath, string assetName)
    {
        string filePath = assetPath;

        AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(filePath);
        yield return assetBundleCreateRequest;

        AssetBundle asseBundle = assetBundleCreateRequest.assetBundle;

        Debug.Log("GameObject: " + asseBundle.name);
        AssetBundleRequest asset = asseBundle.LoadAssetAsync<GameObject>(assetName);
        yield return asset;
        
        if (asset.isDone)
        {
            GameObject loadedAsset = asset.asset as GameObject;
            Instantiate(loadedAsset);
        }
        else
        {
            Debug.Log("GameObject isn't done!  " + asset.isDone);
        }
    }
}
