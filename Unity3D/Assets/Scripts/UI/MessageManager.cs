using UnityEngine;
using System.Collections.Generic;

/* ***************************************************************
 * -----Copyright c 2018 Gansol Studio.  All Rights Reserved.-----
 * -----------            CC BY-NC-SA 4.0            -------------
 * -----------  @Website:  EasyUnity@blogspot.com    -------------
 * -----------  @Email:    GansolTW@gmail.com        -------------
 * -----------  @Author:   Krola.                    -------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * 負責 所有訊息彈出顯示
 * 
 * Crash 還沒寫好
 * ***************************************************************
 *                           ChangeLog           
 * 20171225 v1.1.2   修正階層問題                         
 * 20171224 v1.1.1   修正close按鈕問題 新增註解                                          
 * ****************************************************************/
public class MessageManager : GameSystem
{
    private static Transform messagePanel = null;                    // Hierarchy Message Panel
    private GameObject _msgBox, _lastMsgBox, _chkBtn, _clsBtn;        // 訊息視窗、上個訊息視窗 、確認按鈕、關閉按鈕  
    private Dictionary<string, GameObject> _dictMsgBox;               // 訊息視窗列表
    private string _inviteFriendName;                               // 邀請遊戲的朋友暱稱
    private static bool bFirstLoad = true;                          // 是否第一次載入
    private int _prevMask = 0;

    // 初始化 加入事件
    public MessageManager(MPGame MPGame)
        : base(MPGame)
    {
        if (bFirstLoad)
        {
            Global.photonService.InviteFriendEvent += OnInviteFriend;
            Global.photonService.InviteMatchGameEvent += OnInviteMatchGame;
            Global.ExitGameEvent += OnExitGame;
            Global.ShowMessageEvent += OnMessage;
            _dictMsgBox = new Dictionary<string, GameObject>();
            bFirstLoad = false;
        }
    }

    /// <summary>
    /// 收到訊息時
    /// </summary>
    /// <param name="message">訊息</param>
    /// <param name="MessageBoxType">訊息視窗型態</param>
    void OnMessage(string message, string MessageBoxType, int prevMask)
    {
        try
        {
            _prevMask = prevMask;
            // 不再遊戲時 且 不再配對中 或 系統炸了 顯示消息視窗
            if ((!Global.isGameStart && !Global.isMatching) || Global.MessageBoxType.SystemCrash == MessageBoxType)
            {
                // 如果存在舊的訊息視窗 關閉
                if (_lastMsgBox != null) _lastMsgBox.SetActive(false);

                // 獲取messagePanel並開啟
                if (messagePanel == null)
                    messagePanel = GameObject.Find("Message(Panel)").transform;
                messagePanel.gameObject.SetActive(true);

                // 加入訊息視窗暫存
                if (!_dictMsgBox.ContainsKey(MessageBoxType))
                {
                    _msgBox = messagePanel.Find(MessageBoxType).gameObject;
                    _dictMsgBox.Add(MessageBoxType, _msgBox);
                }

                // 尋找訊息視窗對應按鈕
                if (_dictMsgBox[MessageBoxType].transform.Find("OK_Btn"))
                    _chkBtn = _dictMsgBox[MessageBoxType].transform.Find("OK_Btn").gameObject;
                if (_dictMsgBox[MessageBoxType].transform.Find("Close_Btn"))
                    _clsBtn = _dictMsgBox[MessageBoxType].transform.Find("Close_Btn").gameObject;

                // 加入按鈕監聽事件
                if (_chkBtn != null)
                    UIEventListener.Get(_chkBtn).onClick = OnClosed;
                if (_clsBtn != null)
                    UIEventListener.Get(_clsBtn).onClick = OnClosed;
                //if (Global.MessageBoxType.SystemCrash == MessageBoxType)
                //    UIEventListener.Get(_chkBtn).onClick = ExitGame;

                _lastMsgBox = _dictMsgBox[MessageBoxType];
                _dictMsgBox[MessageBoxType].SetActive(true);
                messagePanel.Find(MessageBoxType).transform.Find("Message").GetComponent<UILabel>().text = message;
                EventMaskSwitch.Switch(messagePanel.gameObject);
            }
        }
        catch (MissingReferenceException)
        {
            messagePanel = GameObject.Find("Message(Panel)").transform;
        }

    }

    // 關閉視窗
    public void OnClosed(GameObject go)
    {
        go.transform.parent.gameObject.SetActive(false);
        if (_prevMask > 0)
            EventMaskSwitch.Prev(_prevMask);
        else
            EventMaskSwitch.PrevToFirst();
    }

    //當收到好友邀請
    private void OnInviteFriend(string friend)
    {
        this._inviteFriendName = friend;
        UIEventListener.Get(_chkBtn).onClick = ApplyInviteFriend;
    }

    // 邀請好友
    private void ApplyInviteFriend(GameObject go)
    {
        Global.photonService.ApplyInviteFriend(_inviteFriendName);
        OnClosed(go);
    }

    //當收到好友對戰邀請
    private void OnInviteMatchGame(string friend)
    {
        this._inviteFriendName = friend;
        if (!Global.isMatching && Global.LoginStatus)
        {
            UIEventListener.Get(_chkBtn).onClick = ApplyMatchGameFriend;
        }
    }

    // 同意好友配對遊戲
    private void ApplyMatchGameFriend(GameObject go)
    {
        Global.photonService.ApplyMatchGameFriend(_inviteFriendName);
        UIEventListener.Get(_chkBtn).onClick = null;
        OnClosed(go);
    }

    // 當收到離開遊戲訊息 加入離開事件
    private void OnExitGame()
    {
        UIEventListener.Get(_chkBtn).onClick = ExitGame;
    }

    private void ExitGame(GameObject go)
    {
        Global.photonService.Disconnect();
        Application.Quit();
    }
}
