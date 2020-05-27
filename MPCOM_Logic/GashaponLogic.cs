using System;
using System.EnterpriseServices;
using ExitGames.Logging;
using Gansol;
using MPProtocol;
using System.Collections.Generic;
using System.Security.Cryptography;

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
 * 這個檔案是用來進行 驗證老鼠資料所使用
 * 載入老鼠資料
 * >>try catch 要移除 使用AutoComplete就可 移除後刪除
 * 邏輯都沒寫
 * ***************************************************************/

namespace MPCOM
{
    // TransactionOption 指定元件要求的自動交易類型。
    // NotSupported	沒有使用支配性的交易在內容中建立元件。
    // Required	共用交易 (如果存在的話)，並且建立新交易 (如果有必要的話)。
    // RequiresNew	不論目前內容的狀態如何，都使用新交易建立元件。
    // Supported	共用交易 (如果有存在的話)。
    [Transaction(TransactionOption.Required)]
    public class GashaponLogic : ServicedComponent// ServicedComponent 表示所有使用 COM+ 服務之類別的基底類別。
    {
        StoreData storeData = new StoreData();
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        int count = 0, _totalChance = 0, totalGradeChance = 0, totalURChance = 0, totalSSRChance = 0, totalSRChance = 0, totalRChance = 0, totalNChance = 0, urBlance = 0, ssrBlance = 0, srBlance = 0, rBlance = 0, nBlance = 0, seriesBlance = 0;


        protected override bool CanBePooled()
        {
            return true;
        }


        #region BuyGashapon 載入轉蛋商品
        /// <summary>
        /// 載入轉蛋商品
        /// </summary>
        /// <returns></returns>
        [AutoComplete]
        public GashaponData[] LoadGashaponData(Int16 series)
        {
            GashaponData[] gashaponData = default(GashaponData[]);
            gashaponData[0].ReturnCode = "(Logic)S1200";
            gashaponData[0].ReturnMessage = "";

            GashaponIO gashaponIO = new GashaponIO();

            try
            {
                gashaponData = gashaponIO.LoadGashaponData(series);
                return gashaponData;
            }
            catch (Exception e)
            {
                throw;
            }
        }
        #endregion

        #region LoadGashaponSeriesChance 載入轉蛋商品
        /// <summary>
        /// 載入轉蛋商品
        /// </summary>
        /// <returns></returns>
        [AutoComplete]
        public GashaponData[] LoadGashaponSeriesChance()
        {
            GashaponData[] gashaponData = default(GashaponData[]);
            gashaponData[0].ReturnCode = "(Logic)S1200";
            gashaponData[0].ReturnMessage = "";

            GashaponIO gashaponIO = new GashaponIO();

            try
            {
                gashaponData = gashaponIO.LoadGashaponSeriesChance();
                return gashaponData;
            }
            catch (Exception e)
            {
                throw;
            }
        }
        #endregion


        #region BuyGashapon 購買轉蛋商品
        /// <summary>
        /// 購買轉蛋商品
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="itemType"></param>
        /// <param name="buyCount"></param>
        /// <returns></returns>
        [AutoComplete]
        public GashaponData[] BuyGashapon(int itemID, byte itemType , Int16 series, int price)
        {
            GashaponData[] gashaponData = default(GashaponData[]);
            GashaponData[] gashaponSeriesChance = default(GashaponData[]);
            List<int> gashaponLotteryBox = new List<int>();


            ENUM_GashaponGrade grade;
            gashaponData[0].ReturnCode = "(Logic)S1200";
            gashaponData[0].ReturnMessage = "";


            try
            {
                //storeData = storeDataIO.LoadStoreData(itemID, itemType);
                //storeData.PromotionsCount = storeData.PromotionsCount > 0 ? (Int16)(storeData.PromotionsCount - (Int16)1) : storeData.PromotionsCount;

                gashaponSeriesChance = LoadGashaponSeriesChance(); // 載入轉蛋資料


                if (gashaponSeriesChance[0].ReturnCode == "S1203")
                {
                    // select series 目前是由 Client確認 應改為Server確認
                    if (series == (byte)ENUM_GashaponSeries.AllSeries)
                        series = SelectGashaponSeries(gashaponSeriesChance);

                    gashaponData = LoadGashaponData(series); // 載入轉蛋資料

                    if (gashaponData[0].ReturnCode == "S1201")
                    {

                        // select Grade 
                        grade = SelectGashaponGrade(gashaponData, series);

                        // store chance and Add2LotteryBox
                        gashaponLotteryBox = Add2LotteryBox(gashaponData, series, grade);

                        // gashapon a gashapon
                        gashaponData = GenerateGashapon(gashaponLotteryBox, price);

                        //return gashapon
                        gashaponData[0].ReturnCode = "S1205";
                        gashaponData[0].ReturnMessage = "購買轉蛋商品資料成功！";
                        //如果驗證成功 寫入玩家資料

                    }
                }

            }
            catch (Exception e)
            {
                throw e;
            }

            return gashaponData;
        }


        #region SelectGashaponSeries

        private Int16 SelectGashaponSeries(GashaponData[] gashaponSeriesData)
        {
            int value = 0;
            List<int> blance = new List<int>();
            List<Int16> gashaponSeriesLotteryBox = new List<Int16>();

            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();


            for (int i = 0; i < gashaponSeriesData.Length; i++)
            {
                for (int j = 0; j < gashaponSeriesData[i].Series_Blance; j++)
                {
                    gashaponSeriesLotteryBox.Add(gashaponSeriesData[i].Series);
                }
            }

            gashaponSeriesLotteryBox.Shuffle();

            value = rng.Next(0, gashaponSeriesData.Length) + 1;

            return gashaponSeriesLotteryBox[value];
        } 
        #endregion

        #region SelectGashaponGrade
        private ENUM_GashaponGrade SelectGashaponGrade(GashaponData[] gashaponData, Int16 series)
        {
            int value = 0;
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            ENUM_GashaponGrade grade = default(ENUM_GashaponGrade);

            // clac chance
            for (int i = 0; i < gashaponData.Length; i++)
            {
                if (gashaponData[i].Grade == (byte)ENUM_GashaponGrade.N)
                    totalNChance += gashaponData[i].Chance + gashaponData[i].N_Blance;
                if (gashaponData[i].Grade == (byte)ENUM_GashaponGrade.R)
                    totalRChance += gashaponData[i].Chance + gashaponData[i].R_Blance;
                if (gashaponData[i].Grade == (byte)ENUM_GashaponGrade.SR)
                    totalSRChance += gashaponData[i].Chance + gashaponData[i].SR_Blance;
                if (gashaponData[i].Grade == (byte)ENUM_GashaponGrade.SSR)
                    totalSSRChance += gashaponData[i].Chance + gashaponData[i].SSR_Blance;
            }

            _totalChance = totalNChance + totalRChance + totalSRChance + totalSSRChance;


            value = rng.Next(0, _totalChance) + 1;

            if (value > 0)
            {
                if (value < totalSSRChance)
                {
                    grade = ENUM_GashaponGrade.SSR;
                }
                else if (value > totalSSRChance && value <= totalSSRChance + totalSRChance)
                {
                    grade = ENUM_GashaponGrade.SR;
                }
                else if (value > totalSSRChance + totalSRChance && value <= totalSRChance + totalRChance)
                {
                    grade = ENUM_GashaponGrade.R;
                }
                else if (value > totalSRChance + totalRChance && value <= totalRChance + totalNChance)
                {
                    grade = ENUM_GashaponGrade.N;
                }
            }

            return grade;
        } 
        #endregion

        #region Add2LotteryBox
        /// <summary>
        /// 新增道具到轉蛋箱 加總機率
        /// </summary>
        /// <param name="gashaponData">轉蛋資料</param>
        /// <param name="series">轉蛋系列</param> 
        private List<int> Add2LotteryBox(GashaponData[] gashaponData, Int16 series, ENUM_GashaponGrade grade)
        {
            int chance = 0,totalChance=0;
            List<int> gashaponLotteryBox = new List<int>();

            for (int i = 0; i < gashaponData.Length; i++)
            {
                if (gashaponData[i].Series == series)
                {
                    if (gashaponData[i].Grade == (byte)grade)
                    {
                        // 機率相加 = 總和機率
                        // totalNChance += gashaponData[i].N_Chance;
                        chance = gashaponData[i].Chance + nBlance;
                        totalChance += chance;

                        // 存入轉蛋箱
                        for (int x = 0; x < chance; x++)
                            gashaponLotteryBox.Add(gashaponData[i].ItemID);
                    }

                    if (gashaponData[i].Grade == (byte)grade)
                    {
                        // totalRChance += gashaponData[i].R_Chance;
                        chance = gashaponData[i].Chance + rBlance;
                        totalChance += chance;

                        for (int x = 0; x < chance; x++)
                            gashaponLotteryBox.Add(gashaponData[i].ItemID);
                    }

                    if (gashaponData[i].Grade == (byte)grade)
                    {
                        //  totalSRChance += gashaponData[i].SR_Chance;
                        chance = gashaponData[i].Chance + srBlance;
                        totalChance += chance;

                        for (int x = 0; x < chance; x++)
                            gashaponLotteryBox.Add(gashaponData[i].ItemID);
                    }

                    if (gashaponData[i].Grade == (byte)grade)
                    {
                        //  totalSSRChance += gashaponData[i].SSR_Chance;
                        chance = gashaponData[i].Chance + ssrBlance;
                        totalChance += chance;

                        for (int x = 0; x < chance; x++)
                            gashaponLotteryBox.Add(gashaponData[i].ItemID);
                    }
                }
            }

            totalGradeChance = totalChance;

            // 打亂陣列
            gashaponLotteryBox.Shuffle();

            return gashaponLotteryBox;
        }
        #endregion

        #region GenerateGashapon
        /// <summary>
        /// 產生 獲得的轉蛋
        /// </summary>
        /// <param name="gashaponLotteryBox">排序好的轉蛋箱</param>
        /// <param name="price">轉蛋價格 確認數量</param>
        /// <returns></returns>
        private GashaponData[] GenerateGashapon(List<int> gashaponLotteryBox, int price)
        {
            int value = 0;
            GashaponData[] data = default(GashaponData[]);
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

            // 取得轉出次數
            if (price == 59) count = 1;
            if (price == 149) count = 3;
            if (price == 349) count = 11;

            value = rng.Next(0, totalGradeChance);
            data = new GashaponData[count];

            for (int i = 0; i < count; i++)
            {
                data[i].ItemID = gashaponLotteryBox[value];
            }

            return data;
        }
        #endregion

        #endregion
    }

}
