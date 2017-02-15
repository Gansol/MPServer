namespace MPProtocol
{

    public enum ItemOperationCode
    {
        LoadItem = 82,
        LoadArmor = 83,
    }

    public enum ItemResponseCode
    {
        LoadItem = 82,
        LoadArmor = 83,
    }

    public enum ItemParameterCode
    {
        Ret=0,
        ItemData=88,
        ItemID = 1,
        ItemName = 2,
        EatingRate = 3,
        MiceSpeed = 4,
        EatFull = 5,
        BossSkill = 6,
        HP = 7,
        MiceCost = 8,
    }

    public enum ItemProperty
    {
        ItemID=0,
        ItemName = 1,
        EatingRate = 2,
        MiceSpeed = 3,
        EatFull = 4,
        BossSkill = 5,
        HP = 6,
        MiceCost = 7,
    }
}
