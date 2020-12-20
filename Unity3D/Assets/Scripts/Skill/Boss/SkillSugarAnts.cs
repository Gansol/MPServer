using UnityEngine;
using System.Collections;

public class SugarAntsSkill : SkillBoss
{
    private bool skillFlag;

    public SugarAntsSkill(SkillAttr skill)
        : base(skill)
    {
        skillFlag = true;
    }

    CreatureAttr arribute = null;

    public override void Initialize()
    {
        skillFlag = true;
    }

    // Update is called once per frame
    public override void UpdateEffect()
    {
        if (Time.time > m_LastTime + skillData.ColdDown && (Time.time - m_StartTime) < skillData.SkillTime)
        {
            if (arribute.GetHP() < arribute.GetHP() / 3 && skillFlag)
            {
                obj.GetComponent<Animator>().Play("Effect1");
                arribute.SetHP(arribute.GetHP() + skillData.Attr + Random.Range(0, skillData.AttrDice + 1));
                skillFlag = false;
            }
        }

        if ((Time.time - m_StartTime) > skillData.SkillTime)
            Release();
    }


    public override void Display(GameObject obj, CreatureAttr arribute/*, IAIState state*/)
    {
        Debug.Log("SugerAnts Display!");
        this.obj = obj;
        this.arribute = arribute;
        Global.photonService.UpdateScoreRate(MPProtocol.ENUM_Rate.Low);

    }

    public override void Release()
    {
        Global.photonService.UpdateScoreRate(MPProtocol.ENUM_Rate.Normal);
    }

    public override void Display()
    {
        throw new System.NotImplementedException();
    }
}
