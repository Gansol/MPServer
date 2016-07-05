using UnityEngine;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public GameObject processBar;
    private AsyncOperation async;
    uint _process;

    void Start()
    {
        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        async = Application.LoadLevelAsync(Global.loadScene);
        async.allowSceneActivation = false;

        yield return async;
    }

    void Update()
    {
        if (async == null)
            return;

        
//        Debug.Log(async.progress * 100);
        if (async.progress < 0.9f)  // 會卡在90% else = 100%
        {
            _process = (uint)(async.progress * 100);
        }
        else
        {
            _process = 100;
        }

        processBar.GetComponent<UILabel>().text = _process.ToString() + "%";

        if (_process == 100)
        {
            async.allowSceneActivation = true;
        }
    }

}