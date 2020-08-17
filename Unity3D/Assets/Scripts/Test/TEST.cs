using UnityEngine;
using System.Collections.Generic;
using System.Text;
using MiniJSON;
using System.IO;
using UnityEngine.Networking;
using System.Collections;
using System.Linq;
using System;

public class TEST : MonoBehaviour
{


    private void Start()
    {
        // 第一個字母大寫
    Debug.Log(    System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase("aAAa".ToLower()));
    }


    //// add dymic scripts componment
    //private void Start()
    //{

    //    Type t = Type.GetType("EggMiceBoss");
    //    gameObject.AddComponent(t);
    //    MiceBossBase c = gameObject.GetComponent(t) as MiceBossBase;
    //    c.enabled = true;
    //}



    // marge two dict , have same key value , diff key value and expect same
    //private void Start()
    //{
    //    Dictionary<string, object> dictMyMice = new Dictionary<string, object>();
    //    Dictionary<string, object> dictOtherMice = new Dictionary<string, object>();
    //    Dictionary<string, object> mergeMice;


    //    dictMyMice.Add("a", "a");
    //    dictMyMice.Add("b", "b");
    //    dictMyMice.Add("f", "f");

    //    dictOtherMice.Add("a", "a");
    //    dictOtherMice.Add("b", "b");
    //    dictOtherMice.Add("d", "d");
    //    dictOtherMice.Add("e", "e");

    //    mergeMice = new Dictionary<string, object>(dictMyMice);

    //    foreach (KeyValuePair<string, object> item in mergeMice)
    //    {
    //        if (dictOtherMice.ContainsKey(item.Key))
    //            dictOtherMice.Remove(item.Key);
    //    }

    //    dictMyMice =  dictMyMice.Concat(dictOtherMice).ToDictionary(x => x.Key, x => x.Value); ;

    //    foreach (KeyValuePair<string, object> item in dictMyMice)
    //    {
    //        Debug.Log(item.Key);
    //    }
    //    Debug.Log("---------------------------------");
    //    foreach (KeyValuePair<string, object> item in dictOtherMice)
    //    {
    //        Debug.Log(item.Key);
    //    }
    //    Debug.Log("---------------------------------");
    //}

    //private void Start()
    //{
    //    HashSet<string> a = new HashSet<string>();
    //    HashSet<string> b = new HashSet<string>();


    //    a.Add("a");
    //    b.Add("a");
    //    b.Add("c");
    //    a.UnionWith(b);


    //    Dictionary<string, object> c = new Dictionary<string, object>();
    //    Dictionary<string, object> d = new Dictionary<string, object>();


    //    c.Add("a", a);
    //    d.Add("a", a);


    //    Debug.Log(a.Count);
    //    Debug.Log(c==d);
    //}

    //private void Start()
    //{
    //    string a = @"C:\Users\Administrator\AppData\LocalLow\Gansol\MicePow2020\AssetBundles\creature\share\bali.unity3d";
    //    Debug.Log(a.Replace(Path.GetFileName(a), ""));
    //}


    // string 比較 false true 
    //string str = "panel/a/a.unity";
    // string[] a ;
    // string o;

    // private void Start()
    // {
    //     string s1 = "A";
    //     string s2 = "A";
    //     string s3 = new string(new char['A']) ;

    //     Debug.Log(s1 == s3);
    //     Debug.Log(s1.Equals( s3));
    // }


    //void Start()
    //{
    //    StartCoroutine(GetText());
    //}

    //IEnumerator GetText()
    //{
    //    string aa = "file:///" + Application.persistentDataPath + "/AssetBundles/" + "panel/share/panelui.unity3d" ;

    //    Debug.Log(aa);
    //    using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(aa))
    //    {
    //        yield return uwr.SendWebRequest();

    //        if (uwr.isNetworkError || uwr.isHttpError)
    //        {
    //            Debug.Log(uwr.error);
    //        }
    //        else
    //        {
    //            // Get downloaded asset bundle
    //            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(uwr);
    //            Debug.Log("OK");
    //        }
    //    }
    //}
}
