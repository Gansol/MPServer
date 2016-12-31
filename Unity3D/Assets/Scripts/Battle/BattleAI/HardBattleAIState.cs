using UnityEngine;
using System.Collections;
using MPProtocol;

public class HardBattleAIState : BattleAIState
{
    int carzyScore = 5000, carzyMaxScore = 10000, carzyCombo = 100, carzyTime = 270, hardMaxScore = 3000, hardCombo = 75;

    public HardBattleAIState()
    {
        Debug.Log("Now State: HardBattleAIState");
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
    }

    public override void UpdateState()
    {

        if ((battleManager.score > carzyScore && battleManager.combo > carzyCombo) || (battleManager.score > carzyMaxScore && battleManager.combo > carzyCombo) || battleManager.gameTime > Global.GameTime - 50)
        {
            battleManager.SetSpawnState(new CrazyBattleAIState());
        }
        else if (battleManager.combo < hardCombo && battleManager.score < hardMaxScore && battleManager.gameTime < Global.GameTime / 2)
        {
            battleManager.SetSpawnState(new NormalBattleAIState());
        }
        else if (battleManager.gameTime > lastTime + spawnOffset)
        {
            nowCombo += (battleManager.combo - nowCombo > 0) ? (short)(battleManager.combo - nowCombo) : (short)0;

            if (nowCombo < normalSpawn)
            {
                // normal spawn
                Spawn("EggMice");
                lastTime = battleManager.gameTime + spawnIntervalTime * 2;
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
