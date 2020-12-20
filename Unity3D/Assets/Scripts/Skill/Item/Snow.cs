using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Snow : SkillItem
{

    private bool animFlag;

    public Snow(SkillAttr attr)
        : base(attr)
    {
        playerState |= MPProtocol.ENUM_PlayerState.Freeze;
    }

    public override void Initialize()
    {
        throw new System.NotImplementedException();
    }

    public override void UpdateEffect()
    {
        if (Time.time - m_StartTime > skillData.SkillTime - 1 && !animFlag)
        {
            effects[0].GetComponent<Animator>().Play("Layer1.Effect1", -1, 0f);
            animFlag = true;
        }

        if (Time.time - m_StartTime > skillData.SkillTime)
        {
            Debug.Log(skillData.SkillName + " Release");
            Release();
        }
    }

    public override void Release()
    {
        // resume motion
        foreach (KeyValuePair<Transform, GameObject> mice in Global.dictBattleMiceRefs)
        {
            if (mice.Value != null) 
                mice.Value.GetComponent<IMice>().OnEffect(skillData.SkillName, true);
        }

        foreach (GameObject go in effects)
        {
            if (go.GetComponent<UIPlaySound>())
                GameObject.Destroy(go.GetComponent<UIPlaySound>());
            GameObject.Destroy(go);   
        }

        playerAIState.Release(playerState);    // 錯誤 這裡如果一次來兩個狀態就會BUG
    }

    public override void Display(GameObject obj, CreatureAttr arribute/*, IAIState state*/)
    {
        Display();
    }

    public override void Display()
    {
        Debug.Log(skillData.SkillName + " Display: " + skillData.Attr);
        AssetLoader assetLoader = MPGame.Instance.GetAssetLoader();
        GameObject bundle = assetLoader.GetAsset("effect_" + skillData.SkillName);
        ObjectFactory objFactory = new ObjectFactory();

        effects.Add(objFactory.Instantiate(bundle, GameObject.Find("HUD(Panel)").transform, "effect_" + skillData.SkillName, Vector3.zero, Vector3.one, Vector2.one, 1));
        effects[0].GetComponent<Animator>().Play("Layer1.Effect1", -1, 0f);

        // stop motion
        foreach (KeyValuePair<Transform, GameObject> item in Global.dictBattleMiceRefs)
        {
            if (item.Value != null)
            {
                //item.Value.GetComponent<MiceBase>().Play(AnimatorState.ENUM_AnimatorState.Frozen);
                item.Value.GetComponent<IMice>().OnEffect(skillData.SkillName, false);
            }
        }

        m_StartTime = Time.time;
    }
}
