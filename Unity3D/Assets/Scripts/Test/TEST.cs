using UnityEngine;
using System.Collections.Generic;
using System.Text;
using MiniJSON;
using System.IO;
using UnityEngine.Networking;
using System.Collections;

public class TEST : MonoBehaviour
{
   string str = "panel/a/a.unity";
    string[] a ;
    string o;

    private void Start()
    {
        string s1 = "A";
        string s2 = "A";
        string s3 = new string(new char['A']) ;

        Debug.Log(s1 == s3);
        Debug.Log(s1.Equals( s3));
    }


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
