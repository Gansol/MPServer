using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuUI : IMPPanelUI
{
    private AttachBtn_MenuUI UI;

    public MenuUI(MPGame MPGame)
        : base(MPGame)
    {
       
    }

    public override void Initinal()
    {
        Debug.Log("MenUI Init!");
        m_RootUI = GameObject.Find(Global.Scene.MainGameAsset.ToString());
        EventMaskSwitch.Init();
        UI = m_RootUI.GetComponentInChildren<AttachBtn_MenuUI>();
        UIEventListener.Get(UI.playerBtn).onClick = m_MPGame.ShowPanel;
        UIEventListener.Get(UI.teamBtn).onClick = m_MPGame.ShowPanel;
        UIEventListener.Get(UI.purchaseBtn).onClick = m_MPGame.ShowPanel;
        UIEventListener.Get(UI.friendBtn).onClick = m_MPGame.ShowPanel;
        UIEventListener.Get(UI.storeBtn).onClick = m_MPGame.ShowPanel;
        UIEventListener.Get(UI.settingBtn).onClick = m_MPGame.ShowPanel;
        UIEventListener.Get(UI.battleBtn).onClick = m_MPGame.ShowPanel;
        UIEventListener.Get(UI.survivalBtn).onClick = m_MPGame.ShowPanel;
    }

    public override void Update()
    {
       
    }





    protected override void GetMustLoadAsset()
    {
        throw new System.NotImplementedException();
    }

    protected override int GetMustLoadedDataCount()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnLoading()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnLoadPanel()
    {
        throw new System.NotImplementedException();
    }

    public override void OnClosed(GameObject obj)
    {
        throw new System.NotImplementedException();
    }

    public override void Release()
    {
        //UIEventListener.Get(UI.playerBtn).onClick -= m_MPGame.ShowPanel;
        //UIEventListener.Get(UI.teamBtn).onClick -= m_MPGame.ShowPanel;
        //UIEventListener.Get(UI.purchaseBtn).onClick -= m_MPGame.ShowPanel;
        //UIEventListener.Get(UI.friendBtn).onClick -= m_MPGame.ShowPanel;
        //UIEventListener.Get(UI.storeBtn).onClick -= m_MPGame.ShowPanel;
        //UIEventListener.Get(UI.settingBtn).onClick -= m_MPGame.ShowPanel;
        //UIEventListener.Get(UI.battleBtn).onClick -= m_MPGame.ShowPanel;
        //UIEventListener.Get(UI.survivalBtn).onClick -= m_MPGame.ShowPanel;
    }
}
