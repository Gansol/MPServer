using ExitGames.Logging;
using MiniJSON;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
 * 
 * 
 * ***************************************************************/

namespace MPCOM
{    // TransactionOption 指定元件要求的自動交易類型。
    // NotSupported	沒有使用支配性的交易在內容中建立元件。
    // Required	共用交易 (如果存在的話)，並且建立新交易 (如果有必要的話)。
    // RequiresNew	不論目前內容的狀態如何，都使用新交易建立元件。
    // Supported	共用交易 (如果有存在的話)。
    [Transaction(TransactionOption.Required)]                   // ServicedComponent 表示所有使用 COM+ 服務之類別的基底類別。
    public class ItemIO : ServicedComponent
    {
        static string host = "localhost\\MPSQLSERVER";           // 主機位置 IP(本機)\\伺服器名稱
        static string id = "Krola";                             // SQL Server帳號
        static string pwd = "1234";                             // SQL Server密碼
        static string database = "MicePowDB";                   // 資料庫名稱
        string connectionString = string.Format("Server = {0};Database = {1};User ID = {2};Password = {3};", host, database, id, pwd);

        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        protected override bool CanBePooled()                   // 覆寫 可以被放入集區 
        {
            return true;
        }

        // to do
        #region LoadItemData 載入道具資料
        [AutoComplete]
        public ItemData LoadItemData()
        {
            ItemData itemData = new ItemData();
            itemData.ReturnCode = "S600";
            itemData.ReturnMessage = "";
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
                    adapter.SelectCommand = new SqlCommand("SELECT * FROM Item_ItemData", sqlConn);
                    adapter.Fill(DS);
                }
                // 若有讀到則 取得所有資料
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
                                dictData2.Add(col.ColumnName, table.Rows[i][col].ToString());
                                j++;
                            }
                            dictData.Add(itemID, dictData2);
                            i++;
                        }
                        itemData.itemProperty = Json.Serialize(dictData);
                    }
                    itemData.ReturnCode = "S601"; //true
                    itemData.ReturnMessage = "取得道具資料成功！";
                }
                else
                {
                    itemData.ReturnCode = "S602";
                    itemData.ReturnMessage = "取得道具資料失敗！";
                }
            }
            catch (Exception ｅ)
            {
                itemData.ReturnCode = "S699";
                itemData.ReturnMessage = "載入道具資料例外情況！";
                throw ｅ;
            }

            return itemData; //回傳資料
        }
        #endregion

    }
        




}
