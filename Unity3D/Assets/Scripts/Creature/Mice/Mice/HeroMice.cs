using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeroMice : IMice
{
    private float _lastTime, _survivalTime;     // 出生時間、存活時間
    private bool bDead;
    UICamera cam;

    public override void Initialize()
    {
        //  if (hitSound == null) hitSound = battleManager.GetComponent<UIPlaySound>();
        cam = Camera.main.GetComponent<UICamera>();
        //    m_AnimState.Init(gameObject, isBoss, lerpSpeed, upSpeed, upDistance, lifeTime);

        bDead = false;
        _lastTime = Time.fixedTime; // 出生時間
    }

    public override void Update()
    {
        //        Debug.Log("Hero State: "+m_AnimState.GetAnimState().ToString());
        if (Global.isGameStart)
        {
            m_AnimState.UpdateAnimation();
        }
        else
        {
            m_go.SetActive(false);
        }

        if (m_AnimState.GetENUM_AnimState() == IAnimatorState.ENUM_AnimatorState.Died && !bDead)
        {
            bDead = true;
            m_go.GetComponent<BoxCollider2D>().enabled = false;
            if (Global.isGameStart /*&& enabled /*&& ((cam.eventReceiverMask & gameObject.layer) == cam.eventReceiverMask) */&& m_Arribute.GetHP() > 0)
            {
                Play(IAnimatorState.ENUM_AnimatorState.Eat);

           CreatureSystem m_CreatureSystem=   MPGame.Instance.GetCreatureSystem();
                Dictionary<int, Vector3> pos = new Dictionary<int, Vector3>();
                pos.Add(0, m_go.transform.position);

                m_CreatureSystem.SetEffect(this.m_Arribute.name, pos);

                //Dictionary<Transform, GameObject> buffer = new Dictionary<Transform, GameObject>(Global.dictBattleMiceRefs);

                //foreach (KeyValuePair<Transform, GameObject> item in buffer)
                //{
                    
                //    if (item.Value != null && Global.dictBattleMiceRefs.ContainsKey(item.Key))
                //    {
                //        if (item.Value.GetComponent<IMice>() != null)
                //            item.Value.GetComponent<IMice>().OnEffect("HeroMice", pos);
                //    }
                //}
            }
        }
    }

    public override void OnHit()
    {
      //  Debug.Log("HeroMice:" + "cam.eventReceiverMask:" + cam.eventReceiverMask + " gameObject.layer:" + m_go.layer + " ENUM_AIState:" + creatureAIState.ToString() + " m_Arribute.GetHP():" + m_Arribute.GetHP());
        if (Global.isGameStart && /*((cam.eventReceiverMask & gameObject.layer) == cam.eventReceiverMask) && enabled &&*/ m_go.GetComponent<BoxCollider2D>().enabled)
        {
            m_go.GetComponent<BoxCollider2D>().enabled = false;
            MPGame.Instance.GeAudioSystem().PlaySound("Hit");
            //OnInjured(1, true);
            Play(IAnimatorState.ENUM_AnimatorState.Eat);
        }
    }

    //protected override void OnDead(float lifeTime)
    //{
    //    //if (Global.isGameStart)
    //    //{
    //    //    Global.dictBattleMiceRefs.Remove(transform.parent);
    //    //    gameObject.SetActive(false);
    //    //    this.transform.parent = GameObject.Find("ObjectPool/" + name).transform;
    //    //}

    //    if (Global.isGameStart)
    //    {
    //        if (m_Arribute.GetHP() == 0)
    //            SetAIState(ENUM_CreatureAIState.Died);
    //           // m_AI.SetAIState(new DiedAIState(m_AI));
    //        else
    //            SetAIState(ENUM_CreatureAIState.ByeBye);
    //        //m_AI.SetAIState(new ByeByeAIState(m_AI));

    //        Play(IAnimatorState.ENUM_AnimatorState.Died);
    //        m_go.SetActive(false);
    //    }

    //}

    public override void OnEffect(string name, object value)
    {
        if (name == "Scorched")
            OnHit();
        if (name == "Snow")
            m_AnimState.SetMotion((bool)value);
    }

    public override void Release()
    {
        throw new System.NotImplementedException();
    }
}
