using UnityEngine;
using System.Collections;

public class dem : MonoBehaviour
{
    public GameObject coll;
    public GameObject coll2;
    public UIAtlas atlas;
    AssetLoaderSystem loader;
    void Start()
    {
        loader = MPGame.Instance.GetAssetLoaderSystem();
        //loader.LoadAsset("miceicon/", "miceicon");
        //loader.LoadPrefab("miceicon/", "icon_Eggmice");

        Debug.Log("coll pos:" + coll.transform.position);
        Debug.Log("coll2 pos:" + coll2.transform.position);

        Debug.Log("coll lpos:" + coll.transform.localPosition);
        Debug.Log("coll2 lpos:" + coll2.transform.localPosition);
        Debug.Log("coll2 l lpos:" + coll.transform.InverseTransformPoint(coll2.transform.position));


        coll.transform.localPosition += coll.transform.InverseTransformPoint(coll2.transform.position);
    }

    void Update()
    {/*
        if (loader.loadedObj && !flag)
        {
            AssetBundleManager.LoadedBundle();
            flag =!flag;
            Instantiate(loader.GetAsset("EggMiceICON"));
            Application.LoadLevel("test2");
        }*/
    }
    public void OnClick()
    {
        coll.transform.localPosition += coll.transform.InverseTransformPoint(coll2.transform.position);
        UISprite sprite = gameObject.GetComponent<UISprite>();
        sprite.spriteName = "NGUI";
        sprite.MakePixelPerfect();
        UIButton btn = gameObject.GetComponent<UIButton>();
        btn.normalSprite = sprite.spriteName;

        GameObject go = new GameObject();

        go.transform.parent = transform;
        go.AddComponent<UISprite>();
        go.GetComponent<UISprite>().atlas = atlas;
        go.GetComponent<UISprite>().spriteName = "NGUI";
    }

    public bool flag { get; set; }
}