using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TeamPanelManager : MonoBehaviour {

    AssetBundleManager assetBundleManager;
    public GameObject[] miceProperty;

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

    public void OnMiceClick(GameObject btn_name)
    {
        Debug.Log("btn_name : " + btn_name.ToString().Remove(0, 4));
        Debug.Log("mice name : " + btn_name.transform.GetChild(0).name);
        assetBundleManager.LoadAtlas(btn_name.transform.GetChild(0).name, typeof(Texture));
        assetBundleManager.LoadAtlas(btn_name.transform.GetChild(0).name, typeof(Material));
        assetBundleManager.LoadAtlas(btn_name.transform.GetChild(0).name, typeof(GameObject));

        foreach (KeyValuePair<string, object> item in Global.miceProperty)
        {
            Debug.LogWarning("We can see this is Dictionary Object:" + item.Key);
            string tmp = btn_name.ToString().Remove(0, 4);
            if ("1" == item.Key.ToString())
            {
                Debug.LogError("SEE YOU!!!!!!!!!");
                var innDict = item.Value as Dictionary<string, object>;
                var i = 0;
                foreach (KeyValuePair<string, object> inner in innDict)
                {
                    miceProperty[i].transform.GetComponent<UILabel>().text = inner.Value.ToString();
                    Debug.Log(" ******Value: " + inner.Value.ToString());
                    i++;
                    if (i == 4) break;
                }
            }
        }
        
    }

    public void OnMiceDrag(string miceName)
    {

    }
}
