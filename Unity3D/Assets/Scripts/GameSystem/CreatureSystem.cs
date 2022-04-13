using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//預備改寫 遊戲系統
public class CreatureSystem : IGameSystem
{
    public delegate void OnEffectHandler(string name, object value);
    public event OnEffectHandler OnEffect;

    Dictionary<string, Dictionary<string, ICreature>> dictBattleMice;    // 戰鬥中老鼠索引
    Dictionary<Transform, ICreature> dictBattleMice_HoleRefs;               // Hole老鼠索引
    Dictionary<string, ICreature> dictWaitingRemoveMice;                        // 等待移除老鼠列表
    
    public CreatureSystem(MPGame MPGame) : base(MPGame)
    {
        Debug.Log("--------------- CreatureSystem Create ----------------");
        Initialize();
    }

    public override void Initialize()
    {
        Debug.Log("--------------- CreatureSystem Initialize ----------------");
        dictBattleMice = new Dictionary<string, Dictionary<string, ICreature>>();
        dictBattleMice_HoleRefs = new Dictionary<Transform, ICreature>();
        dictWaitingRemoveMice = new Dictionary<string, ICreature>();
    }

    public override void Update()
    {
        CreatureUpdate();
        RemoveBattleMice();
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

    #region -- RemoveBattleMice 移除戰鬥中老鼠 --
    /// <summary>
    /// 使用等待移除的老鼠字典，來移除正在Battle中的老鼠
    /// 計算老鼠分數
    /// 重新將老鼠加入Pooling
    /// </summary>
    private void RemoveBattleMice()
    {
        // 逐步檢查每個老鼠的狀態
        foreach (KeyValuePair<string, Dictionary<string, ICreature>> miceClass in dictBattleMice)
        {
            foreach (KeyValuePair<string, ICreature> creature in miceClass.Value)
            {
                ICreature mice = creature.Value;

                // 如果老鼠狀態 = 死亡，且死亡動畫撥放完畢(逃跑)。 
                if (mice.GetAttribute().GetHP() < 1 && (mice.GetAIState() == ICreature.ENUM_CreatureAIState.Died && mice.GetAminState().GetENUM_AnimState() == IAnimatorState.ENUM_AnimatorState.MiceRunAway))
                {
                    // 計算分數、加入等待移除列表
                    m_MPGame.GetBattleSystem().UpadateScore(short.Parse(miceClass.Key), mice.GetSurvivalTime());
                    dictWaitingRemoveMice.Add(creature.Key, mice);
                }

                // 如果老鼠HP>0 且逃離動畫播放完畢( 錯誤 應使用時間判斷) = 老鼠逃跑 
                if (mice.GetAttribute().GetHP() > 0 && mice.GetAminState().GetENUM_AnimState() == IAnimatorState.ENUM_AnimatorState.MiceRunAway)
                {
                    if (mice.GetAttribute().name != "bali") //bali 錯誤
                    {
                        Debug.Log("CreatureSystem BreakCombo: " + mice.GetAttribute().name+" HP: "+ mice.GetAttribute().GetHP());
                        //斷COMBO、損失分數、加入等待移除列表
                        m_MPGame.GetBattleSystem().BreakCombo();
                        m_MPGame.GetBattleSystem().LostScore(short.Parse(miceClass.Key), mice.GetSurvivalTime());
                    }
                    else
                    {
                        Debug.Log("Bali");
                    }
                    dictWaitingRemoveMice.Add(creature.Key, mice);
                }
            }
        }

        // 如果有等待移除的老鼠
        if (dictWaitingRemoveMice.Count > 0)
        {
            // 使用miceID尋找老鼠類別，並刪除指定hashID老鼠、重新加入Pooling
            foreach (KeyValuePair<string, ICreature> mice in dictWaitingRemoveMice)
            {
                // 如果死亡動畫播放完畢 回到 Pool
                if ((mice.Value.GetAminState().GetENUM_AnimState() == IAnimatorState.ENUM_AnimatorState.MiceRunAway))
                {
                    dictBattleMice[mice.Value.m_go.name].Remove(mice.Key);
                    RemoveBattleMice_HoleRefs(mice.Value.m_go.transform.parent);
                  //  Debug.Log("RE Mice :"+ mice.Value.m_go.GetHashCode() + "Hole: "+ mice.Value.m_go.transform.parent.name);
                    m_MPGame.GetPoolSystem().AddMicePool(mice.Value); // mice.Key = hashID
                }
            }
            dictWaitingRemoveMice.Clear();
        }
    }
    #endregion

    public void AddBattleMiceRefs(Transform hole, string miceID, string hashID, ICreature mice)
    {
        if (!dictBattleMice.ContainsKey(miceID))
            dictBattleMice.Add(miceID, new Dictionary<string, ICreature>());
        dictBattleMice[miceID].Add(hashID, mice);

        if (!dictBattleMice_HoleRefs.ContainsKey(hole))
            dictBattleMice_HoleRefs.Add(hole, mice);
    }

    /// <summary>
    /// 移除老鼠索引
    /// </summary>
    /// <param name="hole"></param>
    /// <param name="mice"></param>
    /// <returns></returns>
    public void RemoveBattleMice_HoleRefs(Transform hole)
    {
        dictBattleMice_HoleRefs.Remove(hole);
    }

    /// <summary>
    /// 取得 老鼠 是否正在洞裡的
    /// </summary>
    /// <param name="hole"></param>
    /// <returns></returns>
    public bool GetHoleMice_bActive(Transform hole)
    {
        return dictBattleMice_HoleRefs.ContainsKey(hole); ;
    }

    /// <summary>
    /// 取得正在洞裡的老鼠
    /// </summary>
    /// <param name="miceID"></param>
    /// <param name="hashID"></param>
    /// <returns></returns>
    public ICreature GetMice(string miceID, string hashID)
    {
        dictBattleMice[miceID].TryGetValue(hashID, out ICreature value);
        return value;
    }

    /// <summary>
    /// 取得正在洞裡的老鼠
    /// </summary>
    /// <param name="hole"></param>
    /// <returns></returns>
    public ICreature GetMice(Transform hole)
    {
        dictBattleMice_HoleRefs.TryGetValue(hole, out ICreature value);
        return value;
    }

    public Dictionary<string, Dictionary<string, ICreature>> GetCreatures()
    {
        return dictBattleMice;
    }

    public bool HasMice(ICreature mice)
    {
        if (mice != null)
            return dictBattleMice[mice.m_go.name].ContainsValue(mice);
        return false;
    }

    public void SetEffect(string name, object vaule)
    {
        OnEffect(name, vaule);
    }

    public void SetEffect(string skillName, object vaule, Transform hole)
    {
        foreach (KeyValuePair<string, Dictionary<string, ICreature>> miceClass in dictBattleMice)
        {
            foreach (KeyValuePair<string, ICreature> mice in miceClass.Value)
            {
                if (mice.Value.m_go.transform.parent == hole)
                    mice.Value.m_go.SendMessage("OnEffect", skillName);
            }
        }
    }

    public override void Release()
    {
        base.Release();
        dictBattleMice.Clear();
        dictBattleMice_HoleRefs.Clear();
        dictWaitingRemoveMice.Clear();
    }
}
