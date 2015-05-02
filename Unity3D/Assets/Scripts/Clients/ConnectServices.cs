using UnityEngine;
using System.Collections;

public class ConnectServices : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        // 呼叫Service()
        Global.photonService.Service();
    }

    void OnApplicationQuit()
    {
        // 若玩家關閉遊戲時呼叫斷線，若沒呼叫就必須等到Timeout才會斷線

       // StartCoroutine("ExitGame"); // ＊＊＊＊＊　這還要測試看看有沒有用　＊＊＊＊＊
        Global.photonService.Disconnect();
    }

    IEnumerable ExitGame()
    {
        if (Global.BattleStatus)
        {
            Global.photonService.KickOther();
            Global.photonService.ExitRoom();
        }

        yield return new WaitForSeconds(5);
    }
}
