﻿using System;
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
 * 這個檔案是用來進行 驗證貨幣資料所使用
 * 載入貨幣資料、更新貨幣資料
 * >>try catch 要移除 使用AutoComplete就可 移除後刪除
 * >>邏輯都沒寫
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
    public class CurrencyLogic : ServicedComponent  // ServicedComponent 表示所有使用 COM+ 服務之類別的基底類別。
    {
        CurrencyData currencyData = new CurrencyData();

        protected override bool CanBePooled()
        {
            return true;
        }

        #region LoadCurrency 載入貨幣資料

        [AutoComplete]
        public CurrencyData LoadCurrency(string account)
        {
            currencyData.ReturnCode = "(Logic)S700";
            currencyData.ReturnMessage = "";

            try
            {
                //to do check 

                CurrencyIO currencyIO = new CurrencyIO();
                currencyData = currencyIO.LoadCurrency(account);

            }
            catch (Exception e)
            {
                throw e;
            }

            return currencyData;
        }

        #endregion

        #region UpdateCurrency

        [AutoComplete]
        public CurrencyData UpdateCurrency(string account, int rice,Int16 gold)
        {
            CurrencyData currencyData = new CurrencyData();
            currencyData.ReturnCode = "(Logic)S700";
            currencyData.ReturnMessage = "";

            try
            {
                // to do check
                    CurrencyIO currencyIO = new CurrencyIO();
                    currencyData = currencyIO.UpdateCurrency(account,rice,gold);
            }
            catch (Exception e)
            {
                throw e;
            }

            return currencyData;

        }

        #endregion
    }
}