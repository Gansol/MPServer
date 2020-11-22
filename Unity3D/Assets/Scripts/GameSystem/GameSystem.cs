using UnityEngine;
using System.Collections;
//預備改寫仲介者模式
public abstract class GameSystem  {
    protected MPGame m_MPGame = null;

    public GameSystem(MPGame MPGame)
    {
        m_MPGame = MPGame;
    }

    public virtual void Initinal(){}
    public virtual void Update(){}
    public virtual void FixedUpdate() { }
    public virtual void Release(){}
}
