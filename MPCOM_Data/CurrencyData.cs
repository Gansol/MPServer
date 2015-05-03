using System;

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
 * 這個檔案是用來儲存COM+ 貨幣資料所使用 包含 遊戲幣、金幣、轉蛋
 * 
 * ***************************************************************/

namespace MPCOM
{
    [Serializable()]
    public struct CurrencyData
    {
        public string ReturnCode;               // 回傳值
        public string ReturnMessage;            // 回傳說明文字<30全形字

        public int Rice;		                // 遊戲幣21e
        public Int16 Gold;		                // 金幣65536
        public byte goldMiceEgg;                // 老鼠蛋數量255
        public byte silverMiceEgg;              // 老鼠蛋數量255
    }
}
