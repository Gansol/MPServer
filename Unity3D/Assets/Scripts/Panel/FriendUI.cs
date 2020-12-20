using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using MPProtocol;
/* ***************************************************************
 * -----Copyright © 2015 Gansol Studio.  All Rights Reserved.-----
 * -----------            CC BY-NC-SA 4.0            -------------
 * -----------  @Website:  EasyUnity@blogspot.com    -------------
 * -----------  @Email:    GansolTW@gmail.com        -------------
 * -----------  @Author:   Krola.                    -------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * 負責 好友的所有處理
 * 386行 SelectPlayer 還沒寫 選取朋友時變色
 * ***************************************************************
 *                           ChangeLog
 * 20171225 v1.1.3   優化
 * 20171215 v1.1.2   修正BUG、優化 註解             
 * 20201027 v3.0.0  繼承重構
 * ****************************************************************/
public class FriendUI : IMPPanelUI
{
    private AttachBtn_FriendUI UI;
    /// <summary>
    /// slot偏移量(間距)
    /// </summary>
    public Vector2 offset;

    private Vector2 itemPos;                                           // 朋友SLOT位子
    private GameObject _lastMsgPanel;                                  // 上個訊息視窗
    private static List<string> _clientFriendsList;                     // 本機的朋友列表

    //          初次載入    是否載入ICON    是否載入玩家資料    是否取得線上玩家狀態  是否載入朋友資料    是否載入Panel   是否按下
    private bool _bFirstLoad, _bLoadedIcon, _bLoadPlayerData, _bLoadActoOnlinerState, _bLoadFriendsData, _bLoadedPanel, _bClick;
    private string _selectPlayerNickname, _inviteYouNameOfFriend;       // 選取得朋友名稱 邀請你的朋友名稱
    private int _dataLoadedCount;
    private float _lastClickTime, _clickInterval;                      // 上次點擊時間 點擊間格

    public FriendUI(MPGame MPGame): base(MPGame)
    {
        m_RootUI = GameObject.Find(Global.Scene.MainGameAsset.ToString()).GetComponentInChildren<AttachBtn_MenuUI>().friendPanel;
    }

    public override void Initinal()
    {
        Debug.Log("FrinedUI Init!");
        _bFirstLoad = true;
        _bLoadedPanel = false;
        itemPos = new Vector2(0, 0);
        offset = new Vector2(0, -150);
        _clickInterval = 3;

        Global.photonService.LoadPlayerDataEvent += OnLoadPlayerData;
        Global.photonService.LoadFriendsDataEvent += OnLoadFriendsData;
        Global.photonService.GetOnlineActorStateEvent += OnGetOnlineActorState;
        Global.photonService.ApplyInviteFriendEvent += OnApplyInviteFriend;
        Global.photonService.RemoveFriendEvent += OnRemoveFriend;
    }

    public override  void  Update()
    {
        base.Update();
        // 資料載入完成時 載入PanelAsset
        if (_dataLoadedCount == GetMustLoadedDataCount() && !_bLoadedPanel)
            OnLoadPanel();

        // Asset載入完成時 實體化道具
        if (assetLoader.bLoadedObj && _bLoadedIcon && _bLoadActoOnlinerState)
        {
            _bLoadedIcon = true;
            _bLoadActoOnlinerState = !_bLoadActoOnlinerState;

            // 實體化 朋友資訊列
            foreach (KeyValuePair<string, object> friend in Global.dictOnlineFriendsDetail)
                InstantiateFriend(friend.Value as Dictionary<string, object>, friend.Key);

            //載入朋友狀態
            LoadFriendOnlineState();

            //改變事件遮罩
            ResumeToggleTarget();
        }

        // 配對間隔
        if (Time.time > _lastClickTime + _clickInterval && _bClick)
        {
            _bClick = false;
            UI.itemPanel.Find(_inviteYouNameOfFriend).Find("Match").gameObject.SetActive(true);
        }
    }

    protected override void OnLoading()
    {
        UI = m_RootUI.GetComponentInChildren<AttachBtn_FriendUI>();
        _dataLoadedCount = (int)ENUM_Data.None;
        Global.photonService.LoadPlayerData(Global.Account);
        Global.photonService.LoadFriendsData(Global.dictFriends.ToArray());
        Global.photonService.GetOnlineActorState(Global.dictFriends.ToArray());

        UIEventListener.Get(UI.addBtn).onClick = ShowInviteFirend;
        UIEventListener.Get(UI.okBtn).onClick = InviteFirend;
        UIEventListener.Get(UI.removeBtn).onClick = RemoveFriend;
        UIEventListener.Get(UI.closeFriendCollider).onClick = OnClosed;
        UIEventListener.Get(UI.closeInviteCollider).onClick = OnPrev;

        //if (_bFirstLoad)
        //    Global.photonService.LoadMiceData();
        if (Global.isMatching)
            Global.photonService.ExitWaitingRoom();
    }

    protected override void OnLoadPanel()
    {
        _bLoadedPanel = true;
        GetMustLoadAsset();
        ResumeToggleTarget();

    }

    /// <summary>
    /// 取得必須載入的Asset
    /// </summary>
    protected override void GetMustLoadAsset()
    {
        if (_bFirstLoad)
        {
            Dictionary<string, object> dictMice = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> item in Global.miceProperty)
            {
                Dictionary<string, object> prop = item.Value as Dictionary<string, object>;
                prop.TryGetValue("ItemName", out object itemName);
                dictMice.Add(item.Key, itemName);
            }
            _clientFriendsList = new List<string>(Global.dictFriends);  // 新朋友名單存入client暫存朋友列表
            assetLoader.LoadAssetFormManifest(Global.PanelUniquePath + UI.slotItemName + Global.ext);  // 載入好友列表背景資產
            _bLoadedIcon = LoadIconObjects(dictMice, Global.MiceIconUniquePath);

            _bFirstLoad = false;
        }
    }

    /// <summary>
    /// 重新載入好友
    /// </summary>
    private void FriendSlotChk()
    {
        if (m_RootUI.activeSelf)
        {
            List<string> newFriendList = Global.dictFriends.Except(_clientFriendsList).ToList();
            List<string> oldFriendList = _clientFriendsList.Except(Global.dictFriends).ToList();

            // add
            if (newFriendList.Count > 0) AddFriendSlot();

            Debug.Log("newFriendList.Count newFriendList.Count :" + newFriendList.Count);
            //remove
            if (oldFriendList.Count > 0) RemoveFriendSlot();

            // reload
            _clientFriendsList = new List<string>(Global.dictFriends);
            LoadFriendOnlineState();
            ReloadFriendSlot();
        }
    }

    /// <summary>
    /// 重新載入好友資料
    /// </summary>
    private void ReloadFriendSlot()
    {
        int i = 0;
        foreach (KeyValuePair<string, object> friend in Global.dictOnlineFriendsDetail)
        {
            Dictionary<string, object> details = friend.Value as Dictionary<string, object>;

            details.TryGetValue("Rank", out object rank);
            details.TryGetValue("Nickname", out object nickname);
            details.TryGetValue("Image", out object actorImage);

            if ( UI.itemPanel.childCount != 0)
            {
                UI.itemPanel.GetChild(i).name = friend.Key;
                LoadFriendData(friend.Key, rank.ToString(), nickname.ToString(), actorImage.ToString(), UI.itemPanel.Find(friend.Key));
            }

            i++;
        }
    }

    /// <summary>
    /// 新增朋友、欄位
    /// </summary>
    private void AddFriendSlot()
    {
        Debug.Log("AddFriendSlot AddFriendSlot");
        List<string> addFriendsList = new List<string>();

        addFriendsList = Global.dictFriends.Except(_clientFriendsList).ToList();

        foreach (string friend in addFriendsList)
            if (!_clientFriendsList.Contains(friend))
                InstantiateFriend(Global.dictOnlineFriendsDetail[friend], friend);
    }

    /// <summary>
    /// 移除朋友、欄位
    /// </summary>
    private void RemoveFriendSlot()
    {
        List<string> removeFriendsList = new List<string>();

        removeFriendsList = _clientFriendsList.Except(Global.dictFriends).ToList();

        foreach (string removeName in removeFriendsList)
        {
            if (UI.itemPanel.childCount != 0)
            {
                NGUITools.Destroy(UI.itemPanel.GetChild(UI.itemPanel.childCount - 1));
                Global.dictOnlineFriendsDetail.Remove(removeName);
                _clientFriendsList.Remove(removeName);
                itemPos.y -= offset.y;
            }
        }
    }

    /// <summary>
    /// 實體化朋友列
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    private GameObject InstantiateItem(Vector2 offset)
    {
        GameObject bundle = assetLoader.GetAsset(UI.slotItemName);
        GameObject item = MPGFactory.GetObjFactory().Instantiate(bundle, UI.itemPanel, UI.slotItemName, offset, Vector2.one, Vector2.zero, -1);
        UIEventListener.Get(item).onClick = SelectPlayer;
        return item;
    }

    private void InstantiateICON(string imageName, Transform friendItemSlot)
    {
       // Debug.Log("InstantiateICON: " + imageName);
        GameObject bundle = assetLoader.GetAsset(imageName.ToString());
        MPGFactory.GetObjFactory().Instantiate(bundle, friendItemSlot.Find("Image"), imageName, Vector2.zero, Vector2.one, Vector2.zero, 10);
    }

    /// <summary>
    /// 載入朋友線上狀態
    /// </summary>
    private void LoadFriendOnlineState()
    {
        int i = 0;
        ENUM_MemberState ActorOnlineState;

        // 如果 朋友、欄位>0 載入資料
        if (UI.itemPanel.childCount > 0 && !string.IsNullOrEmpty(Global.dictFriends[0]))
            foreach (string friend in Global.dictFriends)
            {
                if (Global.dictOnlineFriendsState.ContainsKey(friend))
                {
                    UI.itemPanel.GetChild(i).Find("BG").gameObject.SetActive(true);
                    ActorOnlineState = (ENUM_MemberState)Convert.ToInt16(Global.dictOnlineFriendsState[friend]);

                    switch (ActorOnlineState)
                    {
                        case ENUM_MemberState.Online:
                        case ENUM_MemberState.Idle:
                            {
                                if (!_bClick) UI.itemPanel.Find(friend).Find("Match").gameObject.SetActive(true);
                                UIEventListener.Get(UI.itemPanel.Find(friend).Find("Match").gameObject).onClick = InviteMatchGame;
                                break;
                            }
                        case ENUM_MemberState.Offline:
                        case ENUM_MemberState.Playing:
                            {
                                UI.itemPanel.Find(friend).Find("Match").gameObject.SetActive(false);
                                break;
                            }
                        default:
                            UI.itemPanel.Find(friend).Find("Match").gameObject.SetActive(false);
                            break;
                    }
                }
                else
                {
                    // 不再線上 顯示無法使用狀態
                    UI.itemPanel.Find(friend).Find("Match").gameObject.SetActive(false);
                    UI.itemPanel.Find(friend).Find("BG").gameObject.SetActive(false);
                }
                i++;
            }
    }

    /// <summary>
    /// 載入朋友資料
    /// </summary>
    /// <param name="account">帳號</param>
    /// <param name="rank">等級</param>
    /// <param name="nickname">暱稱</param>
    /// <param name="actorImage">玩家頭像icon</param>
    /// <param name="friendItemSlot">位置</param>
    private void LoadFriendData(string account, string rank, string nickname, string actorImage, Transform friendItemSlot)
    {
        friendItemSlot.name = account;
        friendItemSlot.Find("Rank").GetComponent<UILabel>().text = rank;
        friendItemSlot.Find("NickName").GetComponent<UILabel>().text = nickname;
        friendItemSlot.Find("Image").GetComponentInChildren<UISprite>().name = actorImage.ToLower();
    }

    /// <summary>
    /// 完整實體化朋友列、載入朋友資料
    /// </summary>
    /// <param name="detail">詳細資料</param>
    /// <param name="account">詳細資料</param>
    public void InstantiateFriend(object detail, string account)
    {
        Dictionary<string, object> details = detail as Dictionary<string, object>;

        object rank, nickname, actorImage;
        details.TryGetValue("Rank", out rank);
        details.TryGetValue("Nickname", out nickname);
        details.TryGetValue("Image", out actorImage);
        GameObject friendItemSlot = InstantiateItem(itemPos);
        InstantiateICON(actorImage.ToString().ToLower(), friendItemSlot.transform);
        LoadFriendData(account, rank.ToString(), nickname.ToString(), actorImage.ToString(), friendItemSlot.transform);
        itemPos.y += offset.y;
    }

    /// <summary>
    /// 收到玩家資料時
    /// </summary>
    private void OnLoadPlayerData()
    {
        _dataLoadedCount *= (int)ENUM_Data.PlayerData;
    }

    /// <summary>
    /// 收到朋友資料時
    /// </summary>
    private void OnLoadFriendsData()
    {
        _dataLoadedCount *= (int)ENUM_Data.FriendsData;
    }

    /// <summary>
    /// 收到伺服器玩家狀態時
    /// </summary>
    private void OnGetOnlineActorState()
    {
        if (m_RootUI.activeSelf)
        {
            _bLoadActoOnlinerState = true;
            if (!_bFirstLoad)
                FriendSlotChk();
        }
    }

    /// <summary>
    /// 同意好友邀請時
    /// </summary>
    private void OnApplyInviteFriend()
    {
        FriendSlotChk();
    }

    private void OnRemoveFriend()
    {
        FriendSlotChk();
    }

    public override void OnClosed(GameObject obj)
    {
        EventMaskSwitch.lastPanel = null;
        ShowPanel(m_RootUI.name);
       // GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().LoadPanel(obj.transform.parent.gameObject);
    }

    public void OnPrev(GameObject obj)
    {
        _lastMsgPanel.SetActive(false);
        EventMaskSwitch.Prev(1);
    }

    /// <summary>
    /// 選取玩家暱稱時
    /// </summary>
    /// <param name="player"></param>
    public void SelectPlayer(GameObject player)
    {
        _selectPlayerNickname = player.name;
        // Tween 
    }

    /// <summary>
    /// 顯示邀請玩家視窗(輸入邀請玩家)
    /// </summary>
    /// <param name="message"></param>
    public void ShowInviteFirend(GameObject message)
    {
        _lastMsgPanel = UI.messagePanel;
        _lastMsgPanel.SetActive(true);
        EventMaskSwitch.Switch(_lastMsgPanel);
    }

    /// <summary>
    /// 按下邀請好友時
    /// </summary>
    /// <param name="message"></param>
    /// <param name="input"></param>
    public void InviteFirend(GameObject message)
    {
        string friendName = UI.account_Label.GetComponent<UILabel>().text;
        if (!string.IsNullOrEmpty(friendName))
        {
            if (!Global.dictFriends.Contains(friendName))
            {
                Global.photonService.InviteFriend(friendName);
             UI.messagePanel.SetActive(false);
                return;
            }
            Global.ShowMessage("已新增為好友！", Global.MessageBoxType.Yes, 0);
            return;
        }
        Global.ShowMessage("請輸入暱稱或帳號！", Global.MessageBoxType.Yes, 1);
    }

    /// <summary>
    /// 按下移除好友時
    /// </summary>
    public void RemoveFriend(GameObject btn)
    {
        if (!string.IsNullOrEmpty(_selectPlayerNickname))
        {
            Global.photonService.RemoveFriend(_selectPlayerNickname);
            _selectPlayerNickname = null;
        }
    }

    /// <summary>
    /// 按下邀請好友對戰時
    /// </summary>
    /// <param name="go"></param>
    private void InviteMatchGame(GameObject go)
    {
        _inviteYouNameOfFriend = go.transform.parent.name;
        UI.itemPanel.Find(_inviteYouNameOfFriend).Find("Match").gameObject.SetActive(false);
        _lastClickTime = Time.time;
        _bClick = true;
        Global.photonService.InviteMatchGame(_inviteYouNameOfFriend);
    }

    protected override int GetMustLoadedDataCount()
    {
        return (int)ENUM_Data.PlayerData * (int)ENUM_Data.FriendsData;
    }


    public override void Release()
    {
        if (_lastMsgPanel != null)
            _lastMsgPanel.SetActive(false);

        Global.photonService.LoadPlayerDataEvent -= OnLoadPlayerData;
        Global.photonService.LoadFriendsDataEvent -= OnLoadFriendsData;
        Global.photonService.GetOnlineActorStateEvent -= OnGetOnlineActorState;
        Global.photonService.ApplyInviteFriendEvent -= OnApplyInviteFriend;
        Global.photonService.RemoveFriendEvent -= OnRemoveFriend;
    }
}
