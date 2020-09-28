using UnityEngine;
using System.Collections;

public class TutorialPanel : IMPPanelUI {

    public TutorialPanel(MPGame MPGame) : base(MPGame) { }


    void OnEnable()
    {
      //  m_MPGame.StartCoroutine(Resume());
    }

    // Update is called once per frame
    public override void Update()
    {
    }

    //private IEnumerator Resume()
    //{
    //    EventMaskSwitch.lastPanel = m_RootUI;
    //    return null;
    //}

    public override void Initinal()
    {
        throw new System.NotImplementedException();
    }

    public override void Release()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnLoading()
    {
        m_RootUI.SetActive(true);
    EventMaskSwitch.lastPanel = m_RootUI;
}

    protected override void OnLoadPanel()
    {
        throw new System.NotImplementedException();
    }

    protected override void GetMustLoadAsset()
    {
        throw new System.NotImplementedException();
    }

    protected override int GetMustLoadedDataCount()
    {
        throw new System.NotImplementedException();
    }

public override void OnClosed(GameObject obj)
{
    EventMaskSwitch.lastPanel = null;
        m_MPGame.ShowPanel(m_RootUI);
 //   GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().LoadPanel(obj.transform.parent.gameObject);
}

}

