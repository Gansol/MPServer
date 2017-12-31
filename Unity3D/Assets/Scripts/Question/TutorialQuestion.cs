using UnityEngine;
using System.Collections;

public class TutorialQuestion : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

    void OnEnable()
    {
        StartCoroutine(Resume());
    }
	// Update is called once per frame
	void Update () {
	
	}

    public  void OnClosed(GameObject obj)
    {
        EventMaskSwitch.lastPanel = null;
        GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().LoadPanel(obj.transform.parent.gameObject);
    }

    protected  void OnLoading()
    {
        transform.parent.gameObject.SetActive(true);
        EventMaskSwitch.lastPanel = gameObject;
    }

    private IEnumerator Resume()
    {
        yield return new WaitForSeconds(0.5f);
        EventMaskSwitch.Resume();
        GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().Panel[5].SetActive(false);
        EventMaskSwitch.Switch(gameObject/*, false*/);
        EventMaskSwitch.lastPanel = gameObject;
    }

}
