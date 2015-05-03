using System;

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
 * 這個檔案是用來儲存COM+ 會員資料所使用
 * 
 * ***************************************************************/

namespace MPCOM
{
    [Serializable()]
    public struct MemberData
    {
        public string ReturnCode;               // 回傳值
        public string ReturnMessage;            // 回傳說明文字<30全形字

        public int PrimaryID;                   // 主索引
        public string Account;                  // 帳號
        public string Nickname;                 // 角色名稱
        public byte Age;                         // 年齡
        public byte Sex;                         // 性別
        public string IP;                       // IP
    }
}
