namespace MPProtocol
{
    public enum CurrencyOperationCode
    {
        Load = 71,                      // 載入貨幣資料
        LoadRice = 72,                  // 載入遊戲幣資料
        LoadGold = 73,                  // 載入金幣資料
        Update = 74,                    // 更新貨幣資料
    }

    public enum CurrencyParameterCode
    {
        Ret,                            // 回傳碼
        Account,                        // 帳號
        Rice,                           // 遊戲幣
        Gold,                           // 金幣
        Bonus,                          // 紅利   // 目前紅利採金幣模式 與金幣共用優先使用紅利 使用者看不到 未來新增紅利再修正
        FitCurrency,                    // 法幣
       
    }

    public enum CurrencyResponseCode
    {
        Loaded = 75,                     // 載入完成
        Updated = 76,                     // 更新完成
    }

    public enum CurrencyType
    {
        Rice = 0,                       // 遊戲幣
        Gold,                           // 金幣
        Bonus,                          // 紅利   // 目前紅利採金幣模式 與金幣共用優先使用紅利 使用者看不到 未來新增紅利再修正
        FitCurrency,                    // 法幣
    }
}
