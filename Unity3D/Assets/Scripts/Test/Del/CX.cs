using UnityEngine;
using System.Collections;

public class CX : MonoBehaviour {

	// Use this for initialization
	void Start () {
        BX b = new BX();
        AX a = new AX(10);
        b.Test(a);
        b.Get();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
