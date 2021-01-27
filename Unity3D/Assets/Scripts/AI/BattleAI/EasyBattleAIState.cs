using UnityEngine;
using System.Collections;
using MPProtocol;
using System.Collections.Generic;



public class EasyBattleAttr : BattleAttr
{
  //  class EasyBattleAttr { }
}

public class EasyBattleAIState : IBattleAIState
{
    int normalScore = 500, normalMaxScore = 1000, normalCombo = 50;

    public EasyBattleAIState(BattleAttr battleAttr) 
        : base ( battleAttr)
    {
        stateAttr = new BattleAIStateAttr();
        Debug.Log("Now State: EasyBattleAIState");
        stateAttr.battleAIState = ENUM_BattleAIState.EasyMode;
        stateAttr.normalSpawn = 25;    //25 50 50 75
        stateAttr.spawnCount = 6;
        stateAttr.lerpTime = 0.035f;
        stateAttr.spawnTime = 0.4f;
        stateAttr.intervalTime = 1f;
        stateAttr.minStatus = 0;
        stateAttr.maxStatus = 1;
        stateAttr.minMethod = 0;
        stateAttr.maxMethod = System.Enum.GetNames(typeof(ENUM_SpawnMethod)).Length / 3;
        stateAttr.minSpawnInterval = -1;
        stateAttr.maxSpawnInterval = 1;
        stateAttr.lastTime = stateAttr.spawnIntervalTime = 1.5f;
        stateAttr.spawnSpeed = 1.2f;
        stateAttr.wave = 0;
        stateAttr.nextBali = 4; stateAttr.nextMuch = 27; stateAttr.nextHero = 50;

        stateAttr.pervStateTime = 0;
        stateAttr.nextStateTime = Global.GameTime / 4;
    }

    public override void UpdateState()
    {
        if ((battleAttr.score > normalScore && battleAttr.combo > normalCombo) || battleAttr.score > normalMaxScore || battleAttr.gameTime > stateAttr.nextStateTime)
        {
          MPGame.Instance.GetBattleSystem().SetBattleAIState(new NormalBattleAIState( battleAttr));
        }
        else if (battleAttr.gameTime > stateAttr.lastTime + stateAttr.spawnOffset)
        {
            stateAttr.nowCombo += (battleAttr.combo - stateAttr.nowCombo > 0) ? (short)(battleAttr.combo - stateAttr.nowCombo) : (short)0;

            if (stateAttr.nowCombo < stateAttr.normalSpawn)
            {
                // normal spawn
                Spawn(stateAttr.defaultMice, stateAttr);//錯誤
                stateAttr.lastTime = battleAttr.gameTime + stateAttr.spawnIntervalTime * 3;
            }
            else
            {
                // spceial spawn
                SpawnSpecial( stateAttr.defaultMice, stateAttr);//錯誤
                stateAttr.lastTime = battleAttr.gameTime + stateAttr.spawnState.GetIntervalTime();
            }

            SetSpawnIntervalTime();                             // 自動調整間隔時間(依照玩家能力)
        }
    }
}
