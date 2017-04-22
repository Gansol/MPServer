using UnityEngine;
using System.Collections;
using MPProtocol;

public class HardBattleAIState : BattleAIState
{
    int carzyScore = 7500, carzyMaxScore = 10000, carzyCombo = 100, hardMaxScore = 5000, hardCombo = 75;

    public HardBattleAIState()
    {
        Debug.Log("Now State: HardBattleAIState");
        battleAIState = ENUM_BattleAIState.HardMode;
        normalSpawn = 50;
        spawnCount = 12;
        lerpTime = 0.06f;
        spawnTime = 0.45f;
        intervalTime = 3f;
        minStatus = 1;
        maxStatus = 3;
        minMethod = System.Enum.GetNames(typeof(ENUM_SpawnMethod)).Length / 4;
        maxMethod = System.Enum.GetNames(typeof(ENUM_SpawnMethod)).Length - 1;
        minSpawnInterval = -1.5f;
        maxSpawnInterval = 2;
        spawnIntervalTime = 1.5f;
        spawnSpeed = .95f;
        totalSpawn = 0;
        wave = 0;
        nextBali = 4; nextMuch = 27; nextSuper = 50;
    }

    public override void UpdateState()
    {

        if ((battleManager.score > carzyScore && battleManager.combo > carzyCombo) || (battleManager.score > carzyMaxScore && battleManager.combo > carzyCombo) || BattleManager.gameTime > Global.GameTime - 50)
        {
            battleManager.SetSpawnState(new CrazyBattleAIState());
        }
        else if (battleManager.combo < hardCombo && battleManager.score < hardMaxScore && BattleManager.gameTime < Global.GameTime / 2)
        {
            battleManager.SetSpawnState(new NormalBattleAIState());
        }
        else if (BattleManager.gameTime > lastTime + spawnOffset)
        {
            nowCombo += (battleManager.combo - nowCombo > 0) ? (short)(battleManager.combo - nowCombo) : (short)0;

            if (nowCombo < normalSpawn)
            {
                // normal spawn
                Spawn(10001, spawnCount);   //錯誤
                lastTime = BattleManager.gameTime + spawnIntervalTime * 2;
            }
            else
            {
                // spceial spawn
                SpawnSpecial(Random.Range(minMethod, maxMethod), 10001, spawnSpeed, spawnCount);    //錯誤
                lastTime = BattleManager.gameTime + spawnState.GetIntervalTime();
            }

            SetSpawnIntervalTime();
        }
    }
}
