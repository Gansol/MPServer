namespace MPProtocol
{
    public enum OperationCode
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

}
