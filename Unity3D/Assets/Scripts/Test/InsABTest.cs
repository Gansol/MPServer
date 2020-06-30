using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class InsABTest : MonoBehaviour
{
    // public static string sourcePath = Application.dataPath + "/Assetbundles";
    const string AssetBundlesOutputPath = "/_Assetbundles/Android/Panel";
    GameObject obj;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadAsset("/share/PanelUI.unity3d", "tutorial")); ;

        StartCoroutine(LoadAsset("/unique/tutorial.unity3d", "tutorial")); ;


    }

    private void Update()
    {


    }

    IEnumerator LoadAsset(string assetBundleName, string objectNameToLoad)
    {
        string filePath = Application.dataPath + AssetBundlesOutputPath + assetBundleName.ToString();

        Debug.Log("Path:" + filePath);

        //Load "animals" AssetBundle
        var assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(filePath);
        yield return assetBundleCreateRequest;

        AssetBundle asseBundle = assetBundleCreateRequest.assetBundle;

        Debug.Log("asseBundle.name" + asseBundle.name);
        //Load the "dog" Asset (Use Texture2D since it's a Texture. Use GameObject if prefab)
        AssetBundleRequest asset = asseBundle.LoadAssetAsync<GameObject>(objectNameToLoad);
        yield return asset;

        Debug.Log("asset.name" + asset.isDone);

        //Retrieve the object (Use Texture2D since it's a Texture. Use GameObject if prefab)
        GameObject loadedAsset = asset.asset as GameObject;

        //Do something with the loaded loadedAsset  object (Load to RawImage for example) 
        obj = loadedAsset;

        Instantiate(obj);
    }

}
