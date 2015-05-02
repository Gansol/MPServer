using UnityEngine;
using System.Collections;

public class BattleUI : MonoBehaviour
{
    float ckeckTime;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {//＊＊＊＊＊遊戲性測試時砍掉＊＊＊＊＊＊
        
        if (ckeckTime > 15)
        {
            Global.photonService.CheckStatus();
            ckeckTime = 0;
        }
        else
        {
            ckeckTime += Time.deltaTime;
        }
         
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 100), "ExitRoom"))
        {
            Global.photonService.KickOther();
            Global.photonService.ExitRoom();
        }

    }
}
