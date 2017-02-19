using UnityEngine;
using System.Collections;
using MPProtocol;

public class EggMiceBoss : MiceBossBase
{
    private BattleHUD battleHUD = null;
    UICamera cam;
    private bool bDisplaySkill;
    private int myHits, otherHits;              // 打擊紀錄
    //private float _lastTime, _survivalTime;     // 上次,存活時間

    void Awake()
    {
        battleHUD = GameObject.FindGameObjectWithTag("GM").GetComponent<BattleHUD>();

        Global.photonService.BossInjuredEvent += OnInjured;
        Global.photonService.LoadSceneEvent += OnDestory;   // 離開房間時
    }

    /*
    void OnEnable()
    {
        _lastTime = Time.fixedTime;
    }
    */

    public override void Initialize(float lerpSpeed, float upSpeed, float upDistance, float lifeTime)
    {
        m_AIState = null;
        m_Arribute = null;
        m_Skill = null;
        bDisplaySkill = false;
        cam = Camera.main.GetComponent<UICamera>();
        transform.localPosition = new Vector3(0, 0);
        GetComponent<BoxCollider2D>().enabled = true;
    }

    public void Update()
    {
        // 遊戲開始時
        if (Global.isGameStart)
        {
            battleHUD.ShowBossHPBar(m_Arribute.GetHPPrecent(), false);    // 顯示血調
            m_AnimState.UpdateAnimation();
        }
        else
            gameObject.SetActive(false);

        if (!bDisplaySkill)
        {
            bDisplaySkill = true;
            //  Debug.Log("m_Skill:" + m_Skill);
            m_Skill.Display(gameObject, m_Arribute, m_AIState);
        }

        if (m_Arribute.GetHP() == 0)
            OnDead(0);
    }

    /// <summary>
    /// 擊中時
    /// </summary>
    protected override void OnHit()
    {
        if (Global.isGameStart && ((cam.eventReceiverMask & gameObject.layer) == cam.eventReceiverMask) && enabled && m_Arribute.GetHP() != 0)
        {
            //            Debug.Log("Hit");
            if (m_Arribute.GetHP() - 1 == 0)
            {
                GetComponent<BoxCollider2D>().enabled = false;
                Debug.Log("FQQQQQQQQQQQQQQQQ");
            }

            m_AnimState.Play(AnimatorState.ENUM_AnimatorState.OnHit);

            if (m_Arribute.GetShield() == 0)
            {
                Global.photonService.BossDamage(1);  // 傷害1是錯誤的 需要由Server判定、技能等級
            }
            else
            {
                m_Arribute.SetShield(m_Arribute.GetShield() - 1);
                Debug.Log("Hit Shield:" + m_Arribute.GetShield());
            }
        }
    }

    /// <summary>
    /// 死亡時時
    /// </summary>
    /// <param name="lifeTime">存活時間</param>
    protected override void OnDead(float lifeTime)
    {
        if (Global.isGameStart)
        {
            // 關閉血調顯示
            battleHUD.ShowBossHPBar(m_Arribute.GetHPPrecent(), true);
            //// 物件存入物件池
            //this.transform.parent = GameObject.Find("ObjectPool/" + name).transform;
            //gameObject.SetActive(false);

            Global.MiceCount--;
            Global.dictBattleMice.Remove(transform);
            OnDestory();
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 受傷時
    /// </summary>
    /// <param name="damage">傷害</param>
    /// <param name="isMe">是否為自己攻擊</param>
    public void OnInjured(short damage, bool isMe)
    {
        if (m_Arribute.GetShield() > 0)
        {
            m_Arribute.SetShield(m_Arribute.GetShield() - 1);
            Debug.Log("Hit Shield:" + m_Arribute.GetShield());
        }
        else
        {
            base.OnInjured(damage);
        }

        if (this.m_Arribute.GetHP() != 0)
        {
            if (isMe)
            {
                myHits++;
                //                Debug.Log("myHits" + myHits);
            }
            else
            {
                otherHits++;
                //              Debug.Log("otherHits" + otherHits);
            }
        }
        else
        {
            m_AnimState.Play(AnimatorState.ENUM_AnimatorState.Die);
            if (Global.OtherData.RoomPlace != "Host")
            {//0*100=0
                short percent = (short)Mathf.Round((float)myHits / (float)(myHits + otherHits) * 100); // 整數百分比0~100% 目前是用打擊次數當百分比 如果傷害公式有變動需要修正
                Global.photonService.MissionCompleted((byte)Mission.WorldBoss, 1, percent, "");
                //                Debug.Log("percent:" + percent);
            }
            transform.parent.GetComponentInChildren<Animator>().Play("HoleScale_R");
        }
    }

    /// <summary>
    /// 銷毀時，移除事件
    /// </summary>
    void OnDestory()
    {
        Global.photonService.BossInjuredEvent -= OnInjured;
        Global.photonService.LoadSceneEvent -= OnDestory;   // 離開房間時
    }
}
