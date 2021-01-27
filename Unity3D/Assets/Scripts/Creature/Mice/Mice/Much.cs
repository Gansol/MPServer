using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Much : IMice
{
    //private BattleSystem battleManager;
    private float _lastTime, _survivalTime;     // 出生時間、存活時間
    private AnimatorStateInfo animInfo;
    UICamera cam;
    private bool _bEat;

    public override void Initialize()
    {
        //battleManager = GameObject.FindGameObjectWithTag("GM").GetComponent<BattleSystem>();
       // if (hitSound == null) hitSound = battleManager.GetComponent<UIPlaySound>();
        cam = Camera.main.GetComponent<UICamera>();
        // m_AIState = null;
        // m_Arribute = null;
        // m_AnimState = null;
        _bEat = false;
        _lastTime = Time.fixedTime; // 出生時間
    }


    public override void Update()
    {
        if (Global.isGameStart)
        {
            m_AnimState.UpdateAnimation();
            animInfo = m_AnimState.GetAnimStateInfo();

            if (animInfo.fullPathHash == Animator.StringToHash("Layer1.Eat") && animInfo.normalizedTime > 0.5f)
                m_go.GetComponent<BoxCollider2D>().enabled = false;
        }
        else
        {
            m_go.SetActive(false);
        }

        if (m_AnimState.GetENUM_AnimState() == IAnimatorState.ENUM_AnimatorState.Eat && !_bEat)
        {
            _bEat = true;

           // Play(IAnimatorState.ENUM_AnimatorState.Eat);

            CreatureSystem m_CreatureSystem = MPGame.Instance.GetCreatureSystem();
            Dictionary<int, Vector3> pos = new Dictionary<int, Vector3>();
            pos.Add(0, m_go.transform.position);

            m_CreatureSystem.SetEffect(this.m_Arribute.name, pos);

            //Dictionary<Transform, GameObject> buffer = new Dictionary<Transform, GameObject>(Global.dictBattleMiceRefs);
            //foreach (KeyValuePair<Transform, GameObject> item in buffer)
            //{
            //    Dictionary<int, Vector3> pos = new Dictionary<int, Vector3>();
            //    pos.Add(0, transform.position);
            //    if (item.Value != null && Global.dictBattleMiceRefs.ContainsKey(item.Key) && item.Value != gameObject)
            //    {
            //        if (item.Value.GetComponent<IMice>() != null)
            //        {
            //            item.Value.GetComponent<IMice>().OnEffect("Much", pos);
            //            item.Value.GetComponent<BoxCollider2D>().enabled = false;
            //        }
            //    }
            //}
        }
    }


    /// <summary>
    /// 擊中時
    /// </summary>
    public override void OnHit()
    {
        
        if (Global.isGameStart &&/* ((cam.eventReceiverMask & gameObject.layer) == cam.eventReceiverMask) && enabled && */ m_Arribute.GetHP() > 0)
        {
            MPGame.Instance.GeAudioSystem().PlaySound("Hit");
            m_AnimState.SetMotion(true);
            OnInjured(1, true);
            _survivalTime = Time.fixedTime - _lastTime;                // 老鼠存活時間 
            m_AnimState.Play(IAnimatorState.ENUM_AnimatorState.Died);
        }
        else
        {
            Debug.Log("ENUM_AIState: " + GetAIState().ToString() + "   Collider: " + m_go.GetComponent<BoxCollider2D>().enabled + "  m_Arribute.GetHP(): " + m_Arribute.GetHP());
        }
    }


    ///// <summary>
    ///// 死亡時
    ///// </summary>
    ///// <param name="lifeTime">存活時間上限</param>
    //protected override void OnDead(float lifeTime)
    //{
    //    //if (Global.isGameStart)
    //    //{
    //    //    if (m_Arribute.GetHP() == 0)
    //    //        battleManager.UpadateScore(System.Convert.ToInt16(name), lifeTime);  // 增加分數
    //    //    else
    //    //        battleManager.LostScore(System.Convert.ToInt16(name), lifeTime);  // 增加分數
    //    //    Global.dictBattleMiceRefs.Remove(transform.parent);

    //    //    gameObject.SetActive(false);
    //    //    this.transform.parent = GameObject.Find("ObjectPool/" + name).transform;
    //    //}

    //    if (Global.isGameStart)
    //    {
    //        if (m_Arribute.GetHP() == 0)
    //            this.SetAIState(ENUM_CreatureAIState.Died);
    //          //  m_AI.SetAIState(new DiedAIState(m_AI));
    //        else
    //            this.SetAIState(ENUM_CreatureAIState.ByeBye);

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
        if (name == "Shadow")
            Debug.Log("Play Shadow");
        if (name == "HeroMice")
        {
            Dictionary<int, Vector3> pos = value as Dictionary<int, Vector3>;
            MiceAnimState state = m_AnimState as MiceAnimState;
            state.SetAminationPosTo(pos[0]);
            state.SetAminationScaleTo(new Vector3(0.25f, 0.25f));
            OnHit();
        }

        // play("Shadow"
    }

    public override void Release()
    {
        base.Release();
    }
}
