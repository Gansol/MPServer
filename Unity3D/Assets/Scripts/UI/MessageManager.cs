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
        Global.ShowMessageEvent += OnMessage;
        messageBox = messagePanel.transform.GetChild(0).gameObject;
    }

    void OnMessage(string message)
    {
        messagePanel.SetActive(true);
        messageBox.GetComponentInChildren<UILabel>().text = message;

        EventMaskSwitch.Switch(messagePanel,true);
    }

    public bool OnClosed(){
        EventMaskSwitch.openedPanel.SetActive(false);
        EventMaskSwitch.PrevToFirst();
        return true;
        //EventMaskSwitch.Prev();
    }
}
