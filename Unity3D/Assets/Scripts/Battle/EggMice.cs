using UnityEngine;
using System;

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

    void Awake()
    {
        battleManager = new BattleManager();
        upFlag = false;
        dieFlag = false;
        disappearFlag = false;
        eatingFlag = false;
        clickFlag = false;
    }


    void Update()
    {
       // Debug.Log(" DieFlag: " + dieFlag + " upFlag: " + upFlag + " disappearFlag: " + disappearFlag + " eatingFlag : " + eatingFlag);
        aliveTime = Time.time;                                                              // 老鼠存活時間                             
        AnimatorStateInfo currentState = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);      // 取得目前動畫狀態 (0) = Layer

        if (currentState.nameHash == Animator.StringToHash("Layer1.up"))                    // 如果 目前 動化狀態 是 up
        {
            animTime = currentState.normalizedTime;                                         // 目前播放的動畫 "總"時間
            if (!upFlag)        // 限制執行一次
            {
                if (animTime > 1)   // 動畫撥放完畢時
                {
                    GetComponent<Animator>().Play("eating");   // 老鼠開始吃東西
                    upFlag = true;
                }

            }
        }
        else if (currentState.nameHash == Animator.StringToHash("Layer1.die"))              // 如果 目前 動化狀態 是 die
        {
            animTime = currentState.normalizedTime;                                         // 目前播放的動畫 "總"時間
            if (!dieFlag)       // 限制執行一次
            {
                if (animTime > 3)   // 動畫撥放完畢時
                {

                    OnDied(aliveTime);
                    dieFlag = true;
                }

            }
        }
        else if (currentState.nameHash == Animator.StringToHash("Layer1.eating"))
        {
            animTime = currentState.normalizedTime;   
            if (!eatingFlag)        // 限制執行一次
            {
                if (animTime > 10)                       // 動畫撥放完畢時
                {

                    GetComponent<Animator>().Play("disppear");     // 老鼠消失了
                    eatingFlag = true;
                }

            }
        }
        else if (currentState.nameHash == Animator.StringToHash("Layer1.disppear"))
        {
            animTime = currentState.normalizedTime; // 目前播放的動畫 "總"時間
            if (!disappearFlag)     // 限制執行一次
            {
                if (animTime > 0.1)                      // 動畫撥放完畢時
                {

                    GetComponent<Animator>().Play("default");      // 老鼠消失了 回到初始狀態
                    OnDisappear(aliveTime);
                    disappearFlag = true;
                }

            }
        }
    }

    void OnClick()
    {
        if (!clickFlag)  //＊＊＊＊＊＊＊超快還是會combo ＊＊＊＊＊　有時間在改
        {
            clickFlag = true;
            collider2D.enabled = false;
            GetComponent<Animator>().Play("die");   // 播放 死亡動畫
        }

    }

    void OnDisappear(float aliveTime)
    {
        this.transform.parent = GameObject.Find("ObjectPool/" + name).transform;
        GetComponent<UISprite>().enabled = false;
        try
        {
            battleManager.LostScore(this.name, aliveTime);  // 跑掉掉分
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
        this.transform.parent = GameObject.Find("ObjectPool/" + name).transform;
        GetComponent<UISprite>().enabled = false;

        try
        {
            battleManager.UpadateScore(this.name);  // 增加分數
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
        battleManager = new BattleManager();
        upFlag = false;
        dieFlag = false;
        disappearFlag = false;
        eatingFlag = false;
        clickFlag = false;
        collider2D.enabled = true;
        GetComponent<Animator>().Play("up");

    }
}


