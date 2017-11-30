using UnityEngine;
using System.Collections;

public class GameLoop : MonoBehaviour {

	// Use this for initialization
	void Awake () {
        GameObject.DontDestroyOnLoad(this.gameObject);
        MPGame.Instance.Initinal(this);
	}
	
	// Update is called once per frame
	void Update () {
        MPGame.Instance.Update();
	}
}
