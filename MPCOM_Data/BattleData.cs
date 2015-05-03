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
 * 這個檔案是用來儲存COM+ 對戰資料所使用
 * 
 * ***************************************************************/

namespace MPCOM
{
    [Serializable()]
    public struct BattleData
    {
        public string ReturnCode;           // 回傳值
        public string ReturnMessage;        // 回傳說明文字<30全形字

        public Int16 score;                 // 分數

    }
}
