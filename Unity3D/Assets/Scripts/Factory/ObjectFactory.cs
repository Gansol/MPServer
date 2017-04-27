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
    /// <param name="spriteScale">2D圖形縮放(Witdh Height) Vetor2.zero = not scale</param>
    /// <param name="depth">深度值 -1=不改變</param>
    /// <returns></returns>
    public GameObject Instantiate(GameObject bundle, Transform parent, string name, Vector3 position, Vector3 scale, Vector2 spriteScale, int depth)
    {
        if (bundle != null)
        {
            _clone = (GameObject)GameObject.Instantiate(bundle);             // 實體化
            DepthManager.SwitchDepthLayer(_clone, parent, depth);
            _clone.transform.parent = parent;
            _clone.name = name;

            _clone.transform.localPosition = position;
            _clone.transform.localScale = scale;


            if (spriteScale != Vector2.zero && _clone.GetComponent<UISprite>() != null)
            {
                _clone.GetComponent<UISprite>().width = System.Convert.ToInt32(spriteScale.x);
                _clone.GetComponent<UISprite>().height = System.Convert.ToInt32(spriteScale.y);
            }
            return _clone;
        }
        Debug.Log("  Instantiate bundle is null !!!! ");
        return null;
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
    /// <param name="depth">深度</param>
    /// <returns></returns>-
    public GameObject InstantiateActor(GameObject bundle, Transform parent, string name, Vector3 sacle, int depth)
    {
        _clone = Instantiate(bundle, parent, name, Vector3.zero, sacle, Vector2.zero, -1);
        _clone.SetActive(false);
        DepthManager.SwitchDepthLayer(_clone, parent, depth);
        _clone.SetActive(true);

        return _clone;
    }
    #endregion

    /// <summary>
    /// 產生老鼠 還不完整
    /// </summary>
    /// <param name="miceID"></param>
    /// <param name="hole"></param>
    public GameObject InstantiateMice(PoolManager poolManager, short miceID, float miceSize, GameObject hole, bool impose)
    {
        Vector3 _miceSize;
        if (hole.GetComponent<HoleState>().holeState == HoleState.State.Open || impose)
        {
            if (impose && Global.dictBattleMice.ContainsKey(hole.transform))
                Global.dictBattleMice[hole.transform].GetComponentInChildren<MiceBase>().SendMessage("OnDead", 0.0f);

            if (Global.dictBattleMice.ContainsKey(hole.transform))
                Global.dictBattleMice.Remove(hole.transform);

            GameObject clone = poolManager.ActiveObject(miceID.ToString());
            if (clone != null)
            {
                hole.GetComponent<HoleState>().holeState = HoleState.State.Closed;
                _miceSize = hole.transform.Find("ScaleValue").localScale / 10 * miceSize;   // Scale 版本
                clone.transform.parent = hole.transform;              // hole[-1]是因為起始值是0 
                clone.layer = hole.layer;
                clone.transform.localPosition = Vector2.zero;
                clone.transform.localScale = hole.transform.GetChild(0).localScale - _miceSize;  // 公式 原始大小分為10等份 10等份在減掉 要縮小的等份*乘洞的倍率(1.4~0.9) => 1.0整份-0.2份*1(洞口倍率)=0.8份 
                //clone.GetComponent<BoxCollider2D>().enabled = true;

                clone.GetComponent<Creature>().Play(AnimatorState.ENUM_AnimatorState.Hello);
                Global.dictBattleMice.Add(clone.transform.parent, clone);
                clone.transform.gameObject.SetActive(false);
                clone.transform.gameObject.SetActive(true);
                return clone;
            }
        }
        return null;
    }

    /// <summary>
    /// 從道具名稱取得道具ID
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="columns">ItemID SkillID etc</param>
    /// <param name="miceName"></param>
    /// <returns></returns>
    #region -- GetIDFromName --
    public static int GetIDFromName(Dictionary<string, object> dictionary, string columns, string miceName)
    {
        object value;
        foreach (KeyValuePair<string, object> item in dictionary)
        {
            var nestedData = item.Value as Dictionary<string, object>;
            nestedData.TryGetValue("ItemName", out value);
            if (miceName == value.ToString())
            {
                nestedData.TryGetValue(columns, out value);
                return int.Parse(value.ToString());
            }
        }
        return -1;
    }
    #endregion


    public static object GetColumnsDataFromID(Dictionary<string, object> dictionary, string columns, string miceID)
    {
        object value;

        if (dictionary.TryGetValue(miceID, out value))
        {
            Dictionary<string, object> dictSkill = value as Dictionary<string, object>;
            if (dictSkill.TryGetValue(columns, out value))
                return value;
        }

        return -1;
    }






    #region -- GetItemInfoFromType 取得道具(類別)資訊  --
    public static Dictionary<string, object> GetItemInfoFromType(Dictionary<string, object> itemData, int type)
    {
        Dictionary<string, object> data = new Dictionary<string, object>();

        foreach (KeyValuePair<string, object> item in itemData)
        {
            var nestedData = item.Value as Dictionary<string, object>;
            object itemType;
            nestedData.TryGetValue("ItemType", out itemType);
            if (itemType != null)
                if (itemType.ToString() == type.ToString())
                {
                    data.Add(nestedData["ItemID"].ToString(), nestedData);
                }
        }
        return data;
    }
    #endregion

    #region -- GetItemInfoFromType 取得道具(類別)資訊  --
    public static Dictionary<string, object> GetItemInfoFromID(Dictionary<string, object> itemData, string columns, int type)
    {
        Dictionary<string, object> data = new Dictionary<string, object>();

        foreach (KeyValuePair<string, object> item in itemData)
        {
            var nestedData = item.Value as Dictionary<string, object>;
            object itemID;
            nestedData.TryGetValue(columns, out itemID);


            char[] itemType = itemID.ToString().ToCharArray(0, 1);
            if (itemType[0].ToString() == type.ToString())
            {
                data.Add(nestedData[columns].ToString(), nestedData);
            }
        }
        return data;
    }
    #endregion

    //#region -- GetItemNameFromID 從道具ID取得道具名稱 --
    ///// <summary>
    ///// 從道具ID取得道具名稱
    ///// </summary>
    ///// <param name="itemName">道具名稱</param>
    ///// <param name="itemData">2d Dictionary</param>
    ///// <returns>itemName</returns>
    //public static  string GetItemNameFromID(string itemID, Dictionary<string, object> itemData)
    //{
    //    object objNested;

    //    itemData.TryGetValue(itemID.ToString(), out objNested);
    //    if (objNested != null)
    //    {
    //        var dictNested = objNested as Dictionary<string, object>;
    //        return dictNested["ItemName"].ToString();
    //    }
    //    return "";
    //}
    //#endregion

    //#region -- GetItemIDFromName 從道具名稱取得道具ID --
    ///// <summary>
    ///// 從道具名稱取得道具ID
    ///// </summary>
    ///// <param name="itemName">道具名稱</param>
    ///// <param name="columns">ID欄位名稱</param>
    ///// <param name="itemData">2d Dictionary</param>
    ///// <returns></returns>
    //public static string GetItemIDFromName(string itemName,string columns, Dictionary<string, object> itemData)
    //{
    //    object value;
    //    foreach (KeyValuePair<string, object> item in itemData)
    //    {
    //        var nestedData = item.Value as Dictionary<string, object>;
    //        nestedData.TryGetValue("ItemName", out value);
    //        if (itemName == value.ToString())
    //        {
    //            nestedData.TryGetValue(columns, out value);
    //            return value.ToString();
    //        }
    //    }
    //    return "";
    //}
    //#endregion


}
