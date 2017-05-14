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
    public GameObject f;
    public UIInput x;

    public void SXtart(GameObject go ,UIInput input)
    {
        f = go;
        x = input;
        Debug.Log(go.GetComponent<UIInput>().value);
        Debug.Log(input.value);
    }
    void Update()
    {
        Debug.Log(f.GetComponent<UIInput>().value);
        Debug.Log(x.value);
    }

}
