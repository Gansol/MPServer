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
        string account = "dfsf";
        string[] memeberAccount = account.Split('@');
        account = memeberAccount[0];

        Debug.Log(account);
    }
}
