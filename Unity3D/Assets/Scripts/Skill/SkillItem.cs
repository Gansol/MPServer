using UnityEngine;
using System.Collections;
using MPProtocol;
using System.Collections.Generic;
public abstract class SkillItem : SkillBase
{
    protected List<GameObject> effects;
    protected ENUM_PlayerState playerState;
    protected PlayerAIState playerAIState; // AIController
    protected float startTime;

    public SkillItem(SkillAttr attr)
        : base(attr)
    {
        effects = new List<GameObject>();
    }

    public virtual ENUM_PlayerState GetPlayerState()
    {
        return playerState;
    }

    public virtual void SetAIController(PlayerAIState playerAIState)
    {
        this.playerAIState = playerAIState;
    }
}
