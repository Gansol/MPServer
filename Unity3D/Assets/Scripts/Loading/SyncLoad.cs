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
        AssetBundleManager.UnloadUnusedAssets();
        System.GC.Collect();
    }

    void Start()
    {
        assetLoader = gameObject.AddMissingComponent<AssetLoader>();
        LoadAssetCheck();
    }

    void Update()
    {
        if (assetLoader.loadedObj && bLoadAsset)
        {
            InstantiateScene();
            bLoadAsset = !bLoadAsset;
        }
    }

    public void OnLoadScene()       // LoadScene
    {
        Global.photonService.LoadSceneEvent -= OnLoadScene;
        Application.LoadLevel((int)Global.Scene.LoadScene);
    }

    private void LoadAssetCheck()
    {
        if (Application.loadedLevel == (int)Global.Scene.BundleCheck)
        {
            bLoadAsset = true;
        }

        if (Application.loadedLevel == (int)Global.Scene.MainGame)
        {
            //assetLoader.LoadAsset("Panel/", "LiHeiProFont");
            assetLoader.LoadAsset("Panel/", "ComicFont");
            assetLoader.LoadAsset("Panel/", "ComicFontB");
            assetLoader.LoadAsset("Panel/", "PanelUI");
            assetLoader.LoadAsset("Panel/", "GameScene");
            assetLoader.LoadAsset("Panel/", "MainFront");
            assetLoader.LoadAsset("Panel/", "MainBack");
            assetLoader.LoadAsset("Panel/", "ShareObject");
            assetLoader.LoadPrefab("Panel/", "MenuUI");
            bLoadAsset = true;
        }

        if (Application.loadedLevel == (int)Global.Scene.Battle)
        {
            assetLoader.LoadAsset("Panel/", "BattleHUD");
            assetLoader.LoadPrefab("Panel/", "GameUI");
            bLoadAsset = true;
        }
    }

    private void InstantiateScene()
    {
        string sceneName = "";

        switch (Application.loadedLevelName)
        {
            case "BundleCheck":
                sceneName = "MainGame";
                break;
            case "MainGame":
                sceneName = "MenuUI";
                break;
            case "Battle":
                sceneName = "GameUI";
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

            if (Global.prevScene == (int)Global.Scene.MainGame)
                Global.dictLoadedScene["MenuUI"].SetActive(false);
            if (Global.nextScene == (int)Global.Scene.MainGame)
                Global.dictLoadedScene["MenuUI"].SetActive(true);



            if (Application.loadedLevelName != "BundleCheck")
            {
                _clone.transform.FindChild("HUDCamera").GetComponent<Camera>().enabled = false;
                _clone.transform.FindChild("HUDCamera").GetComponent<Camera>().enabled = true;
            }

        Global.photonService.LoadSceneEvent += OnLoadScene;

        Global.prevScene = Application.loadedLevel;


    }

}
