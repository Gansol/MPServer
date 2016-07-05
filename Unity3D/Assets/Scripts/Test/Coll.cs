using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public class Coll : MonoBehaviour {

    CorountineTest ct;
	// Use this for initialization
	void Start () {
        ct = new CorountineTest();
       StartCoroutine(ct.aa(this));
        
	}
    void bb()
    {
        Debug.Log("BB");
    }
}
