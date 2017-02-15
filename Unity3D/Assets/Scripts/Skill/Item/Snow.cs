using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Snow : SkillItem {

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
        if (Time.time - startTime > skillData.SkillTime - 3)
        {
            // battleHUD shing
            // playerAIState.ShingICON();
        }

        if (Time.time - startTime > skillData.SkillTime)
        {
            Debug.Log(skillData.SkillName + " Release");
            Release();
        }
    }

    public override void Release()
    {
        // resume motion
        foreach (KeyValuePair<Transform, GameObject> mice in Global.dictBattleMice)
            mice.Value.GetComponent<MiceBase>().OnEffect(skillData.SkillName, true);

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
        AssetLoader assetLoader = playerAIState.GetAssetLoader();
        GameObject bundle = assetLoader.GetAsset(skillData.SkillName + "Effect");
        ObjectFactory objFactory = new ObjectFactory();

        effects.Add(objFactory.Instantiate(bundle, GameObject.Find("HUD(Panel)").transform, skillData.SkillName + "Effect", Vector3.zero, Vector3.one, Vector2.one, 1));
        //  effects[0].GetComponent<Animator>().Play("Freeze");

        // stop motion
        foreach (KeyValuePair<Transform, GameObject> item in Global.dictBattleMice)
        {
            item.Value.GetComponent<MiceBase>().OnEffect(skillData.SkillName, false);
        }

        startTime = Time.time;
    }
}
