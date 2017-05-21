namespace MPProtocol
{
    public enum MemberType          // 會員型態
    {
        Bot = 0,
        Gansol = 1,
        Google = 2,
        Facebook = 3,
        Twitter = 4,
    }

    public enum ENUM_MemberState          // 會員型態
    {
        Online,
        Offline,
        Idle,
        Matching,
        Playing,
    }

    public enum MemberOperationCode
    {
        JoinMember = 21,                // 加入會員
        UpdateMember = 22,
        LoadFriendsDetail = 23,
        GetOnlineActor = 24,
        GetOnlineActorState = 25,
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
        Friends,                        // 好友
        Custom,                         // 自定資料
        OnlineFriendsDetail,            // 線上詳細資料
        OnlineFriendsState,             // 線上玩家狀態
    }

    public enum MemberResponseCode
    {
        JoinMember = 21,                // 加入會員
        UpdateMember = 22,
        LoadFriendsDetail = 23,
        GetOnlineActor = 24,
        GetOnlineActorState = 25,
    }
}
