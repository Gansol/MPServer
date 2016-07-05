using UnityEngine;
using System.IO;
using System.Collections;

public class TestHash : MonoBehaviour {

//    AssetBundlesHash hash = new AssetBundlesHash();
    byte[] tmp;
    string aa;
	// Use this for initialization
	void Start () {
        
        tmp=File.ReadAllBytes(Application.persistentDataPath +"/AssetBundles/" +"b.unity3d");
        aa = AssetBundlesHash.SHA1Complier(tmp);
        Debug.Log(aa);
        
        /*
        CreateJSON createJSON = new CreateJSON();
        createJSON.AssetBundlesJSON();
         */
    }
	
}
