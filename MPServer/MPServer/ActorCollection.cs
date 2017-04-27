using ExitGames.Logging;
using System;
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
 * 
 * 全部 在線上的會員資料 ，提供 取得各會員之間的關聯與資料
 * 線上會員、移除線上會員、踢除線上會員
 * 
 * ***************************************************************/

namespace MPServer
{
    public class ActorCollection
    {
        protected Dictionary<Guid, MPServerPeer> ConnectedClients { get; set; }     // 連接的Clinet列表 
        protected Dictionary<int, Actor> PrimaryGetActor { get; set; }              // 會員資料列表 Key:PrimaryID Vaule:Actor
        protected Dictionary<int, string> PrimaryGetNickname { get; set; }          // 會員暱稱列表 Key:PrimaryID Vaule:Nickname
        protected Dictionary<int, string> PrimaryGetAccount { get; set; }           // 會員帳號列表 Key:PrimaryID Vaule:Account
        protected Dictionary<int, Guid> PrimaryGetGuid { get; set; }                // 會員暱稱列表 Key:PrimaryID Vaule:Nickname

        protected Dictionary<Guid, int> GuidGetPrimary { get; set; }                // 索引列表 用機器識別 找主索引  Key:guid Vaule:PrimaryID
        protected Dictionary<string, int> AccountGetPrimary { get; set; }           // 索引列表 用帳號 找主索引  Key:Account Vaule:PrimaryID
        protected Dictionary<string, int> NicknameGetPrimary { get; set; }          // 索引列表 用會員名稱 找主索引 Key:Nickname Vaule:PrimaryID
        protected Dictionary<int, byte> MemberTypeGetPrimary { get; set; }    // 索引列表 用Bot 找PK Key:Nickname Vaule:PrimaryID
        private bool isKick = false;                                                // 是否被踢了
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();   // LOG


        public ActorCollection()                                                    // 初始化
        {
            ConnectedClients = new Dictionary<Guid, MPServerPeer>();
            PrimaryGetActor = new Dictionary<int, Actor>();
            PrimaryGetNickname = new Dictionary<int, string>();
            PrimaryGetAccount = new Dictionary<int, string>();
            PrimaryGetGuid = new Dictionary<int, Guid>();

            GuidGetPrimary = new Dictionary<Guid, int>();
            AccountGetPrimary = new Dictionary<string, int>();
            NicknameGetPrimary = new Dictionary<string, int>();
            MemberTypeGetPrimary = new Dictionary<int, byte>();
        }

        #region 線上會員 ActorOnline
        /// <summary>
        /// 加入一筆線上會員資料(成功回傳 (1)，不成功回傳 (2)錯誤碼)
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="PrimaryID"></param>
        /// <param name="Account"></param>
        /// <param name="Nickname"></param>
        /// <param name="Age"></param>
        /// <param name="Sex"></param>
        /// <param name="IP"></param>
        /// <param name="ServerPeer"></param>
        /// <returns></returns>

        public ActorReturn ActorOnline(Guid guid, int PrimaryID, string Account, string Nickname, byte Age, byte Sex, string IP, MPServerPeer serverPeer, byte memberType)
        {
            ActorReturn actorReturn = new ActorReturn();
            actorReturn.ReturnCode = "S300";

            if (PrimaryID <= 0)
            {
                actorReturn.ReturnCode = "S303";
                actorReturn.DebugMessage = "PrimaryID必須有索引值！";
                return actorReturn;
            }

            if (Account.Length <= 0)
            {
                actorReturn.ReturnCode = "S304";
                actorReturn.DebugMessage = "Account必須填入帳號！";
                return actorReturn;
            }

            if (Nickname.Length <= 0)
            {
                actorReturn.ReturnCode = "S305";
                actorReturn.DebugMessage = "Nickname必須填入角色名稱！";
                return actorReturn;
            }

            if (Age <= 0)
            {
                actorReturn.ReturnCode = "S306";
                actorReturn.DebugMessage = "Age必須填入年齡！";
                return actorReturn;
            }

            if (Sex < 0)
            {
                actorReturn.ReturnCode = "S307";
                actorReturn.DebugMessage = "Sex必須填入性別！";
                return actorReturn;
            }


            try
            {

                lock (this)
                {
                    // PrimaryGetGuid[i] 不能有null值 不能用來判斷bool ，PrimaryGetGuid.ContainsKey(i) 可以用來判斷bool
                    // 檢查dictGetAccounts的索引確保沒有重複登入 
                    if (PrimaryGetGuid.ContainsKey(PrimaryID))
                    {
                        actorReturn.ReturnCode = "S302";
                        actorReturn.DebugMessage = "重複登入！";
                        return actorReturn;                              // 不允許重複登入
                    }
                    else
                    {
                        Log.Debug("登入的GUID:" + guid);
                        GuidGetPrimary.Add(guid, PrimaryID);             // 加入guid索引會員編號的列表

                        Actor actor = new Actor(guid, PrimaryID, Account, Nickname, Age, Sex, IP);
                        actor.LoginTime = System.DateTime.Now;          // 登入時間
                        actor.GameStatus = 1;                           // 代表上線中(閒置)

                        if (!ConnectedClients.ContainsKey(guid))
                        {
                            ConnectedClients.Add(guid, serverPeer);
                            Log.Debug("ADD Peer!");
                        }

                        if (!PrimaryGetActor.ContainsKey(PrimaryID))
                            PrimaryGetActor.Add(PrimaryID, actor);              // 加入線上會員列表

                        if (!PrimaryGetNickname.ContainsKey(PrimaryID))   // 加入線上會員名稱 索引
                            PrimaryGetNickname.Add(PrimaryID, Nickname);

                        if (!PrimaryGetAccount.ContainsKey(PrimaryID))      // 加入線上帳號名稱 索引
                            PrimaryGetAccount.Add(PrimaryID, Account);

                        if (!PrimaryGetGuid.ContainsKey(PrimaryID))         // 加入線上GUID名稱 索引
                            PrimaryGetGuid.Add(PrimaryID, guid);

                        if (!AccountGetPrimary.ContainsKey(Account))    // 若會員帳號索引會員編號列表沒有資料，加入索引
                            AccountGetPrimary.Add(Account, PrimaryID);

                        if (!NicknameGetPrimary.ContainsKey(Nickname))  // 若會員帳號暱稱索引會員編號列表沒有資料，加入索引
                            NicknameGetPrimary.Add(Nickname, PrimaryID);

                        if (!MemberTypeGetPrimary.ContainsKey(PrimaryID) && memberType == (byte)MemberType.Bot)  // 若會員帳號暱稱索引會員編號列表沒有資料，加入索引
                            MemberTypeGetPrimary.Add(PrimaryID, memberType);

                        Log.Debug("MemberTypeGetPrimary:" + MemberTypeGetPrimary.Count + "PrimaryID: " + PrimaryID+ "  memberType: " + memberType);
                        actorReturn.ReturnCode = "S301";                     // 加入線上會員資料成功
                        actorReturn.DebugMessage = "";

                    }
                }
                return actorReturn;
            }
            catch (Exception e)
            {
                Log.Debug("Actors例外: " + e);
                actorReturn.ReturnCode = "S399";                     // 加入線上會員資料成功
                actorReturn.DebugMessage = "未知例外情況";
                return actorReturn;
            }

        }
        #endregion

        #region 移除線上會員 ActorOffline
        /// <summary>
        /// 登出一筆會員資料，會順便移除Peer
        /// </summary>
        /// <param name="Guid"></param>

        public void ActorOffline(Guid guid)
        {
            if (!isKick)
            {
                ActorReturn actorReturn = new ActorReturn();
                lock (this)
                {
                    try
                    {
                        RemoveConnectedPeer(guid); // 移除Peer

                        int _PrimaryID = 0;
                        if (GuidGetPrimary.ContainsKey(guid)) // 若有資料
                        {
                            Log.Debug("OFF 移除GUID:" + guid);
                            _PrimaryID = GuidGetPrimary[guid];
                            GuidGetPrimary.Remove(guid);      // 移除guid列表資料

                            if (PrimaryGetActor.ContainsKey(_PrimaryID))                // 若會員列表有資料
                            {
                                Actor actor = GetActorFromPrimary(_PrimaryID);          // 先取得會員資料

                                if (PrimaryGetNickname.ContainsKey(_PrimaryID))         // 移除 索引找會員名稱列表
                                    PrimaryGetNickname.Remove(_PrimaryID);

                                if (PrimaryGetAccount.ContainsKey(_PrimaryID))           // 移除 索引找會員名稱列表
                                    PrimaryGetAccount.Remove(_PrimaryID);

                                if (PrimaryGetGuid.ContainsKey(_PrimaryID))             // 移除線上GUID名稱列表
                                    PrimaryGetGuid.Remove(_PrimaryID);

                                if (AccountGetPrimary.ContainsKey(actor.Account))       // 移除會員帳號索引列表資料
                                    AccountGetPrimary.Remove(actor.Account);

                                if (NicknameGetPrimary.ContainsKey(actor.Nickname))     // 移除會員暱稱索引列表資料
                                    NicknameGetPrimary.Remove(actor.Nickname);

                                if (MemberTypeGetPrimary.ContainsKey(_PrimaryID))     // 移除會員類別索引列表資料
                                    MemberTypeGetPrimary.Remove(_PrimaryID);

                                PrimaryGetActor.Remove(_PrimaryID);                     // 移除會員列表資料

                                actorReturn.ReturnCode = "S308";
                                actorReturn.DebugMessage = "移除玩家成功！";
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Debug("沒移除GUID:" + guid);
                        actorReturn.ReturnCode = "S309";
                        actorReturn.DebugMessage = "不在線上列表 " + e.Message;
                    }
                }
            }
        }
        #endregion

        #region 踢除線上會員 ActorKick
        /// <summary>
        /// 登出一筆會員資料，會順便移除Peer
        /// </summary>
        /// <param name="primaryID"></param>
        public void ActorKick(int primaryID)
        {
            isKick = !isKick;
            Log.Debug("KICK");
            ActorReturn actorReturn = new ActorReturn();
            Guid guid = Guid.Empty;
            lock (this)
            {
                try
                {

                    if (PrimaryGetGuid.ContainsKey(primaryID))
                    {
                        guid = GetGuidFromPrimary(primaryID);

                        RemoveConnectedPeer(guid); // 移除Peer
                        Log.Debug("(IN)HE Guid IS :" + guid);
                    }

                    Log.Debug("(OUT)HE Guid IS :" + guid);

                    int _PrimaryID = 0;
                    if (GuidGetPrimary.ContainsKey(guid)) // 若有資料
                    {
                        Log.Debug("KICK 移除GUID:" + guid);
                        _PrimaryID = GuidGetPrimary[guid];
                        GuidGetPrimary.Remove(guid);      // 移除guid列表資料

                        if (PrimaryGetActor.ContainsKey(_PrimaryID))                // 若會員列表有資料
                        {
                            Actor actor = GetActorFromPrimary(_PrimaryID);          // 先取得會員資料

                            if (PrimaryGetNickname.ContainsKey(_PrimaryID))         // 移除 索引找會員名稱列表
                                PrimaryGetNickname.Remove(_PrimaryID);

                            if (PrimaryGetAccount.ContainsKey(_PrimaryID))           // 移除 索引找會員名稱列表
                                PrimaryGetAccount.Remove(_PrimaryID);

                            if (PrimaryGetGuid.ContainsKey(_PrimaryID))             // 移除線上GUID名稱列表
                                PrimaryGetGuid.Remove(_PrimaryID);

                            if (AccountGetPrimary.ContainsKey(actor.Account))       // 移除會員帳號索引列表資料
                                AccountGetPrimary.Remove(actor.Account);

                            if (NicknameGetPrimary.ContainsKey(actor.Nickname))     // 移除會員暱稱索引列表資料
                                NicknameGetPrimary.Remove(actor.Nickname);

                            if (MemberTypeGetPrimary.ContainsKey(_PrimaryID))     // 移除會員類別索引列表資料
                                MemberTypeGetPrimary.Remove(_PrimaryID);

                            PrimaryGetActor.Remove(_PrimaryID);                     // 移除會員列表資料

                            actorReturn.ReturnCode = "S308";
                            actorReturn.DebugMessage = "移除玩家成功";
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Debug("沒移除PrimaryID:" + primaryID);
                    actorReturn.ReturnCode = "S309";
                    actorReturn.DebugMessage = "不在線上列表 " + e.Message;
                }
            }

        }
        #endregion

        public void AddConnectedPeer(Guid guid, MPServerPeer serverPeer)    // 加入連接的Peer
        {
            ConnectedClients.Add(guid, serverPeer);
        }

        public void RemoveConnectedPeer(Guid guid)                          // 移除連接的Peer
        {
            ConnectedClients.Remove(guid);
        }

        /// <summary>
        /// 用索引 找 玩家資料
        /// </summary>
        /// <param name="PrimaryID"></param>
        /// <returns></returns>
        public Actor GetActorFromPrimary(int PrimaryID)                     // 用索引 找 玩家資料
        {
            Actor actor;
            PrimaryGetActor.TryGetValue(PrimaryID, out actor);
            return actor;
        }

        /// <summary>
        /// 用索引 找 GUID
        /// </summary>
        /// <param name="primaryID"></param>
        /// <returns></returns>
        public Guid GetGuidFromPrimary(int primaryID)                       // 用索引 找 GUID
        {
            Log.Debug("=GetGuidFromPrimary1");
            Guid guid = Guid.Empty;
            guid = PrimaryGetGuid[primaryID];
            Log.Debug("=GetGuidFromPrimary2" + guid);
            return guid;
        }

        /// <summary>
        /// 用索引 找 Peer
        /// </summary>
        /// <param name="primaryID"></param>
        /// <returns></returns>
        public MPServerPeer GetPeerFromPrimary(int primaryID)               // 用索引 找 Peer
        {
            Log.Debug("GetPeerFromPrimary1");
            MPServerPeer peer;
            peer = GetPeerFromGuid(GetGuidFromPrimary(primaryID));
            Log.Debug("GetPeerFromPrimary2" + peer.peerGuid);
            return peer;
        }

        /// <summary>
        /// 以 Guid 取得 玩家資料
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>
        /// 1.成功=Actor
        /// 2.失敗=null
        /// </returns>
        public Actor GetActorFromGuid(Guid guid)                            // 以 guid 取得 玩家資料
        {
            if (!GuidGetPrimary.ContainsKey(guid))
            {
                return null; //找不到
            }
            else
            {
                return GetActorFromPrimary(GuidGetPrimary[guid]);           // 用guid找PrimaryID 再用PrimaryID找 玩家資料
            }
        }

        /// <summary>
        /// 用Guid 取得 玩家暱稱
        /// </summary>
        /// <param name="Guid"></param>
        public string GetNicknameFromGuid(Guid guid)                        // 以 guid 取得 玩家暱稱
        {
            if (!GuidGetPrimary.ContainsKey(guid))
            {
                return null; //找不到
            }
            else
            {
                return PrimaryGetNickname[GuidGetPrimary[guid]];            // 用guid找PrimaryID 再用PrimaryID找 暱稱
            }
        }

        /// <summary>
        /// guid 取得 玩家暱稱
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public string GetAccountFromGuid(Guid guid)                         // 以 guid 取得 玩家暱稱
        {
            if (!GuidGetPrimary.ContainsKey(guid))
            {
                return null; //找不到
            }
            else
            {
                return PrimaryGetAccount[GuidGetPrimary[guid]];             // 用guid找PrimaryID 再用PrimaryID找 暱稱
            }
        }

        /// <summary>
        /// 以帳號 取得 玩家資料
        /// </summary>
        /// <param name="Account"></param>
        /// <returns></returns>
        public Actor GetActorFromAccount(string Account)                    // 以帳號 取得 玩家資料
        {
            if (!AccountGetPrimary.ContainsKey(Account))
            {
                return null; //找不到
            }
            else
            {
                return GetActorFromPrimary(AccountGetPrimary[Account]);     // 用Account找PrimaryID 再用PrimaryID找 玩家資料
            }
        }

        /// <summary>
        /// 以玩家名稱 取得 玩家資料
        /// </summary>
        /// <param name="Nickname"></param>
        /// <returns></returns>
        public Actor GetActorFromNickname(string Nickname)                  // 以玩家名稱 取得 玩家資料
        {
            if (!NicknameGetPrimary.ContainsKey(Nickname))
            {
                return null; //找不到
            }
            else
            {
                return GetActorFromPrimary(NicknameGetPrimary[Nickname]);   // 用Nickname找PrimaryID 再用PrimaryID找 玩家資料
            }
        }

        /// <summary>
        /// 以玩家名稱 取得 玩家資料
        /// </summary>
        /// <param name="Nickname"></param>
        /// <returns></returns>
        public Actor GetActorFromMemberType(byte memberType)                  // 以玩家名稱 取得 玩家資料
        {
            Log.Debug("In GetActorFromMemberType");
            if (!MemberTypeGetPrimary.ContainsValue(memberType))
            {
                foreach (KeyValuePair<int, byte> item in MemberTypeGetPrimary)
                {
                    Log.Debug("In Fuck: " + memberType +" KEY: "+item.Key+" Value: "+item.Value);
                }
                
                return null; //找不到
            }
            else
            {
                List<int> keys = new List<int>(MemberTypeGetPrimary.Keys);
                Log.Debug("In keys:" + keys.Count);
                Actor actor = GetActorFromPrimary(keys[0]);
                Log.Debug("In actor:" + actor.Nickname);
                return actor;   // 用memberType找PrimaryID 再用PrimaryID找 玩家資料
            }
        }

        public Dictionary<Guid, MPServerPeer> GetOnlineActors()
        {
            return ConnectedClients;
        }

        /// <summary>
        /// 用Guid 找 Peer
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public MPServerPeer GetPeerFromGuid(Guid guid)                      // 用機器碼 找 peer
        {
            MPServerPeer peer;
            ConnectedClients.TryGetValue(guid, out peer);
            return peer;
        }

        public class ActorReturn                                            // 回傳值
        {
            public string ReturnCode { get; set; }
            public string DebugMessage { get; set; }
        }
    }
}
