using System;
using System.EnterpriseServices;
using ExitGames.Logging;
using System.Collections.Generic;
using MPProtocol;
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
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

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
        public StoreData LoadStoreData(int itemID, byte itemType)
        {
            storeData.ReturnCode = "(Logic)S900";
            storeData.ReturnMessage = "";

            try
            {
                //to do check 

                StoreDataIO storeDataIO = new StoreDataIO();
                storeData = storeDataIO.LoadStoreData(itemID, itemType);

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
        public StoreData UpdateStoreBuyCount(int itemID, byte itemType, int buyCount/*, Int16 limitCount*/)
        {
            StoreData storeData = new StoreData();
            storeData.ReturnCode = "(Logic)S900";
            storeData.ReturnMessage = "";

            StoreDataIO storeDataIO = new StoreDataIO();

            try
            {
                storeData = storeDataIO.LoadStoreData(itemID, itemType);
                storeData.BuyCount += buyCount;

                storeData.PromotionsCount = storeData.PromotionsCount > 0 ? (Int16)(storeData.PromotionsCount - (Int16)buyCount) : storeData.PromotionsCount;

                if (storeData.ReturnCode == "S901")
                {
                    //如果驗證成功 寫入玩家資料
                    storeData = storeDataIO.UpdateStoreBuyCount(itemID, itemType, storeData.BuyCount, storeData.PromotionsCount);
                    //Log.Debug("storeData.ReturnCode: " + storeData.ReturnCode + "storeData.BuyCount: " + storeData.BuyCount);
                }
                else
                {
                    storeData.ReturnCode = "(Logic)S904";
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
        public StoreData UpdateStoreLimit(int itemID, byte itemType, int buyCount)
        {
            StoreData storeData = new StoreData();
            storeData.ReturnCode = "(Logic)S900";
            storeData.ReturnMessage = "";

            StoreDataIO storeDataIO = new StoreDataIO();

            try
            {
                storeData = storeDataIO.LoadStoreData(itemID, itemType);
                storeData.PromotionsCount = storeData.PromotionsCount > 0 ? (Int16)(storeData.PromotionsCount - (Int16)buyCount) : storeData.PromotionsCount;

                if (storeData.ReturnCode == "S901")
                {
                    //如果驗證成功 寫入玩家資料
                    storeData = storeDataIO.UpdatedStorePromotionsCount(itemID, storeData.ItemType, storeData.PromotionsCount);
                }

            }
            catch (Exception e)
            {
                throw e;
            }
            return storeData;
        }
        #endregion

        #region -- BuyGashapon 載入單筆商店資料 --
        /// <summary>
        /// 載入單筆商店資料
        /// </summary>
        /// <param name="itemName"></param>
        /// <param name="itemType"></param>
        /// <returns></returns>
        [AutoComplete]
        public StoreData BuyGashapon(int itemID)
        {
            storeData.ReturnCode = "(Logic)S900";
            storeData.ReturnMessage = "";

            try
            {

                //to do check 
                object value, price,itemType, promotionsCount,limitCount;
                bool onSell;
                Dictionary<string, object> data = new Dictionary<string, object>();
                Dictionary<string, object> nestedData = new Dictionary<string, object>();

                // 載入商店資料
                storeData = LoadStoreData();

                // 字典轉換
                data = MiniJSON.Json.Deserialize(storeData.StoreItem) as Dictionary<string, object>;

                // 取得對應道具資料
                data.TryGetValue(itemID.ToString(), out value);
                nestedData = value as Dictionary<string, object>;

                // 如果 還有促銷商品
                nestedData.TryGetValue("PromotionsCount", out promotionsCount);

                if (int.Parse(promotionsCount.ToString()) > 0 || int.Parse(promotionsCount.ToString()) == -1)
                {
                    nestedData.TryGetValue("OnSell", out value);

                    bool.TryParse(value.ToString(), out onSell);
                    nestedData.TryGetValue("Price", out price);
                    nestedData.TryGetValue("ItemType", out itemType);

                    // 如果 銷售中
                    if (onSell)
                    {
                        nestedData.TryGetValue("Limit", out limitCount);

                        // 如果 沒有超過購買限制數量
                        if (int.Parse(limitCount.ToString()) == -1 || int.Parse(limitCount.ToString()) > 0)
                        {
                            storeData.Price = short.Parse(price.ToString());

                            storeData = UpdateStoreBuyCount(itemID, (byte)StoreType.Gashapon, (short)1);

                            if (storeData.ReturnCode == "S902")
                            {
                                storeData.Price = short.Parse(price.ToString());
                                storeData.ItemType = byte.Parse(itemType.ToString());
                                storeData.ReturnCode = "S903";
                                storeData.ReturnMessage = "轉蛋購買成功！";
                            }
                            
                            return storeData;
                        }
                    }
                }
                else
                {
                    storeData.ReturnCode = "S905";
                    storeData.ReturnMessage = "轉蛋商品銷售完畢！";
                    return storeData;
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
