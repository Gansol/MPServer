using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiceBoss : IMiceBoss
{
    public MiceBoss()
    {
        m_go.transform.localScale = new Vector3(1.3f, 1.3f);
        m_go.transform.localPosition = Vector2.zero;
    }

}
