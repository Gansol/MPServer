using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
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
    private string localItemListText; //本機 檔案列表內容
    private int reConnTimes = 0;
    private bool _bundleChk = false;
    #endregion

    void Start()
    {
        createJSON = gameObject.AddComponent<CreateJSON>();
        bundleDownloader = gameObject.AddComponent<AssetBundlesDownloader>();
        reConnTimes = 0;
    }

    #region -- CompareAssetBundles --
    public void StartCheck() //開始檢查檔案
    {
        StartCoroutine(CompareAssetBundle(Application.persistentDataPath + "/List/" + Global.sItemList, Global.serverListPath + Global.sItemList));
    }

    private IEnumerator CompareAssetBundle(string localPathFile, string serverPathFile) //比對 檔案
    {
        Global.ReturnMessage = "開始檢查遊戲資產...";
        createJSON.AssetBundlesJSON(); //建立最新 檔案列表

        using (WWW wwwItemList = new WWW(serverPathFile))
        { // 下載 伺服器 檔案列表
            yield return wwwItemList;

            if (wwwItemList.error != null && reConnTimes < Global.maxConnTimes)
            {
                reConnTimes++;
                Global.ReturnMessage = "Download Item List Error !   " + wwwItemList.error + "\n Wait for one second. Reconnecting to download(" + reConnTimes + ")";
                Debug.LogError("Download Item List Error !   " + wwwItemList.error + "\n Wait for one second. Reconnecting to download(" + reConnTimes + ")");
                yield return new WaitForSeconds(1.0f);
            }
            if (wwwItemList.isDone)
            {
                reConnTimes = 0;
                Global.ReturnMessage = "Item List Downloaded!";
                CompareListFile(wwwItemList.text, localPathFile);

            }
            else if (reConnTimes == Global.maxConnTimes)
            {
                Global.ReturnMessage = "Can't connecting to Server! Please check your network status.";
                //Debug.Log("Can't connecting to Server! Please check your network status.");
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
            bundleDownloader.DownloadListFile(Global.sFullPackage);
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
            bundleDownloader.fileCount = hashServerList.Count;

            foreach (string item in hashServerList) // 下載檔案
                bundleDownloader.DownloadFile(Global.assetBundlesPath, item);
            #endregion
            
        }
        else // 與伺服器檔案相同
        {
            Global.ReturnMessage = "Nothing to update.";
            //Debug.Log("Nothing to update.");
            Global.isCompleted = true;
        }
    }
    #endregion
    
}
