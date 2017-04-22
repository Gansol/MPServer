using UnityEngine;

public class ClacExp
{
    float defaultValue = 100;
    float _exp = 0;

    public int Exp { get { return Mathf.RoundToInt(_exp); } }

    /// <summary>
    /// 計算老鼠目前等級最大經驗值
    /// </summary>
    /// <param name="rank"></param>
    public ClacExp(int rank)
    {
        _exp = Mathf.Pow(rank+1, 3) * 0.01f;

        if (rank % 5 == 0)
        {
            _exp += Mathf.Pow(rank+1, 2) * 0.02f;
        }
        else if (rank % 5 == 1)
        {
            _exp -= Mathf.Pow(rank+1, 2) * 0.02f;
        }
        _exp += defaultValue;
    }

    /// <summary>
    /// 取得老鼠目前等級最大經驗值
    /// </summary>
    /// <param name="rank"></param>
    /// <returns>now rank</returns>
    public float ClacMiceExp(float rank)
    {
        return Mathf.Round(rank * rank / 5f) + rank + 1f;
    }
}