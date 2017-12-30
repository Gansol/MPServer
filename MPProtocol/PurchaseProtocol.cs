namespace MPProtocol
{
    public enum PurchaseOperationCode
    {
        Load = 131,                      // 載入貨幣資料
        Confirm = 132,                  // 確認購買商品
    }

    public enum PurchaseParameterCode
    {
        PurchaseItem =0,                // 法幣道具資料
        PurchaseID ,                    // 法幣道具資料
        CurrencyCode,                   // 法幣符號
        CurrencyValue,                  // 法幣價值
        PurchaseName,                   // 法幣商品名稱
        Receipt,                        // 交易碼
        ReceiptCipheredPayload,          // Google訂單
        Description,                    // 備註說明
    }

    public enum PurchaseResponseCode
    {
        Loaded = 131,                     // 載入完成
        Confirmed = 132,                  // 確認購買商品
    }
}
