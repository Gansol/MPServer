using UnityEngine;
using System.Collections;
using System;
using MPProtocol;

public class BossPorperty : MonoBehaviour
{
    BattleHUD battleHUD;
    public float hpMax { get; set; }
    public float hp { get; set; }
    private Int16 myHits;
    private Int16 otherHits;

    private bool isDead;
    private bool flag;

    void Start()
    {
        Debug.Log("Start");
        otherHits = myHits = 0;
        isDead = false;
        flag = true;
        battleHUD = GameObject.Find("GameManager").GetComponent<BattleHUD>();
        Global.photonService.BossDamageEvent += OnBossDamage;
        Global.photonService.OtherDamageEvent += OnOtherDamage;
    }


    void Update()
    {
//        Debug.Log("(TURE)Global.isMissionCompleted:" + Global.isMissionCompleted + "   @@@@@@@@@@@@@@@@@@@@@@@isDead:" + isDead + "HPMAX:" + hpMax + "HP:" + hp + "HITS:" + myHits);
        if (!isDead)
        {
            if (hp <= 0)
                isDead = true;
            battleHUD.ShowBossHPBar(hp / hpMax, isDead);
        }
        else
        {
            Debug.Log("BOSS DEAD:" + hp);
            GetComponent<Animator>().Play("Die");

            if (Global.OtherData.RoomPlace != "Host" && flag)
            {//0*100=0
                flag = false;
                Int16 percent = (Int16)Math.Round((float)myHits / (float)(myHits + otherHits) * 100); // 整數百分比0~100% 目前是用打擊次數當百分比 如果傷害公式有變動需要修正
                Global.photonService.MissionCompleted((byte)Mission.WorldBoss, 1, percent, "");
                Debug.Log("percent:" + percent);
            }
            transform.parent.parent.GetComponent<Animator>().Play("HoleScale_R");
        }
    }

    void OnSpawn()
    {
        isDead = false;
        battleHUD = GameObject.Find("GameManager").GetComponent<BattleHUD>();
    }


    void OnBossDamage(Int16 damage)
    {
        hp -= damage;
        myHits++;
        Debug.Log("myHits" + myHits);
    }

    void OnOtherDamage(Int16 damage)
    {
        hp -= damage;
        otherHits++;
//        Debug.Log("otherHits" + otherHits);
    }

    void OnDestory()
    {
        Debug.Log("Script was destroyed");
        Global.photonService.BossDamageEvent -= OnBossDamage;
        Global.photonService.OtherDamageEvent -= OnOtherDamage;
        hp = otherHits = myHits = 0;
    }
}

