using UnityEngine;
using System.Collections;

public class StealSkill : SkillBoss
{
    public StealSkill(SkillAttr skill)
        : base(skill)
    {
    }
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public override void Initialize()
    {
        throw new System.NotImplementedException();
    }

    public override void Display(GameObject obj, CreatureAttr arribute, AIState state)
    {
        Global.photonService.SendBossSkill(MPProtocol.ENUM_Skill.StealHarvest);
    }

    public override void UpdateEffect()
    {
        throw new System.NotImplementedException();
    }

    public override void Release()
    {
        throw new System.NotImplementedException();
    }

    public override void Display()
    {
        throw new System.NotImplementedException();
    }
}
