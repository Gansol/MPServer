using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
/* ***************************************************************
 * -----Copyright © 2015 Gansol Studio.  All Rights Reserved.-----
 * -----------            CC BY-NC-SA 4.0            -------------
 * -----------  @Website:  EasyUnity@blogspot.com    -------------
 * -----------  @Email:    GansolTW@gmail.com        -------------
 * -----------  @Author:   Krola.                    -------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * 負責 檢查下載檔案列表、資產，刪除檔案
 * 未來須重寫統一下載方式
 * ***************************************************************
 *                           ChangeLog
 * 20160714 v1.0.1  修正部分問題                                       
 * ****************************************************************/
public class AssetBundlesDownloader : MonoBehaviour
{
    #region 欄位
    public int fileCount { get; set; }
    public bool fileDownloaded { get { return _fileDownloaded;} }

    private int _reConnTimes;
    private bool _fileDownloaded;
    #endregion

    #region -- DownloadListFile --
    /// <summary>
    /// pathFile 要下載的List檔案
    /// </summary>
    /// <param name="pathFile"></param>
    public void DownloadListFile(string pathFile)
    {
        StartCoroutine(_DownloadListFile(pathFile));
    }

    public IEnumerator _DownloadListFile(string pathFile) //pathFile 要下載的List檔案
    {
        // www -> storage dict list -> download file

        using (WWW wwwDownloadList = new WWW(Global.serverListPath + pathFile))
        {//下載伺服器List檔案
            yield return wwwDownloadList;

            if (wwwDownloadList.error != null && _reConnTimes < Global.maxConnTimes)  // 如果出現網路錯誤，重新連線下載，並提示重連次數
            {
                _reConnTimes++;

                Global.ReturnMessage = "Download Item List Error !   " + wwwDownloadList.error + "\n Wait for one second. Reconnecting to download(" + _reConnTimes + ")";
                Debug.Log("Download Item List Error !   " + wwwDownloadList.error + "\n Wait for one second. Reconnecting to download(" + _reConnTimes + ")");
                yield return new WaitForSeconds(1.0f);
                StartCoroutine(_DownloadListFile(pathFile));
                Global.isNewlyVision = false;
            }
            else if (wwwDownloadList.isDone)
            {
                Dictionary<string, object> dictDownloadFile = Json.Deserialize(wwwDownloadList.text) as Dictionary<string, object>;

                _reConnTimes = 0;
                fileCount = dictDownloadFile.Count;

                // 一次多線下載 如果檔案太大 需要改寫
                foreach (KeyValuePair<string, object> assets in dictDownloadFile)
                {//等待下載 下載列表中 全部資源
                    StartCoroutine(_DownloadFile(Global.assetBundlesPath, assets.Key.ToString()));
                }
            }
            else    // 如果出現網路錯誤，停止檢測版本，並提示網路錯誤
            {
                Global.ReturnMessage = "Can't connecting to Server! Please check your network status.";
                Debug.LogError("Can't connecting to Server! Please check your network status.");
            }
        }
    }
    #endregion

    #region -- DeleteFile --
    /// <summary>
    /// hashSet 要被刪除的文件陣列
    /// </summary>
    /// <param name="hashSet"></param>
    /// <returns></returns>
    public IEnumerator DeleteFile(HashSet<string> hashSet)
    {
        if (hashSet.Count != 0)
        {
            string localBundlesPath = Application.persistentDataPath + "/AssetBundles/"; //本機資源路徑

            foreach (string file in hashSet)
                File.Delete(localBundlesPath + file); //刪除所有被修改 或 舊版本檔案
        }
        yield return hashSet;
    }
    #endregion

    #region -- DownloadLostFile --
    /// <summary>
    /// hashSet 要下載的文件陣列
    /// </summary>
    /// <param name="hashSet"></param>
    /// <returns></returns>
    public IEnumerator DownloadLostFile(HashSet<string> hashSet) 
    {
        fileCount = hashSet.Count;
        // lost files name -> download all files
        foreach (string assets in hashSet) //下載全部遺失的檔案
            StartCoroutine(_DownloadFile(Global.assetBundlesPath, assets));

        yield return hashSet;
    }
    #endregion

    #region -- DownloadFile --
    /// <summary>
    /// 下載檔案
    /// </summary>
    /// <param name="path">下載路徑</param>
    /// <param name="assets">資產名稱</param>
    public void DownloadFile(string path, string assets)
    {
        StartCoroutine(_DownloadFile(path, assets));
    }

    private IEnumerator _DownloadFile(string path, string assets)
    {
        if (!(File.Exists(Application.persistentDataPath + "/AssetBundles/" + assets)))  //如果檔案不存在資料夾 開始下載
        {
            Global.ReturnMessage = "Start Downloading... " + assets;
            //Debug.Log("Start Downloading... " + assets);
            using (WWW wwwAssetBundles = new WWW(path + assets))
            {
                ; //下載物件
                yield return wwwAssetBundles; //等待下載完成，並回傳值

                if (wwwAssetBundles.error != null && _reConnTimes < Global.maxConnTimes)  // 如果出現網路錯誤，重新連線下載，並提示重連次數
                {
                    _reConnTimes++;
                    yield return new WaitForSeconds(1.0f);
                    DownloadFile(path, assets); // ＊＊＊＊這可能會出錯＊＊＊＊＊
                    Global.ReturnMessage = "Download (" + assets + ") File Error !   " + wwwAssetBundles.error + "\n Wait for one second. Reconnecting to download(" + _reConnTimes + ")";
                    //Debug.Log("Download (" + assets + ") File Error !   " + wwwAssetBundles.error + "\n Wait for one second. Reconnecting to download(" + _reConnTimes + ")");
                }
                else if (wwwAssetBundles.isDone) //伺服器檔案 下載完成
                {
                    Global.ReturnMessage = "AssetBundle Downloaded!";
                    //Debug.Log("AssetBundle Downloaded!");

                    _reConnTimes = 0;
                    fileCount--;

                    System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/AssetBundles/"); //建立 檔案目錄

                    byte[] file = wwwAssetBundles.bytes; //轉換為二進位物件
                    string[] folder = assets.Split('/');
                    string folderPath = Application.persistentDataPath + "/AssetBundles/" + folder[0];
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);
                    File.WriteAllBytes(Application.persistentDataPath + "/AssetBundles/" + assets, file); //寫入 檔案 WriteAllBytes(要寫入的路徑與檔案名稱!!!不能只寫路徑(關鍵),bytes檔案)

                    ChkDownloadedCount();
                }
                else    // 如果出現網路錯誤，停止檢測版本，並提示網路錯誤
                {
                    Global.ReturnMessage = "Can't connecting to Server! Please check your network status.";
                    //Debug.LogError("Can't connecting to Server! Please check your network status.");
                }
            }
        }
        else if (File.Exists(Application.persistentDataPath + "/AssetBundles/" + assets) && !Global.isCompleted) //全部下載完成 取代檔案列表
        {
            fileCount--;
            ChkDownloadedCount();
        }
    }
    #endregion

    #region -- ReplaceItemList --
    private IEnumerator ReplaceItemList() //取代 檔案列表
    {
        using (WWW wwwItemList = new WWW(Global.serverListPath + Global.sItemList))
        {
            yield return wwwItemList;

            if (wwwItemList.error != null && _reConnTimes < Global.maxConnTimes)  // 如果出現網路錯誤，重新連線下載，並提示重連次數
            {
                _reConnTimes++;
                Global.ReturnMessage = "Download Item List Error !   " + wwwItemList.error + "\n Wait for one second. Reconnecting to download(" + _reConnTimes + ")";
                //Debug.Log("Download Item List Error !   " + wwwItemList.error + "\n Wait for one second. Reconnecting to download(" + _reConnTimes + ")");
                yield return new WaitForSeconds(1.0f);
            }
            else if (wwwItemList.isDone)//開始檢查 檔案列表
            {
                _reConnTimes = 0;
                File.WriteAllBytes(Application.persistentDataPath + "/List/" + Global.sItemList, System.Text.Encoding.UTF8.GetBytes(wwwItemList.text)); //WriteAllBytes(要寫入的路徑與檔案名稱!!!不能只寫路徑(關鍵),bytes檔案)
                wwwItemList.Dispose();
                Global.isCompleted = true;
            }
            else    // 如果出現網路錯誤，停止檢測版本，並提示網路錯誤
            {
                Global.ReturnMessage = "Can't connecting to Server! Please check your network status.";
                //Debug.LogError("Can't connecting to Server! Please check your network status.");
            }
        }
    }
    #endregion

    #region -- ChkDownloadedCount --
    void ChkDownloadedCount()    // 檢查下載是否完成
    {
        if (fileCount == 0)
            StartCoroutine(ReplaceItemList());
    }
    #endregion

}

