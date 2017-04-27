namespace MPProtocol
{
    public enum MemberType          // 會員型態
    {
        Bot = 0,
        Gansol = 1,                
        Google = 2,               
        Facebook=3,
        Twitter=4,
    }

    public enum MemberOperationCode
    {
        JoinMember = 21,                // 加入會員
        UpdateMember = 22,
    }

    public enum MemberParameterCode
    {
        Ret,                            // 回傳碼
        Account,                        // 帳號
        Password,                       // 密碼
        Nickname,                       // 暱稱
        Sex,                            // 性別
        Age,                            // 年齡
        IP,                             // IP
        JoinDate,                       // 加入會員日期
        MemberType,                     // 會員類型
        Email,                          // Email
    }

    public enum MemberResponseCode
    {
        JoinMember = 21,                // 加入會員
    }
}
