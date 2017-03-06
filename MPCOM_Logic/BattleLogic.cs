using System;
using System.EnterpriseServices;
using MPProtocol;
using ExitGames.Logging;

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
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        protected override bool CanBePooled()
        {
            return true;
        }
        #region variable 變數區
        private Int16 harvest = 200;
        private Int16 drivingMice = 50;
        private Int16 harvestRate = 2;               // 分數倍率
        private Int16 reduce = -500;                  // 減少分數

        private Int16 harvestReward = 100;
        private Int16 drivingMiceReward = 200;
        private Int16 harvestRateReward = 400;
        private Int16 worldBossReward = 2000;

        private static float scoreRate = 1;

        private struct Rate
        {
            public readonly static float mormal = 1;
            public readonly static float low = 0.8f;
            public readonly static float high = 1.2f;
        }

        private struct EggMice
        {
            public readonly static int ID = 10001;    // ID
            public readonly static int eatFull = 10;    // 2.5s = 4
            public readonly static float perEat = 1;
            public readonly static float eatingRate = 0.25f;
            public readonly static float hp = 25f;
            public readonly static int skill = 1;
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
            public readonly static int ID = 10005;    // ID
            public static int eatFull = 28;    // 2.86s = 9.79
            public static float perEat = 7;
            public static float eatingRate = 0.67f;
        }
        #endregion


        [AutoComplete]
        public BattleData UpdateScoreRate(ENUM_Rate rate)
        {
            battleData.ReturnCode = "S508";
            battleData.ReturnMessage = "更新分數倍率成功！";

            switch ((int)rate)
            {
                case (int)ENUM_Rate.Normal:
                    battleData.scoreRate = Rate.mormal;
                    break;
                case (int)ENUM_Rate.Low:
                    battleData.scoreRate = Rate.low;
                    break;
                case (int)ENUM_Rate.High:
                    battleData.scoreRate = Rate.high;
                    break;
                default:
                    battleData.ReturnCode = "S509";
                    battleData.ReturnMessage = "更新分數倍率失敗！";
                    break;
            }
            return battleData;
        }

        [AutoComplete]
        public BattleData UpdateEnergyRate(ENUM_Rate rate)
        {
            battleData.ReturnCode = "S512";
            battleData.ReturnMessage = "更新分數倍率成功！";

            switch ((int)rate)
            {
                case (int)ENUM_Rate.Normal:
                    battleData.energyRate = Rate.mormal;
                    break;
                case (int)ENUM_Rate.Low:
                    battleData.energyRate = Rate.low;
                    break;
                case (int)ENUM_Rate.High:
                    battleData.energyRate = Rate.high;
                    break;
                default:
                    battleData.ReturnCode = "S513";
                    battleData.ReturnMessage = "更新分數倍率失敗！";
                    break;
            }
            return battleData;
        }

        #region ClacScore 計算老鼠命中分數

        [AutoComplete]
        public BattleData ClacScore(short miceID, float aliveTime, float scoreRate, float energyRate)
        {
            battleData.ReturnCode = "(Logic)S500";
            battleData.ReturnMessage = "";
            Int16 score = 0;

            try
            {
                switch (miceID) //用老鼠ID來判斷 取得的分數是否異常
                {

                    case 10001: //EggMice 錯誤
                        {

                            int ateTimes = Convert.ToInt16(Math.Floor(aliveTime / EggMice.eatingRate));

                            Log.Debug("aliveTime:" + aliveTime + "EggMice.eatingRate: " + EggMice.eatingRate + "EggMice.eatFull: " + EggMice.eatFull + "All Eat: " + ateTimes * EggMice.perEat);
                            if (EggMice.perEat * ateTimes >= EggMice.eatFull)
                            {
                                score -= Convert.ToInt16(EggMice.eatFull * scoreRate);
                            }
                            else
                            {
                                score = Convert.ToInt16(EggMice.eatFull - (EggMice.perEat * ateTimes * scoreRate));
                            }
                            break;
                        }
                    case 10002: //Black 錯誤
                        {
                            int ateTimes = Convert.ToInt16(Math.Floor(aliveTime / BlackMice.eatingRate));

                            if (BlackMice.perEat * ateTimes >= BlackMice.eatFull)
                            {
                                score = Convert.ToInt16(BlackMice.eatFull * scoreRate * -1);
                            }
                            else
                            {
                                score = Convert.ToInt16(BlackMice.eatFull - (BlackMice.perEat * ateTimes * scoreRate));
                            }
                            break;
                        }
                    case 10003: //Candy 錯誤
                        {
                            int ateTimes = Convert.ToInt16(Math.Floor(aliveTime / CandyMice.eatingRate));

                            if (CandyMice.perEat * ateTimes >= CandyMice.eatFull)
                            {
                                score = Convert.ToInt16(CandyMice.eatFull * scoreRate * -1);
                            }
                            else
                            {
                                score = Convert.ToInt16(CandyMice.eatFull - (CandyMice.perEat * ateTimes * scoreRate));
                            }
                            break;
                        }
                    case 10004: //Rabbit 錯誤
                        {
                            int ateTimes = Convert.ToInt16(Math.Floor(aliveTime / RabbitMice.eatingRate));

                            if (RabbitMice.perEat * ateTimes >= RabbitMice.eatFull)
                            {
                                score = Convert.ToInt16(RabbitMice.eatFull * scoreRate * -1);
                            }
                            else
                            {
                                score = Convert.ToInt16(RabbitMice.eatFull - (RabbitMice.perEat * ateTimes * scoreRate));
                            }
                            break;
                        }
                    case 10005: //Njnja 錯誤
                        {
                            int ateTimes = Convert.ToInt16(Math.Floor(aliveTime / NinjaMice.eatingRate));

                            if (NinjaMice.perEat * ateTimes >= NinjaMice.eatFull)
                            {
                                score = Convert.ToInt16(NinjaMice.eatFull * scoreRate * -1);
                            }
                            else
                            {
                                score = Convert.ToInt16(NinjaMice.eatFull - (NinjaMice.perEat * ateTimes * scoreRate));
                            }
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            battleData.energy = (Int16)((score > 0) ? 1 : 0 * energyRate);   // 打死老鼠增加能量
            battleData.score = score;
            battleData.ReturnCode = "S501";
            battleData.ReturnMessage = "驗證分數成功！";
            return battleData;
        }

        #endregion

        #region ClacMissionReward 計算任務完成分數

        [AutoComplete]
        public BattleData ClacMissionReward(byte mission, float missionRate, Int16 customValue)
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
                                float percent = (float)customValue / 100;

                                if (customValue == 50)  // 平手 獎勵/2
                                {
                                    battleData.missionReward = (Int16)(worldBossReward / 2);
                                    battleData.customValue = (Int16)(worldBossReward / 2);
                                    battleData.ReturnCode = "S503";
                                    battleData.ReturnMessage = "驗證任務獎勵成功！";
                                    return battleData;
                                }
                                if (customValue > 50)   // (Host)勝利 獲得獎勵X貢獻百分比
                                {
                                    Int16 reward = (Int16)Math.Round((float)worldBossReward * percent);     // worldBossReward x 獲得百分比
                                    battleData.missionReward = reward;
                                    battleData.customValue = (Int16)(reward / 2);                         // customValue 對手獎勵
                                    battleData.ReturnCode = "S503";
                                    battleData.ReturnMessage = "驗證任務獎勵成功！";
                                    return battleData;
                                }
                                else if (customValue < 50)  // (Host)失敗 獲得50%獎勵
                                {
                                    float otherPercent = 1 - percent;
                                    Int16 reward = (Int16)Math.Round((float)worldBossReward * otherPercent);// worldBossReward x 獲得百分比
                                    battleData.missionReward = (Int16)(reward / 2);
                                    battleData.customValue = reward;                                        // customValue 對手獎勵
                                    battleData.ReturnCode = "S503";
                                    battleData.ReturnMessage = "驗證任務獎勵成功！";
                                    return battleData;
                                }
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
                            battleData.missionScore = (Int16)NinjaMice.ID;
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

        #region GameOver 遊戲結束
        [AutoComplete]
        public BattleData GameOver(short score, short otherScore, short gameTime, short lostMice)
        {
            battleData.ReturnCode = "(Logic)S500";
            battleData.ReturnMessage = "";

            try
            {
                if (score >= 0)
                {
                    battleData.score = score;

                    float reward = 0;
                    float exp = 0;

                    if (gameTime >= 30)
                    {
                        reward = (score / 10 * 1.0f) + 5;
                        exp = 1;
                    }
                    else if (gameTime >= 120)
                    {
                        reward = (score / 10 * 1.05f) + 25;
                        exp = 2;
                    }
                    else if (gameTime >= 300)
                    {
                        reward = (score / 10 * 1.1f) + 50;
                        exp = 5;
                    }

                    if (lostMice == 0) reward *= 1.5f;
                    if (score > otherScore) exp *= 2f;

                    battleData.sliverReward = (Int16)reward;
                    battleData.expReward = (byte)exp;
                    battleData.ReturnCode = "S501";
                    battleData.ReturnMessage = "計算獎勵成功！";
                    return battleData;
                }
                else
                {
                    battleData.ReturnCode = "S507";
                    battleData.ReturnMessage = "計算獎勵失敗！";
                    return battleData;
                }
            }
            catch (Exception e)
            {

                throw e;
            }
        }
        #endregion

    }
}
