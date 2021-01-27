using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Mice : IMice
{
    private float _lastTime, _survivalTime;     // 出生時間、存活時間


    public override void Initialize()
    {
        base.Initialize();
        //   if (hitSound==null) hitSound = battleManager.GetComponent<UIPlaySound>();
        // m_AIState = null;
        // m_Arribute = null;
        // m_AnimState = null;
        // m_AnimState.Init(m_go, isBoss, lerpSpeed, upSpeed, upDistance, lifeTime);
        m_go.transform.localPosition = new Vector3(0, 0);
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


    ///// <summary>
    ///// 擊中時
    ///// </summary>
    //protected override void OnHit()
    //{
    //    //  gameObject.layer = cam.eventReceiverMask;
    //    if (Global.isGameStart && /*((cam.eventReceiverMask & gameObject.layer) == cam.eventReceiverMask) &&*//* enabled && */m_Arribute.GetHP() > 0)
    //    {
    //        m_AnimState.SetMotion(true);
    //        OnInjured(1, true);
    //      //  m_AI.SetAIState(new DiedAIState(m_AI));

    //        _survivalTime = Time.fixedTime - _lastTime;                // 老鼠存活時間 
    //        m_AnimState.Play(IAnimatorState.ENUM_AnimatorState.Died);
    //        //  m_AI.SetAIState(new DiedAIState(m_AI));
    //        SetAIState(ENUM_CreatureAIState.Die);
    //        MPGame.Instance.GeAudioSystem().PlaySound("Hit");
    //    }
    //    else
    //    {
    //        Debug.Log("ENUM_AIState: " + creatureAIState + "   Collider: " + m_go.GetComponent<BoxCollider2D>().enabled + "  m_Arribute.GetHP(): " + m_Arribute.GetHP());
    //    }
    //}


    ///// <summary>
    ///// 死亡時
    ///// </summary>
    ///// <param name="lifeTime">存活時間上限</param>
    //protected override void OnDead(float lifeTime)
    //{
    //    //if (Global.isGameStart)
    //    //{
    //    //    if (m_Arribute.GetHP() == 0)
    //    //        m_AI.SetAIState(new DiedAIState());
    //    //        //ENUM_AIState = ENUM_CreatureState.Die;

    //    // //   battleManager.UpadateScore(System.Convert.ToInt16(name), lifeTime);  // 增加分數 錯誤 lifeTime應為存活時間
    //    //    else
    //    //        battleManager.LostScore(System.Convert.ToInt16(name), lifeTime);  // 失去分數
    //    //    Global.dictBattleMiceRefs.Remove(transform.parent);

    //    //    gameObject.SetActive(false);
    //    //    this.transform.parent = GameObject.Find("ObjectPool/" + name).transform;
    //    //}

    //    if (Global.isGameStart)
    //    {
    //        if (m_Arribute.GetHP() == 0)
    //            //m_AI.SetAIState(new DiedAIState(m_AI));
    //            m_AI.SetAIState(new DiedAIState());
    //        else
    //            //m_AI.SetAIState(new ByeByeAIState(m_AI));
    //        SetAIState(ENUM_CreatureAIState.ByeBye);

    //        Play(IAnimatorState.ENUM_AnimatorState.Died);
    //        m_go.SetActive(false);
    //    }
    //}

    public override void OnEffect(string name, object value)
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
            m_AnimState.Play(IAnimatorState.ENUM_AnimatorState.Died);
        }
        // play("Shadow"
    }

    public override void Release()
    {
        base.Release();
        //m_go = null;
        //m_Skill = null;
        //m_AnimState = null;
        //m_Arribute = null;
        //m_AI = null;
        MPGame.Instance.GetCreatureSystem().OnEffect -= OnEffect;
    }

    public override void OnHit()
    {
        if (m_AI.GetAIState() != (int)ENUM_CreatureAIState.Invincible)
        {
            Debug.Log("OnHit");
            Play(IAnimatorState.ENUM_AnimatorState.Died);
            OnInjured(1, true); // 錯誤 要由Player Item Hammer Attack value輸入 m_attr.hammerAtk
        }
    }
}
