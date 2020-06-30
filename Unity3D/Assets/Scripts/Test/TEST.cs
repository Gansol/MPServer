using UnityEngine;
using System.Collections.Generic;
using System.Text;
using MiniJSON;
using System.IO;
using UnityEngine.Networking;
using System.Collections;

public class TEST : MonoBehaviour
{

    void Start()
    {
        StartCoroutine(GetText());
    }

    IEnumerator GetText()
    {
        string aa = "file:///" + Application.persistentDataPath + "/AssetBundles/" + "panel/share/panelui.unity3d" ;

        Debug.Log(aa);
        using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(aa))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // Get downloaded asset bundle
                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(uwr);
                Debug.Log("OK");
            }
        }
    }
}
