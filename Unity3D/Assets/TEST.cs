using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TEST : MonoBehaviour
{
    public GameObject panel;
    public List<GameObject> haha;

    void Start()
    {
        Dictionary<int, List<int>> aa = new Dictionary<int, List<int>>();
        aa.Add(1, new List<int> { 1, 2, 3 });
        aa.Add(2, new List<int> { 2, 2, 2 });

        aa[1].Insert(aa[1].Count, 8);

       // Debug.Log(aa[1].FindAll(delegate(int i) {Debug.Log(i); return true;}));
        /*
        foreach(List<int> item in aa.Values){
           
            Debug.Log(item[0]);
        }
         * */
    }

}
