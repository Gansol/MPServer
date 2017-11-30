using UnityEngine;
using System.Collections.Generic;
using MPProtocol;
public static class LoadProperty
{
    #region -- LoadProperty 載入老鼠屬性 --
    /// <summary>
    /// 載入老鼠全部屬性
    /// </summary>
    /// <param name="name"></param>
    /// <param name="parent"></param>
    /// <param name="offset">基本=0。載入欄位偏移值</param>
    public static void LoadItemProperty(GameObject item, GameObject parent, Dictionary<string, object> itemData, int itemType)
    {
        string ColunmsName = itemType == (int)StoreType.Mice ? "MiceID" : "ItemID";

        foreach (KeyValuePair<string, object> data in itemData)
        {
            var nestedData = data.Value as Dictionary<string, object>;
            object value;
            nestedData.TryGetValue(ColunmsName, out value);

            if (item.name == value.ToString())
            {
                int i = 0;
                foreach (KeyValuePair<string, object> property in nestedData)
                {
                    Transform infoField = parent.transform.FindChild(property.Key);
                    if (infoField != null) infoField.GetComponent<UILabel>().text = property.Value.ToString();
                    i++;
                }
            }
        }
    }
    #endregion

    #region LoadPrice
    public static void LoadPrice(GameObject item, GameObject parent, int itemType)
    {
        int i = 0;
        foreach (KeyValuePair<string, object> mice in Global.storeItem)
        {
            var nestedData = mice.Value as Dictionary<string, object>;
            object value;
            nestedData.TryGetValue("ItemID", out value);

            if (item.name == value.ToString())
            {
                nestedData.TryGetValue("ItemType", out value);
                if (itemType == int.Parse(value.ToString()))
                {
                    nestedData.TryGetValue("Price", out value);
                    parent.transform.Find("Price").GetComponent<UILabel>().text = value.ToString();
                    break;
                }
            }
            i++;
        }
    }
    #endregion

    #region -- ExpectOutdataObject 移除非同步物件 --
    /// <summary>
    /// 移除非同步物件
    /// </summary>
    /// <param name="dicServerData">Server Data</param>
    /// <param name="dicClinetData">Client Data</param>
    /// <param name="dicLoadedObject">已載入物件</param>
    public static void ExpectOutdataObject(Dictionary<string, object> dicServerData, Dictionary<string, object> dicClinetData, Dictionary<string, GameObject> dicLoadedObject)
    {
        // var delObject = new Dictionary<string, object>();

        if (dicClinetData.Count != 0)
        {
            foreach (KeyValuePair<string, object> item in dicClinetData)
            {
                if (dicServerData.ContainsValue(item.Value)) // 如果Server有Client的物件
                {
                    if (!dicServerData.ContainsKey(item.Key)) // 如果Server的KEY 和 Client的KEY 不同 移除舊的物件
                    {
                        dicLoadedObject[item.Value.ToString()].GetComponentInChildren<UISprite>().spriteName = null;
                        dicLoadedObject.Remove(item.Value.ToString());
                    }
                }
                else if (dicLoadedObject.ContainsKey(item.Value.ToString())) // 如果載入的物件
                {
                    Debug.Log("BUG");
                    dicLoadedObject[item.Value.ToString()].GetComponentInChildren<UISprite>().spriteName = null;
                    dicLoadedObject.Remove(item.Value.ToString());
                }
            }
        }
    }
    #endregion

    #region -- SelectNewData 獲得伺服器新增資料 --
    /// <summary>
    /// 獲得伺服器新增資料，排除重複
    /// </summary>
    /// <param name="dicServerData">Server Data</param>
    /// <param name="dicClinetData">Client Data</param>
    /// <returns></returns>
    public static Dictionary<string, object> SelectNewData(Dictionary<string, object> dicServerData, Dictionary<string, object> dicClinetData)
    {
        var newObject = new Dictionary<string, object>();           // 新資料
        //var buffer = new Dictionary<string, object>();              // buffer

        foreach (KeyValuePair<string, object> item in dicServerData)
        {
            if (!dicClinetData.ContainsValue(item.Value))           // 如果在Clinet找不到Server資料 = 新資料           
            {
                newObject.Add(item.Key, item.Value);
            }                                                       // 如果在Clinet找不到Server資料 且 如果Clinet和Server Key值不相等
            else if (dicClinetData.ContainsValue(item.Value) && !dicClinetData.ContainsKey(item.Key))
            {
                //buffer = dicServerData;
                Debug.LogError("SelectNewData 還沒寫好 Error!");
            }
        }
        return newObject;
    }
    #endregion
}
