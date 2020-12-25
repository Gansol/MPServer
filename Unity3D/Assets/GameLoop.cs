using UnityEngine;
using System.Collections;

public class GameLoop : MonoBehaviour {

	// Use this for initialization
	void Awake () {
        DontDestroyOnLoad(this.gameObject);
        MPGame.Instance.Initialize(this);
	}

    private void OnGUI()
    {
        MPGame.Instance.OnGUI();
    }

    // Update is called once per frame
    void Update () {
        MPGame.Instance.Update();
	}

    void FixedUpdate()
    {
        MPGame.Instance.FixedUpdate();
    }
}
