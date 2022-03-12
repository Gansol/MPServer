﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;


/// <summary>
/// 滑鼠點兩下的程式碼 沒作用
/// </summary>
public class Coll : MonoBehaviour
{
    public float delayBetween2Clicks = 2f;
    public float _lastClickTime = 0;


    void Start()
    {

    }



    #region OnMiceClick
    public void OnMiceClick(GameObject btn_mice)
    {
        if (Time.time - _lastClickTime < delayBetween2Clicks )    // Double Click
            Debug.Log("Double click");
        else
            StartCoroutine(OnClickCoroutine());

        _lastClickTime = Time.time;
    }

    IEnumerator OnClickCoroutine()
    {
        yield return new WaitForSeconds(delayBetween2Clicks);

        if (Time.time - _lastClickTime < delayBetween2Clicks)
            yield break;
        yield return 1;
        Debug.Log("Simple click");
    }
    #endregion
}
