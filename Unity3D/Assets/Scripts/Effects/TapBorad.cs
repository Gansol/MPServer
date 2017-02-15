using UnityEngine;
using System.Collections;

public class TapBorad : MonoBehaviour
{

    private int clickTime, maxTimes;

    void OnClick()
    {
        if (clickTime >= maxTimes / 2 && clickTime != 0)
            Play("Effect2");

        if (clickTime == maxTimes && clickTime != 0)
            Play("Effect3");
    }

    public void SetTimes(int value)
    {
        maxTimes = value;
    }

    private void Play(string animHash)
    {
        gameObject.GetComponent<Animator>().Play(animHash);
    }
}
