using ExitGames.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.EnterpriseServices;
using MiniJSON;
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
 * 這個檔案是用來進行資料庫讀寫 貨幣資料所使用
 * 載入貨幣資料、更新貨幣資料
 * >>IO須加強SQL安全性 加強後刪除
 * >>try catch 要移除 使用AutoComplete就可 移除後刪除
 * >>ILogger似乎沒用 要移除 移除後刪除
 * ***************************************************************/


namespace MPCOM
{
    // TransactionOption 指定元件要求的自動交易類型。
    // NotSupported	沒有使用支配性的交易在內容中建立元件。
    // Required	共用交易 (如果存在的話)，並且建立新交易 (如果有必要的話)。
    // RequiresNew	不論目前內容的狀態如何，都使用新交易建立元件。
    // Supported	共用交易 (如果有存在的話)。
    [Transaction(TransactionOption.Required)]
    public class PurchaseIO : ServicedComponent                 // ServicedComponent 表示所有使用 COM+ 服務之類別的基底類別。
    {
        static string host = "localhost"/*\\MPSQLSERVER"*/;           //主機位置 IP(本機)\\伺服器名稱
        static string id = "Gansol";                             // SQL Server帳號
        static string pwd = "1234";                             // SQL Server密碼
        static string database = "MicePowDB";                   // 資料庫名稱
        string connectionString = string.Format("Server = {0};Database = {1};User ID = {2};Password = {3};", host, database, id, pwd); //格式化連線字串

        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();   // LOG

        protected override bool CanBePooled()                                       //覆寫 可以被放入集區 
        {
            return true;
        }

        #region LoadPurchase 載入法幣商品資料
        [AutoComplete]                                          //如果方法呼叫正常傳回的話，交易會自動呼叫 SetComplete。如果方法呼叫擲回例外狀況，則交易中止。
        public PurchaseData LoadPurchase()
        {

            PurchaseData purchaseData = new PurchaseData();
            purchaseData.ReturnCode = "S1100";                   // 貨幣資料 預設值
            purchaseData.ReturnMessage = "";
            DataSet DS = new DataSet();                         // 儲存資料庫資料用 資料列


            try
            {
                // 把引號'變成''以防止隱碼攻擊
                //Account = Account.Replace("'", "''");
                //Password = Password.Replace("'", "''");

                using (SqlConnection sqlConn = new SqlConnection(connectionString))     // 建立連線 結束後自動關閉
                {

                    SqlCommand sqlCmd = new SqlCommand();
                    sqlCmd.Connection = sqlConn;
                    sqlConn.Open();                                                     // 開啟連線

                    Log.Debug("連線資訊 :" + sqlConn.ToString());

                    // 讀取會員資料 並填入DS資料列
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand("SELECT * FROM Purchase_ItemData", sqlConn);
                    adapter.Fill(DS);

                    // 若有讀到則取得所有資料
                    if (DS.Tables[0].Rows.Count > 0)
                    {
                        int i = 0, j = 0;
                        string itemID = "";

                        foreach (DataTable table in DS.Tables)
                        {
                            Dictionary<string, object> dictData = new Dictionary<string, object>();

                            foreach (DataRow row in table.Rows)
                            {
                                j = 0;
                                Dictionary<string, object> dictData2 = new Dictionary<string, object>();
                                foreach (DataColumn col in table.Columns)
                                {
                                    if (j == 0) itemID = table.Rows[i][col].ToString();
                                    if (col.ColumnName == "NewArrivalsTime" || col.ColumnName == "PromotionsTime")
                                        dictData2.Add(col.ColumnName, Convert.ToDateTime(table.Rows[i][col]).ToString("yyyy/MM/dd HH:mm:ss"));
                                    else
                                        dictData2.Add(col.ColumnName, table.Rows[i][col].ToString());
                                    j++;
                                }
                                dictData.Add(itemID, dictData2);
                                i++;
                            }
                            purchaseData.jPurchaseData = Json.Serialize(dictData);
                        }
                        purchaseData.ReturnCode = "S1101"; //true
                        purchaseData.ReturnMessage = "取得法幣道具資料成功！";
                    }
                    else
                    {
                        purchaseData.ReturnCode = "S1102";
                        purchaseData.ReturnMessage = "取得法幣道具資料失敗！";
                    }
                }
            }
            catch (Exception e)
            {
                Log.Debug("(IO)LoadPurchase failed!" + e.Message + " Track: " + e.StackTrace);
                purchaseData.ReturnCode = "S1199";
                purchaseData.ReturnMessage = "取得法幣道具未知錯誤！";
                throw e;
            }

            return purchaseData;    //回傳資料
        }
        #endregion

        #region UpdatePurchaseLog 紀錄法幣商品購買紀錄
        /// <summary>
        /// 紀錄法幣商品購買紀錄
        /// </summary>
        /// <param name="account"></param>
        /// <param name="purchaseID"></param>
        /// <param name="currencyType"></param>
        /// <param name="price"></param>
        /// <param name="currencyCode"></param>
        /// <param name="currencyValue"></param>
        /// <param name="receiptCipheredPayload"></param>
        /// <param name="receipt"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        [AutoComplete]
        public PurchaseData UpdatePurchaseLog(string account, string purchaseID, CurrencyType currencyType, string price, string currencyCode, string currencyValue, string receiptCipheredPayload, string receipt, string description)
        {
            PurchaseData purchaseData = new PurchaseData();
            purchaseData.ReturnCode = "(IO)S1100";
            purchaseData.ReturnMessage = "";
            DataSet DS = new DataSet();

            try
            {
                using (SqlConnection sqlConn = new SqlConnection(this.connectionString))
                {

                    SqlCommand sqlCmd = new SqlCommand();
                    sqlCmd.Connection = sqlConn;
                    sqlConn.Open();

                    if (sqlConn.State == System.Data.ConnectionState.Connecting)
                    {
                        //SqlDataAdapter adapter = new SqlDataAdapter();
                        //adapter.SelectCommand = new SqlCommand("SELECT * FROM Purchase_Log", sqlConn);
                        //adapter.Fill(DS);

                        //if (DS.Tables[0].Rows.Count > 0)   // 如果找到玩家資料
                        //{
                        //插入 會員資料
                        string query = "INSERT INTO Purchase_Log (Account,ItemName,CurrencyType,Price,CurrencyCode,CurrencyValue,ReceiptCipheredPayload,Receipt,Description) VALUES(@account,@itemName,@currencyType,@price,@currencyCode,@currencyValue,@receiptCipheredPayload,@receipt,@description)";

                        SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
                        command.Parameters.AddWithValue("@account", account);
                        command.Parameters.AddWithValue("@itemName", purchaseID);
                        command.Parameters.AddWithValue("@currencyType", currencyType);
                        command.Parameters.AddWithValue("@price", price);

                        command.Parameters.AddWithValue("@currencyCode", currencyCode);
                        command.Parameters.AddWithValue("@currencyValue", currencyValue);
                        command.Parameters.AddWithValue("@receiptCipheredPayload", receiptCipheredPayload);
                        command.Parameters.AddWithValue("@receipt", receipt);
                        command.Parameters.AddWithValue("@description", description);

                        int ExecuteNonQuery = command.ExecuteNonQuery();

                        purchaseData.ReturnCode = "S1103";
                        purchaseData.ReturnMessage = "新增購買法幣商品紀錄成功！";
                    }
                    else
                    {
                        //  Log.Debug("(IO)UpdateCrrency failed!");
                        purchaseData.ReturnCode = "S1199";
                        purchaseData.ReturnMessage = "新增購買法幣商品紀錄未知錯誤！(conn Error)";
                    }
                }
            }
            catch (Exception e)
            {
                Log.Debug("(IO)UpdateCrrency failed!" + e.Message + " Track: " + e.StackTrace);
                purchaseData.ReturnCode = "S1199";
                purchaseData.ReturnMessage = "新增購買法幣商品紀錄未知錯誤！";

                return purchaseData;
                throw e;
            }
            return purchaseData;
        }
        #endregion
    }
}