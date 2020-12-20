using UnityEngine;
using System.Collections;

public abstract class IMice : ICreature
{
    public IMice() { }
    protected static UIPlaySound hitSound;

    public abstract void Initialize(bool isBoss, float lerpSpeed, float upSpeed, float upDistance, float lifeTime);


    public override void Update()
    {
        m_AI.UpdateAIState();
    }

    /// <summary>
    /// On Touch / On Click
    /// </summary>
    protected abstract void OnHit();

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
        m_Arribute.SetHP(Mathf.Max(0, m_Arribute.GetHP() - damage));
    }

    public override void SetSkill(SkillBase skill)
    {
        if (this.m_Skill != null)
            this.m_Skill.Release();
        this.m_Skill = skill;
    }
    public override void SetAI(ICreatureAI creatureAI)
    {
        if (m_AI != null)
            m_AI = null; // release GC 解構
        m_AI = creatureAI;
    }

    public override void SetGameObject(GameObject go)
    {
        if (this.m_go != null)
            this.m_go = null; // release GC 解構
        this.m_go = go;
    }

    //public override void SetState(IAIState state)
    //{
    //    if (this.m_AIState != null)
    //        this.m_AIState = null;
    //    this.m_AIState = state;
    //}

    public override void SetAnimState(IAnimatorState state)
    {
        if (this.m_AnimState != null)
            this.m_AnimState = null;
        this.m_AnimState = state;
    }

    public override void SetArribute(CreatureAttr arribute)
    {
        if (this.m_Arribute != null)
            this.m_Arribute = null;
        this.m_Arribute = arribute;
    }
}
