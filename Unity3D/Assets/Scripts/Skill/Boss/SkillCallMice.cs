using UnityEngine;
using System.Collections;

public class SkillCallMice : ISkillBoss
{
    SpawnController spawner = GameObject.FindGameObjectWithTag("GM").GetComponent<SpawnController>();

    public override void Initialize()
    {
        throw new System.NotImplementedException();
    }

    public override void Display(GameObject obj, CreatureAttr arribute, AIState state)
    {
        spawner.Spawn(MPProtocol.SpawnStatus.LineL, "BlackMice", 0.1f, 0.1f, 0.1f, 3, false);
        Debug.Log("Spawn");
        //state = new InvincibleState();
    }
}
