namespace MPProtocol
{
    public enum StoreOperationCode
    {
        LoadStore = 91,
        BuyItem = 92,
    }

    public enum StoreResponseCode
    {
        LoadStore = 91,
        BuyItem = 92,
    }

    public enum StoreParameterCode
    {
        Ret = 0,
        StoreData = 1,
    }

    public enum StoreProperty
    {
        ItemID = 0,
        ItemName = 1,
        Price = 2,
        CurrencyType = 3,
        ItemType = 4,
        PromotionsCount = 5,
        PromotionsTime = 6,
        LimitTime = 7,
        BuyCount = 8,
    }

    public enum StoreType
    {
        Mice = 1,
        Item = 2,
        Armor = 3,
        Gashapon = 9,
    }
}
