using UnityEngine;
using System.Collections;

public class TestGUI : MonoBehaviour {
	
	// Update is called once per frame
	void OnGUI () {
        GUI.Button(new Rect(0, 0, 60, 30), " - ");
        GUI.Button(new Rect(60, 0, 50, 30), "Status");
        GUI.Button(new Rect(110, 0, 60, 30), " + ");

        GUI.Button(new Rect(0, 50, 60, 30), " - ");
        GUI.Button(new Rect(60, 50, 50, 30), "Status");
        GUI.Button(new Rect(110, 50, 60, 30), " + ");


        GUI.Button(new Rect(190, 0, 60, 30), " - ");
        GUI.Button(new Rect(250, 0, 50, 30), "Status");
        GUI.Button(new Rect(300, 0, 60, 30), " + ");

        GUI.Button(new Rect(190, 50, 60, 30), " - ");
        GUI.Button(new Rect(250, 50, 50, 30), "Status");
        GUI.Button(new Rect(300, 50, 60, 30), " + ");


        GUI.Button(new Rect(280, 480, 75, 75), "Status");
	}
}
