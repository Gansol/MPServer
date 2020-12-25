using UnityEngine;
using System.Collections;

public abstract class ICreatureAttr : AttrBase
{
    private int hp ;
    private int maxHp ;
    private int shield ;

    public string id ;
    public string name ;

    public ICreatureAttr()
    {
        hp = 1;
        maxHp = 1;
        shield = 0;
        id = "";
        name = "";
    }

    //public CreatureAttr(string name, int hp)
    //{
    //    SetMaxHP(hp);
    //    SetHP(hp);
    //    this.name = name;
    //}

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


    public virtual void SetHP(int value=1)
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
        this.shield = Mathf.Max(0, value);
    }
}
