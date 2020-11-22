using UnityEngine;
using UnityEngine.SceneManagement;

public class SyncLoad : MonoBehaviour
{
    AssetLoader assetLoader;
    private bool _bLoadSceneAsset;
    private GameObject _scene;
    MPGame m_MPGame;

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

        // 如果場景資產已經載入 實體化場景物件
        if (assetLoader.bLoadedObj && _bLoadSceneAsset)
            InstantiateScene();
    }

    public void OnLoadScene()       // LoadScene
    {
        // 顯示 "載入場景"  載入下一個場景
        SceneManager.LoadScene(Global.Scene.LoadScene);
    }

    /// <summary>
    /// 確認要載入的場景 並載入資產
    /// </summary>
    private void LoadAssetCheck()
    {
        if (SceneManager.GetActiveScene().name == Global.Scene.BundleCheck)
        {
            _bLoadSceneAsset = true;
        }

        if (SceneManager.GetActiveScene().name == Global.Scene.MainGame)
        {
            assetLoader.LoadAssetFormManifest(Global.PanelUniquePath + Global.Scene.MainGameAsset + Global.ext);
            _bLoadSceneAsset = true;
        }

        if (SceneManager.GetActiveScene().name == Global.Scene.Battle)
        {
            assetLoader.LoadAssetFormManifest(Global.PanelUniquePath + Global.Scene.BattleAsset + Global.ext);
            _bLoadSceneAsset = true;
        }
    }

    /// <summary>
    /// 實體化場景物件
    /// </summary>
    private void InstantiateScene()
    {
        string sceneAssetName = null;
        _bLoadSceneAsset = !_bLoadSceneAsset;

        // 選擇場景對應資產
        switch (SceneManager.GetActiveScene().name)
        {
            //case Global.Scene.BundleCheck:
            //    sceneName = Global.Scene.MainGame;
                //break;
            case Global.Scene.MainGame:
                sceneAssetName = Global.Scene.MainGameAsset;
                break;
            case Global.Scene.Battle:
                sceneAssetName = Global.Scene.BattleAsset;
                break;
        }

        // 確認是否已經實體化
        if (Global.dictLoadedScene.ContainsKey(sceneAssetName))
            Global.dictLoadedScene.TryGetValue(sceneAssetName, out _scene);

        // 如果場景資產已經載入 且 尚未實體化
        if (AssetBundleManager.GetLoadedAssetbundle(AssetBundleManager.GetAssetBundleNamePath(sceneAssetName)) && _scene == null)
        {
            _scene = Instantiate(assetLoader.GetAsset(sceneAssetName)) as GameObject;
            _scene.name = sceneAssetName;

            // 是否存在場景索引
            if (!Global.dictLoadedScene.ContainsKey(sceneAssetName))
                Global.dictLoadedScene.Add(_scene.name, _scene);
            else
                Global.dictLoadedScene[sceneAssetName] = _scene;
        }

        //錯誤 這裡以後需要改寫 目前只有 MainGame和Battle在做切換隱藏/顯示
        if (Global.prevScene == Global.Scene.MainGame)
            Global.dictLoadedScene[Global.Scene.MainGameAsset].SetActive(false);

        if (Global.nextScene == Global.Scene.MainGame)
            Global.dictLoadedScene[Global.Scene.MainGameAsset].SetActive(true);
            

        // 防止HUDCamera無法顯示
        if (SceneManager.GetActiveScene().name != Global.Scene.BundleCheck)
        {
            _scene.transform.Find("HUDCamera").GetComponent<Camera>().enabled = false;
            _scene.transform.Find("HUDCamera").GetComponent<Camera>().enabled = true;
        }
        MPGame.Instance.LoadedScene(SceneManager.GetActiveScene().name);
        MPGame.Instance.InitScene(_scene);
        // 儲存目前場景
        Global.prevScene = SceneManager.GetActiveScene().name;
    }

    void OnDestory()
    {
        Global.photonService.LoadSceneEvent -= OnLoadScene;
    }
}
