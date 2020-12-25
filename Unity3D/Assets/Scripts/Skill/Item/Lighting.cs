using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Lighting : SkillItem
{
    public Lighting(SkillAttr attr)
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
        if (Time.time - m_StartTime > skillData.SkillTime)
        {
            Debug.Log(skillData.SkillName+" Release");
            Release();
        }
    }

    public override void Release()
    {
        foreach (GameObject go in effects)
            GameObject.Destroy(go);
        playerAIState.Release(playerState);    // 錯誤 這裡如果一次來兩個狀態就會BUG
    }

    public override void Display()
    {
        Debug.Log(skillData.SkillName + " Display");

        AssetLoaderSystem assetLoader = MPGame.Instance.GetAssetLoaderSystem();
        GameObject bundle = assetLoader.GetAsset("effect_" + skillData.SkillName);
        List<Transform> holeBuffer = new List<Transform>(); // not rnd hole
        List<Transform> rndHole = new List<Transform>(); // ok  rnd hole 

        int count = skillData.Attr + Random.Range(0, skillData.AttrDice);


        //MPGame.Instance.GetCreatureSystem().SetEffect(skillData.SkillName, 0);

        foreach (KeyValuePair<string, Dictionary<string, ICreature>>  miceClass in MPGame.Instance.GetCreatureSystem().GetCreatures())
            foreach(KeyValuePair <string, ICreature> mice in miceClass.Value)
            holeBuffer.Add(mice.Value.m_go.transform.parent);

        count = Mathf.Min(count, holeBuffer.Count);

        // add rndHole
        for (int i = count; i > 0; i--)
        {
            int value = Random.Range(0, holeBuffer.Count);
            rndHole.Add(holeBuffer[value]);
            holeBuffer.Remove(holeBuffer[value]);
        }

        // insantiate effect and play
        for (int i = 0; i < count; i++)
        {
            // 如果在動上的老鼠已經消失 則不顯示技能
            if (rndHole[i] != null)
            {
                effects.Add(MPGFactory.GetObjFactory().Instantiate(bundle, rndHole[i], "effect_" + skillData.SkillName, Vector3.zero, Vector3.one, Vector2.one, 1));
                effects[i].GetComponent<Animator>().Play("Layer1.Effect1", -1, 0f);
            }
        }
        m_StartTime = Time.time;
    }
}
