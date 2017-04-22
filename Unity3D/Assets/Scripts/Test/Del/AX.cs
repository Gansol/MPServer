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

    void Start(){
        Debug.Log(Encrypt("a"));
        Debug.Log(Encrypt("b"));
    }

    private string Encrypt(string data)
    {
        string tmpString = TextUtility.SHA512Complier(Gansol.TextUtility.SerializeToStream(data));
        return TextUtility.SHA1Complier(Gansol.TextUtility.SerializeToStream(tmpString));
    }
}
