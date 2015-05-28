using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

public class TEST : MonoBehaviour
{


     bool flag;
     bool onUpdateCall;
    int num;

    void Start()
    {
        
        flag = true;
        onUpdateCall = false;
        num = 1;
        StartCoroutine(A());
    }

    public IEnumerator A()
    {
        Debug.Log(Random.Range(0,2));
        yield return new WaitForSeconds(1);
        if (onUpdateCall)
        {
            flag = true;
        }
    }
    public IEnumerator B()
    {
        Debug.Log(Random.Range(0, 2));
        yield return new WaitForSeconds(1);
        flag = true;
    }
    public IEnumerator C()
    {
        Debug.Log(Random.Range(0, 2));
        yield return new WaitForSeconds(1);
        flag = true;
    }

    void Update()
    {
        //Debug.Log(flag);
        if (flag)
        {
            flag = false;
            switch (num)
            {
                case 1:
                    onUpdateCall = true;
                    StartCoroutine(A());
                    break;
                case 2:
                    onUpdateCall = true;
                    StartCoroutine(B());
                    break;
                case 3:
                    onUpdateCall = true;
                    StartCoroutine(C());
                    break;
            }

            if (num == 3)
            {
                num = 1;
            }
            else
            {
                num++;
            }
        }
    }
}
