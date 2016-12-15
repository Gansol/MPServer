using UnityEngine;
using System.Collections;

public abstract class Skill
{
    public interface ISkill
    {
        void OnSkill(GameObject obj, CreatureAttr arribute);
    }

    protected GameObject skillEffect = null;
    protected int skillType = -1;
    protected int skillLevel = -1;
    protected float coldDown = -1;
    protected float interval = -1;
    protected float delay = -1;

    public abstract void Initialize();
    public abstract void Display(GameObject obj, CreatureAttr arribute, AIState state);
    public virtual void Display2(GameObject obj, CreatureAttr arribute, AIState state) { }

    public void Release()
    {
        Debug.Log("FQ");
    }
}