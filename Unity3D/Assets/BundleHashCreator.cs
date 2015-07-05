using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using MiniJSON;

public class BundleHashCreator : MonoBehaviour
{
    public string bundleFolder =
#if UNITY_ANDROID
 "/_AssetBundles/Android/";
#elif UNITY_IPHONE
    "/_AssetBundles/iOS/";
#elif UNITY_STANDALONE
    "/_AssetBundles/STANDALONE/";
#endif
    public HashType hashType = HashType.SHA1;
    public Planform planform =
#if UNITY_ANDROID
 Planform.Android;
#elif UNITY_IPHONE
        Planform.iOS;
#elif UNITY_STANDALONE
    Planform.STANDALONE;
#endif


    public enum HashType : byte
    {
        SHA1,
        MD5,
    }

    public enum Planform : byte
    {
        Android,
        iOS,
        STANDALONE,
    }

    // Use this for initialization
    void Start()
    {
        string path = Application.dataPath + bundleFolder;
        Debug.Log(path);

        if (!Directory.Exists(path))
        {
            Debug.LogError("Bundle Folder Not Found !");
        }
        else
        {
            AssetBundlesJSON(path);
        }
    }


    AssetBundlesHash bundleHash;

    private string[] pathFiles;
    private byte[] bytesFile;
    string hash;

    public void AssetBundlesJSON(string path) //建立 檔案列表
    {
        bundleHash = gameObject.AddComponent<AssetBundlesHash>();
        Dictionary<string, object> dictBundles = new Dictionary<string, object>();

        pathFiles = Directory.GetFiles(path,"*.unity3d"); //取得 本機資料夾 全部檔案


        foreach (string file in pathFiles) // 尋遍所有資料夾下 檔案路徑
        {
            bytesFile = File.ReadAllBytes(file); //讀取檔案bytes
            hash = bundleHash.SHA1Complier(bytesFile);//Hash bytes
            dictBundles.Add(Path.GetFileName(file), hash);//把hash過的值存入字典檔
        }
        CreateFile(Json.Serialize(dictBundles), path, "BundleHash.json"); //建立 新 檔案列表
    }


    protected void CreateFile(string contant, string path, string fileName) //建立檔案
    {
        using (FileStream fs = File.Create(path + fileName)) //using 會自動關閉Stream 建立檔案
        {
            fs.Write(new UTF8Encoding(true).GetBytes(contant), 0, contant.Length); //寫入檔案
            fs.Dispose(); //避免錯誤 在寫一次關閉
            Debug.Log("Bundle Hash Completed!");
        }
    }
}
