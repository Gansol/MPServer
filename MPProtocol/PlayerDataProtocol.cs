namespace MPProtocol
{
    public enum PlayerDataOperationCode
    {
        Load = 61, // 載入玩家資料
        Update = 62, // 儲存玩家資料
    }

    public enum PlayerDataParameterCode
    {
        Ret=0,
        Account=20,
        Rank=21,
        EXP=22,
        MaxCombo=23,
        MaxScore=24,
        SumScore=25,
        SumLost=26,
        SumKill=27,
        Item=28,
        MiceAll=29,
        Team=30,
        MiceAmount=31,
        Friend=32,
    }

    public enum PlayerDataResponseCode
    {
        Loaded =63,
        Saved=64,
    }
}
