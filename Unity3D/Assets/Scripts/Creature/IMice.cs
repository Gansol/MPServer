using UnityEngine;
using System.Collections;

public abstract class IMice : ICreature
{
    protected static UIPlaySound hitSound;
    public IMice() { }

    public override void Initialize()
    {
      //  m_go.GetComponent<BoxCollider2D>().enabled = true;
    }


    public override void Update()
    {
            m_AI.UpdateAI();
    }

    /// <summary>
    /// 接收效果
    /// </summary>
    /// <param name="name">存活時間</param>
    /// <param name="value">數值1</param>
    public abstract void OnEffect(string name, object value);

    /// <summary>
    /// 受傷
    /// </summary>
    /// <param name="damage"></param>
    protected override void OnInjured(short damage, bool myAttack)
    {
        m_Attribute.SetHP(Mathf.Max(0, m_Attribute.GetHP() - damage));
    }

    public override void SetSkill(ISkill skill)
    {
        if (m_Skill != null)
            m_Skill.Release();
        m_Skill = skill;
    }
    public override void SetAI(ICreatureAI creatureAI)
    {
        if (m_AI != null)
            m_AI = null; // release GC 解構
        m_AI = creatureAI;
    }

    public override void SetGameObject(GameObject go)
    {
        if (m_go != null)
            m_go = null; // release GC 解構
        m_go = go;
    }

    //public override void SetState(IAIState state)
    //{
    //    if (this.m_AIState != null)
    //        this.m_AIState = null;
    //    this.m_AIState = state;
    //}

    public override void SetAnimState(IAnimatorState state)
    {
        if (m_AnimState != null)
            m_AnimState = null;
        m_AnimState = state;
    }

    public override void SetAttribute(ICreatureAttr attribute)
    {
        if (m_Attribute != null)
            m_Attribute = null;
        m_Attribute = attribute;
    }

    public override void Release()
    {
        SetAI(null);
        SetSkill(null);
        SetAttribute(null);
        SetAnimState(null);
        SetGameObject(null);
    }
}
