using UnityEngine;
using System.Collections;

public class SpawnTest : MonoBehaviour {

    private GameObject[] hole;

	// Use this for initialization
	void Start () {

        hole = new GameObject[transform.Find("Hole").childCount];
        for (int i = 0; i < transform.Find("Hole").childCount; i++)
        {
            hole[i] = transform.Find("Hole").GetChild(i).gameObject;
            hole[i].GetComponent<UILabel>().text = i.ToString();
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
