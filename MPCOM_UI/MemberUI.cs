using System;
using System.IO;
using System.EnterpriseServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Gansol;
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
 * 會員資料界面層 提供外部存取使用
 * 加入會員、會員登入
 * 
 * ***************************************************************/

[assembly: ApplicationName("MPCOM"), ApplicationAccessControl(true)]
namespace MPCOM
{
    public interface IMemberUI  // 使用介面 可以提供給不同程式語言繼承使用      
    {
        byte[] JoinMember(string account, string password, string nickname, byte age, byte sex, string IP, string email, string joinTime,byte memberType);
        byte[] JoinMember(string account, string nickname, string IP, string email, string joinTime, byte memberType);
        byte[] MemberLogin(string account,string password);

        byte[] UpdateMember(string account ,string data, string columns);
    }

    public class MemberUI : ServicedComponent, IMemberUI
    {
        protected override bool CanBePooled()
        {
            return true;
        }

        #region JoinMember(Gansol) 加入會員
        /// <summary>
        /// JoinMember(Gansol、Facebook) 加入會員
        /// </summary>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <param name="nickname"></param>
        /// <param name="age"></param>
        /// <param name="sex"></param>
        /// <param name="IP"></param>
        /// <param name="email"></param>
        /// <param name="joinTime"></param>
        /// <param name="memberType"></param>
        /// <returns></returns>
        public byte[] JoinMember(string account, string password, string nickname, byte age, byte sex, string IP, string email, string joinTime, byte memberType)
        {
            MemberData memberData = new MemberData();
            memberData.ReturnCode = "S100";
            memberData.ReturnMessage = "";

            try
            {
                MemberLogic memberLogic = new MemberLogic();
                memberData = memberLogic.JoinMember(account, password, nickname, age, sex, IP, email, joinTime, memberType);
            }
            catch (Exception e)
            {
                memberData.ReturnCode = "S100";
                memberData.ReturnMessage = e.Message;
            }
            return TextUtility.SerializeToStream(memberData);
        }
        #endregion

        #region JoinMember(SNS) 加入會員
        /// <summary>
        /// JoinMember(SNS) 加入會員
        /// </summary>
        /// <param name="account"></param>
        /// <param name="nickname"></param>
        /// <param name="IP"></param>
        /// <param name="email"></param>
        /// <param name="joinTime"></param>
        /// <param name="memberType"></param>
        /// <returns></returns>
        public byte[] JoinMember(string account, string nickname, string IP,string email, string joinTime, byte memberType)
        {
            MemberData memberData = new MemberData();
            memberData.ReturnCode = "S100";
            memberData.ReturnMessage = "";

            try
            {
                MemberLogic memberLogic = new MemberLogic();
                memberData = memberLogic.JoinMember(account, nickname, IP,email, joinTime,memberType);
            }
            catch (Exception e)
            {
                memberData.ReturnCode = "S100";
                memberData.ReturnMessage = e.Message;
            }
            return TextUtility.SerializeToStream(memberData);
        }
        #endregion

        #region MemberLogin 會員登入
        public byte[] MemberLogin(string account, string password)
        {
            MemberData memberData = new MemberData();
            memberData.ReturnCode = "S200";
            memberData.ReturnMessage = "";

            try
            {
                MemberLogic memberLogic = new MemberLogic();
                memberData = memberLogic.MemberLogin(account, password);
            }
            catch (Exception e)
            {
                memberData.ReturnCode = "S200";
                memberData.ReturnMessage = e.Message;
            }
            return TextUtility.SerializeToStream(memberData);
        }
        #endregion



                public byte[] UpdateMember(string account ,string data , string columns)
        {
            MemberData memberData = new MemberData();
            memberData.ReturnCode = "S100";
            memberData.ReturnMessage = "";

            try
            {
                MemberLogic memberLogic = new MemberLogic();
                memberData = memberLogic.UpdateMember( account ,data,columns);
            }
            catch (Exception e)
            {
                memberData.ReturnCode = "S100";
                memberData.ReturnMessage = e.Message;
            }
            return TextUtility.SerializeToStream(memberData);
        }
      
    }
}
