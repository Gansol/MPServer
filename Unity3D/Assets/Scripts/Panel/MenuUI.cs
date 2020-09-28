using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuUI : IMPPanelUI
{
    private GameObject playerBtn;
    private GameObject teamBtn;
    private GameObject storeBtn;
    private GameObject purchaseBtn;
    private GameObject friendBtn;
    private GameObject settingBtn;
    private GameObject battleBtn;
    private GameObject survivalBtn;

    public MenuUI(MPGame MPGame)
        : base(MPGame)
    {
        m_RootUI = GameObject.Find("MenuUI");
    }

    public override void Initinal()
    {
        playerBtn = m_RootUI.gameObject.transform.Find("Player_Btn").gameObject;
        teamBtn = m_RootUI.gameObject.transform.Find("Team_Btn").gameObject;
        purchaseBtn = m_RootUI.gameObject.transform.Find("Purchase_Btn").gameObject;
        friendBtn = m_RootUI.gameObject.transform.Find("Friend_Btn").gameObject;
        storeBtn = m_RootUI.gameObject.transform.Find("Store_Btn").gameObject;
        settingBtn = m_RootUI.gameObject.transform.Find("Setting_Btn").gameObject;
        battleBtn = m_RootUI.gameObject.transform.Find("Battle_Btn").gameObject;
        survivalBtn = m_RootUI.gameObject.transform.Find("Survival_Btn").gameObject;

        UIEventListener.Get(playerBtn).onClick += m_MPGame.ShowPanel;
        UIEventListener.Get(teamBtn).onClick += m_MPGame.ShowPanel;
        UIEventListener.Get(purchaseBtn).onClick += m_MPGame.ShowPanel;
        UIEventListener.Get(friendBtn).onClick += m_MPGame.ShowPanel;
        UIEventListener.Get(storeBtn).onClick += m_MPGame.ShowPanel;
        UIEventListener.Get(settingBtn).onClick += m_MPGame.ShowPanel;
        UIEventListener.Get(battleBtn).onClick += m_MPGame.ShowPanel;
        UIEventListener.Get(survivalBtn).onClick += m_MPGame.ShowPanel;
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
        UIEventListener.Get(playerBtn).onClick -= m_MPGame.ShowPanel;
        UIEventListener.Get(teamBtn).onClick -= m_MPGame.ShowPanel;
        UIEventListener.Get(purchaseBtn).onClick -= m_MPGame.ShowPanel;
        UIEventListener.Get(friendBtn).onClick -= m_MPGame.ShowPanel;
        UIEventListener.Get(storeBtn).onClick -= m_MPGame.ShowPanel;
        UIEventListener.Get(settingBtn).onClick -= m_MPGame.ShowPanel;
        UIEventListener.Get(battleBtn).onClick -= m_MPGame.ShowPanel;
        UIEventListener.Get(survivalBtn).onClick -= m_MPGame.ShowPanel;
    }
}
