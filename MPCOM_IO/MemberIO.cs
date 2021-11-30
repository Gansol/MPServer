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
 * 這個檔案是用來進行資料庫讀寫 會員資料所使用
 * 載入會員資料、插入會員資料
 * >>IO須加強SQL安全性 加強後刪除
 * >>try catch 要移除 使用AutoComplete就可 移除後刪除
 * >>ILogger似乎沒用 要移除 移除後刪除
 * >>SNS登入有給預設值 性別:3 秘密(未設定) 年齡:99
 * ***************************************************************/

[assembly: ApplicationName("MPCOM"), ApplicationAccessControl(true)] //可以對MPCOM之間組件有存取控制權限
namespace MPCOM
{
    // TransactionOption 指定元件要求的自動交易類型。
    // NotSupported	沒有使用支配性的交易在內容中建立元件。
    // Required	共用交易 (如果存在的話)，並且建立新交易 (如果有必要的話)。
    // RequiresNew	不論目前內容的狀態如何，都使用新交易建立元件。
    // Supported	共用交易 (如果有存在的話)。
    [Transaction(TransactionOption.Required)]                   // ServicedComponent 表示所有使用 COM+ 服務之類別的基底類別。
    public class MemberIO : ServicedComponent
    {
        static string host = "localhost"/*\\MPSQLSERVER"*/;           //主機位置 IP(本機)\\伺服器名稱
        static string id = "Gansol";                             // SQL Server帳號
        static string pwd = "1234";                             // SQL Server密碼
        static string database = "MicePowDB";                   // 資料庫名稱
        string connectionString = string.Format("Server = {0};Database = {1};User ID = {2};Password = {3};", host, database, id, pwd);

        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        protected override bool CanBePooled()                   // 覆寫 可以被放入集區 
        {
            return true;
        }

        #region JoinMember(Gansol) 加入會員
        /// <summary>
        /// Gansol 加入會員
        /// </summary>
        /// <returns></returns>
        [AutoComplete]
        public MemberData JoinMember(string account, string password, string nickname, byte age, byte sex, string IP, string email, string joinTime, byte memberType)
        {
            MemberData memberData = default(MemberData);
            memberData.ReturnCode = "(IO)S100";
            memberData.ReturnMessage = "";
            DataSet DS = new DataSet();

            try
            {
                using (SqlConnection sqlConn = new SqlConnection(this.connectionString))
                {
                    bool bJoin = false;
                    SqlCommand sqlCmd = new SqlCommand();
                    sqlCmd.Connection = sqlConn;
                    sqlConn.Open();

                    SqlDataAdapter adapter = new SqlDataAdapter();

                    adapter.SelectCommand = new SqlCommand(string.Format("SELECT * FROM GansolMember WHERE (Account='{0}') ", account), sqlConn);
                    adapter.Fill(DS);
                    Log.Debug("Tables Count: " + DS.Tables[0].Rows.Count);


                    //假如資料表中找不到資料 尚未加入會員
                    if (DS.Tables[0].Rows.Count == 0)
                    {
                        //插入 會員資料
                        string query = "INSERT INTO GansolMember (Account,Password,Nickname,Age,Sex,IP,Email,JoinTime,MemberType) VALUES(@account, @password, @nickname, @age, @sex, @IP, @email, @joinTime, @memberType)";
                        SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
                        command.Parameters.AddWithValue("@account", account);
                        command.Parameters.AddWithValue("@password", password);
                        command.Parameters.AddWithValue("@nickname", nickname);
                        command.Parameters.AddWithValue("@age", age);
                        command.Parameters.AddWithValue("@sex", sex);
                        command.Parameters.AddWithValue("@IP", IP);
                        command.Parameters.AddWithValue("@email", email);
                        command.Parameters.AddWithValue("@joinTime", joinTime);
                        command.Parameters.AddWithValue("@memberType", memberType);
                        int ExecuteNonQuery = command.ExecuteNonQuery();

                        memberData.ReturnCode = "S101";
                        memberData.ReturnMessage = "加入會員成功！";
                        bJoin = true;
                    }
                    else if (DS.Tables[0].Rows.Count > 0)
                    {
                        memberData.ReturnCode = "S108";
                        memberData.ReturnMessage = "已有相同的會員帳號！";
                    }

                    //adapter.SelectCommand = new SqlCommand(string.Format("SELECT * FROM Player_PlayerData WHERE (Account='{0}') ", account), sqlConn);
                    //adapter.Fill(DS);

                    //假如玩家資料中沒有資料 建立一份新玩家資料
                    if (bJoin)
                    {
                        string query = "INSERT INTO Player_PlayerData (Account) VALUES (@account) ";
                        SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
                        command.Parameters.AddWithValue("@account", account);
                        int ExecuteNonQuery = command.ExecuteNonQuery();
                    }


                    //假如玩家道具中沒有資料 建立一份新玩家道具
                    if (bJoin)
                    {
                        string query = "INSERT INTO Player_PlayerItem VALUES (@account,'10001','100','1','False','0','-1','1','0')";
                        SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
                        command.Parameters.AddWithValue("@account", account);
                        int ExecuteNonQuery = command.ExecuteNonQuery();
                    }


                    //adapter.SelectCommand = new SqlCommand(string.Format("SELECT * FROM Player_GameCurrency WHERE (Account='{0}') ", account), sqlConn);
                    //adapter.Fill(DS);

                    //假如玩家貨幣資料中沒有資料 建立一份新玩家貨幣資料
                    if (bJoin)
                    {
                        string query = "INSERT INTO Player_GameCurrency (Account) VALUES (@account) ";
                        SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
                        command.Parameters.AddWithValue("@account", account);
                        int ExecuteNonQuery = command.ExecuteNonQuery();
                    }

                }
            }
            catch (Exception e)
            {
                memberData.ReturnCode = "S199";
                memberData.ReturnMessage = "加入會員失敗，未知例外情況！";
                Log.Debug("(IO)JoinMember failed!" + e.Message + " Track: " + e.StackTrace);
                throw e;
            }
            return memberData;
        }
        #endregion

        #region JoinMember(SNS) 加入會員
        /// <summary>
        /// SNS加入會員
        /// </summary>
        /// <returns></returns>
        [AutoComplete]
        public MemberData JoinMember(string account, string password, string nickname, string IP, string email, string joinTime, byte memberType)
        {
            bool bJoin = false;
            MemberData memberData = default(MemberData);
            memberData.ReturnCode = "(IO)S100";
            memberData.ReturnMessage = "";
            DataSet DS = new DataSet();

            try
            {
                using (SqlConnection sqlConn = new SqlConnection(this.connectionString))
                {

                    SqlCommand sqlCmd = new SqlCommand();
                    sqlCmd.Connection = sqlConn;
                    sqlConn.Open();

                    SqlDataAdapter adapter = new SqlDataAdapter();

                    adapter.SelectCommand = new SqlCommand(string.Format("SELECT * FROM GansolMember WHERE (Account='{0}') ", account), sqlConn);
                    adapter.Fill(DS);
                    Log.Debug("Tables Count: " + DS.Tables[0].Rows.Count);


                    //假如資料表中找不到資料 尚未加入會員
                    if (DS.Tables[0].Rows.Count == 0)
                    {
                        //插入 會員資料
                        string query = "INSERT INTO GansolMember (Account,Password,Nickname,Age,Sex,IP,Email,JoinTime,MemberType) VALUES(@account, @password, @nickname, @age, @sex, @IP, @email, @joinTime, @memberType)";
                        SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
                        command.Parameters.AddWithValue("@account", account);
                        command.Parameters.AddWithValue("@password", password);
                        command.Parameters.AddWithValue("@nickname", nickname);
                        command.Parameters.AddWithValue("@age", 99);    // 99歲
                        command.Parameters.AddWithValue("@sex", 3);     // 秘密(未設定)
                        command.Parameters.AddWithValue("@IP", IP);
                        command.Parameters.AddWithValue("@email", email);
                        command.Parameters.AddWithValue("@joinTime", joinTime);
                        command.Parameters.AddWithValue("@memberType", memberType);
                        int ExecuteNonQuery = command.ExecuteNonQuery();

                        memberData.ReturnCode = "S101";
                        memberData.ReturnMessage = "加入會員成功！";
                        bJoin = true;
                    }
                    else if (DS.Tables[0].Rows.Count > 0)
                    {
                        memberData.ReturnCode = "S108";
                        memberData.ReturnMessage = "已有相同的會員帳號！";
                    }

                    //adapter.SelectCommand = new SqlCommand(string.Format("SELECT * FROM Player_PlayerData WHERE (Account='{0}') ", account), sqlConn);
                    //adapter.Fill(DS);

                    //假如玩家資料中沒有資料 建立一份新玩家資料
                    if (bJoin)
                    {
                        string query = "INSERT INTO Player_PlayerData (Account) VALUES (@account) ";
                        SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
                        command.Parameters.AddWithValue("@account", account);
                        int ExecuteNonQuery = command.ExecuteNonQuery();
                    }


                    //假如玩家道具中沒有資料 建立一份新玩家道具
                    if (bJoin)
                    {
                        string query = "INSERT INTO Player_PlayerItem VALUES (@account,'10001','100','1','False','0','-1','1','0')";
                        SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
                        command.Parameters.AddWithValue("@account", account);
                        int ExecuteNonQuery = command.ExecuteNonQuery();
                    }


                    //adapter.SelectCommand = new SqlCommand(string.Format("SELECT * FROM Player_GameCurrency WHERE (Account='{0}') ", account), sqlConn);
                    //adapter.Fill(DS);

                    //假如玩家貨幣資料中沒有資料 建立一份新玩家貨幣資料
                    if (bJoin)
                    {
                        string query = "INSERT INTO Player_GameCurrency (Account) VALUES (@account) ";
                        SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
                        command.Parameters.AddWithValue("@account", account);
                        int ExecuteNonQuery = command.ExecuteNonQuery();
                    }

                }
            }
            catch (Exception e)
            {
                memberData.ReturnCode = "S199";
                memberData.ReturnMessage = "加入會員失敗，未知例外情況！";
                Log.Debug("(IO)JoinMember failed!" + e.Message + " Track: " + e.StackTrace);
                throw e;
            }
            return memberData;
        }
        #endregion

        #region MemberLogin 會員登入

        [AutoComplete]
        public MemberData MemberLogin(string Account, string Password)
        {
            MemberData memberData = default(MemberData);
            memberData.ReturnCode = "S200";
            memberData.ReturnMessage = "";
            DataSet DS = new DataSet();
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(this.connectionString))
                {
                    SqlCommand sqlCmd = new SqlCommand();
                    sqlCmd.Connection = sqlConn;
                    sqlConn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();

                    adapter.SelectCommand = new SqlCommand(string.Format("SELECT * FROM GansolMember WHERE (Account='{0}') AND (Password='{1}') ", Account, Password), sqlConn);
                    adapter.Fill(DS);

                    //如果找到會員資料(帳號、密碼) 登入成功
                    if (DS.Tables[0].Rows.Count > 0)
                    {
                        memberData.PrimaryID = (int)Convert.ToInt32(DS.Tables[0].Rows[0]["PrimaryID"]);
                        memberData.Account = Convert.ToString(DS.Tables[0].Rows[0]["Account"]);
                        memberData.Nickname = Convert.ToString(DS.Tables[0].Rows[0]["Nickname"]);
                        memberData.Age = (byte)Convert.ToByte(DS.Tables[0].Rows[0]["Age"]);
                        memberData.Sex = (byte)Convert.ToByte(DS.Tables[0].Rows[0]["Sex"]);
                        memberData.IP = Convert.ToString(DS.Tables[0].Rows[0]["IP"]);
                        memberData.MemberType = (byte)Convert.ToByte(DS.Tables[0].Rows[0]["MemberType"]);
                        memberData.ReturnCode = "S201";
                    }
                    else
                    {
                        memberData.ReturnCode = "S204";
                        memberData.ReturnMessage = "登入失敗，帳號或密碼錯誤！";
                    }
                }
            }
            catch (Exception e)
            {
                memberData.ReturnCode = "S299";
                memberData.ReturnMessage = "登入失敗，未知例外情況！";
                Log.Debug("(IO)MemberLogin failed!" + e.Message + " Track: " + e.StackTrace);
                throw e;
            }

            return memberData;
        }
        #endregion

        #region UpdateMember 更新會員資料

        [AutoComplete]
        public MemberData UpdateMember(string account, string jString)
        {
            MemberData memberData = default(MemberData);
            memberData.ReturnCode = "S200";
            memberData.ReturnMessage = "";
            DataSet DS = new DataSet();
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(this.connectionString))
                {
                    SqlCommand sqlCmd = new SqlCommand();
                    sqlCmd.Connection = sqlConn;
                    sqlConn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();

                    adapter.SelectCommand = new SqlCommand(string.Format("SELECT * FROM GansolMember WHERE (Account='{0}') ) ", account), sqlConn);
                    adapter.Fill(DS);

                    //如果找到會員資料(帳號、密碼) 登入成功
                    if (DS.Tables[0].Rows.Count > 0)
                    {
                        Dictionary<string, object> data = Json.Deserialize(jString) as Dictionary<string, object>;
                        string query = @" UPDATE GansolMember WITH(ROWLOCK) SET ";
                        foreach (KeyValuePair<string, object> item in data)
                            query += item.Key + "=" + item.Value + " ";
                        query += " WHERE Account=@account";

                        SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@account", account);
                        Log.Debug("-----------DevTest----------" + query);
                        command.ExecuteNonQuery();

                        memberData.ReturnCode = "S205";
                        memberData.ReturnMessage = "修改會員資料成功！";



                        //string query = @" UPDATE GansolMember WITH(ROWLOCK) SET Email=@data WHERE Account=@account";
                        //SqlCommand command = new SqlCommand(query, sqlCmd.Connection);
                        //command.Parameters.Clear();
                        //command.Parameters.AddWithValue("@account", account);
                        //command.Parameters.AddWithValue("@data", data);

                        //command.ExecuteNonQuery();

                        //memberData.ReturnCode = "S205";
                        //memberData.ReturnMessage = "修改會員資料成功！";
                    }
                    else
                    {
                        memberData.ReturnCode = "S206";
                        memberData.ReturnMessage = "修改失敗，沒有會員資料！";
                    }
                }
            }
            catch (Exception e)
            {
                memberData.ReturnCode = "S299";
                memberData.ReturnMessage = "登入失敗，未知例外情況！";
                Log.Debug("(IO)MemberLogin failed!" + e.Message + " Track: " + e.StackTrace);
                throw e;
            }

            return memberData;
        }
        #endregion
    }
}