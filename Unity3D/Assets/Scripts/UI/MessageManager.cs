using UnityEngine;
using System.Collections;

public class MessageManager : MonoBehaviour
{

    public Transform messagePanel;
    private GameObject _msgBox, _chkBtn, _lastMsgBox;
    private string friend;

    // Use this for initialization
    void Start()
    {
        Global.photonService.ShowMessageEvent += OnMessage;
        Global.photonService.InviteFriendEvent += OnInviteFriend;
        Global.photonService.InviteMatchGameEvent += OnInviteMatchGame;
        Global.ShowMessageEvent += OnMessage;

    }

    void OnMessage(string message, Global.MessageBoxType messageBoxType)
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

    public void OnClosed()  //這有錯
    {
        EventMaskSwitch.openedPanel.SetActive(false);
        EventMaskSwitch.PrevToFirst();//這有錯
        //EventMaskSwitch.Prev();
    }

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

    private void OnInviteMatchGame()
    {
        if (!Global.isGameStart)
            UIEventListener.Get(_chkBtn).onClick = MatchGameFriend;
    }

    private void MatchGameFriend(GameObject go)
    {
        Global.photonService.MatchGameFriend();
        UIEventListener.Get(_chkBtn).onClick = null;
    }
}
