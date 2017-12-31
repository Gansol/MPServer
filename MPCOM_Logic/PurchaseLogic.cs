using System;
using System.EnterpriseServices;
using MPProtocol;
using ExitGames.Logging;
using MiniJSON;
using System.Collections.Generic;
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
    public class PurchaseLogic : ServicedComponent  // ServicedComponent 表示所有使用 COM+ 服務之類別的基底類別。
    {
        PurchaseData purchaseData = new PurchaseData();
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        protected override bool CanBePooled()
        {
            return true;
        }

        #region LoadPurchase 載入法幣商品資料

        [AutoComplete]
        public PurchaseData LoadPurchase()
        {
            purchaseData.ReturnCode = "(Logic)S1100";
            purchaseData.ReturnMessage = "";

            try
            {
                //to do check 

                PurchaseIO purchaseIO = new PurchaseIO();
                purchaseData = purchaseIO.LoadPurchase();

            }
            catch (Exception e)
            {
                throw e;
            }

            return purchaseData;
        }
        #endregion

        #region ConfirmPurchase 確認法幣商品購買

        [AutoComplete]
        public CurrencyData ConfirmPurchase(string account, string purchaseID, string currencyCode, string currencyValue, string receiptCipheredPayload, string receipt, string description)
        {
            CurrencyData currencyData = new CurrencyData();
            currencyData.ReturnCode = "(Logic)S1100";
            currencyData.ReturnMessage = "";

            // Log.Debug("In ConfirmPurchase Logic");

            Dictionary<string, object> dictPurchaseData, nestedProductData;
            CurrencyType currencyType = CurrencyType.Rice;
            object value = "", price = "";  // price 道具貨幣值
            bool onSell;
            int tmpCurrency = 0, maxValue = 9999999; // 相加後的錢

            PurchaseIO purchaseIO = new PurchaseIO();
            CurrencyIO currencyIO = new CurrencyIO();


            purchaseData = purchaseIO.LoadPurchase();
            currencyData = currencyIO.LoadCurrency(account);

            // 全部法幣道具資料
            dictPurchaseData = Json.Deserialize(purchaseData.jPurchaseData) as Dictionary<string, object>;

            // 找對應道具資料
            dictPurchaseData.TryGetValue(purchaseID, out value);
            nestedProductData = value as Dictionary<string, object>;
            nestedProductData.TryGetValue("OnSell", out value);

            Boolean.TryParse(value.ToString(), out onSell);

            // 如果產品在銷售中
            if (onSell)
            {
                nestedProductData.TryGetValue("ItemName", out value);
                nestedProductData.TryGetValue("Price", out price);

                // 如果 商品ID(名稱micepow_0000)相同 之後陣勢使用錢購買 要用 Google 訂單和自己的確認碼
                if (value.ToString() == purchaseID)
                {
                    nestedProductData.TryGetValue("CurrencyType", out value);
                    currencyType = (CurrencyType)byte.Parse(value.ToString());

                    // 計算對應 金額 (還沒有確認 是否有溢位 非法等)
                    switch (currencyType)
                    {
                        case CurrencyType.Rice:
                            tmpCurrency = (currencyData.Rice + int.Parse(price.ToString())) < maxValue ? currencyData.Rice + int.Parse(price.ToString()) : currencyData.Rice;
                            break;
                        case CurrencyType.Gold:
                            tmpCurrency = currencyData.Gold + int.Parse(price.ToString()) < maxValue ? currencyData.Gold + int.Parse(price.ToString()) : currencyData.Gold;
                            break;
                        case CurrencyType.Bonus:
                            tmpCurrency = currencyData.Bonus + int.Parse(price.ToString()) < maxValue ? currencyData.Bonus + int.Parse(price.ToString()) : currencyData.Bonus;
                            break;
                    }

                    purchaseData = purchaseIO.UpdatePurchaseLog(account, purchaseID, currencyType, tmpCurrency.ToString(), currencyCode, currencyValue.ToString(), receiptCipheredPayload, receipt, description);

                    currencyData.ReturnCode = purchaseData.ReturnCode;
                    currencyData.ReturnMessage = "LOG失敗: " + purchaseData.ReturnCode + " " + purchaseData.ReturnMessage;

                    if (purchaseData.ReturnCode == "S1103")
                    {
                        currencyData.ReturnCode = purchaseData.ReturnCode;
                        currencyData.ReturnMessage = "紀錄購買法幣商品成功！";
                    }

                    tmpCurrency = tmpCurrency + int.Parse(nestedProductData["Price"].ToString());
                    currencyData = currencyIO.UpdateCurrency(account, tmpCurrency.ToString(), currencyType);

                    if (currencyData.ReturnCode == "S703")
                    {
                        currencyData.ReturnMessage = currencyData.ReturnMessage + "          value:" + value + "  bool:" + onSell + " dictPurchaseData Count:" + dictPurchaseData.Count + "purchaseData.jPurchaseData:" + "  tmpCurrency:" + tmpCurrency + "  price:" + price;
                    }
                    else
                    {
                        currencyData.ReturnMessage += "  currencyData.ReturnMessage: " + currencyData.ReturnMessage + "  value:" + value + "  bool:" + onSell + " dictPurchaseData Count:" + dictPurchaseData.Count + "  tmpCurrency:" + tmpCurrency + "  price:" + price;
                    }

                }
                else
                {
                    currencyData.ReturnCode = "S1104";
                    currencyData.ReturnMessage = "購買法幣商品失敗！" + "  value:" + value + "  bool:" + onSell + " dictPurchaseData Count:" + dictPurchaseData.Count + "purchaseData.jPurchaseData:" + purchaseData.jPurchaseData + "  tmpCurrency:" + tmpCurrency + "  price:" + price;
                }
            }
            else
            {
                currencyData.ReturnCode = "S1105";
                currencyData.ReturnMessage = "購買法幣商品失敗，商品已下架！" + "  value:" + value + "  bool:" + onSell + " dictPurchaseData Count:" + dictPurchaseData.Count + "purchaseData.jPurchaseData:" + purchaseData.jPurchaseData + "  tmpCurrency:" + tmpCurrency + "  price:" + price;
            }
            //}
            //catch (Exception e)
            //{
            //    throw e;
            //}

            return currencyData;
        }
        #endregion
    }
}
