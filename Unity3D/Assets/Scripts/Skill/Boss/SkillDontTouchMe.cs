using UnityEngine;
using System.Collections;

public class DontTouchMeSkill : ISkillBoss
{
    SpawnController spawner = new SpawnController();
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
        spawner.Spawn(MPProtocol.SpawnStatus.LineL, obj.name, 0.1f, 0.1f, 0.1f, 1, false);
    }
}
