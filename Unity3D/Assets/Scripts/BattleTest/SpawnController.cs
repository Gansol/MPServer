using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 是否觸發Spawn的控制器，儲存Spawn佇列
/// </summary>
public class SpawnController : IGameSystem
{
    private List<ISpawnCommand> _listSpawnQueue;
    private bool flag;

    public SpawnController(MPGame MPGame) : base(MPGame)
    {
        _listSpawnQueue = new List<ISpawnCommand>();
    }

    public override void Update() 
    {
        // if > Extecutor Time
        // if hole count >= spawnCount
        // 這裡還沒寫好 一Update就沒有東西了 陣列是空的因為還沒Spawn (IBattleAIState)
        // 時間間隔
        // 優先SPAWN
        // 中斷 SPAWN
        // if command spawn finish remove Queue

        if (_listSpawnQueue.Count > 0)
        {
            if (_listSpawnQueue[0].spawnFinish)
            {
                _listSpawnQueue.RemoveAt(0);
            }
        }
    }

    // IEnumerator class
    // save Coroutine list
    // Extecutor Coroutine


    public void AddCoroutine(ISpawnCommand coroutine)
    {
        _listSpawnQueue.Add(coroutine);
    }

    private void RemoveLastCoroutine()
    {
        _listSpawnQueue.RemoveAt(_listSpawnQueue.Count - 1);
    }

    public void Extecutor()
    {
        //  MPGame.Instance.StartCoroutine(enumerator);

        MPGame.Instance.StartCoroutine(_listSpawnQueue[0].Spawn());
    }

}
