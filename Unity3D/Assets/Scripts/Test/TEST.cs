using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

public class TEST : MonoBehaviour{

    public GameObject a;
    Vector3 aa;
    void Start()
    {
        aa = new Vector3(-500, 0, 0);
    }

    void Update()
    {
        Debug.Log(a.transform.localPosition.x);
        a.transform.localPosition = Vector3.Lerp(a.transform.localPosition, aa, 0.1f);
    }
}
