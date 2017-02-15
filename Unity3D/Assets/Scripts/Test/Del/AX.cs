using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using MPProtocol;

public class AX : MonoBehaviour
{
    public delegate void A(string name);
    
    void Start()
    {
        BX b = GetComponent<BX>();
        b.StartCoroutineDelegate(XC);
    }

     IEnumerator XC()
    {
        Debug.Log("XC");
        yield return new WaitForSeconds(1);
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
