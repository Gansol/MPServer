using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

public class TEST : MonoBehaviour
{
    float a = 50;

    void Start()
    {
        Debug.Log("A: " + a);
        Debug.Log("B: " + -a*2);
        float tmp;
        tmp = a;
        Debug.Log("C: " + tmp);
        Debug.Log("D: " + -tmp*2);
    }

 
}
