using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
 * 
 * 
 * ***************************************************************/
namespace MPCOM
{
    [Serializable()]
    public struct ItemData
    {
        public string ReturnCode;               // 回傳值
        public string ReturnMessage;            // 回傳說明文字<30全形字

        public string ItemName;                 // 道具名稱
        public int Price;                       // 價格
        public byte ItemType;                   // 道具類型
        public Int16 Limit;                       // 限量
        public DateTime LimitTime;              // 限時
        public int BuyCount;                    // 購買數量
    }
}
