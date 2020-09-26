using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BtnAttachMenuUI : MonoBehaviour
{
    public GameObject[] MenuBtn;
    MPGame m_MPGame;

    private void Awake()
    {
        foreach (GameObject go in MenuBtn)
        {
            UIEventListener.Get(go).onClick += m_MPGame.ShowPanel;
        }
    }
}
