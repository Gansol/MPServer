using UnityEngine;
using System.Collections;

public class TestLoad2 : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI()
    {
        if (GUI.Button(new Rect(100, 100, 100, 100), "Scene2"))
        {
            Application.LoadLevel(0);
        }
    }
}
