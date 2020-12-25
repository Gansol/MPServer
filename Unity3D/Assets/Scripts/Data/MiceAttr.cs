using UnityEngine;
using System.Collections;

public class MiceAttr : ICreatureAttr
{

    public float EatingRate;
    public float MiceSpeed;
    public short EatFull;
    public short SkillID;
    public byte MiceCost;
    public byte SkillTimes;
    public float LifeTime;

    public MiceAttr() 
    {
    }
}
