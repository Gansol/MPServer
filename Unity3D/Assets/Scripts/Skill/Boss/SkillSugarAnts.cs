using UnityEngine;
using System.Collections;

public class SugarAntsSkill : ISkillBoss
{
    CreatureAttr arribute = null;
    private int _tmpHp = 0;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (arribute.GetHP() < arribute.GetHP() / 3)
        {

        }
    }

    public override void Initialize()
    {
        throw new System.NotImplementedException();
    }

    public override void Display(GameObject obj, CreatureAttr arribute, AIState state)
    {
        _tmpHp = arribute.GetHP();
        this.arribute = arribute;
        Global.photonService.UpdateScoreRate(MPProtocol.ENUM_ScoreRate.Low);
    }

    public override void Display2(GameObject obj, CreatureAttr arribute, AIState state)
    {
        arribute.SetHP(arribute.GetHP() + System.Convert.ToInt32(_tmpHp / 3));
    }
}
