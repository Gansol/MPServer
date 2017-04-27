namespace MPProtocol
{
    public enum ENUM_BattleAIState : byte
    {
        Random = 0,
        EasyMode = 1,
        NormalMode = 2,
        HardMode = 3,
        CarzyMode = 4,
        MissionMode = 5,
        EndTimeMode = 6,
        HelpMode = 7,
    }

    public enum SpawnStatus : byte
    {

        ByNum = 1,                        // 一次產生 多少數量
        Random = 2,                       // 隨機

        LineHorA = 3,
        LineHorB = 4,
        LineHorC = 5,
        LineHorD = 6,
        LineVertA = 7,
        LineVertB = 8,
        LineVertC = 9,

        // 1D 方式
        SpawnData1D = 10,                   // 起始值
        LineL = 11,                        // 左到右 直線產生
        LineR = 12,                        // 右到左 直線產生
        LinkLineL = 13,                    // 左到右 > 右到左 接續產生
        LinkLineR = 14,                    // 右到左 > 左到右 接續產生
        CircleLD = 15,                     // 左下開始 繞圈方式產生
        CircleRU = 16,                     // 右上開始 繞圈方式產生
        BevelL = 17,                       // 左邊開始 45度斜角
        BevelR = 18,                       // 右邊開始 45度斜角
        FourPoint = 19,                    // 上下左右 4個點
        STwin = 20,                        // S產生 2組

        // 2D 方式
        SpawnData2D = 100,                  // 起始值
        VerticalL = 101,                    // 左邊開始 水平產生
        VerticalR = 102,                    // 右邊開始 水平產生
        LinkVertL = 103,                    // 左邊開始 水平接續產生
        LinkVertR = 104,                    // 右邊開始 水平接續產生
        HorizontalD = 105,                  // 下方開始 垂直產生
        HorizontalU = 106,                  // 上方開始 垂直產生
        LinkHorD = 107,                     // 下方開始 垂直接續產生
        LinkHorU = 108,                     // 上方開始 垂直接續產生
        HorTwin = 109,                      // 垂直生2個
        VertTwin = 110,                     // 水平生2個

        //// 不規則
        //SpawnDataCustom = 200,              // 起始值
        //TriangleLD = 201,                   // 左下開始 三角形
        //TriangleRD = 202,                   // 右下開始 三角形
        //TriangleLU = 203,                   // 左上開始 三角形
        //TriangleRU = 204,                   // 右上開始 三角形


    }



    public enum ENUM_SpawnMethod
    {
        CrossHor = 0,
        CrossVert = 1,
        Feather = 2,
        Door = 3,

        STwin = 4,
        Fish = 5,
        Snake = 6,
        Swim = 7,

        LoopCricle = 8,
        Cross = 9,
        BillingHV = 10,
        BillingX = 11,
    }

    public enum ENUM_Rate
    {
        Normal = 0,
        Low = 1,
        High = 2,
    }

    public enum ENUM_Difficulty
    {
        Easy = 0,
        Normal = 1,
        Hard = 2,
        Carzy = 3,
        Endless = 4,
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
        None = 0,               // 沒任務
        Harvest = 1,            // 達成XX收穫
        Reduce = 2,             // 減少XX收穫
        DrivingMice = 3,        // 驅趕XX老鼠
        HarvestRate = 4,        // 收穫倍率
        BadMice = 5,            // 壞老鼠(不能打)
        Exchange = 6,           // 交換收穫
        WorldBoss = 7,          // 區域王
    }

    public enum BattleOperationCode
    {
        ExitRoom = 41,              // 戰鬥中途離開房間
        KickOther = 42,             // 踢人
        CheckStatus = 43,           // 檢查玩家遊戲狀態
        SendSkillMice = 44,         // 發動老鼠技能
        SendSkillItem = 45,         // 發動道具技能
        Damage = 48,                 // 造成傷害
        UpdateScore = 52,           // 更新分數
        Mission = 53,               // 任務
        MissionCompleted = 54,      // 任務完成
        BossDamage = 57,            // BOSS傷害
        GameOver = 58,              // 遊戲結束
        UpdateScoreRate = 59,       // 更新分數倍率
        UpdateEnergyRate = 60,      // 更新分數倍率
        SkillBoss = 61,             // 技能老鼠發動技能判斷

    }

    public enum BattleParameterCode
    {
        // 1~30
        Ret = 0,                        // 回傳值
        Damage = 1,                     // 技能傷害
        RoomID = 2,                     // 房間ID
        PrimaryID = 3,                  // 主索引
        Account = 4,                    // 帳號
        OtherScore = 5,                 // 另一位玩家分數
        Score = 6,                      // 分數
        Time = 7,                       // 時間
        MiceID = 8,                     // 老鼠ID
        MiceName = 9,                   // 老鼠名稱
        EatingRate = 10,                // 咀嚼頻率
        Mission = 11,                   // 任務
        MissionRate = 12,               // 任務倍率
        MissionScore = 13,              // 任務目標
        CustomValue = 14,               // 自訂參數1(整數)
        CustomString = 15,              // 自訂參數2(字串)
        MissionReward = 16,             // 任務獎勵
        GoldReward = 17,                // 金幣獎勵
        SliverReward = 18,              // 銀幣獎勵(遊戲幣、糧食)
        EXPReward = 19,                 // 經驗獎勵(遊戲幣、糧食)
        ItemReward = 20,                // 道具獎勵
        BattleResult = 21,              // 戰鬥結果
        ScoreRate = 22,                 // 分數倍率
        SkillType = 23,                 // 技能類型
        Energy = 24,                    // 能量
        EnergyRate = 25,                // 能量倍率
        SpawnCount = 26,                // 產生數量
        Evaluate = 27,                  // 評價
        Combo = 28,                     // COMBO
    }

    public enum BattleResponseCode
    {
        ExitRoom = 41,              // 離開房間
        KickOther = 42,             // 踢人
        DebugMessage = 45,          // OnEvent除錯訊息
        Online = 46,                // 線上
        Offline = 47,               // 離線
        Damage = 48,                // 造成傷害
        ApplySkillMice = 49,        // 接收 老鼠技能攻擊
        ApplySkillItem = 50,        // 接收 道具技能攻擊
        GetScore = 51,              // 取得另一位玩家分數
        UpdateScore = 52,           // 更新分數
        Mission = 53,               // 任務
        MissionCompleted = 54,      // 任務完成
        GetMissionScore = 55,       // 取得對方任務分數
        Reward = 56,                // 獎勵
        BossDamage = 57,            // BOSS傷害
        GameOver = 58,              // 遊戲結束
        UpdatedScoreRate = 59,      // 更新分數倍率
        UpdatedEnergyRate = 60,     // 更新能量倍率
        SkillBoss = 61,             // 技能老鼠發動技能判斷
    }
}
