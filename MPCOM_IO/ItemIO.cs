using ExitGames.Logging;
using System;
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
        static string host = "localhost\\SQLEXPRESS";           // 主機位置 IP(本機)\\伺服器名稱
        static string id = "Krola";                             // SQL Server帳號
        static string pwd = "1234";                             // SQL Server密碼
        static string database = "MicePowDB";                   // 資料庫名稱
        string connectionString = string.Format("Server = {0};Database = {1};User ID = {2};Password = {3};", host, database, id, pwd);

        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        protected override bool CanBePooled()                   // 覆寫 可以被放入集區 
        {
            return true;
        }

        #region UpdatedItemBuyCount 更新道具購買總數
        /// <summary>
        /// 更新道具購買"總數"
        /// </summary>
        /// <param name="itemName">道具名稱</param>
        /// <param name="itemType">道具類別</param>
        /// <param name="buyCount">購買數量</param>
        /// <returns></returns>
        [AutoComplete]
        public ItemData UpdateItemBuyCount(string itemName, byte itemType, int buyCount)
        {
            ItemData itemData = default(ItemData);
            itemData.ReturnCode = "(IO)S600";
            itemData.ReturnMessage = "";
            DataSet DS = new DataSet();

            try
            {
                using (SqlConnection sqlConn = new SqlConnection(this.connectionString))
                {
                    SqlCommand sqlCmd = new SqlCommand();
                    sqlCmd.Connection = sqlConn;
                    sqlConn.Open();

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(string.Format("SELECT * FROM ItemData WHERE (ItemName='{0}') AND (ItemType='{1}') ", itemName, itemType), sqlConn);
                    adapter.Fill(DS);
                    Log.Debug("Tables Count: " + DS.Tables[0].Rows.Count);


                    //假如資料表中找到資料 更新資料
                    if (DS.Tables[0].Rows.Count == 1)
                    {
                        string query = @"UPDATE ItemData SET BuyCount=@buyCount WHERE ItemName=@itemName AND ItemType=@itemType";
                        SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@itemName", itemName);
                        command.Parameters.AddWithValue("@itemType", itemType);
                        command.Parameters.AddWithValue("@buyCount", buyCount);
                        command.ExecuteNonQuery();

                        itemData.ReturnCode = "S602";
                        itemData.ReturnMessage = "更新道具資料成功！";
                    }
                    else if (DS.Tables[0].Rows.Count == 0)
                    {
                        itemData.ReturnCode = "S603";
                        itemData.ReturnMessage = "無法取得更新道具資料！";
                    }
                }
            }
            catch (Exception e)
            {
                itemData.ReturnCode = "S699";
                itemData.ReturnMessage = "無法取得道具資訊，未知例外情況！";
                Log.Debug("(IO)UpdateItemData failed!" + e.Message + " Track: " + e.StackTrace);
                throw e;
            }
            return itemData;
        }
        #endregion

        #region UpdatedItemLimit 更新道具限量數量
        /// <summary>
        /// 更新道具"限量"數量
        /// </summary>
        /// <param name="itemName">道具名稱</param>
        /// <param name="itemType">道具類別</param>
        /// <param name="limit">限量數</param>
        /// <returns></returns>
        [AutoComplete]
        public ItemData UpdatedItemLimit(string itemName, byte itemType, Int16 limit)
        {
            ItemData itemData = default(ItemData);
            itemData.ReturnCode = "(IO)S600";
            itemData.ReturnMessage = "";
            DataSet DS = new DataSet();

            try
            {
                using (SqlConnection sqlConn = new SqlConnection(this.connectionString))
                {
                    SqlCommand sqlCmd = new SqlCommand();
                    sqlCmd.Connection = sqlConn;
                    sqlConn.Open();

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(string.Format("SELECT * FROM ItemData WHERE (ItemName='{0}') AND (ItemType='{1}') ", itemName, itemType), sqlConn);
                    adapter.Fill(DS);
                    Log.Debug("Tables Count: " + DS.Tables[0].Rows.Count);


                    //假如資料表中找到資料 更新資料
                    if (DS.Tables[0].Rows.Count == 1)
                    {
                        string query = @"UPDATE ItemData SET Limit=@limit";
                        SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@limit", limit);
                        command.ExecuteNonQuery();

                        itemData.ReturnCode = "S602";
                        itemData.ReturnMessage = "更新道具資料成功！";
                    }
                    else if (DS.Tables[0].Rows.Count == 0)
                    {
                        itemData.ReturnCode = "S603";
                        itemData.ReturnMessage = "無法取得更新道具資料！";
                    }
                }
            }
            catch (Exception e)
            {
                itemData.ReturnCode = "S699";
                itemData.ReturnMessage = "無法取得道具資訊，未知例外情況！";
                Log.Debug("(IO)UpdateItemData failed!" + e.Message + " Track: " + e.StackTrace);
                throw e;
            }
            return itemData;
        }
        #endregion

        #region GetItemData 取得道具資料
        /// <summary>
        /// 取得道具資料
        /// </summary>
        /// <param name="itemName">道具名稱</param>
        /// <param name="itemType">道具類別</param>
        /// <returns></returns>
        [AutoComplete]
        public ItemData GetItemData(string itemName, byte itemType)
        {
            ItemData itemData = default(ItemData);
            itemData.ReturnCode = "S600";
            itemData.ReturnMessage = "";
            DataSet DS = new DataSet();
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(this.connectionString))
                {
                    SqlCommand sqlCmd = new SqlCommand();
                    sqlCmd.Connection = sqlConn;
                    sqlConn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();

                    adapter.SelectCommand = new SqlCommand(string.Format("SELECT * FROM ItemData WHERE (ItemName='{0}') AND (ItemType='{1}') ", itemName, itemType), sqlConn);
                    adapter.Fill(DS);

                    //如果找到道具資料
                    if (DS.Tables[0].Rows.Count > 0)
                    {
                        itemData.ItemName = (string)DS.Tables[0].Rows[0]["ItemName"];
                        itemData.Price = Convert.ToInt32(DS.Tables[0].Rows[0]["Price"]);
                        itemData.ItemType = Convert.ToByte(DS.Tables[0].Rows[0]["ItemType"]);
                        itemData.Limit = Convert.ToInt16(DS.Tables[0].Rows[0]["Limit"]);
                        itemData.LimitTime = Convert.ToDateTime(DS.Tables[0].Rows[0]["LimitTime"]);
                        itemData.BuyCount = (int)DS.Tables[0].Rows[0]["BuyCount"];
                        itemData.ReturnCode = "S601";
                    }
                    else
                    {
                        itemData.ReturnCode = "S604";
                        itemData.ReturnMessage = "無法取得道具資訊！";
                    }
                }
            }
            catch (Exception e)
            {
                itemData.ReturnCode = "S699";
                itemData.ReturnMessage = "無法取得道具資訊，未知例外情況！";
                Log.Debug("(IO)GetItemData failed!" + e.Message + " Track: " + e.StackTrace);
                throw e;
            }
            return itemData;
        }
    }
        #endregion
}
