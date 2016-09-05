using UnityEngine;
using System.Collections;
using MPProtocol;
public class LoadMiceProperty 
{

    #region -- LoadProperty 載入老鼠屬性 --
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="parent"></param>
    /// <param name="num">7:載入所有屬性(含商品)、1:載入部分屬性(不含商品)、2:載入名稱、價格</param>
    public void LoadProperty(GameObject item, GameObject parent, int num)
    {
        string[,] miceData = Global.miceProperty;
        switch (num)
        {
            case 2:
                parent.transform.GetChild(0).GetChild(1).GetComponent<UILabel>().text = item.GetComponent<Item>().property[(int)ItemProperty.MiceName].ToString();
                parent.transform.GetChild(0).GetChild(2).GetComponent<UILabel>().text = item.GetComponent<Item>().property[(int)ItemProperty.Price].ToString();
                parent.transform.GetChild(0).GetChild(3).GetComponent<UILabel>().text = item.GetComponent<Item>().property[(int)ItemProperty.Price].ToString(); //這是錯的
                break;

            case 0:                               
                for (int i = 0; i < miceData.GetLength(0); i++)// 載入玩家擁有老鼠
                {
                    if (item.name == miceData[i, 0])                                 //如果按鈕和玩家擁有老鼠相同
                    {
                        for (int j = 0; j < parent.transform.childCount; j++)     //載入老鼠資料
                        {
                            if (j != 0) parent.transform.GetChild(0).GetChild(j).GetComponent<UILabel>().text = miceData[i, j - 1];
                        }
                        break;
                    }
                }
                break;
            case 1:
                for (int i = 0; i < miceData.GetLength(0); i++)// 載入玩家擁有老鼠
                {
                    if (item.name == miceData[i, 0])                                 //如果按鈕和玩家擁有老鼠相同
                    {
                        for (int j = 0; j < miceData.GetLength(1) - 2; j++)     //載入老鼠資料
                        {
                            if (j != 0) parent.transform.GetChild(j).GetComponent<UILabel>().text = miceData[i, j - 1];// (PS:0在SQL中是老鼠名稱)  
                        }
                        break;
                    }
                }
                break;
        }
    }
    #endregion
}
