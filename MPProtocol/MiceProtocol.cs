namespace MPProtocol
{
    public enum ENUM_MiceState
    {
        Alive,
        Skilling,
        Frie,
        Frozen,
        Dizzy,
        Slow,
        Dead,
    }

    public enum MiceOperationCode
    {
        LoadMice = 101,             // 載入老鼠資料
        UpdateMice = 102,           // 更新老鼠資料
    }

    public enum MiceParameterCode
    {
        Ret=0,                      // 回傳碼
        MiceData=1,                 // 老鼠資料
    }

    public enum MiceResponseCode
    {
        LoadMice = 101,             // 載入老鼠資料
        UpdateMice = 102,           // 更新老鼠資料
    }


}
