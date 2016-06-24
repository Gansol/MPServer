using UnityEngine;
using System.Collections;

public class asd : MonoBehaviour {

    AssetBundleManager abm;
    private GameObject clone;
    public GameObject root;
    private Animator _animator;
	// Use this for initialization

    void Awake()
    {
        AssetBundleManager.Unload("EggMice", typeof(GameObject), true);
        AssetBundleManager.Unload("EggMice", typeof(Texture), true);
        AssetBundleManager.Unload("EggMice", typeof(Material), true);
        AssetBundleManager.UnloadUnusedAssets();
    }
	void Start () {

        abm = new AssetBundleManager();
       abm.init();
       StartCoroutine(abm.LoadAtlas("EggMice/EggMice", typeof(Material)));
       StartCoroutine(abm.LoadAtlas("EggMice/EggMice", typeof(Texture)));
       StartCoroutine(abm.LoadAtlas("EggMice/EggMice", typeof(GameObject)));
       StartCoroutine(abm.LoadGameObject("EggMice/EggMice", typeof(GameObject)));
	}
	
	// Update is called once per frame
	void Update () {
        if (abm.loadedABCount != 0 && abm.loadedABCount == 1)// 2次修改過 loadedABCount
        {
            Debug.Log("AA");
            clone = (GameObject)Instantiate(abm.request.asset);
            clone.transform.parent = root.transform;
            clone.name = abm.request.asset.name;
            clone.transform.localPosition = Vector3.zero;
            clone.transform.localScale = Vector3.one;
            clone.layer = root.transform.gameObject.layer;
            Debug.Log("CLONE NAME :"+clone.transform.GetChild(0).name);
            _animator = clone.transform.GetChild(0).GetComponent<Animator>();
            Destroy(clone.transform.GetChild(0).GetComponent(clone.name));
            AssetBundleManager.UnloadUnusedAssets();
            abm.loadedABCount = 0;  // 2次修改過
        }
	}

    void OnGUI()
    {
        if(GUI.Button(new Rect(100,100,100,100),"BUTTON")){
            _animator.Play("Die");
        }

        if (GUI.Button(new Rect(200, 100, 100, 100), "Depth"))
        {
            int count=0;
            count = clone.transform.GetChild(0).childCount;
            

            for (int i = 0; i < count; i++) {
                int j = 0;
                
                while (j < clone.transform.GetChild(0).GetChild(i).childCount)
                {
                    if (clone.transform.GetChild(0).GetChild(i).GetChild(j).GetComponent<UISprite>() == null)
                    {
                        int k = 0;
                        while (k < clone.transform.GetChild(0).GetChild(i).GetChild(j).childCount)
                        {
                            if (clone.transform.GetChild(0).GetChild(i).GetChild(j).GetChild(k).GetComponent<UISprite>() == null)
                            {
                                int x = 0;
                                while (x < clone.transform.GetChild(0).GetChild(i).GetChild(j).GetChild(k).childCount)
                                {
                                    UISprite sprite;
                                    sprite = clone.transform.GetChild(0).GetChild(i).GetChild(j).GetChild(k).GetChild(x).GetComponent<UISprite>();
                                    sprite.depth += 500;
                                    x++;
                                }
                                k++;
                            }
                            UISprite sprite2;
                            sprite2 = clone.transform.GetChild(0).GetChild(i).GetChild(j).GetChild(k).GetComponent<UISprite>();
                            sprite2.depth += 500;
                            k++;
                        }
                        j++;
                    }
                    else
                    {
                        UISprite sprite;
                        sprite = clone.transform.GetChild(0).GetChild(i).GetChild(j).GetComponent<UISprite>();
                        sprite.depth += 500;
                        j++;
                    }
                }
            }
        }
    }
}
