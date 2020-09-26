using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuUI : IMPPanelUI
{
    private GameObject[] Panel;
    public MenuUI(MPGame MPGame)
        : base(MPGame)
    {
        //EventDelegate.Set(clone.GetComponent<UIButton>().onClick, clone.GetComponent<ItemBtn>().OnClick);
        m_RootUI = GameObject.Find("MenuUI");
}



















    public override void OnClosed(GameObject obj)
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

    protected override void OnLoading()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnLoadPanel()
    {
        throw new System.NotImplementedException();
    }
}
