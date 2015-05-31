using UnityEngine;
using System.Collections;
using System;

public class SceneLoader : MonoBehaviour {

    private bool flag;
    //public GameObject progressBar;
	// Use this for initialization
	void Start () {
        flag = true;
        Debug.Log("Scene" + Application.loadedLevelName);
      
	}
	
	// Update is called once per frame
	void Update () {
        if (Application.loadedLevel==2 && flag)
        {
            flag = false;
            StartCoroutine(LoadLevel());
        }
	}

    private IEnumerator LoadLevel()
    {
        try
        {
            AsyncOperation progress = Application.LoadLevelAsync(Global.loadScene);  //之後要改成LoadScene
            //progressBar.GetComponent<UILabel>().text = "Name:" + Application.loadedLevelName + "\n" + progress + "%";
        }
        catch (Exception e)
        {
            Debug.Log("Message: " + e.Message + " 於: " + e.StackTrace);
        }
        yield return null;
    }
}
