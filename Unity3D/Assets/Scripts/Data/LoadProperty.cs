using UnityEngine;
using System.Collections;
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
        string[,] miceData = Global.miceProperty;

        for (int i = offset; i < miceData.GetLength(0); i++)// 載入玩家擁有老鼠
        {
            if (item.name == miceData[i, 1])                                 //如果按鈕和玩家擁有老鼠相同
            {
                for (int j = 0; j < miceData.GetLength(1); j++)     //載入老鼠資料
                {
                    if (j != 0) parent.transform.GetChild(j).GetComponent<UILabel>().text = miceData[i, j];
                }
                break;
            }
        }
    }
    #endregion

    #region LoadPrice
    public void LoadPrice(GameObject item, GameObject parent, int offset)
    {
        string[,] storeData = Global.storeItem;

        for (int i = 0; i < storeData.GetLength(0); i++)// 載入玩家擁有老鼠
        {
            if (item.name == storeData[i, 1])                                 //如果按鈕和玩家擁有老鼠相同
            {
                for (int j = offset; j < storeData.GetLength(1); j++)     //載入老鼠資料
                {
                    if (j != 0) parent.transform.GetChild(j).GetComponent<UILabel>().text = storeData[i, (int)StoreProperty.Price];
                }
                break;
            }
        }
    }
    #endregion

    #region LoadItemProperty
    void LoadItemProperty()
    {

    }
    #endregion

}
