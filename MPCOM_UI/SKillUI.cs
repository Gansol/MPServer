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
    public interface ISkillUI   // 使用介面 可以提供給不同程式語言繼承使用      
    {
        byte[] GetSkillProperty(int skillType);
    }

    public class SkillUI : ServicedComponent, ISkillUI
    {
        protected override bool CanBePooled()
        {
            return true;
        }

        #region GetSkillProperty 載入技能資料
        public byte[] GetSkillProperty(int skillType)
        {
            SkillData skillData = new SkillData();
            skillData.ReturnCode = "S800";
            skillData.ReturnMessage = "";

            try
            {
                SkillLogic skillLogic = new SkillLogic();
                skillData = skillLogic.GetSkillProperty(skillType);
            }
            catch (Exception e)
            {
                skillData.ReturnCode = "S800";
                skillData.ReturnMessage = e.Message;
            }
            return TextUtility.SerializeToStream(skillData);
        }
        #endregion

    }
}
