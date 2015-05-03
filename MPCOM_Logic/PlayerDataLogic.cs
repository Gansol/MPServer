using System;
using System.Collections.Generic;
using System.EnterpriseServices;

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
 * 這個檔案是用來進行 驗證玩家資料所使用
 * 載入玩家資料、更新玩家資料
 * >>try catch 要移除 使用AutoComplete就可 移除後刪除
 * >>邏輯都沒寫(還要再看看)
 * 
 * ***************************************************************/

namespace MPCOM
{
    // TransactionOption 指定元件要求的自動交易類型。
    // NotSupported	沒有使用支配性的交易在內容中建立元件。
    // Required	共用交易 (如果存在的話)，並且建立新交易 (如果有必要的話)。
    // RequiresNew	不論目前內容的狀態如何，都使用新交易建立元件。
    // Supported	共用交易 (如果有存在的話)。
    [Transaction(TransactionOption.Required)]
    public class PlayerDataLogic : ServicedComponent    // ServicedComponent 表示所有使用 COM+ 服務之類別的基底類別。
    {
        PlayerData playerData = new PlayerData();
        Dictionary<string, object> clinetData;
        Dictionary<string, object> serverData;

        protected override bool CanBePooled()
        {
            return true;
        }

        #region LoadPlayerData 載入玩家資料

        [AutoComplete]
        public PlayerData LoadPlayerData(string account)
        {
            playerData.ReturnCode = "(Logic)S400";
            playerData.ReturnMessage = "";

            try
            {
                //to do check 

                PlayerDataIO playerDataIO = new PlayerDataIO();
                playerData = playerDataIO.LoadPlayerData(account);

            }
            catch (Exception e)
            {
                throw e;
            }

            return playerData;
        }

        #endregion

        #region UpdatePlayerData 更新玩家資料

        [AutoComplete]
        public PlayerData UpdatePlayerData(string account, byte rank, byte exp, Int16 maxCombo, int maxScore, int sumScore, string miceAll, string team, string miceAmount, string friend)
        {
            PlayerData playerData = new PlayerData();
            playerData.ReturnCode = "(Logic)S400";
            playerData.ReturnMessage = "";

            try
            {
                if (rank < 0 && rank > 99)
                {
                    playerData.ReturnCode = "S407";
                    playerData.ReturnMessage = "玩家等級異常！";
                }

                if (exp < 0 && exp > 100)
                {
                    playerData.ReturnCode = "S408";
                    playerData.ReturnMessage = "玩家經驗異常！";
                }


                if (maxCombo < 0 && maxCombo > 1000)
                {
                    playerData.ReturnCode = "S409";
                    playerData.ReturnMessage = "玩家Combo異常！";
                }

                if (maxScore < 0 && maxScore > int.MaxValue)
                {
                    playerData.ReturnCode = "S410";
                    playerData.ReturnMessage = "玩家最高分異常！";
                }

                if (sumScore < 0 && sumScore > int.MaxValue)
                {
                    playerData.ReturnCode = "S411";
                    playerData.ReturnMessage = "玩家總分異常！";
                }

                //載入伺服器玩家資料提供比對
                playerData = LoadPlayerData(account);

                if (playerData.ReturnCode == "S401")
                {
                    clinetData = MiniJSON.Json.Deserialize(miceAll) as Dictionary<string, object>;
                    serverData = MiniJSON.Json.Deserialize(playerData.MiceAll) as Dictionary<string, object>;

                    if (serverData.Count != clinetData.Count)
                    {
                        playerData.ReturnCode = "S412";
                        playerData.ReturnMessage = "老鼠成員異常！";
                    }


                    clinetData = MiniJSON.Json.Deserialize(team) as Dictionary<string, object>;
                    serverData = MiniJSON.Json.Deserialize(playerData.Team) as Dictionary<string, object>;

                    //如果與伺服器資料 數量不相同
                    if (serverData.Count != clinetData.Count)
                    {
                        playerData.ReturnCode = "S413";
                        playerData.ReturnMessage = "隊伍老鼠異常！";
                    }


                    clinetData = MiniJSON.Json.Deserialize(miceAmount) as Dictionary<string, object>;
                    serverData = MiniJSON.Json.Deserialize(playerData.MiceAmount) as Dictionary<string, object>;

                    //如果與伺服器資料 數量不相同
                    if (serverData.Count != clinetData.Count)
                    {
                        playerData.ReturnCode = "S414";
                        playerData.ReturnMessage = "老鼠數量異常！";
                    }


                    clinetData = MiniJSON.Json.Deserialize(friend) as Dictionary<string, object>;
                    serverData = MiniJSON.Json.Deserialize(playerData.Friend) as Dictionary<string, object>;

                    //如果與伺服器資料 數量不相同
                    if (serverData.Count != clinetData.Count)
                    {
                        playerData.ReturnCode = "S415";
                        playerData.ReturnMessage = "好友名單異常！";
                    }

                    //如果驗證成功 寫入玩家資料
                    PlayerDataIO playerDataIO = new PlayerDataIO();
                    playerData = playerDataIO.UpdatePlayerData(account, rank, exp, maxCombo, maxScore, sumScore, miceAll, team, miceAmount, friend);
                }

            }
            catch (Exception e)
            {
                throw e;
            }

            return playerData;

        }

        #endregion
    }
}
