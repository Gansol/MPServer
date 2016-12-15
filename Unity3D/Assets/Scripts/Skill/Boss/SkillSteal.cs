using UnityEngine;
using System.Collections;

public class StealSkill : ISkillBoss
{

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
        Global.photonService.SendSkillBoss(MPProtocol.ENUM_Skill.StealHarvest);
    }
}
