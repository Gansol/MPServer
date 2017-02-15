using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MPProtocol;
using System;
public class SkillFactory
{
    static SkillBase skill = null;
    static SkillAttr skillData =null;
    /// <summary>
    /// 取得技能
    /// </summary>
    /// <param name="objectID">物件名稱</param>
    /// <returns></returns>
    public SkillBase GetSkill(Dictionary<string, object> dictionary, short objectID)
    {
        skillData = new SkillAttr();
        short skillID = (short)System.Convert.ToInt16(ObjectFactory.GetColumnsDataFromID(dictionary, "SkillID", objectID.ToString()));
        Debug.Log("skillID:" + skillID + "   objectID: " + objectID);
        

        Dictionary<string, object> dictSkillData = new Dictionary<string, object>();

        Global.dictSkills.TryGet<Dictionary<string, object>>(skillID.ToString(), out dictSkillData);

        skillData.SkillName = Convert.ToString(dictSkillData.Get<string>("SkillName")).Replace(" ","");
        skillData.SkillLevel = Convert.ToByte(dictSkillData.Get<string>("SkillLevel"));
        skillData.SkillType = Convert.ToByte(dictSkillData.Get<string>("SkillType"));
        skillData.SkillTime = Convert.ToByte(dictSkillData.Get<string>("SkillTime"));
        skillData.ColdDown = Convert.ToByte(dictSkillData.Get<string>("ColdDown"));
        skillData.Delay = Convert.ToByte(dictSkillData.Get<string>("Delay"));
        skillData.Energy = Convert.ToByte(dictSkillData.Get<string>("Energy"));
        skillData.ProbValue = Convert.ToByte(dictSkillData.Get<string>("ProbValue"));
        skillData.ProbDice = Convert.ToByte(dictSkillData.Get<string>("ProbDice"));
        skillData.Attr = Convert.ToByte(dictSkillData.Get<string>("Attr"));
        skillData.AttrDice = Convert.ToByte(dictSkillData.Get<string>("AttrDice"));

        switch ((ENUM_Skill)skillID)
        {
            case ENUM_Skill.LesserHeal:
                //skill = new LesserHeal();
                break;
            case ENUM_Skill.Shield:
                skill = new SkillShield(skillData);
                break;
            case ENUM_Skill.CallFriends:
                skill = new SkillCallMice(skillData);
                break;
            case ENUM_Skill.SugarAnts:
                skill = new SugarAntsSkill(skillData);
                break;
            case ENUM_Skill.DontTouchMe:
                skill = new DontTouchMeSkill(skillData);
                break;
            case ENUM_Skill.ShadowAvatar:
                skill = new ShadowAvatarSkill(skillData);
                break;
            case ENUM_Skill.StealHarvest:
                skill = new StealSkill(skillData);
                break;
            case ENUM_Skill.ScorePlus:
                skill = new ScorePlus(skillData);
                break;
            case ENUM_Skill.RatTrap:
                skill = new RatTrap(skillData);
                break;
            case ENUM_Skill.Taco:
                skill = new Taco(skillData);
                break;
            case ENUM_Skill.Lighting:
                skill = new Lighting(skillData);
                break;
            case ENUM_Skill.EnergyPlus:
                skill = new EnergyPlus(skillData);
                break;
            case ENUM_Skill.SnowRain:
                skill = new Snow(skillData);
                break;
            case ENUM_Skill.FireBow:
                skill = new FireBow(skillData);
                break;
            case ENUM_Skill.IceGlasses:
                skill = new IceGlasses(skillData);
                break;
            case ENUM_Skill.Reflection:
                skill = new Reflection(skillData);
                break;
            case ENUM_Skill.Invincible:
                skill = new Invincible(skillData);
                break;
            case ENUM_Skill.FeverTime:
                skill = new FeverTime(skillData);
                break;
            default:
                Debug.LogError("Skill is Null !");
                break;
        }
        Debug.Log(skill + "   " + skillData.Attr);
        return skill;
    }
}
