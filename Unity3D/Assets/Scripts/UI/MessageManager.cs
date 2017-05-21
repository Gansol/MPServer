using UnityEngine;
using System.Collections;

public class MessageManager : MonoBehaviour
{

    public Transform messagePanel;
    private GameObject _msgBox, _chkBtn, _lastMsgBox;
    private string friend;
    private static bool bFirstLoad = true;

    // Use this for initialization
    void Awake()
    {
        if (bFirstLoad)
        {
            Global.photonService.ShowMessageEvent += OnMessage;
            Global.photonService.InviteFriendEvent += OnInviteFriend;
            Global.photonService.InviteMatchGameEvent += OnInviteMatchGame;
            Global.ExitGameEvent += OnExitGame;
            Global.ShowMessageEvent += OnMessage;
            bFirstLoad = false;
        }
    }

    void OnMessage(string message, Global.MessageBoxType messageBoxType)
    {
        try
        {
            if (!Global.isGameStart && !Global.isMatching)
            {
                if (_lastMsgBox != null) _lastMsgBox.SetActive(false);

                messagePanel.gameObject.SetActive(true);

                switch (messageBoxType)
                {
                    case Global.MessageBoxType.NonChkBtn:
                        _msgBox = messagePanel.Find("MsgBox_NonChk").gameObject;
                        _msgBox.SetActive(true);
                        messagePanel.Find("MsgBox_NonChk").transform.Find("Message").GetComponent<UILabel>().text = message;
                        break;
                    case Global.MessageBoxType.YesNo:
                        _msgBox = messagePanel.Find("MsgBox_YesNo").gameObject;
                        _chkBtn = _msgBox.transform.Find("OK_Btn").gameObject;
                        _msgBox.SetActive(true);
                        messagePanel.Find("MsgBox_YesNo").transform.Find("Message").GetComponent<UILabel>().text = message;
                        break;
                    //case Global.MessageBoxType.InviteFriend:
                    //    break;
                    default:
                        _msgBox = messagePanel.Find("MsgBox_Yes").gameObject;
                        _chkBtn = _msgBox.transform.Find("OK_Btn").gameObject;
                        _msgBox.SetActive(true);
                        messagePanel.Find("MsgBox_Yes").transform.Find("Message").GetComponent<UILabel>().text = message;
                        break;
                }
                _lastMsgBox = _msgBox;
                EventMaskSwitch.Switch(messagePanel.gameObject, true);
            }
        }
        catch(MissingReferenceException)
        {
            messagePanel = GameObject.Find("Message(Panel)").transform;
        }
    
    }

    public void OnClosed()  //這有錯
    {
        EventMaskSwitch.openedPanel.SetActive(false);
        EventMaskSwitch.PrevToFirst();//這有錯
        //EventMaskSwitch.Prev();
    }

    //當收到好友邀請
    private void OnInviteFriend(string friend)
    {
        this.friend = friend;
        UIEventListener.Get(_chkBtn).onClick = ApplyInviteFriend;
    }

    private void ApplyInviteFriend(GameObject go)
    {
        Global.photonService.ApplyInviteFriend(friend);
        UIEventListener.Get(_chkBtn).onClick = null;
    }

    //當收到好友對戰邀請
    private void OnInviteMatchGame(string friend)
    {
        this.friend = friend;
        if (!Global.isMatching && Global.LoginStatus)
        {
            UIEventListener.Get(_chkBtn).onClick = ApplyMatchGameFriend;
        }
    }

    private void ApplyMatchGameFriend(GameObject go)
    {
        Global.photonService.ApplyMatchGameFriend(friend);
        UIEventListener.Get(_chkBtn).onClick = null;
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
