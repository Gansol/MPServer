namespace MPProtocol
{
    public enum ENUM_PlayerState
    {
        None = 0,
        ScorePlus = 1 << 0,
        EnergyPlus = 1 << 1,
        Freeze = 1 << 2,
        Burn = 1 << 3,
        Protected = 1 << 4,
        Reflection = 1 << 5,
        Invincible = 1 << 6,
        IceGlasses = 1 << 7,
        Boss = 1 << 8,
        WorldBoss = 1 << 9,

        All = -1,
    }

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
        LoadPlayer = 121,          // 載入玩家資料
        UpdatePlayer = 122,        // 更新玩家資料
        UpdateMice = 123,        // 更新老鼠資料
        LoadItem = 124,          // 載入道具資料
        UpdateItem = 125,        // 更新道具資料
        SortItem = 126,          // 排序道具資料
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
        LoadedPlayer = 121,          // 載入玩家資料完成
        UpdatedPlayer = 122,         // 更新玩家資料完成
        UpdatedMice = 123,           // 更新老鼠資料完成
        LoadedItem = 124,            // 載入道具資料完成
        UpdatedItem = 125,           // 更新道具資料完成
        SortedItem = 126,            // 排序道具資料完成
    }
}
