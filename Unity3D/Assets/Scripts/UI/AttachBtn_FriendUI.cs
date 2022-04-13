using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachBtn_FriendUI : MonoBehaviour
{
    public GameObject addBtn;// show invite friend
    public GameObject removeBtn;//remove firend
    public GameObject account_Label;
    public GameObject okBtn;   //invite friend 
    public GameObject closeFriendCollider;
    public GameObject closeInviteCollider;
    public GameObject messagePanel; // 訊息Panel
    /// <summary>
    /// 道具Panel
    /// </summary>       
    public Transform itemPanel;
    /// <summary>
    /// 圖片位子、slot名稱
    /// </summary>
    public string slotItemName = "frienditem";
}
