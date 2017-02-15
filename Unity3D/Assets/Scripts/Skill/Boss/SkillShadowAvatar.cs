using UnityEngine;
using System.Collections;

public class ShadowAvatarSkill : SkillBoss
{
    public ShadowAvatarSkill(SkillAttr skill)
        : base(skill)
    {
    }

    MPFactory spawner = GameObject.FindGameObjectWithTag("GM").GetComponent<MPFactory>();
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
        sbyte[] data = SpawnData.GetSpawnData(MPProtocol.SpawnStatus.FourPoint) as sbyte[];
        spawner.SpawnBy1D(System.Convert.ToInt16(obj.name), data, 0.1f, 0.1f, 0.1f, 1,-1, false, false);// 可能錯誤
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
