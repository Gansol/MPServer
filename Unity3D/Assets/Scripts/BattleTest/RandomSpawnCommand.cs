using MPProtocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RandomSpawnCommand : ISpawnCommand
{

    public RandomSpawnCommand(short miceID, BattleAIStateAttr stateAttr) : base(miceID, stateAttr)
    {

    }

    public override IEnumerator<WaitForSeconds> Spawn()
    {
        //        Debug.Log("Random spawnCount:" + spawnCount);
        sbyte[] holeArray = SpawnData.GetSpawnData(SpawnStatus.Random) as sbyte[];
        sbyte[] rndHoleArray = new sbyte[_stateAttr.spawnCount];       // 隨機陣列
        List<sbyte> listHole = new List<sbyte>(holeArray);

        int holePos = 0, count = 0;

        Random.InitState(unchecked((int)System.DateTime.Now.Ticks));
        bool reSpawn = System.Convert.ToBoolean(Random.Range(0, 1 + 1));

        // 產生隨機值陣列
        for (holePos = 0; holePos < _stateAttr.spawnCount; holePos++)
        {
            int rndNum = UnityEngine.Random.Range(0, listHole.Count);
            rndHoleArray[holePos] = holeArray[rndNum];
            listHole.RemoveAt(rndNum);
        }

        //產生老鼠
        for (holePos = 0; count < _stateAttr.spawnCount; holePos++)
        {
            holePos = SetStartPos(holeArray.Length, holePos, false);
            m_PoolSystem.ActiveMice(_miceID, _miceSize, MPGame.Instance.GetBattleSystem().GetBattleAttr().hole[rndHoleArray[holePos]].transform, reSpawn);
            count++;
            yield return new WaitForSeconds(_stateAttr.spawnTime);
        }

        yield return new WaitForSeconds(_stateAttr.intervalTime);
        spawnFinish = true;
        Global.spawnFlag = true;
    }
}



