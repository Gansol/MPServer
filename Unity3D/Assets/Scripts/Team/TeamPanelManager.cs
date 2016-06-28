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

    private bool _isCompleted = false;

    public float delayBetween2Clicks; // Change value in editor
    private float lastClickTime = 0;

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

        if (assetBundleManager.isStartLoadAsset && !_isCompleted)
        {
            Debug.Log("訊息：" + assetBundleManager.ReturnMessage);
        }

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
    }
    
    public void OnMiceClick(GameObject btn_mice)
    {

        if (Time.time - lastClickTime < delayBetween2Clicks)    // Double Click
        {
            Debug.Log("Double clicked");
            btn_mice.SendMessage("Mice2Team");
        }
        else
        {
            StartCoroutine(OnClickCoroutine(btn_mice));
        }
        lastClickTime = Time.time;


    }

    IEnumerator OnClickCoroutine(GameObject btn_mice)
    {
        yield return new WaitForSeconds(delayBetween2Clicks);

        if (Time.time - lastClickTime < delayBetween2Clicks)
        {
            yield break;
        }

        _btnClick = btn_mice;
        if (_lastClick != btn_mice) // 不同按鈕按太快會出錯
        {
            _lastClick = btn_mice.transform.GetChild(0).gameObject; // 儲存上次按的按鈕
            GameObject _miceImage;
            string miceName = btn_mice.transform.GetChild(0).name;

            if (_tmpMiceImage != null) _tmpMiceImage.SetActive(false);  // 如果暫存老鼠圖片不是空的(防止第一次點擊出錯)，將上一個老鼠圖片隱藏

            if (dictMiceImage.TryGetValue(miceName, out _miceImage))    // 假如已經載入老鼠圖片了 直接顯示
            {
                Debug.Log("MiceName: " + dictMiceImage.TryGetValue(miceName, out _miceImage));
                _miceImage.SetActive(true);
                _tmpMiceImage = _miceImage;
            }
            else
            {
                LoadAsset(btn_mice);
            }

            LoadProperty(btn_mice.name);

        }

        Debug.Log("Simple click");
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
                    try
                    {
                        if (i == 0 && assetBundleManager.bLoadedAssetbundle(name) != true) //如果AB還沒載入 載入老鼠圖 (PS:0在SQL中是老鼠名稱所以用來載入圖片)
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
                    break;
                }
                break;
            }
        }
    }

    private void LoadObject(GameObject obj)
    {
        foreach (KeyValuePair<string, object> item in Global.miceProperty)  // 載入玩家擁有老鼠
        {
            string tmpName = (string)obj.name.ToString().Remove(0, 4); // + pageVaule 還沒加入翻頁值 tmpName = Mice(1)>1
            Debug.Log("TMP NAME: " + tmpName + " / " + "key name: " + item.Key);
            if (tmpName == item.Key.ToString()) //如果按鈕和玩家擁有老鼠相同 Key=123
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
                                _objCount = 1;  // 錯誤
                                //assetBundleManager.init();
                                StartCoroutine(assetBundleManager.LoadGameObject(inner.Value.ToString() + "/" + inner.Value.ToString(), typeof(GameObject)));
                               // StartCoroutine(assetBundleManager.LoadGameObject(inner.Value.ToString() + "/" + inner.Value.ToString(), typeof(Animation)));
                            }
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                    }
                    break;
                }
                break;
            }
        }
    }

    private void LoadProperty(string name)
    {
        string tmpName = (string)name.ToString().Remove(0, 4); // + pageVaule 還沒加入翻頁值
        foreach (KeyValuePair<string, object> item in Global.miceProperty)  // 載入玩家擁有老鼠
        {
            if (tmpName == item.Key.ToString()) //如果按鈕和玩家擁有老鼠相同
            {
                var innDict = item.Value as Dictionary<string, object>;
                int i = 0;
                foreach (KeyValuePair<string, object> inner in innDict) //載入老鼠資料
                {
                    if (i != 0)
                    {
                        miceProperty[i].transform.GetComponent<UILabel>().text = inner.Value.ToString();
                    }
                    i++;
                }
            }
        }
    }

    private void InstantiateObject()
    {
        _clone = (GameObject)Instantiate(assetBundleManager.request.asset);
        ObjectManager.SwitchDepthLayer(_clone, _miceImage, Global.MeunObjetDepth);
        _clone.transform.parent = _miceImage.transform;
        _clone.SetActive(false);
        _clone.name = assetBundleManager.request.asset.name;
        _clone.transform.localPosition = Vector3.zero;
        _clone.transform.localScale = Vector3.one;
        _clone.layer = _miceImage.transform.gameObject.layer;
        _clone.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        

        Destroy(_clone.transform.GetChild(0).GetComponent(_clone.name));


        GameObject _tmp;
        if (dictMiceImage.TryGetValue(_clone.name, out _tmp) != null)
        {
            dictMiceImage.Add(_clone.name, _clone);
        }
        _tmpMiceImage = _clone;
        _clone.SetActive(true);

        _animator = _clone.transform.GetChild(0).GetComponent<Animator>();
        AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);

            Debug.Log(currentState.nameHash);

        assetBundleManager.LoadedBundle();
        Debug.Log(assetBundleManager.loadedObjectCount);
        AssetBundleManager.UnloadUnusedAssets();

        _isCompleted = !_isCompleted;
    }


    /*
    /// <summary>
    /// 實體化載入完成的遊戲物件，利用玩家JASON資料判斷必要實體物件
    /// </summary>
    /// <param name="data">JSON資料</param>
    /// <param name="parent">實體化父系位置</param>
    void InstantiateObject()
    {
        _objCount = 0;
        string bundleName = "EggMice/EggMice";
        if (assetBundleManager.bLoadedAssetbundle(bundleName))
        {
            AssetBundle bundle = AssetBundleManager.getAssetBundle(bundleName);
            if (bundle != null)
            {
                _clone = (GameObject)Instantiate(bundle.mainAsset);
                _clone.transform.parent = _miceImage.transform;
                _clone.name = "EggMice";
                _clone.transform.localPosition = Vector3.zero;
                _clone.transform.localScale = Vector3.one;
                _objCount++;
            }
            else
            {
                Debug.LogError("Assetbundle reference not set to an instance.");
            }
        }

        assetBundleManager.LoadedBundle();
        Debug.Log(assetBundleManager.loadedObjectCount);

        _objCount = 0;
    }
    */

    public void OnMiceDrag(string miceName)
    {

    }   
}
