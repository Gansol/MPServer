using UnityEngine;
using System.Collections;

public class DontTouchMeSkill : SkillBoss
{
    MPFactory spawner = GameObject.FindGameObjectWithTag("GM").GetComponent<MPFactory>();

    public DontTouchMeSkill(SkillAttr skill)
        : base(skill)
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void Initialize()
    {
        throw new System.NotImplementedException();
    }

    public override void Display(GameObject obj, CreatureAttr arribute, AIState state)
    {
        //        spawner.Spawn(MPProtocol.SpawnStatus.LineL, obj.name, 0.1f, 0.1f, 0.1f, 1, false);

        sbyte[] data = SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineL) as sbyte[];
        spawner.SpawnBy1D(System.Convert.ToInt16(obj.name), data, 0.1f, 0.1f, 0.1f, 1, Random.Range(0, data.Length), false, false); // 可能錯誤
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
