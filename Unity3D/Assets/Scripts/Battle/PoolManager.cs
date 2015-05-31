using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using MiniJSON;
using System;

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
 * 使用方法 須配合使用NGUI
 * 否則需要改寫transfrom.SetActive() + 儲存GameObject索引列
 * 
 * ***************************************************************/

public class PoolManager : MonoBehaviour
{
    Dictionary<string, object> _tmpDict;
    private Dictionary<int, string> dictMice;
    private HashSet<int> _myMice;
    private HashSet<int> _otherMice;
    //private List<GameObject> Pool;
    private GameObject clone;
    //private int _miceCount;
    private byte miceID;
    private string miceName;
    private int[] _miceIDArray;
    private float lastTime;
    private float currentTime;
    //private GameObject[] _dynamicPoolName;
    private int index;      // _dynamicPoolName index
    private static int miceAllCount;

    public GameObject Panel;

    [Tooltip("物件池位置")]
    public GameObject ObjectPool;
    [Tooltip("物件匣")]
    public GameObject[] ObjectDeck;



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
        lastTime = 0;
        currentTime = 0;
        clearTime = 10;
        _poolingFlag = false;      // 初始化物件池
        //Pool = new List<GameObject>();
        //dictObjectPool = new Dictionary<string, List<GameObject>>();
        dictMice = new Dictionary<int, string>();
        //_miceCount = 0;
        _myMice = new HashSet<int>();
        _otherMice = new HashSet<int>();
        _tmpDict = new Dictionary<string, object>();
        MergeMice();                                // 將雙方的老鼠合併 剔除相同的老鼠
        /*
        clone = new GameObject();
        clone.name = "ObjectPool";
        clone.transform.parent = Panel.transform;   // 預設位置
        clone.transform.localScale = Vector3.one;   // 預設大小
        clone.layer = 31;                           // 去火星吧！    大家都看不見
        ObjectPool = clone;                         // 物件池位置
        */

        // 生出 預設數量的物件
        foreach (KeyValuePair<int, string> item in dictMice)
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
        Debug.Log("pooling Mice Completed ! ");
        _poolingFlag = true;
    }


    /// <summary>
    /// 每一次顯示一個GameObject。如果GameObject不足，Spawn一個物件並顯示。回傳 ( GameObject / null )
    /// </summary>
    /// <param name="miceID"></param>
    /// <returns></returns>
    public GameObject ActiveObject(int miceID)
    {
        dictMice.TryGetValue(miceID, out miceName);//等傳老鼠ID名稱近來這要改miceName

        if (ObjectPool.transform.FindChild(miceName).childCount == 0)
        {
            clone = (GameObject)Instantiate(ObjectDeck[miceID - 1], Vector3.zero, Quaternion.identity);
            clone.name = miceName;
            clone.transform.parent = ObjectPool.transform.FindChild(miceName).transform;
            clone.transform.localScale = Vector3.one;
            //clone.GetComponent<UISprite>().width = 260;
            //clone.GetComponent<UISprite>().height = 290;

            return clone;
        }

        for (int i = 0; i < ObjectPool.transform.FindChild(miceName).childCount; i++)
        {
            GameObject mice;

            mice = ObjectPool.transform.FindChild(miceName).GetChild(i).gameObject;

            if (mice.name == miceName && !mice.transform.GetChild(0).gameObject.activeSelf)//等傳老鼠ID名稱近來這要改 nicename
            {
                mice.transform.GetChild(0).gameObject.SetActive(true);
                return mice;
            }

        }

        //Debug.Log("miceAllCount : "+miceAllCount+"miceName : " + miceName + " acitveCount : " + acitveCount + "FindGameObjectsWithTag(miceName).Length : " + GameObject.FindGameObjectsWithTag(miceName).Length);





        //Debug.Log("_miceCount "+_miceCount);
        return null;
    }


    public void MergeMice()
    {
        _tmpDict = Json.Deserialize(Global.Team) as Dictionary<string, object>;
        //Debug.Log(_tmpDict.Count);
        //_tmpDict.Add(1, "EggMice");
        //_tmpDict.Add(2, "BggMice");
        //把自己的老鼠存入HashSet中等待比較，再把老鼠存入合併好的老鼠Dict中
        foreach (KeyValuePair<string, object> item in _tmpDict)
        {
            _myMice.Add(Int16.Parse(item.Key));
            dictMice.Add(Int16.Parse(item.Key), item.Value.ToString());
        }

        _tmpDict = Json.Deserialize(Global.OtherData.Team) as Dictionary<string, object>;
        //_tmpDict.Add(2, "BlackMice");
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

                dictMice.Add(item, miceName.ToString());
            }
        }
        _mergeFlag = true;
        Debug.Log("Merge Mice Completed ! ");
    }


    void Update()
    {
        currentTime = Time.time;

        if (currentTime - lastTime > clearTime)     // 達到清除時間時
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
            lastTime = currentTime;
        }
    }
}
