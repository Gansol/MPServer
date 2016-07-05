using System;
using System.EnterpriseServices;
using Gansol;

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
        byte[] ClacScore(string miceName, float aliveTime);
        byte[] ClacMissionReward(byte mission, float missionRate, Int16 customVaule);
        byte[] SelectMission(byte mission, float missionRate);
        byte[] GameOver(Int16 score,Int16 otherScore, Int16 gameTime, Int16 lostMice);
    }

    public class BattleUI : ServicedComponent, IBattleUI
    {
        protected override bool CanBePooled()
        {
            return true;
        }

        #region ClacScore 計算老鼠命中分數
        /// <summary>
        /// 計算老鼠命中分數
        /// </summary>
        /// <param name="miceName"></param>
        /// <param name="time"></param>
        /// <param name="eatingRate"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        public byte[] ClacScore(string miceName, float aliveTime)
        {
            BattleData battleData = new BattleData();
            battleData.ReturnCode = "S500";
            battleData.ReturnMessage = "";

            try
            {
                BattleLogic battleLogic = new BattleLogic();
                battleData = battleLogic.ClacScore(miceName, aliveTime);
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




        public byte[] GameOver(short score,short otherScore, short gameTime, short lostMice)
        {
            BattleData battleData = new BattleData();
            battleData.ReturnCode = "S500";
            battleData.ReturnMessage = "";

            try
            {
                BattleLogic battleLogic = new BattleLogic();
                battleData = battleLogic.GameOver(score,otherScore, gameTime,lostMice);
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
