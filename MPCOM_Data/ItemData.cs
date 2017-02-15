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
        public string itemProperty;             // 道具屬性

        public Int16 itemID;

    }
}
