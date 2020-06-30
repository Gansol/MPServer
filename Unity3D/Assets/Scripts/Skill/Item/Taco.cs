using UnityEngine;
using System.Collections;

public class Taco : SkillItem
{
    private float _skillTime;
    private bool _animFlag;

    public Taco(SkillAttr attr)
        : base(attr)
    {
        playerState |= MPProtocol.ENUM_PlayerState.None;
    }

    public override void Initialize()
    {
        throw new System.NotImplementedException();
    }

    public override void UpdateEffect()
    {
        if (Time.time - m_StartTime > .5f && !_animFlag)
        {
            _skillTime = skillData.Attr + Random.Range(0, skillData.AttrDice);
            float speed = (float)1 / _skillTime;
            effects[0].GetComponent<Animator>().speed = speed;
            effects[0].GetComponent<Animator>().Play("Effect2");

            _animFlag = true;
        }

        if (Time.time - m_StartTime > _skillTime - 3)
        {
            // battleHUD shing
            // playerAIState.ShingICON();
        }

        if ((Time.time - m_StartTime) > _skillTime)
        {
            Debug.Log(skillData.SkillName + " Release");
            Release();
        }
    }

    public override void Release()
    {
        effects[0].GetComponent<Animator>().speed = 1;

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
        _skillTime = skillData.Attr + Random.Range(0, skillData.AttrDice);
        Debug.Log(skillData.SkillName + " Display: " + skillData.Attr);
        AssetLoader assetLoader = MPGame.Instance.GetAssetLoader();
        GameObject bundle = assetLoader.GetAsset("effect_" + skillData.SkillName);
        ObjectFactory objFactory = new ObjectFactory();

        effects.Add(objFactory.Instantiate(bundle, GameObject.Find("HUD(Panel)").transform, "effect_" + skillData.SkillName, Vector3.zero, Vector3.one, Vector2.one, 1));
        effects[0].GetComponent<Animator>().Play("Effect1");

        m_StartTime = Time.time;
    }
}
