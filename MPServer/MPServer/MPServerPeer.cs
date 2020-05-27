using ExitGames.Logging;
using MPProtocol;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using System;
using System.Linq;
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
    public class MPServerPeer : MPClientPeer
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        public Guid peerGuid { get; protected set; }

        private bool isCreateRoom;
        // private int roomID;
        //private int primaryID;
        //   private byte memberType;
        //  MPServerPeer peerOther; // 錯誤

        private BattleUI battleUI;
        private CurrencyUI currencyUI;
        private MemberUI memberUI;
        private MiceDataUI miceDataUI;
        private PlayerDataUI playerDataUI;
        private SkillUI skillUI;
        private StoreDataUI storeDataUI;
        private PurchaseUI purchaseUI;
        private ItemUI itemUI;
        private GashaponUI gashaponUI;

        private static readonly int GameTime = 180;
        private static readonly int maxNumPlayer = 2;
        //初始化
        public MPServerPeer(InitRequest initRequest, MPServerApplication serverApplication)
            : base(initRequest, serverApplication)
        {

            // IRpcProtocol rpcProtocol, IPhotonPeer nativePeer, MPServerApplication ServerApplication

            isCreateRoom = false;
            peerGuid = Guid.NewGuid();      // 建立一個Client唯一識別GUID
            //   _server = ServerApplication;

            _server.Actors.AddConnectedPeer(peerGuid, this);    // 加入連線中的peer列表

            battleUI = new BattleUI();
            memberUI = new MemberUI();
            miceDataUI = new MiceDataUI();
            playerDataUI = new PlayerDataUI();
            skillUI = new SkillUI();
            storeDataUI = new StoreDataUI();
            currencyUI = new CurrencyUI();
            purchaseUI = new PurchaseUI();
            gashaponUI = new GashaponUI(); //實體化 IO (連結資料庫拿資料)
            itemUI = new ItemUI();

            if (_server.Actors.GetOnlineActors().Count >= 100)
            {
                string reasonDetail = "線上玩家數過多";
                OnDisconnect(PhotonHostRuntimeInterfaces.DisconnectReason.ServerDisconnect, reasonDetail);
            }
        }

        //當斷線時
        protected override void OnDisconnect(PhotonHostRuntimeInterfaces.DisconnectReason reasonCode, string reasonDetail)
        {
            Log.Debug("peer:" + peerGuid + "\n已登出玩家：" + _server.Actors.GetAccountFromGuid(peerGuid) + "  時間:" + DateTime.Now);

            Actor existPlayer = _server.Actors.GetActorFromGuid(peerGuid);                                      // 用GUID找到離線的玩家
            try
            {
                int roomID = _server.room.GetPlayingRoomFromGuid(peerGuid);

                if (_server.room.GetPlayingRoomFromGuid(peerGuid) > 0)                                                 // 假如用離開的玩家guid 從GameRoomList中找的到房間 (遊戲中中斷)
                {
                    Log.Debug("RoomID:" + _server.room.GetPlayingRoomFromGuid(peerGuid));
                    //取得房間ID
                    Room.RoomActor otherRoomActor = _server.room.GetPlayingRoomOtherPlayer(roomID, existPlayer.PrimaryID);     // 取得房間內的其他玩家

                    MPServerPeer peerOther;
                    // 玩家斷線時，踢房間內其他玩家(兩人版本)
                    if (otherRoomActor != null)
                    {
                        peerOther = _server.Actors.GetPeerFromPrimary(otherRoomActor.PrimaryID);
                        Dictionary<byte, object> parameter = new Dictionary<byte, object>() { { (byte)BattleResponseCode.DebugMessage, "配對的玩家已經離開房間！" } };
                        EventData exitEventData = new EventData((byte)BattleResponseCode.KickOther, parameter);
                        peerOther.SendEvent(exitEventData, new SendParameters());
                    }
                    _server.room.KillBossFromGuid(peerGuid);                                                    // 移除房間中的BOSS(如果有)
                    _server.room.RemovePlayingRoom(roomID, peerGuid, existPlayer.PrimaryID);                    // 移除房間 與房間內玩家
                }

                if (_server.room.bGetWaitActorFromGuid(peerGuid, roomID))                                               // 如果這個玩家在等待房間中 
                    _server.room.RemoveWaitingRoom(roomID, peerGuid);                                                   // 移除等待房間(這裡要改應位以後可能有很多等待房間)

                Actor player = _server.Actors.GetActorFromGuid(peerGuid);
                if (player != null) player.GameStatus = (byte)ENUM_MemberState.Offline;
                //_server.room.RemovePlayingRoom(roomID, peerGuid, peerGuid);
                _server.Actors.ActorOffline(peerGuid); // 把玩家離線
                _server.Actors.RemoveConnectedPeer(peerGuid);

                Dictionary<Guid, MPServerPeer> actors = new Dictionary<Guid, MPServerPeer>(_server.Actors.GetOnlineActors());
                // 發送線上人數
                foreach (KeyValuePair<Guid, MPServerPeer> actor in actors)
                {
                    Dictionary<byte, object> parameter = new Dictionary<byte, object>() { { (byte)SystemParameterCode.OnlineActors, actors.Count } };
                    actor.Value.SendEvent(new EventData((byte)MemberResponseCode.GetOnlineActor, parameter), new SendParameters());
                }
            }
            catch (Exception e)
            {
                Log.Error("DisConnect Error : " + e.Message + " Track:" + e.StackTrace);
            }
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

                    Log.Debug("Server Get Response Code: " + operationRequest.OperationCode);

                    switch (operationRequest.OperationCode)
                    {


                        #region JoinMember 加入會員 (這裡面有IP、email的參數亂打) (含FB首次登入)

                        case (byte)MemberOperationCode.JoinMember:
                            try
                            {
                                string email = (string)operationRequest.Parameters[(byte)MemberParameterCode.Email];
                                string account = (string)operationRequest.Parameters[(byte)MemberParameterCode.Account];
                                string password = (string)operationRequest.Parameters[(byte)MemberParameterCode.Password];
                                string nickname = (string)operationRequest.Parameters[(byte)MemberParameterCode.Nickname];
                                string IP = (string)operationRequest.Parameters[(byte)MemberParameterCode.IP];
                                byte age = (byte)operationRequest.Parameters[(byte)MemberParameterCode.Age];
                                byte sex = (byte)operationRequest.Parameters[(byte)MemberParameterCode.Sex];

                                string joinTime = (string)operationRequest.Parameters[(byte)MemberParameterCode.JoinDate];
                                MemberType memberType = (MemberType)operationRequest.Parameters[(byte)MemberParameterCode.MemberType];


                                Log.Debug("IN Join Member :" + "Account:" + account + "memberType: " + memberType);
                                MemberData memberData = new MemberData(); ;

                                memberData = (MemberData)TextUtility.DeserializeFromStream(memberUI.JoinMember(account, password, nickname, age, sex, IP, email, joinTime, (byte)memberType));

                                if (memberData.ReturnCode == "S101")
                                {
                                    Log.Debug("Join Member Successd ! ");

                                    if (memberType == MemberType.Facebook)
                                    {
                                        memberData = (MemberData)TextUtility.DeserializeFromStream(memberUI.MemberLogin("FB" + account, password)); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找
                                        if (memberData.ReturnCode == "S201")
                                        {
                                            Log.Debug("Facebook登入會員成功 Code:" + memberData.ReturnCode + "Message:" + memberData.ReturnMessage);
                                            //加入線上玩家列表
                                            ActorCollection.ActorReturn actorReturn = _server.Actors.ActorOnline(peerGuid, memberData.PrimaryID, memberData.Account, memberData.Nickname, memberData.Age, memberData.Sex, memberData.IP, this, memberData.MemberType);

                                            if (actorReturn.ReturnCode == "S301")// 加入線上會員資料成功 回傳資料
                                            {
                                                Actor actor = _server.Actors.GetActorFromGuid(peerGuid);
                                                actor.GameStatus = (byte)ENUM_MemberState.Online;
                                                Log.Debug("Facebook加入線上玩家成功 Code:" + memberData.ReturnCode + "Message:" + memberData.ReturnMessage);
                                                Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                                                    { (byte)LoginParameterCode.Ret, actorReturn.ReturnCode }, { (byte)LoginParameterCode.PrimaryID, memberData.PrimaryID }, { (byte)LoginParameterCode.Account, account },
                                                                    { (byte)LoginParameterCode.Nickname, memberData.Nickname }, { (byte)LoginParameterCode.Age, memberData.Age }, { (byte)LoginParameterCode.Sex, memberData.Sex } , { (byte)LoginParameterCode.MemberType, memberData.MemberType } 
                                                                    };

                                                OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = memberData.ReturnMessage.ToString() };
                                                SendOperationResponse(actorResponse, new SendParameters());

                                                Dictionary<Guid, MPServerPeer> buffer = new Dictionary<Guid, MPServerPeer>(_server.Actors.GetOnlineActors());


                                                // 發送線上人數
                                                foreach (KeyValuePair<Guid, MPServerPeer> peer in buffer)
                                                {
                                                    parameter = new Dictionary<byte, object>() { { (byte)SystemParameterCode.OnlineActors, buffer.Count } };
                                                    peer.Value.SendEvent(new EventData((byte)MemberResponseCode.GetOnlineActor, parameter), new SendParameters());
                                                }
                                            }
                                            else if (actorReturn.ReturnCode == "S302") //登入錯誤 重複登入
                                            {
                                                Log.Debug("登入錯誤 重複登入!: " + account);

                                                // 用這個peer guid的PrimaryUD來找guid 踢掉線上玩家
                                                MPServerPeer peer;
                                                peer = _server.Actors.GetPeerFromPrimary(memberData.PrimaryID); //primaryID取得登入peer


                                                // 送出 重複登入事件 叫另外一個他斷線
                                                EventData eventData = new EventData((byte)LoginOperationCode.ReLogin);
                                                peer.SendEvent(eventData, new SendParameters());


                                                // 回傳給登入者 通知重複登入
                                                Dictionary<byte, object> parameter = new Dictionary<byte, object>{
                                                    { (byte)LoginParameterCode.Ret, actorReturn.ReturnCode }, { (byte)LoginParameterCode.PrimaryID, 0 }, { (byte)LoginParameterCode.Account, "" }, 
                                                    { (byte)LoginParameterCode.Nickname, "" }, { (byte)LoginParameterCode.Age, 0 }, { (byte)LoginParameterCode.Sex, 0 } , { (byte)LoginParameterCode.MemberType, memberData.MemberType } 
                                                };

                                                OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = actorReturn.DebugMessage.ToString() };
                                                SendOperationResponse(actorResponse, new SendParameters());
                                                peer.OnDisconnect(new DisconnectReason(), "重複登入");
                                            }
                                            else // 登入錯誤 回傳空值
                                            {
                                                Log.Debug("Facebook加入會員未知錯誤!!" + "actorReturn ReturnCode:" + actorReturn.ReturnCode + "memberData Code:" + memberData.ReturnCode + "  memberData Message:" + memberData.ReturnMessage);
                                                OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, null) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = actorReturn.DebugMessage.ToString() };
                                                SendOperationResponse(actorResponse, new SendParameters());
                                                break;
                                            }
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        // 加入會員成功
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)MemberParameterCode.Ret, memberData.ReturnCode } };
                                        OperationResponse actorResponse = new OperationResponse((byte)MemberResponseCode.JoinMember, null) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = memberData.ReturnMessage.ToString() };
                                        SendOperationResponse(actorResponse, new SendParameters());
                                    }
                                }
                                else
                                {// 加入會員失敗
                                    Log.Debug("Join Member Failed ! " + memberData.ReturnCode + memberData.ReturnMessage);
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)MemberParameterCode.Ret, memberData.ReturnCode}
                                    };

                                    OperationResponse actorResponse = new OperationResponse((byte)MemberResponseCode.JoinMember, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = memberData.ReturnMessage.ToString() };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }

                            }
                            catch (Exception e)
                            {
                                Log.Debug("Join Member Error ! " + e.Message + "Track: " + e.StackTrace);
                            }
                            break;

                        #endregion

                        #region UpdateMember 更新會員資料
                        case (byte)MemberOperationCode.UpdateMember:
                            try
                            {
                                string account = (string)operationRequest.Parameters[(byte)MemberParameterCode.Account];
                                string jString = (string)operationRequest.Parameters[(byte)MemberParameterCode.Custom]; // 更新會員資料字典(欄位、值)

                                memberUI.UpdateMember(account, jString);
                            }
                            catch (Exception e)
                            {
                                Log.Debug("Update Member Error ! " + e.Message + "Track: " + e.StackTrace);
                            }
                            break;
                        #endregion

                        #region Login 會員登入

                        case (byte)LoginOperationCode.Login:
                            try
                            {
                                MemberUI memberUI = new MemberUI(); //實體化 UI (連結資料庫拿資料)
                                MemberData memberData = new MemberData();
                                string account = (string)operationRequest.Parameters[(byte)LoginParameterCode.Account];
                                string passowrd = (string)operationRequest.Parameters[(byte)LoginParameterCode.Password];
                                MemberType loginMemberType = (MemberType)operationRequest.Parameters[(byte)LoginParameterCode.MemberType];

                                Log.Debug("Account:" + account + "Password:" + passowrd + "memberType:" + memberData.MemberType);

                                memberData = (MemberData)TextUtility.DeserializeFromStream(memberUI.MemberLogin(account, passowrd)); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找

                                switch (memberData.ReturnCode)　//取得資料庫資料成功
                                {
                                    case "S200":
                                        {
                                            Log.Debug("Code:"+ memberData.ReturnCode + "  Message:"+memberData.ReturnMessage +  "  會員資料內部程式錯誤！"  );
                                            break;
                                        }
                                    #region 登入成功
                                    case "S201":
                                        {
                                            Log.Debug("登入成功 Code = S201");
                                            //加入線上玩家列表
                                            ActorCollection.ActorReturn actorReturn = _server.Actors.ActorOnline(peerGuid, memberData.PrimaryID, memberData.Account, memberData.Nickname, memberData.Age, memberData.Sex, memberData.IP, this, memberData.MemberType);
                                            Log.Debug("ReturnCode :" + actorReturn.ReturnCode + " ReturnMessage :" + actorReturn.DebugMessage + "memberData:");


                                            if (actorReturn.ReturnCode == "S301")// 加入線上會員資料成功 回傳資料
                                            {
                                                Actor actor = _server.Actors.GetActorFromGuid(peerGuid);
                                                actor.GameStatus = (byte)ENUM_MemberState.Online;
                                                Log.Debug("加入線上會員資料成功 Code = S301");
                                                Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                                    { (byte)LoginParameterCode.Ret, actorReturn.ReturnCode }, { (byte)LoginParameterCode.PrimaryID, memberData.PrimaryID }, { (byte)LoginParameterCode.Account, account },
                                                    { (byte)LoginParameterCode.Nickname, memberData.Nickname }, { (byte)LoginParameterCode.Age, memberData.Age }, { (byte)LoginParameterCode.Sex, memberData.Sex } , { (byte)LoginParameterCode.MemberType,memberData.MemberType } 
                                                };

                                                OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = memberData.ReturnMessage.ToString() };
                                                SendOperationResponse(actorResponse, new SendParameters());

                                                Dictionary<Guid, MPServerPeer> buffer = new Dictionary<Guid, MPServerPeer>(_server.Actors.GetOnlineActors());


                                                // 發送線上人數
                                                foreach (KeyValuePair<Guid, MPServerPeer> peer in buffer)
                                                {
                                                    parameter = new Dictionary<byte, object>() { { (byte)SystemParameterCode.OnlineActors, buffer.Count } };
                                                    peer.Value.SendEvent(new EventData((byte)MemberResponseCode.GetOnlineActor, parameter), new SendParameters());
                                                }
                                            }
                                            else if (actorReturn.ReturnCode == "S302") //登入錯誤 重複登入
                                            {
                                                Log.Debug("登入錯誤 重複登入!: " + account);

                                                // 用這個peer guid的PrimaryUD來找guid 踢掉線上玩家
                                                MPServerPeer peer;
                                                peer = _server.Actors.GetPeerFromPrimary(memberData.PrimaryID); //primaryID取得登入peer


                                                // 送出 重複登入事件 叫另外一個他斷線
                                                EventData eventData = new EventData((byte)LoginOperationCode.ReLogin);
                                                peer.SendEvent(eventData, new SendParameters());


                                                // 回傳給登入者 通知重複登入
                                                Dictionary<byte, object> parameter = new Dictionary<byte, object>{
                                                    { (byte)LoginParameterCode.Ret, actorReturn.ReturnCode }, { (byte)LoginParameterCode.PrimaryID, 0 }, { (byte)LoginParameterCode.Account, "" }, 
                                                    { (byte)LoginParameterCode.Nickname, "" }, { (byte)LoginParameterCode.Age, 0 }, { (byte)LoginParameterCode.Sex, 0 } , { (byte)LoginParameterCode.MemberType, memberData.MemberType } 
                                                };

                                                OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = actorReturn.DebugMessage.ToString() };
                                                SendOperationResponse(actorResponse, new SendParameters());
                                                peer.OnDisconnect(new DisconnectReason(), "重複登入！");
                                            }
                                            else // 登入錯誤 回傳空值
                                            {
                                                OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, null) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = actorReturn.DebugMessage.ToString() };
                                                SendOperationResponse(actorResponse, new SendParameters());
                                                break;
                                            }
                                            break;
                                        }
                                    #endregion

                                    #region 登入失敗(判斷哪種方式登入) 如果是SNS 加入會員 (含Google首次登入)
                                    case "S204":
                                        {
                                            Log.Debug("登入失敗 = S204    MemeberType:" + loginMemberType + "  account:" + account + "  password:" + passowrd);

                                            if (loginMemberType == MemberType.Gansol || loginMemberType == MemberType.Bot)    // 如果是基本會員登入 回傳 帳密錯誤
                                            {
                                                Log.Debug("Gansol OR Bot Code:" + "LoginMemberType:" + loginMemberType);
                                                OperationResponse response = new OperationResponse(operationRequest.OperationCode) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = memberData.ReturnMessage.ToString() };
                                                SendOperationResponse(response, new SendParameters());
                                                break;
                                            }
                                            else if (loginMemberType == MemberType.Google)  // SNS登入失敗 加入會員
                                            {

                                                string name = (string)operationRequest.Parameters[(byte)LoginParameterCode.Nickname];
                                                string email = (string)operationRequest.Parameters[(byte)MemberParameterCode.Email];
                                                // int sex = (int)operationRequest.Parameters[(byte)MemberParameterCode.Sex];
                                                int age = (int)operationRequest.Parameters[(byte)MemberParameterCode.Age];

                                                Log.Debug("Google" + "name:" + name + " email:" + email + " age:" + age);

                                                memberData = (MemberData)TextUtility.DeserializeFromStream(memberUI.JoinMember(account, passowrd, name, Convert.ToByte(age), 3, LocalIP, email, DateTime.Now.ToString(), (byte)loginMemberType)); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找
                                                if (memberData.ReturnCode == "S101")
                                                {
                                                    Log.Debug("Google加入會員成功 Code:" + memberData.ReturnCode + "Message:" + memberData.ReturnMessage);
                                                    memberData = (MemberData)TextUtility.DeserializeFromStream(memberUI.MemberLogin(account, passowrd)); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找

                                                    if (memberData.ReturnCode == "S201")
                                                    {
                                                        Log.Debug("Google登入會員成功 Code:" + memberData.ReturnCode + "Message:" + memberData.ReturnMessage);
                                                        //加入線上玩家列表
                                                        ActorCollection.ActorReturn actorReturn = _server.Actors.ActorOnline(peerGuid, memberData.PrimaryID, memberData.Account, memberData.Nickname, memberData.Age, memberData.Sex, memberData.IP, this, memberData.MemberType);

                                                        if (actorReturn.ReturnCode == "S301")// 加入線上會員資料成功 回傳資料
                                                        {
                                                            Actor actor = _server.Actors.GetActorFromGuid(peerGuid);
                                                            actor.GameStatus = (byte)ENUM_MemberState.Online;
                                                            Log.Debug("Google加入線上玩家成功 Code:" + memberData.ReturnCode + "Message:" + memberData.ReturnMessage);

                                                            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                                                    { (byte)LoginParameterCode.Ret, actorReturn.ReturnCode }, { (byte)LoginParameterCode.PrimaryID, memberData.PrimaryID }, { (byte)LoginParameterCode.Account, memberData.Account },
                                                                    { (byte)LoginParameterCode.Nickname, memberData.Nickname }, { (byte)LoginParameterCode.Age, memberData.Age }, { (byte)LoginParameterCode.Sex, memberData.Sex } , { (byte)LoginParameterCode.MemberType, memberData.MemberType } 
                                                                    };

                                                            OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = memberData.ReturnMessage.ToString() };
                                                            SendOperationResponse(actorResponse, new SendParameters());
                                                        }
                                                        else // 登入錯誤 回傳空值
                                                        {
                                                            Log.Debug("Google加入會員未知錯誤!!" + memberData.ReturnCode + "Message:" + memberData.ReturnMessage);
                                                            OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, null) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = actorReturn.DebugMessage.ToString() };
                                                            SendOperationResponse(actorResponse, new SendParameters());
                                                            break;
                                                        }
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        Log.Debug("(390)Google 登入失敗 Code:" + memberData.ReturnCode + " Account:" + memberData.Account + "  " + memberData.ReturnMessage);
                                                        OperationResponse response = new OperationResponse(operationRequest.OperationCode) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = memberData.ReturnMessage.ToString() };
                                                        SendOperationResponse(response, new SendParameters());
                                                        break;
                                                    }






                                                }
                                                // 已有相同會員
                                                else if (memberData.ReturnCode == "S108")
                                                {
                                                    Log.Debug("Account" + account + "paswd:" + passowrd);
                                                    memberData = (MemberData)TextUtility.DeserializeFromStream(memberUI.MemberLogin(account, passowrd)); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找
                                                    if (memberData.ReturnCode == "S201")
                                                    {
                                                        Log.Debug("Google登入會員成功 Code:" + memberData.ReturnCode + "Message:" + memberData.ReturnMessage);
                                                        //加入線上玩家列表
                                                        ActorCollection.ActorReturn actorReturn = _server.Actors.ActorOnline(peerGuid, memberData.PrimaryID, memberData.Account, memberData.Nickname, memberData.Age, memberData.Sex, memberData.IP, this, memberData.MemberType);

                                                        if (actorReturn.ReturnCode == "S301")// 加入線上會員資料成功 回傳資料
                                                        {
                                                            Log.Debug("Google加入線上玩家成功 Code:" + memberData.ReturnCode + "Message:" + memberData.ReturnMessage);
                                                            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                                                    { (byte)LoginParameterCode.Ret, actorReturn.ReturnCode }, { (byte)LoginParameterCode.PrimaryID, memberData.PrimaryID }, { (byte)LoginParameterCode.Account, memberData.Account },
                                                                    { (byte)LoginParameterCode.Nickname, memberData.Nickname }, { (byte)LoginParameterCode.Age, memberData.Age }, { (byte)LoginParameterCode.Sex, memberData.Sex } , { (byte)LoginParameterCode.MemberType, memberData.MemberType } 
                                                                    };

                                                            OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = memberData.ReturnMessage.ToString() };
                                                            SendOperationResponse(actorResponse, new SendParameters());
                                                        }
                                                        else // 登入錯誤 回傳空值
                                                        {
                                                            Log.Debug("Google登入錯誤 未知錯誤!!" + memberData.ReturnCode + "Message:" + memberData.ReturnMessage);
                                                            OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, null) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = actorReturn.DebugMessage.ToString() };
                                                            SendOperationResponse(actorResponse, new SendParameters());
                                                            break;
                                                        }
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        Log.Debug("(434)Google 登入失敗 Code:" + memberData.ReturnCode + " Account:" + memberData.Account + "  " + memberData.ReturnMessage);
                                                        OperationResponse response = new OperationResponse(operationRequest.OperationCode) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = memberData.ReturnMessage.ToString() };
                                                        SendOperationResponse(response, new SendParameters());
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    Log.Debug("Google Join Member Failed ! " + memberData.ReturnCode + memberData.ReturnMessage);
                                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                                                { (byte)MemberParameterCode.Ret, memberData.ReturnCode}
                                                                                };

                                                    OperationResponse actorResponse = new OperationResponse((byte)MemberResponseCode.JoinMember, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = memberData.ReturnMessage.ToString() };
                                                    SendOperationResponse(actorResponse, new SendParameters());
                                                }










                                            }
                                            else if (loginMemberType == MemberType.Facebook)  // Facebook登入失敗 加入會員
                                            {
                                                Log.Debug("MemberType.Facebook");
                                                OperationResponse actorResponse = new OperationResponse((byte)LoginOperationCode.GetProfile) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = memberData.ReturnMessage.ToString() };
                                                SendOperationResponse(actorResponse, new SendParameters());
                                            }
                                            break;

                                        }
                                    default: // 無效的登入資訊 無法取得伺服器資料
                                        {
                                            Log.Debug("Code:" + memberData.ReturnCode + "Message:" + memberData.ReturnMessage + "  無效的登入資訊 無法取得伺服器資料!");
                                            OperationResponse response = new OperationResponse(operationRequest.OperationCode) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = memberData.ReturnMessage.ToString() };
                                            SendOperationResponse(response, new SendParameters());
                                            break;
                                        }
                                    #endregion
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
                            //  Log.Debug("-------------------Match Game--------------------");
                            try
                            {
                                int primaryID = (int)operationRequest.Parameters[(byte)MatchGameParameterCode.PrimaryID];
                                string myTeam = (string)operationRequest.Parameters[(byte)MatchGameParameterCode.Team];
                                MemberType matchMemberType = (MemberType)operationRequest.Parameters[(byte)MemberParameterCode.MemberType];
                                Actor actor = _server.Actors.GetActorFromPrimary(primaryID);  // 用primaryID取得角色資料

                                if (matchMemberType != (byte)MemberType.Bot)
                                {
                                    //假如等待房間列表中沒房間 建立等待房間
                                    if (_server.room.dictWaitingRoomList.Count == 0)
                                    {
                                        isCreateRoom = _server.room.CreateRoom(actor.guid, actor.PrimaryID, actor.Account, actor.Nickname, actor.Age, actor.Sex, actor.IP);

                                        actor.GameStatus = (byte)ENUM_MemberState.Matching;

                                        Log.Debug("Actor State = " + _server.Actors.GetActorFromGuid(peerGuid).GameStatus);
                                        Log.Debug("等待房間列表中沒房間 建立等待房間 Create Room !" + "  Count:" + _server.room.dictWaitingRoomList.Count + " Room ID:" + _server.room.GetWaitingRoomFromGuid(peerGuid));
                                    }
                                    else //假如等待列表中有等待配對的房間
                                    {
                                        Log.Debug("等待配對的房間數量:" + _server.room.dictWaitingRoomList.Count + "  加入等待配對的房間 加入房間 Join Room:" + _server.room.GetWaitingRoomFromGuid(actor.guid));
                                        if (_server.room.JoinRoom(actor.guid, actor.PrimaryID, actor.Account, actor.Nickname, actor.Age, actor.Sex, actor.IP))
                                        {
                                            Room.RoomActor otherRoomActor;

                                            int roomID = _server.room.GetPlayingRoomFromGuid(peerGuid);
                                            otherRoomActor = _server.room.GetPlayingRoomOtherPlayer(roomID, primaryID);

                                            PlayerData otherPlayerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.LoadPlayerData(otherRoomActor.Account));
                                            PlayerData playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.LoadPlayerData(actor.Account));

                                            Dictionary<byte, object> myParameter = new Dictionary<byte, object>() { { (byte)MatchGameParameterCode.PrimaryID, actor.PrimaryID }, { (byte)MatchGameParameterCode.Nickname, actor.Nickname }, { (byte)MatchGameParameterCode.RoomID, roomID }, { (byte)MatchGameParameterCode.Team, myTeam }, { (byte)PlayerDataParameterCode.PlayerImage, playerData.PlayerImage }, { (byte)MatchGameParameterCode.RoomPlace, "Guest" } };
                                            Dictionary<byte, object> otherParameter = new Dictionary<byte, object>() { { (byte)MatchGameParameterCode.PrimaryID, otherRoomActor.PrimaryID }, { (byte)MatchGameParameterCode.Nickname, otherRoomActor.Nickname }, { (byte)MatchGameParameterCode.RoomID, roomID }, { (byte)MatchGameParameterCode.Team, otherPlayerData.Team }, { (byte)PlayerDataParameterCode.PlayerImage, otherPlayerData.PlayerImage }, { (byte)MatchGameParameterCode.RoomPlace, "Host" } };

                                            OperationResponse response = new OperationResponse((byte)MatchGameResponseCode.ExitWaiting, myParameter) { DebugMessage = "配對成功，離開等待房間。" };
                                            SendOperationResponse(response, new SendParameters());

                                            // 玩家2加入房間成功後 發送配對成功事件給房間內兩位玩家
                                            EventData myEventData = new EventData((byte)MatchGameResponseCode.Match, myParameter);
                                            EventData otherEventData = new EventData((byte)MatchGameResponseCode.Match, otherParameter);
                                            MPServerPeer peerMe, peerOther;

                                            //取得雙方玩家的peer
                                            peerMe = _server.Actors.GetPeerFromGuid(this.peerGuid);         // 加入房間的人
                                            peerOther = _server.Actors.GetPeerFromGuid(otherRoomActor.guid);    // 建立房間的人

                                            // 改變雙方玩家狀態
                                            actor.GameStatus = (byte)ENUM_MemberState.Playing;
                                            Actor otherActor = _server.Actors.GetActorFromGuid(peerOther.peerGuid);
                                            otherActor.GameStatus = (byte)ENUM_MemberState.Playing;

                                            Log.Debug("My Peer:" + this.peerGuid + "  " + actor.Nickname);
                                            Log.Debug("Other Peer:" + otherRoomActor.guid + "  " + otherRoomActor.Nickname);

                                            peerMe.SendEvent(otherEventData, new SendParameters());           // 要送給相反的人資料 (RoomActor2)Gueest會收到(RoomActor1)Host資料
                                            if (peerOther != null) peerOther.SendEvent(myEventData, new SendParameters());            // 要送給相反的人資料 (RoomActor1)Host會收到(RoomActor2)Gueest資料

                                            // 移除等待列表房間 等待房間永遠只有1
                                            _server.room.RemoveWaitingRoom(roomID, peerGuid);
                                        }
                                        else
                                        {
                                            Log.Error("Join Room Fail !  RoomID: " + _server.room.GetPlayingRoomFromGuid(peerGuid));
                                        }
                                    }
                                }
                                else
                                {
                                    // 等不到人 離開房間吧
                                    actor.GameStatus = (byte)ENUM_MemberState.Idle;
                                    OperationResponse response = new OperationResponse((byte)MatchGameResponseCode.ExitWaiting, null) { DebugMessage = "你是白癡喔 Bot還想排對戰！" };
                                    SendOperationResponse(response, new SendParameters());
                                    Log.Error("ERROR:　你是白癡喔 Bot還想排對戰！");
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Debug("Join Failed ! " + e.Message + "  於程式碼:" + e.StackTrace);
                            }
                            break;

                        #endregion

                        #region MatchGame(Bot) 配對遊戲
                        case (byte)MatchGameOperationCode.MatchGameBot:
                            Log.Debug("Match Game Bot");

                            try
                            {

                                int primaryID = (int)operationRequest.Parameters[(byte)MatchGameParameterCode.PrimaryID];
                                string myTeam = (string)operationRequest.Parameters[(byte)MatchGameParameterCode.Team];
                                MemberType memberType = (MemberType)operationRequest.Parameters[(byte)MemberParameterCode.MemberType];

                                Log.Debug("waitingList Count:" + _server.room.dictWaitingRoomList.Count);
                                Log.Debug("MemberType: " + ((MemberType)memberType).ToString());

                                if (memberType != (byte)MemberType.Bot /*&& _server.room.dictWaitingRoomList.Count != 0 && _server.room.GetWaitActorFromGuid(peerGuid, roomID) != null*/) //假如等待房間列表中沒房間 建立等待房間
                                {
                                    // get player bot 
                                    Actor actor = _server.Actors.GetActorFromPrimary(primaryID);  // 用primaryID取得角色資料
                                    Actor actorBot = _server.Actors.GetActorFromMemberType((byte)MemberType.Bot);
                                    int roomID = _server.room.GetWaitingRoomFromGuid(peerGuid);
                                    // add bot
                                    if (actorBot != null)
                                    {
                                        Log.Debug("等待配對的房間數量:" + _server.room.dictWaitingRoomList.Count + " Bot 加入房間 Join Room:" + _server.room.GetWaitingRoomFromGuid(peerGuid));
                                        if (_server.room.JoinRoom(actorBot.guid, actorBot.PrimaryID, actorBot.Account, actorBot.Nickname, actorBot.Age, actorBot.Sex, actorBot.IP))
                                        {
                                            roomID = _server.room.GetPlayingRoomFromGuid(peerGuid);
                                            PlayerData BotData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.LoadPlayerData(actorBot.Account));
                                            PlayerData playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.LoadPlayerData(actor.Account));

                                            Dictionary<byte, object> myDataParameter = new Dictionary<byte, object>() { { (byte)MatchGameParameterCode.PrimaryID, actor.PrimaryID }, { (byte)MatchGameParameterCode.Nickname, actor.Nickname }, { (byte)MatchGameParameterCode.RoomID, roomID }, { (byte)MatchGameParameterCode.Team, myTeam }, { (byte)PlayerDataParameterCode.PlayerImage, playerData.PlayerImage }, { (byte)MatchGameParameterCode.RoomPlace, "Guest" } };
                                            Dictionary<byte, object> botDataParameter = new Dictionary<byte, object>() { { (byte)MatchGameParameterCode.PrimaryID, actorBot.PrimaryID }, { (byte)MatchGameParameterCode.Nickname, actorBot.Nickname }, { (byte)MatchGameParameterCode.RoomID, roomID }, { (byte)MatchGameParameterCode.Team, BotData.Team }, { (byte)PlayerDataParameterCode.PlayerImage, BotData.PlayerImage }, { (byte)MatchGameParameterCode.RoomPlace, "Host" } };

                                            // 玩家2加入房間成功後 發送配對成功事件給房間內兩位玩家
                                            EventData myEventData = new EventData((byte)MatchGameResponseCode.Match, myDataParameter);
                                            EventData botEventData = new EventData((byte)MatchGameResponseCode.Match, botDataParameter);
                                            MPServerPeer peerMe, peerBot;

                                            //取得雙方玩家的peer
                                            peerMe = _server.Actors.GetPeerFromGuid(this.peerGuid);         // 加入房間的人
                                            peerBot = _server.Actors.GetPeerFromGuid(actorBot.guid);    // 建立房間的人

                                            // 改變雙方玩家狀態
                                            actor.GameStatus = (byte)ENUM_MemberState.Playing;
                                            actorBot.GameStatus = (byte)ENUM_MemberState.Playing;

                                            Log.Debug("My Peer:" + this.peerGuid + "  " + actor.Nickname);
                                            Log.Debug("Bot Peer:" + actorBot.guid + "  " + actorBot.Nickname);

                                            peerMe.SendEvent(botEventData, new SendParameters());           // 要送給相反的人資料 (RoomActor2)Gueest會收到(RoomActor1)Host資料
                                            peerBot.SendEvent(myEventData, new SendParameters());              // 要送給相反的人資料 (RoomActor1)Host會收到(RoomActor2)Gueest資料

                                            // 移除等待列表房間
                                            _server.room.RemoveWaitingRoom(roomID, peerGuid);
                                        }
                                        else
                                        {
                                            _server.room.RemoveWaitingRoom(roomID, peerGuid);    // 移除等待房間

                                            actor.GameStatus = (byte)ENUM_MemberState.Idle;

                                            // 等不到Bot 離開房間吧
                                            OperationResponse response = new OperationResponse((byte)MatchGameResponseCode.ExitWaiting, new Dictionary<byte, object>()) { DebugMessage = "等不到Bot，加入房間失敗!" };
                                            SendOperationResponse(response, new SendParameters());
                                            Log.Error("等不到Bot加入房間失敗! ERROR: Bot OffOnline !  Player: ( " + actor.Account + " ) has been removed!" + "RoomID: " + _server.room.GetPlayingRoomFromGuid(peerGuid));
                                        }
                                    }
                                    else
                                    {
                                        _server.room.RemoveWaitingRoom(roomID, peerGuid);    // 移除等待房間
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object>();

                                        // 等不到人 離開房間吧
                                        OperationResponse response = new OperationResponse((byte)MatchGameResponseCode.ExitWaiting, parameter) { DebugMessage = "等不到Bot，加入房間失敗!" };
                                        SendOperationResponse(response, new SendParameters());
                                        Log.Error("等不到Bot加入房間失敗! ERROR: Bot OffOnline !  Player: ( " + actor.Account + " ) has been removed!" + "RoomID: " + _server.room.GetPlayingRoomFromGuid(peerGuid));
                                    }
                                }
                                else if (memberType == (byte)MemberType.Bot)
                                {
                                    // 等不到人 離開房間吧
                                    OperationResponse response = new OperationResponse((byte)MatchGameResponseCode.ExitWaiting, new Dictionary<string, object>()) { DebugMessage = "你是白癡喔 Bot還想排對戰！" };
                                    SendOperationResponse(response, new SendParameters());
                                    Log.Error("ERROR:　你是白癡喔 Bot還想排對戰！");
                                }

                            }
                            catch (Exception e)
                            {
                                Log.Debug("Join Failed ! " + e.Message + "  於程式碼:" + e.StackTrace);
                            }
                            break;

                        #endregion

                        #region MatchGameFriend 好友配對遊戲
                        case (byte)MatchGameOperationCode.MatchGameFriend:
                            Log.Debug("-------------------MatchGameFriend--------------------");
                            try
                            {
                                int primaryID = (int)operationRequest.Parameters[(byte)MatchGameParameterCode.PrimaryID];
                                string otherAccount = (string)operationRequest.Parameters[(byte)MatchGameParameterCode.OtherAccount];
                                string team = (string)operationRequest.Parameters[(byte)MatchGameParameterCode.Team];
                                // string otherTeam = (string)operationRequest.Parameters[(byte)MatchGameParameterCode.OtherTeam];

                                MemberType matchMemberType = (MemberType)operationRequest.Parameters[(byte)MemberParameterCode.MemberType];

                                Actor actor = _server.Actors.GetActorFromPrimary(primaryID);  // 用primaryID取得角色資料
                                Actor otherActor = _server.Actors.GetActorFromAccount(otherAccount);  // 用primaryID取得角色資料

                                if (matchMemberType != (byte)MemberType.Bot)
                                {
                                    isCreateRoom = _server.room.CreatePrivateRoom(peerGuid, otherActor.guid, actor.PrimaryID, otherActor.Account, actor.Nickname, actor.Age, actor.Sex, actor.IP);

                                    // !isCreateRoom = 已有建立的房間
                                    if (!isCreateRoom && _server.room.JoinPrivateRoom(actor.guid, otherActor.guid, actor.PrimaryID, actor.Account, actor.Nickname, actor.Age, actor.Sex, actor.IP))
                                    {
                                        Log.Debug("  加入好友的房間 加入房間 Join Room:" + _server.room.GetPlayingRoomFromGuid(actor.guid));

                                        Room.RoomActor otherRoomActor;

                                        int roomID = _server.room.GetPlayingRoomFromGuid(peerGuid);
                                        otherRoomActor = _server.room.GetPlayingRoomOtherPlayer(roomID, primaryID);


                                        PlayerData otherPlayerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.LoadPlayerData(otherRoomActor.Account));
                                        PlayerData playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.LoadPlayerData(actor.Account));

                                        Dictionary<byte, object> myParameter = new Dictionary<byte, object>() { { (byte)MatchGameParameterCode.PrimaryID, actor.PrimaryID }, { (byte)MatchGameParameterCode.Nickname, actor.Nickname }, { (byte)MatchGameParameterCode.RoomID, roomID }, { (byte)MatchGameParameterCode.Team, team }, { (byte)PlayerDataParameterCode.PlayerImage, playerData.PlayerImage }, { (byte)MatchGameParameterCode.RoomPlace, "Guest" } };
                                        Dictionary<byte, object> otherParameter = new Dictionary<byte, object>() { { (byte)MatchGameParameterCode.PrimaryID, otherRoomActor.PrimaryID }, { (byte)MatchGameParameterCode.Nickname, otherRoomActor.Nickname }, { (byte)MatchGameParameterCode.RoomID, roomID }, { (byte)MatchGameParameterCode.Team, otherPlayerData.Team }, { (byte)PlayerDataParameterCode.PlayerImage, otherPlayerData.PlayerImage }, { (byte)MatchGameParameterCode.RoomPlace, "Host" } };

                                        OperationResponse response = new OperationResponse((byte)MatchGameResponseCode.ExitWaiting, myParameter) { DebugMessage = "配對成功，離開等待房間。" };
                                        SendOperationResponse(response, new SendParameters());

                                        // 玩家2加入房間成功後 發送配對成功事件給房間內兩位玩家
                                        EventData myEventData = new EventData((byte)MatchGameResponseCode.Match, myParameter);
                                        EventData otherEventData = new EventData((byte)MatchGameResponseCode.Match, otherParameter);
                                        MPServerPeer peerMe, peerOther;

                                        //取得雙方玩家的peer
                                        peerMe = _server.Actors.GetPeerFromGuid(this.peerGuid);         // 加入房間的人
                                        peerOther = _server.Actors.GetPeerFromGuid(otherRoomActor.guid);    // 建立房間的人

                                        // 改變雙方玩家狀態
                                        actor.GameStatus = (byte)ENUM_MemberState.Playing;
                                        otherActor.GameStatus = (byte)ENUM_MemberState.Playing;

                                        Log.Debug("My Peer:" + this.peerGuid + "  " + actor.Nickname);
                                        Log.Debug("Other Peer:" + otherRoomActor.guid + "  " + otherRoomActor.Nickname);

                                        peerMe.SendEvent(otherEventData, new SendParameters());           // 要送給相反的人資料 (RoomActor2)Gueest會收到(RoomActor1)Host資料
                                        peerOther.SendEvent(myEventData, new SendParameters());            // 要送給相反的人資料 (RoomActor1)Host會收到(RoomActor2)Gueest資料

                                        // 移除等待列表房間 等待房間永遠只有1
                                        _server.room.RemoveWaitingRoom(roomID, peerGuid);
                                    }
                                    else
                                    {
                                        Log.Error("建立好友對戰房間  RoomID: " + _server.room.GetWaitingRoomFromGuid(peerGuid));
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Debug("Join Failed ! " + e.Message + "  於程式碼:" + e.StackTrace);
                            }
                            break;

                        #endregion

                        #region InviteMatchGame 邀請好友遊戲
                        case (byte)MatchGameOperationCode.InviteMatchGame:
                            Log.Debug("-------------------InviteMatchGame Friend--------------------");
                            // 取得對手peer並傳送配對資訊

                            try
                            {
                                string account = (string)operationRequest.Parameters[(byte)MatchGameParameterCode.Account];
                                string otherAccount = (string)operationRequest.Parameters[(byte)MatchGameParameterCode.OtherAccount];
                                MemberType memberType = (MemberType)operationRequest.Parameters[(byte)MemberParameterCode.MemberType];

                                Actor actor = _server.Actors.GetActorFromAccount(account);
                                Actor otherActor = _server.Actors.GetActorFromAccount(otherAccount);  // 用primaryID取得角色資料
                                Log.Debug("otherAccount: " + otherAccount + "memberType: " + memberType);
                                MPServerPeer otherPeer;



                                if (memberType != (byte)MemberType.Bot && otherActor != null)
                                {
                                    otherPeer = _server.Actors.GetPeerFromGuid(otherActor.guid);
                                    Log.Debug("otherActor.guid: " + otherActor.guid + "  " + otherActor.Nickname + "otherPeer: " + otherPeer.LocalIP);
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)MatchGameParameterCode.OtherAccount, account }, { (byte)MatchGameParameterCode.Nickname, actor.Nickname } };
                                    EventData eventData = new EventData((byte)MatchGameResponseCode.InviteMatchGame, parameter);
                                    otherPeer.SendEvent(eventData, new SendParameters());
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Debug("Join Failed ! " + e.Message + "  於程式碼:" + e.StackTrace);
                            }
                            break;

                        #endregion

                        #region ApplyMatchGameFriend 同意好友對戰
                        case (byte)MatchGameOperationCode.ApplyMatchGameFriend:
                            {
                                try
                                {
                                    Log.Debug("------------ ApplyMatchGameFriend ------------");
                                    string account = (string)operationRequest.Parameters[(byte)MatchGameParameterCode.Account];
                                    string otherAccount = (string)operationRequest.Parameters[(byte)MatchGameParameterCode.OtherAccount];
                                    MemberType memberType = (MemberType)operationRequest.Parameters[(byte)MemberParameterCode.MemberType];

                                    Actor otherActor = _server.Actors.GetActorFromAccount(otherAccount);  // 用primaryID取得角色資料
                                    MPServerPeer otherPeer = _server.Actors.GetPeerFromGuid(otherActor.guid);
                                    Dictionary<byte, object> parameter;
                                    EventData eventData;

                                    if (memberType != (byte)MemberType.Bot && otherActor != null)
                                    {
                                        otherPeer = _server.Actors.GetPeerFromGuid(otherActor.guid);
                                        Log.Debug("otherActor.guid: " + otherActor.guid + "  " + otherActor.Nickname + "otherPeer: " + otherPeer.LocalIP);
                                        parameter = new Dictionary<byte, object> { { (byte)MatchGameParameterCode.OtherAccount, account } };
                                        eventData = new EventData((byte)MatchGameResponseCode.ApplyMatchGameFriend, parameter);
                                        otherPeer.SendEvent(eventData, new SendParameters());
                                    }

                                    parameter = new Dictionary<byte, object> { { (byte)MatchGameParameterCode.OtherAccount, otherAccount } };
                                    eventData = new EventData((byte)MatchGameResponseCode.ApplyMatchGameFriend, parameter);
                                    SendEvent(eventData, new SendParameters());
                                }
                                catch
                                {
                                    throw;
                                }
                                break;
                            }
                        #endregion

                        #region SyncGameStart 同步開始遊戲
                        case (byte)MatchGameOperationCode.SyncGameStart:
                            {
                                try
                                {
                                    int primaryID = (int)operationRequest.Parameters[(byte)MatchGameParameterCode.PrimaryID];
                                    int roomID = _server.room.GetPlayingRoomFromGuid(peerGuid);
                                    Actor actor = _server.Actors.GetActorFromGuid(peerGuid);
                                    Log.Debug("------------------SyncGameStart-------------------\n primaryID:" + primaryID + " peerGuid:" + peerGuid + " roomID:" + roomID);

                                    if (roomID > 0)
                                    {
                                        _server.room.AddGameLoaded(roomID, primaryID, peerGuid);
                                        Dictionary<int, Guid> loadedPlayer = new Dictionary<int, Guid>(_server.room.GetLoadedPlayerFromRoom(roomID));

                                        foreach (KeyValuePair<int, Guid> item in loadedPlayer)
                                            Log.Debug(" loadedPlayer ID:" + item.Key + " Guid:" + item.Value);

                                        // 如果還沒載入 回傳 請等待開始
                                        if (loadedPlayer.Count != maxNumPlayer)
                                        {
                                            Dictionary<byte, object> parameter = new Dictionary<byte, object>();
                                            OperationResponse response = new OperationResponse((byte)MatchGameResponseCode.WaitingGameStart, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = "WaitingPlayer" };
                                            SendOperationResponse(response, new SendParameters());
                                        }
                                        else// 如果已經載入房間的玩家 = 房間上限 同步開始
                                        {
                                            MPServerPeer peerOther = null;
                                            Room.RoomActor otherActor;


                                            otherActor = _server.room.GetPlayingRoomOtherPlayer(roomID, primaryID);


                                            Log.Debug("RoomID:" + roomID);
                                            Log.Debug("otherActor" + otherActor.guid);
                                            peerOther = _server.Actors.GetPeerFromGuid(otherActor.guid);


                                            // 如果再同步時，對方斷線，離開房間
                                            if (otherActor == null)
                                            {
                                                Log.Debug("IN ExitRoom");

                                                // 移除BOSS資訊 如果有的話
                                                _server.room.KillBoss(roomID);
                                                _server.room.RemovePlayingRoom(roomID, actor.guid, actor.PrimaryID);
                                                Log.Debug("playingRoomList Count:" + _server.room.dictPlayingRoomList.Count);

                                                if (actor != null) actor.GameStatus = (byte)ENUM_MemberState.Idle;
                                                OperationResponse response = new OperationResponse((byte)BattleOperationCode.ExitRoom, new Dictionary<byte, object> { }) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = "已離開房間！" };
                                                SendOperationResponse(response, new SendParameters());
                                            }
                                            else
                                            {
                                                Log.Debug("SyncGameStart RoomID: " + roomID + "Account:" + actor.Account + " peerGuid:" + peerGuid);
                                                Log.Debug("SyncGameStart RoomID: " + roomID + "Account:" + otherActor.Account + " peerGuid:" + otherActor.guid);

                                                Dictionary<byte, object> parameter = new Dictionary<byte, object>() { { (byte)MatchGameParameterCode.ServerTime, DateTime.Now.ToString("yyyyMMddHHmmss") }, { (byte)MatchGameParameterCode.GameTime, GameTime } };
                                                EventData eventData = new EventData((byte)MatchGameResponseCode.SyncGameStart, parameter);
                                                peerOther.SendEvent(eventData, new SendParameters());    // 回傳給另外一位玩家
                                                this.SendEvent(eventData, new SendParameters());         // 回傳給自己
                                            }
                                            _server.room.RemoveLoadedRoom(roomID);
                                        }
                                    }
                                    else // 同步前斷線了 房間被移除
                                    {
                                        _server.room.KillBoss(roomID);
                                        _server.room.RemovePlayingRoom(roomID, actor.guid, actor.PrimaryID);
                                        Log.Debug("playingRoomList Count:" + _server.room.dictPlayingRoomList.Count);

                                        if (actor != null) actor.GameStatus = (byte)ENUM_MemberState.Idle;
                                        OperationResponse response = new OperationResponse((byte)BattleOperationCode.ExitRoom, new Dictionary<byte, object> { }) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = "已離開房間！" };
                                        SendOperationResponse(response, new SendParameters());
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
                        case (byte)BattleOperationCode.ExitRoom:    // 目前是KickOther 之後評審完改回
                            {
                                try
                                {
                                    // Kickother版本
                                    Log.Debug("IN ExitRoom");

                                    int roomID = _server.room.GetPlayingRoomFromGuid(peerGuid);
                                    int primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];
                                    Log.Debug("RoomID: " + roomID);

                                    // 移除BOSS資訊 如果有的話
                                    _server.room.KillBoss(roomID);
                                    // 取得雙方角色
                                    Actor actor = _server.Actors.GetActorFromGuid(peerGuid);
                                    Room.RoomActor otherRoomActor = _server.room.GetPlayingRoomOtherPlayer(roomID, actor.PrimaryID);

                                    // 移除遊戲中房間、玩家
                                    if (otherRoomActor != null)
                                        _server.room.RemovePlayingRoom(roomID, actor.guid, -1);
                                    else
                                        _server.room.RemovePlayingRoom(roomID, actor.guid, actor.PrimaryID);

                                    Log.Debug("playingRoomList Count:" + _server.room.dictPlayingRoomList.Count);

                                    actor = _server.Actors.GetActorFromGuid(peerGuid);
                                    actor.GameStatus = (byte)ENUM_MemberState.Idle;

                                    OperationResponse response = new OperationResponse(operationRequest.OperationCode, new Dictionary<byte, object>()) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = "已離開房間！" };
                                    SendOperationResponse(response, new SendParameters());



                                    Log.Debug("IN KickOther RoomID: " + roomID);

                                    // 取得對方玩家>用GUID找Peer
                                    otherRoomActor = _server.room.GetPlayingRoomOtherPlayer(roomID, primaryID);
                                    if (otherRoomActor != null)
                                    {
                                        Log.Debug("找到要踢的玩家房間 RoomID: " + roomID);
                                        MPServerPeer peerOther = _server.Actors.GetPeerFromGuid(otherRoomActor.guid);

                                        actor = _server.Actors.GetActorFromGuid(peerOther.peerGuid);
                                        actor.GameStatus = (byte)ENUM_MemberState.Idle;

                                        // 把他給踢了
                                        OperationResponse response2 = new OperationResponse(operationRequest.OperationCode, new Dictionary<byte, object>()) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = "已離開房間！" };
                                        peerOther.SendOperationResponse(response2, new SendParameters());
                                    }


                                    // Exit Room 版本
                                    //Log.Debug("IN ExitRoom");

                                    //int roomID = (int)operationRequest.Parameters[(byte)BattleParameterCode.RoomID];
                                    //int primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];
                                    //Log.Debug("RoomID: " + roomID);
                                    ////to do exit room
                                    ////Log.Debug("1 playingRoomList Count:" + _server.room.playingRoomList.Count);
                                    ////Log.Debug("in Dictionary 1 :" + _server.room.playingRoomList[roomID].ContainsKey("RoomActor1"));
                                    ////Log.Debug("in Dictionary 2 :" + _server.room.playingRoomList[roomID].ContainsKey("RoomActor2"));

                                    //// 移除BOSS資訊 如果有的話
                                    //_server.room.KillBoss(roomID);
                                    //// 取得雙方角色
                                    //Actor actor = _server.Actors.GetActorFromGuid(peerGuid);
                                    //Room.RoomActor otherActor = _server.room.GetOtherPlayer(roomID, actor.PrimaryID);

                                    //// 移除遊戲中房間、玩家
                                    //if (otherActor != null)
                                    //    _server.room.RemovePlayingRoom(roomID, actor.guid, otherActor.guid);
                                    //else
                                    //    _server.room.RemovePlayingRoom(roomID, actor.guid, peerGuid);
                                    //Log.Debug("playingRoomList Count:" + _server.room.dictPlayingRoomList.Count);
                                    //Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                    //actor = _server.Actors.GetActorFromGuid(peerGuid);
                                    //actor.GameStatus = (byte)ENUM_MemberState.Idle;
                                    //OperationResponse response = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = "已離開房間！" };
                                    //SendOperationResponse(response, new SendParameters());

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
                                    Log.Debug("IN Exit Waiting Room");
                                    int roomID = _server.room.GetWaitingRoomFromGuid(peerGuid);
                                    _server.room.RemoveWaitingRoom(roomID, peerGuid);    // 移除等待房間
                                    Log.Debug("Remove WaitingRoom : " + roomID);
                                    Actor actor = _server.Actors.GetActorFromGuid(peerGuid);
                                    actor.GameStatus = (byte)ENUM_MemberState.Idle;
                                    // 等不到人 離開房間吧
                                    OperationResponse response = new OperationResponse((byte)MatchGameResponseCode.ExitWaiting, new Dictionary<byte, object>()) { DebugMessage = "等待超時，已離開房間！" };
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
                                    Room.RoomActor otherRoomActor;

                                    int roomID = _server.room.GetPlayingRoomFromGuid(peerGuid);
                                    int primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];
                                    Log.Debug("IN KickOther RoomID: " + roomID);

                                    // 取得對方玩家>用GUID找Peer
                                    otherRoomActor = _server.room.GetPlayingRoomOtherPlayer(roomID, primaryID);

                                    // 移除遊戲中房間、玩家

                                    //if (otherRoomActor != null)
                                    //    _server.room.RemovePlayingRoom(roomID, peerGuid, -1);
                                    //else
                                    _server.room.RemovePlayingRoom(roomID, peerGuid, primaryID);


                                    if (otherRoomActor != null)
                                    {
                                        Log.Debug("找到要踢的玩家房間 RoomID: " + roomID);
                                        MPServerPeer peerOther = _server.Actors.GetPeerFromGuid(otherRoomActor.guid);

                                        Actor actor = _server.Actors.GetActorFromGuid(peerGuid);
                                        actor.GameStatus = (byte)ENUM_MemberState.Idle;

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
                                    //    Log.Debug("IN CheckStatus");

                                    _server.room.GetPlayingRoomFromGuid(peerGuid);
                                    int primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];
                                    int roomID = _server.room.GetPlayingRoomFromGuid(peerGuid);
                                    Room.RoomActor otherActor;//= new Room.RoomActor(actor.guid, actor.PrimaryID, actor.Account, actor.Nickname, actor.Age, actor.Sex, actor.IP); //這裡為了不要再增加變數 所以偷懶使用 actor.XX 正確是沒有actor.
                                    MPServerPeer peer;

                                    otherActor = _server.room.GetPlayingRoomOtherPlayer(roomID, primaryID);    // 取得其他玩家

                                    if (otherActor == null) // 如果他跑了 我也玩不了 離開房間
                                    {

                                        _server.room.RemovePlayingRoom(roomID, peerGuid, primaryID);
                                        peer = _server.Actors.GetPeerFromGuid(peerGuid);
                                        Actor actor = _server.Actors.GetActorFromGuid(peerGuid);
                                        actor.GameStatus = (byte)ENUM_MemberState.Idle;
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object>() { { (byte)BattleResponseCode.DebugMessage, "配對的玩家已經離開房間！" } };
                                        EventData eventData = new EventData((byte)BattleResponseCode.Offline, parameter);
                                        peer.SendEvent(eventData, new SendParameters());
                                    }
                                    //else
                                    //{
                                    //    // 移除遊戲中房間、玩家
                                    //    _server.room.RemovePlayingRoom(roomID, peerGuid, peerGuid);
                                    //}
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
                                try
                                {
                                    short miceID = (short)operationRequest.Parameters[(byte)BattleParameterCode.MiceID];
                                    int roomID = _server.room.GetPlayingRoomFromGuid(peerGuid);
                                    int primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];
                                    int energy = (int)operationRequest.Parameters[(byte)BattleParameterCode.Energy];

                                    Room.RoomActor roomActor, otherRoomActor;

                                    roomActor = _server.room.GetActorFromGuid(peerGuid);
                                    if (roomActor != null) roomActor.energy -= (float)energy;

                                    otherRoomActor = _server.room.GetPlayingRoomOtherPlayer(roomID, primaryID);    // 找對手

                                    if (otherRoomActor != null)
                                    {
                                        MPServerPeer peerOther = _server.Actors.GetPeerFromGuid(otherRoomActor.guid);    // 對手GUID找Peer

                                        // 送給對手 技能傷害
                                        Dictionary<byte, object> skillParameter = new Dictionary<byte, object>() { { (byte)BattleParameterCode.MiceID, miceID }, { (byte)BattleResponseCode.DebugMessage, "" } };
                                        EventData skillEventData = new EventData((byte)BattleResponseCode.ApplySkillMice, skillParameter);
                                        peerOther.SendEvent(skillEventData, new SendParameters());
                                    }
                                    else
                                    {
                                        // to do
                                    }

                                }
                                catch
                                {
                                    throw;
                                }
                            }
                            break;
                        #endregion

                        #region SendSkillItem 發動道具技能傷害
                        case (byte)BattleOperationCode.SendSkillItem:
                            {

                                try
                                {
                                    int itemID = (int)operationRequest.Parameters[(byte)ItemParameterCode.ItemID];
                                    short skillType = (short)operationRequest.Parameters[(byte)SkillParameterCode.SkillType];
                                    int roomID = _server.room.GetPlayingRoomFromGuid(peerGuid);
                                    int primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];

                                    Log.Debug("IN SendSkillItem ID: " + itemID + "   Type: " + skillType);

                                    Dictionary<byte, object> skillParameter = new Dictionary<byte, object>() { { (byte)ItemParameterCode.ItemID, itemID }, { (byte)BattleResponseCode.DebugMessage, "" } };
                                    EventData skillEventData = new EventData((byte)BattleResponseCode.ApplySkillItem, skillParameter);

                                    // 如果是傷害技能 傳送給對手
                                    if ((ENUM_SkillType)skillType == ENUM_SkillType.Damage)
                                    {
                                        Room.RoomActor otherActor;
                                        otherActor = _server.room.GetPlayingRoomOtherPlayer(roomID, primaryID);    // 找對手
                                        MPServerPeer peerOther = _server.Actors.GetPeerFromGuid(otherActor.guid);    // 對手GUID找Peer

                                        // 送給對手 技能傷害
                                        peerOther.SendEvent(skillEventData, new SendParameters());
                                    }
                                    else
                                    {
                                        // 回傳給自己
                                        this.SendEvent(skillEventData, new SendParameters());
                                    }

                                }
                                catch
                                {
                                    throw;
                                }
                                break;

                            }
                        #endregion

                        #region SkillBoss 發動技能BOSS老鼠技能
                        case (byte)BattleOperationCode.SkillBoss:
                            {
                                Log.Debug("IN Send BOSS Skill");


                                int roomID = _server.room.GetPlayingRoomFromGuid(peerGuid);
                                int primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];
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
                                    Room.RoomActor otherRoomActor;
                                    otherRoomActor = _server.room.GetPlayingRoomOtherPlayer(roomID, primaryID);    // 找對手
                                    if (otherRoomActor != null)
                                    {
                                        MPServerPeer peerOther = _server.Actors.GetPeerFromGuid(otherRoomActor.guid);    // 對手GUID找Peer

                                        // 送給對手 技能傷害

                                        EventData skillEventData = new EventData((byte)BattleResponseCode.SkillBoss, skillParameter);
                                        peerOther.SendEvent(skillEventData, new SendParameters());
                                    }
                                    else
                                    {
                                        //todo
                                    }
                                }
                            }
                            break;
                        #endregion




                        #region UpdatePlayerData 更新玩家資料
                        case (byte)PlayerDataOperationCode.UpdatePlayer:
                            {
                                Log.Debug("IN UpdatePlayerData");

                                if (operationRequest.Parameters.ContainsKey((byte)PlayerDataParameterCode.PlayerImage))
                                {
                                    string account = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Account];
                                    string imageName = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.PlayerImage];
                                    playerDataUI.UpdatePlayerData(account, imageName);
                                }
                                else
                                {
                                    string account = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Account];
                                    byte rank = (byte)operationRequest.Parameters[(byte)PlayerDataParameterCode.Rank];
                                    short exp = (byte)operationRequest.Parameters[(byte)PlayerDataParameterCode.Exp];
                                    Int16 maxCombo = (Int16)operationRequest.Parameters[(byte)PlayerDataParameterCode.MaxCombo];
                                    int maxScore = (int)operationRequest.Parameters[(byte)PlayerDataParameterCode.MaxScore];
                                    int sumScore = (int)operationRequest.Parameters[(byte)PlayerDataParameterCode.SumScore];
                                    int sumLost = (int)operationRequest.Parameters[(byte)PlayerDataParameterCode.SumLost];
                                    int sumKill = (int)operationRequest.Parameters[(byte)PlayerDataParameterCode.SumKill];

                                    string item = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.SortedItem];              // Json資料
                                    string miceAll = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.MiceAll];        // Json資料
                                    string team = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Team];              // Json資料
                                    string friend = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Friend];          // Json資料

                                    PlayerDataUI playerDataUI = new PlayerDataUI(); //實體化 IO (連結資料庫拿資料)
                                    PlayerData playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.UpdatePlayerData(account, rank, exp, maxCombo, maxScore, sumScore, sumLost, sumKill, item, miceAll, team, friend)); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找

                                    if (playerData.ReturnCode == "S403")//取得玩家資料成功 回傳玩家資料
                                    {
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)PlayerDataParameterCode.Ret, playerData.ReturnCode }, { (byte)PlayerDataParameterCode.Rank, rank }, { (byte)PlayerDataParameterCode.Exp, exp }, 
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
                            }
                            break;
                        #endregion

                        #region UpdateMiceData 更新玩家(Team)資料
                        case (byte)PlayerDataOperationCode.UpdateMice:
                            {
                                string account = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Account];
                                string miceAll = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.MiceAll];        // Json資料
                                string team = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Team];              // Json資料

                                PlayerDataUI playerDataUI = new PlayerDataUI(); //實體化 IO (連結資料庫拿資料)
                                PlayerData playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.UpdatePlayerData(account, miceAll, team)); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找

                                if (playerData.ReturnCode == "S420")//取得玩家資料成功 回傳玩家資料
                                {
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
                                    int itemID = (int)operationRequest.Parameters[(byte)StoreParameterCode.ItemID];
                                    playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.UpdatePlayerItem(account, itemID, isEquip)); // 更新裝備狀態
                                }
                                else// 更新數量
                                {
                                    string jItemUsage = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.UseCount];
                                    string[] columns = (string[])operationRequest.Parameters[(byte)PlayerDataParameterCode.Columns];
                                    Log.Debug("jItemUsage:  " + jItemUsage);
                                    playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.UpdatePlayerItem(account, jItemUsage, columns)); // 更新道具數量
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
                                    // Log.Debug("playerData.ReturnCode == S428");

                                    playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.LoadPlayerData(account)); // 更新道具數量
                                    Log.Debug(playerData.SortedItem);
                                    if (playerData.ReturnCode == "S401")//取得玩家資料成功 回傳玩家資料
                                    {
                                        // Log.Debug("playerData.ReturnCode == S401");
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

                                BattleData battleData = (BattleData)TextUtility.DeserializeFromStream(battleUI.UpdateScoreRate((ENUM_Rate)rate));

                                Room.RoomActor roomActor = _server.room.GetActorFromGuid(peerGuid);
                                if (roomActor != null) roomActor.scoreRate = battleData.scoreRate;

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

                                BattleData battleData = (BattleData)TextUtility.DeserializeFromStream(battleUI.UpdateEnergyRate((ENUM_Rate)rate));

                                Room.RoomActor roomActor = _server.room.GetActorFromGuid(peerGuid);
                                if (roomActor != null) roomActor.energyRate = battleData.energyRate;

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

                        #region UpdateScore 更新分數 還沒寫好
                        case (byte)BattleOperationCode.UpdateScore:
                            {
                                try
                                {
                                    //Log.Debug("IN UpdateScore");

                                    int primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];
                                    int roomID = _server.room.GetPlayingRoomFromGuid(peerGuid);
                                    int combo = (int)operationRequest.Parameters[(byte)BattleParameterCode.Combo];

                                    short miceID = (short)operationRequest.Parameters[(byte)BattleParameterCode.MiceID];
                                    float aliveTime = (float)operationRequest.Parameters[(byte)BattleParameterCode.Time];

                                    Room.RoomActor roomActor = _server.room.GetActorFromGuid(peerGuid);
                                    if (roomActor != null)
                                    {

                                        //  Log.Debug("BattleUI OK:" + miceID + "actor:" + _server.room.GetActorFromGuid(peerGuid).Nickname);
                                        BattleData battleData = (BattleData)TextUtility.DeserializeFromStream(battleUI.ClacScore(miceID, aliveTime, roomActor.scoreRate, combo, roomActor.energyRate)); //計算分數
                                        //   Log.Debug("BattleData OK : " + battleData.score);


                                        // score = battleData.score;

                                        if (battleData.ReturnCode == "S501")//計算分數成功 回傳玩家資料
                                        {
                                            //   Log.Debug("peerGuid:" + peerGuid);
                                            //  Log.Debug("roomActor Nickname:" + roomActor.Nickname + "  " + roomActor.gameScore);

                                            roomActor.gameScore += battleData.score;
                                            roomActor.energy += battleData.energy;
                                            roomActor.energy = Math.Min(roomActor.energy, 100);
                                            if (roomActor.gameScore < 0) roomActor.gameScore = 0;
                                            Log.Debug("-------------DevTest-----------" + roomActor.energy);
                                            //回傳給原玩家
                                            //Log.Debug("battleData.ReturnCode == S501");
                                            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)BattleParameterCode.Ret, battleData.ReturnCode }, { (byte)BattleParameterCode.Score, battleData.score } , { (byte)BattleParameterCode.Energy, Convert.ToInt16(battleData.energy)} 
                                    };


                                            OperationResponse response = new OperationResponse((byte)BattleResponseCode.UpdateScore, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = battleData.ReturnMessage.ToString() }; // 改過 battleData.ReturnMessage.ToString()
                                            SendOperationResponse(response, new SendParameters());

                                            //回傳給另外一位玩家
                                            Room.RoomActor otherActor = _server.room.GetPlayingRoomOtherPlayer(roomID, primaryID);
                                            if (otherActor != null)
                                            {
                                                MPServerPeer peerOther = _server.Actors.GetPeerFromGuid(otherActor.guid);
                                                Dictionary<byte, object> parameter2 = new Dictionary<byte, object>() { { (byte)BattleParameterCode.OtherScore, battleData.score }, { (byte)BattleParameterCode.Energy, Convert.ToInt16(roomActor.energy) }, { (byte)BattleResponseCode.DebugMessage, "取得對方分數資料" } };
                                                EventData getScoreEventData = new EventData((byte)BattleResponseCode.GetScore, parameter2);

                                                peerOther.SendEvent(getScoreEventData, new SendParameters());
                                            }
                                        }
                                        else
                                        {
                                            Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                            OperationResponse actorResponse = new OperationResponse((byte)BattleResponseCode.UpdateScore, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = battleData.ReturnMessage.ToString() };
                                            SendOperationResponse(actorResponse, new SendParameters());
                                        }
                                    }
                                    else
                                    {
                                        EventData eventData = new EventData((byte)BattleResponseCode.Offline, new Dictionary<byte, object>());
                                        SendEvent(eventData, new SendParameters());
                                        _server.room.KillBoss(roomID);
                                        // 移除遊戲中房間、玩家
                                        _server.room.RemovePlayingRoom(roomID, peerGuid, primaryID);

                                    }
                                }
                                catch (Exception e)
                                {
                                    Log.Debug("發生例外情況: " + e.Message + " 於: " + e.StackTrace);
                                }

                            }
                            break;
                        #endregion

                        #region UpdateLife 更新生命
                        case (byte)BattleOperationCode.UpdateLife:
                            {
                                try
                                {
                                    //Log.Debug("IN UpdateScore");

                                    int primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];
                                    int roomID = (int)operationRequest.Parameters[(byte)BattleParameterCode.RoomID];
                                    short life = (short)operationRequest.Parameters[(byte)BattleParameterCode.Life];
                                    bool bSetDefaultLife = (bool)operationRequest.Parameters[(byte)BattleParameterCode.CustomValue];

                                    Dictionary<byte, object> parameter;
                                    MPServerPeer otherPeer = null;
                                    Room.RoomActor roomActor = _server.room.GetActorFromGuid(peerGuid);
                                    Room.RoomActor roomOtherActor = _server.room.GetPlayingRoomOtherPlayer(roomID, primaryID);

                                    if (roomActor != null)
                                    {
                                        roomActor.life = Math.Max((short)0, bSetDefaultLife ? life : roomActor.life += life);

                                        if (roomActor != null)
                                        {
                                            // 回傳玩家生命
                                            parameter = new Dictionary<byte, object>() { { (byte)BattleParameterCode.Life, roomActor.life } };
                                            OperationResponse response = new OperationResponse((byte)BattleResponseCode.UpdateLife, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = "取得生命資料." };
                                            SendOperationResponse(response, new SendParameters());
                                        }



                                        // 回傳給另外一位玩家
                                        if (roomOtherActor != null)
                                        {
                                            Log.Debug("Other GUID:" + roomOtherActor.guid);
                                            otherPeer = _server.Actors.GetPeerFromGuid(roomOtherActor.guid);
                                            parameter = new Dictionary<byte, object>() { { (byte)BattleParameterCode.Life, roomActor.life }, { (byte)BattleResponseCode.DebugMessage, "取得對方生命資料." } };
                                            EventData eventData = new EventData((byte)BattleResponseCode.GetLife, parameter);
                                            otherPeer.SendEvent(eventData, new SendParameters());
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

                        #region LoadPlayerData 載入玩家資料
                        case (byte)PlayerDataOperationCode.LoadPlayer:
                            {
                                // Log.Debug("IN LoadPlayerData");

                                string account = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Account];

                                PlayerDataUI playerDataUI = new PlayerDataUI(); //初始化 IO (連結資料庫拿資料)
                                // Log.Debug("IO OK");
                                PlayerData playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.LoadPlayerData(account)); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找
                                // Log.Debug("Data OK");

                                byte rank = playerData.Rank;
                                short exp = playerData.Exp;
                                Int16 maxCombo = playerData.MaxCombo;
                                int maxScore = playerData.MaxScore;
                                int sumScore = playerData.SumScore;
                                int sumLost = playerData.SumLost;
                                int sumKill = playerData.SumKill;
                                int sumWin = playerData.SumWin;
                                int sumBattle = playerData.SumBattle;

                                string item = playerData.SortedItem;                  // Json資料
                                string miceAll = playerData.MiceAll;            // Json資料
                                string team = playerData.Team;                  // Json資料
                                string friend = playerData.Friends;              // string[] split ","資料
                                string image = playerData.PlayerImage;

                                // Log.Debug(" playerData.PlayerImage:" + playerData.PlayerImage + " " + image);
                                if (playerData.ReturnCode == "S401")//取得玩家資料成功 回傳玩家資料
                                {
                                    //  Log.Debug("playerData.ReturnCode == S401");
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)PlayerDataParameterCode.Ret, playerData.ReturnCode }, { (byte)PlayerDataParameterCode.Rank, rank }, { (byte)PlayerDataParameterCode.Exp, exp }, 
                                        { (byte)PlayerDataParameterCode.MaxCombo, maxCombo }, { (byte)PlayerDataParameterCode.MaxScore, maxScore }, { (byte)PlayerDataParameterCode.SumScore, sumScore } ,
                                        { (byte)PlayerDataParameterCode.SumLost, playerData.SumBattle - playerData.SumWin } ,{ (byte)PlayerDataParameterCode.SumKill, sumKill },{ (byte)PlayerDataParameterCode.SumWin, sumWin },
                                        { (byte)PlayerDataParameterCode.SumBattle, sumBattle },{ (byte)PlayerDataParameterCode.SortedItem, item } ,{ (byte)PlayerDataParameterCode.MiceAll, miceAll } ,
                                        { (byte)PlayerDataParameterCode.Team, team },{ (byte)PlayerDataParameterCode.Friend, friend} ,{ (byte)PlayerDataParameterCode.PlayerImage, image} ,
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

                        #region LoadPurchase 載入法幣道具資料
                        case (byte)PurchaseOperationCode.Load:
                            {
                                Log.Debug("IN LoadPurchase");

                                PurchaseData purchaseData = (PurchaseData)TextUtility.DeserializeFromStream(purchaseUI.LoadPurchase());

                                string purchaseItem = purchaseData.jPurchaseData;

                                if (purchaseData.ReturnCode == "S1101")  // 取得法幣道具成功 回傳玩家資料
                                {
                                    Log.Debug("purchaseData.ReturnCode == S1101");
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)PurchaseParameterCode.PurchaseItem, purchaseItem }
                                    };

                                    OperationResponse response = new OperationResponse((byte)PurchaseResponseCode.Loaded, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = purchaseData.ReturnMessage.ToString() };
                                    SendOperationResponse(response, new SendParameters());
                                }
                                else  // 失敗 傳空值+錯誤訊息
                                {
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                    OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = purchaseData.ReturnMessage.ToString() };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }
                            }
                            break;
                        #endregion

                        #region ConfirmPurchase 載入法幣道具資料
                        case (byte)PurchaseOperationCode.Confirm:
                            {
                                Log.Debug("IN ConfirmPurchase");
                                string receiptCipheredPayload = "test"; // Google 購買單號                  測試代碼"test"
                                string receipt = "test";               // Google 自訂交易代號(自我驗證)   測試代碼"test"
                                string account = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Account];
                                string purchaseID = (string)operationRequest.Parameters[(byte)PurchaseParameterCode.PurchaseID];
                                string currencyCode = (string)operationRequest.Parameters[(byte)PurchaseParameterCode.CurrencyCode];
                                string currencyValue = System.Convert.ToString(operationRequest.Parameters[(byte)PurchaseParameterCode.CurrencyValue]);
                                string description = (string)operationRequest.Parameters[(byte)PurchaseParameterCode.Description];

                                // 測試中沒有以下兩個代碼 所以要判斷
                                if (operationRequest.Parameters.ContainsKey((byte)PurchaseParameterCode.ReceiptCipheredPayload))
                                    receiptCipheredPayload = (string)operationRequest.Parameters[(byte)PurchaseParameterCode.ReceiptCipheredPayload];
                                if (operationRequest.Parameters.ContainsKey((byte)PurchaseParameterCode.Receipt))
                                    receipt = (string)operationRequest.Parameters[(byte)PurchaseParameterCode.Receipt];



                                CurrencyData currencyData = (CurrencyData)TextUtility.DeserializeFromStream(purchaseUI.ConfirmPurchase(account, purchaseID, currencyCode, currencyValue, receiptCipheredPayload, receipt, description));



                                if (currencyData.ReturnCode == "S703")  // 取得法幣道具成功 回傳玩家資料
                                {
                                    Log.Debug("purchaseData.ReturnCode == S703");
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                       { (byte)CurrencyParameterCode.Rice, currencyData.Rice },  { (byte)CurrencyParameterCode.Gold, currencyData.Gold },  { (byte)CurrencyParameterCode.Bonus, currencyData.Bonus }
                                    };

                                    OperationResponse response = new OperationResponse((byte)PurchaseResponseCode.Confirmed, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = currencyData.ReturnMessage.ToString() };
                                    SendOperationResponse(response, new SendParameters());
                                }
                                else  // 失敗 傳空值+錯誤訊息
                                {
                                    Log.Debug("Confirm Error :" + currencyData.ReturnCode + "  CurrencyData.ReturnMessage: " + currencyData.ReturnMessage);
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                    OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = currencyData.ReturnMessage.ToString() };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }
                            }
                            break;
                        #endregion

                        #region LoadCurrency 載入貨幣資料
                        case (byte)CurrencyOperationCode.Load:
                            {
                                // Log.Debug("IN LoadCurrency");

                                string account = (string)operationRequest.Parameters[(byte)CurrencyParameterCode.Account];
                                CurrencyData currencyData = (CurrencyData)TextUtility.DeserializeFromStream(currencyUI.LoadCurrency(account));

                                int rice = currencyData.Rice;
                                Int16 gold = currencyData.Gold;
                                //  Int16 bonus = currencyData.Bonus;

                                if (currencyData.ReturnCode == "S701")  // 取得遊戲貨幣成功 回傳玩家資料
                                {
                                    // Log.Debug("currencyData.ReturnCode == S701");
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                        { (byte)CurrencyParameterCode.Ret, currencyData.ReturnCode }, { (byte)CurrencyParameterCode.Rice, rice }, { (byte)CurrencyParameterCode.Gold, gold } /*, { (byte)CurrencyParameterCode.Bonus, bonus } */
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
                                //  Log.Debug("IN LoadRice");

                                string account = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Account];

                                CurrencyData currencyData = (CurrencyData)TextUtility.DeserializeFromStream(currencyUI.LoadCurrency(account)); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找

                                int rice = currencyData.Rice;

                                if (currencyData.ReturnCode == "S703")//取得遊戲幣成功 回傳玩家資料
                                {
                                    // Log.Debug("currencyData.ReturnCode == S703");
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

                                CurrencyData currencyData = (CurrencyData)TextUtility.DeserializeFromStream(currencyUI.LoadCurrency(account));

                                Int16 gold = currencyData.Gold;

                                if (currencyData.ReturnCode == "S705")//取得金幣成功 回傳玩家資料
                                {
                                    //  Log.Debug("currencyData.ReturnCode == S705");
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

                        #region LoadMice 載入老鼠資料
                        case (byte)MiceOperationCode.LoadMice:
                            {
                                try
                                {
                                    //  Log.Debug("IN LoadMice");

                                    MiceDataUI miceDataUI = new MiceDataUI(); //實體化 IO (連結資料庫拿資料)
                                    //Log.Debug("LoadMice IO OK");
                                    MiceData miceData = (MiceData)TextUtility.DeserializeFromStream(miceDataUI.LoadMiceData()); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找
                                    // Log.Debug("LoadMice Data OK");
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
                                    //  Log.Debug("IN LoadSkill");

                                    SkillData skillData = (SkillData)TextUtility.DeserializeFromStream(skillUI.LoadSkillProperty()); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找
                                    //Log.Debug("Server Data: " + miceData.miceProperty);
                                    if (skillData.ReturnCode == "S1001")//取得老鼠資料成功 回傳玩家資料
                                    {
                                        //  Log.Debug("skillData.ReturnCode == S1001");
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
                                    //  Log.Debug("IN LoadStore");

                                    StoreData storeData = (StoreData)TextUtility.DeserializeFromStream(storeDataUI.LoadStoreData()); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找
                                    //Log.Debug("Server Data: " + storeData.StoreItem);
                                    if (storeData.ReturnCode == "S901")//取得老鼠資料成功 回傳玩家資料
                                    {
                                        //    Log.Debug("LoadStore.ReturnCode == S901");
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

                                    ItemData itemData = (ItemData)TextUtility.DeserializeFromStream(itemUI.LoadItemData()); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找
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

                        #region LoadFriendsData 載入好友資料
                        case (byte)MemberOperationCode.LoadFriendsDetail:
                            {
                                List<string> friends = ((string[])operationRequest.Parameters[(byte)MemberParameterCode.Friends]).ToList();
                                PlayerData playerData;

                                if (friends.Count != 0)
                                {
                                    playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.LoadFriendsData(friends.ToArray()));

                                    // 取得好友資料成功
                                    if (playerData.ReturnCode == "S432" && !String.IsNullOrEmpty(playerData.Friends))
                                    {
                                        Log.Debug("取得好友資料成功!");
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)MemberParameterCode.OnlineFriendsDetail, playerData.Friends } };
                                        OperationResponse response = new OperationResponse((byte)MemberResponseCode.LoadFriendsDetail, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = playerData.ReturnMessage.ToString() }; ;
                                        SendOperationResponse(response, new SendParameters());
                                    }
                                    else
                                    {
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                        OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = playerData.ReturnMessage.ToString() };
                                        SendOperationResponse(actorResponse, new SendParameters());
                                    }
                                }
                                else
                                {
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                    OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = "" };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }

                                break;
                            }
                        #endregion

                        #region GetOnlineActorState 載入玩家狀態
                        case (byte)MemberOperationCode.GetOnlineActorState:
                            {
                                Dictionary<string, object> actorState = new Dictionary<string, object>();
                                string[] players = (string[])operationRequest.Parameters[(byte)MemberParameterCode.Friends];

                                if (!string.IsNullOrEmpty(String.Join(",", players)))
                                {
                                    foreach (string player in players)
                                    {   // 如果是暱稱則改為
                                        // Actor actor = _server.Actors.GetActorFromNickname(player);

                                        Actor actor = _server.Actors.GetActorFromAccount(player);
                                        if (actor != null) actorState.Add(player, actor.GameStatus);
                                    }
                                }
                                Log.Debug("取得玩家狀態成功!");
                                Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)MemberParameterCode.OnlineFriendsState, MiniJSON.Json.Serialize(actorState) } };
                                OperationResponse response = new OperationResponse((byte)MemberResponseCode.GetOnlineActorState, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = "取得玩家狀態資料成功!" }; ;
                                SendOperationResponse(response, new SendParameters());

                                break;
                            }
                        #endregion

                        #region InviteFriend 邀請好友
                        case (byte)PlayerDataOperationCode.InviteFriend:
                            {

                                string account = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Account];
                                string friend = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Friend];

                                Actor otherActor = _server.Actors.GetActorFromNickname(friend);
                                if (otherActor == null) otherActor = _server.Actors.GetActorFromAccount(friend);

                                if (otherActor != null)
                                {
                                    Log.Debug("加入好友 取得對方玩家成功!");
                                    MPServerPeer otherPeer = _server.Actors.GetPeerFromGuid(otherActor.guid);
                                    string nickname = _server.Actors.GetNicknameFromGuid(peerGuid);

                                    if (otherActor.Account != account)
                                    {
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)MemberParameterCode.Friends, nickname }, { (byte)MemberParameterCode.Account, account } };
                                        OperationResponse response = new OperationResponse((byte)PlayerDataResponseCode.InviteFriend, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = "已成為你的好友囉!" }; ;
                                        otherPeer.SendOperationResponse(response, new SendParameters());
                                    }
                                    else
                                    {
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                        OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, new Dictionary<byte, object>()) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = "怎麼加自己好友呢，太邊緣囉~" };
                                        SendOperationResponse(actorResponse, new SendParameters());
                                    }
                                }
                                else
                                {
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                    OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, new Dictionary<byte, object>()) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = "玩家不再線上哦~可能輸入了錯誤的名稱~" };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }

                                break;
                            }
                        #endregion

                        #region ApplyInviteFriend 同意好友邀請
                        case (byte)PlayerDataOperationCode.ApplyInviteFriend:
                            {//目前加入好友判斷須雙方成功否則無法加入 會導致問題
                                string account = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Account];
                                string nickname = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Friend];

                                Actor actor, otherActor, friend;
                                PlayerData playerData = new PlayerData();
                                PlayerData otherPlayerData = new PlayerData();
                                Dictionary<string, object> actorState = new Dictionary<string, object>();
                                Dictionary<string, object> otherActorState = new Dictionary<string, object>();

                                actor = _server.Actors.GetActorFromGuid(peerGuid);
                                otherActor = _server.Actors.GetActorFromNickname(nickname);

                                if (actor != null)
                                    playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.UpdatePlayerData(actor.Account, otherActor.Account));
                                if (otherActor != null)
                                    otherPlayerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.UpdatePlayerData(otherActor.Account, account));

                                Log.Debug(playerData.ReturnCode + " " + otherPlayerData.ReturnCode);

                                if (playerData.ReturnCode == "S434" && otherPlayerData.ReturnCode == "S434")    // 加入好有成功
                                {
                                    Log.Debug(playerData.ReturnCode + " " + playerData.ReturnMessage);
                                    playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.LoadPlayerData(account));
                                    otherPlayerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.LoadPlayerData(otherActor.Account));
                                    Log.Debug(playerData.ReturnCode + " " + playerData.ReturnMessage + "   " + playerData.Friends);
                                    Log.Debug(otherPlayerData.ReturnCode + " " + otherPlayerData.ReturnMessage + "  " + otherPlayerData.Friends + "  otherActorAccount:" + otherActor.Account);

                                    if (playerData.ReturnCode == "S401" && otherPlayerData.ReturnCode == "S401")// 取得好有成功
                                    {
                                        //Log.Debug(playerData.ReturnCode + " " + playerData.ReturnMessage);

                                        foreach (string player in playerData.Friends.Split(',').ToList())
                                        {
                                            friend = _server.Actors.GetActorFromAccount(player);
                                            if (friend != null) actorState.Add(player, friend.GameStatus);
                                        }

                                        foreach (string player in otherPlayerData.Friends.Split(',').ToList())
                                        {
                                            friend = _server.Actors.GetActorFromAccount(player);
                                            if (friend != null) otherActorState.Add(player, friend.GameStatus);
                                        }

                                        // 取得詳細資料後 互傳給對方自己的資料
                                        PlayerData invitedFriendData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.LoadFriendsData(new string[] { account }));
                                        PlayerData otherInvitedFriendData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.LoadFriendsData(new string[] { otherActor.Account }));

                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)MemberParameterCode.Friends, playerData.Friends }, { (byte)MemberParameterCode.OnlineFriendsDetail, otherInvitedFriendData.Friends }, { (byte)MemberParameterCode.OnlineFriendsState, MiniJSON.Json.Serialize(actorState) } };
                                        OperationResponse response = new OperationResponse((byte)PlayerDataResponseCode.ApplyInviteFriend, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = "增加好友成功!" }; ;
                                        SendOperationResponse(response, new SendParameters());

                                        parameter = new Dictionary<byte, object> { { (byte)MemberParameterCode.Friends, otherPlayerData.Friends }, { (byte)MemberParameterCode.OnlineFriendsDetail, invitedFriendData.Friends }, { (byte)MemberParameterCode.OnlineFriendsState, MiniJSON.Json.Serialize(otherActorState) } };
                                        response = new OperationResponse((byte)PlayerDataResponseCode.ApplyInviteFriend, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = "增加好友成功!" }; ;
                                        _server.Actors.GetPeerFromGuid(otherActor.guid).SendOperationResponse(response, new SendParameters());
                                    }
                                    else
                                    {
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)MemberParameterCode.Ret, playerData.ReturnCode } };
                                        OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, new Dictionary<byte, object>()) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = playerData.ReturnMessage };
                                        SendOperationResponse(actorResponse, new SendParameters());
                                    }
                                }
                                else
                                {
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)MemberParameterCode.Ret, playerData.ReturnCode } };
                                    OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = playerData.ReturnMessage };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }
                                break;
                            }


                        #endregion

                        #region RemoveFriend 移除好友 還沒寫好
                        case (byte)PlayerDataOperationCode.RemoveFriend:
                            {
                                string account = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Account];
                                string friendAccount = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Friend];
                                PlayerData otherPlayerData, playerData;
                                Dictionary<string, object> actorState = new Dictionary<string, object>();
                                Dictionary<string, object> otherActorState = new Dictionary<string, object>();
                                Actor actor, otherActor, friendActor;

                                Log.Debug("account: " + account);


                                playerData = otherPlayerData = new PlayerData();
                                actor = _server.Actors.GetActorFromAccount(account);
                                Log.Debug("actor: " + actor.Account);
                                otherActor = _server.Actors.GetActorFromAccount(friendAccount);
                                playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.LoadPlayerData(account, new string[] { "Friend" }));

                                if (playerData.ReturnCode == "S436")    //取得好有資料成功
                                {
                                    if (actor != null)
                                        playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.RemoveFriend(account, friendAccount));

                                    #region 移除對方版本 關閉則只移除自己
                                    // 移除對方版本
                                    if (otherActor != null)    // 如果對放在線上 直接移除
                                    {
                                        Log.Debug("otherActor.guid: " + otherActor.guid + "  otherActor.Account: " + otherActor.Account);
                                        MPServerPeer otherPeer = _server.Actors.GetPeerFromGuid(otherActor.guid);
                                        otherPlayerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.RemoveFriend(otherActor.Account, actor.Account));

                                        if (otherPlayerData.ReturnCode == "S440")
                                        {
                                            Log.Debug("otherPlayerData.ReturnCode == S440");
                                            otherPlayerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.LoadPlayerData(otherActor.Account, new string[] { "Friend" }));
                                            if (otherPlayerData.ReturnCode == "S436")
                                            {
                                                Log.Debug("otherPlayerData.ReturnCode == S436   " + otherPlayerData.ReturnMessage + "  Friends: " + otherPlayerData.Friends);
                                                string[] friends = otherPlayerData.Friends.Split(',').ToArray();
                                                if (!string.IsNullOrEmpty(otherPlayerData.Friends))
                                                {
                                                    foreach (string player in friends)
                                                    {
                                                        friendActor = _server.Actors.GetActorFromAccount(player);
                                                        if (friendActor != null && player != friendAccount) otherActorState.Add(player, friendActor.GameStatus);
                                                    }
                                                }
                                                else
                                                {
                                                    Log.Debug("FUCK");
                                                    otherPlayerData.Friends = "";
                                                }
                                                // 取得詳細資料後 互傳給對方自己的資料
                                                PlayerData friendDetail = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.LoadFriendsData(friends));


                                                Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)MemberParameterCode.Friends, otherPlayerData.Friends }, { (byte)MemberParameterCode.OnlineFriendsDetail, friendDetail.Friends }, { (byte)MemberParameterCode.OnlineFriendsState, MiniJSON.Json.Serialize(otherActorState) } };
                                                OperationResponse response = new OperationResponse((byte)PlayerDataResponseCode.RemoveFriend, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = otherPlayerData.ReturnMessage }; ;
                                                otherPeer.SendOperationResponse(response, new SendParameters());
                                            }
                                        }
                                        else
                                        {
                                            Log.Debug("ReturnCode:" + playerData.ReturnCode + playerData.ReturnMessage);
                                            Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                            OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, new Dictionary<byte, object>()) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = "取得好友失敗!" };
                                            otherPeer.SendOperationResponse(actorResponse, new SendParameters());
                                        }
                                    }
                                    else//如果對方不在線上 找資料並移除
                                    {
                                        otherPlayerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.LoadPlayerData(friendAccount, new string[] { "Friend" }));
                                        List<string> friends = otherPlayerData.Friends.Split(',').ToList();
                                        friends.Remove(account);
                                        playerDataUI.UpdatePlayerData(friendAccount, string.Join(",", friends.ToArray()));
                                    }
                                    #endregion

                                    if (playerData.ReturnCode == "S440")
                                    {
                                        Log.Debug("移除好友成功!  " + playerData.Friends);
                                        playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.LoadPlayerData(account, new string[] { "Friend" }));

                                        if (playerData.ReturnCode == "S436")    //取得好友資料成功
                                        {
                                            string[] friends = playerData.Friends.Split(',').ToArray();

                                            if (!string.IsNullOrEmpty(playerData.Friends))
                                            {
                                                foreach (string player in friends)
                                                {
                                                    friendActor = _server.Actors.GetActorFromAccount(player);
                                                    if (friendActor != null) actorState.Add(player, friendActor.GameStatus);
                                                }
                                            }
                                            else
                                            {
                                                playerData.Friends = "";
                                            }

                                            // 取得詳細資料後 互傳給對方自己的資料
                                            PlayerData friendDetail = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.LoadFriendsData(friends));

                                            Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)MemberParameterCode.Friends, playerData.Friends }, { (byte)MemberParameterCode.OnlineFriendsDetail, friendDetail.Friends }, { (byte)MemberParameterCode.OnlineFriendsState, MiniJSON.Json.Serialize(actorState) } };
                                            OperationResponse response = new OperationResponse((byte)PlayerDataResponseCode.RemoveFriend, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = playerData.ReturnMessage }; ;
                                            SendOperationResponse(response, new SendParameters());
                                        }
                                        else
                                        {
                                            Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)MemberParameterCode.Friends, "" }, { (byte)MemberParameterCode.OnlineFriendsDetail, "" }, { (byte)MemberParameterCode.OnlineFriendsState, "" } };
                                            OperationResponse response = new OperationResponse((byte)PlayerDataResponseCode.RemoveFriend, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = playerData.ReturnMessage }; ;
                                            SendOperationResponse(response, new SendParameters());
                                        }

                                    }
                                    else
                                    {
                                        Log.Debug("FUCK" + playerData.ReturnCode + playerData.ReturnMessage);
                                    }
                                }
                                else
                                {
                                    Log.Debug("ReturnCode:" + playerData.ReturnCode + playerData.ReturnMessage);
                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                    OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, new Dictionary<byte, object>()) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = "移除好友失敗!" };
                                    SendOperationResponse(actorResponse, new SendParameters());
                                }

                                break;
                            }
                        #endregion


                        #region Mission 發送任務
                        case (byte)BattleOperationCode.Mission:
                            {
                                try
                                {
                                    Log.Debug("IN Mission");

                                    int primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];
                                    int roomID = _server.room.GetPlayingRoomFromGuid(peerGuid);
                                    byte mission = (byte)operationRequest.Parameters[(byte)BattleParameterCode.Mission];
                                    float missionRate = (float)operationRequest.Parameters[(byte)BattleParameterCode.MissionRate];


                                    BattleData battleData = (BattleData)TextUtility.DeserializeFromStream(battleUI.SelectMission(mission, missionRate));
                                    Int16 missionMethod = battleData.missionScore;
                                    Log.Debug("missionMethod:" + missionMethod);

                                    if (mission == (byte)Mission.WorldBoss)
                                    {
                                        // missionMethod = 10004;   // 白癡版本

                                        // 固定值 隨機版本
                                        Random rnd = new Random();
                                        //missionMethod = Convert.ToInt16(rnd.Next(10001, 10005 + 1));
                                        missionMethod = Convert.ToInt16(10001);
                                        Log.Debug("----------DevTest-------------");
                                        Room.RoomActor roomActor = _server.room.GetActorFromGuid(peerGuid);

                                        if (roomActor.roomMice.Count != 0)
                                        {
                                            missionMethod = Convert.ToInt16(roomActor.roomMice[rnd.Next(0, roomActor.roomMice.Count)]);
                                            Log.Debug("Room Mice Count: " + roomActor.roomMice.Count + "  missionMethod: " + missionMethod);
                                        }
                                        Log.Debug("----------DevTest-------------");
                                        _server.room.SpawnBoss(roomID, battleData.bossHP);  // 產生BOSS血量 missionScore是BOSS HP
                                    }

                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)BattleParameterCode.Mission, mission }, { (byte)BattleParameterCode.MissionScore, missionMethod } };

                                    OperationResponse response = new OperationResponse((byte)BattleResponseCode.Mission, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = "Recive Mission" };

                                    Room.RoomActor otherActor;
                                    otherActor = _server.room.GetPlayingRoomOtherPlayer(roomID, primaryID);

                                    if (otherActor != null)
                                    {
                                        MPServerPeer peerOther = _server.Actors.GetPeerFromGuid(otherActor.guid);
                                        EventData missionEventData = new EventData((byte)BattleResponseCode.Mission, parameter);
                                        peerOther.SendEvent(missionEventData, new SendParameters());    // 回傳給另外一位玩家
                                    }
                                    else
                                    {
                                        // to do
                                        Log.Debug("快點寫程式好嗎!!!!!!!!!!");
                                    }

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

                                    int primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];
                                    int roomID = _server.room.GetPlayingRoomFromGuid(peerGuid);
                                    byte mission = (byte)operationRequest.Parameters[(byte)BattleParameterCode.Mission];
                                    float missionRate = (float)operationRequest.Parameters[(byte)BattleParameterCode.MissionRate];
                                    Int16 customValue = (Int16)operationRequest.Parameters[(byte)BattleParameterCode.CustomValue];

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

                                            //  int damage;
                                            Dictionary<byte, object> parameter;
                                            MPServerPeer peerOther = null;
                                            EventData eventData;
                                            Room.RoomActor roomActor, otherActor;

                                            roomActor = _server.room.GetActorFromGuid(peerGuid);
                                            otherActor = _server.room.GetPlayingRoomOtherPlayer(roomID, primaryID);
                                            if (otherActor != null) peerOther = _server.Actors.GetPeerFromGuid(otherActor.guid);

                                            //if (_server.room.GetBossHP(roomID) > 0)
                                            //{
                                            //    damage = _server.room.GetBossHP(roomID);

                                            //    Log.Debug("任務時間超過 還有殘血 發送傷害  -BossHP:" + damage);
                                            //    parameter = new Dictionary<byte, object>() {{ (byte)BattleParameterCode.PrimaryID, primaryID }, { (byte)BattleParameterCode.Damage, damage }, { (byte)BattleResponseCode.DebugMessage, battleData.ReturnMessage.ToString() } };
                                            //    eventData = new EventData((byte)BattleResponseCode.BossDamage, parameter);
                                            //    SendEvent(eventData, new SendParameters());

                                            //    // 傳送殘血傷害給雙方
                                            //    parameter = new Dictionary<byte, object>() { { (byte)BattleParameterCode.PrimaryID, otherActor.PrimaryID }, { (byte)BattleParameterCode.Damage, damage }, { (byte)BattleResponseCode.DebugMessage, battleData.ReturnMessage.ToString() } };
                                            //    eventData = new EventData((byte)BattleResponseCode.BossDamage, parameter);
                                            //    peerOther.SendEvent(eventData, new SendParameters());

                                            //}

                                            _server.room.KillBoss(roomID);  // 世界王任務完成 把Server BOSS殺了          


                                            // 儲存分數
                                            if (roomActor != null) roomActor.gameScore = Math.Max(0, roomActor.gameScore + battleData.missionReward);

                                            // 回傳給原玩家 任務獎勵 參數
                                            parameter = new Dictionary<byte, object> {{ (byte)BattleParameterCode.Ret, battleData.ReturnCode },
                                                         { (byte)BattleParameterCode.MissionReward, missionReward }  };
                                            OperationResponse response = new OperationResponse((byte)BattleResponseCode.MissionCompleted, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = battleData.ReturnMessage.ToString() };

                                            // 回傳給原玩家 對方任務獎勵 事件參數
                                            parameter = new Dictionary<byte, object>() { { (byte)BattleParameterCode.MissionReward, otherReward }, { (byte)BattleResponseCode.DebugMessage, battleData.ReturnMessage.ToString() } };
                                            eventData = new EventData((byte)BattleResponseCode.GetMissionScore, parameter);

                                            SendOperationResponse(response, new SendParameters());// 我方 取得的分數
                                            SendEvent(eventData, new SendParameters());   //  我方 接收對方 取得的分數


                                            // 回傳給另外一位玩家

                                            if (otherActor != null) otherActor.gameScore = Math.Max(0, otherActor.gameScore + otherReward);

                                            // 回傳給對方玩家 任務獎勵 參數
                                            parameter = new Dictionary<byte, object> {
                                                        { (byte)BattleParameterCode.Ret, battleData.ReturnCode }, { (byte)BattleParameterCode.MissionReward, otherReward }  };
                                            response = new OperationResponse((byte)BattleResponseCode.MissionCompleted, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = battleData.ReturnMessage.ToString() };

                                            // 回傳給對方玩家 任務獎勵 事件參數
                                            parameter = new Dictionary<byte, object>() { { (byte)BattleParameterCode.MissionReward, missionReward }, { (byte)BattleResponseCode.DebugMessage, battleData.ReturnMessage.ToString() } };
                                            eventData = new EventData((byte)BattleResponseCode.GetMissionScore, parameter);

                                            if (peerOther != null)
                                            {
                                                peerOther.SendOperationResponse(response, new SendParameters());  // 對方 接收分數
                                                peerOther.SendEvent(eventData, new SendParameters());    // 對方 接收我方 取得的分數
                                            }

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
                                            Room.RoomActor otherActor;
                                            otherActor = _server.room.GetPlayingRoomOtherPlayer(roomID, primaryID);
                                            if (otherActor != null)
                                            {
                                                MPServerPeer peerOther = _server.Actors.GetPeerFromGuid(otherActor.guid);
                                                Dictionary<byte, object> parameter2 = new Dictionary<byte, object>() { { (byte)BattleParameterCode.MissionReward, missionReward }, { (byte)BattleResponseCode.DebugMessage, battleData.ReturnMessage.ToString() } };
                                                EventData getMissionScoreEventData = new EventData((byte)BattleResponseCode.GetMissionScore, parameter2);
                                                peerOther.SendEvent(getMissionScoreEventData, new SendParameters());
                                            }
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

                                    int primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];
                                    int roomID = _server.room.GetPlayingRoomFromGuid(peerGuid);
                                    Int16 damage = (Int16)operationRequest.Parameters[(byte)BattleParameterCode.Damage];

                                    int bossHP = _server.room.UpdateBossHP(roomID, damage);
                                    Log.Debug("BOSS HP:" + bossHP);

                                    Dictionary<byte, object> bossDamageParameter = new Dictionary<byte, object> { { (byte)BattleParameterCode.Damage, damage }, { (byte)BattleParameterCode.PrimaryID, primaryID } };

                                    if (bossHP > 0)
                                    {
                                        //回傳給原玩家
                                        Log.Debug("bossHP > 0");


                                        OperationResponse response = new OperationResponse((byte)BattleResponseCode.BossDamage, bossDamageParameter) { ReturnCode = (short)ErrorCode.Ok };
                                        SendOperationResponse(response, new SendParameters());

                                        //回傳給另外一位玩家
                                        Room.RoomActor otherActor;
                                        otherActor = _server.room.GetPlayingRoomOtherPlayer(roomID, primaryID);
                                        if (otherActor != null)
                                        {
                                            MPServerPeer peerOther = _server.Actors.GetPeerFromGuid(otherActor.guid);
                                            EventData bossEventData = new EventData((byte)BattleResponseCode.BossDamage, bossDamageParameter);             // BOSS傷害, parameter2);
                                            peerOther.SendEvent(bossEventData, new SendParameters());
                                        }
                                    }
                                    else if (bossHP == 0)
                                    {
                                        _server.room.KillBoss(roomID);
                                        Log.Debug("bossHP == 0");

                                        OperationResponse response = new OperationResponse((byte)BattleResponseCode.BossDamage, bossDamageParameter) { ReturnCode = (short)ErrorCode.Ok };
                                        SendOperationResponse(response, new SendParameters());

                                        //回傳給另外一位玩家
                                        Room.RoomActor otherActor;
                                        otherActor = _server.room.GetPlayingRoomOtherPlayer(roomID, primaryID);
                                        if (otherActor != null)
                                        {
                                            MPServerPeer peerOther = _server.Actors.GetPeerFromGuid(otherActor.guid);
                                            EventData bossEventData = new EventData((byte)BattleResponseCode.BossDamage, bossDamageParameter);

                                            peerOther.SendEvent(bossEventData, new SendParameters());
                                        }
                                        else
                                        {
                                            _server.room.RemovePlayingRoom(roomID, peerGuid, primaryID);
                                            Log.Debug("配對的玩家已經離開房間！");
                                            Dictionary<byte, object> parameter = new Dictionary<byte, object>() { { (byte)BattleResponseCode.DebugMessage, "配對的玩家已經離開房間！" } };
                                            EventData eventData = new EventData((byte)BattleResponseCode.Offline, parameter);
                                            SendEvent(eventData, new SendParameters());
                                        }
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

                                    int primaryID = (int)operationRequest.Parameters[(byte)BattleParameterCode.PrimaryID];
                                    int roomID = _server.room.GetPlayingRoomFromGuid(peerGuid);
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
                                        Room.RoomActor otherActor;
                                        otherActor = _server.room.GetPlayingRoomOtherPlayer(roomID, primaryID);
                                        if (otherActor != null)
                                        {
                                            MPServerPeer peerOther = _server.Actors.GetPeerFromGuid(otherActor.guid);
                                            Dictionary<byte, object> parameter2 = new Dictionary<byte, object>() { { (byte)BattleParameterCode.Damage, damage } };
                                            EventData bossEventData = new EventData((byte)BattleResponseCode.Damage, parameter2);             // BOSS傷害, parameter2);
                                            peerOther.SendEvent(bossEventData, new SendParameters());
                                        }
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
                                    int primaryID = (int)operationRequest.Parameters[(byte)LoginParameterCode.PrimaryID];
                                    Int16 score = (Int16)operationRequest.Parameters[(byte)BattleParameterCode.Score];
                                    Int16 otherScore = (Int16)operationRequest.Parameters[(byte)BattleParameterCode.OtherScore];
                                    Int16 gameTime = (Int16)operationRequest.Parameters[(byte)BattleParameterCode.Time];
                                    Int16 maxCombo = (Int16)operationRequest.Parameters[(byte)PlayerDataParameterCode.MaxCombo];
                                    int lostMice = (int)operationRequest.Parameters[(byte)PlayerDataParameterCode.SumLost];
                                    Int16 spawnCount = (Int16)operationRequest.Parameters[(byte)BattleParameterCode.SpawnCount];
                                    int killMice = (int)operationRequest.Parameters[(byte)PlayerDataParameterCode.SumKill];
                                    string item = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.SortedItem];
                                    string jMicesUseCount = (string)operationRequest.Parameters[(byte)BattleParameterCode.CustomValue];
                                    object tmp;
                                    int roomID = _server.room.GetPlayingRoomFromGuid(peerGuid);
                                    string jItemsUseCount = "";
                                    if (operationRequest.Parameters.TryGetValue((byte)BattleParameterCode.CustomString, out tmp))
                                        jItemsUseCount = (string)operationRequest.Parameters[(byte)BattleParameterCode.CustomString];
                                    // 評審後改回
                                    //string jItemsUseCount = (string)operationRequest.Parameters[(byte)BattleParameterCode.CustomString];
                                    string[] columns = ((string[])operationRequest.Parameters[(byte)PlayerDataParameterCode.Columns]);

                                    Log.Debug("IN GameOver : " + account);

                                    Room.RoomActor otherRoomActor = _server.room.GetPlayingRoomOtherPlayer(roomID, primaryID);
                                    Room.RoomActor roomActor = _server.room.GetActorFromGuid(peerGuid); // 這被移除

                                    char[] trim = new char[] { '"' };
                                    jMicesUseCount = jMicesUseCount.TrimStart(trim);
                                    jMicesUseCount = jMicesUseCount.TrimEnd(trim);

                                    jItemsUseCount = jItemsUseCount.TrimStart(trim);
                                    jItemsUseCount = jItemsUseCount.TrimEnd(trim);

                                    Log.Debug("GameOver Data  : " + account + " " + score + " " + otherScore + " " + gameTime + " " + maxCombo + " " + lostMice + " " + spawnCount + " " + killMice + " " + item + " " + jMicesUseCount + " " + jItemsUseCount + " " + columns.Length + " ");

                                    BattleData battleData = (BattleData)TextUtility.DeserializeFromStream(battleUI.GameOver(account, score, otherScore, gameTime, lostMice, roomActor.totalScore, spawnCount, roomActor.missionCompletedCount, roomActor.maxMissionCount, maxCombo, jMicesUseCount, jItemsUseCount, columns)); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找
                                    if (string.IsNullOrEmpty(battleData.ReturnMessage)) Log.Debug("GameOver Data is null ! ");

                                    if (battleData.ReturnCode == "S514")
                                    {
                                        Log.Debug(roomActor.Nickname + "Player Score : " + roomActor.gameScore + "  " + otherRoomActor.Nickname + "   Other Score : " + otherRoomActor.gameScore);

                                        battleData.battleResult = (roomActor.gameScore > otherRoomActor.gameScore && roomActor.life > 0) ? (byte)1 : (byte)0;

                                        Log.Debug("GameOver Peer: " + roomActor.gameScore + " " + battleData.expReward + " " + maxCombo + " " + score + " " + lostMice + " " + killMice + " " + battleData.battleResult + " " + item);
                                        PlayerDataUI playerDataUI = new PlayerDataUI(); //實體化 IO (連結資料庫拿資料)
                                        PlayerData playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.UpdateGameOver(account, roomActor.gameScore, battleData.expReward, maxCombo, score, lostMice, killMice, battleData.battleResult, item));// 更新會員資料
                                        playerDataUI.UpdatePlayerItem(account, battleData.jMiceResult, columns);   // 更新玩家道具資料 battleData.jMiceResult= 已更新的老鼠(用量、經驗)字串
                                        if (playerData.ReturnCode == "S403")
                                        {
                                            playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.LoadPlayerData(account)); // 載入玩家資料 playerData的資料 = 資料庫拿的資料 用account去找
                                            // Log.Debug("battleData.expReward: =" + battleData.expReward + " battleData.sliverReward=  " + battleData.goldReward);
                                        }
                                        else
                                        {
                                            Log.Debug("Update PlayerData Error!" + "  Code:" + playerData.ReturnCode + "  Message:" + playerData.ReturnMessage);
                                        }

                                        if (battleData.sliverReward != 0)
                                            currencyUI.UpdateCurrency(account, (byte)CurrencyType.Rice, battleData.sliverReward);

                                        if (battleData.goldReward != 0)
                                            currencyUI.UpdateCurrency(account, (byte)CurrencyType.Gold, battleData.goldReward);

                                        if (!string.IsNullOrEmpty(battleData.jItemReward) || battleData.jItemReward != "{}")
                                            playerDataUI.UpdatePlayerItem(account, battleData.jItemReward, new string[] { PlayerItem.ItemCount.ToString() });

                                        if (playerData.ReturnCode == "S401")
                                        {

                                            Actor actor = _server.Actors.GetActorFromGuid(peerGuid);
                                            Actor otherActor = _server.Actors.GetActorFromGuid(otherRoomActor.guid);
                                            actor.GameStatus = (byte)ENUM_MemberState.Idle;


                                            if (otherActor != null)
                                            {
                                                _server.room.RemovePlayingRoom(roomID, peerGuid, -1);
                                                Log.Debug("Gameover OK");
                                            }
                                            else
                                            {
                                                Log.Debug(" Gameover FUCK");
                                                _server.room.RemovePlayingRoom(roomID, peerGuid, primaryID);
                                            }

                                            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                                     { (byte)BattleParameterCode.Ret, battleData.ReturnCode },{ (byte)BattleParameterCode.Score, roomActor.gameScore },{ (byte)BattleParameterCode.SliverReward, battleData.sliverReward },{ (byte)BattleParameterCode.GoldReward, battleData.goldReward },
                                                     { (byte)BattleParameterCode.ItemReward, battleData.jItemReward },{ (byte)BattleParameterCode.EXPReward, battleData.expReward },{ (byte)BattleParameterCode.Evaluate, battleData.evaluate },{ (byte)BattleParameterCode.BattleResult, battleData.battleResult },
                                                     { (byte)PlayerDataParameterCode.MaxScore, playerData.MaxScore } ,{ (byte)PlayerDataParameterCode.SumLost, playerData.SumBattle - playerData.SumWin },{ (byte)PlayerDataParameterCode.SumKill, playerData.SumKill },{ (byte)PlayerDataParameterCode.SortedItem, playerData.SortedItem },
                                                     { (byte)PlayerDataParameterCode.MaxCombo, playerData.MaxCombo },{ (byte)PlayerDataParameterCode.Rank, playerData.Rank }};

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
                                    // Log.Debug("IN BuyItem");
                                    string account = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Account];
                                    string miceAll = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.MiceAll];
                                    int itemID = (int)operationRequest.Parameters[(byte)StoreParameterCode.ItemID];
                                    string itemName = (string)operationRequest.Parameters[(byte)StoreParameterCode.ItemName];
                                    byte itemType = (byte)operationRequest.Parameters[(byte)StoreParameterCode.ItemType];
                                    byte currencyType = (byte)operationRequest.Parameters[(byte)StoreParameterCode.CurrencyType];
                                    int buyCount = (int)operationRequest.Parameters[(byte)StoreParameterCode.BuyCount];

                                    Log.Debug(string.Format("itemName={0}   itemType={1}   currencyType={2}   buyCount={3}", itemID, itemType, currencyType, buyCount));
                                    StoreDataUI storeDataUI = new StoreDataUI(); //實體化 IO (連結資料庫拿資料)
                                    StoreData storeData = (StoreData)TextUtility.DeserializeFromStream(storeDataUI.LoadStoreData(itemID, itemType)); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找

                                    if (storeData.ReturnCode == "S901") // 更新玩家貨幣
                                    {
                                        CurrencyData currencyData = (CurrencyData)TextUtility.DeserializeFromStream(currencyUI.UpdateCurrency(account, currencyType, -(storeData.Price * buyCount)));

                                        //Log.Debug("花費金錢:" + -(storeData.Price * buyCount));
                                        //Log.Debug("BuyItem currencyData OK :" + currencyData.ReturnCode + "Rice:" + currencyData.Rice);

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
                                                    // Log.Debug("BuyItem playerData OK");

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
                                        else if (currencyData.ReturnCode == "S711" || currencyData.ReturnCode == "S712")
                                        {
                                            Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
                                            OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = storeData.ReturnMessage.ToString() };
                                            SendOperationResponse(actorResponse, new SendParameters());
                                        }
                                        else
                                        {
                                            Log.Error("Buy Item unknow error!");
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

                        #region BuyGashapon 購買轉蛋商品
                        case (byte)StoreOperationCode.BuyGashapon:
                            {
                                try
                                {
                                    Log.Debug("--------IN BuyGashapon-------");
                                    string account = (string)operationRequest.Parameters[(byte)PlayerDataParameterCode.Account];
                                    int itemID = (int)operationRequest.Parameters[(byte)StoreParameterCode.ItemID];
                                    byte itemType = (byte)operationRequest.Parameters[(byte)StoreParameterCode.ItemType];
                                    byte series = (byte)operationRequest.Parameters[(byte)GashaponParameterCode.Series];

                                    Log.Debug(string.Format("itemID={0}   itemType={1}  Series={2}", itemID, itemType,series));

                                    StoreData storeData;
                                    CurrencyData currencyData;
                                    PlayerData playerData;
                                    GashaponData[] gashaponData;




                                    // 更新商店購買數量
                                    storeData = (StoreData)TextUtility.DeserializeFromStream(storeDataUI.BuyGashapon(itemID)); //g
            
                                    if (storeData.ReturnCode == "S903") // 購買轉蛋商品 商店資料更新成功 ****"－price"****　負的　減少
                                    {
                                        // chk currency
                                        currencyData = (CurrencyData)TextUtility.DeserializeFromStream(currencyUI.UpdateCurrency(account, storeData.CurrencyType, -storeData.Price));

                                        // 更新玩家貨幣 成功
                                        if (currencyData.ReturnCode == "S703") 
                                        {
                                            // grenate gashapon
                                            gashaponData = (GashaponData[])TextUtility.DeserializeFromStream(gashaponUI.BuyGashapon(itemID, itemType,series, storeData.Price)); //memberData的資料 = 資料庫拿的資料 用account, passowrd 去找

                                            // 購買轉蛋 成功
                                            if (gashaponData[0].ReturnCode == "S1205") 
                                            {
                                                List<string> itemIist = new List<string>();
                                                
                                               itemIist= gashaponData.Select(x => x.ItemID.ToString()).ToList();

                                               string[] itemArray = itemIist.ToArray();

                                                currencyData = (CurrencyData)TextUtility.DeserializeFromStream(currencyUI.LoadCurrency(account));
                                                storeData = (StoreData)TextUtility.DeserializeFromStream(storeDataUI.LoadStoreData());
                                                playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.InsertPlayerItem(account, itemArray));

                                                if (playerData.ReturnCode == "S442") // 新增道具成功
                                                {
                                                    playerData = (PlayerData)TextUtility.DeserializeFromStream(playerDataUI.LoadPlayerItem(account));

                                                    byte[] itemSerialize = TextUtility.SerializeToStream(itemIist);

                                                    Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                                                                     { (byte)GashaponParameterCode.Ret, playerData.ReturnCode }, { (byte)PlayerDataParameterCode.PlayerItem, playerData.PlayerItem } , { (byte)PlayerDataParameterCode.SortedItem, itemSerialize } ,
                                                                     { (byte)CurrencyParameterCode.Gold, currencyData.Gold } ,{ (byte)CurrencyParameterCode.Rice, currencyData.Rice } ,{ (byte)CurrencyParameterCode.Bonus, currencyData.Bonus } 
                                                                       ,{ (byte)StoreParameterCode.StoreData, storeData.StoreItem }  };

                                                    OperationResponse response = new OperationResponse((byte)GashaponResponseCode.BuyGashapon, parameter) { ReturnCode = (short)ErrorCode.Ok, DebugMessage = storeData.ReturnMessage.ToString() };
                                                    SendOperationResponse(response, new SendParameters());

                                                }
                                                else    // 失敗
                                                {
                                                    Log.Debug("新增玩家道具失敗: " + playerData.ReturnCode+"  " + playerData.ReturnMessage);
                                                     Dictionary<byte, object> parameter = new Dictionary<byte, object> {{(byte)GashaponParameterCode.Ret,playerData.ReturnCode  } };
                                                    OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = storeData.ReturnMessage.ToString() };
                                                    SendOperationResponse(actorResponse, new SendParameters());
                                                }


                                                // return currencyData storeData playerItem gashaponItemID
                                            }
                                            else    // 失敗
                                            {
                                                Log.Debug("購買轉蛋道具失敗: " + gashaponData[0].ReturnCode + "  " + gashaponData[0].ReturnMessage);
                                                 Dictionary<byte, object> parameter = new Dictionary<byte, object> {{(byte)GashaponParameterCode.Ret,  gashaponData[0].ReturnCode} };
                                                OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = storeData.ReturnMessage.ToString() };
                                                SendOperationResponse(actorResponse, new SendParameters());
                                            }
                                        }
                                        else    // 失敗
                                        {
                                            Log.Debug("更新貨幣失敗: " + currencyData.ReturnCode + "  " + currencyData.ReturnMessage);
                                            Dictionary<byte, object> parameter = new Dictionary<byte, object> {{(byte)GashaponParameterCode.Ret, currencyData.ReturnCode } };
                                            OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = storeData.ReturnMessage.ToString() };
                                            SendOperationResponse(actorResponse, new SendParameters());
                                        }
                                    }
                                    else    // 失敗
                                    {
                                        Log.Debug("更新商店購買量失敗: " + storeData.ReturnCode + "  " + storeData.ReturnMessage);
                                        Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)GashaponParameterCode.Ret, storeData.ReturnCode } };
                                        OperationResponse actorResponse = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.InvalidParameter, DebugMessage = storeData.ReturnMessage.ToString() };
                                        SendOperationResponse(actorResponse, new SendParameters());
                                    }
                                }
                                catch (Exception e)
                                {
                                    Log.Debug("例外情況: " + e.Message + "於： " + e.StackTrace);
                                }
                                break;
                            }
                        #endregion

                        #region RoomMice 取得雙方房間老鼠資料
                        case (byte)BattleOperationCode.RoomMice:
                            {
                                try
                                {
                                    int roomID = _server.room.GetPlayingRoomFromGuid(peerGuid);
                                    string[] roomMice = (string[])operationRequest.Parameters[(byte)BattleParameterCode.MiceID];

                                    Room.RoomActor roomActor = _server.room.GetActorFromGuid(peerGuid);
                                    if (roomActor != null) roomActor.roomMice = roomMice.ToList();

                                    Log.Debug("IN RoomMice: " + roomActor.roomMice.Count);
                                }
                                catch
                                {
                                    throw;
                                }
                                break;
                            }
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
