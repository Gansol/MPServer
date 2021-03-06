﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class Coll : MonoBehaviour
{
    public float delayBetween2Clicks = 2f;
    public float _lastClickTime = 0;
    private GameObject lastClickObject;


    void Start()
    {
        object x;
        Debug.Log(AssetBundleManager.dictAssetBundleRefs.ContainsKey("EggMiceICON"));
        //AssetBundleManager.dictAssetBundleRefs.Remove("EggMiceICON");
        Debug.Log(AssetBundleManager.dictAssetBundleRefs.ContainsKey("EggMiceICON"));
        //System.GC.Collect();
        //Resources.UnloadUnusedAssets();
        AssetLoader loader = gameObject.AddComponent<AssetLoader>();

        Instantiate(loader.GetAsset("EggMiceICON"));
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
