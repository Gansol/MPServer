using UnityEngine;
using System.Collections;
using MPProtocol;

public class NormalBattleAIState : BattleAIState
{
    int hardScore = 3000, hardMaxScore = 5000, hardCombo = 75, normalMaxScore = 1000, normalCombo = 50;

    public NormalBattleAIState()
    {
        Debug.Log("Now State: NormalBattleAIState");
        battleAIState = ENUM_BattleAIState.NormalMode;
        normalSpawn = 50;
        spawnCount = 12;
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
        wave = 0;
        nextBali = 4; nextMuch = 20; nextHero = 47;

        pervStateTime = Global.GameTime / 4;
        nextStateTime = Global.GameTime / 2;
    }

    public override void UpdateState()
    {
        if ((battleManager.score > hardScore && battleManager.combo > hardCombo) || battleManager.score > hardMaxScore || BattleManager.gameTime > nextStateTime)
        {
            battleManager.SetSpawnState(new HardBattleAIState());
        }
        else if (battleManager.combo < normalCombo && battleManager.score < normalMaxScore && BattleManager.gameTime < pervStateTime)
        {
            battleManager.SetSpawnState(new EasyBattleAIState());
        }
        else if (BattleManager.gameTime > lastTime + spawnOffset)
        {
            nowCombo += (battleManager.combo - nowCombo > 0) ? (short)(battleManager.combo - nowCombo) : (short)0;

            //            Debug.Log("BattleManager.gameTime:" + BattleManager.gameTime + spawnIntervalTime + "spawnIntervalTime:" + spawnIntervalTime + " lastTime:" + lastTime);
            if (nowCombo < normalSpawn)
            {
                // normal spawn
                Spawn(defaultMice, spawnCount);   // 錯誤
                lastTime = BattleManager.gameTime + spawnIntervalTime * 3;
            }
            else
            {
                // spceial spawn
                SpawnSpecial(Random.Range(minMethod, maxMethod), defaultMice, spawnSpeed, spawnCount);    //錯誤
                lastTime = BattleManager.gameTime + spawnState.GetIntervalTime();
            }
            SetSpawnIntervalTime();                             // 自動調整間隔時間(依照玩家能力)
        }
    }
}
