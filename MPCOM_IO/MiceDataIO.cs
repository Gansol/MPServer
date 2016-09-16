using ExitGames.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.EnterpriseServices;
using MiniJSON;
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
 * 這個檔案是用來進行資料庫讀寫 老鼠資料所使用
 * 載入老鼠資料
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
    public class MiceDataIO : ServicedComponent
    {
        static string host = "localhost\\SQLEXPRESS";           // 主機位置 IP(本機)\\伺服器名稱
        static string id = "Krola";                             // SQL Server帳號
        static string pwd = "1234";                             // SQL Server密碼
        static string database = "MicePowDB";                   // 資料庫名稱
        string connectionString = string.Format("Server = {0};Database = {1};User ID = {2};Password = {3};", host, database, id, pwd);

        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        protected override bool CanBePooled()
        {
            return true;
        }

        #region LoadMiceData 載入老鼠資料
        [AutoComplete]
        public MiceData LoadMiceData()
        {
            MiceData miceData = new MiceData();
            miceData.ReturnCode = "S800";
            miceData.ReturnMessage = "";
            DataSet DS = new DataSet();

            try
            {
                // 把引號'變成''以防止隱碼攻擊
                //Account = Account.Replace("'", "''");
                //Password = Password.Replace("'", "''");

                using (SqlConnection sqlConn = new SqlConnection(connectionString))
                {

                    SqlCommand sqlCmd = new SqlCommand();
                    sqlCmd.Connection = sqlConn;
                    sqlConn.Open();

                    Log.Debug("連線資訊 :" + sqlConn.ToString());

                    // 讀取老鼠資料 寫入DS資料列
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand("SELECT * FROM Monster_MiceData", sqlConn);
                    adapter.Fill(DS);
                }
                // 若有讀到則 取得所有資料
                if (DS.Tables[0].Rows.Count > 0)
                {
                    int i = 0, j = 0;

                    foreach (DataTable table in DS.Tables)
                    {
                        int arrayX = table.Rows.Count;
                        int arrayY = table.Columns.Count; // -1因為減去索引鍵值
                        string[,] sqlData = new string[arrayX, arrayY];
                        Dictionary<int, object> dictData = new Dictionary<int, object>();
                        
                        
                        foreach (DataRow row in table.Rows)
                        {
                            j = 0;
                            Dictionary<int, object> dictData2 = new Dictionary<int, object>();
                            foreach (DataColumn col in table.Columns)
                            {
                                    sqlData[i, j ] = table.Rows[i][col].ToString();  // 0是索引值
                                    dictData2.Add(j, table.Rows[i][col].ToString());
                                    Log.Debug("Mice Data: " + table.Rows[i][col].ToString());
                                j++;
                            }
                            dictData.Add(i, dictData2);
                            i++;
                        }
                        miceData.miceProperty = Json.Serialize(dictData);
                    }
                    miceData.ReturnCode = "S801"; //true
                }
                else
                {
                    miceData.ReturnCode = "S802";
                    miceData.ReturnMessage = "取得老鼠資料失敗！";
                }
            }
            catch (Exception ｅ)
            {
                miceData.ReturnCode = "S899";
                miceData.ReturnMessage = "載入老鼠資料例外情況！";
                throw ｅ;
            }

            return miceData; //回傳資料
        }
        #endregion
    }
}