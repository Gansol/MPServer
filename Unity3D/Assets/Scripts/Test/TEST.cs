using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using System;

public class TEST : MonoBehaviour
{
    private Dictionary<string, string> abc;

    void Awake()
    {
        abc = new Dictionary<string, string>(); 
      
    }
    void Start()
    {
        DateTime birthday = Convert.ToDateTime("07/03/1990");
        Debug.Log(birthday);
       TimeSpan ts = DateTime.Now.Subtract(birthday);
       byte aa = Convert.ToByte(Math.Floor(ts.TotalDays / 365));
       Debug.Log(aa);
    }

   public void HH()
   {
       Debug.Log(abc.Count);
       abc = Util.DeserializeJSONProfile(@"{""id"":""1137823976234994""}") as Dictionary<string,string>;
       Debug.Log(abc.Count);
   }
}
