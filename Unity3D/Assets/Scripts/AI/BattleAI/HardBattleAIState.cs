using UnityEngine;
using System.Collections;
using MPProtocol;

public class HardBattleAIState : IBattleAIState
{
    int carzyScore = 7500, carzyMaxScore = 10000, carzyCombo = 100, hardMaxScore = 5000, hardCombo = 75;

    public HardBattleAIState( BattleAttr battleAttr)
        : base( battleAttr)
    {
        Debug.Log("Now State: HardBattleAIState");
        stateAttr = new BattleAIStateAttr();
        stateAttr.battleAIState = ENUM_BattleAIState.HardMode;
        stateAttr.normalSpawn = 50;
        stateAttr.spawnCount = 12;
        stateAttr.lerpTime = 0.06f;
        stateAttr.spawnTime = 0.45f;
        stateAttr.intervalTime = 3f;
        stateAttr.minStatus = 1;
        stateAttr.maxStatus = 3;
        stateAttr.minMethod = System.Enum.GetNames(typeof(ENUM_SpawnMethod)).Length / 4;
        stateAttr.maxMethod = System.Enum.GetNames(typeof(ENUM_SpawnMethod)).Length - 1;
        stateAttr.minSpawnInterval = -1.5f;
        stateAttr.maxSpawnInterval = 2;
        stateAttr.spawnIntervalTime = 1.5f;
        stateAttr.spawnSpeed = .95f;
        stateAttr.wave = 0;
        stateAttr.nextBali = 3; stateAttr.nextMuch = 15; stateAttr.nextHero = 45;

        stateAttr.pervStateTime = Global.GameTime / 2;
        stateAttr.nextStateTime = Global.GameTime - 30;
    }

    public override void UpdateState()
    {

        if ((battleAttr.score > carzyScore && battleAttr.combo > carzyCombo) || (battleAttr.score > carzyMaxScore && battleAttr.combo > carzyCombo) || battleAttr.gameTime > stateAttr.nextStateTime)
        {
            MPGame.Instance.GetBattleSystem().SetSpawnState(new CrazyBattleAIState( battleAttr));
        }
        else if (battleAttr.combo < hardCombo && battleAttr.score < hardMaxScore && battleAttr.gameTime < stateAttr.pervStateTime)
        {
            MPGame.Instance.GetBattleSystem().SetSpawnState(new NormalBattleAIState( battleAttr));
        }
        else if (battleAttr.gameTime > stateAttr.lastTime + stateAttr.spawnOffset)
        {
            stateAttr.nowCombo += (battleAttr.combo - stateAttr.nowCombo > 0) ? (short)(battleAttr.combo - stateAttr.nowCombo) : (short)0;

            if (stateAttr.nowCombo < stateAttr.normalSpawn)
            {
                // normal spawn
                Spawn(stateAttr.defaultMice, stateAttr);   //錯誤
                stateAttr.lastTime = battleAttr.gameTime + stateAttr.spawnIntervalTime * 2;
            }
            else
            {
                // spceial spawn
                SpawnSpecial( stateAttr.defaultMice, stateAttr);    //錯誤
                stateAttr.lastTime = battleAttr.gameTime + stateAttr.spawnState.GetIntervalTime();
            }

            SetSpawnIntervalTime();
        }
    }
}
