using System;
using System.EnterpriseServices;

/* ***************************************************************
 * -----Copyright © 2015 Gansol Studio.  All Rights Reserved.-----
 * -----------            CC BY-NC-SA 4.0            -------------
 * -----------  @Website:  EasyUnity@blogspot.com    -------------
 * -----------  @Email:    GansolTW@gmail.com        -------------
 * -----------  @Author:   Krola.                    -------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * 
 * 這個檔案是用來進行判斷對戰邏輯 驗證資料所使用
 * 計算分數
 * >>try catch 要移除 使用AutoComplete就可 移除後刪除
 * >>邏輯都沒寫
 * 
 * ***************************************************************/

namespace MPCOM
{
    // TransactionOption 指定元件要求的自動交易類型。
    // NotSupported	沒有使用支配性的交易在內容中建立元件。
    // Required	共用交易 (如果存在的話)，並且建立新交易 (如果有必要的話)。
    // RequiresNew	不論目前內容的狀態如何，都使用新交易建立元件。
    // Supported	共用交易 (如果有存在的話)。
    [Transaction(TransactionOption.Required)]
    public class BattleLogic : ServicedComponent// ServicedComponent 表示所有使用 COM+ 服務之類別的基底類別。
    {
        BattleData battleData = new BattleData();

        protected override bool CanBePooled()
        {
            return true;
        }

        #region ClacScore 計算分數

        [AutoComplete]
        public BattleData ClacScore(byte miceID, float time, float eatingRate, Int16 score)
        {
            battleData.ReturnCode = "(Logic)S500";
            battleData.ReturnMessage = "";

            try
            {
                switch (miceID) //用老鼠ID來判斷 取得的分數是否異常
                {
                    case 1: //EggMice
                        {
                            //to do verification
                            if (score > 100) //這裡亂寫的 要想驗證方法
                            {
                                battleData.ReturnCode = "S502";
                                battleData.ReturnMessage = "驗證分數失敗！";
                                return battleData;
                            }
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            battleData.score = score;
            battleData.ReturnCode = "S501";
            battleData.ReturnMessage = "驗證分數成功！";
            return battleData;
        }

        #endregion

    }
}
