using UnityEngine;
using System.Collections;

public class TeamSlotChk : MonoBehaviour
{
    public byte level;
    public bool reverse;
    private bool flag;
    // Use this for initialization
    void Awake()
    {
        Global.photonService.LoadPlayerItemEvent += Start;
        if (reverse) flag = !flag;  // 相反確認
    }

    void Start()
    {
        if (this != null)
            if (transform.parent.gameObject.activeSelf)
            {
                if (Global.Rank >= level)
                    gameObject.SetActive(!flag);
                else
                    gameObject.SetActive(flag);
            }
    }

    void OnDestory()
    {
        Global.photonService.LoadPlayerItemEvent -= Start;
    }
}
