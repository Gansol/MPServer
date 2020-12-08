using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Mice : MiceBase
{
    private BattleSystem battleManager;
    MPGame m_MPGame;
    private float _lastTime, _survivalTime;     // 出生時間、存活時間


    public override void Initialize(bool isBoss,float lerpSpeed, float upSpeed, float upDistance, float lifeTime)
    {
        battleManager = GameObject.FindGameObjectWithTag("GM").GetComponent<BattleSystem>();

     //   if (hitSound==null) hitSound = battleManager.GetComponent<UIPlaySound>();
        // m_AIState = null;
        // m_Arribute = null;
        // m_AnimState = null;
        m_AnimState.Init(gameObject, isBoss, lerpSpeed, upSpeed, upDistance, lifeTime);
        transform.localPosition = new Vector3(0, 0);
        GetComponent<BoxCollider2D>().enabled = true;
    }

    void OnEnable()
    {
        GetComponent<BoxCollider2D>().enabled = true;
        _lastTime = Time.fixedTime; // 出生時間
    }


    public void Update()
    {
        if (Global.isGameStart)
        {
            m_AnimState.UpdateAnimation();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }


    /// <summary>
    /// 擊中時
    /// </summary>
    protected override void OnHit()
    {
      //  gameObject.layer = cam.eventReceiverMask;
        if (Global.isGameStart && /*((cam.eventReceiverMask & gameObject.layer) == cam.eventReceiverMask) &&*/ enabled && m_Arribute.GetHP() > 0)
        {
            hitSound.Play();
            m_AnimState.SetMotion(true);
            OnInjured(1, true);
            Global.dictBattleMiceRefs.Remove(transform.parent);
            _survivalTime = Time.fixedTime - _lastTime;                // 老鼠存活時間 
            m_AnimState.Play(IAnimatorState.ENUM_AnimatorState.Die);
            m_MPGame.GeAudioSystem().PlaySound("Hit");

        }
        else
        {
            Debug.Log("enabled: " + enabled + "   Collider: " + GetComponent<BoxCollider2D>().enabled + "  m_Arribute.GetHP(): " + m_Arribute.GetHP());
        }
    }


    /// <summary>
    /// 死亡時
    /// </summary>
    /// <param name="lifeTime">存活時間上限</param>
    protected override void OnDead(float lifeTime)
    {
        if (Global.isGameStart)
        {
            if (m_Arribute.GetHP() == 0)
                battleManager.UpadateScore(System.Convert.ToInt16(name), lifeTime);  // 增加分數 錯誤 lifeTime應為存活時間
            else
                battleManager.LostScore(System.Convert.ToInt16(name), lifeTime);  // 失去分數
            Global.dictBattleMiceRefs.Remove(transform.parent);
            Global.MiceCount--;

            gameObject.SetActive(false);
            this.transform.parent = GameObject.Find("ObjectPool/" + name).transform;
        }
    }

    public override void OnEffect(string name, object value)
    {
        if (name == "Scorched" || name == "HeroMice")
            OnHit();
        if (name == "Snow")
            m_AnimState.SetMotion((bool)value);
        if (name == "Shadow")
            Debug.Log("Play Shadow");
        if (name == "Much")
        {
            GetComponent<BoxCollider2D>().enabled = false;
            Dictionary<int, Vector3> pos = value as Dictionary<int, Vector3>;
            MiceAnimState state = m_AnimState as MiceAnimState;
            m_AnimState.SetMotion(true);
            state.SetToPos(pos[0]);
            state.SetToScale(new Vector3(0.25f, 0.25f));
            m_AnimState.Play(IAnimatorState.ENUM_AnimatorState.Die);
        } 
        // play("Shadow"
    }
}
