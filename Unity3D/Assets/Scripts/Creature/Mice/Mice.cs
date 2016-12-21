using UnityEngine;
using System.Collections;

public class Mice : MiceBase
{
    private BattleManager battleManager;
    private float _lastTime, _survivalTime;     // 出生時間、存活時間

    public override void Initialize(float lerpSpeed, float upSpeed, float upDistance, float lifeTime)
    {
        m_AIState = null;
        m_Arribute = null;

        AnimState = gameObject.AddMissingComponent<MiceAnimState>();
        battleManager = GameObject.FindGameObjectWithTag("GM").GetComponent<BattleManager>();
        AnimState.Initialize(gameObject, false, lerpSpeed, upSpeed, upDistance, lifeTime);

        transform.localPosition = new Vector3(0, 0);
        collider2D.enabled = true;
    }


    void OnEnable()
    {
        _lastTime = Time.fixedTime; // 出生時間
    }


    public void Update()
    {
       if (!Global.isGameStart) gameObject.SetActive(false);
    }


    /// <summary>
    /// 擊中時
    /// </summary>
    protected override void OnHit()
    {
        if (Global.isGameStart && enabled && m_Arribute.GetHP() > 0)
        {
            Debug.Log("Hit");
            Global.dictBattleMice.Remove(transform.parent);
            collider2D.enabled = false;
            _survivalTime = Time.fixedTime - _lastTime;                // 老鼠存活時間 
            AnimState.Play(AnimatorState.ENUM_AnimatorState.Die);
            OnInjured(1);
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
            this.transform.parent = GameObject.Find("ObjectPool/" + name).transform;
            gameObject.SetActive(false);

            if (m_Arribute.GetHP() == 0)
                battleManager.UpadateScore(name, _survivalTime);  // 增加分數
            else
                battleManager.LostScore(name, lifeTime);  // 增加分數
            Global.dictBattleMice.Remove(transform.parent);
            Global.MiceCount--;
        }
    }
}
