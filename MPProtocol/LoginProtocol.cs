namespace MPProtocol
{
    public enum LoginOperationCode
    {
        Login = 11,                 // 登入
        ReLogin = 12,               // 重複登入
        Logout = 13,                // 登出
        GetProfile = 14,            // 取得SNS基本資料
    }

    public enum LoginResponseCode  //Peer那邊使用 和上面一樣的參數所以不打了
    {

    }

    public enum LoginParameterCode
    {
        Ret,                        // 回傳碼
        Account,                    // 帳號
        Password,                   // 密碼
        Nickname,                   // 暱稱
        PrimaryID,                  // 主索引
        Sex,                        // 性別
        Age,                        // 年齡
        IP,                         // IP
        MemberType,                 // 會員類型
    }
}
