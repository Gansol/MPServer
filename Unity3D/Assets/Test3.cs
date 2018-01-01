using UnityEngine;
using System.Collections.Generic;
using MiniJSON;
using System.Linq;

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
        Dictionary<string, object> a;
        a = new Dictionary<string, object>();



        List<string> keys = a.Keys.ToList();
        Debug.Log(keys.Count);

       
	}

	
	// Update is called once per frame
	void Update () {
	
	}
}
