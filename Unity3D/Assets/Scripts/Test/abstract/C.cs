using UnityEngine;
using System.Collections;

public class C : B,InterfaceD<C> {

    public override void Test()
    {
        //Debug.Log("C");
    }


   public void Fly()
    {
        Debug.Log("Flying!");
    }
}
