using UnityEngine;
using System.Collections;

public class PhotonConnect : MonoBehaviour
{
    private InternetChecker internetCheck;
    private string ServerIP = Global.serverIP;
    private int ServerPort = 5055;
    private string ServerName = "MPServer";
    private static bool firstLogin;
    private bool ConnectStatus = true;

    private bool flag ;

    void Awake()
    {
        internetCheck = gameObject.AddMissingComponent< InternetChecker>();
        firstLogin = true;
    }

    // Use this for initialization
    void Start()
    {
        ConnectToServer();
    }

    public void ConnectToServer()
    {
        if (!Global.photonService.ServerConnected && !Global.exitingGame)
        {
            Global.photonService.ConnectEvent += doConnectEvent;
            Global.photonService.Connect(ServerIP, ServerPort, ServerName);
            Global.LoginStatus = false;
        }
    }

    //void OnApplicationPause(bool isPause)
    //{
    //    if (isPause)
    //    {
    //        Debug.Log("isPause: " + isPause);
    //    }
    //}

    void OnApplicationFocus(bool hasFocus)
    {
        if (!ConnectStatus && hasFocus)
        {
            flag = !flag;
        }

        //if (hasFocus)
        //{
        //    Debug.Log("hasFocus: " + hasFocus);
        //}
        
    }


    public void Test()
    {
        flag = false;
        ConnectStatus = false;
    }

    void Update()
    {
        if (!ConnectStatus && !flag)
        {
            flag = !flag;
            Global.photonService.Connect(ServerIP, ServerPort, ServerName);
        }
    }

    // 連線狀態
    private void doConnectEvent(bool Status)
    {
        // 連線成功後若需要做參數初始化放這裡
        if (Status)
        {

            Debug.Log("Connecting . . . . .");
            
            flag = true;
            ConnectStatus = true;
            if(!firstLogin && !Global.exitingGame) Global.photonService.Login(Global.Account, Global.Hash, Global.MemberType);
            firstLogin = false;
        }
        else
        {
            Debug.Log("Connect Fail " + ServerIP);
            flag = false;
            ConnectStatus = false;
        }
    }

    private void OnDestroy()
    {
        Global.photonService.ConnectEvent -= doConnectEvent;
    }

    //void OnGUI()
    //{
    //    if (ConnectStatus == false)
    //    {
    //        GUI.Label(new Rect((Screen.width / 2) - 200, (Screen.height / 2) - 10, 400, 20), "Connect fail");
    //    }
    //}
}
