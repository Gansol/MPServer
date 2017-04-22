using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;


public class BX : MonoBehaviour
{
    void OnClick()
    {
         Dictionary<string, Dictionary<string, object>> dictItemUsage = new Dictionary<string, Dictionary<string, object>>();
            Dictionary<string, object> x = new Dictionary<string, object>();

            x.Add("UseCount", "1");
            dictItemUsage.Add("10001", x);
            dictItemUsage.Add("10002", x);
            dictItemUsage.Add("10003", x);
            string jstring = MiniJSON.Json.Serialize(dictItemUsage);
                List<string> columns = new List<string>();
                columns.Add("UseCount");
                columns.Add("Rank");
                columns.Add("Exp");
                Global.photonService.GameOver(0, 0, 0, 0, 0, 0, 0, jstring, columns.ToArray());
    }

}
