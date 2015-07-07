using UnityEngine;
using System.Collections;
using System.IO;

public class PanelManager : MonoBehaviour
{

    public string pathFile;
    public GameObject[] Panel;
    public string[] assetName;

    public bool bundleLoaded { get { return _bundleLoaded; } }

    private GameObject clone;
    private GameObject myParent;
    private AssetBundle bundle;
    private AssetBundleRequest request;

    private WWW wwwAsset;
    private bool isStartLoadAsset;
    private bool _bundleLoaded;


    void Awake()
    {
        isStartLoadAsset = false;
        _bundleLoaded = false;
    }

    public void LoadPlayerPanel()
    {
        LoadAsset(assetName[0], 0);
    }

    public void LoadTeamPanel()
    {
        LoadAsset(assetName[1], 1);
    }

    /*
    IEnumerator LoadAsset(string assetName,int fileNum)
    {
        isStartLoadAsset = true;
        Debug.Log(Application.persistentDataPath + "/AssetBundles/" + pathFile + assetName);
        wwwAsset = new WWW("file:///" + Application.persistentDataPath + "/AssetBundles/" + pathFile);      // "file:///" 要3槓 扯
        yield return wwwAsset;

        AssetBundle bundle = wwwAsset.assetBundle;
        AssetBundleRequest request = bundle.LoadAsync(assetName, typeof(GameObject));


        if (wwwAsset.error != null)
        {
            Debug.LogError(wwwAsset.error);
        }
        else if (wwwAsset.isDone)
        {
            Panel[fileNum].SetActive(true);
            clone = (GameObject)Instantiate(request.asset);
            clone.transform.parent = Panel[fileNum].transform;
            clone.transform.localPosition = Vector3.zero;
            clone.transform.localScale = Vector3.one;

            wwwAsset.assetBundle.Unload(false);
            wwwAsset.Dispose();
            Resources.UnloadUnusedAssets();
        }
    }
    */

    void LoadAsset(string assetName, int fileNum)
    {
        StartCoroutine(LoadAtlas(assetName));
        StartCoroutine(LoadMat(assetName));
        StartCoroutine(LoadPerfab(assetName));
        StartCoroutine(LoadGameObject(assetName, fileNum));
    }

    IEnumerator LoadAtlas(string assetName)
    {
        Debug.Log("file:///" + Application.persistentDataPath + "/AssetBundles/" + assetName + "Atlas.unity3d");
        WWW www = WWW.LoadFromCacheOrDownload("file:///" + Application.persistentDataPath + "/AssetBundles/" + assetName + "Atlas.unity3d", 3);
        yield return www;

        if (www.error != null)
        {
            Debug.LogError(wwwAsset.error);
        }
        else if (www.isDone)
        {
            AssetBundle bundle = www.assetBundle;
            AssetBundleRequest request = bundle.LoadAsync(assetName+"Atlas", typeof(Texture));
            Debug.Log(request.asset.name);
            yield return new WaitForSeconds(0.5f);

        }
    }

    IEnumerator LoadMat(string assetName)
    {
        Debug.Log("file:///" + Application.persistentDataPath + "/AssetBundles/" + assetName + "Mat.unity3d");
        WWW www = WWW.LoadFromCacheOrDownload("file:///" + Application.persistentDataPath + "/AssetBundles/" + assetName + "Mat.unity3d", 3);
        yield return www;

        if (www.error != null)
        {
            Debug.LogError(wwwAsset.error);
        }
        else if (www.isDone)
        {
            AssetBundle bundle = www.assetBundle;
            AssetBundleRequest request = bundle.LoadAsync(assetName + "Mat", typeof(Material));
            Debug.Log(request.asset.name);
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator LoadPerfab(string assetName)
    {
        Debug.Log("file:///" + Application.persistentDataPath + "/AssetBundles/" + assetName + "Perfab.unity3d");
        WWW www = WWW.LoadFromCacheOrDownload("file:///" + Application.persistentDataPath + "/AssetBundles/" + assetName + "Perfab.unity3d", 3);
        yield return www;

        if (www.error != null)
        {
            Debug.LogError(wwwAsset.error);
        }
        else if (www.isDone)
        {
            AssetBundle bundle = www.assetBundle;
            AssetBundleRequest request = bundle.LoadAsync(assetName + "Perfab", typeof(GameObject));
            Debug.Log(request.asset.name);
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator LoadGameObject(string assetName, int fileNum)
    {
        yield return new WaitForSeconds(3);
        isStartLoadAsset = true;
        Debug.Log(Application.persistentDataPath + "/AssetBundles/" + pathFile);
        wwwAsset = WWW.LoadFromCacheOrDownload("file:///" + Application.persistentDataPath + "/AssetBundles/" + pathFile, 3);
        yield return wwwAsset;


        if (wwwAsset.error != null)
        {
            Debug.LogError(wwwAsset.error);
        }
        else if (wwwAsset.isDone)
        {
            AssetBundle bundle = wwwAsset.assetBundle;
            AssetBundleRequest request = bundle.LoadAsync(assetName, typeof(GameObject));
            Panel[fileNum].SetActive(true);
            clone = (GameObject)Instantiate(request.asset);
            clone.transform.parent = Panel[fileNum].transform;
            clone.transform.localPosition = Vector3.zero;
            clone.transform.localScale = Vector3.one;

            wwwAsset.assetBundle.Unload(false);
            wwwAsset.Dispose();
            Resources.UnloadUnusedAssets();
        }
    }

    void Update()
    {
        if (isStartLoadAsset)
            if (wwwAsset.progress < 0.9)
            {
                Debug.Log("progress:" + (int)(wwwAsset.progress * 100) + "%");
            }
            else if (wwwAsset.progress == 1)
            {
                Debug.Log("progress:" + (int)(wwwAsset.progress * 100) + "%");
                isStartLoadAsset = false;
                _bundleLoaded = true;
            }
    }
}
