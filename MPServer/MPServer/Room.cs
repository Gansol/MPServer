using System;
using System.Collections.Generic;
using ExitGames.Logging;
using System.Linq;

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
 * 這裡的程式碼還很亂 測試版的建立房間 只能開一間房
 * 還沒測試 多人同時開房間 和加入房間 與移除房間的情況)
 *  _waitingList.Remove(1); 1的值代表示 1間房 只移除字典檔中的1 以後多間房要改
 * _waitIndex 現在也沒有增減 如果等待房要很多 需要增減
 * 
 * ***************************************************************/

namespace MPServer
{
    public class Room
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private static Dictionary<int, Dictionary<Guid, RoomActor>> _dictPlayingRoomList;               // 遊戲中的會員列表
        private static Dictionary<int, Dictionary<Guid, RoomActor>> _dictWaitingRoomList;               // 等待房間中的會員列表
        private static Dictionary<int, Dictionary<Guid, RoomActor>> _dictPrivateRoomList;               // 私人等待房間中的會員列表
        private static Dictionary<int, Dictionary<int, Guid>> _loadedPlayer;                                         // 在房間中，已載入遊戲的玩家數量
        /// <summary>
        /// RoomID,HP 房間內BossHP
        /// </summary>
        private static Dictionary<int, int> _worldBossHP;                                          // 自己房間中的BOSSHP
        private static Dictionary<Guid, int> _guidGetPlayingRoom;
        /// <summary>
        /// 包含等待中的私人房間
        /// </summary>
        private static Dictionary<Guid, int> _guidGetWaitingRoom;
        // private static int _waitIndex;
        //private static int _roomIndex;                              // ＊＊＊＊＊因為是房間起始值為1 
        private static readonly int _roomStartIndex = 1;                  // 最大房間數量
        private static readonly int _maxRoomCount = 100;                  // 最大房間數量
        private static int _roomPlayerLimit;
        private static List<int> _roomIndex, _playingRoomIndex;        // 遊戲房間編號
        //private int _myRoom;                                        // ＊＊＊＊＊因為是房間起始值為1


        //public Dictionary<string, RoomActor> gameRoom;
        public Dictionary<int, Dictionary<Guid, RoomActor>> dictWaitingRoomList { get { return _dictWaitingRoomList; } }
        public Dictionary<int, Dictionary<Guid, RoomActor>> dictPrivateRoomList { get { return _dictPrivateRoomList; } }
        public Dictionary<int, Dictionary<Guid, RoomActor>> dictPlayingRoomList { get { return _dictPlayingRoomList; } }
        //  public int myRoom { get { return _myRoom; } }
        public int RoomPlayerLimit { get { return _roomPlayerLimit; } }
        public string roomName { get; set; }

        public enum RoomStatus
        {
            Waiting,
            Loading,
            Gaming,
        }

        public Room() //init
        {
            _dictWaitingRoomList = new Dictionary<int, Dictionary<Guid, RoomActor>>();
            _dictPlayingRoomList = new Dictionary<int, Dictionary<Guid, RoomActor>>();
            _dictPrivateRoomList = new Dictionary<int, Dictionary<Guid, RoomActor>>();
            _loadedPlayer = new Dictionary<int, Dictionary<int, Guid>>();
            _worldBossHP = new Dictionary<int, int>();
            _guidGetPlayingRoom = new Dictionary<Guid, int>();
            _guidGetWaitingRoom = new Dictionary<Guid, int>();
            // _waitIndex = 1;
            // _myRoom = 1;
            _roomPlayerLimit = 2;
            _roomIndex = new List<int>(Enumerable.Range(_roomStartIndex, _maxRoomCount).ToList());
            _playingRoomIndex = new List<int>();
        }

        #region CreateRoom 建立房間
        /// <summary>
        /// 建立房間 返回Bool
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="PrimaryID"></param>
        /// <param name="Account"></param>
        /// <param name="Nickname"></param>
        /// <param name="Age"></param>
        /// <param name="Sex"></param>
        /// <param name="IP"></param>
        /// <returns></returns>
        public bool CreateRoom(Guid guid, int primaryID, string account, string nickname, byte age, byte sex, string IP)
        {
            lock (this)                     // 鎖定程式碼區塊 避免同時間建立同樣的房間
            {
                if (_dictWaitingRoomList.Count == 0) // 限制只有1間房間 需要測後再取消
                {
                    //加入第一間等待房間 並把開房者標示為第一個玩家
                    _dictWaitingRoomList.Add(_roomIndex[0], new Dictionary<Guid, RoomActor>());
                    _dictWaitingRoomList[_roomIndex[0]].Add(guid, new RoomActor(guid, primaryID, account, nickname, age, sex, IP));
                    _guidGetWaitingRoom.Add(guid, _roomIndex[0]);
                    Log.Debug("Create Room _roomIndex[0] :" + _roomIndex[0]);

                    _roomIndex.Remove(_roomIndex[0]);
                    //roomName = "GameRoom" + _nameIndex;
                    //_waitIndex++;
                    return true;
                }
                return false;
            }
        }
        #endregion



        #region CreatePrivateRoom 建立好友房間
        /// <summary>
        /// 建立房間 返回Bool
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="PrimaryID"></param>
        /// <param name="Account"></param>
        /// <param name="Nickname"></param>
        /// <param name="Age"></param>
        /// <param name="Sex"></param>
        /// <param name="IP"></param>
        /// <returns></returns>
        public bool CreatePrivateRoom(Guid guid, Guid otherGuid, int primaryID, string account, string nickname, byte age, byte sex, string IP)
        {
            lock (this)                     // 鎖定程式碼區塊 避免同時間建立同樣的房間
            {
                Log.Debug("GUID 1: " + guid + "    GUID 2: " + otherGuid);
                foreach (KeyValuePair<int, Dictionary<Guid, RoomActor>> room in _dictPrivateRoomList)
                {
                    if (room.Value.ContainsKey(otherGuid))
                    {
                        Log.Debug("CreatePrivateRoom 已建立好友房間!");
                        return false;
                    }
                }

                //加入第一間等待房間 並把開房者標示為第一個玩家
                _dictPrivateRoomList.Add(_roomIndex[0], new Dictionary<Guid, RoomActor>());
                _dictPrivateRoomList[_roomIndex[0]].Add(guid, new RoomActor(guid, primaryID, account, nickname, age, sex, IP));
                _guidGetWaitingRoom.Add(guid, _roomIndex[0]);
                Log.Debug("Create Priavte Room _roomIndex[0] :" + _roomIndex[0]);
                _roomIndex.Remove(_roomIndex[0]);
                return true;
            }
        }
        #endregion

        #region JoinRoom 加入房間

        /// <summary>
        /// 將玩家加入已經建立的房間 返回Bool
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="primaryID"></param>
        /// <param name="account"></param>
        /// <param name="nickname"></param>
        /// <param name="age"></param>
        /// <param name="sex"></param>
        /// <param name="IP"></param>
        /// <returns></returns>
        public bool JoinRoom(Guid guid, int primaryID, string account, string nickname, byte age, byte sex, string IP) //JoinRoom
        {
            Log.Debug("in JoinRoom");
            lock (this)
            {
                try
                {
                    foreach (KeyValuePair<int, Dictionary<Guid, RoomActor>> item in _dictWaitingRoomList)  // 這是用來給很多間房間用的 找遍所有房間
                    {
                        if (item.Value.Count < _roomPlayerLimit)   //假如房間沒有滿 加入玩家
                        {
                            RoomActor actor = new RoomActor(guid, primaryID, account, nickname, age, sex, IP);
                            actor.roomID = item.Key;
                            item.Value.Add(guid, actor);

                            _dictPlayingRoomList.Add(item.Key, item.Value); // 房間ID item.Value=這間房

                            foreach (KeyValuePair<Guid, RoomActor> player in item.Value)  // 把這間房間的玩家 加入索引 可以用GUID來取得房間ID
                            {
                                Log.Debug("Join Player :" + player.Value.Nickname);
                                if (_guidGetPlayingRoom.ContainsKey(player.Value.guid)) 
                                    _guidGetPlayingRoom.Remove(player.Value.guid);
                                _guidGetPlayingRoom.Add(player.Value.guid, item.Key);
                                _guidGetWaitingRoom.Remove(player.Key);
                            }
                            if (_roomIndex.Contains(item.Key)) _roomIndex.Remove(item.Key);
                            if (!_playingRoomIndex.Contains(item.Key)) _playingRoomIndex.Add(item.Key);
                            RemoveWaitingRoom(item.Key, guid);  // 可能錯誤 新增的0521 改MatchGameFriend時增加
                            Log.Debug("in JoinRoom ID:" + item.Key);
                            return true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Debug("找不到房間加入:" + e + "  於程式碼:" + e.StackTrace);
                }
                /*
                if (waitList[waitIndex].Count != 2)
                {
                    waitList[waitIndex].Add("RoomActor2", new RoomActor(guid, PrimaryID, Account, Nickname, Age, Sex, IP));
                    
                    return true;
                }
                */
                return false;
            }
        }
        #endregion


        #region JoinPrivateRoom 加入好友房間

        /// <summary>
        /// 將玩家加入已經建立的房間 返回Bool
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="primaryID"></param>
        /// <param name="account"></param>
        /// <param name="nickname"></param>
        /// <param name="age"></param>
        /// <param name="sex"></param>
        /// <param name="IP"></param>
        /// <returns></returns>
        public bool JoinPrivateRoom(Guid guid, Guid otherGuid, int primaryID, string account, string nickname, byte age, byte sex, string IP) //JoinRoom
        {
            Log.Debug("in JoinPrivateRoom");
            lock (this)
            {
                try
                {
                    Log.Debug("GUID 1: " + guid + "    GUID 2: " + otherGuid);
                    foreach (KeyValuePair<int, Dictionary<Guid, RoomActor>> item in _dictPrivateRoomList)  // 這是用來給很多間房間用的 找遍所有房間
                    {

                        if (item.Value.ContainsKey(otherGuid))
                        {
                            Log.Debug("JoinPrivateRoom RoomID:" + item.Key);



                            RoomActor actor = new RoomActor(guid, primaryID, account, nickname, age, sex, IP);
                            actor.roomID = item.Key;
                            _dictPrivateRoomList[item.Key].Add(guid, actor);

                            _dictPlayingRoomList.Add(item.Key, item.Value); // 房間ID item.Value=這間房

                            foreach (KeyValuePair<Guid, RoomActor> player in item.Value)  // 把這間房間的玩家 加入索引 可以用GUID來取得房間ID
                            {
                                Log.Debug("Join Player :" + player.Value.Nickname);
                                if (_guidGetPlayingRoom.ContainsKey(player.Value.guid))
                                    _guidGetPlayingRoom.Remove(player.Value.guid);
                                _guidGetPlayingRoom.Add(player.Value.guid, item.Key);
                                _guidGetWaitingRoom.Remove(player.Key);
                            }
                            if (_roomIndex.Contains(item.Key)) _roomIndex.Remove(item.Key);
                            if (!_playingRoomIndex.Contains(item.Key)) _playingRoomIndex.Add(item.Key);
                            RemoveWaitingRoom(item.Key, guid);
                            Log.Debug("in Join PrivareRoom ID:" + item.Key);
                            return true;
                        }
                    }
                    return false;
                }
                catch (Exception e)
                {
                    Log.Debug("找不到房間加入:" + e + "  於程式碼:" + e.StackTrace);
                }
                /*
                if (waitList[waitIndex].Count != 2)
                {
                    waitList[waitIndex].Add("RoomActor2", new RoomActor(guid, PrimaryID, Account, Nickname, Age, Sex, IP));
                    
                    return true;
                }
                */
                return false;
            }
        }
        #endregion

        #region RemoveWatingRoom 移除等待房間 這裡以後要改寫 現在只能移除指定房間 房間(1)

        /// <summary>
        /// 移除等待房間
        /// </summary>
        public void RemoveWaitingRoom(int roomID, Guid guid)
        {
            lock (this)
            {
                if (_dictWaitingRoomList.ContainsKey(roomID))    //假如 等待列表中有等代房間
                {
                    _dictWaitingRoomList.Remove(roomID);
                    _guidGetWaitingRoom.Remove(guid);

                    if (!_dictPlayingRoomList.ContainsKey(roomID) && roomID > 0)
                        _roomIndex.Add(roomID);
                    _roomIndex.Sort();

                    Log.Debug("Waiting Room " + roomID + " been removed!");
                    //_waitIndex--;
                }

                if (_dictPrivateRoomList.ContainsKey(roomID))    //假如 等待列表中有等代房間
                {
                    _dictPrivateRoomList.Remove(roomID);
                    _guidGetWaitingRoom.Remove(guid);

                    if (!_dictPlayingRoomList.ContainsKey(roomID) && roomID > 0)
                        _roomIndex.Add(roomID);
                    _roomIndex.Sort();
                    Log.Debug("Private Room " + roomID + " been removed!");
                    //_waitIndex--;
                }

                //if (_dictWaitingRoomList.Count != 0)    //假如 等待列表中有等代房間
                //{
                //    _dictWaitingRoomList.Remove(1);  //因為等待列表中只有 1間房
                //    Log.Debug("Waiting Room " + _myRoom + " been removed!");
                //    //_waitIndex--;
                //}
            }
        }
        #endregion

        //#region RemovePlayingRoom 移除遊戲中房間(使用RoomID)
        ///// <summary>
        ///// 移除遊戲中房間(使用RoomID)
        ///// </summary>
        ///// <param name="roomID"></param>
        //public void RemovePlayingRoom(int roomID)
        //{
        //    lock (this)
        //    {
        //        if (_dictPlayingRoomList.ContainsKey(roomID))     // 假如遊戲中房間找的到
        //        {
        //            _dictPlayingRoomList.Remove(roomID);                              // 這看不懂是啥
        //            Log.Debug("Game Room " + roomID + " been removed!");
        //            _roomIndex--;                                           // 房間移除後 index--
        //        }

        //        Log.Debug("RemovePlayingRoom = " + _dictPlayingRoomList.ContainsKey(roomID));
        //    }

        //}
        //#endregion

        #region RemovePlayingRoom 移除遊戲中房間、房間中玩家(房間索引列表) (使用RoomID,GUID player1 GUID player2)
        /// <summary>
        /// 移除遊戲中房間、房間中玩家(房間索引列表) (使用RoomID,GUID player1 GUID player2)
        /// </summary>
        /// <param name="roomID"></param>
        /// <param name="player1"></param>
        /// <param name="player2"></param>
        public void RemovePlayingRoom(int roomID, Guid player1, Guid player2)
        {
            lock (this)
            {
                if (roomID > 0)
                {
                    //Dictionary<Guid, RoomActor> room;
                    Log.Debug("Game Room " + roomID + " been removed!");
                    _guidGetPlayingRoom.Remove(player1);
                    _guidGetPlayingRoom.Remove(player2);
                    _dictPlayingRoomList.Remove(roomID);

                    _roomIndex.Add(roomID);
                    _roomIndex.Sort();
                }
                else
                {
                    if (_guidGetPlayingRoom.ContainsKey(player1)) _guidGetPlayingRoom.TryGetValue(player1, out roomID);
                    if (_guidGetPlayingRoom.ContainsKey(player2)) _guidGetPlayingRoom.TryGetValue(player2, out roomID);

                    _dictPlayingRoomList.Remove(roomID);

                    Log.Debug("(2)Game Room " + roomID + " been removed!");

                    if (roomID > 0)
                        _roomIndex.Add(roomID);
                    _roomIndex.Sort();
                }

                _playingRoomIndex.Remove(roomID);

                //    if (_guidGetPlayingRoom.ContainsKey(player1))
                //    {
                //        _guidGetPlayingRoom.Remove(player1);
                //        _dictPlayingRoomList.TryGetValue(roomID, out room);
                //        List<Guid> keys = new List<Guid>(room.Keys);

                //        foreach (Guid key in keys)
                //        {
                //            _guidGetPlayingRoom.Remove(key);
                //        }
                //    }

                //    if (_guidGetPlayingRoom.ContainsKey(player2))
                //    {
                //        _guidGetPlayingRoom.Remove(player2);
                //    }
                //}





                //if (_dictPlayingRoomList.ContainsKey(roomID))
                //{
                //    Log.Debug("Game Room " + roomID + " been removed!");
                //    _dictPlayingRoomList.Remove(roomID);
                //    if (_guidGetPlayingRoom.ContainsKey(player1))
                //        _guidGetPlayingRoom.Remove(player1);
                //    if (_guidGetPlayingRoom.ContainsKey(player2))
                //        _guidGetPlayingRoom.Remove(player2);






                //    if (roomID > 0)

                //}
                //else
                //{
                //    if (_guidGetPlayingRoom.ContainsKey(player1))
                //        _guidGetPlayingRoom.Remove(player1);
                //    if (_guidGetPlayingRoom.ContainsKey(player2))
                //        _guidGetPlayingRoom.Remove(player2);

                //    if (!_roomIndex.Contains(roomID))
                //        if (_playingRoomIndex.Contains(roomID) && roomID > 0)
                //            _roomIndex.Add(roomID);

                //    //  Log.Debug("Cant't RemovePlayingRoom = " + _dictPlayingRoomList.ContainsKey(roomID));
                //}

                //}

            }
        }
        #endregion

        #region GameLoaded 同步開始遊戲
        /// <summary>
        /// 同步開始遊戲
        /// </summary>
        /// <param name="roomID"></param>
        /// <param name="primaryID"></param>
        /// <param name="guid"></param>
        public void AddGameLoaded(int roomID, int primaryID, Guid guid)
        {
            if (!_loadedPlayer.ContainsKey(roomID))
                _loadedPlayer.Add(roomID, new Dictionary<int, Guid> { { primaryID, guid } });
            else
                _loadedPlayer[roomID].Add(primaryID, guid);
        }
        #endregion

        #region GetLoadedFromRoom 取得載入遊戲的玩家
        /// <summary>
        /// 取得房間內已載入遊戲的玩家
        /// </summary>
        /// <param name="roomID"></param>
        /// <returns>回傳是否載入</returns>
        public Dictionary<int, Guid> GetLoadedPlayerFromRoom(int roomID)
        {
            if (_loadedPlayer.ContainsKey(roomID))
                return _loadedPlayer[roomID];
            else
                return null;
        }
        #endregion

        #region GetLoadedFromRoom 取得載入遊戲的玩家
        /// <summary>
        /// 取得房間內已載入遊戲的玩家
        /// </summary>
        /// <param name="roomID"></param>
        /// <returns>回傳是否載入</returns>
        public bool bPlayerLoadedFromRoom(int roomID)
        {
            if (_loadedPlayer.ContainsKey(roomID))
                return true;
            return false;
        }
        #endregion

        #region RemoveLoadedRoom 移除載入中房間及已載入的玩家
        /// <summary>
        /// 雙方載入完成時，移除載入中房間
        /// </summary>
        /// <param name="roomID"></param>
        /// <returns></returns>
        public bool RemoveLoadedRoom(int roomID)
        {

            if (_loadedPlayer.ContainsKey(roomID))
            {
                _loadedPlayer.Remove(roomID);
                Log.Debug("Loaded room has benn remove!" + roomID);
                return true;
            }
            Log.Debug("ERROR: room has't benn remove!" + roomID);
            return false;
        }
        #endregion

        /// <summary>
        /// 用GUID找房間
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>RoomID</returns>
        public int GetPlayingRoomFromGuid(Guid guid)
        {
            if (_guidGetPlayingRoom.Count > 0)
            {
                int roomID;
                _guidGetPlayingRoom.TryGetValue(guid, out roomID);
                return roomID;
            }
            return -1;
        }

        /// <summary>
        /// 用GUID找房間
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>RoomID</returns>
        public int GetWaitingRoomFromGuid(Guid guid)
        {
            if (_guidGetWaitingRoom.Count > 0)
            {
                int roomID;
                _guidGetWaitingRoom.TryGetValue(guid, out roomID);
                Log.Debug("GetWaitingRoomFromGuid:" + roomID);
                return roomID;
            }
            return -1;
        }

        /// <summary>
        /// 用GUID找遊戲中 房間中玩家
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>RoomActor or null</returns>
        public RoomActor GetActorFromGuid(Guid guid)
        {
            //  Log.Debug("IN GetActorFromGuid:"+guid);
            //  Log.Debug("_guidGetRoom.Count:" + _guidGetRoom.Count + "   _playingRoomList.Count:" + _dictPlayingRoomList.Count);
            if (_guidGetPlayingRoom.Count > 0 && _dictPlayingRoomList.Count > 0)
            {
                int roomID = -1;
                RoomActor roomActor = null;

                if (_guidGetPlayingRoom.ContainsKey(guid))
                    _guidGetPlayingRoom.TryGetValue(guid, out roomID);

                if (roomID >= _roomStartIndex)
                    _dictPlayingRoomList[roomID].TryGetValue(guid, out roomActor);
                // Log.Debug("Room OtherActor:" + roomActor.Nickname);

                return roomActor;
            }
            //  Log.Debug("Room otherActor is null !!");
            return null;
        }

        /// <summary>
        /// 用GUID 找 等待列表中的玩家
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>RoomActor</returns>
        public RoomActor GetWaitActorFromGuid(Guid guid, int roomID) //＊＊＊＊＊ _waitingList[1] 一定要改成很多間房
        {
            if (_dictWaitingRoomList.ContainsKey(roomID))
            {
                RoomActor roomActor;
                _dictWaitingRoomList[roomID].TryGetValue(guid, out roomActor);   // _dictWaitingRoomList[1] 1是房間ID 錯誤
                return roomActor;
            }
            return null;
        }

        /// <summary>
        /// 用GUID 找 等待列表中的玩家 返回Bool 是否找到
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>bool</returns>
        public bool bGetWaitActorFromGuid(Guid guid, int roomID) //＊＊＊＊＊ _waitingList[1] 一定要改成很多間房
        {
            if (_dictWaitingRoomList.Count > 0)
            {
                RoomActor roomActor;
                if (_dictWaitingRoomList.ContainsKey(roomID))   //_dictWaitingRoomList[1] 1是房間ID 錯誤的
                {
                    if (_dictWaitingRoomList[roomID].TryGetValue(guid, out roomActor))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        ///  用自己的房間ID 和 主索引 找到自己遊戲中房間內的其他玩家(兩人版本)
        /// </summary>
        /// <param name="roomID"></param>
        /// <param name="myPrimaryID"></param>
        /// <returns></returns>
        public RoomActor GetPlayingRoomOtherPlayer(int roomID, int myPrimaryID)
        {
            if (_dictPlayingRoomList.Count > 0) // 是否有遊戲中房間
            {
                if (_dictPlayingRoomList.ContainsKey(roomID))
                {
                    foreach (KeyValuePair<Guid, RoomActor> item in _dictPlayingRoomList[roomID]) //用RoomID找特定遊戲中房間的玩家
                    {
                        if (item.Value.PrimaryID != myPrimaryID) //假如不等於自己的主索引就是另一位玩家
                        {
                            Log.Debug("GetRoom OtherPlayer ID:" + item.Value.PrimaryID + " roomActor.Nickname:" + item.Value.Nickname);
                            return item.Value;
                        }
                    }
                }
                else
                {
                    Log.Debug("ERROR: Can't GetOtherPlayer RoomID: " + roomID + "   PK: " + myPrimaryID);
                    return null;
                }
            }
            else
            {
                Log.Debug("ERROR:  _dictPlayingRoomList Count =0" + "RoomID: " + roomID + "   PK: " + myPrimaryID);
            }
            return null;
        }

        /// <summary>
        ///  用自己的房間ID 和 主索引 找到等待中房間內的其他玩家
        /// </summary>
        /// <param name="roomID"></param>
        /// <param name="myPrimaryID"></param>
        /// <returns></returns>
        public RoomActor GetWaitingPlayer(int roomID, int primaryID)
        {
            if (_dictWaitingRoomList.Count > 0) // 是否有遊戲中房間
            {
                foreach (KeyValuePair<Guid, RoomActor> item in _dictWaitingRoomList[roomID]) //用RoomID找特定遊戲中房間的玩家
                {
                    if (item.Value.PrimaryID != primaryID) //假如不等於自己的主索引就是另一位玩家
                    {
                        Log.Debug("ID:" + item.Value.PrimaryID + " roomActor.Nickname:" + item.Value.Nickname);
                        return item.Value;
                    }
                }
            }
            return null;
        }

        public void SpawnBoss(int roomID, int hp)
        {
            if (_worldBossHP.ContainsKey(roomID))
                _worldBossHP.Remove(roomID);
            _worldBossHP.Add(roomID, hp);
        }

        public int UpdateBossHP(int roomID, int damage)
        {
            int hp;
            _worldBossHP.TryGetValue(roomID, out hp);
            hp = Math.Max(0, hp - damage);
            _worldBossHP[roomID] = hp;
            return hp;
        }


        public void KillBoss(int roomID)
        {
            lock (this)
            {
                int tmp;
                bool hasBoss = _worldBossHP.TryGetValue(roomID, out tmp);
                if (hasBoss)
                    _worldBossHP.Remove(roomID);
            }
        }

        public int GetBossHP(int roomID)
        {
            if (_worldBossHP.ContainsKey(roomID))
                return _worldBossHP[roomID];
            return 0;
        }


        /// <summary>
        /// 移除BOSS 使用GUID
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>是否有BOSS</returns>
        public void KillBossFromGuid(Guid guid)
        {
            int roomID = GetPlayingRoomFromGuid(guid);
            int tmp;
            bool hasBoss = _worldBossHP.TryGetValue(roomID, out tmp);
            if (hasBoss)
                _worldBossHP.Remove(roomID);
        }

        /*        public RoomActor GetOtherPlayer(int roomID, int primaryID)
        {
            RoomActor roomActor;
            Dictionary<int, RoomActor> _tmpDict;

             if (_playingRoomList[roomID].TryGetValue(primaryID, out roomActor))
             {
                 _tmpDict = _playingRoomList[roomID];
                 _tmpDict.Remove(primaryID);

                 return _tmpDict[0];
             }

             return null;
        }
         * */


        // 房間玩家類別 繼承會員
        public class RoomActor : Actor
        {
            protected DateTime joinTime; // 多增加進入房間時間的屬性，並且設為唯讀
            public DateTime JoinTime { get { return joinTime; } }
            public List<string> roomMice { get; set; }
            public int gameScore { get; set; }
            public int roomID { get; set; }
            public short totalScore { get; set; }
            public byte missionCompletedCount { get; set; }
            public short maxMissionCount { get; set; }
            public short life { get; set; }
            public float energy { get; set; }
            public float scoreRate { get; set; }
            public float energyRate { get; set; }

            public RoomActor(Guid guid, int PrimaryID, string Account, string Nickname, byte Age, byte Sex, string IP)
                : base(guid, PrimaryID, Account, Nickname, Age, Sex, IP)
            {
                joinTime = DateTime.Now;
                roomID = -1;
                life = -1;
                missionCompletedCount = 0;
                maxMissionCount = 0;
                energy = gameScore = totalScore = 0;
                scoreRate = energyRate = 1f;
                roomMice = new List<string>();
                this.joinTime = System.DateTime.Now;    // 記錄加人此房間的時間
            }
        }
    }


}
