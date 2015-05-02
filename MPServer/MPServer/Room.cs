using System;
using System.Collections.Generic;
using ExitGames.Logging;

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

        private Dictionary<int, Dictionary<string, RoomActor>> _playingRoomList;               // 等待房間中的會員列表
        private Dictionary<int, Dictionary<string, RoomActor>> _waitingList;               // 等待房間中的會員列表
        private Dictionary<Guid, int> _guidGetRoom;
        private List<int> _EmptyRoom;
        private static int _waitIndex;
        private static int _roomIndex;                              // ＊＊＊＊＊因為是房間起始值為1
        private static int _limit;
        private int _myRoom;                                        // ＊＊＊＊＊因為是房間起始值為1

        //public Dictionary<string, RoomActor> gameRoom;
        public Dictionary<int, Dictionary<string, RoomActor>> waitingList { get { return _waitingList; } }
        public Dictionary<int, Dictionary<string, RoomActor>> playingRoomList { get { return _playingRoomList; } }
        public int myRoom { get { return _myRoom; } }
        public int limit { get { return _limit; } }
        public string roomName { get; set; }

        public Room() //init
        {
            _waitingList = new Dictionary<int, Dictionary<string, RoomActor>>();
            _playingRoomList = new Dictionary<int, Dictionary<string, RoomActor>>();
            _guidGetRoom = new Dictionary<Guid, int>();
            _EmptyRoom = new List<int>();
            _waitIndex = 1;
            _myRoom = 1;
            _limit = 2;
            _roomIndex = 1;
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
                if (waitingList.Count == 0) // 限制只有1間房間 需要測後再取消
                {
                    //加入第一間等待房間 並把開房者標示為第一個玩家
                    waitingList.Add(_waitIndex, new Dictionary<string, RoomActor>());
                    waitingList[_waitIndex].Add("RoomActor1", new RoomActor(guid, primaryID, account, nickname, age, sex, IP));
                    //roomName = "GameRoom" + _nameIndex;
                    //_waitIndex++;
                    return true;
                }
                return false;
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
                    foreach (KeyValuePair<int, Dictionary<string, RoomActor>> item in waitingList)  // 這是用來給很多間房間用的 找遍所有房間
                    {
                        if (item.Value.Count < limit)   //假如房間沒有滿 加入玩家
                        {
                            item.Value.Add("RoomActor2", new RoomActor(guid, primaryID, account, nickname, age, sex, IP));
                            
                            // 假如 清空房間的數量 > 0  這裡看不懂再寫啥
                            if (_EmptyRoom.Count > 0)
                            {
                                Log.Debug("EmptyRoom : " + _EmptyRoom[0]);
                                _playingRoomList.Add(_EmptyRoom[0], item.Value);
                                
                                _myRoom = _EmptyRoom[0];
                                _EmptyRoom.RemoveAt(0);
                            }
                            else // 加入遊戲中房間列表
                            {
                                _playingRoomList.Add(_roomIndex, item.Value); // 房間ID item.Value=這間房
                                _myRoom = _roomIndex;
                            }

                            foreach (KeyValuePair<string, RoomActor> player in item.Value)  // 把這間房間的玩家 加入索引 可以用GUID來取得房間ID
                            {
                                Log.Debug("Join Player :" + player.Value.Nickname);
                                _guidGetRoom.Add(player.Value.guid, _myRoom);
                            }

                            _roomIndex++;   //建立1號房間後++ 以便建立第2間房
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

        #region RemoveWatingRoom 移除等待房間 這裡以後要改寫 現在只能移除指定房間 房間(1)

        /// <summary>
        /// 移除等待房間
        /// </summary>
        public void RemoveWatingRoom()
        {
            lock (this)
            {
                if (_waitingList.Count != 0)    //假如 等待列表中有等代房間
                {
                    _waitingList.Remove(1);  //因為等待列表中只有 1間房
                    Log.Debug("Waiting Room " + _myRoom + " been removed!");
                    //_waitIndex--;
                }
            }
        }
        #endregion

        #region RemovePlayingRoom 移除遊戲中房間(使用RoomID)
        /// <summary>
        /// 移除遊戲中房間(使用RoomID)
        /// </summary>
        /// <param name="roomID"></param>
        public void RemovePlayingRoom(int roomID)
        {
            lock (this)
            {
                Dictionary<string, RoomActor> gameRoom;
                if (_playingRoomList.TryGetValue(roomID, out gameRoom))     // 假如遊戲中房間找的到
                {
                    _playingRoomList.Remove(roomID);
                    _EmptyRoom.Add(roomID);                                 // 這看不懂是啥
                    Log.Debug("Game Room " + roomID + " been removed!");
                    _roomIndex--;                                           // 房間移除後 index--
                }
                                         
                Log.Debug("Found Room ? " + _playingRoomList.TryGetValue(roomID, out gameRoom));
            }

        }
        #endregion

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
                Dictionary<string, RoomActor> gameRoom;
                if (_playingRoomList.TryGetValue(roomID, out gameRoom))
                {
                    _playingRoomList.Remove(roomID);
                    _EmptyRoom.Add(roomID);
                    _guidGetRoom.Remove(player1);
                    _guidGetRoom.Remove(player2);
                    Log.Debug("Game Room " + roomID + " been removed!");
                    _roomIndex--;
                }

                Log.Debug("Found Room ? " + _playingRoomList.TryGetValue(roomID, out gameRoom));
            }

        }
        #endregion

        /// <summary>
        /// 用GUID找房間
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>

        public int GetRoomFromGuid(Guid guid)
        {
            if (_guidGetRoom.Count > 0)
            {
                int roomID;
                _guidGetRoom.TryGetValue(guid, out roomID);
                return roomID;
            }
            return 0;
        }

        /// <summary>
        /// 用GUID找遊戲中 房間中玩家
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>

        public RoomActor GetActorFromGuid(Guid guid)
        {
            if (_guidGetRoom.Count > 0 && _playingRoomList.Count > 0)
            {
                int roomID;
                RoomActor roomActor;
                _guidGetRoom.TryGetValue(guid, out roomID);
                _playingRoomList[roomID].TryGetValue("RoomActor1", out roomActor);

                return roomActor;
            }
            return null;
        }

        /// <summary>
        /// 用GUID 找 等待列表中的玩家
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>

        public RoomActor GetWaitActorFromGuid(Guid guid) //＊＊＊＊＊ _waitingList[1] 一定要改成很多間房
        {
            if (_waitingList.Count > 0)
            {
                RoomActor roomActor;
                _waitingList[1].TryGetValue("RoomActor1", out roomActor);

                return roomActor;
            }
            return null;
        }

        /// <summary>
        /// 用GUID 找 等待列表中的玩家 返回Bool 是否找到
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>

        public bool bGetWaitActorFromGuid(Guid guid) //＊＊＊＊＊ _waitingList[1] 一定要改成很多間房
        {
            if (_waitingList.Count > 0)
            {
                RoomActor roomActor;
                if (_waitingList[1].TryGetValue("RoomActor1", out roomActor))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///  用自己的房間ID 和 主索引 找到自己房間內的其他玩家
        /// </summary>
        /// <param name="roomID"></param>
        /// <param name="primaryID"></param>
        /// <returns></returns>

        public RoomActor GetOtherPlayer(int roomID, int primaryID)
        {
            if (_playingRoomList.Count > 0) // 是否有遊戲中房間
            {
                foreach (KeyValuePair<string, RoomActor> item in _playingRoomList[roomID]) //用RoomID找特定遊戲中房間的玩家
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

            public RoomActor(Guid guid, int PrimaryID, string Account, string Nickname, byte Age, byte Sex, string IP)
                : base(guid, PrimaryID, Account, Nickname, Age, Sex, IP)
            {
                this.joinTime = System.DateTime.Now;    // 記錄加人此房間的時間
            }
        }
    }


}
