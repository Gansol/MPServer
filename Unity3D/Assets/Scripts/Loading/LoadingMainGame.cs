using UnityEngine;
using System.Collections;

public class LoadingMainGame : MonoBehaviour {

    public string level;
    AsyncOperation Loader;

    void Start()
    {
        Global.loadScene = level;
    }

    void Update()
    {
        if (Global.isExitingRoom)
        {
            StartCoroutine(LoadLevel());
        }
    }

    private IEnumerator LoadLevel()
    {
        Global.isExitingRoom = false;
        Application.LoadLevel("LoadScene");  //之後要改成LoadScene
        yield return null;
    }
}
