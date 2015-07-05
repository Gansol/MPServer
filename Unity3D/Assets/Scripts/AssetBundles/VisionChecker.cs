using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MiniJSON;
using System;

//未完成，只有讀伺服器資料
public class VisionChecker : MonoBehaviour
{
    AssetBundlesHash bundleHash; //hash文件用
    AssetBundleChecker bundleChecker = null;
    private string _listText; //暫存 讀取的List文字
    private byte[] _bVisionFile; //暫存 伺服器版本列表
    private int reConnTimes = 0;
    private bool _visionChk = false;

    public bool visionChk { get { return _visionChk; } }

    void Start()
    {
        bundleHash = gameObject.AddComponent<AssetBundlesHash>();
        reConnTimes = 0;
    }

    public IEnumerator CheckVision() //檢查版本
    {
        string localListPath = Application.persistentDataPath + "/List/";
        string localVisionListFile = localListPath + Global.sVisionList;
        string localItemListFile = localListPath + Global.sItemList;

    reconnection: ;
        WWW wwwVisionList = new WWW(Global.serverListPath + Global.sVisionList);
        yield return wwwVisionList;

        //Debug.Log("FILE Contain= " + www.text);
        Debug.Log("IN CHECK VISION");

        if (wwwVisionList.error != null && reConnTimes < Global.maxConnTimes)  // 如果出現網路錯誤，重新連線下載，並提示重連次數
        {
            reConnTimes++;
            Debug.Log("Download Vision List Error !   " + wwwVisionList.error + "\n Wait for one second. Reconnecting to download(" + reConnTimes + ")");
            wwwVisionList.Dispose();
            yield return new WaitForSeconds(1.0f);
            goto reconnection;
        }
        else if (wwwVisionList.isDone && wwwVisionList.error == null)    //開始檢查版本
        {
            reConnTimes = 0;
            _bVisionFile = System.Text.Encoding.UTF8.GetBytes(wwwVisionList.text); // 儲存 下載好的檔案版本

            // 如果本機 版本列表檔案 不存在 建立空檔
            if (!File.Exists(localVisionListFile))
            {
                File.Create(localVisionListFile).Close();
                File.WriteAllText(localVisionListFile, "{}");
            }

            //Dictionary<KEY(鍵值),Vaule(值)> 
            //foreach ( 遞增值(陣列內涵值) in 陣列 )
            //取得伺服器版本列表
            if (!(bundleHash.SHA1Complier(File.ReadAllBytes(localVisionListFile)) == bundleHash.SHA1Complier(_bVisionFile))) //本機 比較 伺服器 版本
            {
                Debug.Log("不是最新版本!");
                Global.isNewlyVision = false;
                wwwVisionList.Dispose();
            }
            else //版本相同 再次檢查檔案 (這是在版本列表不同下的 再次檢查)
            {
                Debug.Log("本機檔案為最新版本!");
                Global.isNewlyVision = true;
                Global.isVisionDownload = true;
                Global.isCheckBundle = true;
                wwwVisionList.Dispose();
            }
        }

        else    // 如果出現網路錯誤，停止檢測版本，並提示網路錯誤
        {
            Debug.LogError("Can't connecting to Server! Please check your network status.");
        }
        wwwVisionList.Dispose();    // 關閉串流
    }

    /// <summary>
    /// 更新完成後 取代版本列表(單純置換版本列表)
    /// </summary>
    /// <returns></returns>
    public IEnumerator ReplaceVisionList()
    {
        File.WriteAllBytes(Application.persistentDataPath + "/List/" + Global.sVisionList, _bVisionFile); //WriteAllBytes(要寫入的路徑與檔案名稱!!!不能只寫路徑(關鍵),bytes檔案)
        Debug.Log("Vision List Replaced!");
        yield return new WaitForSeconds(1.0f);
        _visionChk = true;
        Global.isNewlyVision = true;
    }
}