using UnityEngine;
using System.Collections.Generic;
using MiniJSON;

public class test2 : MonoBehaviour
{
    void Start()
    {
        
        int a =3;
        int b= 2;
        float c =1-0.23f;

        Debug.Log(c);
        Destroy(GetComponent<sss>());
        this.transform.gameObject.AddComponent<sss>();
    }

    void Update()
    {

    }
}