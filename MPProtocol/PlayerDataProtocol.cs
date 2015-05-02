namespace MPProtocol
{
    public enum PlayerDataOperationCode
    {
        Load = 61, // 載入玩家資料
        Update = 62, // 儲存玩家資料
    }

    public enum PlayerDataParameterCode
    {
        Ret,
        Account,
        Rank,
        EXP,
        MaxCombo,
        MaxScore,
        SumScore,
        MiceAll,
        Team,
        MiceAmount,
        Friend,
    }

    public enum PlayerDataResponseCode
    {
        Loaded =63,
        Saved=64,
    }
}
