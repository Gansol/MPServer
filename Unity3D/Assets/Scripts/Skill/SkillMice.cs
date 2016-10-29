using UnityEngine;
using System.Collections.Generic;
using MiniJSON;
using System;

public class SkillMice : MonoBehaviour
{

    bool flag;
    Dictionary<int, string> team;

    void Start()
    {
        flag = true;
        team = new Dictionary<int, string>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnHit()
    {
        if (flag)
        {
            Debug.Log("Click Skill:"+transform.GetChild(0).name);
            Global.photonService.SendSkill(transform.GetChild(0).name);
            flag = false;
        }
    }
}
