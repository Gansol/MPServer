using UnityEngine;
using System.Collections;

public class TapBorad : MonoBehaviour
{

    private int clickTime, maxTimes;

    void Start()
    {

    }

    public void OnClick()
    {
        clickTime++;
        Debug.Log("Click :" + clickTime + "  MaxTimes:"+ maxTimes);
        if (clickTime >= maxTimes / 2 && clickTime != 0)
            Play("Effect2");

        if (clickTime == maxTimes && clickTime != 0)
            Play("Effect3");
    }

    public void SetTimes(int value)
    {
        maxTimes = value;
    }

    public int GetTimes()
    {
        return clickTime;
    }

    private void Play(string animHash)
    {
        gameObject.GetComponent<Animator>().Play(animHash);
    }
}
