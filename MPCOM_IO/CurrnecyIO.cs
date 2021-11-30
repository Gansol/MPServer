using ExitGames.Logging;
using System;
using System.Data;
using System.Data.SqlClient;
using System.EnterpriseServices;
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
    public class CurrencyIO : ServicedComponent                 // ServicedComponent 表示所有使用 COM+ 服務之類別的基底類別。
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
        
        #region LoadCurrency 載入貨幣資料
        /// <summary>
        /// 載入貨幣資料
        /// </summary>
        /// <param name="Account"></param>
        /// <returns></returns>
        [AutoComplete]                                          //如果方法呼叫正常傳回的話，交易會自動呼叫 SetComplete。如果方法呼叫擲回例外狀況，則交易中止。
        public CurrencyData LoadCurrency(string Account)
        {

            CurrencyData currencyData = new CurrencyData();
            currencyData.ReturnCode = "S700";                   // 貨幣資料 預設值
            currencyData.ReturnMessage = "";
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
                    adapter.SelectCommand = new SqlCommand(String.Format("SELECT * FROM Player_GameCurrency WHERE (Account='{0}')", Account), sqlConn);
                    adapter.Fill(DS);

                    // 若有讀到則取得所有資料
                    if (DS.Tables[0].Rows.Count > 0)
                    {
                        currencyData.Rice = Convert.ToInt32(DS.Tables[0].Rows[0]["Rice"]);
                        currencyData.Gold = Convert.ToInt16(DS.Tables[0].Rows[0]["Gold"]);

                        currencyData.ReturnCode = "S701"; //取得貨幣資料成功
                    }
                    else
                    {
                        currencyData.ReturnCode = "S702";
                        currencyData.ReturnMessage = "取得遊戲金幣失敗！";
                    }
                }
            }
            catch (Exception e)
            {
                Log.Debug("(IO)LoadCrrency failed!" + e.Message + " Track: " + e.StackTrace);
                currencyData.ReturnCode = "S799";
                currencyData.ReturnMessage = "更新遊戲金幣未知錯誤！";
                throw e;
            }

            return currencyData;    //回傳資料
        }
        #endregion

        //#region UpdateCurrency 更新遊戲幣資料
        ///// <summary>
        ///// 更新遊戲幣資料
        ///// </summary>
        ///// <param name="account"></param>
        ///// <param name="rice"></param>
        ///// <returns></returns>
        //[AutoComplete]
        //public CurrencyData UpdateCurrency(string account, int rice)  // gold = Int16
        //{
        //    CurrencyData currencyData = new CurrencyData();
        //    currencyData.ReturnCode = "(IO)S700";
        //    currencyData.ReturnMessage = "";
        //    DataSet DS = new DataSet();

        //    try
        //    {
        //        using (SqlConnection sqlConn = new SqlConnection(this.connectionString))
        //        {

        //            SqlCommand sqlCmd = new SqlCommand();
        //            sqlCmd.Connection = sqlConn;
        //            sqlConn.Open();

        //            SqlDataAdapter adapter = new SqlDataAdapter();
        //            adapter.SelectCommand = new SqlCommand(string.Format("SELECT Rice FROM Player_GameCurrency WHERE (Account='{0}') ", account), sqlConn);
        //            adapter.Fill(DS);

        //            if (DS.Tables[0].Rows.Count == 1)   // 如果找到玩家資料
        //            {
        //                string query = @"UPDATE Player_GameCurrency SET Rice=@rice WHERE Account=@account";
        //                SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
        //                command.Parameters.Clear();
        //                command.Parameters.AddWithValue("@account", account);
        //                command.Parameters.AddWithValue("@rice", rice);
        //                command.ExecuteNonQuery();

        //                currencyData.ReturnCode = "S703";
        //                currencyData.ReturnMessage = "更新遊戲金幣成功！";
        //            }
        //            else if (DS.Tables[0].Rows.Count == 0) // 如果沒有找到玩家資料
        //            {
        //                currencyData.ReturnCode = "S704";
        //                currencyData.ReturnMessage = "更新遊戲金幣失敗！";
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Debug("(IO)UpdateCrrency failed!" + e.Message + " Track: " + e.StackTrace);
        //        currencyData.ReturnCode = "S799";
        //        currencyData.ReturnMessage = "更新遊戲金幣未知錯誤！";
        //        throw e;
        //    }
        //    return currencyData;
        //}
        //#endregion

        #region UpdateCurrency 更新貨幣資料
        /// <summary>
        /// 更新貨幣資料
        /// </summary>
        /// <param name="account"></param>
        /// <param name="value">貨幣價值</param>
        /// <param name="currencyType">貨幣類別</param>
        /// <returns></returns>
        [AutoComplete]
        public CurrencyData UpdateCurrency(string account, string value, CurrencyType currencyType)  // gold = Int16
        {
            CurrencyData currencyData = new CurrencyData();
            currencyData.ReturnCode = "(IO)S700";
            currencyData.ReturnMessage = "";
            DataSet DS = new DataSet();


            try
            {
                using (SqlConnection sqlConn = new SqlConnection(this.connectionString))
                {

                    SqlCommand sqlCmd = new SqlCommand();
                    sqlCmd.Connection = sqlConn;
                    sqlConn.Open();

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(string.Format("SELECT Rice,Gold,Bonus FROM Player_GameCurrency WHERE (Account='{0}') ", account), sqlConn);
                    adapter.Fill(DS);


                    if (DS.Tables[0].Rows.Count == 1)   // 如果找到玩家資料
                    {
                        string currencyQuery = "";

                        if (currencyType == CurrencyType.Rice)
                            currencyQuery = "Rice";
                        if (currencyType == CurrencyType.Gold)
                            currencyQuery = "Gold";
                        if (currencyType == CurrencyType.Bonus)
                            currencyQuery = "Bonus";

                        string query = string.Format(@"UPDATE Player_GameCurrency SET {0}=@value WHERE Account=@account", currencyQuery);

                        SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
                        command.Parameters.AddWithValue("@account", account);
                        command.Parameters.AddWithValue("@value", value);
                        command.ExecuteNonQuery();

                        if (currencyType == CurrencyType.Rice)
                            currencyData.ReturnMessage = "更新遊戲幣成功！" + "  currencyType:" + currencyType + "  value:" + value;

                        if (currencyType == CurrencyType.Gold)
                            currencyData.ReturnMessage = "更新遊戲金幣成功！" + "  currencyType:" + currencyType + "  value:" + value;

                        if (currencyType == CurrencyType.Bonus)
                            currencyData.ReturnMessage = "更新遊戲紅利成功！" + "  currencyType:" + currencyType + "  value:" + value;
                        currencyData.ReturnCode = "S703";
                    }
                    else if (DS.Tables[0].Rows.Count == 0) // 如果沒有找到玩家資料
                    {
                        currencyData.ReturnCode = "S704";
                        currencyData.ReturnMessage = "更新遊戲金幣失敗！";
                    }
                }
            }
            catch (Exception e)
            {
                Log.Debug("(IO)UpdateCrrency failed!" + e.Message + " Track: " + e.StackTrace);
                currencyData.ReturnCode = "S799";
                currencyData.ReturnMessage = "更新遊戲金幣未知錯誤！";
                throw e;
            }
            return currencyData;
        }
        #endregion

        //#region UpdateCurrency 更新貨幣資料
        ///// <summary>
        ///// 更新貨幣資料
        ///// </summary>
        ///// <param name="account"></param>
        ///// <param name="rice"></param>
        ///// <param name="gold"></param>
        ///// <returns></returns>
        //[AutoComplete]
        //public CurrencyData UpdateCurrency(string account, int rice, Int16 gold)  // gold = Int16
        //{
        //    CurrencyData currencyData = new CurrencyData();
        //    currencyData.ReturnCode = "(IO)S700";
        //    currencyData.ReturnMessage = "";
        //    DataSet DS = new DataSet();

        //    try
        //    {
        //        using (SqlConnection sqlConn = new SqlConnection(this.connectionString))
        //        {

        //            SqlCommand sqlCmd = new SqlCommand();
        //            sqlCmd.Connection = sqlConn;
        //            sqlConn.Open();

        //            SqlDataAdapter adapter = new SqlDataAdapter();
        //            adapter.SelectCommand = new SqlCommand(string.Format("SELECT * FROM Player_GameCurrency WHERE (Account='{0}') ", account), sqlConn);
        //            adapter.Fill(DS);

        //            if (DS.Tables[0].Rows.Count == 1)   // 如果找到玩家資料
        //            {
        //                string query = @"UPDATE Player_GameCurrency SET Rice=@rice,Gold=@gold WHERE Account=@account";
        //                SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
        //                command.Parameters.AddWithValue("@account", account);
        //                command.Parameters.AddWithValue("@rice", rice);
        //                command.Parameters.AddWithValue("@gold", gold);
        //                command.ExecuteNonQuery();

        //                currencyData.ReturnCode = "S703";
        //                currencyData.ReturnMessage = "更新遊戲貨幣成功！";
        //            }
        //            else if (DS.Tables[0].Rows.Count == 0) // 如果沒有找到玩家資料
        //            {
        //                currencyData.ReturnCode = "S704";
        //                currencyData.ReturnMessage = "更新遊戲金幣失敗！";
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Debug("(IO)UpdateCrrency failed!" + e.Message + " Track: " + e.StackTrace);
        //        currencyData.ReturnCode = "S799";
        //        currencyData.ReturnMessage = "更新遊戲金幣未知錯誤！";
        //        throw e;
        //    }
        //    return currencyData;
        //}
        //#endregion

    }
}