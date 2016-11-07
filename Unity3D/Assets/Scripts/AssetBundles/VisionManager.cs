using UnityEngine;
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
public class VisionManager : MonoBehaviour
{
    private AssetBundleChecker bundleChecker;
    private SyncLoad syncLoad;
    private VisionChecker visionChecker;
    private AssetLoader assetLoader;
    private bool bundleCheckComplete = false;

    void Start() //開始檢查版本
    {
        assetLoader = gameObject.AddComponent<AssetLoader>();
        //bundleChecker = gameObject.AddComponent<AssetBundleChecker>();
        //visionChecker = new VisionChecker();
        //Global.ReturnMessage = "開始檢查遊戲資源. . .";
        //StartCoroutine(visionChecker.CheckVision());
    }

    void Update() //等待完成
    {
        //Debug.Log(Global.ReturnMessage + "  visionChecker.visionChk: " + visionChecker.visionChk);


        if (!string.IsNullOrEmpty(assetLoader.ReturnMessage))
            Debug.Log(assetLoader.ReturnMessage);
        
        //#region BundleCheck 檢查檔案
        //if (visionChecker.visionChk)
        //{
        //    if (!bundleCheckComplete)
        //    {
        //        bundleCheckComplete = true;
        //        bundleChecker.StartCheck();
        //    }
        //}
        //#endregion
        
        //if (assetLoader.loadedObj && bLoadAsset)
        //{
        //    StartCoroutine(visionChecker.ReplaceVisionList());
        //    syncLoad = gameObject.AddComponent<SyncLoad>();
        //    syncLoad.LoadMainGame();
        //    //Instantiate(assetLoader.GetAsset("Sprite"));
        //}
        

        #region Load MainGame 載入遊戲
        if (/*Global.isCompleted &&*/ !bLoadAsset)
        {
            //assetLoader.LoadAsset("Panel/", "PanelUI");
            //assetLoader.LoadAsset("Panel/", "MainBack");
            //assetLoader.LoadAsset("Panel/", "MainFront");
            //assetLoader.LoadAsset("Panel/", "ComicFont");
            //assetLoader.LoadAsset("Panel/", "LiHeiProFont");
            //assetLoader.LoadAsset("Panel/", "ShareObject");
            //assetLoader.LoadPrefab("Panel/", "MenuUI");

            assetLoader.LoadAsset("Panel/", "PanelUI");
            assetLoader.LoadAsset("Panel/", "BattleHUD");
            assetLoader.LoadAsset("Panel/", "ShareObject");
            assetLoader.LoadAsset("Panel/", "GameScene");
            assetLoader.LoadAsset("Panel/", "MainFront");
            assetLoader.LoadAsset("Panel/", "ComicFont");
            assetLoader.LoadAsset("Panel/", "LiHeiProFont");
            //assetLoader.LoadPrefab("Panel/", "TestA");
            //assetLoader.LoadPrefab("Panel/", "TestB");
            //assetLoader.LoadPrefab("Panel/", "GameUI");
            assetLoader.LoadPrefab("Panel/", "MenuUI");

            //assetLoader.LoadAsset("New/", "LiHeiProFont");
            //assetLoader.LoadAsset("New/", "BattleHUD");
            //assetLoader.LoadAsset("New/", "PanelUI");
            //assetLoader.LoadPrefab("New/", "Sprite");

            //assetLoader.LoadAsset("TEST/", "BattleHUD");
            //assetLoader.LoadAsset("TEST/", "GameScene");
            ////assetLoader.LoadAsset("TEST/", "ComicFont");
            ////assetLoader.LoadAsset("TEST/", "ShareObject");
            //assetLoader.LoadPrefab("Panel/", "MenuUI");
            bLoadAsset = !bLoadAsset;
        }


        #endregion
    }

    public bool bLoadAsset { get; set; }
}