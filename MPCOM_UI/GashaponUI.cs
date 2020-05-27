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
 * 老鼠資料界面層 提供外部存取使用
 * 載入老鼠資料
 * 
 * ***************************************************************/

namespace MPCOM
{
    public interface IGashaponUI    // 使用介面 可以提供給不同程式語言繼承使用      
    {

        byte[] BuyGashapon(int itemID, byte itemType, Int16 series, int price);
    }

    public class GashaponUI : ServicedComponent, IGashaponUI
    {
        protected override bool CanBePooled()
        {
            return true;
        }

        #region BuyGashapon 購買轉蛋商品
        /// <summary>
        /// 購買轉蛋商品
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public byte[] BuyGashapon(int itemID, byte itemType, Int16 series, int price)
        {
            GashaponData[] gashaponData = default( GashaponData[]);
            gashaponData[0].ReturnCode = "(UI)S1200";
            gashaponData[0].ReturnMessage = "";

            try
            {
                GashaponLogic gashaponLogic = new GashaponLogic();
                gashaponData = gashaponLogic.BuyGashapon( itemID,  itemType,  series,  price);
            }
            catch (Exception e)
            {
                gashaponData[0].ReturnCode = "S1299";
                gashaponData[0].ReturnMessage = e.Message;
                throw;
            }
            return TextUtility.SerializeToStream(gashaponData);
        }
        #endregion
    }
}