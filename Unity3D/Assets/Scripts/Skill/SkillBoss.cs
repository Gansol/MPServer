using UnityEngine;
using System.Collections;

public abstract class SkillBoss : SkillBase
{
    protected GameObject  obj;

    public SkillBoss(SkillAttr skill)
        : base(skill)
    {
        playerState |= MPProtocol.ENUM_PlayerState.Boss;
        m_StartTime = m_LastTime = Time.time;
    }
}
