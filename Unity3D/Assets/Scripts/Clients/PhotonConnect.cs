using UnityEngine;
using System.Collections;

public class PhotonConnect : MonoBehaviour
{

    public string ServerIP = "180.218.164.232";
    public int ServerPort = 5055;
    public string ServerName = "MPServer";

    private bool ConnectStatus = true;

    // Use this for initialization
    void Start()
    {
        if (!Global.photonService.ServerConnected)
        {
            Global.photonService.ConnectEvent += doConnectEvent;
            Global.photonService.Connect(ServerIP, ServerPort, ServerName);
            Global.LoginStatus = false;
        }
    }


    // 連線狀態
    private void doConnectEvent(bool Status)
    {
        // 連線成功後若需要做參數初始化放這裡
        if (Status)
        {
            Debug.Log("Connecting . . . . .");
            ConnectStatus = true;
        }
        else
        {
            Debug.Log("Connect Fail");
            ConnectStatus = false;
        }
    }

    private void OnDestroy()
    {
        Global.photonService.ConnectEvent -= doConnectEvent;
    }

    void OnGUI()
    {
        if (ConnectStatus == false)
        {
            GUI.Label(new Rect((Screen.width / 2) - 200, (Screen.height / 2) - 10, 400, 20), "Connect fail");
        }
    }
}
