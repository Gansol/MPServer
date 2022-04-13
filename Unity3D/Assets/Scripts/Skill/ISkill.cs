﻿using UnityEngine;
using System.Collections;
using MPProtocol;

public abstract class ISkill
{
    protected ICreature m_Creature = null;
    protected GameObject skillEffect = null;
    protected GameObject skillItem = null;
    protected SkillAttr skillData = null;
    protected ENUM_PlayerState playerState;
    protected PlayerAIState playerAIState; // AIController
    protected float m_LastTime, m_StartTime;

    public ISkill(SkillAttr skillData)
    {
        if (this.skillData != null)
            this.skillData = null;
        this.skillData = skillData;
    }

    public interface ISkill_Interface
    {
        void Display(ICreature creature/*, CreatureAttr arribute, IAIState state*/);
    }

    public abstract void Initialize();
    public abstract void UpdateEffect();
    public abstract void Release();
    /// <summary>
    /// 使用生物技能
    /// </summary>
    /// <param name="go"></param>
    /// <param name="attribute"></param>
    /// <param name="state"></param>
    public virtual void Display(ICreature creature/*, CreatureAttr attribute, IAIState state*/)
    {
        m_Creature = creature;
    }

    /// <summary>
    /// 使用道具技能
    /// </summary>
    public virtual void Display() { }

    public virtual ENUM_PlayerState GetPlayerState()
    {
        return playerState;
    }

    public virtual void SetAIController(PlayerAIState playerAIState)
    {
        this.playerAIState = playerAIState;
    }

    public void SetSkillItem(GameObject skillItem)
    {
        if (this.skillItem != null)
            this.skillItem = null;
        this.skillItem = skillItem;
    }

    public short GetID()
    {
        return skillData.SkillID;
    }

    public short GetSkillTime()
    {
        return skillData.SkillTime;
    }

    public short GetColdDown()
    {
        return skillData.ColdDown;
    }
}