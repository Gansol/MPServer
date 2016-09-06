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
        byte[] GetItemData(string itemName, byte itemType);
        byte[] UpdateItemBuyCount(string itemName, byte itemType, int buyCount);
        byte[] UpdateItemLimit(string itemName, byte itemType, int buyCount);
    }

    public class ItemUI : ServicedComponent, IItemUI
    {
        protected override bool CanBePooled()
        {
            return true;
        }

        #region GetItemData 取得道具資訊
        /// <summary>
        /// 取得道具資訊
        /// </summary>
        /// <param name="itemName">道具名稱</param>
        /// <param name="itemType">道具類別</param>
        /// <returns></returns>
        public byte[] GetItemData(string itemName, byte itemType)
        {
            ItemData itemData = new ItemData();
            itemData.ReturnCode = "S600";
            itemData.ReturnMessage = "";
            
            try
            {
                ItemLogic itemLogic = new ItemLogic();
                itemData = itemLogic.GetItemData(itemName,itemType);
            }
            catch (Exception e)
            {
                itemData.ReturnCode = "S699";
                itemData.ReturnMessage = e.Message;
            }
            return TextUtility.SerializeToStream(itemData);
        }
        #endregion

        #region UpdateItemBuyCount 更新道具購買總數
        /// <summary>
        /// 更新道具購買"總數"
        /// </summary>
        /// <param name="itemName">道具名稱</param>
        /// <param name="itemType">道具類別</param>
        /// <param name="buyCount">購買數量</param>
        /// <returns></returns>
        public byte[] UpdateItemBuyCount(string itemName, byte itemType, int buyCount)
        {
            ItemData itemData = new ItemData();
            itemData.ReturnCode = "S600";
            itemData.ReturnMessage = "";

            try
            {
                ItemLogic itemLogic = new ItemLogic();
                itemData = itemLogic.UpdateItemBuyCount(itemName, itemType, buyCount);
            }
            catch (Exception e)
            {
                itemData.ReturnCode = "S699";
                itemData.ReturnMessage = e.Message;
            }
            return TextUtility.SerializeToStream(itemData);
        }
        #endregion

        #region UpdateItemLimit 更新道具限量數量
        /// <summary>
        /// 更新道具"限量"數量
        /// </summary>
        /// <param name="itemName">道具名稱</param>
        /// <param name="itemType">道具類別</param>
        /// <param name="buyCount">購買數量</param>
        /// <returns></returns>
        public byte[] UpdateItemLimit(string itemName, byte itemType, int buyCount)
        {
            ItemData itemData = new ItemData();
            itemData.ReturnCode = "S600";
            itemData.ReturnMessage = "";

            try
            {
                ItemLogic itemLogic = new ItemLogic();
                itemData = itemLogic.UpdateItemLimit(itemName, itemType, buyCount);
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
