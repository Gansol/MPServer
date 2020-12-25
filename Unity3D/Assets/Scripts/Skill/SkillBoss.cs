using UnityEngine;
using System.Collections;

public abstract class SkillBoss : ISkill
{
    protected GameObject  go;

    public SkillBoss(SkillAttr skill)
        : base(skill)
    {
        playerState |= MPProtocol.ENUM_PlayerState.Boss;
        m_StartTime = m_LastTime = Time.time;
    }
}
