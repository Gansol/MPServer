using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ShadowAvatarSkill : SkillBoss
{
    MPGame objFactory;

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
            if (Global.dictBattleMiceRefs.ContainsKey(mice.Key))
           /     dictMice[mice.Key].GetComponent<IMice>().SendMessage("OnDead", 0);
            dictMice.Remove(mice.Key);
        }

        dictMice = new Dictionary<Transform, GameObject>();
    }

    public override void Display(GameObject obj, CreatureAttr arribute/*, IAIState state*/)
    {
        Debug.Log("Ninja Mice Display");

        Initialize();

        skillFlag = true;
        this.obj = obj;
        int rnd = Random.Range(0, data.Length);
        List<GameObject> hole = MPGame.Instance.GetBattleSystem().GetHole();

        for (int i = 0; i < data.Length; i++)
        {
            dictMice.Add(hole[data[i]].transform, MPGame.Instance.GetPoolSystem().InstantiateMice(System.Convert.ToInt16(obj.name), 3.5f, hole[data[i]].gameObject, true));
            if (i == rnd) correctMice = hole[data[i]].transform;
        }

        buffer = new Dictionary<Transform, GameObject>(dictMice);
    }

    public override void UpdateEffect()
    {
        if (Time.time > m_LastTime + skillData.ColdDown && !skillFlag)
        {
            escapeTime = Time.time;
            Display(obj, null);
            
            Debug.Log("****RE RUN !");
        }


        if ((Time.time - m_StartTime) < skillData.SkillTime && skillFlag)
        {
            foreach (KeyValuePair<Transform, GameObject> mice in buffer)
            {
                // 如果 打到正確老鼠 
                if (!Global.dictBattleMiceRefs.ContainsValue(mice.Value) && mice.Key == correctMice && skillFlag)
                {
                    Debug.Log("****Correct!");
                    dictMice.Remove(mice.Key);
                    buffer2 = new Dictionary<Transform, GameObject>(dictMice);

                    foreach (KeyValuePair<Transform, GameObject> mice2 in buffer2)
                    {
                        if (Global.dictBattleMiceRefs.ContainsKey(mice2.Key))
                        錯誤   dictMice[mice2.Key].GetComponent<IMice>().SendMessage("OnDead", 0.0f);
                        dictMice.Remove(mice.Key);
                    }

                    dictMice.Clear();
                    m_LastTime = Time.time;
                    skillFlag = false;
                    break;
                }
                else if (!Global.dictBattleMiceRefs.ContainsValue(mice.Value) && skillFlag)
                {
                    Debug.Log("****Error!");
                    foreach (KeyValuePair<Transform, GameObject> mice2 in buffer)
                    {
                        if (mice2.Value != null)
                            mice2.Value.GetComponent<IMice>().OnEffect("Shadow", null);
                    }

                    dictMice[mice.Key] = objFactory.InstantiateMice( System.Convert.ToInt16(obj.name), 3.5f, mice.Key.gameObject, false);

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
