namespace MPProtocol
{
    public enum SystemOperationCode
    {
        //GetRoomInfo = 91,            // 取得單一房間資訊
        //GetAllRoomInfo = 92,         // 取得列表中所有房間資訊
    }

    public enum SystemParameterCode
    {
        //GetRoomInfo = 91,            // 取得單一房間資訊
        //GetAllRoomInfo = 92,         // 取得列表中所有房間資訊
        Ret = 0,
        OnlineActors,
        ActorsState,
    }

    public enum SystemResponseCode
    {
        //GetRoomInfo = 91,            // 取得單一房間資訊
        //GetAllRoomInfo = 92,         // 取得列表中所有房間資訊
    }

    public enum ErrorCode
    {
        Ok = 1,                     // 伺服器處理完成 正常回傳
        InvalidOperation=2,         // 未知的傳輸碼
        InvalidParameter=3,         // 沒有參數
        CustomError=4,              // 自訂錯誤
    }

    public enum ENUM_Data : int
    {
        None = 1,
        PlayerData = 2,
        PlayerItem = 3,
        CurrencyData = 5,
        ItemData = 7,
        StoreData = 11,
        FriendsData = 13,
        Purchase = 17, 
        //19 23 29 31 37 41
    }
}
