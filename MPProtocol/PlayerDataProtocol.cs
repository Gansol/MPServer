namespace MPProtocol
{
    public enum PlayerDataOperationCode
    {
        Load = 61,          // 載入玩家資料
        Update = 62,        // 更新玩家資料
        UpdateMice = 63,    // 更新老鼠資料
    }

    public enum PlayerDataParameterCode
    {
        Ret = 0,
        Account = 20,
        Rank = 21,
        EXP = 22,
        MaxCombo = 23,
        MaxScore = 24,
        SumScore = 25,
        SumLost = 26,
        SumKill = 27,
        SumWin = 28,
        SumBattle = 29,
        Item = 30,
        MiceAll = 31,
        Team = 32,
        MiceAmount = 33,
        Friend = 34,
    }

    public enum PlayerDataResponseCode
    {
        Loaded = 64,
        Updated = 65,
        UpdatedMice = 66,
    }
}
