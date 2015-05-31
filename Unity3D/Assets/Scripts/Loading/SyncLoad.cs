using UnityEngine;
using System.Collections;

public class SyncLoad : MonoBehaviour {

    public int nextLevel;
	// Use this for initialization
	void Start () {
        Global.loadScene = nextLevel;
        Global.photonService.LoadSceneEvent += OnLoadScene;
        Global.photonService.ExitRoomEvent += OnLoadScene;
        Debug.Log("Scene" + Application.loadedLevelName);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnLoadScene()
    {
        Application.LoadLevel(2);
    }
}
