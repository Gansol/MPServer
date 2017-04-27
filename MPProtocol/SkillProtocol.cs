using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPProtocol
{
    public enum ENUM_Skill : int
    {
        LesserHeal = 1,
        Shield,
        CallFriends,
        SugarAnts,
        DontTouchMe,
        ShadowAvatar,
        StealHarvest,
        RandomSkill,
        ScorePlus,
        RatTrap,
        Taco,
        Lighting,
        EnergyPlus,
        SnowRain,
        FireBow,
        IceGlasses,
        Reflection,
        Invincible,
        FeverTime,
        FreezeEnergy,
    }
    public enum ENUM_SkillType : int
    {
        Self = 0,
        Buff = 1,
        Debuff = 2,
        Passive = 3,
        Damage = 4,
    }

    public enum SkillOperationCode
    {
        LoadSkill = 111,             // 載入技能資料
    }

    public enum SkillParameterCode
    {
        Ret = 0,                      // 回傳碼
        SkillData = 1,                 // 技能資料
        SkillID,
        SkillName,
        SkillLevel,
        SkillType,

        SkillTime,
        ColdDown,
        Delay,
        Energy,
        ProbValue,
        ProbDice,
        Attr,
        AttrDice,
    }

    public enum SkillResponseCode
    {
        LoadSkill = 111,             // 載入技能資料
    }
}
