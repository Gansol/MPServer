using UnityEngine;
using System.Collections;

public abstract class SkillBase
{
    protected GameObject skillEffect = null;
    protected GameObject skillItem = null;
    protected SkillAttr skillData = null;
    //protected string skillID = "";
    //protected string skillName = "";
    //protected int skillType = -1;
    //protected int skillLevel = -1;
    //protected float skillTime = -1;
    //protected float coldDown = -1;
    //protected float delay = -1;
    //protected float energy = -1;
    //protected float probValue = -1;
    //protected float probDice = -1;
    //protected float attr = -1;
    //protected float attrDice = -1;

    public SkillBase(SkillAttr skillData)
    {
        if (this.skillData != null)
            this.skillData = null;
        this.skillData = skillData;
    }

    public interface ISkill
    {
        void Display(GameObject obj, CreatureAttr arribute, AIState state);
    }

    public abstract void Initialize();
    public abstract void UpdateEffect();
    public abstract void Release();
    /// <summary>
    /// 使用生物技能
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="arribute"></param>
    /// <param name="state"></param>
    public virtual void Display(GameObject obj, CreatureAttr arribute, AIState state) { }

    /// <summary>
    /// 使用道具技能
    /// </summary>
    public virtual void Display() { }


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
}