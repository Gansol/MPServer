using System;
using System.EnterpriseServices;
using Gansol;
using System.Collections.Generic;
using System.Linq;

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
        byte[] LoadPlayerData(string account, string[] columns);

        byte[] LoadPlayerItem(string account);
        byte[] LoadPlayerItem(string account, Int16 itemID);
        byte[] UpdatePlayerData(string account, byte rank, short exp, Int16 maxCombo, int maxScore, int sumScore, int sumLost, int sumKill, string item, string miceAll, string team, string friend);
        byte[] UpdateGameOver(string account, int score, short exp, Int16 maxCombo, int maxScore, int lostMice, int killMice, int battleResult, string item);
        byte[] UpdatePlayerData(string account, string miceAll, string miceName, int amount);
        byte[] UpdatePlayerData(string account, string miceAll, string team);

        byte[] UpdatePlayerData(string account, object imageName);
        byte[] UpdatePlayerItem(string account, Int16 itemID, string itemName, byte itemType, Int16 itemCount);
        byte[] UpdatePlayerItem(string account, string jItemUsage, string[] columns);
        byte[] UpdatePlayerItem(string account, Int16 itemID, bool isEquip);
        byte[] SortPlayerItem(string account, string jString);

        byte[] LoadFriendsData(string[] friends);
        byte[] RemoveFriend(string account,string friend);
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

        #region LoadPlayerData 載入特定欄位資料
        /// <summary>
        /// 載入特定欄位資料
        /// </summary>
        /// <param name="account"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public byte[] LoadPlayerData(string account, string[] columns)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "(Logic)S400";
            playerData.ReturnMessage = "";

            try
            {
                //to do check 
                PlayerDataLogic playerDataLogic = new PlayerDataLogic();
                playerData = playerDataLogic.LoadPlayerData(account, columns.ToList());
            }
            catch
            {
                throw;
            }
            return TextUtility.SerializeToStream(playerData);
        }
        #endregion

        #region LoadPlayerItem 載入玩家道具(全部)資料
        /// <summary>
        /// 載入玩家道具(全部)資料
        /// </summary>
        public byte[] LoadPlayerItem(string account)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "S400";
            playerData.ReturnMessage = "";

            try
            {
                PlayerDataLogic playerDataLogic = new PlayerDataLogic();
                playerData = playerDataLogic.LoadPlayerItem(account);
            }
            catch (Exception e)
            {
                playerData.ReturnCode = "S499";
                playerData.ReturnMessage = e.Message;
                throw e;
            }
            return TextUtility.SerializeToStream(playerData);
        }
        #endregion

        #region LoadPlayerItem 載入玩家道具(單筆)資料
        /// <summary>
        /// 載入玩家道具(全部)資料
        /// </summary>
        public byte[] LoadPlayerItem(string account, Int16 itemID)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "S400";
            playerData.ReturnMessage = "";

            try
            {
                PlayerDataLogic playerDataLogic = new PlayerDataLogic();
                playerData = playerDataLogic.LoadPlayerItem(account, itemID);
            }
            catch (Exception e)
            {
                playerData.ReturnCode = "S499";
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
        public byte[] UpdatePlayerData(string account, byte rank, short exp, Int16 maxCombo, int maxScore, int sumScore, int sumLost, int sumKill, string item, string miceAll, string team, string friend)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "S400";
            playerData.ReturnMessage = "";

            try
            {
                PlayerDataLogic playerDataLogic = new PlayerDataLogic();
                playerData = playerDataLogic.UpdatePlayerData(account, rank, exp, maxCombo, maxScore, sumScore, sumLost, sumKill, item, miceAll, team, friend);
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

        #region UpdatePlayerData 更新玩家(好友)資料
        /// <summary>
        /// 更新玩家(好友)資料 含載入朋友資料
        /// </summary>
        /// <param name="account"></param>
        /// <param name="friend"></param>
        /// <returns></returns>
        [AutoComplete]
        public byte[] UpdatePlayerData(string account, string friend)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "S400";
            playerData.ReturnMessage = "";

            try
            {
                PlayerDataLogic playerDataLogic = new PlayerDataLogic();
                playerData = playerDataLogic.UpdatePlayerData(account, friend);
            }
            catch
            {
                playerData.ReturnCode = "S499";
                playerData.ReturnMessage = "(UI)玩家資料未知例外情況！　";
                throw;
            }
            return TextUtility.SerializeToStream(playerData);
        }
        #endregion

        #region UpdatePlayerData 更新玩家(GameOver時)資料
        /// <summary>
        /// 更新 玩家(GameOver時)資料
        /// </summary>
        public byte[] UpdateGameOver(string account, int score, short exp, Int16 maxCombo, int maxScore, int lostMice, int killMice, int battleResult, string item)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "S400";
            playerData.ReturnMessage = "";

            try
            {
                PlayerDataLogic playerDataLogic = new PlayerDataLogic();
                playerData = playerDataLogic.UpdateGameOver(account, score, exp, maxCombo, maxScore, lostMice, killMice, battleResult, item);
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
        /// <param name="miceName">老鼠名稱</param>
        /// <param name="amount">數量</param>
        /// <returns></returns>
        public byte[] UpdatePlayerData(string account, string miceAll, string miceName, int amount)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "S400";
            playerData.ReturnMessage = "";

            try
            {
                PlayerDataLogic playerDataLogic = new PlayerDataLogic();
                playerData = playerDataLogic.UpdatePlayerData(account, miceAll, miceName, amount);
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
        public byte[] UpdatePlayerData(string account, object imageName)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "S400";
            playerData.ReturnMessage = "";

            try
            {
                PlayerDataLogic playerDataLogic = new PlayerDataLogic();
                playerData = playerDataLogic.UpdatePlayerData(account, imageName);
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
        public byte[] UpdatePlayerData(string account, string miceAll, string team)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "S400";
            playerData.ReturnMessage = "";

            try
            {
                PlayerDataLogic playerDataLogic = new PlayerDataLogic();
                playerData = playerDataLogic.UpdatePlayerData(account, miceAll, team);
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

        #region UpdatePlayerItem 更新玩家(道具)資料
        /// <summary>
        /// 更新玩家(道具)資料
        /// </summary>
        /// <param name="account"></param>
        /// <param name="itemName"></param>
        /// <param name="itemType"></param>
        /// <param name="itemCount"></param>
        /// <returns></returns>
        public byte[] UpdatePlayerItem(string account, Int16 itemID, string itemName, byte itemType, Int16 itemCount)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "S400";
            playerData.ReturnMessage = "";

            try
            {
                PlayerDataLogic playerDataLogic = new PlayerDataLogic();
                playerData = playerDataLogic.UpdatePlayerItem(account, itemID, itemName, itemType, itemCount);
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

        #region UpdatePlayerItem 更新玩家(多筆道具)資料
        /// <summary>
        /// 更新玩家 多筆道具資料(使用量、經驗、等級)
        /// </summary>
        /// <param name="account"></param>
        /// <param name="jItemUsage"></param>
        /// <param name="columns">UseCount、Exp、Rank</param>
        /// <returns></returns>
        public byte[] UpdatePlayerItem(string account, string jItemUsage, string[] columns)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "S400";
            playerData.ReturnMessage = "";

            try
            {
                PlayerDataLogic playerDataLogic = new PlayerDataLogic();
                playerData = playerDataLogic.UpdatePlayerItem(account, jItemUsage, columns);
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

        #region UpdatePlayerItem 更新玩家(道具)裝備狀態
        /// <summary>
        /// 更新玩家(道具)裝備狀態
        /// </summary>
        /// <param name="account"></param>
        /// <param name="itemID"></param>
        /// <param name="isEquip"></param>
        /// <returns></returns>
        public byte[] UpdatePlayerItem(string account, Int16 itemID, bool isEquip)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "S400";
            playerData.ReturnMessage = "";

            try
            {
                PlayerDataLogic playerDataLogic = new PlayerDataLogic();
                playerData = playerDataLogic.UpdatePlayerItem(account, itemID, isEquip);
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

        #region SortPlayerItem 更新玩家(道具)裝備狀態
        /// <summary>
        /// 更新玩家(道具)裝備狀態
        /// </summary>
        /// <param name="account"></param>
        /// <param name="itemID"></param>
        /// <param name="isEquip"></param>
        /// <returns></returns>
        public byte[] SortPlayerItem(string account, string jString)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "S400";
            playerData.ReturnMessage = "";

            try
            {
                PlayerDataLogic playerDataLogic = new PlayerDataLogic();
                playerData = playerDataLogic.SortPlayerItem(account, jString);
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

        #region LoadFriendsData 取得好友資料
        /// <summary>
        /// 取得朋友資料
        /// </summary>
        /// <param name="accounts">朋友們的帳號</param>
        /// <returns></returns>
        public byte[] LoadFriendsData(string[] accounts)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "S100";
            playerData.ReturnMessage = "";

            try
            {
                PlayerDataLogic playerDataLogic = new PlayerDataLogic();
                playerData = playerDataLogic.LoadFriendsData(accounts.ToList());
            }
            catch (Exception e)
            {
                playerData.ReturnCode = "S300";
                playerData.ReturnMessage = e.Message;
            }
            return TextUtility.SerializeToStream(playerData);
        }
        #endregion


        public byte[] RemoveFriend(string account,string friend)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "S400";
            playerData.ReturnMessage = "";

            try
            {
                PlayerDataLogic playerDataLogic = new PlayerDataLogic();
                playerData = playerDataLogic.RemoveFriend(account,friend);
            }
            catch
            {
                playerData.ReturnCode = "(UI)S499";
                playerData.ReturnMessage = "玩家資料未知例外情況！";
            }
            return TextUtility.SerializeToStream(playerData);
        }
    }
}
