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
        messageBox = messagePanel.transform.GetChild(0).gameObject;
    }

    void OnMessage()
    {
        Debug.Log("HERER");
        Debug.Log(messageBox.name);
        messagePanel.SetActive(true);
        messageBox.GetComponentInChildren<UILabel>().text = "Complete !";

        EventMaskSwitch.Switch(messagePanel);
    }

    public void  OnClosed(){
        EventMaskSwitch.openedPanel.SetActive(false);
        EventMaskSwitch.Switch(EventMaskSwitch.lastPanel);
    }
}
