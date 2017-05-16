using UnityEngine;
using System.Collections;
using System.IO;
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
 * 負責 檢查版本、資產列表
 * 未來需加入 大版本檢查
 * ***************************************************************
 *                           ChangeLog
 * 20160714 v1.0.1  修正部分問題                                       
 * ****************************************************************/
public class VersionChecker
{
    //    AssetBundlesHash bundleHash; //hash文件用
    private byte[] _bVisionFile; //暫存 伺服器版本列表
    private int reConnTimes = 0;
    private bool _visionChk;
    private bool _isNewlyVision;
    private string _vision;

    public bool visionChk { get { return _visionChk; } }
    public string vision { get { return _vision; } }
    public bool isNewlyVision { get { return _isNewlyVision; } }

    void Start()
    {
        reConnTimes = 0;
    }

    #region CheckVision
    public IEnumerator CheckVision() //檢查版本
    {

        string localListPath = Application.persistentDataPath + "/List/";
        string localVisionListFile = localListPath + Global.visionListFile;
        if (!Directory.Exists(localListPath)) Directory.CreateDirectory(localListPath);
    //        Debug.Log("Eclipse Debug : " + localListPath + Directory.Exists(localListPath));
    // Debug.LogError("Eclipse Debug : " + localVisionListFile);
    ReCheckFlag:
        using (WWW wwwVisionList = new WWW(Global.serverListPath + Global.visionListFile))
        {

            yield return wwwVisionList;

            //Debug.Log("FILE Contain= " + www.text);

            //            Debug.Log("IN CHECK VISION");

            if (reConnTimes >= Global.maxConnTimes)        // 如果出現網路錯誤，停止檢測版本，並提示網路錯誤
            {
                Global.ReturnMessage = "無法連線至伺服器，請檢查網路狀態!";
                Debug.LogError("Can't connecting to Server! Please check your network status!");
                wwwVisionList.Dispose();
            }
            else if (wwwVisionList.error != null && reConnTimes < Global.maxConnTimes)  // 如果出現網路錯誤，重新連線下載，並提示重連次數
            {
                reConnTimes++;
                Global.ReturnMessage = "無法下載更新列表，嘗試重新下載(" + reConnTimes + "/" + Global.maxConnTimes + ")";
                Debug.Log("Download Vision List Error !   " + wwwVisionList.error + "\n Wait for one second. Reconnecting to download(" + reConnTimes + ")");
                //   wwwVisionList.Dispose();
                 yield return new WaitForSeconds(1.0f);
                goto ReCheckFlag;
            }
            else if (wwwVisionList.isDone && wwwVisionList.error == null)    //開始檢查版本
            {
                reConnTimes = 0;
                _bVisionFile = System.Text.Encoding.UTF8.GetBytes(wwwVisionList.text); // 儲存 下載好的檔案版本

                // 如果本機 版本列表檔案 不存在 建立空檔
                if (!File.Exists(localVisionListFile))
                {

                    File.Create(localVisionListFile).Close();
                    File.WriteAllText(localVisionListFile, Global.defaultVersion);
                }

                string clientVersionText = File.ReadAllText(localVisionListFile);

                Dictionary<string, object> clientVersion = MiniJSON.Json.Deserialize(clientVersionText) as Dictionary<string, object>;
                Dictionary<string, object> serverVersion = MiniJSON.Json.Deserialize(wwwVisionList.text) as Dictionary<string, object>;
                List<string> clientkeys = new List<string>(clientVersion.Keys);
                List<string> serverkeys = new List<string>(serverVersion.Keys);

                //Dictionary<KEY(鍵值),Vaule(值)> 
                //foreach ( 遞增值(陣列內涵值) in 陣列 )
                //取得伺服器版本列表
                if (clientVersion[clientkeys[0]].ToString() != serverVersion[serverkeys[0]].ToString())
                {
                    Debug.Log("遊戲版本: Ver. " + serverVersion[serverkeys[0]].ToString());
                    Global.gameVersion = _vision = serverVersion[serverkeys[0]].ToString();
                    Global.ReturnMessage = "不是最新版本!";
                    Debug.Log("不是最新版本!");

                    _isNewlyVision = false;
                    wwwVisionList.Dispose();
                    _visionChk = true;  // 加入大版本時刪除並改寫 錯誤
                    //to do check v1.0.0 != v2.0.0 > GooglePlay
                }
                else //版本相同 再次檢查檔案 (這是在版本列表不同下的 再次檢查)
                {
                    Debug.Log("遊戲版本: Ver. " + serverVersion[serverkeys[0]].ToString());
                    Global.gameVersion = _vision = serverVersion[serverkeys[0]].ToString();
                    Global.ReturnMessage = "本機檔案為最新版本!";
                    Debug.Log("本機檔案為最新版本!");

                    _isNewlyVision = true;
                    wwwVisionList.Dispose();
                    _visionChk = true;
                }


            }
        }
    }
    #endregion

    #region ReplaceVisionList
    /// <summary>
    /// 更新完成後 取代版本列表(單純置換版本列表)
    /// </summary>
    /// <returns></returns>
    public IEnumerator ReplaceVisionList()
    {
        File.WriteAllBytes(Application.persistentDataPath + "/List/" + Global.visionListFile, _bVisionFile); //WriteAllBytes(要寫入的路徑與檔案名稱!!!不能只寫路徑(關鍵),bytes檔案)
        Global.ReturnMessage = "遊戲版本檢查完成!";
        Debug.Log("遊戲版本檢查完成!");
        yield return new WaitForSeconds(1.0f);
        _visionChk = true;
        Global.isNewlyVision = true;
    }
    #endregion
}