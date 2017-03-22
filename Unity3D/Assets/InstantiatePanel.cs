using UnityEngine;
using System.Collections;

public class InstantiatePanel : MonoBehaviour {
    AssetLoader assetLoader = null;

	// Use this for initialization
	void Start () {


        if (Application.loadedLevelName == "MainGame")
            Instantiate(assetLoader.GetAsset("MenuUI"));


        if (Application.loadedLevelName == "Battle")
            Instantiate(assetLoader.GetAsset("GameUI"));

        
	}
	
	// Update is called once per frame
	void Update () {
	}
}
