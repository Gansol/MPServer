using UnityEngine;
using System.Collections;

public class Question : MonoBehaviour {

    int i; //244 203 140
    public GameObject[] setp;

    void Start()
    {
        i = 0;
    }

    public void OnQusetClick()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        EventMaskSwitch.Switch(transform.GetChild(0).gameObject/*, true*/);
    }


    public void OnSetpClick()
    {
        if (i < setp.Length -1)
        {
            setp[i].SetActive(false);
            setp[i + 1].SetActive(true);
            i++;
        }
        else
        {
            setp[0].SetActive(true);
            setp[i].SetActive(false);
            i = 0;
            transform.GetChild(0).gameObject.SetActive(false);
            EventMaskSwitch.Prev(1);
        }
    }


    public void OnClosed(GameObject go)
    {
        EventMaskSwitch.lastPanel = null;
        //GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().LoadPanel(go.transform.parent.gameObject);
        i = 0;
    }
}
