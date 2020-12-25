using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ShadowAvatarSkill : SkillBoss
{
    ObjectFactory objFactory;
    CreatureSystem m_CreatureSystem;
    PoolSystem m_PoolSystem;
    Dictionary<Transform, ICreature> dictMice, buffer, buffer2;
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
      //  dictMice = new Dictionary<Transform, GameObject>();
        m_CreatureSystem = MPGame.Instance.GetCreatureSystem();
        m_PoolSystem = MPGame.Instance.GetPoolSystem();
    }

    public override void Initialize()
    {
        skillFlag = false;
        m_StartTime = m_LastTime = Time.time;

        buffer = new Dictionary<Transform, ICreature>(dictMice);
        foreach (KeyValuePair<Transform, ICreature> mice in buffer)
        {
            if (m_CreatureSystem.HasMice(mice.Value)) // 初始化時有老鼠 移除老鼠
            {
                mice.Value.Play(IAnimatorState.ENUM_AnimatorState.Died);
                mice.Value.GetAIState().SetAIState(new DiedAIState());
            }
            dictMice.Remove(mice.Key);
        }

        dictMice = new Dictionary<Transform, ICreature>();
    }

    public override void Display(ICreature creature/*, CreatureAttr arribute/*, IAIState state*/)
    {
        base.Display();
        m_Creature.SetState(ICreature.ENUM_CreatureState.Invincible);
        Debug.Log("Ninja Mice Display");
        Initialize();
        go = creature.m_go;
        skillFlag = true;

        int rnd = Random.Range(0, data.Length);
        List<GameObject> hole = MPGame.Instance.GetBattleSystem().GetHole();

        //MPGFactory.GetCreatureFactory().SpawnBy1D(short.Parse(creature.m_go.name), data, 3.5f, 0, 0, 0, 0, true, false);
        //correctMice = hole[data[rnd]].transform;

        // 這是原本的程式碼 有把實體化的老鼠加入陣列
        for (int i = 0; i < data.Length; i++)
        {
            dictMice.Add(hole[data[i]].transform, m_PoolSystem.InstantiateMice(System.Convert.ToInt16(go.name), 3.5f, hole[data[i]].transform, true));
            if (i == rnd) correctMice = hole[data[i]].transform;
        }

        buffer = new Dictionary<Transform, ICreature>(dictMice);
    }

    public override void UpdateEffect()
    {
        if (Time.time > m_LastTime + skillData.ColdDown && !skillFlag)
        {
            escapeTime = Time.time;
            Display(m_Creature/*, m_Arribute, m_AIState*/);

            Debug.Log("****RE RUN !");
        }


        if ((Time.time - m_StartTime) < skillData.SkillTime && skillFlag)
        {
            foreach (KeyValuePair<Transform, ICreature> mice in buffer)
            {
                // 如果 打到正確老鼠 
                if (!m_CreatureSystem.GetbActiveHoleMice(mice.Key)   && mice.Key == correctMice && skillFlag)
                {
                    m_Creature.SetState(ICreature.ENUM_CreatureState.Idle);
                    Debug.Log("****Correct!");
                    dictMice.Remove(mice.Key);
                    buffer2 = new Dictionary<Transform, ICreature>(dictMice);

                    foreach (KeyValuePair<Transform, ICreature> mice2 in buffer2)
                    {
                        if (m_CreatureSystem.HasMice(mice.Value)) // 初始化時有老鼠 移除老鼠
                        {
                            mice.Value.Play(IAnimatorState.ENUM_AnimatorState.Died);
                            mice.Value.GetAIState().SetAIState(new DiedAIState());
                        }
                        dictMice.Remove(mice.Key);
                    }

                    dictMice.Clear();
                    m_LastTime = Time.time;
                    skillFlag = false;
                    break;
                }
                else if (!m_CreatureSystem.HasMice(mice.Value) && skillFlag)
                {
                    Debug.Log("****Error!");
                    foreach (KeyValuePair<Transform, ICreature> mice2 in buffer)
                    {
                        if (mice2.Value != null)
                            MPGame.Instance.GetCreatureSystem().SetEffect(skillData.SkillName, null, mice2.Value.m_go.transform.parent);   // mice2.Value.   GetComponent<IMice>().OnEffect("Shadow", null);
                    }

                    dictMice[mice.Key] = MPGFactory.GetCreatureFactory().SpawnByOne(System.Convert.ToInt16(go.name), 3.5f, mice.Key, false);
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
