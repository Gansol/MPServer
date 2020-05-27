using UnityEngine;
using UnityEngine.Advertisements;
using System;
using System.Collections.Generic;
using Gansol;

public class AX : MonoBehaviour
{
    void Start()
    {
        string b;
        Gansol.TextUtility tx = new TextUtility();
        Debug.Log(b = tx.EncryptBase64String("http://180.218.164.56:58767/MicePowBETA"));
        tx = new TextUtility();
        Debug.Log(tx.DecryptBase64String("aHR0cDovLzE4MC4yMTguMTY0LjIzMjo1ODc2Ny9NaWNlUG93QkVUQQ=="));


    }

}