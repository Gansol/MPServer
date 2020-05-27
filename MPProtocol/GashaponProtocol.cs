namespace MPProtocol
{
    public enum GashaponOperationCode
    {
        LoadGashapon = 141,                 // 載入轉蛋資料
        BuyGashapon = 142,                  // 購買轉蛋
    }

    public enum GashaponParameterCode
    {
        Ret,                            // 回傳碼 
        Series,
    }

    public enum GashaponResponseCode
    {
        LoadGashapon = 141,                 // 載入轉蛋資料
        BuyGashapon = 142,                  // 購買轉蛋
    }

    public enum ENUM_GashaponSeries
    {
        AllSeries = 0,
        Classic,                        // 經典
        NewYear_2018,                  // 新年(2018)
        Spring_2018,                   // 春天(2018)
        Summer_2018,                   // 夏天(2018)
        Pumpkin_2018,                  // 萬聖節(2018)
        Xmas_2018,                     // 聖誕節(2018)
    }

    public enum ENUM_GashaponGrade
    {
        N = 1,                       // 普卡
        R,                           // 精良卡
        SR,                          // 稀有卡
        SSR,                         // 超級稀有卡
        UR,                         // 傳說卡
    }
}
