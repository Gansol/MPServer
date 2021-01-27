using UnityEngine;
using MPProtocol;
using System;
using System.Collections.Generic;

public class AttachBtn_BattleUI : MonoBehaviour
{
    public UILabel playerName;
    public UILabel otherPlayerName;
    public UISprite avatarImage, otherAvatarImage, gameMode;
    public UISlider HPBar;
    public UILabel ComboLabel;
    public UILabel BlueScoreLabel;
    public UILabel RedScoreLabel;
    public UISlider BlueEnergyBar;
    public UISlider RedEnergyBar;
    public UISlider EnergyBar;
    public UISlider FeverBar;
    public UILabel BlueLifeText;
    public UILabel RedLifeText;
    public UISlider BlueLifeBar;
    public UISlider RedLifeBar;
    public GameObject Combo;
    public GameObject MissionObject;
    public GameObject WaitObject;
    public GameObject StartObject;
    public GameObject ScorePlusObject;
    public GameObject OtherPlusObject;
    
    public GameObject SkillArea;
    public UISlider BossHPBar;
    public UILabel GameTime;
    public UILabel energyLabel;
    public GameObject[] StateICON;
    public GameObject MessagePanel;
    public GameObject GoodGamePanel;
    public GameObject TestPanel;
    public GameObject exitBtn;
    public List<GameObject> hole;

  [Range(0.1f, 1.0f)]
    public float _beautyHP;                // 美化血條用

    void Start()
    {
        Debug.Log("Hole:"+hole.Count);
        Debug.Log(hole[0].name);
    }
}
