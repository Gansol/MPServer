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
 * 支援多種物件的物件池
 * 每種物件的數量控制是固定的(個別不同數量請自行修改)
 * 
 * ***************************************************************/

public class ObjectPool 
{
    private Dictionary<int, string> _dictObject;

    private GameObject clone;
    //private byte objectID;          // 取得的ID 須自行改寫
    private string objectName;      // 取得的 物件名稱

    private float _lastTime;
    private float _currentTime;

    [Tooltip("物件池位置")]
    public GameObject Pool;
    [Tooltip("物件匣")]
    public GameObject[] ObjectDeck;
    [Tooltip("產生數量")]
    public int spawnCount = 5;
    [Tooltip("物件池上限(各種類分開)")]
    public int clearLimit = 5;
    [Tooltip("物件池上限(各種類分開)")]
    public int clearTime = 10;
    [Tooltip("物件池保留量(各種類分開)")]
    public int reserveCount = 2;

    private bool _poolingFlag = false;        // 初始化物件池
    public bool poolingFlag { get { return _poolingFlag; } }

    void Awake()
    {
        _lastTime = 0;
        _currentTime = 0;
        clearTime = 10;
        _poolingFlag = false;      // 初始化物件池
        _dictObject = new Dictionary<int, string>();

        Debug.Log(ObjectDeck.Length);
        // 加入字典
        for (int item=0;item < ObjectDeck.Length;item++)
        {
            _dictObject.Add(item, ObjectDeck[item].name);
        }


        // 生出 預設數量的物件
        foreach (KeyValuePair<int, string> item in _dictObject)
        {
            clone = new GameObject();

            clone.name = item.Key.ToString();
            clone.transform.parent = Pool.transform;
            clone.layer = clone.transform.parent.gameObject.layer;
            clone.transform.localScale = Vector3.one;

            for (int i = 0; i < spawnCount; i++)
            {
                clone = (GameObject)GameObject.Instantiate(ObjectDeck[item.Key]);   //　等傳老鼠ID名稱近來這要改
                clone.name = item.Key.ToString();
                clone.transform.parent = Pool.transform.Find(item.Key.ToString()).transform;
                clone.transform.localScale = Vector3.one;
                clone.transform.gameObject.SetActive(false);    // 新版 子物件隱藏
            }
        }
         Debug.Log("pooling Completed ! ");
        _poolingFlag = true;
    }


    /// <summary>
    /// 每一次顯示一個GameObject。如果GameObject不足，Spawn一個物件並顯示。
    /// </summary>
    /// <param name="objectID">使用ID找Object</param>
    /// <returns>回傳 ( GameObject / null )</returns>
    public GameObject ActiveObject(short objectID)
    {
        //_dictObject.TryGetValue(objectID, out objectName);//等傳老鼠ID名稱近來這要改

        objectName = objectID.ToString();

        if (Pool.transform.Find(objectName).childCount == 0)
        {
            clone = (GameObject)GameObject.Instantiate(ObjectDeck[objectID], Vector3.zero, Quaternion.identity);
            clone.name = objectName;
            clone.transform.parent = Pool.transform.Find(objectName).transform;
            clone.transform.localScale = Vector3.one;
            return clone;
        }

        for (int i = 0; i < Pool.transform.Find(objectName).childCount; i++)
        {
            GameObject go;

            go = Pool.transform.Find(objectName).GetChild(i).gameObject;

            if (go.name == objectName && !go.transform.gameObject.activeSelf)
            {
                go.transform.gameObject.SetActive(true);
                return go;
            }
        }
        return null;
    }

    ///// <summary>
    ///// 每一次顯示一個GameObject。如果GameObject不足，Spawn一個物件並顯示。
    ///// </summary>
    ///// <param name="objectName">使用Name找Object</param>
    ///// <returns>回傳 ( GameObject / null )</returns>
    //public GameObject ActiveObject(string objectName)
    //{
    //    int objectID = _dictObject.FirstOrDefault(x => x.Value == objectName).Key;       // 找Key

    //    if (Pool.transform.FindChild(objectName).childCount == 0)
    //    {
    //        clone = (GameObject)Instantiate(ObjectDeck[objectID], Vector3.zero, Quaternion.identity);
    //        clone.name = objectName;
    //        clone.transform.parent = Pool.transform.FindChild(objectName).transform;
    //        clone.transform.localScale = Vector3.one;
    //        return clone;
    //    }

    //    for (int i = 0; i < Pool.transform.FindChild(objectName).childCount; i++)
    //    {
    //        GameObject go;

    //        go = Pool.transform.FindChild(objectName).GetChild(i).gameObject;

    //        if (go.name == objectName && !go.transform.gameObject.activeSelf)
    //        {
    //            go.transform.gameObject.SetActive(true);
    //            return go;
    //        }
    //    }
    //    return null;
    //}

    void Update()
    {
        _currentTime = Time.time;

        if (_currentTime - _lastTime > clearTime)     // 達到清除時間時
        {
            for (int i = 0; i < Pool.transform.childCount; i++)       // 跑遍動態池
            {
                if (Pool.transform.GetChild(i).childCount > clearLimit)           // 如果動態池超過限制數量
                {
                    for (int j = 0; j < Pool.transform.GetChild(i).childCount - reserveCount; j++)    // 銷毀物件
                    {
                        GameObject.Destroy(Pool.transform.GetChild(i).GetChild(j).gameObject);
                    }
                }
            }
            _lastTime = _currentTime;
        }
    }
}
