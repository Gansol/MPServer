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
        public string jMiceResult;          // 遊戲結束老鼠使用量等級經驗計算結果
        public string jItemReward;          // 遊戲獎勵(道具(Item))
        public string evaluate;             // 評價

        public short score;                 // 分數
        public short totalScore;            // 分數
        public short energy;                // 能量
        public short spawnCount;            // 產生數量
        public short missionScore;          // 任務目標
        public short customValue;           // 自訂參數1(整數)
        public short missionReward;         // 任務獎勵
        public short sliverReward;          // 遊戲獎勵(銀幣(RICE))
        public short goldReward;            // 遊戲獎勵(金幣(Gold))
        public short expReward;             // 經驗獎勵
        public short battleResult;          // 戰鬥結果
        public short bossHP;                // 任務目標

        public float scoreRate;             // 分數倍率
        public float energyRate;            // 能量倍率
        public static readonly int GameTime = 150;  // 遊戲時間
    }
}
