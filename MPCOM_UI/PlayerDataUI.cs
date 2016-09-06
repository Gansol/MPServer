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
 * 玩家資料界面層 提供外部存取使用
 * 載入玩家資料、更新玩家資料
 * 
 * ***************************************************************/

namespace MPCOM
{
    public interface IPlayerDataUI  // 使用介面 可以提供給不同程式語言繼承使用  
    {
        byte[] LoadPlayerData(string account);
        byte[] UpdatePlayerData(string account, byte rank, byte exp, Int16 maxCombo, int maxScore, int sumScore, Int16 sumLost, int sumKill, string item, string miceAll, string team, string miceAmount, string friend);
        byte[] UpdatePlayerData(string account, Int16 score, byte exp, Int16 maxCombo, int maxScore, Int16 lostMice, int killMice, string item, string miceAmount);
        byte[] UpdatePlayerData(string account, string miceAll, string miceAmount, string miceName, int amount);
        byte[] UpdatePlayerData(string account, string miceAll, string team, string miceAmount);
    }

    public class PlayerDataUI : ServicedComponent, IPlayerDataUI    // 使用介面 可以提供給不同程式語言繼承使用  
    {
        protected override bool CanBePooled()
        {
            return true;
        }

        #region LoadPlayerData 載入玩家資料
        public byte[] LoadPlayerData(string account)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "S400";
            playerData.ReturnMessage = "";

            try
            {
                PlayerDataLogic playerDataLogic = new PlayerDataLogic();
                playerData = playerDataLogic.LoadPlayerData(account);
            }
            catch (Exception e)
            {
                playerData.ReturnCode = "S400";
                playerData.ReturnMessage = e.Message;
                throw e;
            }
            return TextUtility.SerializeToStream(playerData);
        }
        #endregion

        #region UpdatePlayerData 更新玩家(全部)資料
        /// <summary>
        /// 更新 玩家全部資料
        /// </summary>
        public byte[] UpdatePlayerData(string account, byte rank, byte exp, Int16 maxCombo, int maxScore, int sumScore, Int16 sumLost, int sumKill, string item, string miceAll, string team, string miceAmount, string friend)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "S400";
            playerData.ReturnMessage = "";

            try
            {
                PlayerDataLogic playerDataLogic = new PlayerDataLogic();
                playerData = playerDataLogic.UpdatePlayerData(account, rank, exp, maxCombo, maxScore, sumScore, sumLost, sumKill, item, miceAll, team, miceAmount, friend);
            }
            catch (Exception e)
            {
                playerData.ReturnCode = "S499";
                playerData.ReturnMessage = "(UI)玩家資料未知例外情況！　" + e.Message;
                throw e;
            }
            return TextUtility.SerializeToStream(playerData);
        }
        #endregion

        #region UpdatePlayerData 更新玩家(GameOver時)資料
        /// <summary>
        /// 更新 玩家(GameOver時)資料
        /// </summary>
        public byte[] UpdatePlayerData(string account, Int16 score, byte exp, Int16 maxCombo, int maxScore, Int16 lostMice, int killMice, string item, string miceAmount)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "S400";
            playerData.ReturnMessage = "";

            try
            {
                PlayerDataLogic playerDataLogic = new PlayerDataLogic();
                playerData = playerDataLogic.UpdatePlayerData(account, score, exp, maxCombo, maxScore, lostMice, killMice, item, miceAmount);
            }
            catch (Exception e)
            {
                playerData.ReturnCode = "S499";
                playerData.ReturnMessage = "(UI)玩家資料未知例外情況！　" + e.Message;
                throw e;
            }
            return TextUtility.SerializeToStream(playerData);
        }
        #endregion

        #region UpdatePlayerData 更新玩家老鼠資料
        /// <summary>
        /// 更新玩家老鼠資料
        /// </summary>
        /// <param name="account"></param>
        /// <param name="miceAll"></param>
        /// <param name="miceAmount"></param>
        /// <param name="miceName">老鼠名稱</param>
        /// <param name="amount">數量</param>
        /// <returns></returns>
        public byte[] UpdatePlayerData(string account, string miceAll, string miceAmount, string miceName, int amount)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "S400";
            playerData.ReturnMessage = "";

            try
            {
                PlayerDataLogic playerDataLogic = new PlayerDataLogic();
                playerData = playerDataLogic.UpdatePlayerData(account, miceAll, miceAmount, miceName,amount);
            }
            catch (Exception e)
            {
                playerData.ReturnCode = "S499";
                playerData.ReturnMessage = "(UI)玩家資料未知例外情況！　" + e.Message;
                throw e;
            }
            return TextUtility.SerializeToStream(playerData);
        }
        #endregion

        #region UpdatePlayerData 更新玩家(TeamUpdate時)資料
        /// <summary>
        /// 更新 玩家(TeamUpdate時)資料
        /// </summary>
        public byte[] UpdatePlayerData(string account, string miceAll, string team, string miceAmount)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "S400";
            playerData.ReturnMessage = "";

            try
            {
                PlayerDataLogic playerDataLogic = new PlayerDataLogic();
                playerData = playerDataLogic.UpdatePlayerData(account, miceAll, team, miceAmount);
            }
            catch (Exception e)
            {
                playerData.ReturnCode = "S499";
                playerData.ReturnMessage = "(UI)玩家資料未知例外情況！　" + e.Message;
                throw e;
            }
            return TextUtility.SerializeToStream(playerData);
        }
        #endregion
    }
}
