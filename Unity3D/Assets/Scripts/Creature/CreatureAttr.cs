using UnityEngine;
using System.Collections;

public abstract class CreatureAttr
{
    private int hp = 1;
    private int maxHp = 1;
    private int shield = 0;

    public virtual int GetHP()
    {
        return hp;
    }

    public virtual int GetMaxHP()
    {
        return maxHp;
    }

    public virtual float GetHPPrecent()
    {
        if (maxHp > 0)
            return Mathf.Max(0, (float)hp / (float)maxHp);
        else
            return 0;
    }

    public virtual int GetShield()
    {
        return shield;
    }


    public virtual void SetHP(int value)
    {
        this.hp = Mathf.Min(value, maxHp);
    }

    public virtual void SetMaxHP(int value)
    {
        this.maxHp = value;
        this.hp = Mathf.Min(this.hp, value);
    }

    public virtual void SetShield(int value)
    {
        this.shield =  Mathf.Max(0,value);
    }
}
