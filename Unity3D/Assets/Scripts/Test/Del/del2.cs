using UnityEngine;
using System.Collections.Generic;

public class del2 :MonoBehaviour
{
    Dictionary<string, GameObject> a;
    int x;
    void Start()
    {
        a = new Dictionary<string, GameObject>();
        a.Add("1", gameObject);
        transform.GetChild(0).GetComponent<del>().Send(ref a);
    }


    public void Active()
    {
       enabled = true;
       Debug.Log("true");
    }
}

