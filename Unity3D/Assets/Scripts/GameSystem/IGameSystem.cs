using UnityEngine;
using System.Collections;
//預備改寫仲介者模式
public abstract class IGameSystem  {
    protected MPGame m_MPGame = null;

    public IGameSystem(MPGame MPGame)
    {
        m_MPGame = MPGame;
    }

    public virtual void Initialize(){}
    public virtual void Update(){}
    public virtual void FixedUpdate() { }
    public virtual void Release(){}
}
