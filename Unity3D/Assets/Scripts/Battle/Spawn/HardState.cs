using UnityEngine;
using System.Collections;
using MPProtocol;

public class HardState : SpawnState
{
    public void Initialize()
    {
        spawnCount = 12;
        lerpTime = 0.06f;
        spawnTime = 0.45f;
        intervalTime = 3f;
        minStatus = 1;
        maxStatus = 3;
    }

    /// <summary>
    /// 產生老鼠
    /// </summary>
    /// <param name="miceName">老鼠名稱</param>
    /// <param name="isSkill">市府為技能鼠</param>
    /// <returns>routine</returns>
    public override Coroutine Spawn(string miceName, bool isSkill)
    {
        Random.seed = unchecked((int)System.DateTime.Now.Ticks);

        spawnStatus = SelectStatus(Random.Range(minStatus, maxStatus+1));
        bool reSpawn = System.Convert.ToBoolean(Random.Range(0, 1 + 1));

        Initialize();
        coroutine = SpawnCustom(spawnStatus, miceName, this.spawnTime, this.intervalTime, this.lerpTime, this.spawnCount, isSkill, reSpawn);
        return coroutine;
    }
}
