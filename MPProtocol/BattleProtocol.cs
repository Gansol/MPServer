namespace MPProtocol
{
    public enum SpawnStatus : byte
    {
        // 1D 方式
        LineL = 101,                        // 左到右 直線產生
        LineR = 102,                        // 右到左 直線產生
        LinkLineL = 103,                    // 左到右 > 右到左 接續產生
        LinkLineR = 104,                    // 右到左 > 左到右 接續產生
        CircleLD = 105,                     // 左下開始 繞圈方式產生
        CircleRU = 106,                     // 右上開始 繞圈方式產生

        // 1D 反向 
        ReLineL = 151,                      // 左到右 直線產生
        ReLineR = 152,                      // 右到左 直線產生
        ReLinkLineL = 153,                  // 左到右 > 右到左 接續產生
        ReLinkLineR = 154,                  // 右到左 > 左到右 接續產生
        ReCircleLD = 155,                   // 左下開始 繞圈方式產生
        ReCircleRU = 156,                   // 右上開始 繞圈方式產生



        // 2D 方式
        VerticalL = 201,                    // 左邊開始 水平產生
        VerticalR = 202,                    // 右邊開始 水平產生
        LinkVertL = 203,                    // 左邊開始 水平接續產生
        LinkVertR = 204,                    // 右邊開始 水平接續產生
        HorizontalD = 205,                  // 下方開始 垂直產生
        HorizontalU = 206,                  // 上方開始 垂直產生
        LinkHorD = 207,                     // 下方開始 垂直接續產生
        LinkHorU = 208,                     // 上方開始 垂直接續產生
        HorTwin = 209,                      // 垂直生2個
        VertTwin = 210,                     // 水平生2個
        LinkHorTwin = 211,                  // 垂直接續產生 每次2個 到最後
        LinkVertTwin = 212,                 // 水平接續產生 每次2個 到最後

        TriangleLD = 213,                   // 左下開始 三角形
        TriangleRD = 214,                   // 右下開始 三角形
        TriangleLU = 215,                   // 左上開始 三角形
        TriangleRU = 216,                   // 右上開始 三角形
        BevelL = 217,                       // 左邊開始 45度斜角
        BevelR = 218,                       // 右邊開始 45度斜角

        // 2D 反向
        ReVerticalL = 227,                   // 左邊開始 水平產生
        ReVerticalR = 228,                   // 右邊開始 水平產生
        ReLinkVertL = 229,                   // 左邊開始 水平接續產生
        ReLinkVertR = 230,                   // 右邊開始 水平接續產生
        ReHorizontalD = 231,                 // 下方開始 垂直產生
        ReHorizontalU = 232,                 // 上方開始 垂直產生
        ReLinkHorD = 233,                    // 下方開始 垂直接續產生
        ReLinkHorU = 234,                    // 上方開始 垂直接續產生
        ReHorTwin = 235,                     // 垂直生2個
        ReVertTwin = 236,                    // 水平生2個
        ReLinkHorTwin = 237,                 // 垂直接續產生 每次2個 到最後
        ReLinkVertTwin = 238,                // 水平接續產生 每次2個 到最後

        ReTriangleLD = 239,                   // 左下開始 三角形
        ReTriangleRD = 240,                   // 右下開始 三角形
        ReTriangleLU = 241,                   // 左上開始 三角形
        ReTriangleRU = 242,                   // 右上開始 三角形
        ReBevelL = 243,                       // 左邊開始 45度斜角
        ReBevelR = 244,                       // 右邊開始 45度斜角

        ByNum = 1,                        // 一次產生 多少數量
        Random = 2,                       // 隨機
    }

    public enum MissionMode : byte
    {
        Closed = 0,
        Open = 1,
        Opening = 2,
        Completing = 3,
        Completed = 4,
    }

    public enum Mission : byte
    {
        None=0,                 // 沒任務
        Harvest = 1,            // 達成XX收穫
        Reduce = 2,             // 減少XX收穫
        DrivingMice = 3,        // 驅趕XX老鼠
        HarvestRate = 4,          // 收穫倍率
        BadMice = 5,              // 壞老鼠(不能打)
        Exchange = 6,             // 交換收穫
        WorldBoss = 7,            // 區域王
    }

    public enum BattleOperationCode
    {
        ExitRoom = 41,              // 戰鬥中途離開房間
        KickOther = 42,               // 踢人
        CheckStatus = 43,             // 檢查玩家遊戲狀態
        SendSkill = 44,            // 玩者A發動技能 攻擊 玩者B
        UpdateScore = 52,           // 更新分數
        Mission = 53,                // 任務
        MissionCompleted=54,        // 任務完成
        BossDamage=57,              // BOSS傷害
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
        Mission,                    // 任務
        MissionRate,                // 任務倍率
        MissionScore,               // 任務目標
        CustomValue,                // 自訂參數1(整數)
        CustomString,                // 自訂參數2(字串)
        MissionReward,              // 任務獎勵
        GoldReward,                 // 金幣獎勵
        SliverReward,               // 銀幣獎勵(遊戲幣、糧食)
        EXPReward,                  // 經驗獎勵(遊戲幣、糧食)
    }

    public enum BattleResponseCode
    {
        ExitRoom = 41,              // 離開房間
        KickOther = 42,             // 踢人
        DebugMessage = 45,          // OnEvent除錯訊息
        Online = 46,                // 線上
        Offline = 47,               // 離線
        Damage = 48,                // 造成傷害
        ApplySkill = 49,            // 接收 技能攻擊
        GetScore = 51,              // 取得另一位玩家分數
        UpdateScore = 52,           // 更新分數
        Mission = 53,               // 任務
        MissionCompleted = 54,      // 任務完成
        GetMissionScore = 55,       // 取得對方任務分數
        Reward = 56,                // 獎勵
        BossDamage = 57,            // BOSS傷害
    }
}
