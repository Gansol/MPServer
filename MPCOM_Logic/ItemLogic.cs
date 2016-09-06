using System;
using System.EnterpriseServices;
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
    // TransactionOption 指定元件要求的自動交易類型。
    // NotSupported	沒有使用支配性的交易在內容中建立元件。
    // Required	共用交易 (如果存在的話)，並且建立新交易 (如果有必要的話)。
    // RequiresNew	不論目前內容的狀態如何，都使用新交易建立元件。
    // Supported	共用交易 (如果有存在的話)。
    [Transaction(TransactionOption.Required)]
    public class ItemLogic : ServicedComponent    // ServicedComponent 表示所有使用 COM+ 服務之類別的基底類別。
    {
        protected override bool CanBePooled()
        {
            return true;
        }

        #region GetItemData 取得道具數量
        /// <summary>
        /// 取得道具數量
        /// </summary>
        /// <param name="itemName">道具名稱</param>
        /// <param name="itemType">道具類別</param>
        /// <returns></returns>
        [AutoComplete]
        public ItemData GetItemData(string itemName, byte itemType)
        {
            ItemData itemData = new ItemData();
            itemData.ReturnCode = "(Logic)S600";
            itemData.ReturnMessage = "";

            ItemIO itemIO = new ItemIO();
            
            try
            {
                itemData = itemIO.GetItemData(itemName, itemType);
            }
            catch (Exception e)
            {
                throw e;
            }

            return itemData;

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
        [AutoComplete]
        public ItemData UpdateItemBuyCount(string itemName, byte itemType, int buyCount)
        {
            ItemData itemData = new ItemData();
            itemData.ReturnCode = "(Logic)S600";
            itemData.ReturnMessage = "";

            ItemIO itemIO = new ItemIO();

            try
            {
                itemData = itemIO.GetItemData(itemName, itemType);
                itemData.BuyCount += buyCount;

                if (itemData.ReturnCode == "S601")
                {
                    //如果驗證成功 寫入玩家資料
                    itemData = itemIO.UpdateItemBuyCount(itemData.ItemName, itemData.ItemType, itemData.BuyCount);
                }

            }
            catch (Exception e)
            {
                throw e;
            }

            return itemData;

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
        [AutoComplete]
        public ItemData UpdateItemLimit(string itemName, byte itemType, int buyCount)
        {
            ItemData itemData = new ItemData();
            itemData.ReturnCode = "(Logic)S600";
            itemData.ReturnMessage = "";

            ItemIO itemIO = new ItemIO();

            try
            {
                itemData = itemIO.GetItemData(itemName, itemType);
                itemData.Limit -= (Int16)buyCount;

                if (itemData.ReturnCode == "S601")
                {
                    //如果驗證成功 寫入玩家資料
                    itemData = itemIO.UpdatedItemLimit(itemData.ItemName, itemData.ItemType, itemData.Limit);
                }

            }
            catch (Exception e)
            {
                throw e;
            }

            return itemData;

        }
        #endregion

        

    }



}
