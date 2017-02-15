using UnityEngine;
using System.Collections;

public class SugarAntsSkill : SkillBoss
{
    public SugarAntsSkill(SkillAttr skill)
        : base(skill)
    {
    }

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

    //public override void Display()
    //{
    //    arribute.SetHP(arribute.GetHP() + System.Convert.ToInt32(_tmpHp / 3));
    //}

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
