using UnityEngine;
using System.Collections.Generic;
using System;
using MiniJSON;

public class test2 : MonoBehaviour
{
    Dictionary<string, object> _tmpDict;
    Dictionary<string, object> _myDict;
    Dictionary<int, string> dictMice;

    HashSet<int> _myMice;
    HashSet<int> _otherMice;

    public GameObject[] aa;

    void Start()
    {
        _tmpDict = new Dictionary<string, object>();
        _myDict = new Dictionary<string, object>();
        dictMice = new Dictionary<int, string>();
        _myMice = new HashSet<int>();
        _otherMice = new HashSet<int>();
        Debug.Log(aa[0].name);
    }

    public void MergeMice()
    {
        _tmpDict = Json.Deserialize(Global.Team) as Dictionary<string, object>;

        _tmpDict.Add("1", "EggMice");
        _tmpDict.Add("2", "BlackMice");
        //把自己的老鼠存入HashSet中等待比較，再把老鼠存入合併好的老鼠Dict中
        foreach (KeyValuePair<string, object> item in _tmpDict)
        {
            _myMice.Add(Int16.Parse(item.Key));
            dictMice.Add(Int16.Parse(item.Key), item.Value.ToString());
        }
        
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

                dictMice.Add(item, miceName.ToString());
            }
        }
        Debug.Log("Merge Mice Completed ! ");
    }
}