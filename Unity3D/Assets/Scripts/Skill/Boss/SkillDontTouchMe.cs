using UnityEngine;
using System.Collections;

public class DontTouchMeSkill : SkillBoss
{
    ICreature creature;
    bool _bClick;
    int _spawnCount;
    sbyte[] data;

    public DontTouchMeSkill(SkillAttr skill/*, SpawnAI spawnAI*/)
        : base(skill)
    {
       // this.spawnAI = spawnAI;
        data = SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineL) as sbyte[];
    }


    public override void Initialize()
    {

    }

    public override void Display(ICreature creature/*, CreatureAttr arribute/*, IAIState state*/)
    {
        this.creature = creature;
        go = creature.m_go;
        _spawnCount = skillData.Attr + Random.Range(0, skillData.AttrDice + 1);
    MPGFactory.GetCreatureFactory().SpawnBy1D(System.Convert.ToInt16(go.name), data, 0.1f, 0.1f, 0.1f, _spawnCount, Random.Range(0, data.Length), false, false); // 可能錯誤
        m_LastTime = Time.time;
    }

    public override void UpdateEffect()
    {
        if (Time.time > m_LastTime + skillData.ColdDown && (Time.time - m_StartTime) < skillData.SkillTime)
        {
            Display(creature/*, null*/);
        }
        if ((Time.time - m_StartTime) > skillData.SkillTime)
            Release();
    }

    public override void Release()
    {
       // playerAIState.Release(MPProtocol.ENUM_PlayerState.Boss);
    }

    public override void Display()
    {
        throw new System.NotImplementedException();
    }
}
