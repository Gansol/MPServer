﻿using ExitGames.Logging;
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
    [Transaction(TransactionOption.Required)]
    public class PlayerDataIO : ServicedComponent
    {
        static string host = "localhost\\SQLEXPRESS";               // 主機位置 IP(本機)\\伺服器名稱
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

                    Log.Debug("連線資訊 :" + sqlConn.ToString());

                    // 讀取玩家資料 填入DS資料列
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(String.Format("SELECT * FROM PlayerData WHERE (Account='{0}')", Account), sqlConn);
                    adapter.Fill(DS);

                    // 若有讀到則取得所有資料
                    if (DS.Tables[0].Rows.Count > 0)
                    {
                        playerData.Rank = Convert.ToByte(DS.Tables[0].Rows[0]["Rank"]);
                        playerData.EXP = Convert.ToByte(DS.Tables[0].Rows[0]["EXP"]);
                        playerData.MaxCombo = Convert.ToInt16(DS.Tables[0].Rows[0]["MaxCombo"]);
                        playerData.MaxScore = Convert.ToInt32(DS.Tables[0].Rows[0]["MaxScore"]);
                        playerData.SumScore = Convert.ToInt32(DS.Tables[0].Rows[0]["SumScore"]);
                        playerData.MiceAll = Convert.ToString(DS.Tables[0].Rows[0]["MiceAll"]);
                        playerData.Team = Convert.ToString(DS.Tables[0].Rows[0]["Team"]);
                        playerData.MiceAmount = Convert.ToString(DS.Tables[0].Rows[0]["MiceAmount"]);
                        playerData.Friend = Convert.ToString(DS.Tables[0].Rows[0]["Friend"]);

                        playerData.ReturnCode = "S401"; //true
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
                Log.Debug("Load PlayerData Failed ! " + e.Message +" 於: "+e.StackTrace);
                throw e;
            }

            return playerData;
        }
        #endregion

        #region UpdatePlayerData 更新玩家資料
        [AutoComplete]
        public PlayerData UpdatePlayerData(string account,byte rank,byte exp,Int16 maxCombo,int maxScore,int sumScore,string miceAll,string team ,string miceAmount,string friend)
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
                    adapter.SelectCommand = new SqlCommand(string.Format("SELECT * FROM PlayerData WHERE (Account='{0}') ", account), sqlConn);
                    adapter.Fill(DS);

                    //Log.Debug("Tables Count: " + DS.Tables[0].Rows.Count);

                    // 如果找到玩家資料
                    if (DS.Tables[0].Rows.Count == 1)   
                    {
                        string query = @"UPDATE PlayerData SET Rank=@rank,EXP=@exp,MaxCombo=@maxCombo,MaxScore=@maxScore,SumScore=@sumScore,MiceAll=@miceAll,Team=@team,MiceAmount=@miceAmount,Friend=@friend WHERE Account=@account";
                        SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
                        command.Parameters.AddWithValue("@account", account);
                        command.Parameters.AddWithValue("@rank", rank);
                        command.Parameters.AddWithValue("@exp", exp);
                        command.Parameters.AddWithValue("@maxCombo", maxCombo);
                        command.Parameters.AddWithValue("@maxScore", maxScore);
                        command.Parameters.AddWithValue("@sumScore", sumScore);
                        command.Parameters.AddWithValue("@miceAll", miceAll);
                        command.Parameters.AddWithValue("@team", team);
                        command.Parameters.AddWithValue("@miceAmount", miceAmount);
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

    }
}