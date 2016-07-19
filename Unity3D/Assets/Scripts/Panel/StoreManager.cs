using UnityEngine;
using System.Collections;

public class StoreManager : MonoBehaviour {

    AssetLoader assetLoader;

	void Start () {
        assetLoader = gameObject.AddComponent<AssetLoader>();
	}
	

	void Update () {
	
	}

    void OnMessage()
    {
        Debug.Log("Stroe Recive Message!");
    }

}
