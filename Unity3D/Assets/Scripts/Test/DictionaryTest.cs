using UnityEngine;
using System.Collections;
using System.Linq;
using MiniJSON;
using System.Collections.Generic;

public class DictionaryTest : MonoBehaviour
{
    Dictionary<string, string> a = new Dictionary<string, string>();
    // Use this for initialization
    void Start()
    {
        string xx = "a";
        a.Add("A", "a");
        xx = a.Single(x => x.Value == xx).Key;
        Debug.Log(xx);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
