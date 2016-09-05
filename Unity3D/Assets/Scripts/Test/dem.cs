using UnityEngine;
using System.Collections;
using MiniJSON;
public class dem : MonoBehaviour {

	// Use this for initialization
	void Start () {
	    string[,] a = new string[2,2];
        a.SetValue("a", 0,0);
        a.SetValue("b", 0,1);

        string j = Json.Serialize(a);
        Debug.Log(j);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
