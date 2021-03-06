﻿using System;

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
        public Int16 missionScore;          // 任務目標
        public Int16 customValue;           // 自訂參數1(整數)
        public string customString;           // 自訂參數2(字串)
        public Int16 missionReward;         // 任務獎勵
        public Int16 sliverReward;          // 遊戲獎勵(銀幣(RICE))
        public byte expReward;             // 經驗獎勵
        public byte battleResult;           // 戰鬥結果
    }
}
