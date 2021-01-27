using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gansol;

public class BotAI
{
    private BattleSystem battleManager;
    private Dictionary<Transform, GameObject> buffer;
    private System.Random rnd;
    private float lastAITime, lastBossHitTime, lastSkillTime, skillStartOffset, hitIntervalTime, skillIntervalTime, _lastGameTime;
    private int hitTimes, botLevel;

    private List<int> _skillMiceIDs, _skillItemIDs;
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
        rnd = new System.Random();
        lastAITime = lastBossHitTime = lastSkillTime = 0;
        hitIntervalTime = .5f;
        skillIntervalTime = 10;
        skillStartOffset = 10;
        botLevel = 1;
        hitTimes = 5;//3

        _skillItemIDs = new List<int>();

        foreach (int miceID in skillMiceIDs)
        {
            object value;
            value = MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.miceProperty, "ItemID", miceID);
            if (value != null)
                _skillItemIDs.Add(int.Parse(value.ToString()));
        }

        _skillMiceIDs = skillMiceIDs;

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
        if (Global.isGameStart && MPGame.Instance.GetCreatureSystem().GetCreatures().Count > 0 && Time.fixedTime > lastAITime)
        {
            lastAITime += hitIntervalTime * botLevel;

            // HitAI(); 錯誤 暫時影藏
            SkillAI();

       //     Debug.Log("Time: " + Time.fixedTime + "  Last AI Time:" + lastAITime);
        }
        if (!Global.isGameStart)
        {
            _lastGameTime = Time.fixedTime;
        }
    }

    // 錯誤 暫時隱藏
    //private void HitAI()
    //{

    //    buffer = new Dictionary<Transform, GameObject>(Global.dictBattleMiceRefs);

    //    List<Transform> keys = new List<Transform>(buffer.Keys);

    //    for (int i = 0; i < Random.Range(0, hitTimes + 1) * botLevel; i++)
    //    {
    //        //if (i >= hitTimes * botLevel)
    //        //    break;

    //        int value = rnd.Next(keys.Count);
    //        if (Global.dictBattleMiceRefs.ContainsKey(keys[value]) != null)
    //        {
    //            //if (buffer[keys[value]].GetComponent<MiceBossBase>() != null && lastBossHitTime > Time.fixedTime)
    //            //{
    //            //    lastBossHitTime = hitIntervalTime / 2 + Time.fixedTime;
    //            //}
    //            //else
    //            //{
    //                //if (buffer[keys[value]].GetComponent<Bali>())
    //                //{
    //                //    if (Random.Range(0, 1000 + 1) == 1000 || Random.Range(0, 1000 + 1) == 87)
    //                //    {
    //                //        buffer[keys[value]].SendMessage("OnHit");
    //                //        Debug.Log("Hit Bali");
    //                //    }

    //                //}
    //                //else
    //                //{
    //                if (!buffer[keys[value]].GetComponent<Bali>())
    //                {
    //                    buffer[keys[value]].SendMessage("OnHit");
    //                    Debug.Log("Hit!:" + buffer[keys[value]].name);
    //                }
    //             //   }
    //            //}
    //        }
    //    }
    //}

    // 錯誤
    private void SkillAI()
    {
        //short id, value;
        //Dictionary<string, object> prop;
        //if (Time.time > _lastGameTime + skillStartOffset)
        //{

        //    if (lastSkillTime % 20 == 0)
        //    {
        //        lastSkillTime += skillIntervalTime;
        //        id = System.Convert.ToInt16(rnd.Next(0, _skillMiceIDs.Count + 1));
        //        id = System.Convert.ToInt16(_skillMiceIDs[id].ToString());
        //        if (Global.miceProperty.ContainsKey(id.ToString()))
        //        {
        //            prop = Global.miceProperty[id.ToString()] as Dictionary<string, object>;
        //            prop.TryGet("MiceCost", out value);
        //            Global.photonService.SendSkillMice(id, value);

        //            Debug.Log("ID:" + id + "  MiceCost:" + value);
        //        }

        //    }

        //    if (lastSkillTime % 20 == 0 && lastSkillTime != 0)
        //    {
        //        lastSkillTime += skillIntervalTime;

        //        id = System.Convert.ToInt16(rnd.Next(0, _skillItemIDs.Count + 1));
        //        id = System.Convert.ToInt16(_skillItemIDs[id].ToString());
        //        id = System.Convert.ToInt16(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.itemProperty, "SkillID", id.ToString()));
        //        prop = Global.dictSkills[id.ToString()] as Dictionary<string, object>;
        //        prop.TryGet("SkillType", out value);

        //        Global.photonService.SendSkillItem(id, value);
        //        Debug.Log("ID:" + id + "  SkillType:" + value);
        //    }

        //    _lastGameTime += skillStartOffset;

        //}
    }
}
