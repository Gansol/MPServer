using UnityEngine;
using System.Collections;
//預備改寫仲介者模式
public abstract class GameSystem {
    protected MPGame m_MPSystem = null;

    public GameSystem(MPGame MPSystem)
    {
        m_MPSystem = MPSystem;
    }

    public virtual void Start(){}
    public virtual void Update(){}
    public virtual void Release(){}
}
