using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using MPProtocol;
using System;
// recvie ActorOnline State
// select new data add or del
// 
public class FriendManager : MPPanel
{
    public string iconPath, slotItemName;
    public Transform itemPanel;
    public Vector2 offset;
    private bool _bFirstLoad, _bLoadedIcon, _bLoadActorState, _bLoadFriendsData, _bAddFriend, _bRemoveFriend, _bClick;
    private string _selectPlayerNickname, _tmpFriend;
    private List<string> _tmpFriends;
    private Vector2 itemPos;
    private GameObject _lastMsgPanel;
    private float _lastClickTime, _clickInterval;

    public FriendManager(MPGame MPGame) : base(MPGame) { }

    void Awake()
    {
        itemPos = new Vector2(0, 0);

        _bFirstLoad = true;
        _clickInterval = 3;
    }

    void OnEnable()
    {
        Global.photonService.LoadPlayerDataEvent += OnLoadPanel;
        Global.photonService.LoadFriendsDataEvent += OnLoadFriendsData;
        Global.photonService.GetOnlineActorStateEvent += OnGetOnlineActorState;
        Global.photonService.ApplyInviteFriendEvent += OnApplyInviteFriend;
        Global.photonService.RemoveFriendEvent += OnRemoveFriend;
    }

    void Update()
    {
        if (assetLoader.loadedObj && _bLoadedIcon && _bLoadActorState && _bLoadFriendsData)
        {
            _bLoadedIcon = !_bLoadedIcon;
            _bLoadActorState = !_bLoadActorState;

            // 實體化 朋友資訊列
            foreach (KeyValuePair<string, object> friend in Global.dictOnlineFriendsDetail)
                InstantiateFriend(friend.Value as Dictionary<string, object>, friend.Key);

            //載入朋友狀態
            LoadFriendState();

            //改變事件遮罩
            ResumeToggleTarget();
        }

        if (Time.time > _lastClickTime + _clickInterval && _bClick)
        {
            _bClick = false;
            itemPanel.Find(_tmpFriend).Find("Match").gameObject.SetActive(true);
        }
    }

    protected override void OnLoading()
    {
        Global.photonService.LoadPlayerData(Global.Account);
        Global.photonService.LoadFriendsData(Global.dictFriends.ToArray());
        Global.photonService.GetOnlineActorState(Global.dictFriends.ToArray());
        if (_bFirstLoad)
            Global.photonService.LoadMiceData();
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
                _tmpFriends = Global.dictFriends;
                assetLoader.LoadAsset(iconPath + "/", iconPath);
                _bLoadedIcon = LoadIconObject(dictMice, iconPath);
                assetLoader.LoadPrefab("Panel/", slotItemName);
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
    private void ReloadFriend()
    {
        List<string> buffer = new List<string>();
        int i = 0;

        if (Global.dictFriends.Count >= _tmpFriends.Count && !string.IsNullOrEmpty(Global.dictFriends[0]))    // 新增朋友
        {
            foreach (string friend in Global.dictFriends)
            {
                if (!_tmpFriends.Contains(friend)) buffer.Add(friend);
            }

            _tmpFriends = buffer;

            foreach (string friend in _tmpFriends)
                InstantiateFriend(Global.dictOnlineFriendsDetail[friend] as Dictionary<string, object>, friend);

            _tmpFriends = Global.dictFriends;
        }
        else if (Global.dictFriends.Count < _tmpFriends.Count || string.IsNullOrEmpty(Global.dictFriends[0]))   //移除朋友  如果舊資料比新資料大 且 薪資料是空值
        {

            _bRemoveFriend = true;

            foreach (string friend in _tmpFriends)
            {
                if (!Global.dictFriends.Contains(friend)) buffer.Add(friend);
            }

            foreach (string removeName in buffer)
            {
                if (itemPanel.childCount != 0)
                {
                    NGUITools.Destroy(itemPanel.GetChild(itemPanel.childCount - 1));
                    itemPos.y -= offset.y;
                }
            }
        }

        // 重新載入好友資料
        foreach (KeyValuePair<string, object> friend in Global.dictOnlineFriendsDetail)
        {
            Dictionary<string, object> detail = friend.Value as Dictionary<string, object>;
            object rank, nickname, imageName;

            detail.TryGetValue("Rank", out rank);
            detail.TryGetValue("Nickname", out nickname);
            detail.TryGetValue("Image", out imageName);
            if (itemPanel.childCount != 0)
                LoadFriendData(friend.Key, rank.ToString(), nickname.ToString(), itemPanel.Find(friend.Key));
            i++;
        }

        _tmpFriends = Global.dictFriends;
        LoadFriendState();
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

    private void LoadFriendState()
    {
        int i = 0;
        ENUM_MemberState ActorState;
        if (itemPanel.childCount != 0 && !string.IsNullOrEmpty(Global.dictFriends[0]))
            foreach (string friend in Global.dictFriends)
            {
                if (Global.dictOnlineFriendsState.ContainsKey(friend))
                {
                    itemPanel.GetChild(i).Find("BG").gameObject.SetActive(true);
                    ActorState = (ENUM_MemberState)Convert.ToInt16(Global.dictOnlineFriendsState[friend]);
                    switch (ActorState)
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
                    }
                }
                else
                {
                    itemPanel.Find(friend).Find("Match").gameObject.SetActive(false);
                    itemPanel.Find(friend).Find("BG").gameObject.SetActive(false);
                }
                i++;
            }

    }

    private void LoadFriendData(string account, string rank, string nickname, Transform friendItemSlot)
    {
        friendItemSlot.name = account;
        friendItemSlot.Find("Rank").GetComponent<UILabel>().text = rank;
        friendItemSlot.Find("NickName").GetComponent<UILabel>().text = nickname;
    }


    private void OnGetOnlineActorState()
    {
        _bLoadActorState = true;
        if (!_bFirstLoad) ReloadFriend();
    }

    private void OnApplyInviteFriend()
    {
        ReloadFriend();
    }

    private void OnRemoveFriend()
    {
        ReloadFriend();
        //if (!_bFirstLoad && _bRemoveFriend)
        //{
        //    int i = 0;
        //    _bRemoveFriend = false;
        //    foreach (KeyValuePair<string, object> friend in Global.dictOnlineFriendsDetail)
        //    {
        //        Dictionary<string, object> detail = friend.Value as Dictionary<string, object>;
        //        object rank, nickname, imageName;

        //        detail.TryGetValue("Rank", out rank);
        //        detail.TryGetValue("Nickname", out nickname);
        //        detail.TryGetValue("Image", out imageName);

        //        LoadFriendData(rank.ToString(), nickname.ToString(), itemPanel.GetChild(i));
        //        i++;
        //    }
        //    ReloadFriend();
        //    _tmpFriend = Global.dictFriends;
        //    LoadFriendState();
        //}
    }

    /// <summary>
    /// 完整實體化朋友列、載入朋友資料
    /// </summary>
    /// <param name="detail">詳細資料</param>
    public void InstantiateFriend(Dictionary<string, object> detail, string account)
    {
        object rank, nickname, imageName;
        detail.TryGetValue("Rank", out rank);
        detail.TryGetValue("Nickname", out nickname);
        detail.TryGetValue("Image", out imageName);
        GameObject friendItemSlot = InstantiateItem(itemPos);
        InstantiateICON(imageName.ToString(), friendItemSlot.transform);
        LoadFriendData(account, rank.ToString(), nickname.ToString(), friendItemSlot.transform);
        itemPos.y += offset.y;
    }

    private void OnLoadFriendsData()
    {
        _bLoadFriendsData = true;
    }

    public override void OnClosed(GameObject obj)
    {
        EventMaskSwitch.lastPanel = null;
        GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().LoadPanel(obj.transform.parent.gameObject);
        // EventMaskSwitch.Prev();
    }
    public void OnPrev(GameObject obj)
    {
        _lastMsgPanel.SetActive(false);
        EventMaskSwitch.Prev(1);
    }

    public void SelectPlayer(GameObject player)
    {
        _selectPlayerNickname = player.name;
    }

    public void ShowInviteFirend(GameObject message)
    {
        _lastMsgPanel = message;
        message.SetActive(true);
        EventMaskSwitch.Switch(message, true);
    }

    public void InviteFirend(GameObject message, UIInput input)
    {
        if (!string.IsNullOrEmpty(input.value))
        {
            if (!Global.dictFriends.Contains(input.value))
            {
                Global.photonService.InviteFriend(input.value);
                message.SetActive(false);
            }
            else
            {
                Global.ShowMessage("已新增為好友！", 0);
            }
        }
        else
        {
            Global.ShowMessage("請輸入暱稱或帳號！", 0);
        }
        EventMaskSwitch.Prev(1);
    }

    public void RemoveFirend()
    {
        if (!string.IsNullOrEmpty(_selectPlayerNickname))
        {
            Global.photonService.RemoveFriend(_selectPlayerNickname);
            _selectPlayerNickname = null;
        }
    }

    private void InviteMatchGame(GameObject go)
    {
        _tmpFriend = go.transform.parent.name;
        itemPanel.Find(_tmpFriend).Find("Match").gameObject.SetActive(false);
        _lastClickTime = Time.time;
        _bClick = true;
        Global.photonService.InviteMatchGame(_tmpFriend);
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
