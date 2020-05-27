using ExitGames.Logging;
using System;
using System.Data;
using System.Data.SqlClient;
using System.EnterpriseServices;
using MPProtocol;
using System.Collections.Generic;
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
    public class GashaponIO : ServicedComponent                 // ServicedComponent 表示所有使用 COM+ 服務之類別的基底類別。
    {
        static string host = "localhost\\MPSQLSERVER";           //主機位置 IP(本機)\\伺服器名稱
        static string id = "Krola";                             // SQL Server帳號
        static string pwd = "1234";                             // SQL Server密碼
        static string database = "MicePowDB";                   // 資料庫名稱
        string connectionString = string.Format("Server = {0};Database = {1};User ID = {2};Password = {3};", host, database, id, pwd); //格式化連線字串

        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();   // LOG

        protected override bool CanBePooled()                                       //覆寫 可以被放入集區 
        {
            return true;
        }

        #region LoadGashapon 載入轉蛋資料
        /// <summary>
        /// 載入貨幣資料
        /// </summary>
        /// <param name="Account"></param>
        /// <returns></returns>
        [AutoComplete]                                          //如果方法呼叫正常傳回的話，交易會自動呼叫 SetComplete。如果方法呼叫擲回例外狀況，則交易中止。
        public GashaponData[] LoadGashaponData(Int16 series)
        {

            GashaponData[] gashaponData = new GashaponData[1]; ;
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

                    string selectString = string.Format( "SELECT * FROM Gasnapon_ItemChance LEFT JOIN Gashapon_SeriesChance ON Gasnapon_ItemChance.Series = Gashapon_SeriesChance.Series where Gasnapon_ItemChance.Series='{0}'",series);
                    
                    if(series == (int)ENUM_GashaponSeries.AllSeries)
                        selectString = "SELECT * FROM Gasnapon_ItemChance LEFT JOIN Gashapon_SeriesChance ON Gasnapon_ItemChance.Series = Gashapon_SeriesChance.Series";

                    Log.Debug("連線資訊 :" + sqlConn.ToString());

                    // 讀取會員資料 並填入DS資料列
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(selectString, sqlConn);
                    adapter.Fill(DS);

                    if (DS.Tables[0].Rows.Count > 0)
                    {
                        int i = 0;
                        gashaponData = new GashaponData[DS.Tables[0].Rows.Count];
                        gashaponData[0].ReturnCode = "S1200";                   // 貨幣資料 預設值
                        gashaponData[0].ReturnMessage = "Gashapon Row Count:" + DS.Tables[0].Rows.Count;

                        foreach (DataTable table in DS.Tables)
                        {
                            //Dictionary<string, object> dictData = new Dictionary<string, object>();
                            //string itemID = "";
                            
                            foreach (DataRow row in table.Rows)
                            {
                                gashaponData[i].Series = Convert.ToInt16(table.Rows[i]["Series"]);
                                gashaponData[i].Grade= Convert.ToByte(table.Rows[i]["Grade"]);
                                gashaponData[i].ItemID = Convert.ToInt32(table.Rows[i]["ItemID"]);
                                gashaponData[i].ItemName = Convert.ToString(table.Rows[i]["ItemName"]);
                                gashaponData[i].Chance = Convert.ToInt32(table.Rows[i]["Chance"]);
                                //j = 0;
                                //Dictionary<string, object> dictData2 = new Dictionary<string, object>();
                                //foreach (DataColumn col in table.Columns)
                                //{
                                //    if (j == 0) itemID = table.Rows[i][col].ToString();
                                //    dictData2.Add(col.ColumnName, table.Rows[i][col].ToString());
                                //    j++;

                                   

                                //    //   Log.Debug(dictData2[col.ColumnName]);
                                //}

                                //dictData.Add(itemID, dictData2);
                                ////  Log.Debug(dictData[itemID]);
                                i++;
                            }
                            //gashaponData.StoreItem = Json.Serialize(dictData);
                        }
                        gashaponData[0].ReturnCode = "S1201"; //true
                        gashaponData[0].ReturnMessage = "取得轉蛋商品資料成功！";
                    }
                    else
                    {
                        gashaponData[0].ReturnCode = "1202";
                        gashaponData[0].ReturnMessage = "取得轉蛋商品資料失敗！";
                    }
                }
            }
            catch (Exception e)
            {
                Log.Debug("(IO)LoadCrrency failed!" + e.Message + " Track: " + e.StackTrace);
                gashaponData[0].ReturnCode = "S1299";
                gashaponData[0].ReturnMessage = "取得轉蛋商品資料未知錯誤！";
                throw e;
            }

            return gashaponData;    //回傳資料
        }
        #endregion

        #region LoadGashaponSeriesChance 載入轉蛋機率資料
        /// <summary>
        /// 載入貨幣資料
        /// </summary>
        /// <param name="Account"></param>
        /// <returns></returns>
        [AutoComplete]                                          //如果方法呼叫正常傳回的話，交易會自動呼叫 SetComplete。如果方法呼叫擲回例外狀況，則交易中止。
        public GashaponData[] LoadGashaponSeriesChance()
        {

            GashaponData[] gashaponData = new GashaponData[1]; ;
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
                    adapter.SelectCommand = new SqlCommand("SELECT * FROM Gashapon_SeriesChance", sqlConn);
                    adapter.Fill(DS);

                    if (DS.Tables[0].Rows.Count > 0)
                    {
                        int i = 0;
                        gashaponData = new GashaponData[DS.Tables[0].Rows.Count];
                        gashaponData[0].ReturnCode = "S1200";                   // 貨幣資料 預設值
                        gashaponData[0].ReturnMessage = "Gashapon Series Row Count:" + DS.Tables[0].Rows.Count;

                        foreach (DataTable table in DS.Tables)
                        {
                            //Dictionary<string, object> dictData = new Dictionary<string, object>();
                            //string itemID = "";
                            
                            foreach (DataRow row in table.Rows)
                            {
                                gashaponData[i].Series = Convert.ToInt16(table.Rows[i]["Series"]);
                                gashaponData[i].SSR_Chance = Convert.ToInt32(table.Rows[i]["SSR_Chance"]);
                                gashaponData[i].SR_Chance = Convert.ToInt32(table.Rows[i]["SR_Chance"]);
                                gashaponData[i].R_Chance = Convert.ToInt32(table.Rows[i]["R_Chance"]);
                                gashaponData[i].N_Chance = Convert.ToInt32(table.Rows[i]["N_Chance"]);
                                gashaponData[i].SSR_Blance = Convert.ToInt32(table.Rows[i]["SSR_Blance"]);
                                gashaponData[i].SR_Blance = Convert.ToInt32(table.Rows[i]["SR_Blance"]);
                                gashaponData[i].R_Blance = Convert.ToInt32(table.Rows[i]["R_Blance"]);
                                gashaponData[i].N_Blance = Convert.ToInt32(table.Rows[i]["N_Blance"]);
                                gashaponData[i].Series_Blance = Convert.ToInt32(table.Rows[i]["Series_Blance"]);

                                //j = 0;
                                //Dictionary<string, object> dictData2 = new Dictionary<string, object>();
                                //foreach (DataColumn col in table.Columns)
                                //{
                                //    if (j == 0) itemID = table.Rows[i][col].ToString();
                                //    dictData2.Add(col.ColumnName, table.Rows[i][col].ToString());
                                //    j++;

                                   

                                //    //   Log.Debug(dictData2[col.ColumnName]);
                                //}

                                //dictData.Add(itemID, dictData2);
                                ////  Log.Debug(dictData[itemID]);
                                i++;
                            }
                            //gashaponData.StoreItem = Json.Serialize(dictData);
                        }
                        gashaponData[0].ReturnCode = "S1203"; //true
                        gashaponData[0].ReturnMessage = "取得轉蛋商品系列機率資料成功！";
                    }
                    else
                    {
                        gashaponData[0].ReturnCode = "1204";
                        gashaponData[0].ReturnMessage = "取得轉蛋商品系列機率資料失敗！";
                    }
                }
            }
            catch (Exception e)
            {
                Log.Debug("(IO)LoadCrrency failed!" + e.Message + " Track: " + e.StackTrace);
                gashaponData[0].ReturnCode = "S1299";
                gashaponData[0].ReturnMessage = "取得轉蛋商品系列機率資料未知錯誤！";
                throw e;
            }

            return gashaponData;    //回傳資料
        }
        #endregion
    }
}