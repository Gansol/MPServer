using UnityEngine;
using System.Collections;
using MPProtocol;

public class CrazyBattleAIState : BattleAIState
{
    int carzyScore = 5000, carzyMaxScore = 10000, carzyCombo = 100, carzyTime = 270;

    public CrazyBattleAIState()
    {
        Debug.Log("Now State: CrazyBattleAIState");
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
    }


    public override void UpdateState()
    {
        if (battleManager.combo < carzyCombo && battleManager.score < carzyMaxScore && battleManager.gameTime < Global.GameTime - 50)
        {
            battleManager.SetSpawnState(new HardBattleAIState());
        }
        else if (battleManager.gameTime > lastTime + spawnOffset)
        {
            nowCombo += (battleManager.combo - nowCombo > 0) ? (short)(battleManager.combo - nowCombo) : (short)0;

            if (nowCombo < normalSpawn)
            {
                // normal spawn
                Spawn("EggMice");
                lastTime = battleManager.gameTime + spawnIntervalTime*2;
            }
            else
            {
                // spceial spawn
                SpawnSpecial(Random.Range(minMethod, maxMethod), "EggMice", spawnSpeed);
                lastTime = battleManager.gameTime + spawnState.GetIntervalTime();
            }
            SetSpawnIntervalTime();
        }
    }
}
