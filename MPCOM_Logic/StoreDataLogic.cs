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
 * 這個檔案是用來進行 驗證老鼠資料所使用
 * 載入老鼠資料
 * >>try catch 要移除 使用AutoComplete就可 移除後刪除
 * 邏輯都沒寫
 * ***************************************************************/

namespace MPCOM
{
    // TransactionOption 指定元件要求的自動交易類型。
    // NotSupported	沒有使用支配性的交易在內容中建立元件。
    // Required	共用交易 (如果存在的話)，並且建立新交易 (如果有必要的話)。
    // RequiresNew	不論目前內容的狀態如何，都使用新交易建立元件。
    // Supported	共用交易 (如果有存在的話)。
    [Transaction(TransactionOption.Required)]
    public class StoreDataLogic : ServicedComponent// ServicedComponent 表示所有使用 COM+ 服務之類別的基底類別。
    {
        StoreData storeData = new StoreData();


        protected override bool CanBePooled()
        {
            return true;
        }

        #region -- LoadStoreData 載入單筆商店資料 --
        /// <summary>
        /// 載入單筆商店資料
        /// </summary>
        /// <param name="itemName"></param>
        /// <param name="itemType"></param>
        /// <returns></returns>
        [AutoComplete]
        public StoreData LoadStoreData(string itemName , byte itemType)
        {
            storeData.ReturnCode = "(Logic)S900";
            storeData.ReturnMessage = "";

            try
            {
                //to do check 

                StoreDataIO storeDataIO = new StoreDataIO();
                storeData = storeDataIO.LoadStoreData(itemName,itemType);

            }
            catch (Exception e)
            {
                throw e;
            }

            return storeData;
        }

        #endregion

        #region -- LoadStoreData 載入全部商店資料 --
        /// <summary>
        /// 載入全部商店資料
        /// </summary>
        /// <returns></returns>
        [AutoComplete]
        public StoreData LoadStoreData()
        {
            storeData.ReturnCode = "(Logic)S900";
            storeData.ReturnMessage = "";

            try
            {
                //to do check 

                StoreDataIO storeDataIO = new StoreDataIO();
                storeData = storeDataIO.LoadStoreData();

            }
            catch (Exception e)
            {
                throw e;
            }

            return storeData;
        }

        #endregion


        #region UpdateStoreBuyCount 更新道具購買總數
        /// <summary>
        /// 更新道具購買"總數"
        /// </summary>
        /// <param name="itemName">道具名稱</param>
        /// <param name="itemType">道具類別</param>
        /// <param name="buyCount">購買數量</param>
        /// <returns></returns>
        [AutoComplete]
        public StoreData UpdateStoreBuyCount(string itemName, byte itemType, int buyCount)
        {
            StoreData storeData = new StoreData();
            storeData.ReturnCode = "(Logic)S900";
            storeData.ReturnMessage = "";

            StoreDataIO storeDataIO = new StoreDataIO();

            try
            {
                storeData = storeDataIO.LoadStoreData(itemName,itemType);
                storeData.BuyCount += buyCount;

                if (storeData.ReturnCode == "S901")
                {
                    //如果驗證成功 寫入玩家資料
                    storeData = storeDataIO.UpdateStoreBuyCount(storeData.ItemName, storeData.ItemType, storeData.BuyCount);
                }

            }
            catch (Exception e)
            {
                throw e;
            }

            return storeData;

        }
        #endregion

        #region UpdateStoreLimit 更新道具限量數量
        /// <summary>
        /// 更新道具"限量"數量
        /// </summary>
        /// <param name="itemName">道具名稱</param>
        /// <param name="itemType">道具類別</param>
        /// <param name="buyCount">購買數量</param>
        /// <returns></returns>
        [AutoComplete]
        public StoreData UpdateStoreLimit(string itemName, byte itemType, int buyCount)
        {
            StoreData storeData = new StoreData();
            storeData.ReturnCode = "(Logic)S900";
            storeData.ReturnMessage = "";

            StoreDataIO storeDataIO = new StoreDataIO();

            try
            {
                storeData = storeDataIO.LoadStoreData(itemName, itemType);
                storeData.PromotionsCount -= (Int16)buyCount;

                if (storeData.ReturnCode == "S901")
                {
                    //如果驗證成功 寫入玩家資料
                    storeData = storeDataIO.UpdatedStoreLimit(storeData.ItemName, storeData.ItemType, storeData.PromotionsCount);
                }

            }
            catch (Exception e)
            {
                throw e;
            }
            return storeData;
        }
        #endregion

    }
}
