using System;
using System.EnterpriseServices;
using MPProtocol;
using ExitGames.Logging;

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
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
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

        #region UpdateCurrency 更新貨幣資料
        /// <summary>
        /// 更新貨幣資料 目前沒有驗證邏輯
        /// </summary>
        /// <param name="account"></param>
        /// <param name="currencyType">貨幣種類</param>
        /// <param name="currency">貨幣價值</param>
        /// <returns></returns>
        [AutoComplete]
        public CurrencyData UpdateCurrency(string account, byte currencyType, int currency)
        {
            CurrencyData currencyData = new CurrencyData();
            currencyData.ReturnCode = "(Logic)S700";
            currencyData.ReturnMessage = "";

            // 目前沒有驗證邏輯
            try
            {
                int value = 0;

                CurrencyIO currencyIO = new CurrencyIO();
                currencyData = currencyIO.LoadCurrency(account);

                switch (currencyType)
                {
                    case (byte)CurrencyType.Rice:
                        {
                            value = currencyData.Rice + currency;
                            break;
                        }
                    case (byte)CurrencyType.Gold:
                        {
                            value = currencyData.Gold + currency;
                            break;
                        }
                    case (byte)CurrencyType.Bonus:
                        {
                            value = currencyData.Rice + currency;
                            break;
                        }
                    default:
                        currencyData.ReturnMessage = "金流資料未知例外情況！";
                        currencyData.ReturnCode = "S799";
                        break;
                }



                if (value >= 0)
                {
                    currencyData = currencyIO.UpdateCurrency(account, value.ToString(), (CurrencyType)currencyType);
                }
                else
                {
                    if (currencyType == (byte)CurrencyType.Rice)
                    {
                        currencyData.ReturnMessage = "遊戲幣不足！";
                        currencyData.ReturnCode = "S711";
                    }
                    if (currencyType == (byte)CurrencyType.Rice)
                    {
                        if (currencyType == (byte)CurrencyType.Gold)
                            currencyData.ReturnMessage = "金幣不足！";
                        currencyData.ReturnCode = "S712";
                    }
                    if (currencyType == (byte)CurrencyType.Bonus)
                    {
                        value = value + currencyData.Gold;

                        // 目前紅利採金幣模式 與金幣共用優先使用紅利 使用者看不到 未來新增紅利再修正
                        if (value >= 0)
                        {
                            currencyData = currencyIO.UpdateCurrency(account, value.ToString(), (CurrencyType)currencyType);
                        }
                        else
                        {
                            currencyData.ReturnMessage = "金幣不足！";
                            currencyData.ReturnCode = "S712";
                        }
                    }
                }
            }
            catch
            {
                throw;
            }

            return currencyData;

        }
        #endregion
    }
}
