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
    [Transaction(TransactionOption.RequiresNew)]
    public class StoreDataIO : ServicedComponent
    {
        static string host = "localhost\\MPSQLSERVER";           // 主機位置 IP(本機)\\伺服器名稱
        static string id = "Krola";                             // SQL Server帳號
        static string pwd = "1234";                             // SQL Server密碼
        static string database = "MicePowDB";                   // 資料庫名稱
        string connectionString = string.Format("Server = {0};Database = {1};User ID = {2};Password = {3};", host, database, id, pwd);

        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        protected override bool CanBePooled()
        {
            return true;
        }



        #region LoadStoreData 載入單筆商店資料
        /// <summary>
        /// 載入單筆商店資料
        /// </summary>
        /// <param name="itemName"></param>
        /// <param name="itemType"></param>
        /// <returns></returns>
        [AutoComplete]
        public StoreData LoadStoreData(int itemID, byte itemType)
        {
            StoreData storeData = new StoreData();
            storeData.ReturnCode = "S900";
            storeData.ReturnMessage = "";
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
                    adapter.SelectCommand = new SqlCommand(string.Format("SELECT Price,PromotionsCount,BuyCount,CurrencyType FROM Store_ItemData WHERE (ItemID='{0}') ", itemID), sqlConn);
                    adapter.Fill(DS);
                }
                // 若有讀到則 取得所有資料
                if (DS.Tables[0].Rows.Count > 0)
                {
                    storeData.PromotionsCount = Convert.ToInt16(DS.Tables[0].Rows[0]["PromotionsCount"]);
                    storeData.BuyCount = Convert.ToInt16(DS.Tables[0].Rows[0]["BuyCount"]);
                    storeData.Price = Convert.ToInt16(DS.Tables[0].Rows[0]["Price"]);
                    storeData.Price = Convert.ToInt16(DS.Tables[0].Rows[0]["CurrencyType"]);

                    storeData.ReturnCode = "S901"; //true
                    storeData.ReturnMessage = "取得商店資料成功！";
                }
                else
                {
                    storeData.ReturnCode = "904";
                    storeData.ReturnMessage = "取得商店資料失敗！";
                }
            }
            catch (Exception ｅ)
            {
                storeData.ReturnCode = "S999";
                storeData.ReturnMessage = "載入商店資料例外情況！";
                throw ｅ;
            }

            return storeData; //回傳資料
        }
        #endregion

        #region LoadStoreData 載入全部商店資料
        /// <summary>
        /// 載入全部商店資料
        /// </summary>
        /// <returns></returns>
        [AutoComplete]
        public StoreData LoadStoreData()
        {
            StoreData storeData = new StoreData();
            storeData.ReturnCode = "S900";
            storeData.ReturnMessage = "";
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
                    adapter.SelectCommand = new SqlCommand("SELECT * FROM Store_ItemData", sqlConn);
                    adapter.Fill(DS);
                }
                // 若有讀到則 取得所有資料
                if (DS.Tables[0].Rows.Count > 0)
                {
                    int i = 0, j = 0;

                    foreach (DataTable table in DS.Tables)
                    {
                        Dictionary<string, object> dictData = new Dictionary<string, object>();
                        string itemID = "";

                        foreach (DataRow row in table.Rows)
                        {
                            j = 0;
                            Dictionary<string, object> dictData2 = new Dictionary<string, object>();
                            foreach (DataColumn col in table.Columns)
                            {
                                if (j == 0) itemID = table.Rows[i][col].ToString();
                                dictData2.Add(col.ColumnName, table.Rows[i][col].ToString());
                                j++;

                             //   Log.Debug(dictData2[col.ColumnName]);
                            }

                            dictData.Add(itemID, dictData2);
                          //  Log.Debug(dictData[itemID]);
                            i++;
                        }
                        storeData.StoreItem = Json.Serialize(dictData);
                    }
                    storeData.ReturnCode = "S901"; //true
                    storeData.ReturnMessage = "取得商店資料成功！";
                }
                else
                {
                    storeData.ReturnCode = "904";
                    storeData.ReturnMessage = "取得商店資料失敗！";
                }
            }
            catch (Exception ｅ)
            {
                storeData.ReturnCode = "S999";
                storeData.ReturnMessage = "載入商店資料例外情況！";
                throw ｅ;
            }

            return storeData; //回傳資料
        }
        #endregion


        #region UpdatedStoreBuyCount 更新道具購買總數
        /// <summary>
        /// 更新道具購買"總數"
        /// </summary>
        /// <param name="storeName">道具名稱</param>
        /// <param name="storeType">道具類別</param>
        /// <param name="buyCount">購買數量</param>
        /// <returns></returns>
        [AutoComplete]
        public StoreData UpdateStoreBuyCount(int itemID, byte itemType, int buyCount, Int16 promotionsCount)
        {
            StoreData storeData = default(StoreData);
            storeData.ReturnCode = "(IO)S900";
            storeData.ReturnMessage = "";
            DataSet DS = new DataSet();
            
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(this.connectionString))
                {
                    SqlCommand sqlCmd = new SqlCommand();
                    sqlCmd.Connection = sqlConn;
                    sqlConn.Open();

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(string.Format("SELECT ItemID FROM Store_ItemData WHERE (ItemID='{0}')  ", itemID), sqlConn);
                    adapter.Fill(DS);
                    Log.Debug("(StoreIO)Tables Count: " + DS.Tables[0].Rows.Count + itemID + itemType);


                    //假如資料表中找到資料 更新資料
                    if (DS.Tables[0].Rows.Count == 1)
                    {
                        string query = @"UPDATE Store_ItemData SET BuyCount=@buyCount,PromotionsCount=@promotionsCount WHERE ItemID=@itemID";
                        SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@itemID", itemID);
                        command.Parameters.AddWithValue("@buyCount", buyCount);
                        command.Parameters.AddWithValue("@promotionsCount", promotionsCount);
                        command.ExecuteNonQuery();

                        storeData.ReturnCode = "S902";
                        storeData.ReturnMessage = "更新商店資料成功！";
                    }
                    else if (DS.Tables[0].Rows.Count == 0)
                    {
                        storeData.ReturnCode = "S904";
                        storeData.ReturnMessage = "取得商店資料失敗！";
                    }
                }
            }
            catch (Exception e)
            {
                storeData.ReturnCode = "S999";
                storeData.ReturnMessage = "無法取得商店資訊，未知例外情況！";
                Log.Debug("(IO)UpdateStoreData failed!" + e.Message + " Track: " + e.StackTrace);
                throw e;
            }
            return storeData;
        }
        #endregion

        #region UpdatedStoreLimit 更新道具限量數量
        /// <summary>
        /// 更新道具"限量"數量
        /// </summary>
        /// <param name="storeName">道具名稱</param>
        /// <param name="storeType">道具類別</param>
        /// <param name="limit">限量數</param>
        /// <returns></returns>
        [AutoComplete]
        public StoreData UpdatedStorePromotionsCount(int itemID, byte itemType, Int16 promotionsCount)
        {
            StoreData storeData = default(StoreData);
            storeData.ReturnCode = "(IO)S900";
            storeData.ReturnMessage = "";
            DataSet DS = new DataSet();

            try
            {
                using (SqlConnection sqlConn = new SqlConnection(this.connectionString))
                {
                    SqlCommand sqlCmd = new SqlCommand();
                    sqlCmd.Connection = sqlConn;
                    sqlConn.Open();

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(string.Format("SELECT ItemID FROM Store_ItemData WHERE (ItemID='{0}')  ", itemID), sqlConn);
                    adapter.Fill(DS);
                    Log.Debug("Tables Count: " + DS.Tables[0].Rows.Count);


                    //假如資料表中找到資料 更新資料
                    if (DS.Tables[0].Rows.Count == 1)
                    {
                        string query = @"UPDATE Store_ItemData SET PromotionsCount=@promotionsCount WHERE ItemID=@itemID";
                        SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@ItemID", itemID);
                        command.Parameters.AddWithValue("@promotionsCount", promotionsCount);
                        command.ExecuteNonQuery();

                        storeData.ReturnCode = "S902";
                        storeData.ReturnMessage = "更新商店資料成功！";
                    }
                    else if (DS.Tables[0].Rows.Count == 0)
                    {
                        storeData.ReturnCode = "S904";
                        storeData.ReturnMessage = "取得商店資料失敗！";
                    }
                }
            }
            catch (Exception e)
            {
                storeData.ReturnCode = "S999";
                storeData.ReturnMessage = "無法取得商店資訊，未知例外情況！";
                Log.Debug("(IO)UpdateStoreData failed!" + e.Message + " Track: " + e.StackTrace);
                throw e;
            }
            return storeData;
        }
        #endregion

    }
}