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
 * 老鼠資料界面層 提供外部存取使用
 * 載入老鼠資料
 * 
 * ***************************************************************/

namespace MPCOM
{
    public interface IStoreDataUI    // 使用介面 可以提供給不同程式語言繼承使用      
    {
        byte[] LoadStoreData();
        byte[] LoadStoreData(string itemName, byte itemType);
        byte[] UpdateStoreBuyCount(string itemName, byte itemType, int buyCount);
        byte[] UpdateStoreLimit(string itemName, byte itemType, int buyCount);

    }

    public class StoreDataUI : ServicedComponent, IStoreDataUI
    {
        protected override bool CanBePooled()
        {
            return true;
        }

        #region LoadStoreData 載入單筆商店資料
        /// <summary>
        /// 載入單筆商店資料
        /// </summary>
        /// <param name="itemName"></param>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public byte[] LoadStoreData(string itemName,byte itemType)
        {
            StoreData storeData = new StoreData();
            storeData.ReturnCode = "(UI)S900";
            storeData.ReturnMessage = "";

            try
            {

                StoreDataLogic storeDataLogic = new StoreDataLogic();
                storeData = storeDataLogic.LoadStoreData(itemName, itemType);
            }
            catch (Exception e)
            {
                storeData.ReturnCode = "S999";
                storeData.ReturnMessage = e.Message;
            }
            return TextUtility.SerializeToStream(storeData);
        }
        #endregion

        #region LoadStoreData 載入全部商店資料
        /// <summary>
        /// 載入全部商店資料
        /// </summary>
        /// <returns></returns>
        public byte[] LoadStoreData()
        {
            StoreData storeData = new StoreData();
            storeData.ReturnCode = "(UI)S900";
            storeData.ReturnMessage = "";

            try
            {

                StoreDataLogic storeDataLogic = new StoreDataLogic();
                storeData = storeDataLogic.LoadStoreData();
            }
            catch (Exception e)
            {
                storeData.ReturnCode = "S999";
                storeData.ReturnMessage = e.Message;
            }
            return TextUtility.SerializeToStream(storeData);
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
        public byte[] UpdateStoreBuyCount(string itemName, byte itemType, int buyCount)
        {
            StoreData storeData = new StoreData();
            storeData.ReturnCode = "S900";
            storeData.ReturnMessage = "";

            try
            {
                StoreDataLogic storeDataLogic = new StoreDataLogic();
                storeData = storeDataLogic.UpdateStoreBuyCount(itemName, itemType, buyCount);
            }
            catch (Exception e)
            {
                storeData.ReturnCode = "S999";
                storeData.ReturnMessage = e.Message;
            }
            return TextUtility.SerializeToStream(storeData);
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
        public byte[] UpdateStoreLimit(string itemName, byte itemType, int buyCount)
        {
            StoreData storeData = new StoreData();
            storeData.ReturnCode = "S900";
            storeData.ReturnMessage = "";

            try
            {
                StoreDataLogic storeDataLogic = new StoreDataLogic();
                storeData = storeDataLogic.UpdateStoreLimit(itemName, itemType, buyCount);
            }
            catch (Exception e)
            {
                storeData.ReturnCode = "S999";
                storeData.ReturnMessage = e.Message;
            }
            return TextUtility.SerializeToStream(storeData);
        }
        #endregion

    }
}
