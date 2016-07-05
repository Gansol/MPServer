using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TEST2 : MonoBehaviour
{
    int i;
    void Start()
    {
        i = 0;
    }

    public IEnumerator A(int a)
    {
        i++;
        Debug.Log("times:"+a.ToString());

        Debug.Log("value:" + i);
        yield return null;
    }
}
