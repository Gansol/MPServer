using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

// DownloadFile 需要改寫 有空再改 目前只能傳單一值 非陣列值
// 一次多線下載 如果檔案太大 需要改寫

public class AssetBundlesDownloader : MonoBehaviour
{
    public static int _fileCount = 0;
    private int _reConnTimes = 0;
    private bool _fileDownload = false;

    public IEnumerator DownloadList(string pathFile) //pathFile 要下載的List檔案
    {
        // www -> storage dict list -> download file

        WWW wwwDownloadList = new WWW(Global.serverListPath + pathFile); //下載伺服器List檔案
        yield return wwwDownloadList; 
        
        if (wwwDownloadList.error != null && _reConnTimes < Global.maxConnTimes)  // 如果出現網路錯誤，重新連線下載，並提示重連次數
        {
            _reConnTimes++;
            Debug.Log("Download Item List Error !   " + wwwDownloadList.error + "\n Wait for one second. Reconnecting to download(" + _reConnTimes + ")");
            yield return new WaitForSeconds(1.0f);
            Global.isNewlyVision = false;
        }
        else if (wwwDownloadList.isDone)
        {
            Dictionary<string, object> dictDownloadFile = Json.Deserialize(wwwDownloadList.text) as Dictionary<string, object>;

            _reConnTimes = 0;
            _fileCount = dictDownloadFile.Count;

            // 一次多線下載 如果檔案太大 需要改寫
            foreach (KeyValuePair<string, object> assets in dictDownloadFile)
            {//等待下載 下載列表中 全部資源
                _fileDownload = false;
                StartCoroutine(DownloadFile(Global.assetBundlesPath, assets.Key.ToString()));
                while (_fileDownload == false)
                    yield return null;
            }
        }
        else    // 如果出現網路錯誤，停止檢測版本，並提示網路錯誤
        {
            Debug.LogError("Can't connecting to Server! Please check your network status.");
        }
        wwwDownloadList.Dispose();//關閉串流
    }


    public IEnumerator DeleteFile(HashSet<string> hashSet) //hashSet 要被刪除的文件陣列
    {
        // files name -> delete all files
        string localBundlesPath = Application.persistentDataPath + "/AssetBundles/"; //本機資源路徑

        foreach (string file in hashSet)
            File.Delete(localBundlesPath + file); //刪除所有被修改 或 舊版本檔案

        yield return hashSet;
    }


    public IEnumerator DownloadLostFile(HashSet<string> hashSet) //hashSet 要下載的文件陣列
    {
        _fileCount = hashSet.Count;
        // lost files name -> download all files
        foreach (string assets in hashSet) //下載全部遺失的檔案
            StartCoroutine(DownloadFile(Global.assetBundlesPath, assets));

        yield return hashSet;
    }


    public IEnumerator DownloadFile(string path, string assets) //path 下載路徑 assets 文件陣列
    {
        if (!(File.Exists(Application.persistentDataPath + "/AssetBundles/" + assets)))  //如果檔案不存在資料夾 開始下載
        {
            Debug.Log("Start Downloading " + assets);
            WWW wwwAssetBundles = new WWW(path + assets); //下載物件
            yield return wwwAssetBundles; //等待下載完成，並回傳值

            if (wwwAssetBundles.error != null && _reConnTimes < Global.maxConnTimes)  // 如果出現網路錯誤，重新連線下載，並提示重連次數
            {
                _reConnTimes++;
                Debug.Log("Download (" + assets + ") File Error !   " + wwwAssetBundles.error + "\n Wait for one second. Reconnecting to download(" + _reConnTimes + ")");
                yield return new WaitForSeconds(1.0f);
                StartCoroutine(DownloadFile(path, assets)); // ＊＊＊＊這可能會出錯＊＊＊＊＊
            }
            else if (wwwAssetBundles.isDone) //伺服器檔案 下載完成
            {
                _reConnTimes = 0;
                Debug.Log("AssetBundle Downloaded!");
                System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/AssetBundles/"); //建立 檔案目錄
                byte[] file = wwwAssetBundles.bytes; //轉換為二進位物件
                string[] folder = assets.Split('/');
                string folderPath = Application.persistentDataPath + "/AssetBundles/" + folder[0];
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
                File.WriteAllBytes(Application.persistentDataPath + "/AssetBundles/" + assets, file); //寫入 檔案 WriteAllBytes(要寫入的路徑與檔案名稱!!!不能只寫路徑(關鍵),bytes檔案)
                _fileCount--;
                GetComponent<AssetBundleChecker>().fileDownload = true;
                _fileDownload = true;
                if (_fileCount == 0) {
                    StartCoroutine(ReplaceItemList());
                    Global.isCheckBundle = true;
                    Global.isVisionDownload = true;
                }
            }
            else    // 如果出現網路錯誤，停止檢測版本，並提示網路錯誤
            {
                Debug.LogError("Can't connecting to Server! Please check your network status.");
            }
                wwwAssetBundles.Dispose();//關閉串流
        }
        else if (File.Exists(Application.persistentDataPath + "/AssetBundles/" + assets) && !Global.isCompleted) //全部下載完成 取代檔案列表
        {
            _fileCount--;
            _fileDownload = true;
            if (_fileCount == 0)
            {
                StartCoroutine(ReplaceItemList());
                Global.isCheckBundle = true;
                Global.isVisionDownload = true;
            }
        }
    }


    private IEnumerator ReplaceItemList() //取代 檔案列表
    {
        WWW wwwItemList = new WWW(Global.serverListPath + Global.sItemList);
        yield return wwwItemList;

        if (wwwItemList.error != null && _reConnTimes < Global.maxConnTimes)  // 如果出現網路錯誤，重新連線下載，並提示重連次數
        {
            _reConnTimes++;
            Debug.Log("Download Item List Error !   " + wwwItemList.error + "\n Wait for one second. Reconnecting to download(" + _reConnTimes + ")");
            yield return new WaitForSeconds(1.0f);
            //Global.isReplaced = false;
        }
        else if(wwwItemList.isDone)//開始檢查 檔案列表
        {
            _reConnTimes = 0;
            File.WriteAllBytes(Application.persistentDataPath + "/List/" + Global.sItemList, System.Text.Encoding.UTF8.GetBytes(wwwItemList.text)); //WriteAllBytes(要寫入的路徑與檔案名稱!!!不能只寫路徑(關鍵),bytes檔案)
            wwwItemList.Dispose();
            //Global.isReplaced = true;
        }
        else    // 如果出現網路錯誤，停止檢測版本，並提示網路錯誤
        {
            Debug.LogError("Can't connecting to Server! Please check your network status.");
        }
        wwwItemList.Dispose();//關閉串流
    }

    public void SetFileCount(int value)
    {
        _fileCount = value;
    }
}
