using UnityEngine;
using System.Collections;
using MPProtocol;

public class CrazyBattleAIState : IBattleAIState
{
    int carzyMaxScore = 10000, carzyCombo = 100;

    public CrazyBattleAIState( BattleAttr battleAttr)
        : base( battleAttr)
    {
        Debug.Log("Now State: CrazyBattleAIState");
        stateAttr = new BattleAIStateAttr();
        stateAttr.battleAIState = ENUM_BattleAIState.CarzyMode;
        stateAttr.normalSpawn = 75;
        stateAttr.spawnCount = 12;
        stateAttr.lerpTime = 0.15f;
        stateAttr.spawnTime = 0.35f;
        stateAttr.intervalTime = 2.5f;
        stateAttr.minStatus = 1;
        stateAttr.maxStatus = 3;
        stateAttr.minMethod = System.Enum.GetNames(typeof(ENUM_SpawnMethod)).Length / 2;
        stateAttr.maxMethod = System.Enum.GetNames(typeof(ENUM_SpawnMethod)).Length - 1;
        stateAttr.minSpawnInterval = -1f;
        stateAttr.maxSpawnInterval = 3;
        stateAttr.spawnIntervalTime = 2f;
        stateAttr.spawnSpeed = .9f;
        stateAttr.wave = 0;
        stateAttr.nextBali = 3; stateAttr.nextMuch = 10; stateAttr.nextHero = 40;

        stateAttr.pervStateTime = Global.GameTime - 30;
    }


    public override void UpdateState()
    {
        if (battleAttr.combo < carzyCombo && battleAttr.score < carzyMaxScore && battleAttr.gameTime < stateAttr.pervStateTime)
        {
            MPGame.Instance.GetBattleSystem().SetSpawnState(new HardBattleAIState( battleAttr));
        }
        else if (battleAttr.gameTime > stateAttr.lastTime + stateAttr.spawnOffset)
        {
            stateAttr.nowCombo += (battleAttr.combo - stateAttr.nowCombo > 0) ? (short)(battleAttr.combo - stateAttr.nowCombo) : (short)0;

            if (stateAttr.nowCombo < stateAttr.normalSpawn)
            {
                // normal spawn
                Spawn(stateAttr.defaultMice, stateAttr);// 錯誤
                stateAttr.lastTime = battleAttr.gameTime + stateAttr.spawnIntervalTime * 2;
            }
            else
            {
                // spceial spawn
                SpawnSpecial( stateAttr.defaultMice, stateAttr);    // 錯誤
                stateAttr.lastTime = battleAttr.gameTime + stateAttr.spawnState.GetIntervalTime();
            }
            SetSpawnIntervalTime();
        }
    }
}
