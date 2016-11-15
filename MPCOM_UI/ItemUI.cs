using System;
using System.EnterpriseServices;
using Gansol;

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
 * 
 * 
 * ***************************************************************/

namespace MPCOM
{
    public interface IItemUI  // 使用介面 可以提供給不同程式語言繼承使用      
    {
        byte[] LoadItemData();
    }

    public class ItemUI : ServicedComponent, IItemUI
    {
        protected override bool CanBePooled()
        {
            return true;
        }

        #region LoadItemData 取得道具資訊
        /// <summary>
        /// 取得道具資訊
        /// </summary>
        /// <param name="itemName">道具名稱</param>
        /// <param name="itemType">道具類別</param>
        /// <returns></returns>
        public byte[] LoadItemData()
        {
            ItemData itemData = new ItemData();
            itemData.ReturnCode = "S600";
            itemData.ReturnMessage = "";
            
            try
            {
                ItemLogic itemLogic = new ItemLogic();
                itemData = itemLogic.LoadItemData();
            }
            catch (Exception e)
            {
                itemData.ReturnCode = "S699";
                itemData.ReturnMessage = e.Message;
            }
            return TextUtility.SerializeToStream(itemData);
        }
        #endregion
    }
}
