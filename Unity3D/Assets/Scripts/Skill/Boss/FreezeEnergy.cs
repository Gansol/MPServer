using UnityEngine;
using System.Collections;

public class FreezeEnergy : SkillBoss
{
    private bool skillFlag;
    public FreezeEnergy(SkillAttr skill)
        : base(skill)
    {
        skillFlag = false;
    }

    public override void Initialize()
    {
        skillFlag = false;
    }

    public override void Display(GameObject ob, CreatureAttr arribute/*, IAIState state*/)
    {
        // Global.photonService.SendBossSkill(MPProtocol.ENUM_Skill.StealHarvest, true);
    }

    public override void UpdateEffect()
    {
        if (Time.time > m_LastTime + skillData.ColdDown && (Time.time - m_StartTime) < skillData.SkillTime)
        {
            // "-" 分數是正的
            Global.photonService.UpdateEnergyRate(MPProtocol.ENUM_Rate.None);
            m_LastTime = Time.time;
        }

        if ((Time.time - m_StartTime) > skillData.SkillTime)
            Release();
    }

    public override void Release()
    {
        Global.photonService.UpdateEnergyRate(MPProtocol.ENUM_Rate.Normal);
    }

    public override void Display()
    {
        skillFlag = true;
        m_LastTime = Time.time;
    }
}
