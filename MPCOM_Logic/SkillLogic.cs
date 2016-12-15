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
    public class SkillLogic : ServicedComponent// ServicedComponent 表示所有使用 COM+ 服務之類別的基底類別。
    {
        SkillData skillData = new SkillData();


        protected override bool CanBePooled()
        {
            return true;
        }

        #region LoadSkillProperty

        [AutoComplete]
        public SkillData GetSkillProperty(int skillType)
        {
            skillData.ReturnCode = "(Logic)S500";
            skillData.ReturnMessage = "";

            try
            {
                switch (skillType)
                {
                    case (int)ENUM_Skill.StealHarvest:
                        skillData.SkillProperty = "-50";
                        skillData.ReturnCode = "S510";
                        skillData.ReturnMessage = "取得技能屬性成功！";
                        break;
                    default:
                        skillData.ReturnCode = "S511";
                        skillData.ReturnMessage = "取得技能屬性失敗！";
                        break;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return skillData;
        }

        #endregion
    }
}
