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
    public struct GashaponData
    {
        public string ReturnCode;               // 回傳值
        public string ReturnMessage;            // 回傳說明文字<30全形字

        public Int16 Series;                 // 轉蛋系列類別
        public byte Grade;                   // 道具階級
        public int ItemID;                   // 道具ID
        public string ItemName;                   // 道具名稱
        public int Chance;                    // 機率

        public int SSR_Chance;                   // SSR機率
        public int SR_Chance;                   // SR機率
        public int R_Chance;                   // R機率
        public int N_Chance;                   // N機率

        public int SSR_Blance;                   //  SSR類別機率平衡
        public int SR_Blance;                   //  SR類別機率平衡
        public int R_Blance;                   //  R類別機率平衡
        public int N_Blance;                   //  N類別機率平衡
        public int Series_Blance;               // 系列機率平衡
    }
}
