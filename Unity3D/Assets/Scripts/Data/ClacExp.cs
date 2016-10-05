using UnityEngine;

public class ClacExp
{
    float defaultValue = 100;
    float _exp = 0;

    public int exp { get { return Mathf.RoundToInt(_exp); } }

    /// <summary>
    /// 計算經驗值
    /// </summary>
    /// <param name="level">等級</param>
    public ClacExp(int level)
    {
        _exp = Mathf.Pow(level + 1, 3) * 0.01f;

        if (level % 5 == 0)
        {
            _exp += Mathf.Pow(level + 1, 2) * 0.02f;
        }
        else if (level % 5 == 1)
        {
            _exp -= Mathf.Pow(level + 1, 2) * 0.02f;
        }
        _exp += defaultValue;
    }
}