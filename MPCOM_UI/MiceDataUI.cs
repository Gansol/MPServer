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
    public interface IMiceDataUI    // 使用介面 可以提供給不同程式語言繼承使用      
    {
        byte[] LoadMiceData();
    }

    public class MiceDataUI : ServicedComponent, IMiceDataUI
    {
        protected override bool CanBePooled()
        {
            return true;
        }

        #region LoadMiceData 載入老鼠資料
        public byte[] LoadMiceData()
        {
            MiceData miceData = new MiceData();
            miceData.ReturnCode = "S800";
            miceData.ReturnMessage = "";

            try
            {
                MiceDataLogic miceDataLogic = new MiceDataLogic();
                miceData = miceDataLogic.LoadMiceData();
            }
            catch (Exception e)
            {
                miceData.ReturnCode = "S800";
                miceData.ReturnMessage = e.Message;
            }
            return TextUtility.SerializeToStream(miceData);
        }
        #endregion

    }
}
