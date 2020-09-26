using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Much : MiceBase
{
    private BattleManager battleManager;
    private float _lastTime, _survivalTime;     // 出生時間、存活時間
    private AnimatorStateInfo animInfo;
    UICamera cam;
    private bool _bEat;

    public override void Initialize(bool isBoss,float lerpSpeed, float upSpeed, float upDistance, float lifeTime)
    {
        battleManager = GameObject.FindGameObjectWithTag("GM").GetComponent<BattleManager>();
        if (hitSound == null) hitSound = battleManager.GetComponent<UIPlaySound>();
        cam = Camera.main.GetComponent<UICamera>();
        // m_AIState = null;
        // m_Arribute = null;
        // m_AnimState = null;
        m_AnimState.Init(gameObject, isBoss, lerpSpeed, upSpeed, upDistance, lifeTime);
        transform.localPosition = new Vector3(0, 0);
        GetComponent<BoxCollider2D>().enabled = true;
        _bEat = false;
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
            animInfo = m_AnimState.GetAnimStateInfo();

            if (animInfo.nameHash == Animator.StringToHash("Layer1.Eat") && animInfo.normalizedTime > 0.5f)
                GetComponent<BoxCollider2D>().enabled = false;
        }
        else
        {
            gameObject.SetActive(false);
        }

        if (m_AnimState.GetAnimState() == IAnimatorState.ENUM_AnimatorState.Eat && !_bEat)
        {
            _bEat = true;
            
            Dictionary<Transform, GameObject> buffer = new Dictionary<Transform, GameObject>(Global.dictBattleMiceRefs);
            foreach (KeyValuePair<Transform, GameObject> item in buffer)
            {
                Dictionary<int, Vector3> pos = new Dictionary<int, Vector3>();
                pos.Add(0, transform.position);
                if (item.Value != null && Global.dictBattleMiceRefs.ContainsKey(item.Key) && item.Value != gameObject)
                {
                    if (item.Value.GetComponent<MiceBase>() != null)
                    {
                        item.Value.GetComponent<MiceBase>().OnEffect("Much", pos);
                        item.Value.GetComponent<BoxCollider2D>().enabled = false;
                    }
                }
            }
        }
    }


    /// <summary>
    /// 擊中時
    /// </summary>
    protected override void OnHit()
    {
        
        if (Global.isGameStart &&/* ((cam.eventReceiverMask & gameObject.layer) == cam.eventReceiverMask) &&*/ enabled && m_Arribute.GetHP() > 0)
        {
            hitSound.Play();
            m_AnimState.SetMotion(true);
            OnInjured(1, true);
            _survivalTime = Time.fixedTime - _lastTime;                // 老鼠存活時間 
            m_AnimState.Play(IAnimatorState.ENUM_AnimatorState.Die);
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
                battleManager.UpadateScore(System.Convert.ToInt16(name), lifeTime);  // 增加分數
            else
                battleManager.LostScore(System.Convert.ToInt16(name), lifeTime);  // 增加分數
            Global.dictBattleMiceRefs.Remove(transform.parent);
            Global.MiceCount--;

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
        if (name == "Shadow")
            Debug.Log("Play Shadow");
        if (name == "HeroMice")
        {
            Dictionary<int, Vector3> pos = value as Dictionary<int, Vector3>;
            MiceAnimState state = m_AnimState as MiceAnimState;
            state.SetToPos(pos[0]);
            state.SetToScale(new Vector3(0.25f, 0.25f));
            OnHit();
        }

        // play("Shadow"
    }
}
