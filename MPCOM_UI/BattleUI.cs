using System;
using System.EnterpriseServices;
using Gansol;
using MPProtocol;
using System.Collections.Generic;

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
 * 對戰界面層 提供外部存取使用
 * 計算分數
 * 
 * ***************************************************************/

namespace MPCOM
{
    public interface IBattleUI  // 使用介面 可以提供給不同程式語言繼承使用        
    {
        byte[] ClacScore(short miceID, float aliveTime, float scoreRate, int combo, float energyRate);
        byte[] ClacMissionReward(byte mission, float missionRate, Int16 customVaule);
        byte[] SelectMission(byte mission, float missionRate);
        byte[] GameOver(string account, short score, short otherScore, short gameTime, int lostMice, short totalScore, short spawnCount, short missionCompletedCount, short maxMissionCount, short combo , string JMicesUseCount,string jItemsUseCount, string[] columns);
        byte[] UpdateScoreRate(ENUM_Rate rate);
    }

    public class BattleUI : ServicedComponent, IBattleUI
    {
        protected override bool CanBePooled()
        {
            return true;
        }

        /// <summary>
        /// 更新分數倍率
        /// </summary>
        /// <param name="rate"></param>
        /// <returns></returns>
        public byte[] UpdateScoreRate(ENUM_Rate rate)
        {
            BattleData battleData = new BattleData();
            battleData.ReturnCode = "S500";
            battleData.ReturnMessage = "";

            try
            {
                BattleLogic battleLogic = new BattleLogic();
                battleData = battleLogic.UpdateScoreRate(rate);
            }
            catch (Exception e)
            {
                battleData.ReturnCode = "S599";
                battleData.ReturnMessage = "(UI)對戰資料未知例外情況！　" + e.Message + " 於: " + e.StackTrace; ;
            }
            return TextUtility.SerializeToStream(battleData);
        }


        /// <summary>
        /// 更新能量倍率
        /// </summary>
        /// <param name="rate"></param>
        /// <returns></returns>
        public byte[] UpdateEnergyRate(ENUM_Rate rate)
        {
            BattleData battleData = new BattleData();
            battleData.ReturnCode = "S500";
            battleData.ReturnMessage = "";

            try
            {
                BattleLogic battleLogic = new BattleLogic();
                battleData = battleLogic.UpdateEnergyRate(rate);
            }
            catch (Exception e)
            {
                battleData.ReturnCode = "S599";
                battleData.ReturnMessage = "(UI)對戰資料未知例外情況！　" + e.Message + " 於: " + e.StackTrace; ;
            }
            return TextUtility.SerializeToStream(battleData);
        }

        #region ClacScore 計算老鼠命中分數
        /// <summary>
        /// 計算老鼠命中分數
        /// </summary>
        /// <param name="miceID"></param>
        /// <param name="time"></param>
        /// <param name="eatingRate"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        public byte[] ClacScore(short miceID, float aliveTime, float scoreRate, int combo, float energyRate)
        {
            BattleData battleData = new BattleData();
            battleData.ReturnCode = "S500";
            battleData.ReturnMessage = "";

            try
            {
                BattleLogic battleLogic = new BattleLogic();
                battleData = battleLogic.ClacScore(miceID, aliveTime, scoreRate,combo, energyRate);
            }
            catch (Exception e)
            {
                battleData.ReturnCode = "S599";
                battleData.ReturnMessage = "(UI)對戰資料未知例外情況！　" + e.Message + " 於: " + e.StackTrace; ;
            }
            return TextUtility.SerializeToStream(battleData);
        }
        #endregion

        #region ClacMissionReward 計算任務完成的分數
        /// <summary>
        /// 計算任務完成的分數。
        /// </summary>
        /// <param name="mission">任務</param>
        /// <param name="missionRate">任務倍率</param>
        /// <param name="customVaule">自訂參數1</param>
        /// <returns>回傳任務獎勵</returns>
        public byte[] ClacMissionReward(byte mission, float missionRate,Int16 customVaule)
        {
            BattleData battleData = new BattleData();
            battleData.ReturnCode = "S500";
            battleData.ReturnMessage = "";

            try
            {
                BattleLogic battleLogic = new BattleLogic();
                battleData = battleLogic.ClacMissionReward(mission, missionRate, customVaule);
            }
            catch (Exception e)
            {
                battleData.ReturnCode = "S599";
                battleData.ReturnMessage = "(UI)對戰資料未知例外情況！　" + e.Message + " 於: "+e.StackTrace;
            }
            return TextUtility.SerializeToStream(battleData);
        }
        #endregion

        #region SelectMission 選擇任務
        /// <summary>
        /// 計算任務完成的分數
        /// </summary>
        /// <param name="mission"></param>
        /// <param name="missionRate"></param>
        /// <returns></returns>
        public byte[] SelectMission(byte mission, float missionRate)
        {
            BattleData battleData = new BattleData();
            battleData.ReturnCode = "S500";
            battleData.ReturnMessage = "";

            try
            {
                BattleLogic battleLogic = new BattleLogic();
                battleData = battleLogic.SelectMission(mission, missionRate);
            }
            catch (Exception e)
            {
                battleData.ReturnCode = "S599";
                battleData.ReturnMessage = "(UI)對戰資料未知例外情況！　" + e.Message + " 於: " + e.StackTrace;
            }
            return TextUtility.SerializeToStream(battleData);
        }
        #endregion



        /// <summary>
        /// 處理GameOver時事件
        /// </summary>
        /// <param name="account">帳號</param>
        /// <param name="score">分數</param>
        /// <param name="otherScore">對手分數</param>
        /// <param name="gameTime">遊戲時間</param>
        /// <param name="lostMice">遺漏的老鼠</param>
        /// <param name="totalScore">遊戲可獲得的最大分數</param>
        /// <param name="spawnCount">產生數量</param>
        /// <param name="missionCompletedCount">任務完成數</param>
        /// <param name="maxMissionCount">任務數量</param>
        /// <param name="combo">遊戲時最大Combo</param>
        /// <param name="dictClientMiceData">老鼠資料(Rank、Exp、UseCount)</param>
        /// <param name="columns">更新欄位</param>
        /// <returns></returns>
        public byte[] GameOver(string account, short score, short otherScore, short gameTime, int lostMice, short totalScore, short spawnCount, short missionCompletedCount, short maxMissionCount, short combo, string jMicesUseCount, string jItemsUseCount, string[] columns)
        {
            BattleData battleData = new BattleData();
            battleData.ReturnCode = "S500";
            battleData.ReturnMessage = "";

            try
            {
                BattleLogic battleLogic = new BattleLogic();
                battleData = battleLogic.GameOver(account, score, otherScore, gameTime, lostMice, totalScore, spawnCount, missionCompletedCount, maxMissionCount, combo, jMicesUseCount, jItemsUseCount, columns);
            }
            catch (Exception e)
            {
                battleData.ReturnCode = "S599";
                battleData.ReturnMessage = "(UI)對戰資料未知例外情況！　" + e.Message + " 於: " + e.StackTrace;
            }
            return TextUtility.SerializeToStream(battleData);
        }
    }
}
