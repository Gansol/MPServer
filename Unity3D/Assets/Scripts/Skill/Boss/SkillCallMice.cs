using UnityEngine;
using System.Collections;

public class SkillCallMice : SkillBoss
{
    MPFactory spawner = GameObject.FindGameObjectWithTag("GM").GetComponent<MPFactory>();

    public SkillCallMice(SkillAttr skill)
        : base(skill)
    {
    }
    

    public override void Initialize()
    {
        throw new System.NotImplementedException();
    }

    public override void Display(GameObject obj, CreatureAttr arribute, AIState state)
    {
//        spawner.Spawn(MPProtocol.SpawnStatus.LineL, "BlackMice", 0.1f, 0.1f, 0.1f, 3, false);
//        Debug.Log("Spawn");
        //state = new InvincibleState();


        sbyte[] data = SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineL) as sbyte[];
        spawner.SpawnBy1D(10002, data, 0.1f, 0.1f, 0.1f, 3, Random.Range(0, data.Length), false, false);  //錯誤
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
