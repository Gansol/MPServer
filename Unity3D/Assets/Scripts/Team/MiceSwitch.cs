using UnityEngine;
using System.Collections;

public class MiceSwitch : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.name.ToString().Remove(4) == "Team")
        {
            Debug.Log("OK!");
        }
    }
}
