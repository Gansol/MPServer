using UnityEngine;
using System.Collections;

public class IceGlasses : SkillItem
{
    TapBorad tap;
    private int attr, clickTimes;
    private float animTime;

    public IceGlasses(SkillAttr attr)
        : base(attr)
    {
        playerState |= MPProtocol.ENUM_PlayerState.IceGlasses;
    }

    public override void Initialize()
    {
        throw new System.NotImplementedException();
    }

    public override void UpdateEffect()
    {
        clickTimes = tap.GetTimes();
        animTime = tap.gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime;
        if (Time.time - m_StartTime > skillData.SkillTime - 3)
        {
            // battleHUD shing
            // playerAIState.ShingICON();
        }

        if (Time.time - m_StartTime > skillData.SkillTime || clickTimes >= attr && animTime > .6f)
        {
            Debug.Log(skillData.SkillName + " Release");
            Release();
        }
    }

    public override void Release()
    {
        EventMaskSwitch.Resume();
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
        ObjectFactory objFactory = new ObjectFactory();

        bundle = objFactory.Instantiate(bundle, GameObject.Find("HUD(Panel)").transform, "effect_" + skillData.SkillName, Vector3.zero, Vector3.one, Vector2.one, 1);
        attr = skillData.Attr + Random.Range(0, skillData.AttrDice);
        bundle.AddComponent<TapBorad>().SetTimes(attr);
        tap = bundle.GetComponent<TapBorad>();
        effects.Add(bundle);
        effects[0].GetComponent<Animator>().Play("Effect1");
        EventMaskSwitch.Switch(bundle/*, false*/);
        m_StartTime = Time.time;
    }
}
