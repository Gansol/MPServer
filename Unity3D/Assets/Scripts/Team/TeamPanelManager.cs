using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TeamPanelManager : MonoBehaviour
{
    AssetBundleManager assetBundleManager;
    public GameObject[] miceProperty;
    private GameObject _clone;
    private Animator _animator;
    private GameObject _miceImage;
    private GameObject _tmpMiceImage;
    private GameObject _btnClick;
    private GameObject _lastClick;
    private Dictionary<string, GameObject> dictMiceImage;
    private int _matCount;
    private int _objCount;

    void Awake()
    {
        assetBundleManager = new AssetBundleManager();
        dictMiceImage = new Dictionary<string, GameObject>();
    }

    // Use this for initialization
    void Start()
    {
        _miceImage = miceProperty[0];    // 方便程式辨認用 
    }

    // Update is called once per frame
    void Update()
    {//錯誤

        if (assetBundleManager.loadedABCount == _matCount && _matCount != 0)
        {
            Debug.Log("assetBundleManager.loadedCount = " + assetBundleManager.loadedABCount + "     _matCount:= " + _matCount);
            assetBundleManager.loadedABCount = _matCount = 0;
            LoadObject(_btnClick);
        }

        if (assetBundleManager.loadedObjectCount == _objCount && _objCount != 0)
        {
            Debug.Log("assetBundleManager.loadedCount = " + assetBundleManager.loadedABCount + "     _objCount:= " + _objCount);
            assetBundleManager.loadedObjectCount = _matCount = 0;
            InstantiateObject();
        }

        /*
        if (assetBundleManager.loadedCount != 0 && assetBundleManager.loadedCount == 1)     // 載入完成時顯示老鼠
        {
            _clone = (GameObject)Instantiate(assetBundleManager.request.asset);
            _clone.SetActive(false);
            _clone.transform.parent = _miceImage.transform;
            _clone.name = assetBundleManager.request.asset.name;
            _clone.transform.localPosition = Vector3.zero;
            _clone.transform.localScale = Vector3.one;
            _clone.layer = _miceImage.transform.gameObject.layer;
            _clone.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

            _animator = _clone.transform.GetChild(0).GetComponent<Animator>();
            Destroy(_clone.transform.GetChild(0).GetComponent(_clone.name));
            SwitchDepth(_clone);

            GameObject _tmp;
            if (dictMiceImage.TryGetValue(_clone.name, out _tmp) != null)
            {
                dictMiceImage.Add(_clone.name, _clone);
            }
            _tmpMiceImage = _clone;
            _clone.SetActive(true);

            AssetBundleManager.UnloadUnusedAssets();
            assetBundleManager.loadedCount = 0; //錯誤
        }
         * */
    }

    public void OnMiceClick(GameObject btn_mice)
    {
        _btnClick = btn_mice;
        if (_lastClick != btn_mice) // 不同按鈕按太快會出錯
        {
            _lastClick = btn_mice.transform.GetChild(0).gameObject; // 儲存上次按的按鈕
            GameObject _miceImage;
            string miceName = btn_mice.transform.GetChild(0).name;

            if (_tmpMiceImage != null) _tmpMiceImage.SetActive(false);  // 如果暫存老鼠圖片不是空的(防止第一次點擊出錯)，將上一個老鼠圖片隱藏

            if (dictMiceImage.TryGetValue(miceName, out _miceImage))    // 假如已經載入老鼠圖片了 直接顯示
            {
                _miceImage.SetActive(true);
                _tmpMiceImage = _miceImage;
            }
            else
            {
                LoadAsset(btn_mice);
            }



        }
    }

    private void LoadAsset(GameObject obj)
    {
        foreach (KeyValuePair<string, object> item in Global.miceProperty)  // 載入玩家擁有老鼠
        {
            string tmp = (string)obj.name.ToString().Remove(0, 4); // + pageVaule 還沒加入翻頁值
            Debug.Log("TMP NAME: " + tmp + " / " + "key name: " + item.Key);
            if (tmp == item.Key.ToString()) //如果按鈕和玩家擁有老鼠相同
            {
                var innDict = item.Value as Dictionary<string, object>;
                var i = 0;
                foreach (KeyValuePair<string, object> inner in innDict) //載入老鼠資料
                {
                    if (i == 0) //載入老鼠圖 (PS:0在SQL中是老鼠名稱所以用來載入圖片)
                    {
                        try
                        {
                            if (assetBundleManager.bLoadedAssetbundle(name) != true)    //如果AB還沒載入
                            {
                                _matCount = 1 * 3;
                                assetBundleManager.init();
                                StartCoroutine(assetBundleManager.LoadAtlas(inner.Value.ToString() + "/" + inner.Value.ToString(), typeof(Texture)));
                                StartCoroutine(assetBundleManager.LoadAtlas(inner.Value.ToString() + "/" + inner.Value.ToString(), typeof(Material)));
                                StartCoroutine(assetBundleManager.LoadAtlas(inner.Value.ToString() + "/" + inner.Value.ToString(), typeof(GameObject)));
                            }
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                    }
                    i++;
                }
            }
        }
    }

    private void LoadObject(GameObject obj)
    {
        foreach (KeyValuePair<string, object> item in Global.miceProperty)  // 載入玩家擁有老鼠
        {
            string tmp = (string)obj.name.ToString().Remove(0, 4); // + pageVaule 還沒加入翻頁值
            Debug.Log("TMP NAME: " + tmp + " / " + "key name: " + item.Key);
            if (tmp == item.Key.ToString()) //如果按鈕和玩家擁有老鼠相同
            {
                var innDict = item.Value as Dictionary<string, object>;
                var i = 0;
                foreach (KeyValuePair<string, object> inner in innDict) //載入老鼠資料
                {
                    if (i == 0) //載入老鼠圖 (PS:0在SQL中是老鼠名稱所以用來載入圖片)
                    {
                        try
                        {
                            if (assetBundleManager.bLoadedAssetbundle(name) != true)    //如果AB還沒載入
                            {
                                _objCount=1*2;
                                assetBundleManager.init();
                                StartCoroutine(assetBundleManager.LoadGameObject(inner.Value.ToString() + "/" + inner.Value.ToString(), typeof(GameObject)));
                                StartCoroutine(assetBundleManager.LoadGameObject(inner.Value.ToString() + "/" + inner.Value.ToString(), typeof(Animation)));
                            }
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                    }
                    else if (i < innDict.Count) // 載入老鼠數值說明 innDict.Count SQL中轉換成字典的資料筆數
                    {
                        Debug.Log("Name: " + miceProperty[i].transform.name);
                        miceProperty[i].transform.GetComponent<UILabel>().depth = 3077;
                        miceProperty[i].transform.GetComponent<UILabel>().fontSize = 30;
                        miceProperty[i].transform.GetComponent<UILabel>().alignment = NGUIText.Alignment.Left;
                        miceProperty[i].transform.GetComponent<UILabel>().text = inner.Value.ToString();
                        Debug.Log(" ******Value: " + inner.Value.ToString());
                    }
                    i++;
                }
            }
        }
    }

    private void InstantiateObject()
    {
        _clone = (GameObject)Instantiate(assetBundleManager.request.asset);
        _clone.SetActive(false);
        _clone.transform.parent = _miceImage.transform;
        _clone.name = assetBundleManager.request.asset.name;
        _clone.transform.localPosition = Vector3.zero;
        _clone.transform.localScale = Vector3.one;
        _clone.layer = _miceImage.transform.gameObject.layer;
        _clone.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

        _animator = _clone.transform.GetChild(0).GetComponent<Animator>();
        Destroy(_clone.transform.GetChild(0).GetComponent(_clone.name));
        SwitchDepth(_clone);

        GameObject _tmp;
        if (dictMiceImage.TryGetValue(_clone.name, out _tmp) != null)
        {
            dictMiceImage.Add(_clone.name, _clone);
        }
        _tmpMiceImage = _clone;
        _clone.SetActive(true);

        AssetBundleManager.UnloadUnusedAssets();
        //assetBundleManager.loadedCount = 0; //錯誤
    }

    public void OnMiceDrag(string miceName)
    {

    }

    private void SwitchDepth(GameObject clone)  //改變老鼠圖片深度
    {
        int count = 0;
        count = clone.transform.GetChild(0).childCount;
        for (int i = 0; i < count; i++)
        {
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
                                clone.layer = clone.transform.parent.gameObject.layer;
                                x++;
                            }
                            k++;
                        }
                        UISprite sprite2;
                        sprite2 = clone.transform.GetChild(0).GetChild(i).GetChild(j).GetChild(k).GetComponent<UISprite>();
                        sprite2.depth += 500;
                        clone.layer = clone.transform.parent.gameObject.layer;
                        k++;
                    }
                    j++;
                }
                else
                {
                    UISprite sprite;
                    sprite = clone.transform.GetChild(0).GetChild(i).GetChild(j).GetComponent<UISprite>();
                    sprite.depth += 500;
                    clone.layer = clone.transform.parent.gameObject.layer;
                    j++;
                }
            }
        }
    }
}
