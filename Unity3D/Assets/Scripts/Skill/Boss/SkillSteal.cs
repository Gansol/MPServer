using UnityEngine;
using System.Collections;

public class StealSkill : SkillBoss
{
    private bool skillFlag;
    public StealSkill(SkillAttr skill)
        : base(skill)
    {
        skillFlag = false;
    }

    public override void Initialize()
    {
        skillFlag = false;
    }

    public override void Display(GameObject obj, CreatureAttr arribute, AIState state)
    {
       // Global.photonService.SendBossSkill(MPProtocol.ENUM_Skill.StealHarvest, true);
    }

    public override void UpdateEffect()
    {
        if (Time.time > m_LastTime + skillData.ColdDown && (Time.time - m_StartTime) < skillData.SkillTime)
        {
            // "-" 分數是正的
            Global.photonService.Damage((short)(-skillData.Attr + -Random.Range(0, skillData.AttrDice + 1)), true);
            m_LastTime = Time.time;
        }

        if ((Time.time - m_StartTime) > skillData.SkillTime)
            Release();
    }

    public override void Release()
    {
        //throw new System.NotImplementedException();
    }

    public override void Display()
    {
        skillFlag = true;
        Global.photonService.Damage((short)(skillData.Attr + Random.Range(0, skillData.AttrDice + 1)), true);
        m_LastTime = Time.time;
    }
}
