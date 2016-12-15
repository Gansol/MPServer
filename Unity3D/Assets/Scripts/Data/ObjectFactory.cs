using System.Collections.Generic;
using UnityEngine;
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
 * 20160914 v1.0.0 新增實體化物件、角色                         
 * ****************************************************************/
public class ObjectFactory
{
    GameObject _clone;

    #region -- Instantiate 實體化物件 --
    /// <summary>
    /// 實體化物件
    /// </summary>
    /// <param name="bundle">實體化物件</param>
    /// <param name="parent">上層</param>
    /// <param name="name">名稱</param>
    /// <param name="position">位置</param>
    /// <param name="scale">縮放</param>
    /// <param name="spriteScale">2D圖形縮放 Vetor2.zero = not scale</param>
    /// <param name="depth">深度值 -1=不改變</param>
    /// <returns></returns>
    public GameObject Instantiate(GameObject bundle, Transform parent, string name, Vector3 position, Vector3 scale, Vector2 spriteScale, int depth)
    {
        _clone = (GameObject)GameObject.Instantiate(bundle);             // 實體化
        _clone.layer = parent.gameObject.layer;
        _clone.transform.parent = parent;
        _clone.name = name;

        _clone.transform.localPosition = position;
        _clone.transform.localScale = scale;

        if (depth != -1) _clone.GetComponent<UISprite>().depth = depth;
        if (spriteScale != Vector2.zero)
        {
            _clone.GetComponent<UISprite>().width = System.Convert.ToInt32(spriteScale.x);
            _clone.GetComponent<UISprite>().height = System.Convert.ToInt32(spriteScale.y);
        }
        return _clone;
    }
    #endregion

    #region -- InstantiateActor 實體化老鼠角色  --
    /// <summary>
    /// 實體化老鼠角色
    /// </summary>
    /// <param name="bundle">實體化物件</param>
    /// <param name="parent">上層</param>
    /// <param name="name">名稱</param>
    /// <param name="sacle">縮放</param>
    /// <returns></returns>-
    public GameObject InstantiateActor(GameObject bundle, Transform parent, string name, Vector3 sacle)
    {
        _clone = Instantiate(bundle, parent, name, Vector3.zero, sacle, Vector2.zero, -1);
        _clone.SetActive(false);
        DepthManager.SwitchDepthLayer(_clone, parent, Global.MeunObjetDepth);
        _clone.SetActive(true);
        
        return _clone;
    }
    #endregion

    /// <summary>
    /// 從道具名稱取得道具ID
    /// </summary>
    /// <param name="miceName">道具名稱</param>
    /// <param name="itemData">2d Dictionary</param>
    /// <returns>itemName</returns>
    #region -- GetItemNameFromID --
    public static int GetItemIDFromName(string miceName)
    {
        object value;
        foreach (KeyValuePair<string, object> item in Global.miceProperty)
        {
            var nestedData = item.Value as Dictionary<string, object>;
            nestedData.TryGetValue("ItemName", out value);
            if (miceName == value.ToString())
            {
                nestedData.TryGetValue("ItemID", out value);
                return int.Parse(value.ToString());
            }
        }
        return -1;
    }
    #endregion
}
