namespace MPProtocol
{
    public enum SpawnStatus
    {
        Line,                       // 左到右>右到左 直線產生
        OppositeLine,               // 反向 左到右>右到左 至最前 直線產生
        Circle,                     // 繞圈方式產生
        OppositeCircle,             // 反向 繞圈方式產生
        ByNum,                      // 一次產生 多少數量
        Horizontal,                 // 水平方式產生
    }

    public enum BattleOperationCode
    {
        ExitRoom = 41,              // 戰鬥中途離開房間
        KickOther=42,               // 踢人
        CheckStatus=43,             // 檢查玩家遊戲狀態
        SendDamage = 44,            // 玩者A發動技能 攻擊 玩者B
        UpdateScore = 52,           // 更新分數
    }

    public enum BattleParameterCode
    {
        Ret,                        // 回傳值
        Damage,                     // 技能傷害
        RoomID,                     // 房間ID
        PrimaryID,                  // 主索引
        Account,                    // 帳號
        OtherScore,                 // 另一位玩家分數
        Score,                      // 分數
        Time,                       // 時間
        MiceID,                     // 老鼠ID
        MiceName,                   // 老鼠名稱
        EatingRate,                 // 咀嚼頻率
    }

    public enum BattleResponseCode
    {
        ExitRoom =41,               // 離開房間
        KickOther=42,               // 踢人
        DebugMessage=45,            // OnEvent除錯訊息
        Online=46,                  // 線上
        Offline=47,                 // 離線
        Damage=48,                  // 造成傷害
        ApplyDamage=49,             // 接收 技能攻擊
        GetScore=51,                // 取得另一位玩家分數
        UpdateScore = 52,           // 更新分數
    }
}
