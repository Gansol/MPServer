using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ShadowAvatarSkill : SkillBoss
{

    PoolManager poolManager = GameObject.FindGameObjectWithTag("GM").GetComponent<PoolManager>();
    SpawnAI spawnAI = GameObject.FindGameObjectWithTag("GM").GetComponent<BattleManager>().GetSpawnAI();
    ObjectFactory objFactory;

    Dictionary<Transform, GameObject> dictMice, buffer, buffer2;
    sbyte[] data;
    float escapeTime;
    bool skillFlag;
    Transform correctMice;

    public ShadowAvatarSkill(SkillAttr skill)
        : base(skill)
    {
        skillFlag = false;
        objFactory = new ObjectFactory();
        data = SpawnData.GetSpawnData(MPProtocol.SpawnStatus.FourPoint) as sbyte[];
        dictMice = new Dictionary<Transform, GameObject>();
    }

    public override void Initialize()
    {
        skillFlag = false;
        m_StartTime = m_LastTime = Time.time;

        buffer = new Dictionary<Transform, GameObject>(dictMice);
        foreach (KeyValuePair<Transform, GameObject> mice in buffer)
        {
            if (Global.dictBattleMice.ContainsKey(mice.Key))
                dictMice[mice.Key].GetComponent<MiceBase>().SendMessage("OnDead", 0);
            dictMice.Remove(mice.Key);
        }

        dictMice = new Dictionary<Transform, GameObject>();
    }

    public override void Display(GameObject obj, CreatureAttr arribute, AIState state)
    {
        Debug.Log("Ninja Mice Display");

        Initialize();

        skillFlag = true;
        this.obj = obj;
        int rnd = Random.Range(0, data.Length);

        for (int i = 0; i < data.Length; i++)
        {
            dictMice.Add(spawnAI.GetHole(data[i]).transform, objFactory.InstantiateMice(poolManager, System.Convert.ToInt16(obj.name), 3.5f, spawnAI.GetHole(data[i]).gameObject, true));
            if (i == rnd) correctMice =spawnAI.GetHole(data[i]).transform;
        }

        buffer = new Dictionary<Transform, GameObject>(dictMice);
    }

    public override void UpdateEffect()
    {
        if (Time.time > m_LastTime + skillData.ColdDown && !skillFlag)
        {
            escapeTime = Time.time;
            Display(obj, null, null);
            
            Debug.Log("****RE RUN !");
        }


        if ((Time.time - m_StartTime) < skillData.SkillTime && skillFlag)
        {
            foreach (KeyValuePair<Transform, GameObject> mice in buffer)
            {
                // 如果 打到正確老鼠 
                if (!Global.dictBattleMice.ContainsValue(mice.Value) && mice.Key == correctMice && skillFlag)
                {
                    Debug.Log("****Correct!");
                    dictMice.Remove(mice.Key);
                    buffer2 = new Dictionary<Transform, GameObject>(dictMice);

                    foreach (KeyValuePair<Transform, GameObject> mice2 in buffer2)
                    {
                        if (Global.dictBattleMice.ContainsKey(mice2.Key))
                           dictMice[mice2.Key].GetComponent<MiceBase>().SendMessage("OnDead", 0.0f);
                        dictMice.Remove(mice.Key);
                    }

                    dictMice.Clear();
                    m_LastTime = Time.time;
                    skillFlag = false;
                    break;
                }
                else if (!Global.dictBattleMice.ContainsValue(mice.Value) && skillFlag)
                {
                    Debug.Log("****Error!");
                    foreach (KeyValuePair<Transform, GameObject> mice2 in buffer)
                    {
                        if (mice2.Value != null)
                            mice2.Value.GetComponent<MiceBase>().OnEffect("Shadow", null);
                    }

                    dictMice[mice.Key] = objFactory.InstantiateMice(poolManager, System.Convert.ToInt16(obj.name), 3.5f, mice.Key.gameObject, false);

                    buffer = dictMice;
                    break;
                }

            }
        }

        if ((Time.time - m_StartTime) > skillData.SkillTime)
            Release();
    }

    public int GetShadowCount()
    {
        return dictMice.Count;
    }


    public override void Release()
    {
        Initialize();
//        playerAIState.Release(MPProtocol.ENUM_PlayerState.Boss);
    }

    public override void Display()
    {
        Debug.Log("SkillShadowAvatar NotImplementedException!");
    }
}
