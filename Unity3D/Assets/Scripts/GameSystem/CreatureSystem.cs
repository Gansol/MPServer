using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//預備改寫 遊戲系統
public class CreatureSystem : GameSystem {

    Dictionary<GameObject, MiceBase> _mice = new Dictionary<GameObject, MiceBase>();

    public CreatureSystem(MPGame MPGame) : base(MPGame) { }

    public void AddMice(GameObject mice,MiceBase miceBase){
        _mice.Add(mice, miceBase);
    }

    public void RemoveMice(GameObject mice)
    {
        _mice.Remove(mice);
    }
}
