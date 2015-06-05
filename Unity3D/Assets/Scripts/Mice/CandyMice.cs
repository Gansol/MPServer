using UnityEngine;
using System;
using System.Collections;

/* ***************************************************************
 * -----Copyright © 2015 Gansol Studio.  All Rights Reserved.-----
 * -----------            CC BY-NC-SA 4.0            -------------
 * -----------  @Website:  EasyUnity@blogspot.com    -------------
 * -----------  @Email:    GansolTW@gmail.com        -------------
 * -----------  @Author:   Krola.                    -------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * 
 * 糖果專用腳本
 * 被點到時、跑掉時、播放動畫
 * 這裡可能出現S級BUG 卡洞!!!
 * 如果會出現 老鼠不會消失 檢查 UpAnim和DownAnim
 * ***************************************************************/

public class CandyMice : MonoBehaviour
{
    BattleManager battleManager;

    float aliveTime;
    float animTime;

    bool upFlag;

    bool dieFlag;
    bool isDisappear;
    bool eatingFlag;
    bool clickFlag;

    public float upDistance; // mouse pos
    public float upSpeed;
    [Range(0.01f, 0.99f)]
    public float lerpSpeed;

    private float _lerpSpeed;
    private float _upDistance;
    private float _lastTime;
    private bool _timeFlag;

    void Awake()
    {
        battleManager = GameObject.Find("GameManager").GetComponent<BattleManager>();
        upFlag = true;
        dieFlag = false;
        isDisappear = false;
        eatingFlag = false;
        clickFlag = false;
        _timeFlag = true;
    }

    void Start()
    {
        _lastTime = 0;
        collider2D.enabled = true;
        _upDistance = upDistance * transform.parent.parent.localScale.x;     // 放到Update很好玩
    }

    void FixedUpdate()
    {
        #region Amination

        if (upFlag && transform.parent.localPosition.y < upDistance)        // AnimationUp
            StartCoroutine(AnimationUp());

        if (isDisappear && transform.parent.localPosition.y > -_upDistance) // AnimationDown
            StartCoroutine(AnimationDown());

        if (transform.gameObject.activeSelf == true && _timeFlag)  // 如果被Spawn儲存現在時間 注意 DisActive時Time還是會一直跑 所以要存起來減掉
        {
            _timeFlag = false;
            _lastTime = Time.time;
        }

        aliveTime = Time.time - _lastTime;                                                              // 老鼠存活時間 
        Animator anims = GetComponent("Animator") as Animator;   // 播放 死亡動畫                  
        AnimatorStateInfo currentState = anims.GetCurrentAnimatorStateInfo(0);      // 取得目前動畫狀態 (0) = Layer
        //Debug.Log("currentState : " + currentState.nameHash);
        if (currentState.nameHash == Animator.StringToHash("Layer1.Hello"))                    // 如果 目前 動化狀態 是 up
        {
            animTime = currentState.normalizedTime;

            // 目前播放的動畫 "總"時間
            if (animTime > 1)   // 動畫撥放完畢時
            {

                anims.Play("Idle");   // 老鼠開始吃東西
                upFlag = true;
            }
        }
        else if (currentState.nameHash == Animator.StringToHash("Layer1.Die"))              // 如果 目前 動畫狀態 是 die
        {
            animTime = currentState.normalizedTime;                                         // 目前播放的動畫 "總"時間
            if (!dieFlag)       // 限制執行一次
            {
                if (animTime > 0.5)   // 動畫撥放完畢時
                {
                    OnDied(aliveTime);
                    dieFlag = true;
                }
            }
        }
        else if (currentState.nameHash == Animator.StringToHash("Layer1.Idle"))
        {
            animTime = currentState.normalizedTime;

//            Debug.Log(animTime);
            if (!eatingFlag)        // 限制執行一次
            {
                if (animTime > 5)                       // 動畫撥放完畢時
                {
                    isDisappear = true;
                    eatingFlag = true;
                }
            }
        }
        #endregion
    }

    void OnHit()
    {
        Debug.Log("HIT!");
        if (!clickFlag)  //＊＊＊＊＊＊＊超快還是會combo ＊＊＊＊＊　有時間在改
        {
            clickFlag = true;
            collider2D.enabled = false;
            GetComponent<Animator>().Play("Die");
        }
    }

    void OnDisappear(float aliveTime)
    {
        this.transform.parent.parent = GameObject.Find("ObjectPool/" + transform.parent.name).transform;
        gameObject.SetActive(false);
        //Debug.Log("OnDisappear : " + aliveTime);
        try
        {
            _lastTime = aliveTime;
            battleManager.LostScore(transform.parent.name, aliveTime);  // 跑掉掉分
        }
        catch (Exception e)
        {
            Debug.Log("失去連線，重新連線中‧‧‧  你想太多了");
            throw e;
        }
        Global.MiceCount--;
    }

    void OnDied(float aliveTime)
    {
        this.transform.parent.parent = GameObject.Find("ObjectPool/" + transform.parent.name).transform;
        gameObject.SetActive(false);

        try
        {
            _lastTime = aliveTime;
            battleManager.UpadateScore(transform.parent.name, aliveTime);  // 增加分數
        }
        catch (Exception e)
        {
            Debug.Log("失去連線，重新連線中‧‧‧ 你想太多了");
            throw e;
        }
        Global.MiceCount--;
    }

    public void Play()
    {
        battleManager = GameObject.Find("GameManager").GetComponent<BattleManager>();
        upFlag = true;
        dieFlag = false;
        isDisappear = false;
        eatingFlag = false;
        clickFlag = false;
        collider2D.enabled = true;
        _timeFlag = true;

        _upDistance = upDistance * transform.parent.parent.localScale.x;     // 放到Update很好玩
        _lerpSpeed = upSpeed;
        GetComponent<Animator>().Play("Hello");
        transform.parent.localPosition = new Vector3(0, 0);
        //transform.parent.GetComponent<Animator>().Play("Up");
    }

    IEnumerator AnimationUp()
    {
        collider2D.enabled = true;
        _lerpSpeed = Mathf.Lerp(_lerpSpeed, 1, lerpSpeed);
        if (transform.parent.localPosition.y + _lerpSpeed > _upDistance)
        {
            transform.parent.localPosition = new Vector3(0, _upDistance, 0);
            upFlag = false;
        }
        else
        {
            transform.parent.localPosition += new Vector3(0, _lerpSpeed, 0);
        }
        yield return null;
    }

    IEnumerator AnimationDown() // 2   = 2 ~ 1
    {
        _lerpSpeed = Mathf.Lerp(_lerpSpeed, 1, lerpSpeed);
        if (transform.parent.localPosition.y - 10 <= -_upDistance)
        {
            Vector3 _tmp;
            _tmp = new Vector3(0, -_upDistance * 2, 0);
            transform.parent.localPosition = _tmp;
            OnDisappear(aliveTime);
            isDisappear = false;
        }
        else
        {
            transform.parent.localPosition = Vector3.Slerp(transform.parent.localPosition, new Vector3(0, -_upDistance * 2, 0), Time.deltaTime * 5);
        }
        yield return null;
    }
}


