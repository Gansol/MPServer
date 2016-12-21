using UnityEngine;
using System.Collections;

public class DontTouchMeSkill : ISkillBoss
{
    MiceSpawner spawner = GameObject.FindGameObjectWithTag("GM").GetComponent<MiceSpawner>();
    // Use this for initialization
    void Start()
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
        spawner.SpawnBy1D(obj.name, data, 0.1f, 0.1f, 0.1f, 1, Random.Range(0, data.Length), false, false);
    }
}
