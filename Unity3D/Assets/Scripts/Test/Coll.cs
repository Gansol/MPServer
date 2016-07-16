using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public class Coll : MonoBehaviour {

    AssetLoader loader;

	void Start () {
        gameObject.AddComponent<AssetLoader>();
        loader = GetComponent<AssetLoader>();
        loader.LoadAsset("MiceICON/", "MiceICON");
        loader.LoadPrefab("MiceICON/", "EggICON");
        
        Debug.Log("Start End");
	}

    void Update()
    {
        if (loader.loadedObj)
        {
            loader.init();
            SS();
        }
    }

    void SS()
    {
        Instantiate(AssetBundleManager.getAssetBundle("MiceICON/EggICON").mainAsset as GameObject);
    }


    public bool Completed { get; set; }
}
