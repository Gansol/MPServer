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
        byte[] UpdatePlayerData(string account, byte rank, byte exp, Int16 maxCombo, int maxScore, int sumScore, string miceAll, string team, string miceAmount, string friend);
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

        #region UpdatePlayerData 更新玩家資料
        public byte[] UpdatePlayerData(string account, byte rank, byte exp, Int16 maxCombo, int maxScore, int sumScore, string miceAll, string team, string miceAmount, string friend)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "S400";
            playerData.ReturnMessage = "";

            try
            {
                PlayerDataLogic playerDataLogic = new PlayerDataLogic();
                playerData = playerDataLogic.UpdatePlayerData(account, rank, exp, maxCombo, maxScore, sumScore, miceAll, team, miceAmount, friend);
            }
            catch (Exception e)
            {
                playerData.ReturnCode = "S499";
                playerData.ReturnMessage ="(UI)玩家資料未知例外情況！　"+ e.Message;
                throw e;
            }
            return TextUtility.SerializeToStream(playerData);
        }
        #endregion

    }
}
