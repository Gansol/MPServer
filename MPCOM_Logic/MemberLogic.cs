using System;
using System.EnterpriseServices;
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
 * 這個檔案是用來進行 驗證會員所使用
 * 加入會員驗證、登入驗證
 * >>try catch 要移除 使用AutoComplete就可 移除後刪除
 * 
 * ***************************************************************/

[assembly: ApplicationName("MPCOM"), ApplicationAccessControl(true)]
namespace MPCOM
{
    // TransactionOption 指定元件要求的自動交易類型。
    // NotSupported	沒有使用支配性的交易在內容中建立元件。
    // Required	共用交易 (如果存在的話)，並且建立新交易 (如果有必要的話)。
    // RequiresNew	不論目前內容的狀態如何，都使用新交易建立元件。
    // Supported	共用交易 (如果有存在的話)。
    [Transaction(TransactionOption.Required)]
    public class MemberLogic : ServicedComponent // ServicedComponent 表示所有使用 COM+ 服務之類別的基底類別。
    {
        protected override bool CanBePooled()
        {
            return true;
        }

        #region JoinMember(Gansol) 加入會員

        [AutoComplete]
        public MemberData JoinMember(string account, string password, string nickname, byte age, byte sex, string IP, string email, string joinTime, byte memberType)
        {
            MemberData memberData = new MemberData();
            memberData.ReturnCode = "(Logic)S100";
            memberData.ReturnMessage = "";

            try
            {
                TextUtility textUtility = new TextUtility();

                // 檢查會員帳號
                if (textUtility.SaveTextChk(account) != 1)
                {
                    memberData.ReturnCode = "S102";
                    memberData.ReturnMessage = "會員帳號內含不合法字元!!";
                    return memberData;
                }
                else if (account.Length < 8 || account.Length > 64)
                {
                    memberData.ReturnCode = "S102";
                    memberData.ReturnMessage = "會員帳號長度不正確(8~16字元)!!";
                    return memberData;
                }

                // 檢查密碼
                if (textUtility.SaveTextChk(password, new char[] { '#', '$', '%', '^', '&', '*', '(', ')' }) != 1)
                {
                    memberData.ReturnCode = "S103";
                    memberData.ReturnMessage = "會員密碼內含不合法字元!!";
                    return memberData;
                }
                //else if (password.Length < 8 || password.Length > 16)
                //{
                //    memberData.ReturnCode = "S103";
                //    memberData.ReturnMessage = "會員密碼長度不正確(8~16字元)!!";
                //    return memberData;
                //}

                char[] charsToTrim = { '　', '*', ' ', '\'', '#', '$', '%', '^', '&', '*', '(', ')' };
                nickname = nickname.Trim(charsToTrim);

                // 檢查暱稱
                //if (textUtility.SaveTextChk(nickname, new char[] { '#', '$', '%', '^', '&', '*', '(', ')' }) != 1)
                //{
                //    memberData.ReturnCode = "S104";
                //    memberData.ReturnMessage = "會員暱稱內含不合法字元!!";
                //    return memberData;
                //}
                if (nickname == "" || nickname.Length <= 0 || nickname.Length > 32)
                {
                    memberData.ReturnCode = "S104";
                    memberData.ReturnMessage = "會員暱稱長度不正確(1~12字元)!!";
                    return memberData;
                }

                // 檢查年齡
                if ((textUtility.NumTextChk(age.ToString()) != 1) || age <= 0 || age >= 100)
                {
                    memberData.ReturnCode = "S105";
                    memberData.ReturnMessage = "請選擇正確年齡!!";
                    return memberData;
                }

                // 檢查性別
                if ((sex < 0 || sex > 1) && sex != 3)
                {
                    memberData.ReturnCode = "S106";
                    memberData.ReturnMessage = "請選擇正確性別!!";
                    return memberData;
                }


                // 檢查Email
                if (textUtility.EMailChk(email) != 1)
                {
                    memberData.ReturnCode = "S107";
                    memberData.ReturnMessage = "請輸入正確Email !!";
                    return memberData;
                }
                else if (email.Length <= 5 || email.Length > 48)
                {
                    memberData.ReturnCode = "S107";
                    memberData.ReturnMessage = "Email長度不正確(6~48字元)!!";
                    return memberData;
                }

                // 驗證成功後再進行IO寫入 寫入後存入 memberData
                MemberIO memberIO = new MemberIO();
                memberData = memberIO.JoinMember(account, password, nickname, age, sex, IP, email, joinTime, memberType);

            }
            catch (Exception e)
            {
                throw e;
            }

            return memberData; //回傳資料
        }

        #endregion

        #region JoinMember(SNS) 加入會員

        [AutoComplete]
        public MemberData JoinMember(string account, string nickname, string IP, string email, string joinTime, byte memberType)
        {
            MemberData memberData = new MemberData();
            memberData.ReturnCode = "(Logic)S100";
            memberData.ReturnMessage = "";
            try
            {
                // 驗證成功後再進行IO寫入 寫入後存入 memberData
                MemberIO memberIO = new MemberIO();
                // memberData = memberIO.JoinMember(account,"25d55ad283aa400af464c76d713c07ad", nickname,IP,joinTime, SNSType); //MD5
                memberData = memberIO.JoinMember(account, "80f99b1aa38deb107754bfc11286b00248daa15b", nickname, IP, email, joinTime, memberType);

            }
            catch (Exception e)
            {
                throw e;
            }

            return memberData; //回傳資料
        }

        #endregion

        #region MemberLogin 會員登入
        [AutoComplete]
        public MemberData MemberLogin(string account, string password)
        {
            MemberData memberData = new MemberData();
            memberData.ReturnCode = "S200";
            memberData.ReturnMessage = "";
            try
            {
                TextUtility textUtility = new TextUtility();

                // 檢查會員帳號
                if (textUtility.SaveTextChk(account) != 1)
                {
                    memberData.ReturnCode = "S202";
                    memberData.ReturnMessage = "會員帳號內含不合法字元!!";
                    return memberData;
                }

                // 檢查密碼
                if (textUtility.SaveTextChk(password, new char[] { '#', '$', '%', '^', '&', '*', '(', ')' }) != 1)
                {
                    memberData.ReturnCode = "S203";
                    memberData.ReturnMessage = "會員密碼內含不合法字元!!";
                    return memberData;
                }

                //如果檢查成功 進行會員資料比對
                MemberIO memberIO = new MemberIO();
                memberData = memberIO.MemberLogin(account, password);
            }
            catch (Exception e)
            {
                throw e;
            }

            return memberData;
        }
        #endregion



        #region MemberLogin 會員登入
        [AutoComplete]
        public MemberData UpdateMember(string account,string jString)
        {
            MemberData memberData = new MemberData();
            memberData.ReturnCode = "S200";
            memberData.ReturnMessage = "";
            try
            {
                //如果檢查成功 進行會員資料比對
                MemberIO memberIO = new MemberIO();
                memberData = memberIO.UpdateMember(account, jString);
            }
            catch (Exception e)
            {
                throw e;
            }
            return memberData;
        }
        #endregion
    }
}
