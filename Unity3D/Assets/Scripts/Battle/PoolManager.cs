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
 * 
 * ***************************************************************/

public class PoolManager : MonoBehaviour
{
    private Dictionary<string, object> _tmpDict;
    private Dictionary<int, string> _dictObject;

    private HashSet<int> _myMice;
    private HashSet<int> _otherMice;

    private GameObject clone;
    private byte objectID;          // 取得的ID 須自行改寫
    private string objectName;      // 取得的 物件名稱

    private float _lastTime;
    private float _currentTime;

    Dictionary<string, object> _skillMice;

    public GameObject Panel;

    [Tooltip("物件池位置")]
    public GameObject ObjectPool;   
    [Tooltip("物件匣")]
    public GameObject[] ObjectDeck;

    [Tooltip("技能位置")]
    public GameObject Skill;

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

    void Awake()
    {
        _lastTime = 0;
        _currentTime = 0;
        clearTime = 10;
        _poolingFlag = false;      // 初始化物件池
        _dictObject = new Dictionary<int, string>();
        _myMice = new HashSet<int>();
        _otherMice = new HashSet<int>();
        _tmpDict = new Dictionary<string, object>();
        _skillMice = new Dictionary<string, object>();
        MergeMice();                                // 將雙方的老鼠合併 剔除相同的老鼠


        // 生出 預設數量的物件
        foreach (KeyValuePair<int, string> item in _dictObject)
        {
            clone = new GameObject();

            clone.name = item.Value;
            clone.transform.parent = ObjectPool.transform;
            clone.layer = clone.transform.parent.gameObject.layer;
            clone.transform.localScale = Vector3.one;

            for (int i = 0; i < spawnCount; i++)
            {
                clone = (GameObject)Instantiate(ObjectDeck[item.Key - 1]);   //　等傳老鼠ID名稱近來這要改
                clone.name = item.Value;
                clone.transform.parent = ObjectPool.transform.FindChild(item.Value).transform;
                clone.transform.localScale = Vector3.one;
                clone.transform.GetChild(0).gameObject.SetActive(false);    // 新版 子物件隱藏
            }
        }

        SkillMice();

        

//        Debug.Log("pooling Mice Completed ! ");
        _poolingFlag = true;
    }


    /// <summary>
    /// 每一次顯示一個GameObject。如果GameObject不足，Spawn一個物件並顯示。
    /// </summary>
    /// <param name="miceID">使用ID找Object</param>
    /// <returns>回傳 ( GameObject / null )</returns>
    public GameObject ActiveObject(int miceID)
    {
        _dictObject.TryGetValue(miceID, out objectName);//等傳老鼠ID名稱近來這要改miceName

        if (ObjectPool.transform.FindChild(objectName).childCount == 0)
        {
            clone = (GameObject)Instantiate(ObjectDeck[miceID - 1], Vector3.zero, Quaternion.identity);
            clone.name = objectName;
            clone.transform.parent = ObjectPool.transform.FindChild(objectName).transform;
            clone.transform.localScale = Vector3.one;
            return clone;
        }

        for (int i = 0; i < ObjectPool.transform.FindChild(objectName).childCount; i++)
        {
            GameObject mice;

            mice = ObjectPool.transform.FindChild(objectName).GetChild(i).gameObject;

            if (mice.name == objectName && !mice.transform.GetChild(0).gameObject.activeSelf)
            {
                mice.transform.GetChild(0).gameObject.SetActive(true);
                return mice;
            }

        }
        return null;
    }

    /// <summary>
    /// 每一次顯示一個GameObject。如果GameObject不足，Spawn一個物件並顯示。
    /// </summary>
    /// <param name="objectName">使用Name找Object</param>
    /// <returns>回傳 ( GameObject / null )</returns>
    public GameObject ActiveObject(string objectName)
    {
        int miceID = _dictObject.FirstOrDefault(x => x.Value == objectName).Key;       // 找Key
        /*
        foreach (KeyValuePair<int, string> item in dictMice)
        {
            Debug.Log("In Dict: "+ item.Value);
        }
         * */
        if (ObjectPool.transform.FindChild(objectName).childCount == 0)
        {
            clone = (GameObject)Instantiate(ObjectDeck[miceID - 1], Vector3.zero, Quaternion.identity);
            clone.name = objectName;
            clone.transform.parent = ObjectPool.transform.FindChild(objectName).transform;
            clone.transform.localScale = Vector3.one;
            return clone;
        }

        for (int i = 0; i < ObjectPool.transform.FindChild(objectName).childCount; i++)
        {
            GameObject mice;

            mice = ObjectPool.transform.FindChild(objectName).GetChild(i).gameObject;

            if (mice.name == objectName && !mice.transform.GetChild(0).gameObject.activeSelf)
            {
                mice.transform.GetChild(0).gameObject.SetActive(true);
                return mice;
            }
        }
        return null;
    }

    public void SkillMice()
    {
        _skillMice = Json.Deserialize(Global.Team) as Dictionary<string, object>;

//        Debug.Log(Global.Team);
//        Debug.Log(_skillMice.Count);
        int i = 0;
        // 產生 技能老鼠
        foreach (KeyValuePair<string, object> item in _skillMice)
        {
            clone = (GameObject)Instantiate(ObjectDeck[Convert.ToInt16(item.Key) - 1]);   //　等傳老鼠ID名稱近來這要改
            clone.transform.GetChild(0).GetComponent<Animator>().enabled = false;
            clone.transform.GetChild(0).GetComponent<BoxCollider2D>().enabled = false;
            clone.transform.GetChild(0).GetComponent<MonoBehaviour>().enabled = false;
            clone.name = item.Value.ToString();
            Skill.transform.GetChild(i).transform.gameObject.SetActive(true);
            clone.transform.parent = Skill.transform.GetChild(i);
            clone.layer = clone.transform.parent.gameObject.layer;

            if (i == 4)
            {
                clone.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f); 
            }
            else
            {
                clone.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f); 
            }

            clone.transform.localPosition = Vector3.zero;

            i++;
        }
    }

    public void MergeMice()
    {
        _tmpDict = Json.Deserialize(Global.Team) as Dictionary<string, object>;

        //把自己的老鼠存入HashSet中等待比較，再把老鼠存入合併好的老鼠Dict中
        foreach (KeyValuePair<string, object> item in _tmpDict)
        {
            _myMice.Add(Int16.Parse(item.Key));
            _dictObject.Add(Int16.Parse(item.Key), item.Value.ToString());
        }
        _tmpDict.Clear();
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
//        Debug.Log("Merge Mice Completed ! ");
    }


    void Update()
    {
        _currentTime = Time.time;

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
}
