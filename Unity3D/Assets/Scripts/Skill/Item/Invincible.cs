﻿using UnityEngine;
using System.Collections;

public class Invincible : SkillItem {
    public Invincible(SkillAttr attr)
        : base(attr)
    {
        playerState |= MPProtocol.ENUM_PlayerState.Invincible;
    }

    public override void Initialize()
    {
        throw new System.NotImplementedException();
    }

    public override void UpdateEffect()
    {
        if (Time.time - m_StartTime > skillData.SkillTime - 3)
        {
            // battleHUD shing
            // playerAIState.ShingICON();
        }

        if (Time.time - m_StartTime > skillData.SkillTime)
        {
            Debug.Log(skillData.SkillName+" Release");
            Release();
        }
    }

    public override void Release()
    {
        BattleManager.SetInvincible(false); // 錯誤

        foreach(GameObject go in effects)
            GameObject.Destroy(go);
        playerAIState.Release(playerState);    // 錯誤 這裡如果一次來兩個狀態就會BUG
    }

    public override void Display(GameObject obj, CreatureAttr arribute, AIState state)
    {
        Display();
    }

    public override void Display()
    {
        Debug.Log(skillData.SkillName+" Display: "+skillData.Attr);
        AssetLoader assetLoader = MPGame.Instance.AssetLoader();
        GameObject bundle = assetLoader.GetAsset(skillData.SkillName + "Effect");
        ObjectFactory objFactory = new ObjectFactory();

        effects.Add(objFactory.Instantiate(bundle, GameObject.Find("HUD(Panel)").transform, skillData.SkillName+"Effect", Vector3.zero, Vector3.one, Vector2.one, 1));
        effects[0].GetComponent<Animator>().Play("Yellow");

        BattleManager.SetInvincible(true);  // 錯誤

        m_StartTime = Time.time;
    }
}
