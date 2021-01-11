using UnityEngine;
using System.Collections;
/* ***************************************************************
 * -----Copyright © 2015 Gansol Studio.  All Rights Reserved.-----
 * -----------            CC BY-NC-SA 4.0            -------------
 * -----------  @Website:  EasyUnity@blogspot.com    -------------
 * -----------  @Email:    GansolTW@gmail.com        -------------
 * -----------  @Author:   Krola.                    -------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * 負責 負責所有法幣商品交易的 所有處理
 * 
 * 
 * ***************************************************************
 *                           ChangeLog
 * 20201027 v3.0.0  繼承重構    
 * ****************************************************************/
public class TutorialUI : IMPPanelUI
{

    private AttachBtn_TutorialUI UI;

    public TutorialUI(MPGame MPGame) : base(MPGame)
    {
        Debug.Log("--------------- TutorialUI Create ----------------");
    }
    public override void Initialize()
    {
        Debug.Log("--------------- TutorialUI Initialize ----------------");
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }



    public override void ShowPanel(string panelName)
    {
        m_RootUI = GameObject.Find(Global.Scene.MainGameAsset.ToString()).GetComponentInChildren<AttachBtn_MenuUI>().tutorialPanel;
        base.ShowPanel(panelName);
    }

    protected override void OnLoading()
    {
        UI = m_RootUI.GetComponentInChildren<AttachBtn_TutorialUI>();
        UIEventListener.Get(UI.okBtn).onClick = OnClosed;
        UIEventListener.Get(UI.closeCollider).onClick = OnClosed;

        m_RootUI.SetActive(true);
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

    public override void OnClosed(GameObject go)
    {
        EventMaskSwitch.LastPanel = null;
        ShowPanel(m_RootUI.name);
        //   GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().LoadPanel(go.transform.parent.gameObject);
    }
    public override void Release()
    {

    }
}

