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
        byte[] ClacScore(byte miceID, float time, float eatingRate, Int16 score);
    }

    public class BattleUI : ServicedComponent, IBattleUI
    {
        protected override bool CanBePooled()
        {
            return true;
        }

        #region ClacScore
        public byte[] ClacScore(byte miceID, float time,float eatingRate, Int16 score)
        {
            BattleData battleData = new BattleData();
            battleData.ReturnCode = "S500";
            battleData.ReturnMessage = "";

            try
            {
                BattleLogic battleLogic = new BattleLogic();
                battleData = battleLogic.ClacScore(miceID, time,eatingRate, score);
            }
            catch (Exception e)
            {
                battleData.ReturnCode = "S599";
                battleData.ReturnMessage = "(UI)對戰資料未知例外情況！　" + e.Message;
            }
            return TextUtility.SerializeToStream(battleData);
        }
        #endregion
    }
}
