using UnityEngine;
using System.Collections;

public class DontDestory : MonoBehaviour {

   
	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(transform.gameObject);

        if (FindObjectsOfType(GetType()).Length > 1)
        {
            Destroy(transform.gameObject);
        }
	}
}
