using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using MPProtocol;

public class AX : MonoBehaviour
{

    
    public virtual void Start()
    {
        Debug.Log("Base");
    }


    //void Start()
    //{
    //    BX b = new BX();
    //    b.Method(SayHello);
    //}

    //void SayHello(string name)
    //{
    //    Debug.Log("Hello" + name);
    //}

    /*
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.GetComponent<BX>())
        {
            col.gameObject.GetComponent<BX>().FUCK();
        }
    }*/
}
