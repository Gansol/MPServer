using System;
using System.EnterpriseServices;
using MPProtocol;
using ExitGames.Logging;
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
        private Int16 drivingMice = 35;
        private Int16 harvestRate = 50;               // 分數倍率
        private Int16 reduce = -500;                  // 減少分數

        private Int16 harvestReward = 100;
        private Int16 drivingMiceReward = 200;
        private Int16 harvestRateReward = 400;
        private Int16 worldBossReward = 2000;

        private static float scoreRate = 1;

        private struct Rate
        {
            public readonly static float Normal = 1;
            public readonly static float Low = 0.8f;
            public readonly static float High = 1.2f;
        }

        private struct EnergyRate
        {
            public readonly static float Normal = 1;
            public readonly static float Low = 0.5f;
            public readonly static float High = 2f;
        }

        private struct EggMice
        {
            public readonly static int ID = 10001;    // ID
            public readonly static int eatFull = 10;    // 2.5s = 4
            public readonly static float perEat = 1;
            public readonly static float eatingRate = 0.25f;
            public readonly static float hp = 30f;
            public readonly static int skill = 1;
        }

        private struct BlackMice
        {
            public readonly static int ID = 10002;    // ID
            public static int eatFull = 20;    // 2s = 10
            public static float perEat = 5;
            public static float eatingRate = 0.5f;
            public readonly static float hp = 30f;
        }

        private struct CandyMice
        {
            public readonly static int ID = 10003;    // ID
            public static int eatFull = 32;    // 3s = 10.667
            public static float perEat = 8;
            public static float eatingRate = 0.75f;
            public readonly static float hp = 30f;
        }

        private struct RabbitMice
        {
            public readonly static int ID = 10004;    // ID
            public static int eatFull = 24;    // 2.64s = 9.091
            public static float perEat = 3;
            public static float eatingRate = 0.33f;
            public readonly static float hp = 30f;
        }

        private struct NinjaMice
        {
            public readonly static int ID = 10005;    // ID
            public static int eatFull = 15;    // 2.86s = 9.79
            public static float perEat = 2;
            public static float eatingRate = 0.25f;
            public readonly static float hp = 30f;
        }

        private struct ThiefMice
        {
            public readonly static int ID = 10006;    // ID
            public static int eatFull = 15;    // 2.86s = 9.79
            public static float perEat = 2;
            public static float eatingRate = 0.25f;
            public readonly static float hp = 30f;
        }

        private struct MagicMice
        {
            public readonly static int ID = 10007;    // ID
            public static int eatFull = 15;    // 2.86s = 9.79
            public static float perEat = 2;
            public static float eatingRate = 0.25f;
            public readonly static float hp = 30f;
        }

        private struct EngineerMice
        {
            public readonly static int ID = 10008;    // ID
            public static int eatFull = 15;    // 2.86s = 9.79
            public static float perEat = 2;
            public static float eatingRate = 0.25f;
            public readonly static float hp = 30f;
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
                    battleData.scoreRate = Rate.Normal;
                    break;
                case (int)ENUM_Rate.Low:
                    battleData.scoreRate = Rate.Low;
                    break;
                case (int)ENUM_Rate.High:
                    battleData.scoreRate = Rate.High;
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
                    battleData.energyRate = EnergyRate.Normal;
                    break;
                case (int)ENUM_Rate.Low:
                    battleData.energyRate = EnergyRate.Low;
                    break;
                case (int)ENUM_Rate.High:
                    battleData.energyRate = EnergyRate.High;
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
        public BattleData ClacScore(short miceID, float aliveTime, float scoreRate, int combo, float energyRate)
        {
            battleData.ReturnCode = "(Logic)S500";
            battleData.ReturnMessage = "";
            Int16 score = 0;
            Int16 totalScore = 0;
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
                                score = Convert.ToInt16(EggMice.eatFull * scoreRate * -1);
                            }
                            else
                            {
                                score = Convert.ToInt16(EggMice.eatFull - (EggMice.perEat * ateTimes * scoreRate));
                            }

                            totalScore = Convert.ToInt16(EggMice.eatFull * scoreRate);
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
                            totalScore = Convert.ToInt16(BlackMice.eatFull * scoreRate);
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
                            totalScore = Convert.ToInt16(CandyMice.eatFull * scoreRate);
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
                            totalScore = Convert.ToInt16(RabbitMice.eatFull * scoreRate);
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
                            totalScore = Convert.ToInt16(NinjaMice.eatFull * scoreRate);
                            break;
                        }
                    case 10006: //Njnja 錯誤
                        {
                            int ateTimes = Convert.ToInt16(Math.Floor(aliveTime / ThiefMice.eatingRate));

                            if (ThiefMice.perEat * ateTimes >= ThiefMice.eatFull)
                            {
                                score = Convert.ToInt16(ThiefMice.eatFull * scoreRate * -1);
                            }
                            else
                            {
                                score = Convert.ToInt16(ThiefMice.eatFull - (ThiefMice.perEat * ateTimes * scoreRate));
                            }
                            totalScore = Convert.ToInt16(ThiefMice.eatFull * scoreRate);
                            break;
                        }
                    case 10007: //Njnja 錯誤
                        {
                            int ateTimes = Convert.ToInt16(Math.Floor(aliveTime / MagicMice.eatingRate));

                            if (NinjaMice.perEat * ateTimes >= MagicMice.eatFull)
                            {
                                score = Convert.ToInt16(MagicMice.eatFull * scoreRate * -1);
                            }
                            else
                            {
                                score = Convert.ToInt16(MagicMice.eatFull - (MagicMice.perEat * ateTimes * scoreRate));
                            }
                            totalScore = Convert.ToInt16(MagicMice.eatFull * scoreRate);
                            break;
                        }
                    case 10008: //Njnja 錯誤
                        {
                            int ateTimes = Convert.ToInt16(Math.Floor(aliveTime / EngineerMice.eatingRate));

                            if (EngineerMice.perEat * ateTimes >= EngineerMice.eatFull)
                            {
                                score = Convert.ToInt16(EngineerMice.eatFull * scoreRate * -1);
                            }
                            else
                            {
                                score = Convert.ToInt16(EngineerMice.eatFull - (EngineerMice.perEat * ateTimes * scoreRate));
                            }
                            totalScore = Convert.ToInt16(EngineerMice.eatFull * scoreRate);
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                throw e;
            }


            battleData.energy = 0;
            if (score > 0)
            {
                battleData.energy = Convert.ToInt16(Math.Round(2 * energyRate, 0, MidpointRounding.AwayFromZero));
                if (combo > 25)
                    battleData.energy = Convert.ToInt16(Math.Round(3 * energyRate, 0, MidpointRounding.AwayFromZero));
                if (combo > 50)
                    battleData.energy = Convert.ToInt16(Math.Round(4 * energyRate, 0, MidpointRounding.AwayFromZero));
                if (combo > 100)
                    battleData.energy = Convert.ToInt16(Math.Round(5 * energyRate, 0, MidpointRounding.AwayFromZero));
            }

            battleData.score = score;
            battleData.totalScore = totalScore;
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
                                battleData.missionReward = (Int16)Math.Round(missionRate * drivingMiceReward * customValue / 10);
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
                                Log.Debug("Mission.WorldBoss: 時間到！");
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
                            Random rnd = new Random();
                            short value = Convert.ToInt16(missionRate * drivingMice);
                            //to do verification
                            battleData.missionScore = Convert.ToInt16(rnd.Next(value - 10, value + 1));
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
                            battleData.bossHP = (Int16)NinjaMice.hp;
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
        public BattleData GameOver(string account, short score, short otherScore, short gameTime, int lostMice,
             short totalScore, short spawnCount, short missionCompletedCount, short maxMissionCount
            , short combo, string jStringMiceData, string[] col)
        {
            battleData.ReturnCode = "(Logic)S500";
            battleData.ReturnMessage = "";

            string itemReward = "{}", evaluateText;
            short expReward = 0, sumEvaluateScore;
            short sliverReward = 0, goldReward = 0, miceExp;
            short[] lostRate, comboRate, scoreRate, timeRate, missionRate;
            float[] evaluate;

            List<string> columns = new List<string>(col);
            try
            {

                jStringMiceData = jStringMiceData.Replace(@"\", "");
                Log.Debug("jStringMiceData:" + jStringMiceData);
                Dictionary<string, object> dictClientMiceData = new Dictionary<string, object>();
                dictClientMiceData = MiniJSON.Json.Deserialize(jStringMiceData) as Dictionary<string, object>;
                Log.Debug("jStringMiceData:" + dictClientMiceData.Count);
                Dictionary<string, Dictionary<string, object>> result = new Dictionary<string, Dictionary<string, object>>();
                lostRate = GetLostRate(lostMice, spawnCount);
                scoreRate = GetScoreRate(score, totalScore);
                timeRate = GetTimeRate(gameTime);
                comboRate = GetComboRate(combo, spawnCount);
                missionRate = GetMissionRate(missionCompletedCount, maxMissionCount);
                sumEvaluateScore = Convert.ToInt16(lostRate[1] + scoreRate[1] + timeRate[1] + comboRate[1] + missionRate[1]);
                evaluate = GetEvaluate(sumEvaluateScore);
                evaluateText = GetEvaluateText(sumEvaluateScore);

                //score / ( 100 + lost rate + combo rate+ mission rate+ score rate + time rate ) * evaluate + default

                sliverReward = (short)Math.Max(0, score / (100 + lostRate[0] + comboRate[0] + missionRate[0] + scoreRate[0] + timeRate[0]) * evaluate[0] + evaluate[1]);


                PlayerData playerData = new PlayerData();
                PlayerDataIO playerDataIO = new PlayerDataIO();

                playerData = playerDataIO.LoadPlayerItem(account);
                result = UpdateResult(account, playerData, dictClientMiceData);

                playerData = playerDataIO.LoadPlayerData(account);
                expReward = GetExp(sliverReward, playerData.Rank);

                itemReward = GetItemReward(new List<string>(dictClientMiceData.Keys), sumEvaluateScore);

                goldReward = GetGoldReward(sumEvaluateScore);

                // clac mice exp
                // playerData = playerDataIO.UpdatePlayerItem(account, result, columns);

                battleData.totalScore = score;
                battleData.sliverReward = sliverReward;
                battleData.goldReward = goldReward;
                battleData.expReward = expReward;
                battleData.jItemReward = itemReward;
                battleData.evaluate = evaluateText;
                battleData.jMiceResult = MiniJSON.Json.Serialize(result);
                battleData.battleResult = 0;

                battleData.ReturnCode = "S514";
                battleData.ReturnMessage = "遊戲結束計算完成！";
                Log.Debug("battleData.sliverReward:" + battleData.sliverReward + "battleData.expReward:" + battleData.expReward + " battleData.jItemReward:" + battleData.jItemReward + " battleData.goldReward:" + battleData.goldReward);
                return battleData;
            }
            catch
            {
                throw;
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="micesID">玩家擁有老鼠ID</param>
        /// <param name="sumEvaluateScore">評價分數加總</param>
        /// <returns></returns>
        private string GetItemReward(List<string> micesID, int sumEvaluateScore)
        {
            string result = "";
            int itemKind = 0, itemCount = 0;
            Random rnd = new Random();
            Dictionary<string, object> rewardItem = new Dictionary<string, object>();
            if (sumEvaluateScore >= 18f)
            {
                itemKind = rnd.Next(2, 4 + 1);
                itemCount = rnd.Next(1, 5 + 1);
            }

            else if (sumEvaluateScore >= 15f)
            {
                itemKind = rnd.Next(2, 3 + 1);
                itemCount = rnd.Next(2, 3 + 1);
            }
            else if (sumEvaluateScore >= 12f)
            {
                itemKind = rnd.Next(1, 3 + 1);
                itemCount = rnd.Next(1, 3 + 1);
            }
            else if (sumEvaluateScore >= 9f)
            {
                itemKind = rnd.Next(1, 2 + 1);
                itemCount = rnd.Next(1, 2 + 1);
            }
            else if (sumEvaluateScore < 9f)
            {
                itemKind = 1;
                itemCount = rnd.Next(1, 3 + 1);
            }

            for (int i = 0; i < itemKind; i++)
            {
                string key = micesID[rnd.Next(0, micesID.Count)];
                if (!rewardItem.ContainsKey(key))
                {
                    Dictionary<string, object> count = new Dictionary<string, object>();
                    count.Add("ItemCount", itemCount);
                    rewardItem.Add(key, count);
                }
            }
            result = MiniJSON.Json.Serialize(rewardItem);
            return result;
        }

        /// <summary>
        /// 取得金幣獎勵
        /// </summary>
        /// <param name="sumEvaluateScore">評價分數加總</param>
        /// <returns>金幣獎勵</returns>
        private short GetGoldReward(int sumEvaluateScore)
        {
            short result = 0;
            Random rnd = new Random();
            if (sumEvaluateScore >= 18f)
            {
                result = Convert.ToInt16(rnd.Next(1, 5 + 1));
            }
            return result;
        }

        #region GameOver Clac Functions
        private short[] GetLostRate(float lost, float spawnCount)
        {
            float rate = lost / spawnCount;
            short[] result = new short[2];

            if (rate == 1) { result[0] = -10; result[1] = 5; }
            else if (rate <= .05f) { result[0] = -5; result[1] = 4; }
            else if (rate <= .08f) { result[0] = 0; result[1] = 3; }
            else if (rate <= .15f) { result[0] = 10; result[1] = 2; }
            else if (rate > .15f) { result[0] = 20; result[1] = 1; }

            return result;
        }

        private short[] GetComboRate(float combo, float spawnCount)
        {
            float rate = combo / spawnCount;
            short[] result = new short[2];

            if (rate == 1) { result[0] = -20; result[1] = 5; }
            else if (rate >= .6f) { result[0] = -15; result[1] = 4; }
            else if (rate >= .3f) { result[0] = -10; result[1] = 3; }
            else if (rate >= .2f) { result[0] = -5; result[1] = 2; }
            else if (rate < .2f) { result[0] = 0; result[1] = 1; }

            return result;
        }

        private short[] GetMissionRate(float mission, float maxMission)
        {
            float rate = mission / maxMission;
            short[] result = new short[2];

            if (rate == 1) { result[0] = -15; result[1] = 3; }
            else if (rate >= .6f) { result[0] = -10; result[1] = 2; }
            else if (rate >= .3f) { result[0] = -5; result[1] = 1; }
            else { result[0] = 10; result[1] = 0; }

            return result;
        }

        private short[] GetScoreRate(float score, float totalScore)
        {
            float rate = score / totalScore;
            short[] result = new short[2];

            if (rate >= .8f) { result[0] = -10; result[1] = 5; }
            else if (rate >= .6f) { result[0] = -5; result[1] = 4; }
            else if (rate >= .5f) { result[0] = 0; result[1] = 3; }
            else if (rate >= .4f) { result[0] = 5; result[1] = 2; }
            else if (rate < .4f) { result[0] = 10; result[1] = 1; }

            return result;
        }

        private short[] GetTimeRate(float time)
        {
            int gameTime = 150;
            float rate = time / gameTime;
            short[] result = new short[2];

            if (rate >= .8f) { result[0] = -10; result[1] = 3; }
            else if (rate >= .6f) { result[0] = -5; result[1] = 2; }
            else if (rate >= .5f) { result[0] = 0; result[1] = 1; }
            else if (rate >= .4f) { result[0] = 5; result[1] = 0; }
            else if (rate < .4f) { result[0] = 10; result[1] = -5; }

            return result;
        }

        /// <summary>
        /// 取得評價
        /// </summary>
        /// <param name="sumEvaluateScore">評價分數加總</param>
        /// <returns>result[0]=獎勵倍率 result[1]=基礎獎勵</returns>
        private float[] GetEvaluate(float sumEvaluateScore)
        {
            float[] result = new float[2];

            if (sumEvaluateScore >= 18f) { result[0] = 1.5f; result[1] = 200; }
            else if (sumEvaluateScore >= 15f) { result[0] = 1.1f; result[1] = 100; }
            else if (sumEvaluateScore >= 12f) { result[0] = 1.05f; result[1] = 50; }
            else if (sumEvaluateScore >= 9f) { result[0] = 1; result[1] = 50; }
            else if (sumEvaluateScore < 9f) { result[0] = .5f; result[1] = 0; }
            return result;
        }

        private string GetEvaluateText(float sumEvaluateScore)
        {
            if (sumEvaluateScore >= 18f) { return "S"; }
            else if (sumEvaluateScore >= 15f) { return "A"; }
            else if (sumEvaluateScore >= 12f) { return "B"; }
            else if (sumEvaluateScore >= 9f) { return "C"; }
            else if (sumEvaluateScore < 9f) { return "F"; }

            return "F";
        }

        /// <summary>
        /// 取得經驗值
        /// </summary>
        /// <param name="reward">銀幣獎勵</param>
        /// <param name="level">等級</param>
        /// <returns>經驗值</returns>
        private short GetExp(float reward, float level)
        {
            reward = (reward / 10f * (100f + level)) + level;
            Log.Debug("Battle Logic Get Exp:" + reward);
            return Convert.ToInt16(Math.Round(Math.Min(short.MaxValue, reward), 0, MidpointRounding.AwayFromZero));
        }

        /// <summary>
        /// 更新老鼠資料
        /// </summary>
        /// <param name="account">帳號</param>
        /// <param name="playerData">伺服器玩家資料</param>
        /// <param name="dictClientMiceData">客戶端老鼠資料(UseCount)</param>
        /// <returns></returns>
        private Dictionary<string, Dictionary<string, object>> UpdateResult(string account, PlayerData playerData, Dictionary<string, object> dictClientMiceData)
        {
            try
            {
                object level, exp, sUseCount, cUseCount, itemCount, data, outClientMiceData;

                Dictionary<string, Dictionary<string, object>> result = new Dictionary<string, Dictionary<string, object>>();
                Dictionary<string, object> dictServerData = new Dictionary<string, object>();
                dictServerData = MiniJSON.Json.Deserialize(playerData.PlayerItem) as Dictionary<string, object>;

                List<string> clientKeys = new List<string>(dictClientMiceData.Keys);
                Dictionary<string, object> miceProp = new Dictionary<string, object>();

                foreach (string key in clientKeys)
                {
                    Log.Debug("clientKey:" + key);
                    if (dictServerData.ContainsKey(key))    // itemID = itemID
                    {
                        Log.Debug("Key Equal !!");
                        dictServerData.TryGetValue(key, out data);  // 取出道具資料
                        Dictionary<string, object> outServerMiceData = data as Dictionary<string, object>;

                        if (outServerMiceData.TryGetValue(PlayerItem.Rank.ToString(), out level))
                        {
                            Log.Debug("Get Rank !!");
                            if (outServerMiceData.TryGetValue(PlayerItem.Exp.ToString(), out exp))
                            {
                                Log.Debug("Get Exp !!");
                                dictClientMiceData.TryGetValue(key, out outClientMiceData);

                                miceProp = outClientMiceData as Dictionary<string, object>;
                                miceProp.TryGetValue(PlayerItem.UseCount.ToString(), out cUseCount); // 從玩家老鼠資料中取出 使用次數

                                Log.Debug("useCount:" + cUseCount);
                                short[] clacResult = ClacMiceRank(Convert.ToInt32(level), Convert.ToInt32(exp), Convert.ToInt32(cUseCount));

                                Log.Debug("  Key:" + key.ToString() + "Rank:" + clacResult[0] + "Exp:" + clacResult[1]);

                                miceProp.Add(PlayerItem.Rank.ToString(), clacResult[0].ToString());                    // 使用clientMiceData參考修改資料
                                miceProp.Add(PlayerItem.Exp.ToString(), clacResult[1].ToString());                     // 使用clientMiceData參考修改資料

                                //// itemcount
                                //outServerMiceData.TryGetValue(PlayerItem.ItemCount.ToString(), out itemCount);
                                //if (ItemCountChk(Convert.ToInt16(itemCount), Convert.ToInt16(cUseCount)))
                                //    miceProp.Add(PlayerItem.ItemCount.ToString(), (Convert.ToInt16(itemCount) - Convert.ToInt16(cUseCount)).ToString());

                                // Log.Debug(" itemCount: " + itemCount + " cUseCount: " + Convert.ToInt16(cUseCount));
                                // usecount
                                //outServerMiceData.TryGetValue(PlayerItem.UseCount.ToString(),out sUseCount);
                                //Log.Debug(" cUseCount: "+ Convert.ToInt16(miceProp[PlayerItem.UseCount.ToString()]) +" sUseCount: "+ Convert.ToInt16(sUseCount));
                                //miceProp[PlayerItem.UseCount.ToString()] = Convert.ToInt16(cUseCount) + Convert.ToInt16(sUseCount);

                            }
                        }
                        result.Add(key, miceProp);
                    }

                }

                Log.Debug("result.Count: " + result.Count);

                foreach (KeyValuePair<string, Dictionary<string, object>> x in result)
                {
                    foreach (KeyValuePair<string, object> y in x.Value)
                    {
                        Log.Debug("result inner.Key: " + y.Key + "  result inner.Key: " + y.Value);
                    }
                }

                return result;
            }
            catch
            {
                throw;
            }
        }
        #endregion
        private bool ItemCountChk(int serverCount, int clientUseCount)
        {
            return (serverCount > clientUseCount) ? true : false;
        }

        /// <summary>
        /// 計算老鼠等級、經驗
        /// </summary>
        /// <param name="level">伺服器老鼠等級</param>
        /// <param name="exp">伺服器老鼠經驗</param>
        /// <param name="useCount">客戶端老鼠使用量</param>
        /// <returns>result[0] = exp result[1] = level</returns>
        private short[] ClacMiceRank(float level, int exp, int useCount)
        {
            short[] result = new short[2];

            useCount = Math.Max(0, useCount);
            level = Math.Max(0, level);

            int levelMaxExp = (int)(Math.Round(level * level / 5f, MidpointRounding.AwayFromZero) + 2 + level - 1);

            if (exp + useCount < levelMaxExp)
                exp += useCount;
            else
            {
                exp = useCount + exp - levelMaxExp;
                level++;
            }

            result[0] = (short)exp;
            result[1] = (short)level;

            return result;
        }
    }
}
