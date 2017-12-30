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
    }

    public class PurchaseUI : ServicedComponent, IPurchaseUI
    {
        protected override bool CanBePooled()
        {
            return true;
        }

        #region LoadPurchase 載入貨幣資料

        public byte[] LoadPurchase()
        {
            PurchaseData purchaseData = new PurchaseData();
            purchaseData.ReturnCode = "S700";
            purchaseData.ReturnMessage = "";

            try
            {
                PurchaseLogic purchaseLogic = new PurchaseLogic();
                purchaseData = purchaseLogic.LoadPurchase();
            }
            catch (Exception e)
            {
                purchaseData.ReturnCode = "S799";
                purchaseData.ReturnMessage = "(UI)載入貨幣資料未知例外情況！　" + e.Message + " 於: " + e.StackTrace;
                throw e;
            }
            return TextUtility.SerializeToStream(purchaseData);
        }
        #endregion
    }
}