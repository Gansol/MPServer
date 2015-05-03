using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

public class TEST : MonoBehaviour
{
    void Start()
    {
        //var string = "{s}";
        var dict = Json.Deserialize(Global.Team) as Dictionary<string, object>;
        Debug.Log(dict);
    }


}
