using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public class PhotonTestOnly : MonoBehaviour {


    enum a :int
    {
        a=1,
    }

	// Use this for initialization
	void Start () {
        Debug.Log(((int)a.a).ToString());
        object x = 1;

        string updateString ="";
        updateString += String.Format(" when ItemID='{0}' ", x);
        Debug.Log(updateString);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI()
    {
        Dictionary<string, Dictionary<string, object>> test = new Dictionary<string, Dictionary<string, object>>();
        Dictionary<string, object> data;

        data = new Dictionary<string, object>();
        data.Add("0", 20001);
        data.Add("1", 100);
        data.Add("4", 66);
        test.Add("20001", data);

        data = new Dictionary<string, object>();
        data.Add("0", 30002);
        data.Add("1", 200);
        data.Add("4", 77);
        test.Add("30002", data);

        string js = MiniJSON.Json.Serialize(test);

        var dict = MiniJSON.Json.Deserialize(js) as Dictionary<string,object>;
        object outd;
        dict.TryGetValue("30002", out outd);

        Debug.Log(outd);
        /*
        foreach(KeyValuePair<string,Dictionary<string,object>> item in test){
            Debug.Log(item.Key);

           Dictionary<string,object> items= item.Value as Dictionary<string,object>;

           foreach (KeyValuePair<string, object> item2s in items)
           {
               Debug.Log(item2s.Key);
           }

        }
        */
        object x, y;
        int a, b;
        x = 10001; y = 2;

        a = (int)x - (int)y;
        Debug.Log(a);
        Debug.Log((int)x > (int)y);
        Debug.Log(Int16.Parse(a.ToString()));
        string vv = "32767";
        Debug.Log(System.Convert.ToInt16(vv));

        Debug.Log(js);
        if (GUI.Button(new Rect(100, 400, 250, 100), "Test"))
        {
            Global.photonService.UpdatePlayerItem(js);
        }
    }
}
