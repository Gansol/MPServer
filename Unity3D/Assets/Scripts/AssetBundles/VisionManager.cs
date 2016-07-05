using UnityEngine;

public class VisionManager : MonoBehaviour
{
    private AssetBundleChecker bundleChecker;
    private AssetBundlesDownloader downloader; //下載用
    private SyncLoad syncLoad;
    private VisionChecker visionChecker;
    private bool bundleCheckComplete = false;

    void Start() //開始檢查版本
    {
        downloader = gameObject.AddComponent<AssetBundlesDownloader>();
        visionChecker = gameObject.AddComponent<VisionChecker>();
        bundleChecker = gameObject.AddComponent<AssetBundleChecker>();
        StartCoroutine(visionChecker.CheckVision());
    }

    void Update() //等待完成
    {

        #region BundleCheck 檢查檔案
        try
        {
            if (!Global.isCompleted) //全部完成 停止全部比較作業
            {
                if (Global.isNewlyVision && !Global.isVisionDownload) //版本太舊 開始下載
                {
                    Debug.Log("Your vision outdatad ! ");
                    Global.isVisionDownload = true;
                    Debug.Log("Start Downloading . . .");
                    StartCoroutine(downloader.DownloadList(Global.sDownloadList));
                }
                else if (Global.isCheckBundle)//版本相同 檢查資源 (第一次檢查)
                {
                    Debug.Log("開始檢查遊戲資源. . .");
                    Global.isCheckBundle = false;
                    StartCoroutine(bundleChecker.StartCheck());
                }

            }
            else if (!Global.isReplaced)//替換最新資源列表
            {
                StartCoroutine(visionChecker.ReplaceVisionList());
                Global.isReplaced = true;
            }
        }
        catch (System.IO.IOException e)
        {
            Debug.Log("Vision Manager Error: " + e);
        }

        #endregion

        #region Load MainGame 載入遊戲
        if (bundleChecker.bundleChk && visionChecker.visionChk)
        {
            syncLoad = gameObject.AddComponent<SyncLoad>();
            syncLoad.LoadMainGame();
        }
        #endregion
    }
}