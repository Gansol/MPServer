using UnityEngine;
using System.Collections;

public class TutorialQuestion : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnClosed(GameObject obj)
    {
        EventMaskSwitch.lastPanel = null;
        GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().LoadPanel(obj.transform.parent.gameObject);
    }

    private void OnLoading()
    {
        transform.parent.gameObject.SetActive(true);
    }

}
