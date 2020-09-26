using System;
using UnityEngine;

public static class Clac
{
    static int defaultCost = 17;
    static float defaultValue = 100;
    static float _exp = 0;

    public static int Exp { get { return Mathf.RoundToInt(_exp); } }

    /// <summary>
    /// 計算老鼠目前等級最大經驗值
    /// </summary>
    /// <param name="rank"></param>
    public static int ClacExp(int rank)
    {
        _exp = Mathf.Pow(rank + 1, 3) * 0.01f;

        if (rank % 5 == 0)
        {
            _exp += Mathf.Pow(rank + 1, 2) * 0.02f;
        }
        else if (rank % 5 == 1)
        {
            _exp -= Mathf.Pow(rank + 1, 2) * 0.02f;
        }
        _exp += defaultValue;

        return Convert.ToInt32(_exp);
    }

    /// <summary>
    /// 取得老鼠目前等級最大經驗值
    /// </summary>
    /// <param name="rank"></param>
    /// <returns>now rank</returns>
    public static float ClacMiceExp(float rank)
    {
        return Mathf.Round(rank * rank / 5f) + rank + 1f;
    }

    public static int ClacCost(byte rank) // 亂寫 FUCK
    {
        int cost = defaultCost;                                     // Cost = 初始值
        for (int i = 1; i <= rank; i++)
        {
            cost += (i <= 10) ? 2 : ((i % 5 == 0) ? 2 : 0);         // 小於10等 每等級+2 Cost 5等為+2 Cost
            cost += (i <= 30) ? ((i % 5 == 0) ? 2 : 1) : 0;       // 小於30等 每等級+1 Cost 5等為+2 Cost
            cost += (i % 2 == 0) ? 1 : 0;                                 // 每偶數等級 +1 Cost
        }
        return cost;
    }
}