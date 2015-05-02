namespace MPProtocol
{
    public enum JoinMemberOperationCode
    {
        JoinMember = 21,                // 加入會員
    }

    public enum JoinMemberParameterCode
    {
        Ret,                            // 回傳碼
        Account,                        // 帳號
        Password,                       // 密碼
        Nickname,                       // 暱稱
        Sex,                            // 性別
        Age,                            // 年齡
        IP,                             // IP
        JoinDate,                       // 加入會員日期
    }

    public enum JoinMemberResponseCode
    {
        JoinMember = 21,                // 加入會員
    }
}
