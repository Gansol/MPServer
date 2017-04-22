using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bali : MiceBase
{
    private BattleManager battleManager;
    private float _lastTime, _survivalTime;     // 出生時間、存活時間
    UICamera cam;

    public override void Initialize(bool isBoss, float lerpSpeed, float upSpeed, float upDistance, float lifeTime)
    {
        battleManager = GameObject.FindGameObjectWithTag("GM").GetComponent<BattleManager>();
        cam = Camera.main.GetComponent<UICamera>();
        // m_AIState = null;
        // m_Arribute = null;
        // m_AnimState = null;
        m_AnimState.init(gameObject, isBoss, lerpSpeed, upSpeed, upDistance, lifeTime);
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
        if (Global.isGameStart && ((cam.eventReceiverMask & gameObject.layer) == cam.eventReceiverMask) && enabled && m_Arribute.GetHP() > 0)
        {
            m_AnimState.SetMotion(true);
            OnInjured(1, true);
            _survivalTime = Time.fixedTime - _lastTime;                // 老鼠存活時間 
            m_AnimState.Play(AnimatorState.ENUM_AnimatorState.Die);
            battleManager.GetComponent<UIPlaySound>().Play();
            battleManager.GetComponent<UIPlaySound>().volume = .87f;
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
            if(m_Arribute.GetHP()==0)
                battleManager.LostScore(System.Convert.ToInt16(name), lifeTime);  // 失去分數
            Global.dictBattleMice.Remove(transform.parent);
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
    }
}
