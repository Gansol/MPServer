using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MPProtocol;
using System;
using Gansol;
public class SkillFactory : IFactory
{
    static ISkill skill = null;
    static SkillAttr skillData = null;

    /// <summary>
    /// 取得技能
    /// </summary>
    /// <param name="dictionary">技能所在字典</param>
    /// <param name="objectID">物件名稱</param>
    /// <returns></returns>
    public ISkill GetSkill(Dictionary<string, object> dictionary, short objectID)
    {

        short skillID = (short)System.Convert.ToInt16(MPGFactory.GetObjFactory().GetColumnsDataFromID(dictionary, "SkillID", objectID.ToString()));
        Debug.Log("skillID:" + skillID + "   objectID: " + objectID);

        GetSkillProperty(skillID);


        switch ((ENUM_Skill)skillID)
        {
            case ENUM_Skill.LesserHeal:
                skill = new LesserHeal(skillData);
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

            case ENUM_Skill.FreezeEnergy:
                skill = new FreezeEnergy(skillData);
                break;
            default:
                Debug.LogError("Skill is Null !");
                break;
        }
        Debug.Log(skill + "   " + skillData.Attr);
        return skill;
    }

    public List<ISkill> GetSkillsByType(ENUM_SkillType skillType)
    {
        List<ISkill> list = new List<ISkill>();

        foreach (KeyValuePair<string, object> skill in Global.dictSkills)
        {
            Dictionary<string, string> skillAttr = skill.Value as Dictionary<string, string>;
            if ((short)skillType == Convert.ToInt16(skillAttr["SkillType"]))
                list.Add(GetSkill(Global.dictSkills, Convert.ToInt16(skill.Key)));
        }

        return list;
    }


    private SkillAttr GetSkillProperty(short skillID)
    {
        skillData = new SkillAttr();
        Dictionary<string, object> dictSkillData = new Dictionary<string, object>();

        Global.dictSkills.TryGet<Dictionary<string, object>>(skillID.ToString(), out dictSkillData);

        skillData.SkillID = Convert.ToInt16(dictSkillData.Get<string>("SkillID"));
        skillData.SkillName = Convert.ToString(dictSkillData.Get<string>("SkillName")).Replace(" ", "");
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

        return skillData;
    }
}
