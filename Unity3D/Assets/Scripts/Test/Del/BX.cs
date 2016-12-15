using UnityEngine;
using System.Collections;

public class BX
{
    AX ax = null;

    public void Test(AX aax)
    {
        ax = aax;
        Debug.Log(ax);
    }
    public void Get()
    {
        Debug.Log(ax);
    }

    void OnEnable()
    {
        Debug.Log(aa());
    }

    float aa()
    {
        return (float)1 / (float)2;
    }
    void Update()
    {

    }

}
