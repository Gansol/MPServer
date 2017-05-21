using UnityEngine;
using System.Collections.Generic;
using MPProtocol;
public class LoadProperty
{
    #region -- LoadProperty 載入老鼠屬性 --
    /// <summary>
    /// 載入老鼠全部屬性
    /// </summary>
    /// <param name="name"></param>
    /// <param name="parent"></param>
    /// <param name="offset">基本=0。載入欄位偏移值</param>
    public void LoadItemProperty(GameObject item, GameObject parent,Dictionary<string,object> itemData, int itemType)
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
    public void LoadPrice(GameObject item, GameObject parent, int itemType)
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

}
