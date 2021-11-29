using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class ISpawnCommand
{
    public bool spawnFinish;
    [Range(2, 5)]
    protected float _miceSize;
    protected short _miceID;
    protected BattleAIStateAttr _stateAttr;
    protected PoolSystem m_PoolSystem;

    public ISpawnCommand(short miceID, BattleAIStateAttr stateAttr)
    {
        spawnFinish = false;
        _miceSize = 3.5f;
        _miceID = miceID;
        _stateAttr = stateAttr;
        m_PoolSystem = MPGame.Instance.GetPoolSystem();
        // difficult
        // speed
        // count
        // mice id
    }

    public abstract IEnumerator<WaitForSeconds> Spawn();


    #region -- SetDefaultValue --

    /// <summary>
    /// 取得初始位置
    /// </summary>
    /// <param name="arrayLength">陣列長度</param>
    /// <param name="randomPos">隨機位置</param>
    /// <param name="reSpawn">正反產生</param>
    /// <returns></returns>
    protected int SetStartPos(int arrayLength, int randomPos, bool reSpawn)
    {
        if (randomPos < 0 || randomPos >= arrayLength)
            randomPos = (reSpawn) ? arrayLength - 1 : 0;
        return randomPos;
    }

    ///// <summary>
    ///// 如果超過陣列大小 回復初始值
    ///// </summary>
    ///// <param name="holePos">目前位置</param>
    ///// <param name="maxValue">最大值</param>
    ///// <param name="reSpawn">正反產生</param>
    ///// <returns></returns>
    //private int ResetStartPos(int holePos, int maxValue, bool reSpawn)
    //{
    //    if (!reSpawn)
    //    {
    //        return holePos = ((float)holePos / (float)maxValue == 1) ? 0 : holePos; // 如果出生位置=陣列長度 重新(0)開始 避免超出最大值
    //    }
    //    else
    //    {
    //        return holePos = (holePos < 0 || (float)holePos / (float)maxValue == 1) ? maxValue - 1 : holePos; // 如果出生位置=陣列長度 重新(0)開始 避免超出最大值
    //    }
    //}
    #endregion
}

