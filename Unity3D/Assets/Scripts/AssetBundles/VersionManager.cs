using UnityEngine;
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
 * 負責 檢查遊戲版本
 * ***************************************************************
 *                           ChangeLog
 * 20160714 v1.0.1  1次重構，版本、資產檢查組件化                                         
 * ****************************************************************/
public class VersionManager : MonoBehaviour
{
    public GameObject message;
    public UILabel vision, downloadName;

    private AssetBundleChecker bundleChecker;
    private SyncLoad syncLoad;
    private VersionChecker visionChecker;
    private AssetLoader assetLoader;
    InternetChecker connCheck;
    private bool bundleCheckComplete, flag, bFirstDownload, reConnChk, bTwiceChk, bThreeChk, bCompleted, bDownload, bGoStore;
    //private bool bLoadAsset;


    void Awake()
    {
        connCheck = gameObject.AddComponent<InternetChecker>();
    }

    void Start() //開始檢查版本
    {
        Global.nextScene = (int)Global.Scene.MainGame;
        assetLoader = gameObject.AddComponent<AssetLoader>();
        bundleChecker = gameObject.AddComponent<AssetBundleChecker>();
        visionChecker = new VersionChecker();
        Global.ReturnMessage = "開始檢查遊戲資源. . .";
        Global.prevScene = Application.loadedLevel;

        if (connCheck.ConnStatus)
            StartCoroutine(visionChecker.CheckVision());
        else
            reConnChk = !reConnChk;

        if (!File.Exists(Application.persistentDataPath + "/List/" + Global.visionListFile))
            bFirstDownload = true;
    }

    void Update() //等待完成
    {
        //Debug.Log(Global.ReturnMessage + "  visionChecker.visionChk: " + visionChecker.visionChk);

        if (!string.IsNullOrEmpty(Global.ReturnMessage))
            downloadName.text = Global.ReturnMessage;

        if (!string.IsNullOrEmpty(assetLoader.ReturnMessage))
            downloadName.text = assetLoader.ReturnMessage;

        if (!connCheck.ConnStatus)
        {
            message.SetActive(true);
            message.GetComponentInChildren<UILabel>().text = "請檢查網路連線狀態!";
        }
        else if (!Global.isCompleted && connCheck.ConnStatus)
        {
            message.SetActive(false);
        }

        #region BundleCheck 檢查檔案

        // 遊戲版本檢查
        if (reConnChk && connCheck.ConnStatus)
        {
            StartCoroutine(visionChecker.CheckVision());
            reConnChk = !reConnChk;
        }

        // 第一次檢查
        if (visionChecker.visionChk && connCheck.ConnStatus)
        {
            if (!visionChecker.isNewlyVision )
            {
                if (!bGoStore)
                {
                    bGoStore = !bGoStore;
                    Debug.Log("Go Google Play!");
                    Application.OpenURL("https://play.google.com/store/apps/details?id=com.Gansol.MicePow1");
                }
            }
            else
            {
                if (!bundleCheckComplete)
                {
                    bundleCheckComplete = true;
                    bundleChecker.StartCheck();
                }
            }
        }


        #endregion

        if (bFirstDownload)
        {
            Debug.Log(bFirstDownload);
            if (Global.isCompleted && !bTwiceChk && bundleChecker.bundleChk)
            {
                bundleChecker.StartCheck();
                Global.isCompleted = false;
                bDownload = bTwiceChk = true;
            }

            if (Global.isCompleted && !bundleChecker.bundleChk)
                bTwiceChk = true;
        }
        else
        {
            bTwiceChk = true;
        }

        if (Global.isCompleted && bTwiceChk && !bCompleted)
        {
            bCompleted = true;
            if (!bundleChecker.bundleChk)   // 如果沒有新增檔案
            {
                if (Global.connStatus) StartCoroutine(visionChecker.ReplaceVisionList());
                syncLoad = gameObject.AddComponent<SyncLoad>();
                syncLoad.OnLoadScene();
            }
            else if (!flag)
            {
                flag = !flag;
                message.SetActive(true);
                message.GetComponentInChildren<UILabel>().text = "更新完畢 請重新啟動遊戲！";
            }
        }
    }


    void OnGUI()
    {
        if (bundleCheckComplete)
            vision.text = "Ver." + Global.gameVersion + " build " + Global.bundleVersion;
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}