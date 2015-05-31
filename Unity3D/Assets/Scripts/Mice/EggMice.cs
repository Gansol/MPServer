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
 * 蛋殼鼠專用腳本
 * 被點到時、跑掉時、播放動畫
 * 這裡可能出現S級BUG 卡洞!!!
 * 
 * ***************************************************************/

public class EggMice : MonoBehaviour
{
    BattleManager battleManager;

    float aliveTime;
    float animTime;

    bool upFlag;
    bool dieFlag;
    bool disappearFlag;
    bool eatingFlag;
    bool clickFlag;

    public float upDistance; // mouse pos
    public float upSpeed;
    [Range(0.01f,0.99f)]
    public float lerpSpeed;

    private float _lerpSpeed;
    private float _upDistance;

    void Awake()
    {
        battleManager = GameObject.Find("GameManager").GetComponent<BattleManager>();
        upFlag = false;
        dieFlag = false;
        disappearFlag = false;
        eatingFlag = false;
        clickFlag = false;
    }

    void Start()
    {
         _upDistance = upDistance * transform.parent.parent.localScale.x;     // 放到Update很好玩
    }

    void FixedUpdate()
    {

        if (transform.parent.localPosition.y < upDistance)
            StartCoroutine(AnimationUp());

    }

    void Update()
    {
        #region Amination
        // Debug.Log(" DieFlag: " + dieFlag + " upFlag: " + upFlag + " disappearFlag: " + disappearFlag + " eatingFlag : " + eatingFlag);
        aliveTime = Time.time;                                                              // 老鼠存活時間 
        Animator anims = GetComponent("Animator") as Animator;   // 播放 死亡動畫                  
        AnimatorStateInfo currentState = anims.GetCurrentAnimatorStateInfo(0);      // 取得目前動畫狀態 (0) = Layer
        //Debug.Log("currentState : " + currentState.nameHash);
        if (currentState.nameHash == Animator.StringToHash("Layer1.Hello"))                    // 如果 目前 動化狀態 是 up
        {
            animTime = currentState.normalizedTime;
            //            Debug.Log(this.transform.parent.parent.name + "   animTime:" + animTime);
            // 目前播放的動畫 "總"時間
            if (!upFlag)        // 限制執行一次
            {
                if (animTime > 1)   // 動畫撥放完畢時
                {

                    anims.Play("Eat");   // 老鼠開始吃東西
                    upFlag = true;
                }

            }
        }
        else if (currentState.nameHash == Animator.StringToHash("Layer1.Die"))              // 如果 目前 動畫狀態 是 die
        {

            animTime = currentState.normalizedTime;                                         // 目前播放的動畫 "總"時間
            // Debug.Log("(D)animTime = " + animTime);
            if (!dieFlag)       // 限制執行一次
            {
                if (animTime > 0.5)   // 動畫撥放完畢時
                {

                    OnDied(aliveTime);
                    dieFlag = true;
                }

            }
        }
        else if (currentState.nameHash == Animator.StringToHash("Layer1.Eat"))
        {
            animTime = currentState.normalizedTime;
            if (!eatingFlag)        // 限制執行一次
            {
                if (animTime > 5)                       // 動畫撥放完畢時
                {
                    anims.Play("Die");     // 老鼠消失了  ＊＊＊＊＊暫時用ＤＩＥ取代要改回Ｄｉｓｐｐｅａｒ＊＊＊＊＊
                    eatingFlag = true;
                    clickFlag = false;                          // ＊＊＊＊＊改回DISPPEAR時去掉＊＊＊＊＊
                }

            }
        }
        else if (currentState.nameHash == Animator.StringToHash("Layer1.Disppear")) //disppear 現在暫時用DIE
        {
            animTime = currentState.normalizedTime; // 目前播放的動畫 "總"時間
            //Debug.Log("(D)animTime = " + animTime);
            if (!disappearFlag)     // 限制執行一次
            {
                if (animTime > 0.1)                      // 動畫撥放完畢時
                {

                    anims.Play("Idle");      // 老鼠消失了 回到初始狀態
                    OnDisappear(aliveTime);
                    disappearFlag = true;
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
        this.transform.parent = GameObject.Find("ObjectPool/" + transform.parent.name).transform;
        //transform.parent.GetComponent<Animator>().Play("Default");
        gameObject.SetActive(false);

        try
        {
            battleManager.LostScore(transform.parent.name, aliveTime);  // 跑掉掉分

        }
        catch (Exception e)
        {
            Debug.Log("失去連線，重新連線中‧‧‧  你想太多了");
            throw e;
        }
        collider2D.enabled = false;
        Global.MiceCount--;
    }

    void OnDied(float aliveTime)
    {
        this.transform.parent.parent = GameObject.Find("ObjectPool/" + transform.parent.name).transform;
        //transform.parent.GetComponent<Animator>().Play("Default");
        gameObject.SetActive(false);

        try
        {
            battleManager.UpadateScore(transform.parent.name);  // 增加分數
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
        upFlag = false;
        dieFlag = false;
        disappearFlag = false;
        eatingFlag = false;
        clickFlag = false;
        collider2D.enabled = true;

        _upDistance = upDistance * transform.parent.parent.localScale.x;     // 放到Update很好玩
        _lerpSpeed = upSpeed;
        GetComponent<Animator>().Play("Hello");
        transform.parent.localPosition = new Vector3(0, 0);
        //transform.parent.GetComponent<Animator>().Play("Up");
    }

    IEnumerator AnimationUp()
    {
        _lerpSpeed = Mathf.Lerp(_lerpSpeed, 1, lerpSpeed);
        if (transform.parent.localPosition.y + _lerpSpeed > _upDistance)
        {
            transform.parent.localPosition = new Vector3(0, _upDistance);
        }
        else
        {
            transform.parent.localPosition += new Vector3(0, _lerpSpeed);
        }
        yield return null;
    }
}


