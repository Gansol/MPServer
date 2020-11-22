using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachBtn_TeamUI : MonoBehaviour
{
    public GameObject startShowActor;                                               // 起始顯示老鼠
    public Transform miceImage;
    public UILabel itemName;
    public UILabel eatingRate;
    public UILabel miceSpeed;
    public UILabel lifeTime;
    public UILabel eatFull;
    public UILabel skillID;
    public UILabel hp;
    public UILabel miceCost;
    public UILabel life;
    public UILabel description;
    public UILabel sayHello;
    public UILabel rank;
    public UILabel exp;
    public UILabel cost;
    public UISlider expSlider;

    public Transform miceGroup;
    public Transform infoGroup;
    public Transform teamGroup;
    public Transform itemGroup;
    public Transform skillGroup;
    public GameObject closeCollider;
}
