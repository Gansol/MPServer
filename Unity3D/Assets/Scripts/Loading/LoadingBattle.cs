﻿using UnityEngine;
using System.Collections;

public class LoadingBattle : MonoBehaviour
{
    public int level;
    //AsyncOperation Loader;

    void Awake()
    {
        Global.nextScene = level;
    }

    void Start()
    {
        Global.nextScene = level;
//        Debug.Log(Global.loadScene);
    }

    void Update()
    {
        if (Global.BattleStatus)
        {
            StartCoroutine(LoadLevel());
        }
    }

    private IEnumerator LoadLevel()
    {
        Application.LoadLevel(Global.nextScene);  //之後要改成LoadScene
        yield return null;
    }

    /*
    private IEnumerator loadLevel(string levelToLoad)
    {
        int displayProgress = 0;
        int loaderProgress = 0;
        AsyncOperation Loader = Application.LoadLevelAsync(levelToLoad);
        Loader.allowSceneActivation = false;
        while (Loader.progress < 0.99f)
        {
            loaderProgress = (int)Loader.progress * 100;
            while (displayProgress < loaderProgress)
            {
                ++displayProgress;
                SetLoadingPercentage(displayProgress);
                yield return new WaitForEndOfFrame();
            }
        }

        loaderProgress = 100;
        while (displayProgress < loaderProgress)
        {
            ++displayProgress;
            SetLoadingPercentage(displayProgress);
            yield return new WaitForEndOfFrame();
        }
        Loader.allowSceneActivation = true;
    }
    

    void SetLoadingPercentage(int displayProgress)
    {
        Debug.Log(displayProgress);
    }
    */
}
