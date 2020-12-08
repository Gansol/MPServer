using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
* 負責 檢查資產
* ***************************************************************
*                           ChangeLog
* 20160714 v1.0.1  修正部分問題      
* 20200717 v1.1.0  修正無法辨認檔案差異
* ****************************************************************/
public class AssetBundleChecker : MonoBehaviour
{
    #region 欄位
    CreateJSON createJSON;
    AssetBundlesDownloader bundleDownloader;

    public int downloadCount;           // 下載數量
    public bool bundleChk, bFristLoad;              // 資源檢查
    private string localItemListText;   // 本機 檔案列表內容
    private int reConnTimes = 0;

    #endregion

    void Start()
    {
        createJSON = new CreateJSON();
        bundleDownloader = gameObject.AddComponent<AssetBundlesDownloader>();
        downloadCount = reConnTimes = 0;
        bundleChk = false;
    }

    //private void Update()
    //{
    //    bundleChk = bundleDownloader.BundleChk;
    //}

    #region -- CompareAssetBundles --
    public void StartCheck() //開始檢查檔案
    {
        bundleChk = false;
        StartCoroutine(DownloadList(Application.persistentDataPath + "/List/" + Global.MusicsFile, Global.serverListPath + Global.MusicsFile));
        StartCoroutine(DownloadList(Application.persistentDataPath + "/List/" + Global.SoundsFile, Global.serverListPath + Global.SoundsFile));
        StartCoroutine(DownloadBundleVersion(Application.persistentDataPath + "/List/" + Global.bundleVersionFile, Global.serverListPath + Global.bundleVersionFile));
        StartCoroutine(CompareAssetBundle(Application.persistentDataPath + "/List/" + Global.itemListFile, Global.serverListPath + Global.itemListFile));
    }

    /// <summary>
    /// 只有下載缺失檔案，不比對
    /// </summary>
    /// <param name="localPathFile"></param>
    /// <param name="serverPathFile"></param>
    /// <returns></returns>
    private IEnumerator DownloadList(string localPathFile, string serverPathFile) 
    {
        Global.ReturnMessage = "開始檢查遊戲音樂列表...";
        if (!File.Exists(localPathFile))
        {
        ReCheckFlag:
            using (UnityWebRequest wwwList = UnityWebRequest.Get(serverPathFile))
            { // 下載 伺服器 檔案列表
                yield return wwwList.SendWebRequest();

                if (wwwList.error != null && reConnTimes < Global.maxConnTimes)
                {
                    reConnTimes++;
                    Global.ReturnMessage = "無法下載資源列表，嘗試重新下載(" + reConnTimes + "/" + Global.maxConnTimes + ")";
                    Debug.Log("Download Vision List Error !   " + wwwList.error + "\n Wait for one second. Reconnecting to download(" + reConnTimes + ")");
                    yield return new WaitForSeconds(1.0f);
                    goto ReCheckFlag;
                    //StartCoroutine(DownloadBundleVersion(localPathFile, serverPathFile));
                }
                else if (wwwList.isDone && wwwList.error == null)
                {
                    // 如果本機 版本列表檔案 不存在 建立空檔
                    File.Create(localPathFile).Close();
                    File.WriteAllText(localPathFile, wwwList.downloadHandler.text);

                    Debug.Log("下載音樂列表完成!");
                    Global.ReturnMessage = "下載音樂列表完成!";
                    reConnTimes = 0;
                }
                else if (reConnTimes >= Global.maxConnTimes)
                {
                    Global.ReturnMessage = Global.ReturnMessage = "無法連線至伺服器，請檢查網路狀態!"; ;
                    Debug.Log("Can't connecting to Server! Please check your network status.");
                    wwwList.Dispose();
                }
            }
        }
    }

    private IEnumerator DownloadBundleVersion(string localPathFile, string serverPathFile) //比對 檔案
    {
        Global.ReturnMessage = "開始檢查遊戲資產...";
        createJSON.AssetBundlesJSON(); //建立最新 檔案列表
    ReCheckFlag:
        using (UnityWebRequest wwwVersionList = UnityWebRequest.Get(serverPathFile))
        { // 下載 伺服器 檔案列表
            yield return wwwVersionList.SendWebRequest();

            if (wwwVersionList.error != null && reConnTimes < Global.maxConnTimes)
            {
                reConnTimes++;
                Global.ReturnMessage = "無法下載資源列表，嘗試重新下載(" + reConnTimes + "/" + Global.maxConnTimes + ")";
                Debug.Log("Download Vision List Error !   " + wwwVersionList.error + "\n Wait for one second. Reconnecting to download(" + reConnTimes + ")");
                yield return new WaitForSeconds(1.0f);
                goto ReCheckFlag;
                //StartCoroutine(DownloadBundleVersion(localPathFile, serverPathFile));
            }
            if (wwwVersionList.isDone && wwwVersionList.error == null)
            {
                Global.bundleVersion = System.Convert.ToUInt16(wwwVersionList.downloadHandler.text);
                Debug.Log("遊戲資源版本:" + Global.bundleVersion);
                reConnTimes = 0;
                Global.ReturnMessage = "下載資源版本列表完成!";
            }
            else if (reConnTimes >= Global.maxConnTimes)
            {
                Global.ReturnMessage = Global.ReturnMessage = "無法連線至伺服器，請檢查網路狀態!"; ;
                Debug.Log("Can't connecting to Server! Please check your network status.");
                wwwVersionList.Dispose();
            }
        }
    }


    private IEnumerator CompareAssetBundle(string localPathFile, string serverPathFile) //比對 檔案
    {
        Global.ReturnMessage = "開始檢查遊戲資產...";
        createJSON.AssetBundlesJSON(); //建立最新 檔案列表
    ReCheckFlag:

        using (UnityWebRequest wwwItemList = UnityWebRequest.Get(serverPathFile))
        { // 下載 伺服器 檔案列表
            yield return wwwItemList.SendWebRequest();

            if (wwwItemList.error != null && reConnTimes < Global.maxConnTimes)
            {
                reConnTimes++;
                Global.ReturnMessage = "無法下載檔案列表，嘗試重新下載(" + reConnTimes + "/" + Global.maxConnTimes + ")";
                Debug.LogError("Download Item List Error !   " + wwwItemList.error + "\n Wait for one second. Reconnecting to download(" + reConnTimes + ")");
                yield return new WaitForSeconds(.5f);
                goto ReCheckFlag;
                //StartCoroutine(CompareAssetBundle(localPathFile, serverPathFile));
            }
            if (wwwItemList.isDone && wwwItemList.error == null)
            {
                reConnTimes = 0;
                Global.ReturnMessage = "檔案列表下載完成!";
                CompareListFile(wwwItemList.downloadHandler.text, localPathFile);
            }
            else if (reConnTimes >= Global.maxConnTimes)
            {
                Global.ReturnMessage = Global.ReturnMessage = "無法連線至伺服器，請檢查網路狀態!"; ;
                Debug.Log("Can't connecting to Server! Please check your network status.");
                wwwItemList.Dispose();
            }
        }
    }
    #endregion

    #region -- OpenListFile --
    /// <summary>
    /// 開啟並讀取本機資產列表
    /// </summary>
    /// <param name="path"></param>
    /// <returns>列表檔案內容</returns>
    private string OpenListFile(string path) //存入本機
    {
        if (!File.Exists(path))
        {
            File.Create(path).Close();
            File.WriteAllText(path, "{}");
            return "{}";
        }
        else
        {
            // File.ReadAllText 方法 開啟文字檔，讀取檔案的所有行，然後關閉檔案。
            localItemListText = File.ReadAllText(path);
            return localItemListText;
        }
    }
    #endregion

    #region -- CompareListFile --
    /// <summary>
    /// 比較不同的資產，下載遺失檔案並刪除多的檔案
    /// </summary>
    /// <param name="wwwText">伺服器資產列表內容</param>
    /// <param name="localPathFile">本機資產列表內容</param>
    private void CompareListFile(string wwwText, string localPathFile)
    {
        //把JSON文字解析 後 存入字典檔 等待比較
        Dictionary<string, object> dictServerFileList = MiniJSON.Json.Deserialize(wwwText) as Dictionary<string, object>;
        Dictionary<string, object> dictLocalFileList = MiniJSON.Json.Deserialize(OpenListFile(localPathFile)) as Dictionary<string, object>;
        Dictionary<string, object> dictServerCompareList = new Dictionary<string, object>(dictServerFileList);
        Dictionary<string, object> dictLocalCompareList = new Dictionary<string, object>(dictLocalFileList);

        //比較用 HashSet
        HashSet<string> hashServerList = new HashSet<string>();
        HashSet<string> hashLocalList = new HashSet<string>();
        HashSet<string> hashModifyItem = new HashSet<string>();
        HashSet<string> hashNewServerItem, hashDelItem, hashTmpItem;


        if (localItemListText == "{}" || localItemListText == "") //本機檔案 被砍光光了 下載全部檔案
        {
            bFristLoad = true;
            bundleDownloader.DownloadListFile(Global.fullPackageFile);
        }
        else if (localItemListText != wwwText)  // 如果 檔案不同 就儲存 進 HashSet 比對資料
        {
            foreach (KeyValuePair<string, object> serverItem in dictServerFileList)
                hashServerList.Add(serverItem.Key);

            foreach (KeyValuePair<string, object> localItem in dictLocalFileList)
                hashLocalList.Add(localItem.Key);


            // 使用HashSet 排除Server相同檔案 找出 新增檔案 並存入HashSet       Server files - local files = new file
            hashTmpItem = new HashSet<string>(hashServerList);
            hashTmpItem.ExceptWith(hashLocalList);
            hashNewServerItem = new HashSet<string>(hashTmpItem);
            //Debug.Log(hashNewServerItem.Count);

            // 使用HashSet 排除Server相同檔案 找出 多餘檔案 並存入HashSet 刪除      local files -  Server files = del file
            hashLocalList.ExceptWith(hashServerList);
            hashDelItem = hashLocalList;    // 方便辨認刪除物件 無意義HashSet
            //Debug.Log(hashDelItem.Count);


            // 取得沒有新檔案的伺服器檔案列表 (用來比較與本機檔案差異)  ServerFiles - NewFiles = no NewFile ServerFile
            foreach (KeyValuePair<string, object> serverItem in dictServerFileList)
            {
                if (hashNewServerItem.Contains(serverItem.Key))
                    dictServerCompareList.Remove(serverItem.Key);
            }

            //Debug.Log("dictServerCompareList.Count :" + dictServerCompareList.Count);

            // 取得沒有舊檔案的本機檔案列表 (用來比較與伺服器檔案差異)  ServerFiles - NewFiles = no NewFile ServerFile
            foreach (KeyValuePair<string, object> localItem in dictLocalFileList)
            {
                if (hashDelItem.Contains(localItem.Key))
                    dictLocalCompareList.Remove(localItem.Key);
            }

            //Debug.Log("dictServerCompareList.Count :" + dictServerCompareList.Count);

            // 檢查相同的檔案是否被修改 
            foreach (KeyValuePair<string, object> item in dictServerCompareList)
            {
                dictLocalCompareList.TryGetValue(item.Key, out object value);

                if (item.Value.ToString() != value.ToString())
                    hashModifyItem.Add(item.Key);
            }

            hashNewServerItem.UnionWith(hashModifyItem);    // 合併HashSet 新增檔案 與 差異檔案 
            bundleDownloader.DeleteFile(hashDelItem);   // 刪除檔案

            //   Debug.Log(hashNewServerItem.Count);

            // 如果有 新增檔案
            if (hashNewServerItem.Count > 0)
            {
                bundleDownloader.ModifyFile();
                bundleDownloader.DownloadFile(Global.assetBundlesPath, hashNewServerItem.ToList<string>());
                bundleChk = bundleDownloader.BundleChk;
            }
            else
            {
                Global.ReturnMessage = "遊戲資源為最新版本!";
                Global.isCompleted = true;
            }
        }
        else // 與伺服器檔案相同
        {
            //Debug.Log("Nothing to update.");
            Global.ReturnMessage = "遊戲資源為最新版本!";
            Global.isCompleted = true;
        }
    }
    #endregion

}
