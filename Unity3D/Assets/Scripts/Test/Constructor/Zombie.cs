using UnityEngine;
using System.Collections;

public class Zombie : Human
{
    bool posion;

    public Zombie(string name, int age, bool sex, bool posion)
        : base(name, age, sex)
    {
        this.posion = posion;
        Debug.Log("posion:"+this.posion);
    }

    public new void  SayHello()
    {
        Debug.Log("Hello i'm Zombie!!!");
    }
}
