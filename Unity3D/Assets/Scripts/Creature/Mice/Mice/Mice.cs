using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Mice : IMice
{
    private float _lastTime, _survivalTime;     // 出生時間、存活時間


    public override void Initialize()
    {
        base.Initialize();
        _lastTime = Time.fixedTime; // 出生時間
        MPGame.Instance.GetCreatureSystem().OnEffect += OnEffect;
    }


    public override void Update()
    {
        if (Global.isGameStart && m_go.activeSelf)
        {
            base.Update();
            m_AnimState.UpdateAnimation();
        }
        else
        {
            m_go.SetActive(false);
        }
    }

    public override void OnEffect(string name, object value)
    {
        if (m_go.activeSelf)
        {
            if (name == "Scorched" || name == "HeroMice")
                OnInjured(1, true);
            if (name == "Snow")
                m_AnimState.SetMotion((bool)value);
            if (name == "Shadow")
                Debug.Log("Play Shadow");
            if (name == "Much")
            {
                m_go.GetComponent<BoxCollider2D>().enabled = false;
                Dictionary<int, Vector3> pos = value as Dictionary<int, Vector3>;
                MiceAnimState state = m_AnimState as MiceAnimState;
                m_AnimState.SetMotion(true);
                state.SetAminationPosTo(pos[0]);
                state.SetAminationScaleTo(new Vector3(0.25f, 0.25f));
                state.Play(IAnimatorState.ENUM_AnimatorState.Died, m_go);
            }
        }
        // play("Shadow"
    }

    public override void Release()
    {
        base.Release();
        MPGame.Instance.GetCreatureSystem().OnEffect -= OnEffect;
    }

    public override void OnHit()
    {
        if (m_AI.GetAIState() != (int)ENUM_CreatureAIState.Invincible)
        {
            Debug.Log("OnHit: " + m_go.GetHashCode() + " Hole:" + m_go.transform.parent);
            Play(IAnimatorState.ENUM_AnimatorState.Died);
            OnInjured(1, true); // 錯誤 要由Player Item Hammer Attack value輸入 m_attr.hammerAtk
        }
        else
        {
            Debug.Log("BUG OnHit: " + m_go.GetHashCode() + " Hole:" + m_go.transform.parent);
        }
    }

    public override void Play(IAnimatorState.ENUM_AnimatorState state)
    {
        if (m_AnimState != null)
        {
            m_AnimState.Play(state, m_go);
              Debug.Log("Mice Play:" + "   " + m_go.transform.parent.name + "   " + m_go.name + "  "+state.ToString());
        }
        else
        {
            Debug.LogError("m_AnimState is null!");
        }
    }
}
