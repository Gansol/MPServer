using UnityEngine;
using System.Collections;

public class ShadowAvatarSkill : ISkillBoss
{
    MiceSpawner spawner = GameObject.FindGameObjectWithTag("GM").GetComponent<MiceSpawner>();
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
        sbyte[][] data = SpawnData.GetSpawnData(MPProtocol.SpawnStatus.FourPoint) as sbyte[][];
        spawner.SpawnByCustom(obj.name, data, 0.1f, 0.1f, 0.1f, 1, false, false);
    }
}
