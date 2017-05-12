using UnityEngine;
using System.Collections;
using MPProtocol;

public class CrazyBattleAIState : BattleAIState
{
    int carzyMaxScore = 10000, carzyCombo = 100;

    public CrazyBattleAIState()
    {
        Debug.Log("Now State: CrazyBattleAIState");
        battleAIState = ENUM_BattleAIState.CarzyMode;
        normalSpawn = 75;
        spawnCount = 12;
        lerpTime = 0.15f;
        spawnTime = 0.35f;
        intervalTime = 2.5f;
        minStatus = 1;
        maxStatus = 3;
        minMethod = System.Enum.GetNames(typeof(ENUM_SpawnMethod)).Length / 2;
        maxMethod = System.Enum.GetNames(typeof(ENUM_SpawnMethod)).Length - 1;
        minSpawnInterval = -1f;
        maxSpawnInterval = 3;
        spawnIntervalTime = 2f;
        spawnSpeed = .9f;
        wave = 0;
        nextBali = 3; nextMuch = 10; nextHero = 40;

        pervStateTime = Global.GameTime - 30;
    }


    public override void UpdateState()
    {
        if (battleManager.combo < carzyCombo && battleManager.score < carzyMaxScore && BattleManager.gameTime < pervStateTime)
        {
            battleManager.SetSpawnState(new HardBattleAIState());
        }
        else if (BattleManager.gameTime > lastTime + spawnOffset)
        {
            nowCombo += (battleManager.combo - nowCombo > 0) ? (short)(battleManager.combo - nowCombo) : (short)0;

            if (nowCombo < normalSpawn)
            {
                // normal spawn
                Spawn(defaultMice, spawnCount);// 錯誤
                lastTime = BattleManager.gameTime + spawnIntervalTime * 2;
            }
            else
            {
                // spceial spawn
                SpawnSpecial(Random.Range(minMethod, maxMethod), defaultMice, spawnSpeed, spawnCount);    // 錯誤
                lastTime = BattleManager.gameTime + spawnState.GetIntervalTime();
            }
            SetSpawnIntervalTime();
        }
    }
}
