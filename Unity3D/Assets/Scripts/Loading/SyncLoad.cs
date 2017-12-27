using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class SyncLoad : MonoBehaviour
{
    AssetLoader assetLoader;

    private bool bLoadAsset;
    private GameObject _clone;


    void Awake()
    {
        Global.photonService.LoadSceneEvent += OnLoadScene;
        AssetBundleManager.UnloadUnusedAssets();
        System.GC.Collect();
    }

    void Start()
    {
        assetLoader = MPGame.Instance.GetAssetLoader();
        LoadAssetCheck();
    }

    void Update()
    {
        //if (!string.IsNullOrEmpty(assetLoader.ReturnMessage))
        //    Debug.Log("訊息：" + assetLoader.ReturnMessage);

        if (assetLoader.loadedObj && bLoadAsset)
        {
            InstantiateScene();
            bLoadAsset = !bLoadAsset;
        }
    }

    public void OnLoadScene()       // LoadScene
    {
        Application.LoadLevel(Global.Scene.LoadScene);
    }

    private void LoadAssetCheck()
    {
        if (Application.loadedLevelName == Global.Scene.BundleCheck)
        {
            bLoadAsset = true;
        }

        if (Application.loadedLevelName == Global.Scene.MainGame)
        {
            assetLoader.init();
            assetLoader.LoadAsset("Panel/", "LiHeiProFont");
            assetLoader.LoadAsset("Panel/", "ComicFont");
            assetLoader.LoadAsset("Panel/", "ComicFontB");

            assetLoader.LoadAsset("Panel/", "PanelUI");
            assetLoader.LoadAsset("Panel/", "GameScene");
            assetLoader.LoadAsset("Panel/", "MainFront");
            assetLoader.LoadAsset("Panel/", "MainBack");
            assetLoader.LoadAsset("Panel/", "ShareObject");

            assetLoader.LoadPrefab("Panel/", Global.Scene.MainGameAsset);
            bLoadAsset = true;
        }

        if (Application.loadedLevelName == Global.Scene.Battle)
        {
            assetLoader.init();
            assetLoader.LoadAsset("Panel/", "BattleHUD");
            assetLoader.LoadPrefab("Panel/", Global.Scene.BattleAsset);
            bLoadAsset = true;
        }
    }

    private void InstantiateScene()
    {
        string sceneName = "";

        switch (Application.loadedLevelName)
        {
            case Global.Scene.BundleCheck:
                sceneName = Global.Scene.MainGame;
                break;
            case Global.Scene.MainGame:
                sceneName =  Global.Scene.MainGameAsset;
                break;
            case  Global.Scene.Battle:
                sceneName = Global.Scene.BattleAsset;
                break;
        }

        if (Global.dictLoadedScene.ContainsKey(sceneName))
            Global.dictLoadedScene.TryGetValue(sceneName, out _clone);

        if (AssetBundleManager.bLoadedAssetbundle(sceneName) && _clone == null)
        {

            _clone = Instantiate(assetLoader.GetAsset(sceneName)) as GameObject;
            _clone.name = sceneName;
            if (!Global.dictLoadedScene.ContainsKey(sceneName))
                Global.dictLoadedScene.Add(_clone.name, _clone);
            else
                Global.dictLoadedScene[sceneName] = _clone;
        }

        if (Global.prevScene == Global.Scene.MainGame)
            Global.dictLoadedScene[Global.Scene.MainGameAsset].SetActive(false);
        if (Global.nextScene == Global.Scene.MainGame)
            Global.dictLoadedScene[Global.Scene.MainGameAsset].SetActive(true);



        if (Application.loadedLevelName != "BundleCheck")
        {
            _clone.transform.FindChild("HUDCamera").GetComponent<Camera>().enabled = false;
            _clone.transform.FindChild("HUDCamera").GetComponent<Camera>().enabled = true;
        }

        Global.prevScene = Application.loadedLevelName;


    }

    void OnDestory()
    {
        Global.photonService.LoadSceneEvent -= OnLoadScene;
    }

}
