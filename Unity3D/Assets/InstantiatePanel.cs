using UnityEngine;
using System.Collections;

public class InstantiatePanel : MonoBehaviour {
    AssetLoader assetLoader;

	// Use this for initialization
	void Start () {
        assetLoader = gameObject.AddComponent<AssetLoader>();
        Instantiate(assetLoader.GetAsset("MenuUI"));
        
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
