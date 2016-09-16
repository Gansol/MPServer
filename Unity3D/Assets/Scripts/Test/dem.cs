using UnityEngine;
using System.Collections.Generic;
using MiniJSON;
using System;
public class dem : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        List<string> a = new List<string>();
        List<List<string>> b = new List<List<string>>();
        a.Add("a1");
        a.Add("a2");
        b.Add(a);
        a.Add("b1");
        a.Add("b2");
        b.Add(a);

        int x = 0, y = 0;
        string[,] data;


        for (int i = 0; i < b.Count-1; i++)
        {
            List<string> c = b[i];
            data = new string[b.Count,c.Count];
            for (int j = 0; j < c.Count; j++)
            {
                data[i, j] = c[j];
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
