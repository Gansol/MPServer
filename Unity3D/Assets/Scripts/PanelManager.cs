using UnityEngine;
using System.Collections;
using System.IO;

public class PanelManager : MonoBehaviour {

    public GameObject[] Panel;

    protected string path;
    public string pathFile;
    public string assetName;
    public int version;
    protected GameObject clone;
    protected GameObject myParent;
    protected AssetBundle bundle;
    private AssetBundleRequest request;

    void Start()
    {
        LoadAsset();
    }

    public void LoadAsset()
    {
        Debug.Log("(LOAD)Path Load= " + Application.persistentDataPath + "/AssetBundles/" + pathFile);
        bundle = AssetBundle.CreateFromFile(Application.persistentDataPath + "/AssetBundles/" + pathFile);
        request = bundle.LoadAsync("Team", typeof(GameObject));
        clone = request.asset as GameObject;
        Debug.Log(bundle);

        if ((File.Exists(Application.persistentDataPath + "/AssetBundles/panel.unity3d")))
            Debug.Log("yes");

        Panel[0].SetActive(true);
        clone = (GameObject)Instantiate(clone);
        clone.transform.parent = Panel[0].transform;
        clone.transform.localPosition =Vector3.zero;
        clone.transform.localScale = Vector3.one;
        //a.renderer.mainTexture = AssetBundle1.Load("贴图1") //載入貼圖
    }

}
