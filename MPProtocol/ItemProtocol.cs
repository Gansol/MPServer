namespace MPProtocol
{

    public enum ItemOperationCode
    {
        BuyItem = 81,
    }

    public enum ItemResponseCode
    {
        BuyItem = 81,
    }

    public enum ItemParameterCode
    {
        ItemName = 84,
        ItemType = 85,
        BuyCount = 86,
        Limit= 87,
    }

    public enum ItemProperty
    {
        MiceName = 0,
        EatingRate = 1,
        MiceSpeed = 2,
        EatFull = 3,
        BossSkill = 4,
        BossHP = 5,
        MiceCost = 6,
        Price = 7,
        Description =8,
        SayHello = 9,
        ItemType = 10,
        Limit = 11,
        BuyCount = 12,
    }
}
