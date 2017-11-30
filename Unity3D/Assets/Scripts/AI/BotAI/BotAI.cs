using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BotAI
{
    private BattleManager battleManager;
    private Dictionary<Transform, GameObject> buffer;
    private System.Random rnd;
    private float lastAITime, lastBossHitTime, lastSkillTime, skillStartOffset, hitIntervalTime, skillIntervalTime, _lastGameTime;
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


    public BotAI(List<int> skillMiceIDs)
    {

        lastAITime = lastBossHitTime = lastSkillTime = 0;
        hitIntervalTime = 1;
        skillIntervalTime = 10;
        skillStartOffset = 10;
        botLevel = 1;
        hitTimes = 3;

        this.skillItemIDs = new List<int>();

        foreach (int miceID in skillMiceIDs)
        {
            object value;
            //Global.miceProperty.TryGetValue(miceID.ToString(), out value);

            //Dictionary<string,object> prop = value as Dictionary<string,object>;

            value = MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.miceProperty, "ItemID", miceID.ToString());
            this.skillItemIDs.Add(int.Parse(value.ToString()));

        }


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
        if (!Global.isGameStart)
        {
            _lastGameTime = Time.time;
        }
    }


    private void HitAI()
    {

        buffer = new Dictionary<Transform, GameObject>(Global.dictBattleMice);

        List<Transform> keys = new List<Transform>(buffer.Keys);

        for (int i = 0; i < Random.Range(0, hitTimes + 1) * botLevel; i++)
        {
            //if (i >= hitTimes * botLevel)
            //    break;

            int value = rnd.Next(keys.Count);
            if (Global.dictBattleMice.ContainsKey(keys[value]) != null)
            {
                if (buffer[keys[value]].GetComponent<MiceBossBase>() != null && lastBossHitTime > Time.time)
                {
                    lastBossHitTime = hitIntervalTime / 2 + Time.time;
                }
                else
                {
                    if (buffer[keys[value]].GetComponent<Bali>())
                    {
                        if (Random.Range(0, 100 + 1) == 100)
                        {
                            buffer[keys[value]].SendMessage("OnHit");
                            Debug.Log("Hit Bali");
                        }

                    }
                    else
                    {
                        buffer[keys[value]].SendMessage("OnHit");
                    }
                }
            }
        }
    }

    // 錯誤
    private void SkillAI()
    {
        short id, value;
        Dictionary<string, object> prop;
        if (Time.time > _lastGameTime + skillStartOffset)
        {

            if (lastSkillTime % 10 == 0)
            {
                lastSkillTime += skillIntervalTime;
                id = System.Convert.ToInt16(rnd.Next(0, skillMiceIDs.Count + 1));
                id = System.Convert.ToInt16(skillMiceIDs[id].ToString());
                if (Global.miceProperty.ContainsKey(id.ToString()))
                {
                    prop = Global.miceProperty[id.ToString()] as Dictionary<string, object>;
                    prop.TryGet("MiceCost", out value);
                    Global.photonService.SendSkillMice(id, value);

                    Debug.Log("ID:" + id + "  MiceCost:" + value);
                }
                
            }

            if (lastSkillTime % 20 == 0 && lastSkillTime != 0)
            {
                lastSkillTime += skillIntervalTime;

                id = System.Convert.ToInt16(rnd.Next(0, skillItemIDs.Count + 1));
                id = System.Convert.ToInt16(skillItemIDs[id].ToString());
                id = System.Convert.ToInt16(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.itemProperty, "SkillID", id.ToString()));

                prop = Global.dictSkills[id.ToString()] as Dictionary<string, object>;
                prop.TryGet("SkillType", out value);

                Global.photonService.SendSkillItem(id, value);
                Debug.Log("ID:" + id + "  SkillType:" + value);
                
            }

            _lastGameTime +=skillStartOffset;

        }
    }
}
