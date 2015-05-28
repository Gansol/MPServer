using System;
using System.EnterpriseServices;
using MPProtocol;

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
 * 這個檔案是用來進行判斷對戰邏輯 驗證資料所使用
 * 計算分數
 * >>try catch 要移除 使用AutoComplete就可 移除後刪除
 * >>邏輯都沒寫
 * 任務部分的獎勵與條件要再考慮看看資料放哪
 * ***************************************************************/

namespace MPCOM
{
    // TransactionOption 指定元件要求的自動交易類型。
    // NotSupported	沒有使用支配性的交易在內容中建立元件。
    // Required	共用交易 (如果存在的話)，並且建立新交易 (如果有必要的話)。
    // RequiresNew	不論目前內容的狀態如何，都使用新交易建立元件。
    // Supported	共用交易 (如果有存在的話)。
    [Transaction(TransactionOption.Required)]
    public class BattleLogic : ServicedComponent// ServicedComponent 表示所有使用 COM+ 服務之類別的基底類別。
    {
        BattleData battleData = new BattleData();

        protected override bool CanBePooled()
        {
            return true;
        }

        private int harvest = 200;
        private int harvestReward = 1000;

        #region ClacScore 計算老鼠命中分數

        [AutoComplete]
        public BattleData ClacScore(byte miceID, float time, float eatingRate, Int16 score)
        {
            battleData.ReturnCode = "(Logic)S500";
            battleData.ReturnMessage = "";

            try
            {
                switch (miceID) //用老鼠ID來判斷 取得的分數是否異常
                {
                    case 1: //EggMice
                        {
                            //to do verification
                            if (score > 100) //這裡亂寫的 要想驗證方法
                            {
                                battleData.ReturnCode = "S502";
                                battleData.ReturnMessage = "驗證分數失敗！";
                                return battleData;
                            }
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            battleData.score = score;
            battleData.ReturnCode = "S501";
            battleData.ReturnMessage = "驗證分數成功！";
            return battleData;
        }

        #endregion

        #region ClacScore 計算任務完成分數

        [AutoComplete]
        public BattleData ClacScore(byte mission, float missionRate)
        {
            battleData.ReturnCode = "(Logic)S500";
            battleData.ReturnMessage = "";

            try
            {
                switch ((Mission)mission) // 判斷任務獎勵
                {
                    case Mission.Harvest: //EggMice
                        {
                            //to do verification
                            if (missionRate <= 0 || missionRate > 10)
                            {
                                battleData.ReturnCode = "S504";
                                battleData.ReturnMessage = "驗證任務獎勵失敗！";
                                return battleData;
                            }
                            else
                            {
                                battleData.missionScore = (short)Math.Round((missionRate * harvestReward), 0);
                                battleData.ReturnCode = "S503";
                                battleData.ReturnMessage = "驗證任務獎勵成功！";
                                return battleData;
                            }
                        }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return battleData;
        }

        #endregion

        #region SelectMission 選擇任務

        [AutoComplete]
        public BattleData SelectMission(byte mission, float missionRate)
        {
            battleData.ReturnCode = "(Logic)S500";
            battleData.ReturnMessage = "";

            try
            {
                switch ((Mission)mission) // 判斷任務獎勵
                {
                    case Mission.Harvest: //EggMice
                        {
                            //to do verification
                            battleData.missionScore = (short)Math.Round((missionRate * harvest),0);
                            battleData.ReturnCode = "S505";
                            battleData.ReturnMessage = "選擇任務成功！";
                            return battleData;
                        }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            battleData.ReturnCode = "S506";
            battleData.ReturnMessage = "選擇任務失敗！";
            return battleData;
        }

        #endregion
    }
}
