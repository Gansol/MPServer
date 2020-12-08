using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireBow : SkillItem
{
    public FireBow(SkillAttr attr)
        : base(attr)
    {
    }

    public override void Initialize()
    {
        throw new System.NotImplementedException();
    }

    public override void UpdateEffect()
    {
        if (Time.time - m_StartTime > skillData.SkillTime - 3)
        {
            // battleUI shing
            // playerAIState.ShingICON();
        }

        if (Time.time - m_StartTime > skillData.SkillTime)
        {
            Debug.Log(skillData.SkillName + " Release");
            Release();
        }
    }

    public override void Release()
    {
        foreach (GameObject go in effects)
            GameObject.Destroy(go);

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
        GameObject bundle = assetLoader.GetAsset("effect_" + skillData.SkillName);
        
       // effects.Add(objFactory.Instantiate(bundle, GameObject.Find("HUD(Panel)").transform, "effect_" + skillData.SkillName, Vector3.zero, Vector3.one, Vector2.one, 1));
        effects.Add(MPGFactory.GetObjFactory().Instantiate(bundle, MPGame.Instance.GetBattleSystem().GetHole()[0].transform.parent.transform, "effect_" + skillData.SkillName, Vector3.zero, Vector3.one, Vector2.one, 1));
        effects[0].GetComponent<Animator>().Play("Layer1.Effect1",-1,0f);

        m_StartTime = Time.time;
    }
}
