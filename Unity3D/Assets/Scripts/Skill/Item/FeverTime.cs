using UnityEngine;
using System.Collections;

public class FeverTime : SkillItem {

    public FeverTime(SkillAttr attr)
        : base(attr)
    {
        playerState |= MPProtocol.ENUM_PlayerState.ScorePlus;
        playerState |= MPProtocol.ENUM_PlayerState.EnergyPlus;
        playerState |= MPProtocol.ENUM_PlayerState.Protected;
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
        Global.photonService.UpdateScoreRate(MPProtocol.ENUM_Rate.Normal);
        BattleManager.SetPropected(false);

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
        AssetLoader assetLoader = MPGame.Instance.GetAssetLoader();
        GameObject bundle = assetLoader.GetAsset("effect_" + skillData.SkillName);
        ObjectFactory objFactory = new ObjectFactory();

        effects.Add(objFactory.Instantiate(bundle, GameObject.Find("HUD(Panel)").transform, "effect_" + skillData.SkillName, Vector3.zero, Vector3.one, Vector2.one, 1));
        effects[0].GetComponent<Animator>().Play("Layer1.Green",-1,0f);

        Global.photonService.UpdateScoreRate(MPProtocol.ENUM_Rate.High);
        Global.photonService.UpdateEnergyRate(MPProtocol.ENUM_Rate.High);
        BattleManager.SetPropected(true);
        m_StartTime = Time.time;
    }
}
