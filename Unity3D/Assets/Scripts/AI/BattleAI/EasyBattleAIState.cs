using UnityEngine;
using System.Collections;
using MPProtocol;

public class EasyBattleAIState : BattleAIState
{
    int normalScore = 500, normalMaxScore = 1000, normalCombo = 50;

    public EasyBattleAIState()
    {
        Debug.Log("Now State: EasyBattleAIState");
        battleAIState = ENUM_BattleAIState.EasyMode;
        normalSpawn = 25;    //25 50 50 75
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
        wave = 0;
        nextBali = 4; nextMuch = 27; nextHero = 50;

        pervStateTime = 0;
        nextStateTime = Global.GameTime / 4;
    }



    public override void UpdateState()
    {
        if ((battleManager.score > normalScore && battleManager.combo > normalCombo) || battleManager.score > normalMaxScore || BattleManager.gameTime > nextStateTime)
        {
            battleManager.SetSpawnState(new NormalBattleAIState());
        }
        else if (BattleManager.gameTime > lastTime + spawnOffset)
        {
            nowCombo += (battleManager.combo - nowCombo > 0) ? (short)(battleManager.combo - nowCombo) : (short)0;

            if (nowCombo < normalSpawn)
            {
                // normal spawn
                Spawn(defaultMice, spawnCount);//錯誤
                lastTime = BattleManager.gameTime + spawnIntervalTime * 3;
            }
            else
            {
                // spceial spawn
                SpawnSpecial(Random.Range(minMethod, maxMethod), defaultMice, spawnSpeed, spawnCount);//錯誤
                lastTime = BattleManager.gameTime + spawnState.GetIntervalTime();
            }

            SetSpawnIntervalTime();                             // 自動調整間隔時間(依照玩家能力)
        }
    }
}
