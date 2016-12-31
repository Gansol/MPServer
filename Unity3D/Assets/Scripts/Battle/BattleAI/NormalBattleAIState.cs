using UnityEngine;
using System.Collections;
using MPProtocol;

public class NormalBattleAIState : BattleAIState
{
    int hardScore = 1000, hardMaxScore = 3000, hardCombo = 75, hardTime = 200, normalMaxScore = 1000, normalCombo = 50;

    public NormalBattleAIState()
    {
        Debug.Log("Now State: NormalBattleAIState");
        normalSpawn = 50;
        spawnCount = 9;
        lerpTime = 0.065f;
        spawnTime = 0.4f;
        intervalTime = 1.8f;
        minStatus = 1;
        maxStatus = 2;
        minMethod = 0;
        maxMethod = System.Enum.GetNames(typeof(ENUM_SpawnMethod)).Length / 2;
        minSpawnInterval = -1.5f;
        maxSpawnInterval = 2;
        spawnIntervalTime = 1.5f;
        spawnSpeed = 1.1f;
    }

    public override void UpdateState()
    {
        if ((battleManager.score > hardScore && battleManager.combo > hardCombo) || battleManager.score > hardMaxScore || battleManager.gameTime > Global.GameTime / 2)
        {
            battleManager.SetSpawnState(new HardBattleAIState());
        }
        else if (battleManager.combo < normalCombo && battleManager.score < normalMaxScore && battleManager.gameTime < Global.GameTime / 4)
        {
            battleManager.SetSpawnState(new EasyBattleAIState());
        }
        else if (battleManager.gameTime > lastTime + spawnOffset)
        {
            nowCombo += (battleManager.combo - nowCombo > 0) ? (short)(battleManager.combo - nowCombo) : (short)0;

//            Debug.Log("battleManager.gameTime:" + battleManager.gameTime + spawnIntervalTime + "spawnIntervalTime:" + spawnIntervalTime + " lastTime:" + lastTime);
            if (nowCombo < normalSpawn)
            {
                // normal spawn
                Spawn("EggMice");
                lastTime = battleManager.gameTime + spawnIntervalTime;
            }
            else
            {
                // spceial spawn
                SpawnSpecial(Random.Range(minMethod, maxMethod), "EggMice", spawnSpeed);
                lastTime = battleManager.gameTime + spawnState.GetIntervalTime();
            }
            SetSpawnIntervalTime();                             // 自動調整間隔時間(依照玩家能力)
        }
    }
}
