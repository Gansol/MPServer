using System;
using UnityEngine;
using System.Security.Cryptography;
using System.Collections.Generic;
using Gansol;
using System.Linq;

/// <summary>
/// 使用 RNGCryptoServiceProvider 產生由密碼編譯服務供應者 (CSP) 提供的亂數產生器。
/// </summary>
public class Test3 : MonoBehaviour
{
    Data[] datas;
    Data[] datas2;
    Dictionary<string, object> dict = new Dictionary<string, object>();
    [Serializable()]
    public struct Data
    {
        public string name;
        public string id;
    }



    public void Start()
    {
        datas = new Data[5];


        for (int i = 0; i < datas.Length; i++)
        {
            datas[i].name = "a" + i.ToString();
            datas[i].id = "1";
        }

        datas[3].id = "2";
        datas[4].id = "2";
       

        byte[] x = TextUtility.SerializeToStream(datas);


        for (int i = 0; i < x.Length; i++)
        {
            Debug.Log(x[i]);
        }

        datas2 = TextUtility.DeserializeFromStream(x) as Data[];

        for (int i = 0; i < datas2.Length; i++)
        {
            Debug.Log("datas2:" + datas2[i].id + " " + datas2[i].name);
        }

        List<string> aaa = new List<string>();
       aaa =  datas2.Select(c => c.id).ToList();


       foreach (string s in aaa)
       {
           Debug.Log("list:"+s);
       }

    }

}
