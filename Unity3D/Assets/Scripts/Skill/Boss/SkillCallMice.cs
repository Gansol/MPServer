using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SkillCallMice : SkillBoss
{
   // SpawnAI spawnAI = GameObject.FindGameObjectWithTag("GM").GetComponent<BattleSystem>().GetSpawnAI();
   //private readonly Dictionary<Transform, IMice> dictMice;
   // private  Dictionary<Transform, IMice>  buffer;
    private IMice mice; 

    private int _spawnCount;
    sbyte[] data;

    public SkillCallMice(SkillAttr skill)
        : base(skill)
    {
        data = SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineL) as sbyte[];
       // dictMice = new Dictionary<Transform, IMice>();
    }

    public override void Initialize()
    {
        m_StartTime = m_LastTime = Time.time;
       // dictMice.Clear();
    }

    public override void Display(ICreature creature/*, CreatureAttr arribute/*, IAIState state*/)
    {
        Debug.Log("Call Mice Display");

        int spawnCount = skillData.Attr + Random.Range(0, skillData.AttrDice + 1);
    //    int[] rndHole = new int[spawnCount];
        //List<GameObject> hole = MPGame.Instance.GetBattleSystem().GetHole();
   //     System.Random rnd = new System.Random();

      //  mice = (IMice) creature;
        data = SpawnData.GetSpawnData(MPProtocol.SpawnStatus.LineL) as sbyte[];
        

     
        
        //for (int i = 0; i < spawnCount;i++ )
        //    rndHole[i] = rnd.Next(data.Min(), data.Max());

        m_LastTime = Time.time;

    //    dictMice.Clear();
        //go = mice.m_go;

   MPGFactory.GetCreatureFactory().SpawnByRandom(System.Convert.ToInt16(go.name), data,3.5f,.2f,.2f,spawnCount,true);
    }

    public override void UpdateEffect()
    {
        if (Time.time > m_LastTime + skillData.ColdDown && (Time.time - m_StartTime) < skillData.SkillTime)
        {
            //foreach (KeyValuePair<Transform, IMice> mice in buffer)
            //{
            //    if (!Global.dictBattleMiceRefs.ContainsValue(mice.Value)) dictMice.Remove(mice.Key);
            //}

            Display(mice/*, null/*, null*/);
            m_LastTime = Time.time;
        }

        if ((Time.time - m_StartTime) > skillData.SkillTime)
            Release();
    }
    //public int GetMiceCount()
    //{
    //    return dictMice.Count;
    //}

    public override void Release()
    {
     //   dictMice.Clear();
       // playerAIState.Release(MPProtocol.ENUM_PlayerState.Boss);
    }

    public override void Display()
    {
    }
}
