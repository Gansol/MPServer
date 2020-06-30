using UnityEngine;
using UnityEngine.SceneManagement;

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
        SceneManager.LoadScene(Global.Scene.LoadScene);
    }

    private void LoadAssetCheck()
    {

        if (SceneManager.GetActiveScene().name == Global.Scene.BundleCheck)
        {
            bLoadAsset = true;
        }

        if (SceneManager.GetActiveScene().name == Global.Scene.MainGame)
        {
            assetLoader.init();
            assetLoader.LoadAssetFormManifest(Global.PanelUniquePath, Global.PanelUniquePath+ "menuui.unity3d", "menuui");

            //assetLoader.LoadAsset("panel/share/", "liheiprofont");
            //assetLoader.LoadAsset("panel/share/", "comicfont");
            //assetLoader.LoadAsset("panel/", "comicfontoutline");

            //assetLoader.LoadAsset("panel/share/", "panelui");
            //assetLoader.LoadAsset("panel/share/", "gamescene");
            //assetLoader.LoadAsset("panel/share/", "mainfront");
            //assetLoader.LoadAsset("panel/share/", "mainback");
            //assetLoader.LoadAsset("panel/share/", "shareobject");

            //assetLoader.LoadPrefab("panel/", Global.Scene.MainGameAsset);
            bLoadAsset = true;
        }

        if (SceneManager.GetActiveScene().name == Global.Scene.Battle)
        {
            assetLoader.init();
            assetLoader.LoadAsset(Global.PanelPath, "battlehud");
            assetLoader.LoadPrefab(Global.PanelPath, Global.Scene.BattleAsset);
            bLoadAsset = true;
        }
    }

    private void InstantiateScene()
    {
        string sceneName = "";

        switch (SceneManager.GetActiveScene().name)
        {
            case Global.Scene.BundleCheck:
                sceneName = Global.Scene.MainGame;
                break;
            case Global.Scene.MainGame:
                sceneName = Global.Scene.MainGameAsset;
                break;
            case Global.Scene.Battle:
                sceneName = Global.Scene.BattleAsset;
                break;
        }

        if (Global.dictLoadedScene.ContainsKey(sceneName))
            Global.dictLoadedScene.TryGetValue(sceneName, out _clone);

        if (AssetBundleManager.bLoadedAssetbundle(Global.PanelUniquePath+ sceneName +Global.ext) && _clone == null)
        {
            string assetName = Global.PanelUniquePath + sceneName + Global.ext;

            _clone = Instantiate(assetLoader.GetAsset(assetName)) as GameObject;
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



        if (SceneManager.GetActiveScene().name != "BundleCheck")
        {
            _clone.transform.Find("HUDCamera").GetComponent<Camera>().enabled = false;
            _clone.transform.Find("HUDCamera").GetComponent<Camera>().enabled = true;
        }

        Global.prevScene = SceneManager.GetActiveScene().name;


    }

    void OnDestory()
    {
        Global.photonService.LoadSceneEvent -= OnLoadScene;
    }

}
