using ExitGames.Logging;
using MPProtocol;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using System;
using System.Collections.Generic;
using Gansol;
using MPCOM;
/* ***************************************************************
 * -----Copyright © 2015 Gansol Studio.  All Rights Reserved.-----
 * -----------            CC BY-NC-SA 4.0            -------------
 * -----------  @Website:  EasyUnity@blogspot.com    -------------
 * -----------  @Email:    GansolTW@gmail.com        -------------
 * -----------  @Author:   Krola.                    -------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * 
 * 主要的伺服器邏輯區塊 負責交換訊息 Request Response
 * 每一個玩家都會有一個MPServerPeer
 * 
 * Match Game 完成 還需加入 傳給對方的值
 * room.waitingList[1].TryGetValue("RoomActor1", out otherActor);
 * 1是代表字典檔中的 第1間房 多間房需要修改值
 * 這裡的otherAcotr都是單一玩家，需要取得多人要改寫
 * 回傳給另外玩家時，需要判斷他是否斷線了
 * ***************************************************************/

namespace MPServer
{
    public class MPServerPeer : PeerBase
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        public Guid peerGuid { get; protected set; }
        private MPServerApplication _server;
        private bool isCreateRoom;
        private Actor actor;
        private int roomID;
        private int primaryID;
        MPServerPeer peerOther;

        //初始化
        public MPServerPeer(IRpcProtocol rpcProtocol, IPhotonPeer nativePeer, MPServerApplication ServerApplication)
            : base(rpcProtocol, nativePeer)
        {
            isCreateRoom = false;
            peerGuid = Guid.NewGuid();      // 建立一個Client唯一識別GUID
            _server = ServerApplication;

            _server.Actors.AddConnectedPeer(peerGuid, this);    // 加入連線中的peer列表
        }

        //當斷線時
        protected override void OnDisconnect(PhotonHostRuntimeInterfaces.DisconnectReason reasonCode, string reasonDetail)
        {
            Log.Debug("peer:" + peerGuid + "\n已登出玩家：" + _server.Actors.GetAccountFromGuid(peerGuid) + "  時間:" + DateTime.Now);

            Actor existPlayer = _server.Actors.GetActorFromGuid(peerGuid);                                      // 用GUID找到離線的玩家
            try
            {
                if (_server.room.GetRoomFromGuid(peerGuid) > 0)                                                 // 假如用離開的玩家guid 從GameRoomList中找的到房間 (遊戲中中斷)
                {
                    int roomID = _server.room.GetRoomFromGuid(peerGuid);                                        //取得房間ID
                    Room.RoomActor otherActor = _server.room.GetOtherPlayer(roomID, existPlayer.PrimaryID);     // 取得房間內的其他玩家

                    _server.room.KillBossFromGuid(peerGuid);                                                    // 移除房間中的BOSS(如果有)
                    _server.room.RemovePlayingRoom(roomID, peerGuid, otherActor.guid);                          // 移除房間 與房間內玩家
                }

                if (_server.room.bGetWaitActorFromGuid(peerGuid))                                               // 如果這個玩家在等待房間中 
                {
                    _server.room.RemoveWatingRoom();                                                            // 移除等待房間(這裡要改應位以後可能有很多等待房間)
                }

            }
            catch (Exception e)
            {
                Log.Error("DisConnect Error : " + e.Message + " Track:" + e.StackTrace);
            }
            _server.Actors.ActorOffline(peerGuid); // 把玩家離線
        }

        //當收到Client回應時
        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            try
            {
                if (operationRequest.OperationCode < 9) // 若參數小於2則返回錯誤 (用來接收Client錯誤訊息使用)
                {
                    Log.Debug("IN <9 :" + operationRequest.OperationCode.ToString());
                    OperationResponse response = new OperationResponse(operationRequest.OperationCode) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = "Login Fail" };
                    SendOperationResponse(response, new SendParameters());
                }
                else
                {
                    //Log.Debug("IN >9 :" + operationRequest.OperationCode.ToString());

                    switch (operationRequest.OperationCode)
                    {

                        #region JoinMember 加入會員 (這裡面有IP、email的參數亂打)

                        case (byte)JoinMemberOperationCode.JoinMember:
                            try
                            {
                                Log.Debug("IN Join Member");
                                string email = (string)operationRequest.Parameters[(byte)JoinMemberParameterCode.Email];
                                string account = (string)operationRequest.Parameters[(byte)JoinMemberParameterCode.Account];
                                string password = (string)operationRequest.Parameters[(byte)JoinMemberParameterCode.Password];
                                string nickname = (string)operationRequest.Parameters[(byte)JoinMemberParameterCode.Nickname];
                                string IP = (string)operationRequest.Parameters[(byte)JoinMemberParameterCode.IP];
                                byte age = (byte)operationRequest.Parameters[(byte)JoinMemberParameterCode.Age];
                                byte sex = (byte)operationRequest.Parameters[(byte)JoinMemberParameterCode.Sex];

                                string joinTime = (string)operationRequest.Parameters[(byte)JoinMemberParameterCode.JoinDate];
                                MemberType memberType = (MemberType)operationRequest.Parameters[(byte)JoinMemberParameterCode.MemberType];

                                MemberUI memberUI = new MemberUI();
                                MemberData memberData;

                                if ((byte)memberType == 1)
                                {
                                    memberData = (MemberData)TextUtility.DeserializeFromStream(memberUI.JoinMember(account, password, nickname, age, sex, IP, email, joinTime, (byte)memberType));
                                }
                                else
                                {
                                    memberData = (MemberData)TextUtility.DeserializeFromStream(memberUI.JoinMember(account, nickname, IP, email, joinTime, (byte)memberType));
                                }

                                if (memberData.ReturnCode == "S101")
                                {
                                    Log.Debug("Join Member Successd ! ");
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)JoinMemberParameterCode.Ret, memberData.ReturnCode}
                                    };

                                    OperationResponse actorResponse = new OperationResponse((byte)JoinMemberResponseCode.JoinMember, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = memberData.ReturnMessage.ToString() };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }
                                else
                                {
                                    Log.Debug("Join Member Failed ! " + memberData.ReturnCode + memberData.ReturnMessage);
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)JoinMemberParameterCode.Ret, memberData.ReturnCode}
                                    };

                                    OperationResponse actorResponse = new OperationResponse((byte)JoinMemberResponseCode.JoinMember, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = memberData.ReturnMessage.ToString() };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }

                            }
                            catch (Exception e)
                            {
                                Log.Debug("Join Member Error ! " + e.Message + "Track: " + e.StackTrace);
                            }
                            break;

                        #endregion

                        #region Login 會員登入

                        case (byte)LoginOperationCode.Login:
                            try
                            {
                                string account = (string)operationRequest.Parameters[(byte)LoginParameterCode.Account];
                                string passowrd = (string)operationRequest.Parameters[(byte)LoginParameterCode.Password];
                                MemberType memberType = (MemberType)operationRequest.Parameters[(byte)LoginParameterCode.MemberType];
                                Log.Debug("Account:" + account + "Password:" + passowrd);
                                MemberUI memberUI = new MemberUI(); //實體化 UI (連結資料庫拿資料)
                                Log.Debug("Login IO OK");
                                MemberData memberData = (MemberData)TextUtility.DeserializeFromStream(memberUI.MemberLogin(account, passowrd)); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找
                                Log.Debug("Login Data OK");
                                Log.Debug("memberData.Account: " + memberData.Account);
                                //＊＊＊＊＊這有問題 如果重複登入 不能去拿伺服器資料 會偷資料＊＊＊＊＊　
                                switch (memberData.ReturnCode)　//取得資料庫資料成功
                                {
                                    case "S200":
                                        {
                                            Log.Debug("會員資料內部程式錯誤！");
                                            break;
                                        }
                                    #region 登入成功
                                    case "S201":
                                        {
                                            Log.Debug("memberData.ReturnCode == S201");
                                            primaryID = memberData.PrimaryID; //將資料傳入變數
                                            string nickname = memberData.Nickname;
                                            byte sex = memberData.Sex;
                                            byte age = memberData.Age;
                                            string IP = memberData.IP;

                                            //加入線上玩家列表
                                            ActorCollection.ActorReturn actorReturn = _server.Actors.ActorOnline(peerGuid, primaryID, account, nickname, age, sex, IP, this);
                                            Log.Debug("ReturnCode :" + actorReturn.ReturnCode);
                                            Log.Debug("ReturnMessage :" + actorReturn.DebugMessage);

                                            if (actorReturn.ReturnCode == "S301")// 加入線上會員資料成功 回傳資料
                                            {
                                                Log.Debug("actorReturn.ReturnCode == S301");
                                                Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                                    { (byte)LoginParameterCode.Ret, actorReturn.ReturnCode }, { (byte)LoginParameterCode.PrimaryID, primaryID }, { (byte)LoginParameterCode.Account, account },
                                                    { (byte)LoginParameterCode.Nickname, nickname }, { (byte)LoginParameterCode.Age, age }, { (byte)LoginParameterCode.Sex, sex } , { (byte)LoginParameterCode.MemberType, memberType } 
                                                };

                                                OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = memberData.ReturnMessage.ToString() };
                                                SendOperationResponse(actorResponse, new SendParameters());
                                            }
                                            else if (actorReturn.ReturnCode == "S302") //登入錯誤 重複登入
                                            {
                                                Log.Debug("重複區(3) PK:" + primaryID);

                                                // 用這個peer guid的PrimaryUD來找guid 踢掉線上玩家
                                                MPServerPeer peer;
                                                peer = _server.Actors.GetPeerFromPrimary(primaryID); //primaryID取得登入peer


                                                // 送出 重複登入事件 叫另外一個他斷線
                                                EventData eventData = new EventData((byte)LoginOperationCode.ReLogin);
                                                peer.SendEvent(eventData, new SendParameters());


                                                // 回傳給登入者 通知重複登入
                                                Dictionary<byte, object> parameter = new Dictionary<byte, object>{
                                                    { (byte)LoginParameterCode.Ret, actorReturn.ReturnCode }, { (byte)LoginParameterCode.PrimaryID, 0 }, { (byte)LoginParameterCode.Account, "" }, 
                                                    { (byte)LoginParameterCode.Nickname, "" }, { (byte)LoginParameterCode.Age, 0 }, { (byte)LoginParameterCode.Sex, 0 } , { (byte)LoginParameterCode.MemberType, memberType } 
                                                };

                                                OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = actorReturn.DebugMessage.ToString() };
                                                SendOperationResponse(actorResponse, new SendParameters());

                                                peer.OnDisconnect(new DisconnectReason(), "重複登入");
                                                Log.Debug("重複登入了!");
                                            }
                                            else // 登入錯誤 回傳空值
                                            {
                                                Dictionary<byte, object> parameter = new Dictionary<byte, object>{
                                                    { (byte)LoginParameterCode.Ret, actorReturn.ReturnCode }, { (byte)LoginParameterCode.PrimaryID, 0 }, { (byte)LoginParameterCode.Account, "" }, 
                                                    { (byte)LoginParameterCode.Nickname, "" }, { (byte)LoginParameterCode.Age, 0 }, { (byte)LoginParameterCode.Sex, 0 } , { (byte)LoginParameterCode.MemberType, memberType } 
                                                };

                                                OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = actorReturn.DebugMessage.ToString() };
                                                SendOperationResponse(actorResponse, new SendParameters());
                                                break;
                                            }
                                            break;
                                        }
                                    #endregion

                                    #region 登入失敗(判斷哪種方式登入) 如果是SNS 加入會員
                                    case "S204":
                                        {
                                            Log.Debug("memberData.ReturnCode == S204");

                                            if (memberType == MemberType.Gansol)    // 如果是基本會員登入 回傳 帳密錯誤
                                            {
                                                Log.Debug("Code:" + memberData.ReturnCode + "Message:" + memberData.ReturnMessage);
                                                OperationResponse response = new OperationResponse(operationRequest.OperationCode) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = memberData.ReturnMessage.ToString() };
                                                SendOperationResponse(response, new SendParameters());
                                                break;
                                            }
                                            else if (memberType == MemberType.Google)  // SNS登入失敗 加入會員
                                            {
                                                memberData = (MemberData)TextUtility.DeserializeFromStream(memberUI.JoinMember(account, "DefaultName", LocalIP, "example@example.com", DateTime.Now.ToString(), (byte)memberType)); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找
                                                if (memberData.ReturnCode == "S101")
                                                {
                                                    memberData = (MemberData)TextUtility.DeserializeFromStream(memberUI.MemberLogin(account, passowrd)); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找
                                                    if (memberData.ReturnCode == "S201")
                                                    {
                                                        Log.Debug("memberData.ReturnCode == S201");
                                                        primaryID = memberData.PrimaryID; //將資料傳入變數
                                                        string nickname = memberData.Nickname;
                                                        byte sex = memberData.Sex;
                                                        byte age = memberData.Age;
                                                        string IP = memberData.IP;

                                                        //加入線上玩家列表
                                                        ActorCollection.ActorReturn actorReturn = _server.Actors.ActorOnline(peerGuid, primaryID, account, nickname, age, sex, IP, this);
                                                        Log.Debug("ReturnCode :" + actorReturn.ReturnCode);
                                                        Log.Debug("ReturnMessage :" + actorReturn.DebugMessage);

                                                        if (actorReturn.ReturnCode == "S301")// 加入線上會員資料成功 回傳資料
                                                        {
                                                            Log.Debug("actorReturn.ReturnCode == S301");
                                                            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                                    { (byte)LoginParameterCode.Ret, actorReturn.ReturnCode }, { (byte)LoginParameterCode.PrimaryID, primaryID }, { (byte)LoginParameterCode.Account, account },
                                                    { (byte)LoginParameterCode.Nickname, nickname }, { (byte)LoginParameterCode.Age, age }, { (byte)LoginParameterCode.Sex, sex } , { (byte)LoginParameterCode.MemberType, memberType } 
                                                };

                                                            OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = memberData.ReturnMessage.ToString() };
                                                            SendOperationResponse(actorResponse, new SendParameters());
                                                        }
                                                        else // 登入錯誤 回傳空值
                                                        {
                                                            Log.Debug("SNS加入會員未知錯誤!!");
                                                            Dictionary<byte, object> parameter = new Dictionary<byte, object>{
                                                                { (byte)LoginParameterCode.Ret, actorReturn.ReturnCode }, { (byte)LoginParameterCode.PrimaryID, 0 }, { (byte)LoginParameterCode.Account, "" }, 
                                                                { (byte)LoginParameterCode.Nickname, "" }, { (byte)LoginParameterCode.Age, 0 }, { (byte)LoginParameterCode.Sex, 0 } , { (byte)LoginParameterCode.MemberType, memberType } 
                                                            };

                                                            OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = actorReturn.DebugMessage.ToString() };
                                                            SendOperationResponse(actorResponse, new SendParameters());
                                                            break;
                                                        }
                                                        break;
                                                    }
                                                }
                                            }
                                            else if (memberType == MemberType.Facebook)  // SNS登入失敗 加入會員
                                            {
                                                Log.Debug("MemberType.Facebook");
                                                OperationResponse actorResponse = new OperationResponse((byte)LoginOperationCode.GetProfile) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = memberData.ReturnMessage.ToString() };
                                                SendOperationResponse(actorResponse, new SendParameters());
                                            }
                                            break;
                                        }
                                    #endregion

                                    default: // 無效的登入資訊 無法取得伺服器資料
                                        {
                                            Log.Debug("Code:" + memberData.ReturnCode + "Message:" + memberData.ReturnMessage + "  無效的登入資訊 無法取得伺服器資料!");
                                            OperationResponse response = new OperationResponse(operationRequest.OperationCode) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = memberData.ReturnMessage.ToString() };
                                            SendOperationResponse(response, new SendParameters());
                                            break;
                                        }
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Debug("(S299)Login Failed ! " + e.Message + "  data:" + e.Data + "  Trace:" + e.StackTrace);
                            }
                            break;
                        #endregion

                        #region MatchGame 配對遊戲
                        case (byte)MatchGameOperationCode.MatchGame:
                            Log.Debug("Match Game");
                            primaryID = (int)operationRequest.Parameters[(byte)MatchGameParameterCode.PrimaryID];
                            string myTeam = (string)operationRequest.Parameters[(byte)MatchGameParameterCode.Team];

                            actor = _server.Actors.GetActorFromPrimary(primaryID);  // 用primaryID取得角色資料
                            Log.Debug("waitingList Count:" + _server.room.dictWaitingRoomList.Count);

                            try
                            {
                                if (_server.room.dictWaitingRoomList.Count == 0) //假如等待房間列表中沒房間 建立等待房間
                                {
                                    isCreateRoom = _server.room.CreateRoom(actor.guid, actor.PrimaryID, actor.Account, actor.Nickname, actor.Age, actor.Sex, actor.IP);
                                    Log.Debug("Create Room !" + "  Count:" + _server.room.dictWaitingRoomList.Count);
                                }
                                else //假如等待列表中有等待配對的房間
                                {
                                    if (_server.room.JoinRoom(actor.guid, actor.PrimaryID, actor.Account, actor.Nickname, actor.Age, actor.Sex, actor.IP))
                                    {
                                        Log.Debug("Join MyRoom:" + _server.room.myRoom);
                                        Room.RoomActor otherActor = new Room.RoomActor(actor.guid, actor.PrimaryID, actor.Account, actor.Nickname, actor.Age, actor.Sex, actor.IP); //這裡為了不要再增加變數 所以偷懶使用 actor.XX 正確是沒有actor.

                                        Log.Debug("Count:" + _server.room.dictWaitingRoomList.Count);

                                        int roomID = _server.room.GetRoomFromGuid(peerGuid);
                                        otherActor = _server.room.GetWaitingPlayer(roomID, primaryID);

                                        PlayerDataUI playerDataUI = new PlayerDataUI();
                                        PlayerData otherPlayerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.LoadPlayerData(otherActor.Account));
                                        Log.Debug("otherPlayerData.Team: " + otherPlayerData.Team);
                                        Dictionary<byte, object> myParameter = new Dictionary<byte, object>() { { (byte)MatchGameParameterCode.PrimaryID, otherActor.PrimaryID }, { (byte)MatchGameParameterCode.Nickname, otherActor.Nickname }, { (byte)MatchGameParameterCode.RoomID, _server.room.myRoom }, { (byte)MatchGameParameterCode.Team, myTeam }, { (byte)MatchGameParameterCode.RoomPlace, "Guest" } };
                                        Dictionary<byte, object> otherParameter = new Dictionary<byte, object>() { { (byte)MatchGameParameterCode.PrimaryID, actor.PrimaryID }, { (byte)MatchGameParameterCode.Nickname, actor.Nickname }, { (byte)MatchGameParameterCode.RoomID, _server.room.myRoom }, { (byte)MatchGameParameterCode.Team, otherPlayerData.Team }, { (byte)MatchGameParameterCode.RoomPlace, "Host" } };

                                        // 玩家2加入房間成功後 發送配對成功事件給房間內兩位玩家
                                        EventData matchMyEventData = new EventData((byte)MatchGameResponseCode.Match, myParameter);
                                        EventData matchOthertEventData = new EventData((byte)MatchGameResponseCode.Match, otherParameter);
                                        MPServerPeer peerMe;

                                        //取得雙方玩家的peer
                                        peerMe = _server.Actors.GetPeerFromGuid(this.peerGuid);         // 加入房間的人
                                        peerOther = _server.Actors.GetPeerFromGuid(otherActor.guid);    // 建立房間的人

                                        Log.Debug("Me Peer:" + this.peerGuid + "  " + actor.Nickname);
                                        Log.Debug("Other Peer:" + otherActor.guid + "  " + otherActor.Nickname);

                                        peerMe.SendEvent(matchOthertEventData, new SendParameters());           // 要送給相反的人資料 (RoomActor2)Gueest會收到(RoomActor1)Host資料
                                        peerOther.SendEvent(matchMyEventData, new SendParameters());            // 要送給相反的人資料 (RoomActor1)Host會收到(RoomActor2)Gueest資料

                                        // 移除等待列表房間
                                        _server.room.RemoveWatingRoom();
                                    }
                                    else
                                    {
                                        Log.Error("Join Room Fail !  RoomID: " + _server.room.myRoom);
                                    }
                                }

                            }
                            catch (Exception e)
                            {
                                Log.Debug("Join Failed ! " + e.Message + "  於程式碼:" + e.StackTrace);
                            }
                            break;

                        #endregion

                        #region SyncGameStart 同步開始遊戲
                        case (byte)MatchGameOperationCode.SyncGameStart:
                            {
                                int roomID = (int)operationRequest.Parameters[(byte)MatchGameParameterCode.RoomID];
                                primaryID = (int)operationRequest.Parameters[(byte)MatchGameParameterCode.PrimaryID];
                                bool isloaded;

                                try
                                {
                                    isloaded = _server.room.GetLoadedFromRoom(roomID);

                                    if (!isloaded)
                                    {
                                        _server.room.GameLoaded(roomID, primaryID);

                                        Dictionary<byte, object> parameter = new Dictionary<byte, object>();
                                        OperationResponse response = new OperationResponse((byte)MatchGameResponseCode.WaitingGameStart, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = "WaitingPlayer" };
                                        SendOperationResponse(response, new SendParameters());
                                    }
                                    else
                                    {
                                        Room.RoomActor otherActor = new Room.RoomActor(actor.guid, actor.PrimaryID, actor.Account, actor.Nickname, actor.Age, actor.Sex, actor.IP); //這裡為了不要再增加變數 所以偷懶使用 actor.XX 正確是沒有actor.
                                        otherActor = _server.room.GetOtherPlayer(roomID, primaryID);
                                        peerOther = _server.Actors.GetPeerFromGuid(otherActor.guid);

                                        Log.Debug("actor:" + actor.Nickname + "  guid:" + actor.guid);
                                        Log.Debug("otherActor:" + otherActor.Nickname + "  guid:" + otherActor.guid);

                                        Dictionary<byte, object> parameter = new Dictionary<byte, object>() { { (byte)MatchGameParameterCode.ServerTime, DateTime.Now.ToString("yyyyMMddHHmmss") }, { (byte)MatchGameParameterCode.GameTime, 300 } };
                                        EventData eventData = new EventData((byte)MatchGameResponseCode.SyncGameStart, parameter);
                                        peerOther.SendEvent(eventData, new SendParameters());    // 回傳給另外一位玩家
                                        this.SendEvent(eventData, new SendParameters());         // 回傳給自己
                                    }

                                }
                                catch (Exception e)
                                {
                                    Log.Error("發生例外情況！" + e.Message + "於：" + e.StackTrace);
                                }
                                break;
                            }
                        #endregion

                        #region ExitRoom 離開房間
                        case (byte)BattleOperationCode.ExitRoom:
                            {
                                try
                                {

                                    Log.Debug("IN ExitRoom");

                                    roomID = (int)operationRequest.Parameters[(byte)BattleParameterCode.RoomID];
                                    primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];
                                    Log.Debug("RoomID: " + roomID);
                                    //to do exit room
                                    //Log.Debug("1 playingRoomList Count:" + _server.room.playingRoomList.Count);
                                    //Log.Debug("in Dictionary 1 :" + _server.room.playingRoomList[roomID].ContainsKey("RoomActor1"));
                                    //Log.Debug("in Dictionary 2 :" + _server.room.playingRoomList[roomID].ContainsKey("RoomActor2"));

                                    // 移除BOSS資訊 如果有的話
                                    _server.room.KillBoss(roomID);
                                    // 取得雙方角色
                                    actor = _server.Actors.GetActorFromGuid(peerGuid);
                                    Room.RoomActor otherActor = _server.room.GetOtherPlayer(roomID, actor.PrimaryID);

                                    // 移除遊戲中房間、玩家
                                    if (otherActor != null)
                                        _server.room.RemovePlayingRoom(roomID, actor.guid, otherActor.guid);

                                    Log.Debug("playingRoomList Count:" + _server.room.dictPlayingRoomList.Count);
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> { };

                                    OperationResponse response = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = "已離開房間！" };
                                    SendOperationResponse(response, new SendParameters());

                                }
                                catch (Exception e)
                                {
                                    Log.Debug("ExitRoom Error: " + e.Message + "  Track: " + e.StackTrace);
                                }

                                break;
                            }

                        #endregion

                        #region ExitWaitingRoom 離開等待房間
                        case (byte)MatchGameOperationCode.ExitWaiting:
                            {
                                try
                                {
                                    Log.Debug("IN ExitWaitingRoom");

                                    _server.room.RemoveWatingRoom();    // 移除等待房間
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object>();

                                    // 等不到人 離開房間吧
                                    OperationResponse response = new OperationResponse((byte)MatchGameResponseCode.ExitWaiting, parameter) { DebugMessage = "等待超時，已離開房間！" };
                                    SendOperationResponse(response, new SendParameters());

                                }
                                catch (Exception e)
                                {
                                    Log.Debug("ExitRoom Error: " + e.Message + "  Track: " + e.StackTrace);
                                }

                                break;
                            }

                        #endregion


                        #region Logout 登出
                        case (byte)LoginOperationCode.Logout:
                            {
                                try
                                {
                                    OnDisconnect(new DisconnectReason(), "Logout");
                                }
                                catch (Exception e)
                                {
                                    Log.Debug("ExitRoom Error: " + e.Message + "  Track: " + e.StackTrace);
                                }

                                break;
                            }

                        #endregion

                        #region KickOther 踢人
                        case (byte)BattleOperationCode.KickOther:
                            {
                                try
                                {
                                    Log.Debug("IN KickOther");

                                    roomID = (int)operationRequest.Parameters[(byte)BattleParameterCode.RoomID];
                                    primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];
                                    Log.Debug("RoomID: " + roomID);
                                    //to do exit room
                                    Room.RoomActor otherActor = new Room.RoomActor(actor.guid, actor.PrimaryID, actor.Account, actor.Nickname, actor.Age, actor.Sex, actor.IP); //這裡為了不要再增加變數 所以偷懶使用 actor.XX 正確是沒有actor.
                                    //Log.Debug("playingRoomList Count:" + room.playingRoomList.Count);
                                    //Log.Debug("room.playingRoomList[roomID] Vaule:" + room.playingRoomList[roomID].Values.Count);

                                    // 取得對方玩家>用GUID找Peer
                                    otherActor = _server.room.GetOtherPlayer(roomID, primaryID);
                                    if (otherActor != null)
                                    {
                                        peerOther = _server.Actors.GetPeerFromGuid(otherActor.guid);

                                        // 把他給踢了
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object>() { { (byte)BattleResponseCode.DebugMessage, "配對的玩家已經離開房間！" } };
                                        EventData exitEventData = new EventData((byte)BattleResponseCode.KickOther, parameter);
                                        peerOther.SendEvent(exitEventData, new SendParameters());
                                    }
                                }
                                catch (Exception e)
                                {
                                    Log.Debug("ExitRoom Error: " + e.Message + "  Track: " + e.StackTrace);
                                }

                                break;
                            }

                        #endregion

                        #region CheckStatus 檢查遊戲狀態
                        case (byte)BattleOperationCode.CheckStatus:
                            {
                                try
                                {
                                    Log.Debug("IN CheckStatus");

                                    roomID = (int)operationRequest.Parameters[(byte)BattleParameterCode.RoomID];
                                    primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];

                                    Room.RoomActor otherActor = new Room.RoomActor(actor.guid, actor.PrimaryID, actor.Account, actor.Nickname, actor.Age, actor.Sex, actor.IP); //這裡為了不要再增加變數 所以偷懶使用 actor.XX 正確是沒有actor.
                                    MPServerPeer peer;

                                    otherActor = _server.room.GetOtherPlayer(roomID, primaryID);    // 取得其他玩家

                                    //Log.Debug("otherActor : " + otherActor);
                                    if (otherActor == null) // 如果他跑了 我也玩不了 離開房間
                                    {
                                        peer = _server.Actors.GetPeerFromGuid(peerGuid);
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object>() { { (byte)BattleResponseCode.DebugMessage, "配對的玩家已經離開房間！" } };
                                        EventData eventData = new EventData((byte)BattleResponseCode.Offline, parameter);
                                        peer.SendEvent(eventData, new SendParameters());
                                    }
                                }
                                catch (Exception e)
                                {
                                    Log.Error("CheckStatus Error: " + e.Message + "  Track: " + e.StackTrace);
                                }

                                break;
                            }

                        #endregion

                        #region SendSkillMice 發動老鼠技能傷害
                        case (byte)BattleOperationCode.SendSkillMice:
                            {
                                Log.Debug("IN SendSkill");

                                short miceID = (short)operationRequest.Parameters[(byte)BattleParameterCode.MiceID];
                                roomID = (int)operationRequest.Parameters[(byte)BattleParameterCode.RoomID];
                                primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];

                                Room.RoomActor otherActor;
                                otherActor = _server.room.GetOtherPlayer(roomID, primaryID);    // 找對手
                                peerOther = _server.Actors.GetPeerFromGuid(otherActor.guid);    // 對手GUID找Peer

                                // 送給對手 技能傷害
                                Dictionary<byte, object> skillParameter = new Dictionary<byte, object>() { { (byte)BattleParameterCode.MiceID, miceID }, { (byte)BattleResponseCode.DebugMessage, "" } };
                                EventData skillEventData = new EventData((byte)BattleResponseCode.ApplySkillMice, skillParameter);
                                peerOther.SendEvent(skillEventData, new SendParameters());
                            }
                            break;
                        #endregion

                        #region SendSkillItem 發動道具技能傷害
                        case (byte)BattleOperationCode.SendSkillItem:
                            {


                                short itemID = (short)operationRequest.Parameters[(byte)ItemParameterCode.ItemID];
                                short skillType = (short)operationRequest.Parameters[(byte)SkillParameterCode.SkillType];
                                roomID = (int)operationRequest.Parameters[(byte)BattleParameterCode.RoomID];
                                primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];

                                Log.Debug("IN SendSkillItem ID: " + itemID + "   Type: " + skillType);

                                Dictionary<byte, object> skillParameter = new Dictionary<byte, object>() { { (byte)ItemParameterCode.ItemID, itemID }, { (byte)BattleResponseCode.DebugMessage, "" } };
                                EventData skillEventData = new EventData((byte)BattleResponseCode.ApplySkillItem, skillParameter);

                                // 如果是傷害技能 傳送給對手
                                if ((ENUM_SkillType)skillType == ENUM_SkillType.Damage)
                                {
                                    Room.RoomActor otherActor;
                                    otherActor = _server.room.GetOtherPlayer(roomID, primaryID);    // 找對手
                                    peerOther = _server.Actors.GetPeerFromGuid(otherActor.guid);    // 對手GUID找Peer

                                    // 送給對手 技能傷害
                                    peerOther.SendEvent(skillEventData, new SendParameters());
                                }
                                else
                                {
                                    // 回傳給自己
                                    this.SendEvent(skillEventData, new SendParameters());
                                }
                            }
                            break;
                        #endregion

                        #region SkillBoss 發動技能BOSS老鼠技能
                        case (byte)BattleOperationCode.SkillBoss:
                            {
                                Log.Debug("IN Send BOSS Skill");


                                roomID = (int)operationRequest.Parameters[(byte)BattleParameterCode.RoomID];
                                primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];
                                int skill = (int)operationRequest.Parameters[(byte)SkillParameterCode.SkillID];
                                bool self = (bool)operationRequest.Parameters[(byte)BattleParameterCode.CustomValue];

                                Dictionary<byte, object> skillParameter = new Dictionary<byte, object>() { { (byte)SkillParameterCode.SkillID, skill }, { (byte)BattleResponseCode.DebugMessage, "接收BOSS技能!" } };

                                if (self)
                                {
                                    OperationResponse response = new OperationResponse((byte)BattleResponseCode.SkillBoss, skillParameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = "接收BOSS技能!" };
                                    SendOperationResponse(response, new SendParameters());
                                }
                                else
                                {
                                    Room.RoomActor otherActor;
                                    otherActor = _server.room.GetOtherPlayer(roomID, primaryID);    // 找對手
                                    peerOther = _server.Actors.GetPeerFromGuid(otherActor.guid);    // 對手GUID找Peer

                                    // 送給對手 技能傷害

                                    EventData skillEventData = new EventData((byte)BattleResponseCode.SkillBoss, skillParameter);
                                    peerOther.SendEvent(skillEventData, new SendParameters());
                                }
                            }
                            break;
                        #endregion


                        #region LoadPlayerData 載入玩家資料
                        case (byte)PlayerDataOperationCode.LoadPlayer:
                            {
                                Log.Debug("IN LoadPlayerData");

                                string account = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Account];

                                PlayerDataUI playerDataUI = new PlayerDataUI(); //初始化 IO (連結資料庫拿資料)
                                Log.Debug("IO OK");
                                PlayerData playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.LoadPlayerData(account)); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找
                                Log.Debug("Data OK");

                                byte rank = playerData.Rank;
                                byte exp = playerData.EXP;
                                Int16 maxCombo = playerData.MaxCombo;
                                Int32 maxScore = playerData.MaxScore;
                                Int32 sumScore = playerData.SumScore;
                                Int16 sumLost = playerData.SumLost;
                                Int32 sumKill = playerData.SumKill;
                                int sumWin = playerData.SumWin;
                                int sumBattle = playerData.SumBattle;

                                string item = playerData.SortedItem;                  // Json資料
                                string miceAll = playerData.MiceAll;            // Json資料
                                string team = playerData.Team;                  // Json資料
                                string friend = playerData.Friend;              // Json資料

                                if (playerData.ReturnCode == "S401")//取得玩家資料成功 回傳玩家資料
                                {
                                    Log.Debug("playerData.ReturnCode == S401");
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)PlayerDataParameterCode.Ret, playerData.ReturnCode }, { (byte)PlayerDataParameterCode.Rank, rank }, { (byte)PlayerDataParameterCode.EXP, exp }, 
                                        { (byte)PlayerDataParameterCode.MaxCombo, maxCombo }, { (byte)PlayerDataParameterCode.MaxScore, maxScore }, { (byte)PlayerDataParameterCode.SumScore, sumScore } ,
                                        { (byte)PlayerDataParameterCode.SumLost, sumLost } ,{ (byte)PlayerDataParameterCode.SumKill, sumKill },{ (byte)PlayerDataParameterCode.SumWin, sumWin },
                                        { (byte)PlayerDataParameterCode.SumBattle, sumBattle },{ (byte)PlayerDataParameterCode.SortedItem, item } ,{ (byte)PlayerDataParameterCode.MiceAll, miceAll } ,
                                        { (byte)PlayerDataParameterCode.Team, team },{ (byte)PlayerDataParameterCode.Friend, friend } 
                                    };

                                    OperationResponse actorResponse = new OperationResponse((byte)PlayerDataResponseCode.LoadedPlayer, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = playerData.ReturnMessage.ToString() };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }
                                else // 失敗 傳空值+錯誤訊息
                                {
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                    OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = playerData.ReturnMessage.ToString() };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }
                            }
                            break;
                        #endregion

                        #region UpdatePlayerData 更新玩家資料
                        case (byte)PlayerDataOperationCode.UpdatePlayer:
                            {
                                Log.Debug("IN UpdatePlayerData");

                                string account = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Account];
                                byte rank = (byte)operationRequest.Parameters[(byte)PlayerDataParameterCode.Rank];
                                byte exp = (byte)operationRequest.Parameters[(byte)PlayerDataParameterCode.EXP];
                                Int16 maxCombo = (Int16)operationRequest.Parameters[(byte)PlayerDataParameterCode.MaxCombo];
                                int maxScore = (int)operationRequest.Parameters[(byte)PlayerDataParameterCode.MaxScore];
                                int sumScore = (int)operationRequest.Parameters[(byte)PlayerDataParameterCode.SumScore];
                                Int16 sumLost = (Int16)operationRequest.Parameters[(byte)PlayerDataParameterCode.SumLost];
                                int sumKill = (int)operationRequest.Parameters[(byte)PlayerDataParameterCode.SumKill];

                                string item = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.SortedItem];              // Json資料
                                string miceAll = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.MiceAll];        // Json資料
                                string team = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Team];              // Json資料
                                string friend = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Friend];          // Json資料

                                PlayerDataUI playerDataUI = new PlayerDataUI(); //實體化 IO (連結資料庫拿資料)
                                Log.Debug("IO OK");
                                PlayerData playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.UpdatePlayerData(account, rank, exp, maxCombo, maxScore, sumScore, sumLost, sumKill, item, miceAll, team, friend)); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找
                                Log.Debug("Data OK");

                                if (playerData.ReturnCode == "S403")//取得玩家資料成功 回傳玩家資料
                                {
                                    Log.Debug("playerData.ReturnCode == S403");
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)PlayerDataParameterCode.Ret, playerData.ReturnCode }, { (byte)PlayerDataParameterCode.Rank, rank }, { (byte)PlayerDataParameterCode.EXP, exp }, 
                                        { (byte)PlayerDataParameterCode.MaxCombo, maxCombo }, { (byte)PlayerDataParameterCode.MaxScore, maxScore }, { (byte)PlayerDataParameterCode.SumScore, sumScore } ,
                                        { (byte)PlayerDataParameterCode.MiceAll, miceAll } , { (byte)PlayerDataParameterCode.Team, team } , 
                                        { (byte)PlayerDataParameterCode.Friend, friend } 
                                    };

                                    OperationResponse actorResponse = new OperationResponse((byte)PlayerDataResponseCode.UpdatedPlayer, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = playerData.ReturnMessage.ToString() };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }
                                else// 失敗 傳空值+錯誤訊息
                                {
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                    OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = playerData.ReturnMessage.ToString() };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }

                            }
                            break;
                        #endregion

                        #region UpdateMiceData 更新玩家(Team)資料
                        case (byte)PlayerDataOperationCode.UpdateMice:
                            {
                                Log.Debug("IN UpdateMiceData");

                                string account = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Account];
                                string miceAll = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.MiceAll];        // Json資料
                                string team = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Team];              // Json資料

                                PlayerDataUI playerDataUI = new PlayerDataUI(); //實體化 IO (連結資料庫拿資料)
                                Log.Debug("IO OK");
                                PlayerData playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.UpdatePlayerData(account, miceAll, team)); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找
                                Log.Debug("Data OK");

                                if (playerData.ReturnCode == "S420")//取得玩家資料成功 回傳玩家資料
                                {
                                    Log.Debug("playerData.ReturnCode == S420");
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)PlayerDataParameterCode.Ret, playerData.ReturnCode },{ (byte)PlayerDataParameterCode.MiceAll, miceAll } , { (byte)PlayerDataParameterCode.Team, team }
                                        };


                                    OperationResponse actorResponse = new OperationResponse((byte)PlayerDataResponseCode.UpdatedMice, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = playerData.ReturnMessage.ToString() };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }
                                else// 失敗 傳空值+錯誤訊息
                                {
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                    OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = playerData.ReturnMessage.ToString() };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }

                            }
                            break;
                        #endregion

                        #region UpdatePlayerItem 更新玩家道具資料
                        case (byte)PlayerDataOperationCode.UpdateItem:
                            {
                                Log.Debug("IN UpdatePlayerData");

                                string account = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Account];

                                PlayerDataUI playerDataUI = new PlayerDataUI(); //實體化 IO (連結資料庫拿資料)
                                PlayerData playerData = new PlayerData();

                                if (operationRequest.Parameters.Count == 3)
                                { // 3個參數=更新裝備狀態
                                    bool isEquip = (bool)operationRequest.Parameters[(byte)PlayerDataParameterCode.Equip];
                                    Int16 itemID = (Int16)operationRequest.Parameters[(byte)PlayerDataParameterCode.SortedItem];
                                    playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.UpdatePlayerItem(account, itemID, isEquip)); // 更新裝備狀態
                                }
                                else// 更新數量
                                {
                                    string jItemUsage = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.UseCount];
                                    Log.Debug("jItemUsage:  " + jItemUsage);
                                    playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.UpdatePlayerItem(account, jItemUsage)); // 更新道具數量
                                }

                                if (playerData.ReturnCode == "S422")//取得玩家資料成功 回傳玩家資料
                                {
                                    Log.Debug("playerData.ReturnCode == S422");

                                    playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.LoadPlayerItem(account)); // 更新道具數量

                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                            { (byte)PlayerDataParameterCode.Ret, playerData.ReturnCode }, { (byte)PlayerDataParameterCode.PlayerItem, playerData.PlayerItem }, 
                                         };

                                    OperationResponse actorResponse = new OperationResponse((byte)PlayerDataResponseCode.UpdatedItem, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = playerData.ReturnMessage.ToString() };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }
                                else// 失敗 傳空值+錯誤訊息
                                {
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                    OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = playerData.ReturnMessage.ToString() };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }

                            }
                            break;
                        #endregion

                        #region UpdatePlayerItem 更新玩家道具資料
                        case (byte)PlayerDataOperationCode.SortItem:
                            {
                                Log.Debug("IN UpdatePlayerData");
                                PlayerDataUI playerDataUI = new PlayerDataUI(); //實體化 IO (連結資料庫拿資料)
                                PlayerData playerData = new PlayerData();

                                string account = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Account];
                                string item = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.SortedItem];
                                Log.Debug(item);
                                playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.SortPlayerItem(account, item)); // 更新排序狀態

                                if (playerData.ReturnCode == "S428")//取得玩家資料成功 回傳玩家資料
                                {
                                    Log.Debug("playerData.ReturnCode == S428");

                                    playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.LoadPlayerData(account)); // 更新道具數量
                                    Log.Debug(playerData.SortedItem);
                                    if (playerData.ReturnCode == "S401")//取得玩家資料成功 回傳玩家資料
                                    {
                                        Log.Debug("playerData.ReturnCode == S401");
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                            { (byte)PlayerDataParameterCode.Ret, playerData.ReturnCode }, { (byte)PlayerDataParameterCode.SortedItem, playerData.SortedItem }, 
                                         };

                                        OperationResponse actorResponse = new OperationResponse((byte)PlayerDataResponseCode.SortedItem, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = playerData.ReturnMessage };
                                        SendOperationResponse(actorResponse, new SendParameters());
                                    }
                                    else// 失敗 傳空值+錯誤訊息
                                    {
                                        Log.Debug(playerData.ReturnCode + "  " + playerData.ReturnMessage);
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                        OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = playerData.ReturnMessage };
                                        SendOperationResponse(actorResponse, new SendParameters());
                                    }
                                }
                                else// 失敗 傳空值+錯誤訊息
                                {
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                    OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = playerData.ReturnMessage };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }

                            }
                            break;
                        #endregion

                        #region UpdateScoreRate 更新分數倍率
                        case (byte)BattleOperationCode.UpdateScoreRate:
                            {
                                Log.Debug("IN UpdateScoreRate");

                                string account = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Account];
                                int rate = (int)operationRequest.Parameters[(byte)BattleParameterCode.ScoreRate];

                                BattleUI battleUI = new BattleUI();
                                BattleData battleData = (BattleData)TextUtility.DeserializeFromStream(battleUI.UpdateScoreRate((ENUM_Rate)rate));

                                Room.RoomActor rActor = _server.room.GetActorFromGuid(peerGuid);
                                rActor.scoreRate = battleData.scoreRate;

                                if (battleData.ReturnCode == "S508")
                                {
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)BattleParameterCode.Ret, battleData.ReturnCode }, { (byte)BattleParameterCode.ScoreRate, battleData.scoreRate }
                                    };
                                    OperationResponse response = new OperationResponse((byte)BattleResponseCode.UpdatedScoreRate, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = battleData.ReturnMessage };
                                    SendOperationResponse(response, new SendParameters());
                                }
                                else  // 失敗 傳空值+錯誤訊息
                                {
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                    OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = battleData.ReturnMessage };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }
                            }
                            break;
                        #endregion

                        #region UpdateEnergyRate 更新能量倍率
                        case (byte)BattleOperationCode.UpdateEnergyRate:
                            {
                                Log.Debug("IN UpdateEnergyRate");

                                string account = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Account];
                                int rate = (int)operationRequest.Parameters[(byte)BattleParameterCode.EnergyRate];

                                BattleUI battleUI = new BattleUI();
                                BattleData battleData = (BattleData)TextUtility.DeserializeFromStream(battleUI.UpdateEnergyRate((ENUM_Rate)rate));

                                Room.RoomActor rActor = _server.room.GetActorFromGuid(peerGuid);
                                rActor.energyRate = battleData.energyRate;

                                if (battleData.ReturnCode == "S508")
                                {
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)BattleParameterCode.Ret, battleData.ReturnCode }, { (byte)BattleParameterCode.EnergyRate, battleData.energyRate }
                                    };
                                    OperationResponse response = new OperationResponse((byte)BattleResponseCode.UpdatedEnergyRate, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = battleData.ReturnMessage };
                                    SendOperationResponse(response, new SendParameters());
                                }
                                else  // 失敗 傳空值+錯誤訊息
                                {
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                    OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = battleData.ReturnMessage };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }
                            }
                            break;
                        #endregion

                        #region LoadCurrency 載入貨幣資料
                        case (byte)CurrencyOperationCode.Load:
                            {
                                Log.Debug("IN LoadCurrency");

                                string account = (string)operationRequest.Parameters[(byte)CurrencyParameterCode.Account];
                                Log.Debug("Account:" + account);
                                CurrencyUI currencyUI = new CurrencyUI(); //實體化 IO (連結資料庫拿資料)
                                Log.Debug("IO OK");
                                CurrencyData currencyData = (CurrencyData)TextUtility.DeserializeFromStream(currencyUI.LoadCurrency(account));
                                Log.Debug("Data OK");

                                int rice = currencyData.Rice;
                                Int16 gold = currencyData.Gold;

                                if (currencyData.ReturnCode == "S701")  // 取得遊戲貨幣成功 回傳玩家資料
                                {
                                    Log.Debug("currencyData.ReturnCode == S701");
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)CurrencyParameterCode.Ret, currencyData.ReturnCode }, { (byte)CurrencyParameterCode.Rice, rice }, { (byte)CurrencyParameterCode.Gold, gold } 
                                    };

                                    OperationResponse response = new OperationResponse((byte)CurrencyResponseCode.Loaded, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = currencyData.ReturnMessage.ToString() };
                                    SendOperationResponse(response, new SendParameters());
                                }
                                else  // 失敗 傳空值+錯誤訊息
                                {
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                    OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = currencyData.ReturnMessage.ToString() };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }
                            }
                            break;
                        #endregion

                        #region LoadRice 載入遊戲幣資料
                        case (byte)CurrencyOperationCode.LoadRice:
                            {
                                Log.Debug("IN LoadRice");

                                string account = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Account];

                                CurrencyUI currencyUI = new CurrencyUI(); //實體化 IO (連結資料庫拿資料)
                                Log.Debug("IO OK");
                                CurrencyData currencyData = (CurrencyData)TextUtility.DeserializeFromStream(currencyUI.LoadCurrency(account)); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找
                                Log.Debug("Data OK");

                                int rice = currencyData.Rice;

                                if (currencyData.ReturnCode == "S703")//取得遊戲幣成功 回傳玩家資料
                                {
                                    Log.Debug("currencyData.ReturnCode == S703");
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)CurrencyParameterCode.Ret, currencyData.ReturnCode }, { (byte)CurrencyParameterCode.Rice, rice }
                                    };

                                    OperationResponse response = new OperationResponse((byte)CurrencyResponseCode.Loaded, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = currencyData.ReturnMessage.ToString() };
                                    SendOperationResponse(response, new SendParameters());
                                }
                                else// 失敗
                                {
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                    OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = currencyData.ReturnMessage.ToString() };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }

                            }
                            break;
                        #endregion

                        #region LoadGold 載入金幣資料
                        case (byte)CurrencyOperationCode.LoadGold:
                            {
                                Log.Debug("IN LoadGold");

                                string account = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Account];

                                CurrencyUI currencyUI = new CurrencyUI(); //實體化 IO (連結資料庫拿資料)
                                Log.Debug("IO OK");
                                CurrencyData currencyData = (CurrencyData)TextUtility.DeserializeFromStream(currencyUI.LoadCurrency(account));
                                Log.Debug("Data OK");

                                Int16 gold = currencyData.Gold;

                                if (currencyData.ReturnCode == "S705")//取得金幣成功 回傳玩家資料
                                {
                                    Log.Debug("currencyData.ReturnCode == S705");
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)CurrencyParameterCode.Ret, currencyData.ReturnCode }, { (byte)CurrencyParameterCode.Gold, gold } 
                                    };

                                    OperationResponse response = new OperationResponse((byte)CurrencyResponseCode.Loaded, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = currencyData.ReturnMessage.ToString() };
                                    SendOperationResponse(response, new SendParameters());
                                }
                                else    // 失敗
                                {
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                    OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = currencyData.ReturnMessage.ToString() };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }

                            }
                            break;
                        #endregion

                        #region UpdateScore 更新分數 還沒寫好
                        case (byte)BattleOperationCode.UpdateScore:
                            {
                                try
                                {
                                    //Log.Debug("IN UpdateScore");

                                    primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];
                                    roomID = (int)operationRequest.Parameters[(byte)BattleParameterCode.RoomID];

                                    short miceID = (short)operationRequest.Parameters[(byte)BattleParameterCode.MiceID];
                                    float aliveTime = (float)operationRequest.Parameters[(byte)BattleParameterCode.Time];

                                    Room.RoomActor roomActor = _server.room.GetActorFromGuid(peerGuid);
                                    if (roomActor != null)
                                    {

                                        BattleUI battleUI = new BattleUI(); //初始化 UI 
                                        Log.Debug("BattleUI OK:" + miceID + "actor:" + _server.room.GetActorFromGuid(peerGuid).Nickname);
                                        BattleData battleData = (BattleData)TextUtility.DeserializeFromStream(battleUI.ClacScore(miceID, aliveTime, roomActor.scoreRate, roomActor.energyRate)); //計算分數
                                        Log.Debug("BattleData OK : " + battleData.score);


                                        // score = battleData.score;

                                        if (battleData.ReturnCode == "S501")//計算分數成功 回傳玩家資料
                                        {
                                            Log.Debug("peerGuid:" + peerGuid);
                                            Log.Debug("roomActor Nickname:" + roomActor.Nickname + "  " + roomActor.gameScore);

                                            roomActor.gameScore += battleData.score;
                                            roomActor.energy += battleData.energy;
                                            if (roomActor.gameScore < 0) roomActor.gameScore = 0;

                                            //回傳給原玩家
                                            //Log.Debug("battleData.ReturnCode == S501");
                                            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)BattleParameterCode.Ret, battleData.ReturnCode }, { (byte)BattleParameterCode.Score, battleData.score } , { (byte)BattleParameterCode.Energy, battleData.energy} 
                                    };


                                            OperationResponse response = new OperationResponse((byte)BattleResponseCode.UpdateScore, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = battleData.ReturnMessage.ToString() }; // 改過 battleData.ReturnMessage.ToString()
                                            SendOperationResponse(response, new SendParameters());

                                            //回傳給另外一位玩家
                                            Room.RoomActor otherActor = new Room.RoomActor(actor.guid, actor.PrimaryID, actor.Account, actor.Nickname, actor.Age, actor.Sex, actor.IP); //這裡為了不要再增加變數 所以偷懶使用 actor.XX 正確是沒有actor.
                                            otherActor = _server.room.GetOtherPlayer(roomID, primaryID);
                                            peerOther = _server.Actors.GetPeerFromGuid(otherActor.guid);
                                            Dictionary<byte, object> parameter2 = new Dictionary<byte, object>() { { (byte)BattleParameterCode.OtherScore, battleData.score }, { (byte)BattleParameterCode.Energy, battleData.energy }, { (byte)BattleResponseCode.DebugMessage, "取得對方分數資料" } };
                                            EventData getScoreEventData = new EventData((byte)BattleResponseCode.GetScore, parameter2);

                                            peerOther.SendEvent(getScoreEventData, new SendParameters());
                                        }
                                        else
                                        {
                                            Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                            OperationResponse actorResponse = new OperationResponse((byte)BattleResponseCode.UpdateScore, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = battleData.ReturnMessage.ToString() };
                                            SendOperationResponse(actorResponse, new SendParameters());
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Log.Debug("發生例外情況: " + e.Message + " 於: " + e.StackTrace);
                                }

                            }
                            break;
                        #endregion

                        #region LoadMice 載入老鼠資料
                        case (byte)MiceOperationCode.LoadMice:
                            {
                                try
                                {
                                    Log.Debug("IN LoadMice");

                                    MiceDataUI miceDataUI = new MiceDataUI(); //實體化 IO (連結資料庫拿資料)
                                    Log.Debug("LoadMice IO OK");
                                    MiceData miceData = (MiceData)TextUtility.DeserializeFromStream(miceDataUI.LoadMiceData()); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找
                                    Log.Debug("LoadMice Data OK");
                                    //Log.Debug("Server Data: " + miceData.miceProperty);
                                    if (miceData.ReturnCode == "S801")//取得老鼠資料成功 回傳玩家資料
                                    {
                                        Log.Debug("miceData.ReturnCode == S801");
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)MiceParameterCode.Ret, miceData.ReturnCode }, { (byte)MiceParameterCode.MiceData,miceData.miceProperty } 
                                    };

                                        OperationResponse response = new OperationResponse((byte)MiceResponseCode.LoadMice, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = miceData.ReturnMessage.ToString() };
                                        SendOperationResponse(response, new SendParameters());
                                    }
                                    else    // 失敗
                                    {
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                        OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = miceData.ReturnMessage.ToString() };
                                        SendOperationResponse(actorResponse, new SendParameters());
                                    }
                                }
                                catch (Exception e)
                                {
                                    Log.Debug("例外情況: " + e.Message + "於： " + e.StackTrace);
                                }

                            }
                            break;
                        #endregion

                        #region LoadMice 載入老鼠資料
                        case (byte)SkillOperationCode.LoadSkill:
                            {
                                try
                                {
                                    Log.Debug("IN LoadSkill");

                                    SkillUI skillUI = new SkillUI(); //實體化 IO (連結資料庫拿資料)
                                    Log.Debug("LoadSkill IO OK");
                                    SkillData skillData = (SkillData)TextUtility.DeserializeFromStream(skillUI.LoadSkillProperty()); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找
                                    Log.Debug("LoadSkill Data OK");
                                    //Log.Debug("Server Data: " + miceData.miceProperty);
                                    if (skillData.ReturnCode == "S1001")//取得老鼠資料成功 回傳玩家資料
                                    {
                                        Log.Debug("skillData.ReturnCode == S1001");
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)MiceParameterCode.Ret, skillData.ReturnCode }, { (byte)MiceParameterCode.MiceData,skillData.skillProperty } 
                                    };

                                        OperationResponse response = new OperationResponse((byte)SkillResponseCode.LoadSkill, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = skillData.ReturnMessage.ToString() };
                                        SendOperationResponse(response, new SendParameters());
                                    }
                                    else    // 失敗
                                    {
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                        OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = skillData.ReturnMessage.ToString() };
                                        SendOperationResponse(actorResponse, new SendParameters());
                                    }
                                }
                                catch (Exception e)
                                {
                                    Log.Debug("例外情況: " + e.Message + "於： " + e.StackTrace);
                                }

                            }
                            break;
                        #endregion

                        #region LoadStore 載入商店資料
                        case (byte)StoreOperationCode.LoadStore:
                            {
                                try
                                {
                                    Log.Debug("IN LoadStore");

                                    StoreDataUI storeDataUI = new StoreDataUI(); //實體化 IO (連結資料庫拿資料)
                                    Log.Debug("LoadStore IO OK");
                                    StoreData storeData = (StoreData)TextUtility.DeserializeFromStream(storeDataUI.LoadStoreData()); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找
                                    Log.Debug("LoadStore Data OK");
                                    //Log.Debug("Server Data: " + storeData.StoreItem);
                                    if (storeData.ReturnCode == "S901")//取得老鼠資料成功 回傳玩家資料
                                    {
                                        Log.Debug("LoadStore.ReturnCode == S901");
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)StoreParameterCode.Ret, storeData.ReturnCode }, { (byte)StoreParameterCode.StoreData,storeData.StoreItem } 
                                    };

                                        OperationResponse response = new OperationResponse((byte)StoreResponseCode.LoadStore, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = storeData.ReturnMessage };
                                        SendOperationResponse(response, new SendParameters());
                                    }
                                    else    // 失敗
                                    {
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                        OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = storeData.ReturnMessage };
                                        SendOperationResponse(actorResponse, new SendParameters());
                                    }
                                }
                                catch (Exception e)
                                {
                                    Log.Debug("例外情況: " + e.Message + "於： " + e.StackTrace);
                                }

                            }
                            break;
                        #endregion

                        #region LoadItem 載入道具資料
                        case (byte)ItemOperationCode.LoadItem:
                            {
                                try
                                {
                                    Log.Debug("IN LoadItem");

                                    ItemUI ItemUI = new ItemUI(); //實體化 IO (連結資料庫拿資料)
                                    Log.Debug("LoadItem IO OK");
                                    ItemData itemData = (ItemData)TextUtility.DeserializeFromStream(ItemUI.LoadItemData()); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找
                                    Log.Debug("LoadItem Data OK");
                                    //Log.Debug("Server Data: " + itemData.itemProperty);
                                    if (itemData.ReturnCode == "S601")//取得老鼠資料成功 回傳玩家資料
                                    {
                                        Log.Debug("LoadItem.ReturnCode == S601");
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)ItemParameterCode.Ret, itemData.ReturnCode }, { (byte)ItemParameterCode.ItemData,itemData.itemProperty } 
                                    };

                                        OperationResponse response = new OperationResponse((byte)ItemResponseCode.LoadItem, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = itemData.ReturnMessage.ToString() };
                                        SendOperationResponse(response, new SendParameters());
                                    }
                                    else    // 失敗
                                    {
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                        OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = itemData.ReturnMessage.ToString() };
                                        SendOperationResponse(actorResponse, new SendParameters());
                                    }
                                }
                                catch (Exception e)
                                {
                                    Log.Debug("例外情況: " + e.Message + "於： " + e.StackTrace);
                                }

                            }
                            break;
                        #endregion

                        #region LoadItem 載入玩家道具資料
                        case (byte)PlayerDataOperationCode.LoadItem:
                            {
                                try
                                {
                                    Log.Debug("IN LoadPlayerItem");
                                    string account = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Account];
                                    PlayerDataUI palyerDataUI = new PlayerDataUI(); //實體化 IO (連結資料庫拿資料)
                                    Log.Debug("LoadPlayerItem IO OK");
                                    PlayerData playerData = (PlayerData)TextUtility.DeserializeFromStream(palyerDataUI.LoadPlayerItem(account)); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找
                                    Log.Debug("LoadPlayerItem Data OK");
                                    //Log.Debug("Server Data: " + playerData.PlayerItem);
                                    if (playerData.ReturnCode == "S425")//取得老鼠資料成功 回傳玩家資料
                                    {
                                        Log.Debug("LoadPlayerItem.ReturnCode == S425");
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)PlayerDataParameterCode.Ret, playerData.ReturnCode }, { (byte)PlayerDataParameterCode.PlayerItem,playerData.PlayerItem } 
                                            };

                                        OperationResponse response = new OperationResponse((byte)PlayerDataResponseCode.LoadedItem, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = playerData.ReturnMessage.ToString() };
                                        SendOperationResponse(response, new SendParameters());
                                    }
                                    else    // 失敗
                                    {
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                        OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = playerData.ReturnMessage.ToString() };
                                        SendOperationResponse(actorResponse, new SendParameters());
                                    }
                                }
                                catch (Exception e)
                                {
                                    Log.Debug("例外情況: " + e.Message + "於： " + e.StackTrace);
                                }

                            }
                            break;
                        #endregion

                        #region Mission 發送任務
                        case (byte)BattleOperationCode.Mission:
                            {
                                try
                                {
                                    Log.Debug("IN Mission");

                                    primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];
                                    roomID = (int)operationRequest.Parameters[(byte)BattleParameterCode.RoomID];
                                    byte mission = (byte)operationRequest.Parameters[(byte)BattleParameterCode.Mission];
                                    float missionRate = (float)operationRequest.Parameters[(byte)BattleParameterCode.MissionRate];


                                    BattleUI battleUI = new BattleUI();
                                    BattleData battleData = (BattleData)TextUtility.DeserializeFromStream(battleUI.SelectMission(mission, missionRate));
                                    Int16 missionScore = battleData.missionScore;

                                    if (mission == (byte)Mission.WorldBoss)
                                    {
                                        _server.room.SpawnBoss(roomID, missionScore);  // 產生BOSS血量 missionScore是BOSS HP
                                    }

                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)BattleParameterCode.Mission, mission }, { (byte)BattleParameterCode.MissionScore, missionScore } };

                                    OperationResponse response = new OperationResponse((byte)BattleResponseCode.Mission, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = "Recive Mission" };

                                    Room.RoomActor otherActor = new Room.RoomActor(actor.guid, actor.PrimaryID, actor.Account, actor.Nickname, actor.Age, actor.Sex, actor.IP); //這裡為了不要再增加變數 所以偷懶使用 actor.XX 正確是沒有actor.
                                    otherActor = _server.room.GetOtherPlayer(roomID, primaryID);
                                    peerOther = _server.Actors.GetPeerFromGuid(otherActor.guid);
                                    EventData missionEventData = new EventData((byte)BattleResponseCode.Mission, parameter);

                                    peerOther.SendEvent(missionEventData, new SendParameters());    // 回傳給另外一位玩家
                                    SendOperationResponse(response, new SendParameters());          // 回傳
                                }
                                catch (Exception e)
                                {
                                    Log.Debug("發生例外情況: " + e.Message + " 於: " + e.StackTrace);
                                }
                                break;
                            }
                        #endregion

                        #region MissionCompleted 任務完成 回傳獎勵
                        case (byte)BattleOperationCode.MissionCompleted:
                            {
                                try
                                {
                                    Log.Debug("IN MissionCompleted");

                                    primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];
                                    roomID = (int)operationRequest.Parameters[(byte)BattleParameterCode.RoomID];
                                    byte mission = (byte)operationRequest.Parameters[(byte)BattleParameterCode.Mission];
                                    float missionRate = (float)operationRequest.Parameters[(byte)BattleParameterCode.MissionRate];
                                    Int16 customValue = (Int16)operationRequest.Parameters[(byte)BattleParameterCode.CustomValue];

                                    BattleUI battleUI = new BattleUI(); //初始化 UI 
                                    Log.Debug("BattleUI OK");
                                    BattleData battleData = (BattleData)TextUtility.DeserializeFromStream(battleUI.ClacMissionReward(mission, missionRate, customValue)); //計算分數
                                    Log.Debug("BattleData OK");



                                    if (battleData.ReturnCode == "S503")//計算分數成功 回傳玩家資料
                                    {
                                        Log.Debug("battleData.ReturnCode == S503");
                                        Int16 missionReward = battleData.missionReward;
                                        Int16 otherReward = battleData.customValue;

                                        if (mission == (byte)Mission.WorldBoss) // 如果是 世界王任務 回傳給雙方任務完成
                                        {
                                            _server.room.KillBoss(roomID);  // 世界王任務完成 把Server BOSS殺了          

                                            #region 自己

                                            // 儲存分數
                                            Room.RoomActor roomActor;
                                            roomActor = _server.room.GetActorFromGuid(peerGuid);
                                            roomActor.gameScore += battleData.missionReward;



                                            if (roomActor.gameScore < 0) roomActor.gameScore = 0;

                                            // 回傳給原玩家
                                            // ResponseParameter
                                            Dictionary<byte, object> myResParameter = new Dictionary<byte, object> {
                                        { (byte)BattleParameterCode.Ret, battleData.ReturnCode }, { (byte)BattleParameterCode.MissionReward, missionReward }  };
                                            OperationResponse response = new OperationResponse((byte)BattleResponseCode.MissionCompleted, myResParameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = battleData.ReturnMessage.ToString() };

                                            // EventData
                                            Dictionary<byte, object> myEventParameter = new Dictionary<byte, object>() { { (byte)BattleParameterCode.MissionReward, otherReward }, { (byte)BattleResponseCode.DebugMessage, battleData.ReturnMessage.ToString() } };
                                            EventData myScoreEventData = new EventData((byte)BattleResponseCode.GetMissionScore, myEventParameter);
                                            #endregion

                                            #region 對方

                                            // 回傳給另外一位玩家
                                            Room.RoomActor otherActor = new Room.RoomActor(actor.guid, actor.PrimaryID, actor.Account, actor.Nickname, actor.Age, actor.Sex, actor.IP); //這裡為了不要再增加變數 所以偷懶使用 actor.XX 正確是沒有actor.
                                            otherActor = _server.room.GetOtherPlayer(roomID, primaryID);
                                            peerOther = _server.Actors.GetPeerFromGuid(otherActor.guid);

                                            // 儲存分數
                                            otherActor.gameScore += otherReward;
                                            if (otherActor.gameScore < 0) otherActor.gameScore = 0;



                                            // ResponseParameter
                                            Dictionary<byte, object> otherResParameter = new Dictionary<byte, object> {
                                        { (byte)BattleParameterCode.Ret, battleData.ReturnCode }, { (byte)BattleParameterCode.MissionReward, otherReward }  };
                                            OperationResponse response2 = new OperationResponse((byte)BattleResponseCode.MissionCompleted, otherResParameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = battleData.ReturnMessage.ToString() };

                                            // EventData
                                            Dictionary<byte, object> otherEventParameter = new Dictionary<byte, object>() { { (byte)BattleParameterCode.MissionReward, missionReward }, { (byte)BattleResponseCode.DebugMessage, battleData.ReturnMessage.ToString() } };
                                            EventData otherScoreEventData = new EventData((byte)BattleResponseCode.GetMissionScore, otherEventParameter);


                                            SendOperationResponse(response, new SendParameters());// 我方 取得的分數
                                            SendEvent(myScoreEventData, new SendParameters());   //  我方 接收對方 取得的分數

                                            peerOther.SendOperationResponse(response2, new SendParameters());  // 對方 接收分數
                                            peerOther.SendEvent(otherScoreEventData, new SendParameters());    // 對方 接收我方 取得的分數

                                            #endregion
                                            Log.Debug("World Boss roomActor.gameScore:" + roomActor.gameScore + "roomOtherActor.gameScore:" + otherActor.gameScore);

                                            Log.Debug("\n\nMissionReward: " + missionReward + "  \n\nOtherReward: " + otherReward + "  \n\n");
                                        }
                                        else // 如果是 其他任務 獨立回傳任務完成
                                        {

                                            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)BattleParameterCode.Ret, battleData.ReturnCode }, { (byte)BattleParameterCode.MissionReward, missionReward } 
                                    };

                                            OperationResponse response = new OperationResponse((byte)BattleResponseCode.MissionCompleted, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = battleData.ReturnMessage.ToString() };
                                            SendOperationResponse(response, new SendParameters());

                                            //回傳給另外一位玩家
                                            Room.RoomActor otherActor = new Room.RoomActor(actor.guid, actor.PrimaryID, actor.Account, actor.Nickname, actor.Age, actor.Sex, actor.IP); //這裡為了不要再增加變數 所以偷懶使用 actor.XX 正確是沒有actor.
                                            otherActor = _server.room.GetOtherPlayer(roomID, primaryID);
                                            peerOther = _server.Actors.GetPeerFromGuid(otherActor.guid);
                                            Dictionary<byte, object> parameter2 = new Dictionary<byte, object>() { { (byte)BattleParameterCode.MissionReward, missionReward }, { (byte)BattleResponseCode.DebugMessage, battleData.ReturnMessage.ToString() } };
                                            EventData getMissionScoreEventData = new EventData((byte)BattleResponseCode.GetMissionScore, parameter2);

                                            peerOther.SendEvent(getMissionScoreEventData, new SendParameters());
                                        }
                                    }
                                    else
                                    {
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                        OperationResponse actorResponse = new OperationResponse((byte)BattleResponseCode.MissionCompleted, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = battleData.ReturnMessage.ToString() };
                                        SendOperationResponse(actorResponse, new SendParameters());
                                    }

                                }
                                catch (Exception e)
                                {
                                    Log.Error("發生例外情況: " + e.Message + " 於: " + e.StackTrace);
                                }

                            }
                            break;
                        #endregion

                        #region BossDamage 世界王生命計算
                        case (byte)BattleOperationCode.BossDamage:
                            {
                                try
                                {
                                    Log.Debug("IN BossDamage");

                                    primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];
                                    roomID = (int)operationRequest.Parameters[(byte)BattleParameterCode.RoomID];
                                    Int16 damage = (Int16)operationRequest.Parameters[(byte)BattleParameterCode.Damage];

                                    int bossHP = _server.room.UpLoadBossHP(roomID, damage);
                                    Log.Debug("BOSS HP:" + bossHP);
                                    if (bossHP > 0)
                                    {
                                        //回傳給原玩家
                                        Log.Debug("bossHP > 0");
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)BattleParameterCode.Damage, damage } };

                                        OperationResponse response = new OperationResponse((byte)BattleResponseCode.BossDamage, parameter) { ReturnCode = (short)ErrorCode.Ok };
                                        SendOperationResponse(response, new SendParameters());

                                        //回傳給另外一位玩家
                                        Room.RoomActor otherActor = new Room.RoomActor(actor.guid, actor.PrimaryID, actor.Account, actor.Nickname, actor.Age, actor.Sex, actor.IP); //這裡為了不要再增加變數 所以偷懶使用 actor.XX 正確是沒有actor.
                                        otherActor = _server.room.GetOtherPlayer(roomID, primaryID);
                                        peerOther = _server.Actors.GetPeerFromGuid(otherActor.guid);
                                        Dictionary<byte, object> parameter2 = new Dictionary<byte, object>() { { (byte)BattleParameterCode.Damage, damage } };
                                        EventData bossEventData = new EventData((byte)BattleResponseCode.BossDamage, parameter2);             // BOSS傷害, parameter2);

                                        peerOther.SendEvent(bossEventData, new SendParameters());
                                    }
                                    else if (bossHP == 0)
                                    {
                                        _server.room.KillBoss(roomID);
                                        Log.Debug("bossHP == 0");
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)BattleParameterCode.Damage, damage } };

                                        OperationResponse response = new OperationResponse((byte)BattleResponseCode.BossDamage, parameter) { ReturnCode = (short)ErrorCode.Ok };
                                        SendOperationResponse(response, new SendParameters());

                                        //回傳給另外一位玩家
                                        Room.RoomActor otherActor = new Room.RoomActor(actor.guid, actor.PrimaryID, actor.Account, actor.Nickname, actor.Age, actor.Sex, actor.IP); //這裡為了不要再增加變數 所以偷懶使用 actor.XX 正確是沒有actor.
                                        otherActor = _server.room.GetOtherPlayer(roomID, primaryID);
                                        peerOther = _server.Actors.GetPeerFromGuid(otherActor.guid);
                                        Dictionary<byte, object> parameter2 = new Dictionary<byte, object>() { { (byte)BattleParameterCode.Damage, damage } };
                                        EventData bossEventData = new EventData((byte)BattleResponseCode.BossDamage, parameter2);

                                        peerOther.SendEvent(bossEventData, new SendParameters());

                                    }
                                    else
                                    {
                                        Log.Error("發生例外情況 Boss血量小於0: " + bossHP);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Log.Error("發生例外情況: " + e.Message + " 於: " + e.StackTrace);
                                }
                            }
                            break;
                        #endregion

                        #region Damage 接收傷害
                        case (byte)BattleOperationCode.Damage:
                            {
                                try
                                {
                                    Log.Debug("IN Damage");

                                    primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];
                                    roomID = (int)operationRequest.Parameters[(byte)BattleParameterCode.RoomID];
                                    Int16 damage = (Int16)operationRequest.Parameters[(byte)BattleParameterCode.Damage];
                                    bool self = (bool)operationRequest.Parameters[(byte)BattleParameterCode.CustomValue];

                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)BattleParameterCode.Damage, damage } };
                                    if (self)
                                    {
                                        OperationResponse response = new OperationResponse((byte)BattleResponseCode.Damage, parameter) { ReturnCode = (short)ErrorCode.Ok };
                                        SendOperationResponse(response, new SendParameters());
                                    }
                                    else
                                    {
                                        //回傳給另外一位玩家
                                        Room.RoomActor otherActor = new Room.RoomActor(actor.guid, actor.PrimaryID, actor.Account, actor.Nickname, actor.Age, actor.Sex, actor.IP); //這裡為了不要再增加變數 所以偷懶使用 actor.XX 正確是沒有actor.
                                        otherActor = _server.room.GetOtherPlayer(roomID, primaryID);
                                        peerOther = _server.Actors.GetPeerFromGuid(otherActor.guid);
                                        Dictionary<byte, object> parameter2 = new Dictionary<byte, object>() { { (byte)BattleParameterCode.Damage, damage } };
                                        EventData bossEventData = new EventData((byte)BattleResponseCode.Damage, parameter2);             // BOSS傷害, parameter2);

                                        peerOther.SendEvent(bossEventData, new SendParameters());
                                    }
                                }
                                catch (Exception e)
                                {
                                    Log.Error("發生例外情況: " + e.Message + " 於: " + e.StackTrace);
                                }
                            }
                            break;
                        #endregion

                        #region GameOver 遊戲結束 處理資料回傳
                        case (byte)BattleOperationCode.GameOver:
                            {
                                try
                                {
                                    Log.Debug("IN GameOver");

                                    string account = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Account];
                                    Int16 score = (Int16)operationRequest.Parameters[(byte)BattleParameterCode.Score];
                                    Int16 otherScore = (Int16)operationRequest.Parameters[(byte)BattleParameterCode.OtherScore];
                                    Int16 gameTime = (Int16)operationRequest.Parameters[(byte)BattleParameterCode.Time];
                                    Int16 maxCombo = (Int16)operationRequest.Parameters[(byte)PlayerDataParameterCode.MaxCombo];
                                    Int16 lostMice = (Int16)operationRequest.Parameters[(byte)PlayerDataParameterCode.SumLost];
                                    int killMice = (int)operationRequest.Parameters[(byte)PlayerDataParameterCode.SumKill];
                                    string item = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.SortedItem];

                                    Room.RoomActor otherActor = _server.room.GetOtherPlayer(roomID, primaryID);
                                    peerOther = _server.Actors.GetPeerFromGuid(otherActor.guid);

                                    BattleUI battleUI = new BattleUI(); //實體化 IO (連結資料庫拿資料)
                                    BattleData battleData = (BattleData)TextUtility.DeserializeFromStream(battleUI.GameOver(score, otherScore, gameTime, lostMice)); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找
                                    Log.Debug("GameOver Data OK");

                                    if (battleData.ReturnCode == "S501")
                                    {
                                        Room.RoomActor roomActor;
                                        roomActor = _server.room.GetActorFromGuid(peerGuid);

                                        Log.Debug(roomActor.Nickname + "Player Score : " + roomActor.gameScore + "  " + otherActor.Nickname + "   Other Score : " + otherActor.gameScore);

                                        battleData.battleResult = (roomActor.gameScore > otherActor.gameScore) ? (byte)1 : (byte)0;


                                        PlayerDataUI playerDataUI = new PlayerDataUI(); //實體化 IO (連結資料庫拿資料)
                                        PlayerData playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.UpdateGameOver(account, roomActor.gameScore, battleData.expReward, maxCombo, score, lostMice, killMice, battleData.battleResult, item));// 更新會員資料

                                        if (playerData.ReturnCode == "S403")
                                        {
                                            playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.LoadPlayerData(account)); // 載入玩家資料 playerData的資料 = 資料庫拿的資料 用account去找
                                        }
                                        else
                                        {
                                            Log.Debug("Update PlayerData Error!" + "  Code:" + playerData.ReturnCode + "  Message:" + playerData.ReturnMessage);
                                        }

                                        if (playerData.ReturnCode == "S401")
                                        {
                                            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                                     { (byte)BattleParameterCode.Ret, battleData.ReturnCode },{ (byte)BattleParameterCode.Score, roomActor.gameScore },{ (byte)BattleParameterCode.SliverReward, battleData.sliverReward },
                                                     { (byte)BattleParameterCode.EXPReward, battleData.expReward },{ (byte)BattleParameterCode.BattleResult, battleData.battleResult },{ (byte)PlayerDataParameterCode.MaxScore, playerData.MaxScore } ,
                                                     { (byte)PlayerDataParameterCode.SumLost, playerData.SumLost },{ (byte)PlayerDataParameterCode.SumKill, playerData.SumKill },{ (byte)PlayerDataParameterCode.SortedItem, playerData.SortedItem },
                                                     { (byte)PlayerDataParameterCode.MaxCombo, playerData.MaxCombo },{ (byte)PlayerDataParameterCode.Rank, playerData.Rank },                                            };

                                            OperationResponse response = new OperationResponse((byte)BattleResponseCode.GameOver, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = battleData.ReturnMessage };
                                            SendOperationResponse(response, new SendParameters());
                                        }
                                        else
                                        {
                                            Log.Debug("playerData.ReturnCode=" + playerData.ReturnCode + "playerData.ReturnMessage" + playerData.ReturnMessage);
                                            Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)BattleParameterCode.Ret, battleData.ReturnCode } };
                                            OperationResponse response = new OperationResponse((byte)BattleResponseCode.GameOver, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = playerData.ReturnMessage.ToString() };
                                            SendOperationResponse(response, new SendParameters());
                                        }
                                    }
                                    else
                                    {
                                        Log.Debug("battleData.ReturnCode=" + battleData.ReturnCode + "battleData.ReturnMessage" + battleData.ReturnMessage);
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)BattleParameterCode.Ret, battleData.ReturnCode } };
                                        OperationResponse response = new OperationResponse((byte)BattleResponseCode.GameOver, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = battleData.ReturnMessage.ToString() };
                                        SendOperationResponse(response, new SendParameters());
                                    }


                                }
                                catch (Exception e)
                                {
                                    Log.Debug("例外情況: " + e.Message + "於： " + e.StackTrace);
                                }

                            }
                            break;
                        #endregion

                        #region BuyItem 購買商品
                        case (byte)StoreOperationCode.BuyItem:
                            {
                                try
                                {
                                    Log.Debug("IN BuyItem");
                                    string account = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Account];
                                    string miceAll = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.MiceAll];
                                    Int16 itemID = (Int16)operationRequest.Parameters[(byte)StoreParameterCode.ItemID];
                                    string itemName = (string)operationRequest.Parameters[(byte)StoreParameterCode.ItemName];
                                    byte itemType = (byte)operationRequest.Parameters[(byte)StoreParameterCode.ItemType];
                                    byte currencyType = (byte)operationRequest.Parameters[(byte)StoreParameterCode.CurrencyType];
                                    Int16 buyCount = (Int16)operationRequest.Parameters[(byte)StoreParameterCode.BuyCount];

                                    Log.Debug(string.Format("itemName={0}   itemType={1}   currencyType={2}   buyCount={3}", itemID, itemType, currencyType, buyCount));
                                    StoreDataUI storeDataUI = new StoreDataUI(); //實體化 IO (連結資料庫拿資料)
                                    Log.Debug("BuyItem IO OK");
                                    StoreData storeData = (StoreData)TextUtility.DeserializeFromStream(storeDataUI.LoadStoreData(itemID, itemType)); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找
                                    Log.Debug("BuyItem Data OK" + storeData.Price);

                                    if (storeData.ReturnCode == "S901") // 更新玩家貨幣
                                    {
                                        CurrencyUI currencyUI = new CurrencyUI();
                                        CurrencyData currencyData = (CurrencyData)TextUtility.DeserializeFromStream(currencyUI.UpdateCurrency(account, currencyType, storeData.Price * buyCount));


                                        Log.Debug("BuyItem currencyData OK :" + currencyData.ReturnCode);

                                        if (currencyData.ReturnCode == "S703") // 更新商店資料
                                        {
                                            #region UpdateStoreBuyCount
                                            storeData = (StoreData)TextUtility.DeserializeFromStream(storeDataUI.UpdateStoreBuyCount(itemID, itemType, buyCount));

                                            if (storeData.ReturnCode == "S902") // 更新玩家資料
                                            {
                                                currencyData = (CurrencyData)TextUtility.DeserializeFromStream(currencyUI.LoadCurrency(account));

                                                #region UpdatePlayerData
                                                PlayerDataUI playerUI = new PlayerDataUI();
                                                PlayerData playerData = (PlayerData)TextUtility.DeserializeFromStream(playerUI.UpdatePlayerItem(account, itemID, itemName, itemType, buyCount));

                                                if (playerData.ReturnCode == "S422" || playerData.ReturnCode == "S423") //更新玩家道具資料成功 回傳玩家資料
                                                {
                                                    playerData = (PlayerData)TextUtility.DeserializeFromStream(playerUI.LoadPlayerData(account));
                                                    Log.Debug("BuyItem playerData OK");

                                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                                                     { (byte)MiceParameterCode.Ret, storeData.ReturnCode }, { (byte)PlayerDataParameterCode.MiceAll, playerData.MiceAll } ,
                                                                     { (byte)CurrencyParameterCode.Gold, currencyData.Gold } ,{ (byte)CurrencyParameterCode.Rice, currencyData.Rice } 
                                                                        };

                                                    OperationResponse response = new OperationResponse((byte)StoreOperationCode.BuyItem, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = storeData.ReturnMessage.ToString() };
                                                    SendOperationResponse(response, new SendParameters());
                                                }
                                                else    // 失敗
                                                {
                                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                                    OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = storeData.ReturnMessage.ToString() };
                                                    SendOperationResponse(actorResponse, new SendParameters());
                                                }
                                                #endregion
                                            }
                                            else    // 失敗
                                            {
                                                Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                                OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = storeData.ReturnMessage.ToString() };
                                                SendOperationResponse(actorResponse, new SendParameters());
                                            }
                                            #endregion
                                        }
                                    }
                                    else    // 失敗
                                    {
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                        OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = storeData.ReturnMessage.ToString() };
                                        SendOperationResponse(actorResponse, new SendParameters());
                                    }
                                }
                                catch (Exception e)
                                {
                                    Log.Debug("例外情況: " + e.Message + "於： " + e.StackTrace);
                                }

                            }
                            break;
                        #endregion
                    }
                }
            }
            catch (Exception e)
            {
                Log.Debug("ServerPeer例外情況: " + e.Message + "於： " + e.StackTrace);
            }

        }
    }
}
