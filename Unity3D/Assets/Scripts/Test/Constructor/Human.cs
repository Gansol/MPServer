using UnityEngine;
using System.Collections;

public class Human : MonoBehaviour
{

    public string name;
    public int age;
    public bool sex;

    public Human(string name, int age, bool sex)
    {
        this.name = name;
        this.age = age;
        this.sex = sex;

        Debug.Log("name:" + name + "  age:" + age + "  sex:" + sex);
    }

    public virtual void SayHello(){
        Debug.Log("Hello i'm people!");
    }

}
