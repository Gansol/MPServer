using UnityEngine;
using System.Collections;

public class MiceAI : ICreatureAI
{
    public MiceAI(ICreature mice) : base(mice)
    {
        SetAIState(new IdleAIState());
    }
}
