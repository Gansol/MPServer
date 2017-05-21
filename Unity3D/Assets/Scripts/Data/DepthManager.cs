﻿using UnityEngine;
using System.Collections;
using System;
/* ***************************************************************
 * -----Copyright © 2015 Gansol Studio.  All Rights Reserved.-----
 * -----------            CC BY-NC-SA 4.0            -------------
 * -----------  @Website:  EasyUnity@blogspot.com    -------------
 * -----------  @Email:    GansolTW@gmail.com        -------------
 * -----------  @Author:   Krola.                    -------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * 
 * ***************************************************************
 *                           ChangeLog
 * 20160705 v1.0.0  改變遊戲物件深度(NGUI)                         
 * ****************************************************************/
public class DepthManager : MonoBehaviour
{
    /// <summary>
    /// 改變遊戲物件 深度 與 Layer
    /// </summary>
    /// <param name="go">要改變深度的遊戲物件(NGUI)</param>
    /// <param name="parent">要改變Layer父系位置(遊戲物件)</param>
    /// <param name="depth">增加深度值 (-1不改變)</param>
    /// <returns></returns>
    public static int SwitchDepthLayer(GameObject go, Transform parent, int depth)  //使用遞迴改變老鼠圖片深度  ※※注意遞迴※※
    {
        if (go != null && parent != null)
        {
            if (go.GetComponent<UISprite>() != null)
            {
                UISprite sprite = go.GetComponent<UISprite>();
                if (sprite != null && parent != null)
                {
                    if (depth != -1) sprite.depth += depth;
                    go.gameObject.layer = parent.gameObject.layer;
                }
            }

            go.gameObject.layer = parent.gameObject.layer;

            foreach (Transform child in go.transform)
            {
                //Debug.Log("Layer : " + child.name);
                UISprite sprite = child.GetComponent<UISprite>();
                if (sprite != null)
                {
                    if (depth != -1) sprite.depth += depth;
                }

                child.gameObject.layer = parent.gameObject.layer;
                SwitchDepthLayer(child.gameObject, parent, depth);  // 遞迴
            }
        }

        return depth;
    }
}
