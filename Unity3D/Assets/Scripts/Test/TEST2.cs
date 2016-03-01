using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TEST2 : MonoBehaviour
{
    public IEnumerator A(string a)
    {

        Debug.Log(a);
        yield return null;
    }
}
