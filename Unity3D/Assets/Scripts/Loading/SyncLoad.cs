﻿using UnityEngine;
using System.Collections;

public class SyncLoad : MonoBehaviour
{

    public int nextLevel;
    // Use this for initialization
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

    void OnLoadScene()      // MainGame > Battle
    {
        Global.photonService.LoadSceneEvent -= OnLoadScene;
        Global.photonService.ExitRoomEvent += OnExitRoom;
        Application.LoadLevel(2);

    }

    void OnExitRoom()       // MainGame < Battle
    {
        Global.photonService.ExitRoomEvent -= OnExitRoom;
        Application.LoadLevel(2);

    }
}