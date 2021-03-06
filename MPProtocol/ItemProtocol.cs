﻿namespace MPProtocol
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
