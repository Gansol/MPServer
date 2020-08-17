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
 * ****************************************************************/
public class FriendManager : MPPanel
{
    /// <summary>
    /// 圖片位子、slot名稱
    /// </summary>
    public string  slotItemName;
    /// <summary>
    /// 道具Panel
    /// </summary>               
    public Transform itemPanel;
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
    private float _lastClickTime, _clickInterval;                      // 上次點擊時間 點擊間格

    public FriendManager(MPGame MPGame)
        : base(MPGame) { }

    void Awake()
    {
        _bFirstLoad = true;
        itemPos = new Vector2(0, 0);
        _clickInterval = 3;
    }

    void OnEnable()
    {
        _bLoadedPanel = false;
        Global.photonService.LoadPlayerDataEvent += OnPlayerDataEvent;
        Global.photonService.LoadFriendsDataEvent += OnLoadFriendsData;
        Global.photonService.GetOnlineActorStateEvent += OnGetOnlineActorState;
        Global.photonService.ApplyInviteFriendEvent += OnApplyInviteFriend;
        Global.photonService.RemoveFriendEvent += OnRemoveFriend;
    }

    void Update()
    {
        // 資料載入完成時 載入PanelAsset
        if (_bLoadPlayerData && _bLoadFriendsData && !_bLoadedPanel)
        {
            OnLoadPanel();
            _bLoadedPanel = true;
        }

        // Asset載入完成時 實體化道具
        if (m_MPGame.GetAssetLoader().bLoadedObj && _bLoadedIcon && _bLoadActoOnlinerState)
        {
            Debug.Log("FUCK");

            _bLoadedIcon = !_bLoadedIcon;
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
            itemPanel.Find(_inviteYouNameOfFriend).Find("Match").gameObject.SetActive(true);
        }
    }

    protected override void OnLoading()
    {
        Global.photonService.LoadPlayerData(Global.Account);
        Global.photonService.LoadFriendsData(Global.dictFriends.ToArray());
        Global.photonService.GetOnlineActorState(Global.dictFriends.ToArray());
        if (_bFirstLoad)
            Global.photonService.LoadMiceData();
        if (Global.isMatching)
            Global.photonService.ExitWaitingRoom();
    }

    protected override void OnLoadPanel()
    {
        GetMustLoadAsset();
        ResumeToggleTarget();
    }

    /// <summary>
    /// 取得必須載入的Asset
    /// </summary>
    protected override void GetMustLoadAsset()
    {
        if (enabled)
        {
            if (_bFirstLoad)
            {
                object itemName;
                Dictionary<string, object> dictMice = new Dictionary<string, object>();
                foreach (KeyValuePair<string, object> item in Global.miceProperty)
                {
                    Dictionary<string, object> prop = item.Value as Dictionary<string, object>;
                    prop.TryGetValue("ItemName", out itemName);
                    dictMice.Add(item.Key, itemName);
                }
                _clientFriendsList = Global.dictFriends;
                //assetLoader.LoadAsset(iconPath + "/", iconPath);
                _bLoadedIcon = LoadIconObjects(dictMice, Global.MiceIconUniquePath);
                assetLoader.LoadAssetFormManifest(Global.PanelUniquePath + slotItemName + Global.ext);
              //  assetLoader.LoadPrefab("panel/", slotItemName);
                _bFirstLoad = false;
            }
        }
        else
        {
            Debug.Log("dictFriends is null");
        }
    }

    /// <summary>
    /// 重新載入好友
    /// </summary>
    private void FriendSlotChk()
    {
        List<string> buffer = new List<string>();

        if (enabled)
        {
            List<string> newFriendList = Global.dictFriends.Except(_clientFriendsList).ToList();
            List<string> oldFriendList = _clientFriendsList.Except(Global.dictFriends).ToList();

            // add
            if (newFriendList.Count > 0) AddFriendSlot();

            //remove
            if (oldFriendList.Count > 0) RemoveFriendSlot();

            // reload
            _clientFriendsList = new List<string>( Global.dictFriends);
            ReloadFriendSlot();
            LoadFriendOnlineState();  
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
            object rank, nickname, imageName;
            Dictionary<string, object> detail = friend.Value as Dictionary<string, object>;
            
            detail.TryGetValue("Rank", out rank);
            detail.TryGetValue("Nickname", out nickname);
            detail.TryGetValue("Image", out imageName);

            if (itemPanel.childCount != 0)
            {
                itemPanel.GetChild(i).name = friend.Key;
                LoadFriendData(friend.Key, rank.ToString(), nickname.ToString(), itemPanel.Find(friend.Key));
            }

            i++;
        }
    }

    /// <summary>
    /// 新增朋友、欄位
    /// </summary>
    private void AddFriendSlot()
    {
        List<string> addFriendsList = new List<string>();

        addFriendsList = Global.dictFriends.Except(_clientFriendsList).ToList();

        //foreach (string friend in Global.dictFriends)
        //    if (!_clientFriendsList.Contains(friend)) addFriendsList.Add(friend);

        foreach (string friend in addFriendsList)
            InstantiateFriend(Global.dictOnlineFriendsDetail[friend], friend);
    }

    /// <summary>
    /// 移除朋友、欄位
    /// </summary>
    private void RemoveFriendSlot()
    {
        List<string> removeFriendsList = new List<string>();

        removeFriendsList = _clientFriendsList.Except(Global.dictFriends).ToList();
        //foreach (string friend in _clientFriendsList)
        //    if (!Global.dictFriends.Contains(friend)) removeFriendsList.Add(friend);

        foreach (string removeName in removeFriendsList)
        {
            if (itemPanel.childCount != 0)
            {
                NGUITools.Destroy(itemPanel.GetChild(itemPanel.childCount - 1));
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
        GameObject bundle = assetLoader.GetAsset(slotItemName);
        GameObject item = MPGFactory.GetObjFactory().Instantiate(bundle, itemPanel, slotItemName, offset, Vector2.one, Vector2.zero, -1);
        UIEventListener.Get(item).onClick = SelectPlayer;
        return item;
    }

    private void InstantiateICON(string imageName, Transform friendItemSlot)
    {
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
        if (itemPanel.childCount > 0 && !string.IsNullOrEmpty(Global.dictFriends[0]))
            foreach (string friend in Global.dictFriends)
            {
                if (Global.dictOnlineFriendsState.ContainsKey(friend))
                {
                    itemPanel.GetChild(i).Find("BG").gameObject.SetActive(true);
                    ActorOnlineState = (ENUM_MemberState)Convert.ToInt16(Global.dictOnlineFriendsState[friend]);

                    switch (ActorOnlineState)
                    {
                        case ENUM_MemberState.Online:
                        case ENUM_MemberState.Idle:
                            {
                                if (!_bClick) itemPanel.Find(friend).Find("Match").gameObject.SetActive(true);
                                UIEventListener.Get(itemPanel.Find(friend).Find("Match").gameObject).onClick = InviteMatchGame;
                                break;
                            }
                        case ENUM_MemberState.Offline:
                        case ENUM_MemberState.Playing:
                            {
                                itemPanel.Find(friend).Find("Match").gameObject.SetActive(false);
                                break;
                            }
                        default:
                            itemPanel.Find(friend).Find("Match").gameObject.SetActive(false);
                            break;
                    }
                }
                else
                {
                    // 不再線上 顯示無法使用狀態
                    itemPanel.Find(friend).Find("Match").gameObject.SetActive(false);
                    itemPanel.Find(friend).Find("BG").gameObject.SetActive(false);
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
    /// <param name="friendItemSlot">位置</param>
    private void LoadFriendData(string account, string rank, string nickname, Transform friendItemSlot)
    {
        friendItemSlot.name = account;
        friendItemSlot.Find("Rank").GetComponent<UILabel>().text = rank;
        friendItemSlot.Find("NickName").GetComponent<UILabel>().text = nickname;
    }

    /// <summary>
    /// 完整實體化朋友列、載入朋友資料
    /// </summary>
    /// <param name="detail">詳細資料</param>
    /// <param name="account">詳細資料</param>
    public void InstantiateFriend(object detail, string account)
    {
        Dictionary<string, object> details = detail as Dictionary<string, object>;

        object rank, nickname, imageName;
        details.TryGetValue("Rank", out rank);
        details.TryGetValue("Nickname", out nickname);
        details.TryGetValue("Image", out imageName);
        GameObject friendItemSlot = InstantiateItem(itemPos);
        InstantiateICON(imageName.ToString(), friendItemSlot.transform);
        LoadFriendData(account, rank.ToString(), nickname.ToString(), friendItemSlot.transform);
        itemPos.y += offset.y;
    }

    /// <summary>
    /// 收到玩家資料時
    /// </summary>
    private void OnPlayerDataEvent()
    {
        _bLoadPlayerData = true;
    }

    /// <summary>
    /// 收到朋友資料時
    /// </summary>
    private void OnLoadFriendsData()
    {
        _bLoadFriendsData = true;
    }

    /// <summary>
    /// 收到伺服器玩家狀態時
    /// </summary>
    private void OnGetOnlineActorState()
    {
        if (enabled)
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
        GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().LoadPanel(obj.transform.parent.gameObject);
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
        _lastMsgPanel = message;
        message.SetActive(true);
        EventMaskSwitch.Switch(message);
    }

    /// <summary>
    /// 按下邀請好友時
    /// </summary>
    /// <param name="message"></param>
    /// <param name="input"></param>
    public void InviteFirend(GameObject message, UIInput input)
    {
        if (!string.IsNullOrEmpty(input.value))
        {
            if (!Global.dictFriends.Contains(input.value))
            {
                Global.photonService.InviteFriend(input.value);
                message.SetActive(false);
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
    public void RemoveFriend()
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
        itemPanel.Find(_inviteYouNameOfFriend).Find("Match").gameObject.SetActive(false);
        _lastClickTime = Time.time;
        _bClick = true;
        Global.photonService.InviteMatchGame(_inviteYouNameOfFriend);
    }

    void OnDisable()
    {
        if (_lastMsgPanel != null)
            _lastMsgPanel.SetActive(false);

        Global.photonService.LoadPlayerDataEvent -= OnLoadPanel;
        Global.photonService.LoadFriendsDataEvent -= OnLoadFriendsData;
        Global.photonService.GetOnlineActorStateEvent -= OnGetOnlineActorState;
        Global.photonService.ApplyInviteFriendEvent -= OnApplyInviteFriend;
        Global.photonService.RemoveFriendEvent -= OnRemoveFriend;
    }
}
