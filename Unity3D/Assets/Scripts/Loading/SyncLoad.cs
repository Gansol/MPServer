using UnityEngine;
using System.Collections;

public class SyncLoad : MonoBehaviour
{
    public int nextLevel;
    // Use this for initialization


    void Awake()
    {
        AssetBundleManager.UnloadUnusedAssets();
        System.GC.Collect();
    }

    void Start()
    {
        Global.loadScene = nextLevel;
        if(Application.loadedLevelName=="MainGame")
            Global.photonService.LoadSceneEvent += OnLoadScene;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadMainGame()
    {
        Application.LoadLevel(1);
    }

    void OnLoadScene()      // MainGame > Battle
    {
        Global.photonService.LoadSceneEvent -= OnLoadScene;
        Global.photonService.ExitRoomEvent += OnExitRoom;
        Application.LoadLevel(2);

    }

    void OnExitRoom()       // MainGame < Battle
    {
        Global.photonService.ExitRoomEvent -= OnExitRoom;
        Application.LoadLevel(1);

    }
}
