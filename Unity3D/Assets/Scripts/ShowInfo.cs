using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MPProtocol;
using MiniJSON;
using System;

public class ShowInfo : MonoBehaviour
{
    private static bool bFirstLoad = true;
    public UILabel vision, frinedLabel;
    public float _updateInfoInterval;
    private float _lastTime;

    void Awake()
    {
        _lastTime = 0;
        _updateInfoInterval = 60;
        if (bFirstLoad)
        {
            Global.photonService.ActorOnlineEvent += OnGetActorOnline;
            Global.photonService.GetOnlineActorStateEvent += OnGetOnlineActorState;
            Global.photonService.ApplyInviteFriendEvent += OnGetOnlineActorState;
            bFirstLoad = false;
        }
    }

    void Start()
    {
        vision.text = "線上玩家數:" + Global.OnlineActor + "  Ver." + Global.gameVersion + " build " + Global.bundleVersion;
    }

    void Update()
    {
        if (enabled && Global.LoginStatus && Global.dictFriends.Count != 0 && Time.time > _lastTime && Global.MemberType != MemberType.Bot)
        {
            Global.photonService.GetOnlineActorState(Global.dictFriends.ToArray());
            _lastTime = Time.time + _updateInfoInterval;
        }
    }
    private void OnGetActorOnline()
    {
        vision.text = "線上玩家數:" + Global.OnlineActor + "  Ver." + Global.gameVersion + " build " + Global.bundleVersion;
    }

    private void OnGetOnlineActorState()
    {
        int friendsCount = 0;
        ENUM_MemberState actorState;

        // 取得 朋友的線上狀態
        foreach (KeyValuePair<string, object> friend in Global.dictOnlineFriendsState)
        {
            actorState = (ENUM_MemberState)Convert.ToInt16(friend.Value);
            if (actorState != ENUM_MemberState.Offline)
                friendsCount++;
        }
        frinedLabel.text = friendsCount.ToString();
    }
}
