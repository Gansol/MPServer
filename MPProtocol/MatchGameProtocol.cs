namespace MPProtocol
{
    public enum MatchGameOperationCode
    {
        MatchGame = 31,             // 配對遊戲
        JoinRoom = 32,              // 加入房間
        QuitRoom = 33,              // 離開房間 可以砍了
        ExitWaiting = 35,           // 離開等待房間
        SyncGameStart = 36,         // 遊戲載入完成
        MatchGameBot = 37,          // Bot配對
        MatchGameFriend = 38,       // 好友配對
        InviteMatchGame = 39,       // 邀請對戰
        ApplyMatchGameFriend = 40,  // 同意好友配對邀請
    }

    public enum MatchGameResponseCode
    {
        Match = 34,                 // 配對成功
        ExitWaiting = 35,           // 離開等待房間
        SyncGameStart = 36,         // 同步遊戲
        WaitingGameStart = 37,      // 等待同步遊戲
        MatchGameFriend = 38,       // 好友配對
        InviteMatchGame = 39,       // 邀請對戰
        ApplyMatchGameFriend = 40,  // 同意好友配對邀請
    }

    public enum MatchGameParameterCode
    {
        PrimaryID = 0,              // 主索引
        RoomID,                     // 房間ID
        Nickname,                   // 暱稱
        Team,                       // 隊伍資料
        RoomPlace,                  // 在房間中的位置(主機or客人)
        ServerTime,                 // 遊戲開始時間
        GameTime,                   // 遊戲時間
        OtherAccount,               // 對手帳號
        OtherTeam,                  // 對手隊伍
        Account,                    // 帳號
    }
}
