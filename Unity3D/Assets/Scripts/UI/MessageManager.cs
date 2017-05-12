using UnityEngine;
using System.Collections;

public class MessageManager : MonoBehaviour
{

    public GameObject messagePanel;
    private GameObject messageBox;
    // Use this for initialization
    void Start()
    {
        Global.photonService.ShowMessageEvent += OnMessage;
        Global.photonService.InviteMatchGameEvent += OnInviteMatchGame;
        Global.ShowMessageEvent += OnMessage;
        messageBox = messagePanel.transform.GetChild(0).gameObject;
        Debug.Log(messageBox);
    }

    void OnMessage(string message)
    {
        messagePanel.SetActive(true);
        if (messagePanel.activeSelf)
        {
            Debug.Log(messageBox.transform.Find("Message"));
            messageBox.transform.Find("Message").GetComponent<UILabel>().text = message;
        }
        EventMaskSwitch.Switch(messagePanel, true);
    }

    public void OnClosed()
    {
        EventMaskSwitch.openedPanel.SetActive(false);
        EventMaskSwitch.PrevToFirst();
        //EventMaskSwitch.Prev();
    }

    private void OnInviteMatchGame()
    {
        if (!Global.isGameStart)
        {
            UIEventListener.Get(messageBox).onClick = MatchGameFriend;
        }
    }

    private void MatchGameFriend(GameObject go)
    {
        UIEventListener.Get(messageBox).onClick = null;
    }
}
