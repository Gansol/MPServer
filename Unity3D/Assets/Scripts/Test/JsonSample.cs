using UnityEngine;
using System.Collections.Generic;
using System.Text;
using MiniJSON;

public class JsonSample : MonoBehaviour
{
//    List<string> a = new List<string> { };
    // Use this for initialization
    void Start()
    {
        string nestedJSON = @"{
      ""1"": {
        ""MiceName"": ""EggMice"",
""Speed"": ""0.5""
      },
      ""2"": {
        ""MiceName"": ""BlackMice""
""Speed"": ""1""
      }

}";
        var deJSON = Json.Deserialize(nestedJSON) as Dictionary<string, object>;
        Debug.LogWarning("We can see this is Dictionary Object:" + deJSON);

        foreach (KeyValuePair<string, object> item in deJSON)
        {
            Debug.LogWarning("We can see this is Dictionary Object:" + item.Value);
            var innDict = item.Value as Dictionary<string, object>;

            foreach (KeyValuePair<string, object> inner in innDict)
            {
                Debug.Log("Key:" + inner.Key + " Value:" + inner.Value);
            }
        }

    }
}
