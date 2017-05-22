using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
 * ****************************************************************/
public class AssetBundleChecker : MonoBehaviour
{
    #region 欄位
    CreateJSON createJSON;
    AssetBundlesDownloader bundleDownloader;

    public int downloadCount;           // 下載數量
    public bool bundleChk,bFristLoad;              // 資源檢查
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

    #region -- CompareAssetBundles --
    public void StartCheck() //開始檢查檔案
    {
        bundleChk = false;
        StartCoroutine(DownloadBundleVersion(Application.persistentDataPath + "/List/" + Global.bundleVersionFile, Global.serverListPath + Global.bundleVersionFile));
        StartCoroutine(CompareAssetBundle(Application.persistentDataPath + "/List/" + Global.itemListFile, Global.serverListPath + Global.itemListFile));
    }

    private IEnumerator DownloadBundleVersion(string localPathFile, string serverPathFile) //比對 檔案
    {
        Global.ReturnMessage = "開始檢查遊戲資產...";
        createJSON.AssetBundlesJSON(); //建立最新 檔案列表
    ReCheckFlag:
        using (WWW wwwVersionList = new WWW(serverPathFile))
        { // 下載 伺服器 檔案列表
            yield return wwwVersionList;

            if (wwwVersionList.error != null && reConnTimes < Global.maxConnTimes)
            {
                reConnTimes++;
                Global.ReturnMessage = "無法下載資源列表，嘗試重新下載(" + reConnTimes + "/" + Global.maxConnTimes + ")";
                Debug.Log("Download Vision List Error !   " + wwwVersionList.error + "\n Wait for one second. Reconnecting to download(" + reConnTimes + ")");
                yield return new WaitForSeconds(1.0f);
                goto ReCheckFlag;
            }
            if (wwwVersionList.isDone && wwwVersionList.error == null)
            {
                Global.bundleVersion = System.Convert.ToInt16(wwwVersionList.text);
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
        using (WWW wwwItemList = new WWW(serverPathFile))
        { // 下載 伺服器 檔案列表
            yield return wwwItemList;

            if (wwwItemList.error != null && reConnTimes < Global.maxConnTimes)
            {
                reConnTimes++;
                Global.ReturnMessage = "無法下載檔案列表，嘗試重新下載(" + reConnTimes + "/" + Global.maxConnTimes + ")";
                Debug.LogError("Download Item List Error !   " + wwwItemList.error + "\n Wait for one second. Reconnecting to download(" + reConnTimes + ")");
                yield return new WaitForSeconds(1.0f);
                goto ReCheckFlag;
            }
            if (wwwItemList.isDone && wwwItemList.error == null)
            {
                reConnTimes = 0;
                Global.ReturnMessage = "檔案列表下載完成!";
                CompareListFile(wwwItemList.text, localPathFile);

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
        Dictionary<string, object> dictJsonServerList = MiniJSON.Json.Deserialize(wwwText) as Dictionary<string, object>;
        Dictionary<string, object> dictJsonLocalList = MiniJSON.Json.Deserialize(OpenListFile(localPathFile)) as Dictionary<string, object>;

        //比較用 
        HashSet<string> hashServerList = new HashSet<string>();
        HashSet<string> hashLocalList = new HashSet<string>();
        HashSet<string> hashDelorAdd = new HashSet<string>();


        if (localItemListText == "{}" || localItemListText == "") //本機檔案 被砍光光了 下載全部檔案
        {
            bFristLoad = true;
            bundleDownloader.DownloadListFile(Global.fullPackageFile);
        }
        else if ((localItemListText != wwwText))  // 如果 檔案不同 就儲存 進 HashSet 比對資料
        {
            //Debug.Log("AssetBundles Not Equals");
            #region 刪除多的檔案
            foreach (KeyValuePair<string, object> localItem in dictJsonLocalList) //比對 本機 與 伺服器 全部資料
            {
                foreach (KeyValuePair<string, object> serverItem in dictJsonServerList)
                {
                    hashServerList.Add(serverItem.Key); //把 伺服器 檔案列表存入HashSet等待比較
                    if (localItem.Value.ToString() == serverItem.Value.ToString()) //如果發現相同檔案 (Value值是 SHA1)
                    {
                        hashDelorAdd.Add(localItem.Key.ToString()); //把本機資料 存入HashSet中比較 不必刪除 localItem.Key (Key值 是 檔案名稱)
                        break;
                    }
                }
                hashLocalList.Add(localItem.Key); //把 本機 檔案列表存入HashSet等待比較
            }
            hashLocalList.ExceptWith(hashDelorAdd); // (hashLocalList)要刪除的檔案 = 本機檔案 - 要保存的檔案(無變動檔案)
            //Debug.Log(hashLocalList.Count);
            bundleDownloader.DeleteFile(hashLocalList); //刪除檔案  //目前狀態 檔案已刪除 LocalList還是舊的

            hashLocalList.Clear(); //清除 本機資料列表(要刪除的檔案，不能再比對，要重新再讀一次本機資料)
            hashDelorAdd.Clear(); //清除 保留檔案列表
            createJSON.AssetBundlesJSON();//重新建立 本機 檔案列表
            #endregion

            #region 新增檔案
            dictJsonLocalList = MiniJSON.Json.Deserialize(File.ReadAllText(localPathFile)) as Dictionary<string, object>; //存入本機檔案列表至 dictJsonLocalList

            foreach (KeyValuePair<string, object> localItem in dictJsonLocalList) //比較 本機檔案 與 伺服器檔案 新增項目
            {
                foreach (KeyValuePair<string, object> serverItem in dictJsonServerList)
                {
                    if (localItem.Value.ToString() == serverItem.Value.ToString())  // 如果 本機檔案 = 伺服器檔案 -->檔案存在
                    {
                        hashDelorAdd.Add(localItem.Key.ToString()); //存入 已存在檔案
                        break;
                    }
                }
            }

            hashServerList.ExceptWith(hashDelorAdd);// hashServerList 新增檔案 = 伺服器檔案 - 已存在檔案
            downloadCount = bundleDownloader.fileCount = hashServerList.Count;
            if (downloadCount > 0) bundleChk = true;
            if (hashServerList.Count != 0)
            {
                //foreach (string item in hashServerList) // 下載檔案
                //    bundleDownloader.DownloadFile(Global.assetBundlesPath, item);

                bundleDownloader.DownloadFile(Global.assetBundlesPath, hashServerList.ToList<string>());
            }
            else
            {
                Global.ReturnMessage = "遊戲資源為最新版本!";
                Global.isCompleted = true;
            }
            #endregion

        }
        else // 與伺服器檔案相同
        {
            Global.ReturnMessage = "遊戲資源為最新版本!";
            //Debug.Log("Nothing to update.");
            Global.isCompleted = true;
        }
    }
    #endregion

}
