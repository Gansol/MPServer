using UnityEngine;
using System.Collections.Generic;
using System.Text;
using MiniJSON;

public class TEST : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        TEST2 t = new TEST2();
       StartCoroutine( t.A());
       StartCoroutine(t.B());
    }
}
