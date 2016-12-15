using UnityEngine;
using System.Collections;

public class MiceAttr : CreatureAttr {

    public MiceAttr(int hp)
    {
        SetMaxHP(hp);
        SetHP(hp);
    }
}
