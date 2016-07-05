using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
/* ***************************************************************
 * -----Copyright © 2015 Gansol Studio.  All Rights Reserved.-----
 * -----------            CC BY-NC-SA 4.0            -------------
 * -----------  @Website:  EasyUnity@blogspot.com    -------------
 * -----------  @Email:    GansolTW@gmail.com        -------------
 * -----------  @Author:   Krola.                    -------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * 負責 開啟/關閉/載入 Team的的所有處理
 * (有空改為 LoadIcon.cs LoadActor.cs)
 * NGUI BUG : Team交換時Tween會卡色
 * ***************************************************************
 *                           ChangeLog
 * 20160705 v1.0.0  0版完成，載入老鼠部分未來需要修改                    
 * ****************************************************************/
public class TeamManager : MonoBehaviour
{
    #region 欄位
    AssetBundleManager assetBundleManager;

    public GameObject[] miceProperty;
    public GameObject[] teamParent;
    public static Dictionary<string, GameObject> dictLoadedMice { get; set; }       // <string, GameObject>Icon名稱、Icon的按鈕
    public static Dictionary<string, GameObject> dictLoadedTeam { get; set; }       // <string, GameObject>Icon名稱、Icon的按鈕
    public float delayBetween2Clicks = 0.3f;                                        // Change value in editor

    private GameObject _clone, _miceActor, _tmpActor, _btnClick, _chkDoubleClick;   // 克隆、老鼠角色、暫存角色、按下按鈕、雙擊檢查
    private static Dictionary<string, object> _dictMice, _dictTeam;                 // Json老鼠、隊伍
    private Dictionary<string, GameObject> _dictActor;                              // 已載入角色參考

    private int _matCount, _objCount, _page;    // 材質數量、物件數量、翻一頁+10
    private float lastClickTime;
    private bool _diableBtn, _LoadedIcon, _LoadedActor;

    //public GameObject[] miceAll;
    //public GameObject[] teamMice;
    //private GameObject _lastClick;
    //private Dictionary<string, GameObject> _dictHideMice;
    #endregion

    void Awake()
    {
        _matCount = _objCount =_page = 0;;
        _miceActor = miceProperty[0];    // 方便程式辨認用 miceProperty[0]=image
        assetBundleManager = new AssetBundleManager();
        dictLoadedMice = new Dictionary<string, GameObject>();
        dictLoadedTeam = new Dictionary<string, GameObject>();
        _dictMice = Json.Deserialize(Global.MiceAll) as Dictionary<string, object>;
        _dictTeam = Json.Deserialize(Global.Team) as Dictionary<string, object>;
        _dictActor = new Dictionary<string, GameObject>();
    }

    void Update()
    {
        if (assetBundleManager.loadedABCount == _matCount && _matCount != 0 && !_LoadedIcon)    //
        {
            assetBundleManager.loadedABCount = _matCount = 0;
            LoadIconObject(_dictMice);
        }

        if (assetBundleManager.loadedObjectCount == _objCount && _objCount != 0 && !_LoadedIcon)
        {
            assetBundleManager.loadedObjectCount = _objCount = 0;
            InstantiateIcon(_dictMice, teamParent[0].transform);
            InstantiateIcon(_dictTeam, teamParent[1].transform);
            HideMice();
        }

        if (assetBundleManager.loadedABCount == _matCount && _matCount != 0 && !_LoadedActor)   // 載入第2個 或 如果發生錯誤沒有載入要重新開啟
        {
            assetBundleManager.loadedABCount = _matCount = 0;
            LoadActorObject(_btnClick);   // 如果要讓Team也能顯示老鼠這裡要修改 _btnClick > ditcLoadedTeam[x]
            assetBundleManager.LoadedBundle();
        }

        if (assetBundleManager.loadedObjectCount == _objCount && _objCount != 0 && !_LoadedActor)
        {
            assetBundleManager.loadedObjectCount = _objCount = 0;
            InstantiateActor();
            assetBundleManager.LoadedBundle();
        }
    }

    #region -- OnMiceClick 當按下老鼠時 --
    public void OnMiceClick(GameObject btn_mice)
    {
        if (Time.time - lastClickTime < delayBetween2Clicks && _chkDoubleClick == btn_mice)    // Double Click
            btn_mice.SendMessage("Mice2Click");
        else
            StartCoroutine(OnClickCoroutine(btn_mice));

        lastClickTime = Time.time;
        _chkDoubleClick = btn_mice;
    }

    IEnumerator OnClickCoroutine(GameObject btn_mice)
    {
        yield return new WaitForSeconds(delayBetween2Clicks);

        if (Time.time - lastClickTime < delayBetween2Clicks)
            yield break;

        LoadActor(btn_mice);
        LoadProperty(btn_mice.name);
        //Debug.Log("Simple click");
    }
    #endregion

    #region -- OnMessage 載入老鼠(外部呼叫) -- 這裡未來會錯 如果載入超過1張老鼠Icon圖
    void OnMessage()
    {
        LoadAsset("MiceICON/MiceICON", 1);
    }
    #endregion

    #region -- LoadAsset 載入老鼠資產 --
    void LoadAsset(string assetName, int count)
    {
        _matCount += count * 3;   // 錯誤 有問題
        StartCoroutine(assetBundleManager.LoadAtlas(assetName, typeof(Texture)));
        StartCoroutine(assetBundleManager.LoadAtlas(assetName, typeof(Material)));
        StartCoroutine(assetBundleManager.LoadAtlas(assetName, typeof(GameObject)));
    }
    #endregion

    #region -- LoadIconObject 載入老鼠物件 --
    void LoadIconObject(Dictionary<string, object> dictionary)    // 載入遊戲物件
    {
        _objCount += dictionary.Count;
        foreach (KeyValuePair<string, object> item in dictionary)
        {
            StartCoroutine(assetBundleManager.LoadGameObject("MiceICON/" + item.Value.ToString().Remove(item.Value.ToString().Length - 4) + "ICON", typeof(GameObject)));
        }
    }
    #endregion

    #region -- InstantiateIcon 實體化老鼠物件--
    /// <summary>
    /// 實體化載入完成的遊戲物件，利用玩家JASON資料判斷必要實體物件
    /// </summary>
    /// <param name="dictionary">資料字典</param>
    /// <param name="myParent">實體化父系位置</param>
    void InstantiateIcon(Dictionary<string, object> dictionary, Transform myParent)
    {
        _objCount = 0;
        foreach (KeyValuePair<string, object> item in dictionary)
        {
            string bundleName = "MiceICON/" + item.Value.ToString().Remove(item.Value.ToString().Length - 4) + "ICON";
            if (assetBundleManager.bLoadedAssetbundle(bundleName))                  // 已載入資產時
            {
                AssetBundle bundle = AssetBundleManager.getAssetBundle(bundleName);
                Transform miceBtn = myParent.GetChild(_objCount);

                Add2Refs((GameObject)bundle.mainAsset, miceBtn);     // 加入物件參考

                _clone = (GameObject)Instantiate(bundle.mainAsset);             // 實體化
                _clone.layer = myParent.gameObject.layer;
                _clone.transform.parent = miceBtn;
                _clone.name = item.Value.ToString();
                _clone.transform.localPosition = Vector3.zero;
                _clone.transform.localScale = Vector3.one;

                miceBtn.GetComponent<TeamSwitcher>().enabled = true;           // 開啟老鼠隊伍交換功能
                miceBtn.GetComponent<TeamSwitcher>().SendMessage("EnableBtn"); // 開啟按鈕功能

                _objCount++;
            }
            else
            {
                Debug.LogError("Assetbundle reference not set to an instance.");
            }
        }
        _LoadedIcon = true;
        _objCount = 0;
    }
    #endregion


    #region -- LoadActor 載入老鼠角色 --
    private void LoadActor(GameObject btn_mice)
    {
        _btnClick = btn_mice;
        float _loadTime;
        _loadTime = Time.time;
        _LoadedActor = false;
        //_lastClick = btn_mice.transform.GetChild(0).gameObject; // 儲存上次按的按鈕
        GameObject _miceImage;
        string miceName = btn_mice.transform.GetChild(0).name;

        if (_tmpActor != null) _tmpActor.SetActive(false);  // 如果暫存老鼠圖片不是空的(防止第一次點擊出錯)，將上一個老鼠圖片隱藏

        if (_dictActor.TryGetValue(miceName, out _miceImage))    // 假如已經載入老鼠圖片了 直接顯示
        {
            _miceImage.SetActive(true);
            _tmpActor = _miceImage;
        }
        else
        {
            LoadMiceAsset(btn_mice);
        }
    }
    #endregion

    #region -- LoadMiceAsset 載入老鼠資產 --
    private void LoadMiceAsset(GameObject obj)
    {
        string key = (int.Parse(obj.name.Remove(0, 4)) + _page).ToString(); // + pageVaule 還沒加入翻頁值
        string assetName = obj.transform.GetChild(0).name + "/" + obj.transform.GetChild(0).name;

        if (Global.miceProperty.ContainsKey(key))
        {
            if (!assetBundleManager.bLoadedAssetbundle(assetName))
            {
                _matCount = 3;
                assetBundleManager.init();
                StartCoroutine(assetBundleManager.LoadAtlas(assetName, typeof(Texture)));
                StartCoroutine(assetBundleManager.LoadAtlas(assetName, typeof(Material)));
                StartCoroutine(assetBundleManager.LoadAtlas(assetName, typeof(GameObject)));
            }
            else
            {
                Debug.LogError("Error: " + assetName + " has loaded!");
            }
        }
    }
    #endregion

    #region -- LoadActorObject 載入老鼠角色物件 --
    private void LoadActorObject(GameObject obj)  // 錯誤
    {
        string miceName = obj.transform.GetChild(0).name;
        string assetName = miceName + "/" + miceName;

        if (!assetBundleManager.bLoadedAssetbundle(assetName))    //如果AB還沒載入
        {
            _objCount = 1;  // 錯誤
            StartCoroutine(assetBundleManager.LoadGameObject(assetName, typeof(GameObject)));
        }
        else
        {
            Debug.LogError("Error: " + assetName + " has loaded!");
        }
    }
    #endregion

    #region -- InstantiateActor 實體化老鼠角色 --
    private void InstantiateActor()
    {
        string miceName = _btnClick.transform.GetChild(0).name;

        _clone = (GameObject)Instantiate(AssetBundleManager.getAssetBundle(miceName + "/" + miceName).mainAsset);
        ObjectManager.SwitchDepthLayer(_clone, _miceActor, Global.MeunObjetDepth);

        _clone.transform.parent = _miceActor.transform;
        _clone.SetActive(false);
        _clone.name = assetBundleManager.request.asset.name;
        _clone.transform.localPosition = Vector3.zero;
        _clone.transform.localScale = Vector3.one;
        _clone.layer = _miceActor.transform.gameObject.layer;
        _clone.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

        Destroy(_clone.transform.GetChild(0).GetComponent(_clone.name));    // 刪除Battle用腳本

        GameObject _tmp;
        if (_dictActor.TryGetValue(_clone.name, out _tmp) != null)
            _dictActor.Add(_clone.name, _clone);

        _tmpActor = _clone;
        _clone.SetActive(true);
        _LoadedActor = true;

        AssetBundleManager.UnloadUnusedAssets();
        Debug.Log(assetBundleManager.loadedObjectCount);
    }
    #endregion


    #region -- HideMice 隱藏老鼠 --
    void HideMice() // 把按鈕變成無法使用 如果老鼠已Team中
    {
        foreach (KeyValuePair<string, object> item in _dictTeam)
        {
            if (dictLoadedMice.ContainsKey(item.Value.ToString()))
                dictLoadedMice[item.Value.ToString()].SendMessage("DisableBtn");
        }
    }
    #endregion

    #region -- LoadProperty 載入老鼠屬性 --
    private void LoadProperty(string name)
    {
        string key = (int.Parse(name.Remove(0, 4)) + _page).ToString();     // + pageVaule 還沒加入翻頁值
        foreach (KeyValuePair<string, object> item in Global.miceProperty)  // 載入玩家擁有老鼠
        {
            if (key == item.Key.ToString())                                 //如果按鈕和玩家擁有老鼠相同
            {
                var innDict = item.Value as Dictionary<string, object>;
                int i = 0;
                foreach (KeyValuePair<string, object> inner in innDict)     //載入老鼠資料
                {
                    if (i != 0) miceProperty[i].transform.GetComponent<UILabel>().text = inner.Value.ToString();// (PS:0在SQL中是老鼠名稱)
                    i++;
                }
                break;
            }
        }
    }
    #endregion

    #region -- Add2Refs 加入老鼠參考 --
    /// <summary>
    /// 加入老鼠參考
    /// </summary>
    /// <param name="obj">AssetBundle</param>
    /// <param name="myParent">參考按鈕</param>
    void Add2Refs(GameObject obj, Transform myParent)
    {
        string btnArea = myParent.parent.name;                          //按鈕存放區域名稱 Team / Mice 區域
        string miceName = obj.name.Remove(obj.name.Length - 4) + "Mice";

        if (btnArea == "Mice")
            dictLoadedMice.Add(miceName, myParent.gameObject);          // 加入索引 老鼠所在的MiceBtn位置
        else
            dictLoadedTeam.Add(miceName, GetLoadedMice(miceName));      // 參考至 老鼠所在的MiceBtn位置
    }
    #endregion

    #region --字典 檢查/取值 片段程式碼 --
    public bool bLoadedTeam(string miceName)
    {
        GameObject obj;
        return dictLoadedTeam.TryGetValue(miceName, out obj);
    }

    public bool bLoadedMice(string miceName)
    {
        GameObject obj;
        return dictLoadedMice.TryGetValue(miceName, out obj);
    }

    public GameObject GetLoadedMice(string miceName)
    {
        GameObject obj;
        if (dictLoadedMice.TryGetValue(miceName, out obj))
            return obj;
        return null;
    }

    public GameObject GetLoadedTeam(string miceName)
    {
        GameObject obj;
        if (dictLoadedTeam.TryGetValue(miceName, out obj))
            return obj;
        return null;
    }
    #endregion
}
