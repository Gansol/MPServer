using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using MPProtocol;
using System.Security.Cryptography;
using System.Text;
using Gansol;
public class AX : MonoBehaviour
{

    void Start()
    {
        List<string> b = new List<string> { "a", "b" };
        string[] a = new string[] { "a", "b" };
        String.Join(",", b.ToArray());
        Debug.Log(String.Join(",", b.ToArray()));
    }


}
