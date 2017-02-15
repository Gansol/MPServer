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
    public void LoadMiceProperty(GameObject item, GameObject parent, int offset)
    {
        foreach (KeyValuePair<string, object> mice in Global.miceProperty)
        {
            var nestedData = mice.Value as Dictionary<string, object>;
            object value;
            nestedData.TryGetValue("MiceID", out value);

            if (item.name == value.ToString())
            {
                int i = 0;
                foreach (KeyValuePair<string, object> property in nestedData)
                {
                    if (i != 0) parent.transform.GetChild(i).GetComponent<UILabel>().text = property.Value.ToString();
                    i++;
                }
            }
        }


        //for (int i = offset; i < miceData.GetLength(0); i++)// 載入玩家擁有老鼠
        //{
        //    if (item.name == miceData[i, 1])                                 //如果按鈕和玩家擁有老鼠相同
        //    {
        //        for (int j = 0; j < miceData.GetLength(1); j++)     //載入老鼠資料
        //        {
        //            if (j != 0) parent.transform.GetChild(j).GetComponent<UILabel>().text = miceData[i, j];
        //        }
        //        break;
        //    }
        //}
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
            nestedData.TryGetValue("ItemName", out value);

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

        //for (int i = 0; i < storeData.GetLength(0); i++)// 載入玩家擁有老鼠
        //{
        //    if (item.name == storeData[i, 1])                                 //如果按鈕和玩家擁有老鼠相同
        //    {
        //        for (int j = offset; j < storeData.GetLength(1); j++)     //載入老鼠資料
        //        {
        //            if (j != 0) parent.transform.GetChild(j).GetComponent<UILabel>().text = storeData[i, (int)StoreProperty.Price];
        //        }
        //        break;
        //    }
        //}
    }
    #endregion

    #region LoadItemProperty
    void LoadItemProperty()
    {

    }
    #endregion

}
