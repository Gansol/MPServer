using UnityEngine;
using System.Collections.Generic;
using System.Text;
public class TEST : MonoBehaviour
{
    List<string> a = new List<string> { };
    // Use this for initialization
    void Start()
    {
        a.Add("A");
        a.Add("B");
        a.Remove("A");
        a.Remove("A");
        foreach (string item in a)
        {
            Debug.Log(item);
        }
    }
}
