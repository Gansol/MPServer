﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeroMice : MiceBase
{

    private BattleManager battleManager;
    private float _lastTime, _survivalTime;     // 出生時間、存活時間
    private bool bDead;
    UICamera cam;

    public override void Initialize(bool isBoss,float lerpSpeed, float upSpeed, float upDistance, float lifeTime)
    {
        battleManager = GameObject.FindGameObjectWithTag("GM").GetComponent<BattleManager>();
        if (hitSound == null) hitSound = battleManager.GetComponent<UIPlaySound>();
        cam = Camera.main.GetComponent<UICamera>();
        m_AnimState.init(gameObject, isBoss, lerpSpeed, upSpeed, upDistance, lifeTime);
        transform.localPosition = new Vector3(0, 0);
        GetComponent<BoxCollider2D>().enabled = true;
        bDead = false;
    }

    void OnEnable()
    {
        GetComponent<BoxCollider2D>().enabled = true;
        bDead = false;
        _lastTime = Time.fixedTime; // 出生時間
    }

    void Update()
    {
//        Debug.Log("Hero State: "+m_AnimState.GetAnimState().ToString());
        if (Global.isGameStart)
        {
            m_AnimState.UpdateAnimation();
        }
        else
        {
            gameObject.SetActive(false);
        }

        if (m_AnimState.GetAnimState() == AnimatorState.ENUM_AnimatorState.Die && !bDead)
        {
            bDead = true;
            GetComponent<BoxCollider2D>().enabled = false;
            if (Global.isGameStart && ((cam.eventReceiverMask & gameObject.layer) == cam.eventReceiverMask) && enabled && m_Arribute.GetHP() > 0)
            {
                m_AnimState.Play(AnimatorState.ENUM_AnimatorState.Eat);
                Dictionary<Transform, GameObject> buffer = new Dictionary<Transform, GameObject>(Global.dictBattleMice);
                foreach (KeyValuePair<Transform, GameObject> item in buffer)
                {
                    Dictionary<int, Vector3> pos = new Dictionary<int, Vector3>();
                    pos.Add(0, transform.position);
                    if (item.Value != null && Global.dictBattleMice.ContainsKey(item.Key))
                    {
                        if (item.Value.GetComponent<MiceBase>()!=null)
                            item.Value.GetComponent<MiceBase>().OnEffect("HeroMice", pos);
                    }
                }
            }
        }
    }

    protected override void OnHit()
    {
        Debug.Log("HeroMice:" + "cam.eventReceiverMask:" + cam.eventReceiverMask + " gameObject.layer:" + gameObject.layer + " enabled:" + enabled + " m_Arribute.GetHP():" + m_Arribute.GetHP());
        if (Global.isGameStart && ((cam.eventReceiverMask & gameObject.layer) == cam.eventReceiverMask) && enabled && GetComponent<BoxCollider2D>().enabled)
        {
            GetComponent<BoxCollider2D>().enabled = false;
            hitSound.Play();
            //OnInjured(1, true);
            m_AnimState.Play(AnimatorState.ENUM_AnimatorState.Eat);
        }
    }

    protected override void OnDead(float lifeTime)
    {
        if (Global.isGameStart)
        {
            Global.dictBattleMice.Remove(transform.parent);
            gameObject.SetActive(false);
            this.transform.parent = GameObject.Find("ObjectPool/" + name).transform;
        }
    }

    public override void OnEffect(string name, object value)
    {
        if (name == "Scorched")
            OnHit();
        if (name == "Snow")
            m_AnimState.SetMotion((bool)value);
    }
}
