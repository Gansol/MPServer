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

    public IEnumerator A()
    {
        Debug.Log("-");
        yield return new WaitForSeconds(2.0f);
        Debug.Log("-");
        
    }

    public IEnumerator B()
    {
        i++;
        Debug.Log("---" );
        Debug.Log("---");

        yield return null;

    }
}
