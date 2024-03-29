﻿using UnityEngine;
using System.Collections;

public class SugarAntsSkill : SkillBoss
{
    private bool skillFlag;

    public SugarAntsSkill(SkillAttr skill)
        : base(skill)
    {
        skillFlag = true;
    }

    ICreatureAttr attribute = null;

    public override void Initialize()
    {
        skillFlag = true;
    }

    // Update is called once per frame
    public override void UpdateEffect()
    {
        if (Time.time > m_LastTime + skillData.ColdDown && (Time.time - m_StartTime) < skillData.SkillTime)
        {
            if (attribute.GetHP() < attribute.GetHP() / 3 && skillFlag)
            {
                go.GetComponent<Animator>().Play("Effect1");
                attribute.SetHP(attribute.GetHP() + skillData.Attr + Random.Range(0, skillData.AttrDice + 1));
                skillFlag = false;
            }
        }

        if ((Time.time - m_StartTime) > skillData.SkillTime)
            Release();
    }


    public override void Display(ICreature creature/*, CreatureAttr arribute/*, IAIState state*/)
    {
        Debug.Log("SugerAnts Display!");
        this.go = go;
        this.attribute = attribute;
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
