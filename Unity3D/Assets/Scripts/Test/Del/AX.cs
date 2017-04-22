using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using MPProtocol;

public class AX : MonoBehaviour
{

    enum fuc : short
    {
        a,
        b,
        c,
    }

    void HAHA(int ff)
    {
        Debug.Log((int)ff);
    }
    void Start()
    {
        transform.GetChild(0).localScale = Vector3.Lerp(transform.GetChild(0).localScale, new Vector3(.5f, .5f),0.1f);
        transform.GetChild(0).localScale = Vector3.Lerp(transform.GetChild(0).localScale, new Vector3(.5f, .5f), 0.1f);
        transform.GetChild(0).localScale = Vector3.Lerp(transform.GetChild(0).localScale, new Vector3(.5f, .5f), 0.1f);
        transform.GetChild(0).localScale = Vector3.Lerp(transform.GetChild(0).localScale, new Vector3(.5f, .5f), 0.1f);
    }

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
