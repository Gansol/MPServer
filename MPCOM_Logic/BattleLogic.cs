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
        #region variable 變數區
        private Int16 harvest = 200;
        private Int16 drivingMice = 50;
        private Int16 harvestRate = 10;               // 分數倍率
        private Int16 reduce = -500;                  // 減少分數

        private Int16 harvestReward = 100;
        private Int16 drivingMiceReward = 200;
        private Int16 harvestRateReward = 400;
        private Int16 worldBossReward = 10000;

        private struct EggMice
        {
            public static int eatFull = 10;    // 2.5s = 4
            public static float perEat = 1;
            public static float eatingRate = 0.25f;
        }

        private struct BlackMice
        {
            public static int eatFull = 20;    // 2s = 10
            public static float perEat = 5;
            public static float eatingRate = 0.5f;
        }

        private struct CandyMice
        {
            public static int eatFull = 32;    // 3s = 10.667
            public static float perEat = 8;
            public static float eatingRate = 0.75f;
        }

        private struct RabbitMice
        {
            public static int eatFull = 24;    // 2.64s = 9.091
            public static float perEat = 3;
            public static float eatingRate = 0.33f;
        }

        private struct NinjaMice
        {
            public static int eatFull = 28;    // 2.86s = 9.79
            public static float perEat = 7;
            public static float eatingRate = 0.67f;
        }
        #endregion

        #region ClacScore 計算老鼠命中分數

        [AutoComplete]
        public BattleData ClacScore(string miceName, float aliveTime)
        {
            battleData.ReturnCode = "(Logic)S500";
            battleData.ReturnMessage = "";
            Int16 score = 0;

            try
            {
                switch (miceName) //用老鼠ID來判斷 取得的分數是否異常
                {
                    case "EggMice": //EggMice
                        {
                            int ateTimes = Convert.ToInt16(Math.Floor(aliveTime / EggMice.eatingRate));

                            if (EggMice.perEat * ateTimes >= EggMice.eatFull)
                            {
                                score = Convert.ToInt16(EggMice.eatFull * -1);
                            }
                            else
                            {
                                score = Convert.ToInt16(EggMice.eatFull - EggMice.perEat * ateTimes);
                            }
                            break;
                        }
                    case "BlackMice": //EggMice
                        {
                            int ateTimes = Convert.ToInt16(Math.Floor(aliveTime / BlackMice.eatingRate));

                            if (BlackMice.perEat * ateTimes >= BlackMice.eatFull)
                            {
                                score = Convert.ToInt16(BlackMice.eatFull * -1);
                            }
                            else
                            {
                                score = Convert.ToInt16(BlackMice.eatFull - BlackMice.perEat * ateTimes);
                            }
                            break;
                        }
                    case "CandyMice": //EggMice
                        {
                            int ateTimes = Convert.ToInt16(Math.Floor(aliveTime / CandyMice.eatingRate));

                            if (CandyMice.perEat * ateTimes >= CandyMice.eatFull)
                            {
                                score = Convert.ToInt16(CandyMice.eatFull * -1);
                            }
                            else
                            {
                                score = Convert.ToInt16(CandyMice.eatFull - CandyMice.perEat * ateTimes);
                            }
                            break;
                        }
                    case "RabbitMice": //EggMice
                        {
                            int ateTimes = Convert.ToInt16(Math.Floor(aliveTime / RabbitMice.eatingRate));

                            if (RabbitMice.perEat * ateTimes >= RabbitMice.eatFull)
                            {
                                score = Convert.ToInt16(RabbitMice.eatFull * -1);
                            }
                            else
                            {
                                score = Convert.ToInt16(RabbitMice.eatFull - RabbitMice.perEat * ateTimes);
                            }
                            break;
                        }
                    case "NinjaMice": //EggMice
                        {
                            int ateTimes = Convert.ToInt16(Math.Floor(aliveTime / NinjaMice.eatingRate));

                            if (NinjaMice.perEat * ateTimes >= NinjaMice.eatFull)
                            {
                                score = Convert.ToInt16(NinjaMice.eatFull * -1);
                            }
                            else
                            {
                                score = Convert.ToInt16(NinjaMice.eatFull - NinjaMice.perEat * ateTimes);
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

        #region ClacMissionReward 計算任務完成分數

        [AutoComplete]
        public BattleData ClacMissionReward(byte mission, float missionRate, int customValue)
        {
            battleData.ReturnCode = "(Logic)S500";
            battleData.ReturnMessage = "";

            try
            {
                switch ((Mission)mission) // 判斷任務獎勵
                {
                    case Mission.Harvest:
                        {
                            if (missionRate > 0 || missionRate < 10)
                            {
                                battleData.missionReward = (Int16)Math.Round((missionRate * harvestReward), 0);
                                battleData.ReturnCode = "S503";
                                battleData.ReturnMessage = "驗證任務獎勵成功！";
                                return battleData;
                            }
                            break;
                        }
                    case Mission.DrivingMice:
                        {
                            if (missionRate > 0 && customValue >= drivingMice) // 這裡自訂參數 customValue 是Combo
                            {
                                battleData.missionReward = (Int16)Math.Round(missionRate * drivingMiceReward);
                                battleData.ReturnCode = "S503";
                                battleData.ReturnMessage = "驗證任務獎勵成功！";
                                return battleData;
                            }
                            break;
                        }
                    case Mission.Reduce:
                        {
                            if (missionRate > 0) // 時間倒扣分
                            {
                                battleData.missionReward = (Int16)Math.Round(missionRate * reduce);
                                battleData.ReturnCode = "S503";
                                battleData.ReturnMessage = "驗證任務獎勵成功！";
                                return battleData;
                            }
                            break;
                        }
                    case Mission.HarvestRate:
                        {
                            if (missionRate > 0) // 時間倒扣分
                            {
                                battleData.missionReward = 0;
                                battleData.ReturnCode = "S503";
                                battleData.ReturnMessage = "驗證任務獎勵成功！";
                                return battleData;
                            }
                            break;
                        }
                    case Mission.Exchange:
                        {
                            if (missionRate > 0) // 時間倒扣分
                            {
                                battleData.missionReward = 0;
                                battleData.ReturnCode = "S503";
                                battleData.ReturnMessage = "驗證任務獎勵成功！";
                                return battleData;
                            }
                            break;
                        }
                    case Mission.WorldBoss:
                        {
                            if (missionRate > 0) // 時間倒扣分
                            {
                                battleData.missionReward = worldBossReward;
                                battleData.ReturnCode = "S503";
                                battleData.ReturnMessage = "驗證任務獎勵成功！";
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

            battleData.ReturnCode = "S504";
            battleData.ReturnMessage = "驗證任務獎勵失敗！";
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
                    case Mission.Harvest: // 收穫
                        {
                            //to do verification
                            battleData.missionScore = (Int16)Math.Round((missionRate * harvest), 0);
                            battleData.ReturnCode = "S505";
                            battleData.ReturnMessage = "選擇任務成功！";
                            return battleData;
                        }
                    case Mission.DrivingMice: // 趕老鼠
                        {
                            //to do verification
                            battleData.missionScore = (Int16)Math.Round((missionRate * drivingMice), 0);
                            battleData.ReturnCode = "S505";
                            battleData.ReturnMessage = "選擇任務成功！";
                            return battleData;
                        }
                    case Mission.Exchange: // 交換分數
                        {
                            battleData.ReturnCode = "S505";
                            battleData.ReturnMessage = "選擇任務成功！";
                            return battleData;
                        }
                    case Mission.HarvestRate: // 分數倍率
                        {
                            battleData.missionScore = (Int16)Math.Round((harvestRate * missionRate), 0);
                            battleData.ReturnCode = "S505";
                            battleData.ReturnMessage = "選擇任務成功！";
                            return battleData;
                        }
                    case Mission.Reduce: // 減少分數
                        {
                            battleData.missionScore = (Int16)Math.Round((missionRate * reduce), 0);
                            battleData.ReturnCode = "S505";
                            battleData.ReturnMessage = "選擇任務成功！";
                            return battleData;
                        }
                    case Mission.WorldBoss: // BOSS
                        {
                            // missionRate 是老鼠ID
                            battleData.missionScore = (Int16)missionRate;
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
