﻿using ExitGames.Logging;
using System;
using System.Data;
using System.Data.SqlClient;
using System.EnterpriseServices;
using System.Collections.Generic;
using MiniJSON;
using MPProtocol;
using System.Linq;
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
 * 這個檔案是用來進行資料庫讀寫 玩家資料所使用
 * 載入玩家資料、更新玩家資料
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
    public class PlayerDataIO : ServicedComponent
    {
        static string host = "localhost\\SQLEXPRESS01";               // 主機位置 IP(本機)\\伺服器名稱
        static string id = "Krola";                                 // SQL Server帳號
        static string pwd = "1234";                                 // SQL Server密碼
        static string database = "MicePowDB";                       // 資料庫名稱
        string connectionString = string.Format("Server = {0};Database = {1};User ID = {2};Password = {3};", host, database, id, pwd);

        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        protected override bool CanBePooled()
        {
            return true;
        }

        #region LoadPlayerData 載入玩家資料

        [AutoComplete]
        public PlayerData LoadPlayerData(string Account)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "S400";             //預設值
            playerData.ReturnMessage = "";
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

                   // Log.Debug("連線資訊 :" + sqlConn.ToString());

                    // 讀取玩家資料 填入DS資料列
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(String.Format("SELECT * FROM Player_PlayerData WITH(NOLOCK) WHERE (Account='{0}')", Account), sqlConn);
                    adapter.Fill(DS);

                    // 若有讀到則取得所有資料
                    if (DS.Tables[0].Rows.Count > 0)
                    {
                        playerData.Rank = Convert.ToByte(DS.Tables[0].Rows[0]["Rank"]);
                        playerData.Exp = Convert.ToInt16(DS.Tables[0].Rows[0]["EXP"]);
                        playerData.MaxCombo = Convert.ToInt16(DS.Tables[0].Rows[0]["MaxCombo"]);
                        playerData.MaxScore = Convert.ToInt32(DS.Tables[0].Rows[0]["MaxScore"]);
                        playerData.SumScore = Convert.ToInt32(DS.Tables[0].Rows[0]["SumScore"]);
                        playerData.SumLost = Convert.ToInt32(DS.Tables[0].Rows[0]["SumLost"]);
                        playerData.SumKill = Convert.ToInt32(DS.Tables[0].Rows[0]["SumKill"]);
                        playerData.SumWin = Convert.ToInt32(DS.Tables[0].Rows[0]["SumWin"]);
                        playerData.SumBattle = Convert.ToInt32(DS.Tables[0].Rows[0]["SumBattle"]);
                        playerData.SortedItem = Convert.ToString(DS.Tables[0].Rows[0]["Item"]);
                        playerData.MiceAll = Convert.ToString(DS.Tables[0].Rows[0]["MiceAll"]);
                        playerData.Team = Convert.ToString(DS.Tables[0].Rows[0]["Team"]);
                        playerData.Friends = Convert.ToString(DS.Tables[0].Rows[0]["Friend"]);
                        playerData.PlayerImage = Convert.ToString(DS.Tables[0].Rows[0]["Image"]);

                        playerData.ReturnCode = "S401"; //true
                        playerData.ReturnMessage = "取得玩家資料成功！";
                        //Log.Debug("IN DataIO playerData.Friend" + playerData.Friend);
                    }
                    else
                    {
                        playerData.ReturnCode = "S402";
                        playerData.ReturnMessage = "取得玩家資料失敗！";
                    }
                }
            }
            catch (Exception e)
            {
                playerData.ReturnCode = "S499";
                playerData.ReturnMessage = "取得玩家資料例外情況！";
                Log.Debug("Load PlayerData Failed ! " + e.Message + " 於: " + e.StackTrace);
                throw e;
            }

            return playerData;
        }
        #endregion

        #region LoadPlayerItem 載入玩家道具資料
        /// <summary>
        /// 載入玩家道具資料
        /// </summary>
        /// <returns>PlayerData</returns>
        [AutoComplete]
        public PlayerData LoadPlayerItem(string account)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "S400";
            playerData.ReturnMessage = "";
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

                   // Log.Debug("連線資訊 :" + sqlConn.ToString());

                    // 讀取老鼠資料 寫入DS資料列
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    //adapter.SelectCommand = new SqlCommand(String.Format("SELECT * FROM Player_PlayerItem WHERE Account IN (SELECT Account ='{0}' FROM Player_PlayerItem GROUP BY Account HAVING COUNT(Account) > 1)", account), sqlConn);
                    adapter.SelectCommand = new SqlCommand(String.Format("SELECT ItemID,ItemCount,ItemType,IsEquip,UseCount,MaxRank,Rank,Exp FROM Player_PlayerItem WITH(NOLOCK) WHERE Account='{0}' ORDER BY ItemID ASC", account), sqlConn);
                    adapter.Fill(DS);
                }
                // 若有讀到則 取得所有資料
                if (DS.Tables[0].Rows.Count > 0)
                {
                    int i = 0, j = 0;
                    foreach (DataTable table in DS.Tables)
                    {
                        string itemID = "";
                        Dictionary<string, object> dictData = new Dictionary<string, object>();

                        foreach (DataRow row in table.Rows)
                        {
                            j = 0;
                            Dictionary<string, object> dictData2 = new Dictionary<string, object>();
                            foreach (DataColumn col in table.Columns)
                            {
                                if (j == 0) itemID = table.Rows[i][col].ToString();// 0 = itemID

                                dictData2.Add(col.ColumnName, table.Rows[i][col].ToString());
                                j++;
                            }
                            dictData.Add(itemID, dictData2);
                            i++;
                        }
                        playerData.PlayerItem = Json.Serialize(dictData);
                    }

                    playerData.ReturnCode = "S425"; //true
                    playerData.ReturnMessage = "取得玩家道具資料成功！";
                }
                else
                {
                    playerData.ReturnCode = "S426";
                    playerData.ReturnMessage = "取得玩家道具資料失敗！";
                }
            }
            catch (Exception ｅ)
            {
                playerData.ReturnCode = "S499";
                playerData.ReturnMessage = "載入玩家道具資料例外情況！";
                throw ｅ;
            }

            return playerData; //回傳資料
        }
        #endregion

        #region LoadPlayerItem 載入玩家道具(單筆)資料
        /// <summary>
        /// 載入玩家道具(單筆)資料
        /// </summary>
        /// <returns>PlayerData</returns>
        [AutoComplete]
        public PlayerData LoadPlayerItem(string account, Int16 itemID)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "S400";             //預設值
            playerData.ReturnMessage = "";
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

                   // Log.Debug("(PlayerIO)連線資訊 :" + sqlConn.ToString());

                    // 讀取玩家資料 填入DS資料列
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(String.Format("SELECT ItemCount,ItemType FROM Player_PlayerItem WITH(NOLOCK) WHERE (Account='{0}') AND (ItemID='{1}')", account, itemID), sqlConn);
                    adapter.Fill(DS);

                    // 若有讀到則取得所有資料
                    if (DS.Tables[0].Rows.Count > 0)
                    {
                        playerData.ItemID = itemID;
                        playerData.ItemCount = Convert.ToInt16(DS.Tables[0].Rows[0]["ItemCount"]);
                        playerData.ItemType = Convert.ToByte(DS.Tables[0].Rows[0]["ItemType"]);
                       // Log.Debug(playerData.ReturnMessage + "playerData.ItemCount:" + playerData.ItemCount);
                        playerData.ReturnCode = "S425"; //true
                        playerData.ReturnMessage = "取得玩家道具資料成功！";
                    }
                    else
                    {
                        playerData.ReturnCode = "S426";
                        playerData.ReturnMessage = "取得玩家道具資料失敗！";
                    }
                }
            }
            catch (Exception e)
            {
                playerData.ReturnCode = "S499";
                playerData.ReturnMessage = "取得玩家資料例外情況！";
                Log.Debug("Load PlayerData Failed ! " + e.Message + " 於: " + e.StackTrace);
                throw e;
            }

            return playerData;
        }
        #endregion

        #region LoadPlayerData 載入玩家特定欄位資料
        /// <summary>
        /// 載入玩家特定欄位資料
        /// </summary>
        /// <param name="account"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        [AutoComplete]
        public PlayerData LoadPlayerData(string account, List<string> columns)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "S400";             //預設值
            playerData.ReturnMessage = "";
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

                    //Log.Debug("(PlayerIO)連線資訊 :" + sqlConn.ToString());

                    // 讀取玩家資料 填入DS資料列
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    char[] charsToTrim = new char[] { ',' };
                    string selectString = "SELECT ";

                    foreach (string col in columns)
                    {
                        selectString += col + ",";
                    }

                    selectString = selectString.TrimEnd(charsToTrim) + String.Format("  FROM Player_PlayerData WITH(NOLOCK) WHERE Account='{0}'", account);

                    Log.Debug("PlayerDataIO SelectString:" + selectString);
                    adapter.SelectCommand = new SqlCommand(selectString, sqlConn);
                    adapter.Fill(DS);

                    // 若有讀到則取得所有資料
                    if (DS.Tables[0].Rows.Count > 0)
                    {
                       // Log.Debug("columns.Contains(Friend):" + columns.Contains("Friend"));
                        if (columns.Contains("PrimaryID")) playerData.PrimaryID = Convert.ToInt32(DS.Tables[0].Rows[0]["PrimaryID"]);
                        if (columns.Contains("Account")) playerData.Account = Convert.ToString(DS.Tables[0].Rows[0]["Account"]);
                        if (columns.Contains("Rank")) playerData.Rank = Convert.ToByte(DS.Tables[0].Rows[0]["Rank"]);
                        if (columns.Contains("EXP")) playerData.Exp = Convert.ToInt16(DS.Tables[0].Rows[0]["EXP"]);
                        if (columns.Contains("MaxCombo")) playerData.MaxCombo = Convert.ToInt16(DS.Tables[0].Rows[0]["MaxCombo"]);
                        if (columns.Contains("MaxScore")) playerData.MaxScore = Convert.ToInt32(DS.Tables[0].Rows[0]["MaxScore"]);
                        if (columns.Contains("SumScore")) playerData.SumScore = Convert.ToInt32(DS.Tables[0].Rows[0]["SumScore"]);
                        if (columns.Contains("SumLost")) playerData.SumLost = Convert.ToInt32(DS.Tables[0].Rows[0]["SumLost"]);
                        if (columns.Contains("SumKill")) playerData.SumKill = Convert.ToInt32(DS.Tables[0].Rows[0]["SumKill"]);
                        if (columns.Contains("SumWin")) playerData.SumWin = Convert.ToInt32(DS.Tables[0].Rows[0]["SumWin"]);
                        if (columns.Contains("Item")) playerData.PlayerItem = Convert.ToString(DS.Tables[0].Rows[0]["Item"]);
                        if (columns.Contains("MiceAll")) playerData.MiceAll = Convert.ToString(DS.Tables[0].Rows[0]["MiceAll"]);
                        if (columns.Contains("Team")) playerData.Team = Convert.ToString(DS.Tables[0].Rows[0]["Team"]);
                        if (columns.Contains("Friend")) playerData.Friends = Convert.ToString(DS.Tables[0].Rows[0]["Friend"]);
                        if (columns.Contains("Image")) playerData.PlayerImage = Convert.ToString(DS.Tables[0].Rows[0]["Image"]);
                        if (columns.Contains("AccountLevel")) playerData.AccountLevel = Convert.ToByte(DS.Tables[0].Rows[0]["AccountLevel"]);

                        playerData.ReturnCode = "S436"; //true
                        playerData.ReturnMessage = "取得玩家特定資料成功！  " + selectString;
                    }
                    else
                    {
                        playerData.ReturnCode = "S437";
                        playerData.ReturnMessage = "取得玩家特定資料失敗！";
                    }
                }
                return playerData;
            }
            catch 
            {
                playerData.ReturnCode = "S499";
                playerData.ReturnMessage = "取得玩家特定資料例外情況！";
                Log.Debug("Load PlayerData Failed ! " + playerData.ReturnCode + playerData.ReturnMessage);
                throw ;
            }
        }
        #endregion

        #region UpdatePlayerData 更新玩家(全部)資料
        [AutoComplete]
        public PlayerData UpdatePlayerData(string account, byte rank, short exp, Int16 maxCombo, int maxScore, int sumScore, int sumLost, int sumKill, string item, string miceAll, string team, string friend)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "(IO)S400";
            playerData.ReturnMessage = "";
            DataSet DS = new DataSet();

            try
            {
                using (SqlConnection sqlConn = new SqlConnection(this.connectionString))
                {
                    SqlCommand sqlCmd = new SqlCommand();
                    sqlCmd.Connection = sqlConn;
                    sqlConn.Open();

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(string.Format("SELECT Account FROM Player_PlayerData WHERE (Account='{0}') ", account), sqlConn);
                    adapter.Fill(DS);

                    //Log.Debug("Tables Count: " + DS.Tables[0].Rows.Count);

                    // 如果找到玩家資料
                    if (DS.Tables[0].Rows.Count == 1)
                    {
                        string query = @"UPDATE Player_PlayerData WITH(ROWLOCK) SET Rank=@rank,EXP=@exp,MaxCombo=@maxCombo,MaxScore=@maxScore,SumScore=@sumScore,SumLost=@sumLost,SumKill=@sumKill,Item=@item,MiceAll=@miceAll,Team=@team,Friend=@friend WHERE Account=@account";
                        SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@account", account);
                        command.Parameters.AddWithValue("@rank", rank);
                        command.Parameters.AddWithValue("@exp", exp);
                        command.Parameters.AddWithValue("@maxCombo", maxCombo);
                        command.Parameters.AddWithValue("@maxScore", maxScore);
                        command.Parameters.AddWithValue("@sumScore", sumScore);
                        command.Parameters.AddWithValue("@sumLost", sumLost);
                        command.Parameters.AddWithValue("@sumKill", sumKill);
                        command.Parameters.AddWithValue("@item", item);
                        command.Parameters.AddWithValue("@miceAll", miceAll);
                        command.Parameters.AddWithValue("@team", team);
                        command.Parameters.AddWithValue("@friend", friend);
                        command.ExecuteNonQuery();

                        playerData.ReturnCode = "S403";
                        playerData.ReturnMessage = "更新玩家資料成功！";
                    }
                    else if (DS.Tables[0].Rows.Count == 0) // 如果沒有找到玩家資料
                    {
                        playerData.ReturnCode = "S404";
                        playerData.ReturnMessage = "更新玩家資料失敗！";
                    }
                }
            }
            catch (Exception e)
            {
                playerData.ReturnCode = "S499";
                playerData.ReturnMessage = "取得玩家資料例外情況！";
                Log.Debug("(IO)UpdatePlayerData failed! " + e.Message + " 於: " + e.StackTrace);
                throw e;
            }
            return playerData;
        }

        #endregion UpdatePlayerData

        #region UpdatePlayerData 更新玩家(單筆)好友資料
        /// <summary>
        /// 更新玩家(好友)資料
        /// </summary>
        /// <param name="account"></param>
        /// <param name="friends"></param>
        /// <returns></returns>
        [AutoComplete]
        public PlayerData UpdatePlayerData(string account, List<string> friends)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "(IO)S400";
            playerData.ReturnMessage = "";
            DataSet DS = new DataSet();

            try
            {
                using (SqlConnection sqlConn = new SqlConnection(this.connectionString))
                {
                    SqlCommand sqlCmd = new SqlCommand();
                    sqlCmd.Connection = sqlConn;
                    sqlConn.Open();

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(string.Format("SELECT Account FROM Player_PlayerData WHERE (Account='{0}') ", account), sqlConn);
                    adapter.Fill(DS);

                    //Log.Debug("Tables Count: " + DS.Tables[0].Rows.Count);

                    // 如果找到玩家資料
                    if (DS.Tables[0].Rows.Count == 1)
                    {
                        string friend = String.Join(",", friends.ToArray());
                        string query = @"UPDATE Player_PlayerData WITH(ROWLOCK) SET Friend=@friend WHERE Account=@account ";
                        SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@account", account);
                        command.Parameters.AddWithValue("@friend", friend);
                        command.ExecuteNonQuery();

                        playerData.ReturnCode = "S434";
                        playerData.ReturnMessage = "更新好友資料成功！";
                    }
                    else if (DS.Tables[0].Rows.Count == 0) // 如果沒有找到玩家資料
                    {
                        playerData.ReturnCode = "S435";
                        playerData.ReturnMessage = "更新好友資料失敗！";
                    }
                }
            }
            catch (Exception e)
            {
                playerData.ReturnCode = "S499";
                playerData.ReturnMessage = "更新好友資料例外情況！";
                Log.Debug("(IO)UpdatePlayerData failed! " + e.Message + " 於: " + e.StackTrace);
                throw e;
            }
            return playerData;
        }

        #endregion UpdatePlayerData

        #region UpdateGameOver 更新玩家(GameOver)資料
        [AutoComplete]
        public PlayerData UpdateGameOver(string account, byte rank, short exp, Int16 maxCombo, int maxScore, int sumScore, int sumLost, int sumKill, int sumWin, int sumBattle, string item)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "(IO)S400";
            playerData.ReturnMessage = "";
            DataSet DS = new DataSet();

            try
            {
                using (SqlConnection sqlConn = new SqlConnection(this.connectionString))
                {
                    SqlCommand sqlCmd = new SqlCommand();
                    sqlCmd.Connection = sqlConn;
                    sqlConn.Open();

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(string.Format("SELECT Account FROM Player_PlayerData WHERE (Account='{0}') ", account), sqlConn);
                    adapter.Fill(DS);

                    //Log.Debug("Tables Count: " + DS.Tables[0].Rows.Count);

                    // 如果找到玩家資料
                    if (DS.Tables[0].Rows.Count == 1)
                    {
                        string query =
                                @"UPDATE Player_PlayerData WITH(ROWLOCK) SET Rank=@rank,EXP=@exp,MaxCombo=@maxCombo,MaxScore=@maxScore,SumScore=@sumScore,
                                SumLost=@sumLost,SumKill=@sumKill,SumWin=@sumWin,SumBattle=@sumBattle,Item=@item WHERE Account=@account";
                        SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@account", account);
                        command.Parameters.AddWithValue("@rank", rank);
                        command.Parameters.AddWithValue("@exp", exp);
                        command.Parameters.AddWithValue("@maxCombo", maxCombo);
                        command.Parameters.AddWithValue("@maxScore", maxScore);
                        command.Parameters.AddWithValue("@sumScore", sumScore);
                        command.Parameters.AddWithValue("@sumLost", sumLost);
                        command.Parameters.AddWithValue("@sumKill", sumKill);
                        command.Parameters.AddWithValue("@sumWin", sumWin);
                        command.Parameters.AddWithValue("@sumBattle", sumBattle);
                        command.Parameters.AddWithValue("@item", item);
                        command.ExecuteNonQuery();

                        playerData.ReturnCode = "S403";
                        playerData.ReturnMessage = "更新玩家資料成功！";
                    }
                    else if (DS.Tables[0].Rows.Count == 0) // 如果沒有找到玩家資料
                    {
                        playerData.ReturnCode = "S404";
                        playerData.ReturnMessage = "更新玩家資料失敗！";
                    }
                }
            }
            catch (Exception e)
            {
                playerData.ReturnCode = "S499";
                playerData.ReturnMessage = "取得玩家資料例外情況！";
                Log.Debug("(IO)UpdatePlayerData failed! " + e.Message + " 於: " + e.StackTrace);
                throw e;
            }
            return playerData;
        }

        #endregion UpdatePlayerData

        #region UpdatePlayerData 更新玩家(圖片)資料
        [AutoComplete]
        public PlayerData UpdatePlayerData(string account, object imageName)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "(IO)S400";
            playerData.ReturnMessage = "";
            DataSet DS = new DataSet();

            try
            {
                using (SqlConnection sqlConn = new SqlConnection(this.connectionString))
                {
                    SqlCommand sqlCmd = new SqlCommand();
                    sqlCmd.Connection = sqlConn;
                    sqlConn.Open();

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(string.Format("SELECT Account FROM Player_PlayerData WHERE (Account='{0}') ", account), sqlConn);
                    adapter.Fill(DS);

                    //Log.Debug("Tables Count: " + DS.Tables[0].Rows.Count);

                    // 如果找到玩家資料
                    if (DS.Tables[0].Rows.Count == 1)
                    {
                        string query = @"UPDATE Player_PlayerData SET Image=@imageName WHERE Account=@account";
                        SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@account", account);
                        command.Parameters.AddWithValue("@imageName", imageName);
                        command.ExecuteNonQuery();

                        playerData.ReturnCode = "S430";
                        playerData.ReturnMessage = "更新圖片資料成功！";
                    }
                    else if (DS.Tables[0].Rows.Count == 0) // 如果沒有找到玩家資料
                    {
                        playerData.ReturnCode = "S431";
                        playerData.ReturnMessage = "更新圖片資料失敗！";
                    }
                }
            }
            catch (Exception e)
            {
                playerData.ReturnCode = "S499";
                playerData.ReturnMessage = "取得玩家資料例外情況！";
                Log.Debug("(IO)UpdatePlayerData failed! " + e.Message + " 於: " + e.StackTrace);
                throw e;
            }
            return playerData;
        }

        #endregion


        #region UpdatePlayerData 更新玩家(Team)資料
        [AutoComplete]
        public PlayerData UpdatePlayerData(string account, string miceAll, string team)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "(IO)S400";
            playerData.ReturnMessage = "";
            DataSet DS = new DataSet();

            try
            {
                using (SqlConnection sqlConn = new SqlConnection(this.connectionString))
                {
                    SqlCommand sqlCmd = new SqlCommand();
                    sqlCmd.Connection = sqlConn;
                    sqlConn.Open();

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(string.Format("SELECT Account FROM Player_PlayerData WHERE (Account='{0}') ", account), sqlConn);
                    adapter.Fill(DS);

                    //Log.Debug("Tables Count: " + DS.Tables[0].Rows.Count);

                    // 如果找到玩家資料
                    if (DS.Tables[0].Rows.Count == 1)
                    {
                        string query = @"UPDATE Player_PlayerData SET MiceAll=@miceAll,Team=@team WHERE Account=@account";
                        SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@account", account);
                        command.Parameters.AddWithValue("@miceAll", miceAll);
                        command.Parameters.AddWithValue("@team", team);
                        command.ExecuteNonQuery();

                        playerData.ReturnCode = "S420";
                        playerData.ReturnMessage = "更新老鼠資料成功！";
                    }
                    else if (DS.Tables[0].Rows.Count == 0) // 如果沒有找到玩家資料
                    {
                        playerData.ReturnCode = "S421";
                        playerData.ReturnMessage = "更新老鼠資料失敗！";
                    }
                }
            }
            catch (Exception e)
            {
                playerData.ReturnCode = "S499";
                playerData.ReturnMessage = "取得玩家資料例外情況！";
                Log.Debug("(IO)UpdatePlayerData failed! " + e.Message + " 於: " + e.StackTrace);
                throw e;
            }
            return playerData;
        }

        #endregion

        #region UpdatePlayerData 更新玩家(老鼠)資料
        [AutoComplete]
        public PlayerData UpdatePlayerData(string account, string miceAll)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "(IO)S400";
            playerData.ReturnMessage = "";
            DataSet DS = new DataSet();

            try
            {
                using (SqlConnection sqlConn = new SqlConnection(this.connectionString))
                {
                    SqlCommand sqlCmd = new SqlCommand();
                    sqlCmd.Connection = sqlConn;
                    sqlConn.Open();

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(string.Format("SELECT Account FROM Player_PlayerData WHERE (Account='{0}') ", account), sqlConn);
                    adapter.Fill(DS);

                    //Log.Debug("Tables Count: " + DS.Tables[0].Rows.Count);

                    // 如果找到玩家資料
                    if (DS.Tables[0].Rows.Count == 1)
                    {
                        string query = @"UPDATE Player_PlayerData WITH(ROWLOCK) SET MiceAll=@miceAll WHERE Account=@account";
                        SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@account", account);
                        command.Parameters.AddWithValue("@miceAll", miceAll);
                        command.ExecuteNonQuery();

                        playerData.ReturnCode = "S420";
                        playerData.ReturnMessage = "更新老鼠資料成功！";
                    }
                    else if (DS.Tables[0].Rows.Count == 0) // 如果沒有找到玩家資料
                    {
                        playerData.ReturnCode = "S421";
                        playerData.ReturnMessage = "更新老鼠資料失敗！";
                    }
                }
            }
            catch (Exception e)
            {
                playerData.ReturnCode = "S499";
                playerData.ReturnMessage = "取得玩家資料例外情況！";
                Log.Debug("(IO)UpdatePlayerData failed! " + e.Message + " 於: " + e.StackTrace);
                throw e;
            }
            return playerData;
        }

        #endregion


        #region UpdatePlayerItem 更新玩家(道具)資料
        [AutoComplete]
        public PlayerData UpdatePlayerItem(string account, Int16 itemID, byte itemType, Int16 itemCount)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "(IO)S400";
            playerData.ReturnMessage = "";
            DataSet DS = new DataSet();
         //   Log.Debug("account:" + account + "itemName:" + itemID + "itemType:" + itemType + "itemCount:" + itemCount);
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(this.connectionString))
                {
                    SqlCommand sqlCmd = new SqlCommand();
                    sqlCmd.Connection = sqlConn;
                    sqlConn.Open();

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(string.Format("SELECT ItemID FROM Player_PlayerItem WHERE Account='{0}' AND ItemID='{1}' ", account, itemID), sqlConn);
                    adapter.Fill(DS);

                    int dataCount = DS.Tables[0].Rows.Count;
                    // 如果找到玩家資料
                    if (dataCount == 1)
                    {
                        string query = @"UPDATE Player_PlayerItem WITH(ROWLOCK) SET ItemCount=@itemCount WHERE Account=@account AND ItemID=@itemID";
                        SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@account", account);
                        command.Parameters.AddWithValue("@ItemID", itemID);
                        command.Parameters.AddWithValue("@itemCount", itemCount);
                        command.ExecuteNonQuery();

                        playerData.ReturnCode = "S422";
                        playerData.ReturnMessage = "更新玩家道具資料成功！";
                    }
                    else if (dataCount == 0) // 如果沒有找到玩家資料
                    {
                        string query = "INSERT INTO Player_PlayerItem (Account,ItemID,ItemCount,ItemType) VALUES(@account, @itemID, @itemCount, @itemType)";
                        SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@account", account);
                        command.Parameters.AddWithValue("@itemID", itemID);
                        command.Parameters.AddWithValue("@itemCount", itemCount);
                        command.Parameters.AddWithValue("@itemType", itemType);
                        command.ExecuteNonQuery();

                        playerData.ReturnCode = "S423";
                        playerData.ReturnMessage = "新增玩家道具資料成功！";
                    }
                    else
                    {
                        playerData.ReturnCode = "S424";
                        playerData.ReturnMessage = "更新玩家道具資料失敗！";
                    }
                }
            }
            catch (Exception e)
            {
                playerData.ReturnCode = "S499";
                playerData.ReturnMessage = "更新玩家道具資料例外情況！";
                Log.Debug("(IO)UpdatePlayerData failed! " + e.Message + " 於: " + e.StackTrace);
                throw e;
            }
            return playerData;
        }

        #endregion

        #region UpdatePlayerItem 更新玩家(多筆道具)資料
        [AutoComplete]
        public PlayerData UpdatePlayerItem(string account, Dictionary<string, Dictionary<string, object>> dictItemUsage, List<string> columns)
        {
            PlayerData playerData = new PlayerData();
            DataSet DS = new DataSet();
            int counter = 0;
            string updateString = @"UPDATE Player_PlayerItem SET "; // unique
            string updateString2 = "";  // sql where data
            char[] charsToTrim = { ',' };

            playerData.ReturnCode = "(IO)S400";
            playerData.ReturnMessage = "";
            List<string> keys = new List<string>(dictItemUsage.Keys);


            //foreach (String key in keys)
            //{
            //    Log.Debug("Key:" + key);
            //}

            //foreach (KeyValuePair<string, Dictionary<string, object>> inner in dictItemUsage)
            //{
            //    foreach (KeyValuePair<string, object> item in inner.Value)
            //    {
            //        Log.Debug("Key:" + item.Key + "  Value:" + item.Value);
            //    }
            //}


            int i = 0;
            foreach (string col in columns)
            {
                // 建立 SQL字串 ItemCount
                foreach (KeyValuePair<string, Dictionary<string, object>> inner in dictItemUsage)
                {
                    //Log.Debug("dictItemUsage Key:" + inner.Key + "col:" + col);
                    object value;

                    if (inner.Value.ContainsKey(col))
                    {
                        if (counter == 0) updateString += col + " = (case";
                        updateString += String.Format(" when " + PlayerItem.ItemID.ToString() + " ='{0}' ", keys[counter]);

                        if (inner.Value.TryGetValue(col, out value))
                            updateString += String.Format(" then '{0}' ", value.ToString());

                        counter++;
                    }
                    else
                    {
                        break;
                    }
                    if (dictItemUsage.Count == counter) updateString += "end),";
                }

                counter = 0;

                i++;

                //Log.Debug("updateString:" + updateString);
            }

            foreach (String key in keys)
            {
                updateString2 += key + ",";
            }

            updateString = updateString.TrimEnd(charsToTrim);
            updateString2 = updateString2.TrimEnd(charsToTrim);
            updateString += " WHERE " + PlayerItem.ItemID.ToString() + " in (" + updateString2 + ") AND Account='" + account + "'"; // unique

       //     Log.Debug("PlayerData IO updateString:" + updateString);

            try
            {
                using (SqlConnection sqlConn = new SqlConnection(this.connectionString))
                {
                    SqlCommand sqlCmd = new SqlCommand();
                    sqlCmd.Connection = sqlConn;
                    sqlConn.Open();


                    string query = updateString;
                    SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
                    command.Parameters.Clear();
                    command.ExecuteNonQuery();

                    playerData.ReturnCode = "S422";
                    playerData.ReturnMessage = "更新玩家道具資料成功！";
                }
            }
            catch (Exception e)
            {
                playerData.ReturnCode = "S499";
                playerData.ReturnMessage = "更新玩家道具資料例外情況！";
                Log.Debug("(IO)UpdatePlayerData failed! " + e.Message + " 於: " + e.StackTrace);
                throw e;
            }
            return playerData;
        }
        #endregion

        //#region UpdatePlayerItem 更新玩家(多筆道具)資料
        //[AutoComplete]
        //public PlayerData UpdatePlayerItem(string account, Dictionary<Int16, Dictionary<string, object>> dictItemUsage)
        //{
        //    PlayerData playerData = new PlayerData();
        //    DataSet DS = new DataSet();
        //    string updateString = @"UPDATE Player_PlayerItem SET ItemCount = (case";
        //    string updateString2 = "";  // sql where data
        //    char[] charsToTrim = { ',' };

        //    playerData.ReturnCode = "(IO)S400";
        //    playerData.ReturnMessage = "";

        //    // 建立 SQL字串 ItemCount
        //    foreach (KeyValuePair<Int16, Dictionary<string, object>> inner in dictItemUsage)
        //    {
        //        foreach (KeyValuePair<string, object> item in inner.Value)
        //        {
        //            if (item.Key == ((short)PlayerItem.ItemID).ToString())
        //            {
        //                updateString += String.Format(" when ItemID='{0}' ", item.Value);
        //                updateString2 += item.Value + ",";
        //            }
        //            if (item.Key == ((short)PlayerItem.ItemCount).ToString())
        //                updateString += String.Format(" then '{0}' ", item.Value);
        //        }
        //    }

        //    updateString2 = updateString2.TrimEnd(charsToTrim);
        //    updateString += "end), UseCount = (case";

        //    // 建立 SQL字串 UseCount
        //    foreach (KeyValuePair<Int16, Dictionary<string, object>> inner in dictItemUsage)
        //    {
        //        foreach (KeyValuePair<string, object> item in inner.Value)
        //        {
        //            if (item.Key == ((short)PlayerItem.ItemID).ToString())
        //                updateString += String.Format(" when ItemID='{0}' ", item.Value);

        //            if (item.Key == ((short)PlayerItem.UseCount).ToString())
        //                updateString += String.Format(" then '{0}' ", item.Value);
        //        }
        //    }

        //    updateString += "end)  WHERE ItemID in (" + updateString2 + ")";

        //    try
        //    {
        //        using (SqlConnection sqlConn = new SqlConnection(this.connectionString))
        //        {
        //            SqlCommand sqlCmd = new SqlCommand();
        //            sqlCmd.Connection = sqlConn;
        //            sqlConn.Open();


        //            string query = updateString;
        //            SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
        //            command.Parameters.Clear();
        //            command.ExecuteNonQuery();

        //            playerData.ReturnCode = "S422";
        //            playerData.ReturnMessage = "更新玩家道具資料成功！";
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        playerData.ReturnCode = "S499";
        //        playerData.ReturnMessage = "更新玩家道具資料例外情況！";
        //        Log.Debug("(IO)UpdatePlayerData failed! " + e.Message + " 於: " + e.StackTrace);
        //        throw e;
        //    }
        //    return playerData;
        //}
        //#endregion

        #region UpdatePlayerItem 更新玩家(道具)裝備狀態
        [AutoComplete]
        public PlayerData UpdatePlayerItem(string account, Int16 itemID, bool isEquip)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "(IO)S400";
            playerData.ReturnMessage = "";
            DataSet DS = new DataSet();
            //Log.Debug("account:" + account + "itemName:" + itemID + "itemType:" + itemType + "itemCount:" + itemCount);
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(this.connectionString))
                {
                    //SqlDataAdapter adapter = new SqlDataAdapter();
                    //adapter.SelectCommand = new SqlCommand(string.Format("SELECT ItemID FROM Player_PlayerItem WHERE Account='{0}' AND ItemID='{1}' ", account, itemID), sqlConn);
                    //adapter.Fill(DS);

                    //int dataCount = DS.Tables[0].Rows.Count;
                    //// 如果找到玩家資料
                    //if (dataCount == 1)
                    //{
                    //Log.Debug("Select OK");
                    string query = @"UPDATE Player_PlayerItem WITH(ROWLOCK) SET IsEquip=@isEquip WHERE Account=@account AND ItemID=@itemID";
                    //string query = @"UPDATE Player_PlayerItem SET isEquip=@isEquip WHERE PrimaryID=@primaryID";
                    SqlCommand command = new SqlCommand(query, sqlConn);
                    sqlConn.Open();
                    command.Parameters.AddWithValue("@account", account);
                    command.Parameters.AddWithValue("@itemID", itemID);
                    command.Parameters.AddWithValue("@isEquip", isEquip);
                    command.ExecuteNonQuery();
                    //Log.Debug("Update OK");
                    playerData.ReturnCode = "S422";
                    playerData.ReturnMessage = "更新玩家道具資料成功！";
                    //}
                    //else
                    //{
                    //    playerData.ReturnCode = "S424";
                    //    playerData.ReturnMessage = "更新玩家道具資料失敗！";
                    //}
                }
            }
            catch (Exception e)
            {
                playerData.ReturnCode = "S499";
                playerData.ReturnMessage = "更新玩家道具資料例外情況！";
                Log.Debug("(IO)UpdatePlayerData failed! " + e.Message + " 於: " + e.StackTrace);
                throw e;
            }
            return playerData;
        }

        #endregion

        #region SortPlayerItem 更新玩家(道具排序)資料
        [AutoComplete]
        public PlayerData SortPlayerItem(string account, string jString)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "(IO)S400";
            playerData.ReturnMessage = "";
            DataSet DS = new DataSet();
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(this.connectionString))
                {
                    string query = @"UPDATE Player_PlayerData WITH(ROWLOCK) SET Item=@item WHERE Account=@account";
                    SqlCommand command = new SqlCommand(query, sqlConn);
                    sqlConn.Open();
                    command.Parameters.AddWithValue("@account", account);
                    command.Parameters.AddWithValue("@item", jString);
                    command.ExecuteNonQuery();
                   // Log.Debug("Update OK");
                    playerData.ReturnCode = "S428";
                    playerData.ReturnMessage = "排序玩家道具資料成功！";
                }
            }
            catch (Exception e)
            {
                playerData.ReturnCode = "S499";
                playerData.ReturnMessage = "排序玩家道具資料例外情況！";
                Log.Debug("(IO)UpdatePlayerData failed! " + e.Message + " 於: " + e.StackTrace);
                throw e;
            }
            return playerData;
        }

        #endregion

        #region LoadFriendsData 取得朋友資料
        [AutoComplete]
        public PlayerData LoadFriendsData(List<string> accounts)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "S300";
            playerData.ReturnMessage = "";
            DataSet DS = new DataSet();
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(this.connectionString))
                {
                    SqlCommand sqlCmd = new SqlCommand();
                    sqlCmd.Connection = sqlConn;
                    sqlConn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();

                    string selectString = @"SELECT Player_PlayerData.Rank, GansolMember.Nickname,Player_PlayerData.Image FROM Player_PlayerData LEFT JOIN  GansolMember ON GansolMember.Account = Player_PlayerData.Account WHERE "; // unique

                    foreach (string account in accounts)
                        selectString += "Player_PlayerData.Account = '" + account + "' OR ";

                    selectString = selectString.Remove(selectString.Length - 3);
                   // Log.Debug("FUCK: " + selectString);
                    adapter.SelectCommand = new SqlCommand(selectString, sqlConn);
                    adapter.Fill(DS);

                    //如果找到會員資料(帳號、密碼) 登入成功
                    if (DS.Tables[0].Rows.Count > 0)
                    {
                        int i = 0, j = 0;

                        foreach (DataTable table in DS.Tables)
                        {
                            Dictionary<string, object> dictFirends = new Dictionary<string, object>();

                            foreach (DataRow row in table.Rows)
                            {
                                j = 0;
                                Dictionary<string, object> dictData2 = new Dictionary<string, object>();
                                foreach (DataColumn col in table.Columns)
                                {
                                    //if(col.ColumnName=="Account")  account = table.Rows[i][col].ToString();// 0 = itemID
                                    dictData2.Add(col.ColumnName, table.Rows[i][col].ToString());
                                    j++;
                                }
                                dictFirends.Add(accounts[i], dictData2);
                                i++;
                            }
                            playerData.Friends = Json.Serialize(dictFirends);
                        }


                        playerData.ReturnCode = "S432";
                        playerData.ReturnMessage = "取得朋友資料成功！";
                    }
                    else
                    {
                        playerData.ReturnCode = "S433";
                        playerData.ReturnMessage = "取得朋友資料失敗，沒有資料！";
                    }
                }
            }
            catch
            {
                playerData.ReturnCode = "S499";
                playerData.ReturnMessage = "取得朋友資料失敗，未知例外情況！";
                throw;
            }

            return playerData;
        }
        #endregion



    }
}