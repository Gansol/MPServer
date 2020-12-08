using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SkillCallMice : SkillBoss
{
   // SpawnAI spawnAI = GameObject.FindGameObjectWithTag("GM").GetComponent<BattleSystem>().GetSpawnAI();
   private readonly Dictionary<Transform, GameObject> dictMice;
    private  Dictionary<Transform, GameObject>  buffer;


    private int _spawnCount;
    sbyte[] data;

    public SkillCallMice(SkillAttr skill)
        : base(skill)
    {
        data = SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineL) as sbyte[];
        dictMice = new Dictionary<Transform, GameObject>();
    }

    public override void Initialize()
    {
        m_StartTime = m_LastTime = Time.time;
        dictMice.Clear();
    }

    public override void Display(GameObject obj, CreatureAttr arribute, AIState state)
    {
        data = SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineL) as sbyte[];
        Debug.Log("Call Mice Display");
        int spawnCount = skillData.Attr + Random.Range(0, skillData.AttrDice + 1);
        int[] rndHole = new int[spawnCount];
        System.Random rnd = new System.Random();
        
        for (int i = 0; i < spawnCount;i++ )
            rndHole[i] = rnd.Next(data.Min(), data.Max());

        m_LastTime = Time.time;

        dictMice.Clear();
        this.obj = obj;

        List<GameObject> hole = MPGame.Instance.GetBattleSystem().GetHole();
        for (int i = 0; i < spawnCount; i++)
        {
            dictMice.Add(hole[i].transform,  MPGFactory.GetObjFactory().InstantiateMice(System.Convert.ToInt16(obj.name), 3.5f, hole[rndHole[i]].gameObject, true));
        }

        buffer = new Dictionary<Transform, GameObject>(dictMice);
    }

    public override void UpdateEffect()
    {
        if (Time.time > m_LastTime + skillData.ColdDown && (Time.time - m_StartTime) < skillData.SkillTime)
        {
            foreach (KeyValuePair<Transform, GameObject> mice in buffer)
            {
                if (!Global.dictBattleMiceRefs.ContainsValue(mice.Value)) dictMice.Remove(mice.Key);
            }

            Display(obj, null, null);
            m_LastTime = Time.time;
        }

        if ((Time.time - m_StartTime) > skillData.SkillTime)
            Release();
    }
    public int GetMiceCount()
    {
        return dictMice.Count;
    }

    public override void Release()
    {
        dictMice.Clear();
       // playerAIState.Release(MPProtocol.ENUM_PlayerState.Boss);
    }

    public override void Display()
    {
    }
}
