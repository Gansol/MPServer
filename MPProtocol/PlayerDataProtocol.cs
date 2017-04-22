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
        ItemCount,
        ItemType ,
        IsEquip,
        UseCount ,
        Rank,
        Exp,
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
        // 30~50
        Ret = 0,
        Account = 30,
        Rank = 31,
        Exp = 32,
        MaxCombo = 33,
        MaxScore = 34,
        SumScore = 35,
        SumLost = 36,
        SumKill = 37,
        SumWin = 38,
        SumBattle = 39,
        SortedItem = 40,
        MiceAll = 41,
        Team = 42,
        MiceAmount = 43,
        Friend = 44,
        PlayerItem = 45,
        Equip = 46,
        Count = 47,
        UseCount = 48,
        Columns = 49,
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
