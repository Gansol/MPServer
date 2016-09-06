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
 * 遊戲貨幣界面層 提供外部存取使用
 * 載入貨幣、更新貨幣資料
 * 
 * ***************************************************************/

namespace MPCOM
{
    public interface ICurrencyUI    // 使用介面 可以提供給不同程式語言繼承使用                                
    {
        byte[] LoadCurrency(string account);
        byte[] UpdateCurrency(string account, byte itemType, int currency);
    }

    public class CurrencyUI : ServicedComponent, ICurrencyUI
    {
        protected override bool CanBePooled()
        {
            return true;
        }

        #region LoadCurrency 載入貨幣資料

        public byte[] LoadCurrency(string account)
        {
            CurrencyData currencyData = new CurrencyData();
            currencyData.ReturnCode = "S700";
            currencyData.ReturnMessage = "";

            try
            {
                CurrencyLogic currencyLogic = new CurrencyLogic();
                currencyData = currencyLogic.LoadCurrency(account);
            }
            catch (Exception e)
            {
                currencyData.ReturnCode = "S799";
                currencyData.ReturnMessage = "(UI)載入貨幣資料未知例外情況！　" + e.Message + " 於: " + e.StackTrace;
                throw e;
            }
            return TextUtility.SerializeToStream(currencyData);
        }
        #endregion

        #region UpdateCurrency 更新貨幣資料
        /// <summary>
        /// 更新貨幣資料
        /// </summary>
        /// <param name="account">帳號</param>
        /// <param name="itemType">道具種類</param>
        /// <param name="currency">消費總額</param>
        /// <returns></returns>
        public byte[] UpdateCurrency(string account, byte itemType, int currency)
        {
            CurrencyData currencyData = new CurrencyData();
            currencyData.ReturnCode = "S700";
            currencyData.ReturnMessage = "";

            try
            {
                CurrencyLogic currencyLogic = new CurrencyLogic();
                currencyData = currencyLogic.UpdateCurrency(account,  itemType,  currency);
            }
            catch (Exception e)
            {
                currencyData.ReturnCode = "S799";
                currencyData.ReturnMessage = "(UI)更新貨幣資料未知例外情況！　" + e.Message + " 於: " + e.StackTrace;
                throw e;
            }
            return TextUtility.SerializeToStream(currencyData);
        }
        #endregion


    }
}
