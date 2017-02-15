using UnityEngine;
using System.Collections;
using System;

public class BX : MonoBehaviour
{
    public delegate IEnumerator CoroutineMethod();

    IEnumerator RunCoroutine(CoroutineMethod coroutineMethod)
    {
        return coroutineMethod();
    }

    public void StartCoroutineDelegate(CoroutineMethod coroutineMethod)
    {
        StartCoroutine("RunCoroutine", coroutineMethod);
    }
}
