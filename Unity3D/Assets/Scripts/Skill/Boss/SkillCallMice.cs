using UnityEngine;
using System.Collections;

public class SkillCallMice : ISkillBoss
{
    MiceSpawner spawner = GameObject.FindGameObjectWithTag("GM").GetComponent<MiceSpawner>();

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
        spawner.SpawnBy1D("BlackMice", data, 0.1f, 0.1f, 0.1f, 3, Random.Range(0, data.Length), false, false);
    }
}
