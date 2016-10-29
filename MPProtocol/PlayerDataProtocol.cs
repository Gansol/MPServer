namespace MPProtocol
{
    public enum PlayerItem : short
    {
        ItemID = 0,
        ItemCount = 1,
        ItemType = 2,
        IsEquip = 3,
        UseCount = 4,
    }

    public enum PlayerDataOperationCode
    {
        LoadPlayer = 61,          // 載入玩家資料
        UpdatePlayer = 62,        // 更新玩家資料
        UpdateMice = 63,        // 更新老鼠資料
        LoadItem = 64,          // 載入道具資料
        UpdateItem = 65,        // 更新道具資料
        SortItem = 66,          // 排序道具資料
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
        SortedItem = 30,
        MiceAll = 31,
        Team = 32,
        MiceAmount = 33,
        Friend = 34,
        PlayerItem = 35,
        Equip = 36,
        Count = 37,
        UseCount = 38,
    }

    public enum PlayerDataResponseCode
    {
        LoadedPlayer = 61,          // 載入玩家資料完成
        UpdatedPlayer = 62,         // 更新玩家資料完成
        UpdatedMice = 63,           // 更新老鼠資料完成
        LoadedItem = 64,            // 載入道具資料完成
        UpdatedItem = 65,           // 更新道具資料完成
        SortedItem = 66,            // 排序道具資料完成
    }
}
