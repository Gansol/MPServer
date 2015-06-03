using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

public class TEST : MonoBehaviour
{
    float a = 50;
    bool _timeFlag;
    static float _lastTime;

    void Start()
    {
        _timeFlag = true;
        _lastTime = 0;
    }

    void Update()
    {
        if (transform.gameObject.activeSelf == true && _timeFlag)  // 如果被Spawn儲存現在時間
        {
            _timeFlag = false;
            _lastTime = Time.time;
        }
        Debug.Log(Time.time - _lastTime);
    }

    public void Play(){
        _timeFlag = true;
    }
 
}
