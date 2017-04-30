using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BotAI
{
    private BattleManager battleManager;
    private Dictionary<Transform, GameObject> buffer;
    private System.Random rnd;
    private float lastAITime, lastBossHitTime, lastSkillTime, skillStartOffset, hitIntervalTime, skillIntervalTime;
    private int hitTimes, botLevel;

    private List<int> skillMiceIDs, skillItemIDs;
    //public void init(BattleManager battleManager)
    //{
    //    this.battleManager = battleManager;
    //    lastTime = 0;
    //    botLevel = 1;
    //    hitTimes = 1;
    //}
    // Use this for initialization


    public BotAI(List<int> skillMiceIDs, List<int> skillItemIDs)
    {
        lastAITime = lastBossHitTime = lastSkillTime = 0;
        hitIntervalTime = 1;
        skillIntervalTime = 5;
        skillStartOffset = 10;
        botLevel = 1;
        hitTimes = 3;

        this.skillMiceIDs = skillMiceIDs;
        this.skillItemIDs = skillItemIDs;

        rnd = new System.Random();





        //Global.dictBattleMice;
        // get battle mice
        // get skill
        // get mission
        // get status
        // get player level set bot level

        // level A all hit
        // level B not hit bali
        // level C energy *2
        // level D energy *2 score *2

    }

    // Update is called once per frame
    public void UpdateAI()
    {

        if (Global.isGameStart && Global.dictBattleMice.Count > 0 && Time.time > lastAITime)
        {
            lastAITime += hitIntervalTime * botLevel;

            HitAI();
            SkillAI();
        }

    }


    private void HitAI()
    {

        buffer = new Dictionary<Transform, GameObject>(Global.dictBattleMice);

        List<Transform> keys = new List<Transform>(buffer.Keys);

        for (int i = 0; i < hitTimes * botLevel; i++)
        {
            if (i >= hitTimes * botLevel)
                break;

            int value = rnd.Next(keys.Count);
            if (Global.dictBattleMice.ContainsKey(keys[value]) != null)
            {
                if (buffer[keys[rnd.Next(value)]].GetComponent<MiceBossBase>() != null && lastBossHitTime > Time.time)
                {
                    lastBossHitTime = hitIntervalTime / 2 + Time.time;
                }
                else
                {
                    buffer[keys[rnd.Next(value)]].SendMessage("OnHit");
                }
            }
        }
    }


    private void SkillAI()
    {
        short id, value;
        Dictionary<string, object> prop;
        if (Time.time > lastSkillTime + skillStartOffset)
        {
            if (lastSkillTime % 20 != 0)
            {

                id = System.Convert.ToInt16(rnd.Next(0, skillMiceIDs.Count + 1));
                prop = Global.miceProperty[id.ToString()] as Dictionary<string, object>;
                prop.TryGet("MiceCost", out value);
                Global.photonService.SendSkillMice(id, value);

                Debug.Log("ID:" + id + "  MiceCost:" + value);
            }
            else
            {
                id = System.Convert.ToInt16(rnd.Next(0, skillItemIDs.Count + 1));
                prop = Global.dictSkills[id.ToString()] as Dictionary<string, object>;
                prop.TryGet("SkillType", out value);
                Global.photonService.SendSkillItem(id, value);
                Debug.Log("ID:" + id + "  SkillType:" + value);
            }

            lastSkillTime += skillIntervalTime;

        }
    }
}
