using UnityEngine;
using System.Collections;

public class LoadSceneDestroyTest : MonoBehaviour {
    static int i = 0;
    public int c = 100;
	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(gameObject);
        if (FindObjectsOfType(GetType()).Length > 1)
        {
            Destroy(gameObject);
        }
        Debug.Log(transform.name+i);
        i++;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI()
    {
        if (GUI.Button(new Rect(50, 50, 100, 100), "Load"))
        {
            Application.LoadLevel(5);
        }
    }
}
