using UnityEngine;
using System.Collections;

public class AX :MonoBehaviour  {

    public int cc = 10;

    public AX(int c )
    {
        cc = c;
    }

    void Start()
    {
        int i = 0  ,count =0;
        for (i = 9; count < 10; )
        {
            Debug.Log(i);
            i--;
            count++;
        }
    }
}
