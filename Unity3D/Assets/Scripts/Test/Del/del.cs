using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using MPProtocol;

public class del : MonoBehaviour
{
    int c = 0;
    static Dictionary<string, GameObject> a;

    void Start()
    {

    }


    public void Message(){
        Debug.Log("FUCK");
        enabled = false;
    }

    public void Send(ref Dictionary<string, GameObject> f, Dictionary<string, GameObject> c)
    {
        a = f;
        a["1"].GetComponent<del2>().enabled = false;
        a["1"].SendMessage("Active");
        c["1"] = null;
        //a["1"].GetComponent<del2>().Active();
    }
}
