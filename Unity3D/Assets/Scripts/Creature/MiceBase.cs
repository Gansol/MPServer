using UnityEngine;
using System.Collections;

public abstract class MiceBase : Creature
{
    public abstract void Initialize(float lerpSpeed, float upSpeed, float upDistance, float lifeTime);

    /// <summary>
    /// On Touch / On Click
    /// </summary>
    protected abstract void OnHit();

    /// <summary>
    /// 死亡
    /// </summary>
    /// <param name="lifeTime">存活時間</param>
    protected abstract void OnDead(float lifeTime);

    /// <summary>
    /// 受傷
    /// </summary>
    /// <param name="damage"></param>
    protected override void OnInjured(short damage)
    {
        m_Arribute.SetHP(Mathf.Max(0, m_Arribute.GetHP() - damage));
    }

    public override void SetSkill(Skill skill)
    {
        if (this.m_Skill != null)
            this.m_Skill.Release();
        this.m_Skill = skill;
    }

    public override void SetState(AIState state)
    {
        if (this.m_AIState != null)
            this.m_AIState = null;
        this.m_AIState = state;
    }

    public override void SetArribute(CreatureAttr arribute)
    {
        if (this.m_Arribute != null)
            this.m_Arribute = null;
        this.m_Arribute = arribute;
    }


}
