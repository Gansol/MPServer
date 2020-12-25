﻿using UnityEngine;
using System.Collections;

public abstract class IMiceBoss : ICreature
{
    public static UICamera cam;
    public static BattleUI battleUI = null;
    protected int _shield = 0;
    protected int myHits, otherHits;              // 打擊紀錄
    protected float m_LastTime, m_StartTime;
    protected bool flag;


    public IMiceBoss()
    {
    }

    public override void Initialize()
    {
        m_go.GetComponent<BoxCollider2D>().enabled = true;
        m_go.transform.localPosition =  Vector3.zero;
        battleUI = MPGame.Instance.GetBattleUI();
        cam = Camera.main.GetComponent<UICamera>();

        m_StartTime = m_LastTime = Time.time;

        Global.photonService.BossInjuredEvent += OnInjured;
        Global.photonService.LoadSceneEvent += Release;   // 離開房間時
    }


    public override void Update()
    {
        // 遊戲開始時
        if (Global.isGameStart)
        {
            m_AI.UpdateAIState();
            battleUI.ShowBossHPBar(m_Arribute.GetHPPrecent(), false);    // 顯示血調
            m_AnimState.UpdateAnimation();
            if (Time.time < m_StartTime + m_Skill.GetSkillTime())
                m_Skill.UpdateEffect();
            if (m_Arribute.GetHP() == 0) OnDead(0);
        }
        //else
        //    m_go.SetActive(false);
    }



    /// <summary>
    /// On Touch / On Click
    /// </summary>
    protected virtual void OnHit()
    {
        Debug.Log("HP:" + m_Arribute.GetHP() + "SHIELD:" + m_Arribute.GetShield());
        if (Global.isGameStart /*&& enabled */&& m_Arribute.GetHP() > 0)
        {
            m_AnimState.Play(IAnimatorState.ENUM_AnimatorState.OnHit);

            if (m_Arribute.GetHP() - 1 == 0) m_go.GetComponent<BoxCollider2D>().enabled = false;

            if (m_Arribute.GetShield() == 0 && ENUM_AIState!=ENUM_CreatureState.Invincible)
                Global.photonService.BossDamage(1);  // 傷害1是錯誤的 需要由Server判定、技能等級
            else
                m_Arribute.SetShield(m_Arribute.GetShield() - 1);
        }

        if (m_Arribute.GetHP() <= 0)
            GameObject.Destroy(m_go);
    }

    /// <summary>
    /// 受傷
    /// </summary>
    /// <param name="damage"></param>
    protected override void OnInjured(short damage, bool myAttack)
    {
        if (myAttack && m_Arribute.GetShield() > 0)
        {
            m_Arribute.SetShield(m_Arribute.GetShield() - damage);
            Debug.Log("Hit Shield:" + m_Arribute.GetShield());
        }
        else
        {
            m_Arribute.SetHP(Mathf.Max(0, m_Arribute.GetHP() - damage));
        }

        if (m_Arribute.GetHP() != 0)
        {
            int haha = myAttack ? myHits++ : otherHits++;
        }
        else
        {
            m_AnimState.Play(IAnimatorState.ENUM_AnimatorState.Died);
            if (Global.OpponentData.RoomPlace != "Host")
            {
                short percent = (short)Mathf.Round((float)myHits / (float)(myHits + otherHits) * 100); // 整數百分比0~100% 目前是用打擊次數當百分比 如果傷害公式有變動需要修正
                Global.photonService.MissionCompleted((byte)MPProtocol.Mission.WorldBoss, 1, percent, "");  // 1是錯誤的
                Debug.Log("------------Send WorldBoss Died!---------");
            }
            m_go.transform.parent.GetComponentInChildren<Animator>().Play("Layer1.HoleScale_R", -1, 0f);
        }
    }

    /// <summary>
    /// 死亡時
    /// </summary>
    /// <param name="lifeTime">存活時間</param>
    protected override void OnDead(float lifeTime)
    {
        if (Global.isGameStart)
        {
            // 關閉血調顯示
            battleUI.ShowBossHPBar(m_Arribute.GetHPPrecent(), true);
            Release();
            Play(IAnimatorState.ENUM_AnimatorState.Died);
            m_AI.SetAIState(new DiedAIState());
            // Destroy(gameObject);
        }
    }

    public override void SetSkill(ISkill skill)
    {
        if (m_Skill != null)
            m_Skill.Release();
        m_Skill = skill;
    }

    //public override void SetState(IAIState state)
    //{
    //    if (this.m_AIState != null)
    //        this.m_AIState = null;
    //    this.m_AIState = state;
    //}

    public override void SetArribute(ICreatureAttr arribute)
    {
        if (m_Arribute != null)
            m_Arribute = null;
        m_Arribute = arribute;
    }

    public override void SetAnimState(IAnimatorState state)
    {
        if (m_AnimState != null)
            m_AnimState = null;
        m_AnimState = state;
    }

    public override void SetAI(ICreatureAI ai)
    {
        m_AI = ai;
    }

    public override void SetGameObject(GameObject go)
    {
        m_go = go;
    }

    public override void Release()
    {
        Global.photonService.BossInjuredEvent -= OnInjured;
        Global.photonService.LoadSceneEvent -= Release;   // 離開房間時
    }
}