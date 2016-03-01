using UnityEngine;
using System.Collections;

public class TeamPanelManager : MonoBehaviour {

    AssetBundleManager assetBundleManager;

    void Awake()
    {
        assetBundleManager = new AssetBundleManager();
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnMiceClick(string miceName)
    {
        assetBundleManager.LoadAtlas(miceName, typeof(Texture));
        assetBundleManager.LoadAtlas(miceName, typeof(Material));
        assetBundleManager.LoadAtlas(miceName, typeof(GameObject));
    }

    public void OnMiceDrag(string miceName)
    {

    }
}
