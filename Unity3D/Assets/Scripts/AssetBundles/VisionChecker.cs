using UnityEngine;
using System.Collections;
using System.IO;
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
public class VisionChecker
{
    //    AssetBundlesHash bundleHash; //hash文件用
    private byte[] _bVisionFile; //暫存 伺服器版本列表
    private int reConnTimes = 0;
    private bool _visionChk ;
    private bool _isNewlyVision;

    public bool visionChk { get { return _visionChk; } }
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
        if(!Directory.Exists(localListPath)) Directory.CreateDirectory(localListPath);
        Debug.Log("Eclipse Debug : " + localListPath + Directory.Exists(localListPath));
        // Debug.LogError("Eclipse Debug : " + localVisionListFile);
        using (WWW wwwVisionList = new WWW(Global.serverListPath + Global.visionListFile))
        {

            yield return wwwVisionList;

            //Debug.Log("FILE Contain= " + www.text);
        CheckVisionFlag:
            Debug.Log("IN CHECK VISION");

            if (wwwVisionList.error != null && reConnTimes < Global.maxConnTimes)  // 如果出現網路錯誤，重新連線下載，並提示重連次數
            {
                reConnTimes++;
                Global.ReturnMessage = "Download Vision List Error !   " + wwwVisionList.error + "\n Wait for one second. Reconnecting to download(" + reConnTimes + ")";
                Debug.Log("Download Vision List Error !   " + wwwVisionList.error + "\n Wait for one second. Reconnecting to download(" + reConnTimes + ")");
                wwwVisionList.Dispose();
                yield return new WaitForSeconds(1.0f);
                goto CheckVisionFlag;
            }
            else if (wwwVisionList.isDone)    //開始檢查版本
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
                if (!(AssetBundlesHash.SHA1Complier(File.ReadAllBytes(localVisionListFile)) == AssetBundlesHash.SHA1Complier(_bVisionFile))) //本機 比較 伺服器 版本
                {
                    Global.ReturnMessage = "不是最新版本!";
                    Debug.Log("不是最新版本!");
                    _isNewlyVision = false;
                    wwwVisionList.Dispose();
                    _visionChk = true;  // 加入大版本時刪除並改寫 錯誤
                    //to do check v1.0.0 > v2.0.0 > GooglePlay
                }
                else //版本相同 再次檢查檔案 (這是在版本列表不同下的 再次檢查)
                {
                    Global.ReturnMessage = "本機檔案為最新版本!";
                    Debug.Log("本機檔案為最新版本!");
                    _isNewlyVision = true;
                    wwwVisionList.Dispose();
                    _visionChk = true;
                }


            }

            else    // 如果出現網路錯誤，停止檢測版本，並提示網路錯誤
            {
                Global.ReturnMessage = "Can't connecting to Server! Please check your network status!";
                Debug.LogError("Can't connecting to Server! Please check your network status!");
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
        Global.ReturnMessage = "Vision List Updated!";
        Debug.Log("Vision List Updated!");
        yield return new WaitForSeconds(1.0f);
        _visionChk = true;
        Global.isNewlyVision = true;
    }
    #endregion
}