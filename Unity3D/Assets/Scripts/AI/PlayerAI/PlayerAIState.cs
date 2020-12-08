using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MPProtocol;
using System.Linq;

public class PlayerAIState
{

    short State;
    private BattleSystem battleManager;
    Dictionary<ENUM_PlayerState, SkillBase> _dictSkills = new Dictionary<ENUM_PlayerState, SkillBase>();

    // Use this for initialization
    public PlayerAIState(BattleSystem battleManager)
    {
        this.battleManager = battleManager;
    }

    // Update is called once per frame
    public void UpdatePlayerState()
    {
        // 防止 out of sync 需要替代Key
        List<ENUM_PlayerState> keys = new List<ENUM_PlayerState>(_dictSkills.Keys);

        if (_dictSkills.Count != 0)
        {
            foreach (ENUM_PlayerState key in keys)
            {
                _dictSkills[key].UpdateEffect();
            }
        }
    }

    //private void DisplayPlayerState(){

    //    if ((State & (short)ENUM_PlayerState.ScorePlus) > 0)
    //    {
    //        // display image
    //    }
    //    else
    //    {
    //        // hide image
    //    }

    //}

    //private void ShowICON(short skillID)
    //{

    //}

    //public void ShingICON(short skillID)
    //{

    //}

    //private void DisableICON()
    //{

    //}

    public void SetPlayerAIState(ENUM_PlayerState state, SkillBase skill)
    {
        // show image
        //ShowICON(skill.GetID());
        Debug.Log("SetPlayerAIState:" + skill);

        // 如果已有舊的狀態 移除 後 覆蓋
        if ((this.State & (short)state) > 0)
        {
            SkillBase oldSkill;
            _dictSkills.TryGetValue(state, out oldSkill);   // 找出重複且已存在技能 (舊的技能)
            oldSkill.Release();
            Release(state);
        }

        this.State ^= (short)state;
        if (!_dictSkills.ContainsKey(state))
            _dictSkills.Add(state, skill);
    }


    // 錯誤 這裡如果一次來兩個狀態就會BUG
    public void Release(ENUM_PlayerState state)
    {
        if (_dictSkills.ContainsKey(state))
        {
            Debug.Log("Release:" + state);
            // BattlePanel disable image
            _dictSkills.Remove(state);
            this.State ^= (short)state;
        }
    }
}
