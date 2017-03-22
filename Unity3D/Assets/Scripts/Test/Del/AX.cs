using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using MPProtocol;

public class AX : MonoBehaviour
{

    void OnCollision2DEnter(Collision2D col)
    {
        col.gameObject.GetComponent<ParticleSystem>().enableEmission = true;
        Debug.Log("Fuck");
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
