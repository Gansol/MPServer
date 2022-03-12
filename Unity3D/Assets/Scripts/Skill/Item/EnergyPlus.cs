﻿using UnityEngine;
using System.Collections;

public class EnergyPlus : SkillItem
{
    public EnergyPlus(SkillAttr attr)
        : base(attr)
    {
        playerState |= MPProtocol.ENUM_PlayerState.EnergyPlus;
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
            Debug.Log("EnergyPlus Release");
            Release();
        }
    }

    public override void Release()
    {
        Global.photonService.UpdateEnergyRate(MPProtocol.ENUM_Rate.Normal);
        playerAIState.Release(playerState);    // 錯誤 這裡如果一次來兩個狀態就會BUG
    }

    public override void Display(GameObject obj, CreatureAttr arribute, AIState state)
    {
        Display();
    }

    public override void Display()
    {
        Debug.Log(skillData.SkillName + " Display: " + skillData.Attr);
        AssetLoader assetLoader = MPGame.Instance.GetAssetLoader();
        GameObject bundle = assetLoader.GetAsset(skillData.SkillName + "Effect");
        ObjectFactory objFactory = new ObjectFactory();

        effects.Add(objFactory.Instantiate(bundle, GameObject.Find("HUD(Panel)").transform, skillData.SkillName + "Effect", Vector3.zero, Vector3.one, Vector2.one, 1));
        effects[0].GetComponent<Animator>().Play("Blue");

        Global.photonService.UpdateEnergyRate(MPProtocol.ENUM_Rate.High);

        m_StartTime = Time.time;
    }

    public override void Initialize()
    {
        throw new System.NotImplementedException();
    }
}
