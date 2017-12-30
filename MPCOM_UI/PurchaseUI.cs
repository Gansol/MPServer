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
    public interface IPurchaseUI    // 使用介面 可以提供給不同程式語言繼承使用                                
    {
        byte[] LoadPurchase();
        byte[] ConfirmPurchase(string account, string purchaseID, string currencyCode, string currencyValue, string receiptCipheredPayload, string receipt, string description);
    }

    public class PurchaseUI : ServicedComponent, IPurchaseUI
    {
        protected override bool CanBePooled()
        {
            return true;
        }

        #region LoadPurchase 載入法幣商品

        public byte[] LoadPurchase()
        {
            PurchaseData purchaseData = new PurchaseData();
            purchaseData.ReturnCode = "S1100";
            purchaseData.ReturnMessage = "";

            try
            {
                PurchaseLogic purchaseLogic = new PurchaseLogic();
                purchaseData = purchaseLogic.LoadPurchase();
            }
            catch (Exception e)
            {
                purchaseData.ReturnCode = "S1199";
                purchaseData.ReturnMessage = "(UI載入法幣商品資料未知例外情況！　" + e.Message + " 於: " + e.StackTrace;
                throw e;
            }
            return TextUtility.SerializeToStream(purchaseData);
        }
        #endregion


        #region LoadPurchase 確認法幣商品交易

        public byte[] ConfirmPurchase(string account, string purchaseID, string currencyCode, string currencyValue, string receiptCipheredPayload, string receipt, string description)
        {
            CurrencyData currencyData = new CurrencyData();
            currencyData.ReturnCode = "S1100";
            currencyData.ReturnMessage = "";

            try
            {
                PurchaseLogic purchaseLogic = new PurchaseLogic();
                currencyData = purchaseLogic.ConfirmPurchase(account, purchaseID, currencyCode, currencyValue, receiptCipheredPayload, receipt, description);
            }
            catch (Exception e)
            {
                currencyData.ReturnCode = "S1199";
                currencyData.ReturnMessage = "(UI)確認法幣商品交易資料未知例外情況！　" + e.Message + " 於: " + e.StackTrace;
                throw e;
            }
            return TextUtility.SerializeToStream(currencyData);
        }
        #endregion
    }
}