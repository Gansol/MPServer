using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;


public class BX : MonoBehaviour
{
    void Awake()
    {

        Debug.Log("F");
    }

    private void A()
    {
        B();
    }
    private void B()
    {
        Debug.Log("B");
    }

}
