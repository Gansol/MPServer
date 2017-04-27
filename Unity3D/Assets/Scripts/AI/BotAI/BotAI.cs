using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BotAI : MonoBehaviour
{
    BattleManager battleManager;

    public float lastTime, intervalTime;
    public int hitTimes, botLevel;

    //public void init(BattleManager battleManager)
    //{
    //    this.battleManager = battleManager;
    //    lastTime = 0;
    //    botLevel = 1;
    //    hitTimes = 1;
    //}
    // Use this for initialization
    void Start()
    {
        lastTime = 0;
        intervalTime = 1;
        botLevel = 1;
        hitTimes = 3;
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
    void Update()
    {

        if (Global.isGameStart && Global.dictBattleMice.Count > 0 && Time.time > lastTime)
        {
            lastTime += intervalTime * botLevel;

            Dictionary<Transform, GameObject> buffer = new Dictionary<Transform, GameObject>(Global.dictBattleMice);
            System.Random rnd = new System.Random();
            List<Transform> keys = new List<Transform>(buffer.Keys);

            for (int i = 0; i < hitTimes * botLevel; i++)
            {
                if (i >= hitTimes * botLevel)
                    break;

                int value = rnd.Next(keys.Count);
                if (  Global.dictBattleMice.ContainsKey(keys[value]) != null)
                    buffer[keys[rnd.Next(value)]].SendMessage("OnHit");
            }
        }

    }
}
