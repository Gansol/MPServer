using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using Gansol;
using System.Web;
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
 * 負責 檢查版本、資產列表
 * 未來需加入 大版本檢查
 * ***************************************************************
 *                           ChangeLog
 * 20160714 v1.0.1  修正部分問題                                       
 * ****************************************************************/
public class PatchFileChecker
{
    //    AssetBundlesHash bundleHash; //hash文件用
    private byte[] _bVisionFile; //暫存 伺服器版本列表
    private int reConnTimes = 0;
    private bool _patcherChk;
    private TextUtility txtUtil;
    public bool PatcherChk { get { return _patcherChk; } }


    void Awake()
    {
        reConnTimes = 0;
    }

    #region GetPatcher
    public IEnumerator GetPatcher() //取得patch路徑
    {

        string localListPath = Application.persistentDataPath + "/List/";
        string localVisionListFile = localListPath + Global.patchFile;
        if (!Directory.Exists(localListPath)) Directory.CreateDirectory(localListPath);
        //        Debug.Log("Eclipse Debug : " + localListPath + Directory.Exists(localListPath));
        // Debug.LogError("Eclipse Debug : " + localVisionListFile);
        txtUtil = new TextUtility();
    ReCheckFlag:
        string patch = txtUtil.DecryptBase64String(Global.serverPath);
        using (UnityWebRequest wwwPatcher =  UnityWebRequest.Get(patch + Global.patchFile))
        {
            yield return wwwPatcher.SendWebRequest();

            if (reConnTimes >= Global.maxConnTimes)        // 如果出現網路錯誤，停止檢測版本，並提示網路錯誤
            {
                Global.ReturnMessage = "無法連線至伺服器，請檢查網路狀態!";
                Debug.LogError("Can't connecting to Server! Please check your network status!");
                wwwPatcher.Dispose();
            }
            else if (wwwPatcher.error != null && reConnTimes < Global.maxConnTimes)  // 如果出現網路錯誤，重新連線下載，並提示重連次數
            {
                reConnTimes++;
                Global.ReturnMessage = "無法下載更新列表，嘗試重新下載(" + reConnTimes + "/" + Global.maxConnTimes + ")";
                Debug.Log("Download Vision List Error !   " + wwwPatcher.error + "\n Wait for one second. Reconnecting to download(" + reConnTimes + ")");
                //   wwwVisionList.Dispose();
                yield return new WaitForSeconds(1.0f);
                goto ReCheckFlag;
            }
            else if (wwwPatcher.isDone && wwwPatcher.error == null)    //開始檢查版本
            {
                reConnTimes = 0;

                _bVisionFile = System.Text.Encoding.UTF8.GetBytes(wwwPatcher.downloadHandler.text); // 儲存 下載好的檔案版本

                string ss = System.Text.Encoding.UTF8.GetString(_bVisionFile);
                string path = wwwPatcher.downloadHandler.text.Replace("\n","");
                path = wwwPatcher.downloadHandler.text.Replace("\\", "");
                path = wwwPatcher.downloadHandler.text.Replace("/", "");
                Debug.Log(path);
                Global.serverPath = txtUtil.DecryptBase64String("aHR0cDovLzE4MC4yMTguMTY0LjIzMjo1ODc2Ny9NaWNlUG93QkVUQQ==");
                _patcherChk = true;
            }
        }
    }
    #endregion
}