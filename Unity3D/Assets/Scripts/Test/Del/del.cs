using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class del : MonoBehaviour
{

    void Start()
    {
        Dictionary<string, object> a = new Dictionary<string, object>();

        a.Add("1", 0);
        a.Add("2", 0);
        a.Add("3", 0);
       RenameKey(a,"2","4");
     

        foreach (KeyValuePair<string, object> item in a)
            Debug.Log(item.Key);
    }

    public void RenameKey<TKey, TValue>(this IDictionary<TKey, TValue> dic,
                                          TKey fromKey, TKey toKey)
    {
        TValue value = dic[fromKey];
        dic.Remove(fromKey);
        dic[toKey] = value;
    }

}
