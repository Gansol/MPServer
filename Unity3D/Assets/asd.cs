using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class asd : MonoBehaviour {

    AssetLoader a;
	// Use this for initialization

    void Awake()
    {
        a = gameObject.AddComponent<AssetLoader>();
    }

	void Start () {
        if (!string.IsNullOrEmpty(a.ReturnMessage))
            Debug.Log(a.ReturnMessage);

        a.LoadAsset("Panel/", "ComicFont");
        a.LoadAsset("Panel/", "LiHeiProFont");
        a.LoadPrefab("Panel/", "Label");
       
	}
	
	// Update is called once per frame
	void Update () {
        if (a.loadedObj)
            Instantiate(a.GetAsset("Label"));
	}

    void OnGUI()
    {
     
    }
}
