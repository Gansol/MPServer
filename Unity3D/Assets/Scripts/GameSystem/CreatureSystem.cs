using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//預備改寫 遊戲系統
public class CreatureSystem : GameSystem
{

    Dictionary<string, Dictionary<string, ICreature>> dictBattleMice;
    Dictionary<string, ICreature> dictWaitingRemoveMice;

    public CreatureSystem(MPGame MPGame) : base(MPGame) { }


    public override void Initinal()
    {
        dictBattleMice = new Dictionary<string, Dictionary<string, ICreature>>();
        dictWaitingRemoveMice = new Dictionary<string, ICreature>();
    }

    public override void Update()
    {
        CreatureUpdate();
        RemoveMice();
    }

    private void CreatureUpdate()
    {
        foreach (KeyValuePair<string, Dictionary<string, ICreature>> miceClass in dictBattleMice)
        {
            foreach (KeyValuePair<string, ICreature> mice in miceClass.Value)
            {
                mice.Value.Update();
            }
        }
    }

    /// <summary>
    /// 使用等待移除的老鼠字典，來移除正在Battle中的老鼠
    /// 計算老鼠分數
    /// 重新將老鼠加入Pooling
    /// </summary>
    private void RemoveMice()
    {
        // if mice state = died
        // return pooling
        // change state = pooling
        // init mice


        foreach (KeyValuePair<string, Dictionary<string, ICreature>> miceClass in dictBattleMice)
        {
            foreach (KeyValuePair<string, ICreature> creature in miceClass.Value)
            {
                IMice mice = (IMice)creature.Value;

                // 如果老鼠狀態 = 死亡。 計算分數、加入等待移除列表
                if (mice.GetState() == ICreature.ENUM_CreatureState.Die)
                {
                    m_MPGame.GetBattleSystem().UpadateScore(short.Parse(miceClass.Key), mice.GetSurvivalTime());
                    dictWaitingRemoveMice.Add(creature.Key, creature.Value);
                }

                // 如果老鼠狀態 = 逃跑。 損失分數、加入等待移除列表
                if (mice.GetState() == ICreature.ENUM_CreatureState.ByeBye)
                {
                    m_MPGame.GetBattleSystem().LostScore(short.Parse(miceClass.Key), mice.GetSurvivalTime());
                    dictWaitingRemoveMice.Add(creature.Key, creature.Value);
                }
            }
        }


        // 如果有等待移除的老鼠
        if (dictWaitingRemoveMice.Count > 0)
        {
            // 使用miceID尋找老鼠類別，並刪除指定hashID老鼠、重新加入Pooling
            foreach (KeyValuePair<string, ICreature> mice in dictWaitingRemoveMice)
            {
                dictBattleMice[mice.Value.m_go.name].Remove(mice.Key);
                m_MPGame.GetPoolSystem().AddMicePool(mice.Value.m_go.name, mice.Key, (IMice)mice.Value);
            }
        }
    }

    public void AddMice(string miceID, string hashID, ICreature mice)
    {
        if (!dictBattleMice.ContainsKey(miceID))
            dictBattleMice.Add(miceID, new Dictionary<string, ICreature>());

        dictBattleMice[miceID].Add(hashID, mice);
    }

    public void RemoveMice(string miceID, string hashID)
    {
        if (dictBattleMice.ContainsKey(miceID))
        {
            dictBattleMice[miceID].TryGetValue(hashID, out ICreature value);
            dictWaitingRemoveMice.Add(miceID, value);
            dictBattleMice[miceID].Remove(hashID);
        }
    }

    public ICreature GetMice(string miceID, string hashID)
    {
        dictBattleMice[miceID].TryGetValue(hashID, out ICreature value);
        return value;
    }
}
