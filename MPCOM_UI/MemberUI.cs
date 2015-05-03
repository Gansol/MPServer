using System;
using System.IO;
using System.EnterpriseServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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
 * 會員資料界面層 提供外部存取使用
 * 加入會員、會員登入
 * 
 * ***************************************************************/

[assembly: ApplicationName("MPCOM"), ApplicationAccessControl(true)]
namespace MPCOM
{
    public interface IMemberUI  // 使用介面 可以提供給不同程式語言繼承使用      
    {
        byte[] JoinMember(string account, string password, string nickname, byte age, byte sex, string IP, string email, string joinTime);
        byte[] MemberLogin(string account,string password);
    }

    public class MemberUI : ServicedComponent, IMemberUI
    {
        protected override bool CanBePooled()
        {
            return true;
        }

        #region JoinMember 加入會員
        public byte[] JoinMember(string account, string password, string nickname, byte age, byte sex, string IP, string email, string joinTime)
        {
            MemberData memberData = new MemberData();
            memberData.ReturnCode = "S100";
            memberData.ReturnMessage = "";

            try
            {
                MemberLogic memberLogic = new MemberLogic();
                memberData = memberLogic.JoinMember(account, password, nickname, age, sex, IP, email, joinTime);
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
    }
}
