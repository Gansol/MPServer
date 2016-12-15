using UnityEngine;
using System.Collections;

public abstract class Creature : MonoBehaviour
{
    public AnimatorState AnimState = null;

    protected Skill m_Skill = null;
    protected AIState m_AIState = null;
    protected CreatureAttr m_Arribute = null;


    public abstract void SetSkill(Skill skill);
    public abstract void SetState(AIState state);
    public abstract void SetArribute(CreatureAttr arribute);

    protected abstract void OnInjured(short damage);
}
