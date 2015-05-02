using UnityEngine;
using System.Collections.Generic;
using MiniJSON;
using System;

public class SkillMice : MonoBehaviour
{

    bool flag;
    Dictionary<int, string> team;
    int miceID;

    void Start()
    {
        flag = true;
        team = new Dictionary<int, string>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnClick()
    {
        /*
        Dictionary<int, string> team = Json.Deserialize(Global.Team) as Dictionary<int, string>;

        foreach (KeyValuePair<int, string> item in team)
        {
            if (item.Value == name)
            {
                miceID = item.Key;
                break;
            }
        }
        */

        if (flag)
        {
            Global.photonService.SendDamage(2);
            flag = false;
        }
    }
}
