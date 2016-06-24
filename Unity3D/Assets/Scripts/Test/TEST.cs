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
        for (int i = 0; i < 3; i++)
        {
            StartCoroutine(t.A(i));
        }

        for (int i = 0; i < 3; i++)
        {
            StartCoroutine(t.A(i));
        }
    }
}
