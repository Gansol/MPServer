using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using MiniJSON;
using System;
using System.Linq;

/* ***************************************************************
 * -----Copyright © 2015 Gansol Studio.  All Rights Reserved.-----
 * -----------            CC BY-NC-SA 4.0            -------------
 * -----------  @Website:  EasyUnity@blogspot.com    -------------
 * -----------  @Email:    GansolTW@gmail.com        -------------
 * -----------  @Author:   Krola.                    -------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * 
 * 物件池 提供一個高效率的物件生成、回收方式
 * 231行，EggMice和ID必須由程式控制 現在亂寫
 * ***************************************************************/

public class PoolManager : MonoBehaviour
{
    private AssetLoader assetLoader;
    private InstantiateObject insObj;
    private Dictionary<string, object> _tmpDict;
    private Dictionary<int, string> _dictObject;

    private HashSet<int> _myMice;
    private HashSet<int> _otherMice;

    private GameObject clone;

    private float _lastTime;
    private float _currentTime;

    Dictionary<string, object> _dictSkillMice;

    public GameObject Panel;

    [Tooltip("物件池位置")]
    public GameObject ObjectPool;

    [Tooltip("技能位置")]
    public GameObject skillArea;

    [Tooltip("產生數量")]
    [Range(3, 5)]
    public int spawnCount = 5;

    [Tooltip("物件池上限(各種類分開)")]
    [Range(3, 10)]
    public int clearLimit = 5;

    [Tooltip("物件池上限(各種類分開)")]
    [Range(10, 20)]
    public int clearTime = 10;

    [Tooltip("物件池保留量(各種類分開)")]
    [Range(2, 5)]
    public int reserveCount = 2;

    private bool _mergeFlag = false;          // 合併老鼠完成
    private bool _poolingFlag = false;        // 初始化物件池

    public bool mergeFlag { get { return _mergeFlag; } }
    public bool poolingFlag { get { return _poolingFlag; } }


    private Vector3 bossScale = new Vector3(1.2f, 1.2f, 1.2f);
    private Vector3 skillScale = new Vector3(0.9f, 0.9f, 0.9f);

    void Awake()
    {
        assetLoader = gameObject.AddComponent<AssetLoader>();
        insObj = new InstantiateObject();
        _lastTime = 0;
        _currentTime = 0;
        clearTime = 10;
        _poolingFlag = false;      // 初始化物件池
        _dictObject = new Dictionary<int, string>();
        _myMice = new HashSet<int>();
        _otherMice = new HashSet<int>();
        _tmpDict = new Dictionary<string, object>();
        _dictSkillMice = new Dictionary<string, object>();
        MergeMice();                                // 將雙方的老鼠合併 剔除相同的老鼠

    }

    void Start()
    {
        assetLoader.init();
        foreach (KeyValuePair<int, string> item in _dictObject)
        {
            assetLoader.LoadAsset(item.Value + "/", item.Value);
        }

        foreach (KeyValuePair<int, string> item in _dictObject)
        {
            assetLoader.LoadPrefab(item.Value + "/", item.Value);
        }
    }



    void Update()
    {
        if (!_poolingFlag && !string.IsNullOrEmpty(assetLoader.ReturnMessage))
            Debug.Log(assetLoader.ReturnMessage);

        if (assetLoader.loadedObj && !_poolingFlag)
        {
            _dictSkillMice = Global.Team;
            InstantiateObject(_dictObject);
            InstantiateSkillMice(_dictSkillMice);
            _poolingFlag = true;
            Debug.Log("pooling Mice Completed ! ");
        }


        _currentTime = Time.time;

        // 定時清除物件池
        if (_currentTime - _lastTime > clearTime)     // 達到清除時間時
        {
            for (int i = 0; i < ObjectPool.transform.childCount; i++)       // 跑遍動態池
            {
                if (ObjectPool.transform.GetChild(i).childCount > clearLimit)           // 如果動態池超過限制數量
                {
                    for (int j = 0; j < ObjectPool.transform.GetChild(i).childCount - reserveCount; j++)    // 銷毀物件
                    {
                        Destroy(ObjectPool.transform.GetChild(i).GetChild(j).gameObject);
                    }
                }
            }
            _lastTime = _currentTime;
        }
    }

    void InstantiateObject(Dictionary<int, string> objectData)
    {
        foreach (KeyValuePair<int, string> item in objectData)
        {
            GameObject bundle = assetLoader.GetAsset(item.Value);
            if (bundle != null)
            {
                clone = new GameObject();

                clone.name = item.Value;
                clone.transform.parent = ObjectPool.transform;
                clone.layer = clone.transform.parent.gameObject.layer;
                clone.transform.localScale = Vector3.one;

                Transform parent = ObjectPool.transform.FindChild(item.Value).transform;
                clone = insObj.Instantiate(bundle, parent, item.Value, Vector3.zero, Vector3.one, Vector2.zero, -1);
                clone.transform.GetChild(0).gameObject.SetActive(false);    // 新版 子物件隱藏
            }
        }
    }

    void InstantiateSkillMice(Dictionary<string, object> objectData)
    {

        int i = 0;
        foreach (KeyValuePair<string, object> item in objectData)
        {
            GameObject bundle = assetLoader.GetAsset(item.Value.ToString());
            if (bundle != null)
            {
                Vector3 scale = Vector3.zero;
                InstantiateObject insObj = new InstantiateObject();
                clone = new GameObject();
                Transform parent = skillArea.transform.GetChild(i);

                scale = (i == 4) ? bossScale : skillScale;
                clone = insObj.Instantiate(bundle, parent, item.Value.ToString(), Vector3.zero, scale, Vector2.zero, -1);
                clone.transform.GetChild(0).GetComponent<Animator>().enabled = false;
                clone.transform.GetChild(0).GetComponent<BoxCollider2D>().enabled = false;
                clone.transform.GetChild(0).GetComponent<MonoBehaviour>().enabled = false;
                clone.layer = clone.transform.parent.gameObject.layer;

                skillArea.transform.GetChild(i).transform.gameObject.SetActive(true);    // 新版 子物件隱藏
                i++;
            }
            else
            {
                Debug.LogError("bundle is null!");
            }

        }
    }

    /// <summary>
    /// 每一次顯示一個GameObject。如果GameObject不足，Spawn一個物件並顯示。
    /// </summary>
    /// <param name="objectName">使用Name找Object</param>
    /// <returns>回傳 ( GameObject / null )</returns>
    public GameObject ActiveObject(string objectName)
    {
        Debug.Log("_dictObject.Count:" + _dictObject.Count);
        //int objectID = _dictObject.FirstOrDefault(x => x.Value == objectName).Key;       // 找Key

        if (ObjectPool.transform.FindChild(objectName).childCount == 0)
        {
            GameObject bundle = assetLoader.GetAsset(objectName);

            if (bundle != null)
            {
                Transform parent = ObjectPool.transform.FindChild(objectName).transform;
                clone = insObj.Instantiate(bundle, parent, objectName, Vector3.zero, Vector3.one, Vector2.zero, -1);
                return clone;
            }

            Debug.LogError("bundle is null !");
            return null;
        }

        for (int i = 0; i < ObjectPool.transform.FindChild(objectName).childCount; i++)
        {
            GameObject clone;

            clone = ObjectPool.transform.FindChild(objectName).GetChild(i).gameObject;

            if (clone.name == objectName && !clone.transform.GetChild(0).gameObject.activeSelf)
            {
                clone.transform.GetChild(0).gameObject.SetActive(true);
                return clone;
            }
        }
        return null;
    }
    /// <summary>
    /// 從道具名稱取得道具ID
    /// </summary>
    /// <param name="miceName">道具名稱</param>
    /// <param name="itemData">2d Dictionary</param>
    /// <returns>itemName</returns>
    #region -- GetItemNameFromID --
    public int GetItemIDFromName(string miceName)
    {
        object value;
        foreach (KeyValuePair<string, object> item in Global.miceProperty)
        {
            var nestedData = item.Value as Dictionary<string, object>;
            nestedData.TryGetValue("ItemName",out value);
            if(miceName == value.ToString()){
                nestedData.TryGetValue("ItemID", out value);
                return int.Parse(value.ToString());
            }
        }
        return -1;
    }
    #endregion

    public void MergeMice()
    {
        Debug.Log("Team:" + Global.Team);
        Debug.Log("ITEM2:" + Global.SortedItem);
        Dictionary<string, object> dictMyMice = Global.Team;
        Dictionary<string, object> dictOtherMice = Global.OtherData.Team;

        foreach (KeyValuePair<string, object> item in dictMyMice)
        {
            _dictObject.Add(GetItemIDFromName(item.Value.ToString()), item.Value.ToString());
        }

        foreach (KeyValuePair<string, object> item in dictOtherMice)
        {
            if (!dictMyMice.ContainsValue(item.Value))
                _dictObject.Add(GetItemIDFromName(item.Value.ToString()), item.Value.ToString());
        }

        Debug.Log(_dictObject);
        _mergeFlag = true;
        /*
        //把自己的老鼠存入HashSet中等待比較，再把老鼠存入合併好的老鼠Dict中
        foreach (KeyValuePair<string, object> item in _tmpDict)
        {
            _myMice.Add(Int16.Parse(item.Key));
            _dictObject.Add(Int16.Parse(item.Key), item.Value.ToString());
        }

        if (!_dictObject.ContainsValue("EggMice"))
        {
            _dictObject.Add(1, "EggMice");
        }

        _tmpDict.Clear();
        Debug.Log(Global.OtherData.Team);
        _tmpDict = Json.Deserialize(Global.OtherData.Team) as Dictionary<string, object>;

        //把對手的老鼠存入HashSet中等待比較
        foreach (KeyValuePair<string, object> item in _tmpDict)
        {
            _otherMice.Add(Int16.Parse(item.Key));
        }

       _otherMice.ExceptWith(_myMice); // 把對手重複的老鼠丟掉

        if (_otherMice.Count != 0)      // 如果對手老鼠有不重複的話
        {
            foreach (int item in _otherMice)    // 加入合併好的老鼠Dict
            {
                object miceName;
                _tmpDict.TryGetValue(item.ToString(), out miceName);

                _dictObject.Add(item, miceName.ToString());
            }
        }
        _mergeFlag = true;
        Debug.Log("Merge Mice Completed ! ");
         * */



    }
}
