using UnityEngine;
using System.Collections;
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
public class DepthManager : MonoBehaviour {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="go">要改變深度的遊戲物件(NGUI)</param>
    /// <param name="parent">要改變Layer父系位置(遊戲物件)</param>
    /// <returns>返回深度值</returns>
    public static int SwitchDepthLayer(GameObject go, Transform parent, int depth)  //使用遞迴改變老鼠圖片深度  ※※注意遞迴※※
    {
        foreach (Transform child in go.transform)
        {
            //Debug.Log("Layer : " + child.name);
            UISprite sprite = child.GetComponent<UISprite>();
            if (sprite != null)
            {
                sprite.depth += depth;
                child.gameObject.layer = parent.gameObject.layer;
            }
            else
            {
                child.gameObject.layer = parent.gameObject.layer;
            }
            SwitchDepthLayer(child.gameObject, parent, depth);  // 遞迴
        }

        return depth;
    }
}
