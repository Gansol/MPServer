using UnityEngine;
using System.Collections;
using MPProtocol;

public class NormalBattleAIState : IBattleAIState
{
    int hardScore = 3000, hardMaxScore = 5000, hardCombo = 75, normalMaxScore = 1000, normalCombo = 50;

    public NormalBattleAIState( BattleAttr battleAttr)
        : base( battleAttr)
    {
        Debug.Log("Now State: NormalBattleAIState");
        stateAttr = new BattleAIStateAttr();
        stateAttr.battleAIState = ENUM_BattleAIState.NormalMode;
        stateAttr.normalSpawn = 50;
        stateAttr.spawnCount = 12;
        stateAttr.lerpTime = 0.065f;
        stateAttr.spawnTime = 0.4f;
        stateAttr.intervalTime = 1.8f;
        stateAttr.minStatus = 1;
        stateAttr.maxStatus = 2;
        stateAttr.minMethod = 0;
        stateAttr.maxMethod = System.Enum.GetNames(typeof(ENUM_SpawnMethod)).Length / 2;
        stateAttr.minSpawnInterval = -1.5f;
        stateAttr.maxSpawnInterval = 2;
        stateAttr.spawnIntervalTime = 1.5f;
        stateAttr.spawnSpeed = 1.1f;
        stateAttr.wave = 0;
        stateAttr.nextBali = 4; stateAttr.nextMuch = 20; stateAttr.nextHero = 47;

        stateAttr.pervStateTime = Global.GameTime / 4;
        stateAttr.nextStateTime = Global.GameTime / 2;
    }

    public override void UpdateState()
    {
        if ((battleAttr.score > hardScore && battleAttr.combo > hardCombo) || battleAttr.score > hardMaxScore || battleAttr.gameTime > stateAttr.nextStateTime)
        {
         MPGame.Instance.GetBattleSystem().SetSpawnState(new HardBattleAIState( battleAttr));
        }
        else if (battleAttr.combo < normalCombo && battleAttr.score < normalMaxScore && battleAttr.gameTime < stateAttr.pervStateTime)
        {
            MPGame.Instance.GetBattleSystem().SetSpawnState(new EasyBattleAIState( battleAttr));
        }
        else if (battleAttr.gameTime > stateAttr.lastTime + stateAttr.spawnOffset)
        {
            stateAttr.nowCombo += (battleAttr.combo - stateAttr.nowCombo > 0) ? (short)(battleAttr.combo - stateAttr.nowCombo) : (short)0;

            //            Debug.Log("battleAttr.gameTime:" + battleAttr.gameTime + spawnIntervalTime + "spawnIntervalTime:" + spawnIntervalTime + " lastTime:" + lastTime);
            if (stateAttr.nowCombo < stateAttr.normalSpawn)
            {
                // normal spawn
                Spawn(stateAttr.defaultMice, stateAttr);   // 錯誤
                stateAttr.lastTime = battleAttr.gameTime + stateAttr.spawnIntervalTime * 3;
            }
            else
            {
                // spceial spawn
                SpawnSpecial( stateAttr.defaultMice, stateAttr);    //錯誤
                stateAttr.lastTime = battleAttr.gameTime + stateAttr.spawnState.GetIntervalTime();
            }
            SetSpawnIntervalTime();                             // 自動調整間隔時間(依照玩家能力)
        }
    }
}
