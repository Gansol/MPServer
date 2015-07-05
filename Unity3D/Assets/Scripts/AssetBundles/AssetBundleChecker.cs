using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

//downloader 需要修改

public class AssetBundleChecker : MonoBehaviour
{
    CreateJSON createJSON;
    AssetBundlesDownloader bundleDownloader;
    private string localItemListText; //本機 檔案列表內容
    private int reConnTimes = 0;
    private bool _bundleChk = false;

    public bool fileDownload = false;
    public bool bundleChk { get { return _bundleChk; } }

    void Start()
    {
        createJSON = gameObject.AddComponent<CreateJSON>();
        bundleDownloader = gameObject.AddComponent<AssetBundlesDownloader>();
        reConnTimes = 0;
    }

    public IEnumerator StartCheck() //開始檢查檔案
    {
        StartCoroutine(CompareAssetBundles(Application.persistentDataPath + "/List/" + Global.sItemList, Global.serverListPath + Global.sItemList));
        yield return null;
    }


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
            Debug.Log("OpenListFile= " + localItemListText);
            return localItemListText;
        }
    }


    private IEnumerator CompareAssetBundles(string localPathFile, string serverPathFile) //比對 檔案
    {
        string localPath = Application.persistentDataPath + "/List/"; // 本機 檔案列表 路徑
        createJSON.AssetBundlesJSON(); //建立最新 檔案列表

        WWW wwwItemList = new WWW(serverPathFile); // 下載 伺服器 檔案列表
        yield return wwwItemList;

        if (wwwItemList.error != null && reConnTimes < Global.maxConnTimes)
        {
            reConnTimes++;
            Debug.Log("Download Item List Error !   " + wwwItemList.error + "\n Wait for one second. Reconnecting to download(" + reConnTimes + ")");
            yield return new WaitForSeconds(1.0f);
            Global.isCheckBundle = true;    // 下載失敗 重新下載 檔案列表
        }
        if (wwwItemList.isDone && wwwItemList.error == null)
        {
            reConnTimes = 0;
            Debug.Log("Item List Downloaded!");
            //把JSON文字解析 後 存入字典檔 等待比較
            Dictionary<string, object> dictJsonServerList = MiniJSON.Json.Deserialize(wwwItemList.text) as Dictionary<string, object>;
            Dictionary<string, object> dictJsonLocalList = MiniJSON.Json.Deserialize(OpenListFile(localPathFile)) as Dictionary<string, object>;

            //比較用 
            HashSet<string> hashServerList = new HashSet<string>();
            HashSet<string> hashLocalList = new HashSet<string>();
            HashSet<string> hashDelorAdd = new HashSet<string>();


            if (localItemListText == "{}" || localItemListText == "") //本機檔案 被砍光光了 下載全部檔案
            {
                StartCoroutine(GetComponent<AssetBundlesDownloader>().DownloadList(Global.sFullPackage));
            }
            else if ((localItemListText != wwwItemList.text))  // 如果 檔案不同 就儲存 進 HashSet 比對資料
            {
                //not equals
                Debug.Log("AssetBundles Not Equals");

                foreach (KeyValuePair<string, object> localItem in dictJsonLocalList) //比對 本機 與 伺服器 全部資料
                {
                    foreach (KeyValuePair<string, object> serverItem in dictJsonServerList)
                    {
                        hashServerList.Add(serverItem.Key); //把 伺服器 檔案列表存入HashSet等待比較
                        if (localItem.Value.ToString() == serverItem.Value.ToString()) //如果發現相同檔案 (Value值是 SHA1)
                            hashDelorAdd.Add(localItem.Key.ToString()); //把本機資料 存入HashSet中比較 不必刪除 localItem.Key (Key值 是 檔案名稱)
                    }
                    hashLocalList.Add(localItem.Key); //把 本機 檔案列表存入HashSet等待比較
                }
                hashLocalList.ExceptWith(hashDelorAdd); // (hashLocalList)要刪除的檔案 = 本機檔案 - 要保存的檔案(無變動檔案)



                StartCoroutine(GetComponent<AssetBundlesDownloader>().DeleteFile(hashLocalList)); //刪除檔案  //目前狀態 檔案已刪除 LocalList還是舊的

                hashLocalList.Clear(); //清除 本機資料列表(要刪除的檔案，不能再比對，要重新再讀一次本機資料)
                hashDelorAdd.Clear(); //清除 保留檔案列表
                createJSON.AssetBundlesJSON();//重新建立 本機 檔案列表

                dictJsonLocalList = MiniJSON.Json.Deserialize(File.ReadAllText(localPathFile)) as Dictionary<string, object>; //存入本機檔案列表至 dictJsonLocalList

                foreach (KeyValuePair<string, object> localItem in dictJsonLocalList) //比較 本機檔案 與 伺服器檔案 新增項目
                {
                    foreach (KeyValuePair<string, object> serverItem in dictJsonServerList)
                    {
                        if (localItem.Value.ToString() == serverItem.Value.ToString()) // 如果 本機檔案 = 伺服器檔案 -->檔案存在
                            hashDelorAdd.Add(localItem.Key.ToString()); //存入 已存在檔案
                    }
                }

                hashServerList.ExceptWith(hashDelorAdd);//hashServerList 新增檔案 = 伺服器檔案 - 已存在檔案

                bundleDownloader.SetFileCount(hashServerList.Count);
                foreach (string item in hashServerList)
                { //這裡要修改  下載新增的 多個檔案
                    fileDownload = false;
                    StartCoroutine(bundleDownloader.DownloadFile(Global.assetBundlesPath, item));
                    while (fileDownload == false)
                        yield return null;
                }
                //Dictionary<string, object> dictLevels = dictJson["item"] as Dictionary<string, object>; //檢視第二層資料 可加上<list>顯示二層內容
            }
            else //與伺服器檔案相同
            {
                // equals
                Debug.Log("AssetBundles Equals");
                Global.isCompleted = true;
                _bundleChk = true;
            }
        }
        else if(reConnTimes == Global.maxConnTimes)
        {
            Debug.LogError("Can't connecting to Server! Please check your network status.");
        }
        wwwItemList.Dispose();// 關閉串流
    }

}
