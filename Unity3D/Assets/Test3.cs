using UnityEngine;
using System.Collections.Generic;
using MiniJSON;

public class Test3 : MonoBehaviour {

	// Use this for initialization
	void Start () {
        //string a = "90.00005";
        //string b= "90.60005";

        //int value = 0;
        //int.TryParse(a, out value);
        //Debug.Log("1-1:"+value);
        //int.TryParse(b, out value);
        //Debug.Log("1-2:" + value);



        //value = System.Convert.ToInt32(a);
        //Debug.Log("3-1:" + value);
        //value = System.Convert.ToInt32(b);
        //Debug.Log("3-3:" + value);

        //value = int.Parse(a);
        //Debug.Log("2-1:" + value);
        //value = int.Parse(b);
        //Debug.Log("2-2:" + value);
        object f;
        Dictionary<string, object> a, b, c, d;
        a = new Dictionary<string, object>();
        b = new Dictionary<string, object>();
        a.Add("a", "a");
        b.Add("b1", a);
        b.Add("b2", a);


       
        b.TryGetValue("b1", out f);

        c = f as Dictionary<string, object>;

        Debug.Log(c["a"]);


        int i =-1;

        foreach (var x in b)
        {
            if (++i == 0) Debug.Log("XXX");
        }

	}

	
	// Update is called once per frame
	void Update () {
	
	}
}
