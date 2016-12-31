using UnityEngine;
using System.Collections;
using MPProtocol;

public class EasyBattleAIState : BattleAIState
{
    int normalScore = 500, normalMaxScore = 1000, normalCombo = 50, normalTime = 120;

    int i = 0;

    public EasyBattleAIState()
    {
        Debug.Log("Now State: EasyBattleAIState");
        normalSpawn = 3;    //25 50 50 75
        spawnCount = 6;
        lerpTime = 0.035f;
        spawnTime = 0.4f;
        intervalTime = 1f;
        minStatus = 0;
        maxStatus = 1;
        minMethod = 0;
        maxMethod = System.Enum.GetNames(typeof(ENUM_SpawnMethod)).Length / 3;
        minSpawnInterval = -1;
        maxSpawnInterval = 1;
        lastTime = spawnIntervalTime = 1.5f;
        spawnSpeed = 1.2f;
    }



    public override void UpdateState()
    {
        if ((battleManager.score > normalScore && battleManager.combo > normalCombo) || battleManager.score > normalMaxScore || battleManager.gameTime > Global.GameTime / 4)
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
