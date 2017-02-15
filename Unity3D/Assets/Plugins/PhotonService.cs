using System;
using UnityEngine;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using MPProtocol;
using MiniJSON;
using Gansol;

/*
 * 分數驗證目前有問題 目前只把自己的分數傳給對方並更新，沒有驗證自己的分數(兩個更新分數都是)
 *
 * 
*/
public class PhotonService : MonoBehaviour, IPhotonPeerListener
{
    // ConvertUtility convertUtility = new ConvertUtility();

    protected PhotonPeer peer;		    // 連線用
    protected bool isConnected;	        // 是否已連接連線伺服器  true:已連線 false:已斷線
    protected string DebugMessage;	    // 錯誤訊息

    //委派事件 連接伺服器
    public delegate void ConnectHandler(bool ConnectStatus);
    public event ConnectHandler ConnectEvent;

    //委派事件 加入會員
    public delegate void JoinMemberHandler(bool joinStatus, string returnCode, string message);
    public event JoinMemberHandler JoinMemberEvent;

    //委派事件 登入
    public delegate void LoginHandler(bool loginStatus, string nessage, string returnCode);
    public event LoginHandler LoginEvent;

    //委派事件 重複登入、取得個人資料
    public delegate void ReLoginHandler();
    public event ReLoginHandler ReLoginEvent;
    public event ReLoginHandler GetProfileEvent;

    //委派事件 接收技能傷害
    public delegate void ApplySkillHandler(short ID);
    public event ApplySkillHandler ApplySkillMiceEvent;
    public delegate void ApplySkillXHandler(short ID);
    public event ApplySkillXHandler ApplySkillItemEvent;

    //委派事件 接收分數、對手分數
    public delegate void UpdateScoreHandler(Int16 score, Int16 energy);
    public event UpdateScoreHandler UpdateScoreEvent;


    //委派事件 接收對手分數
    public delegate void ScoreHandler(Int16 score, Int16 energy);
    public event ScoreHandler OtherScoreEvent;

    //委派事件 接收對手BOSS傷害、接收BOSS受傷
    public delegate void InjuredHandler(Int16 score, bool isMe);
    public event InjuredHandler BossInjuredEvent;

    //委派事件 接收對手BOSS技能
    public delegate void BossSkillHandler(ENUM_Skill skill);
    public event BossSkillHandler BossSkillEvent;

    //委派事件 離開房間、載入關卡
    public delegate void SceneHandler();
    public event SceneHandler LoadSceneEvent;
    public event SceneHandler GameStartEvent;
    public event SceneHandler WaitingPlayerEvent;
    public event SceneHandler ExitWaitingEvent;

    //委派事件 接收任務
    public delegate void ApplyMissionHandler(Mission mission, Int16 missionScore);
    public event ApplyMissionHandler ApplyMissionEvent;

    //委派事件 接收對方任務完成分數
    public delegate void ShowMissionScoreHandler(Int16 missionScore);
    public event ShowMissionScoreHandler OtherMissionScoreEvent;
    public event ShowMissionScoreHandler MissionCompleteEvent;

    //委派事件 GameOver
    public delegate void GameOverHandler(int score, byte exp, Int16 sliverReward, byte battleResult);
    public event GameOverHandler GameOverEvent;

    //委派事件 UpdateCurrency
    public delegate void UpdateCurrencyHandler();
    public event UpdateCurrencyHandler UpdateCurrencyEvent;

    //委派事件 ShowMessage
    public delegate void ShowMessageHandler();
    public event ShowMessageHandler ShowMessageEvent;

    //委派事件 ShowMessage
    public delegate void LoadDataHandler();
    public event LoadDataHandler LoadPlayerDataEvent;
    public event LoadDataHandler LoadStoreDataEvent;
    public event LoadDataHandler LoadPlayerItemEvent;

    public class PlayerItemData
    {
        public string itemID { get; set; }
        public string itemCount { get; set; }
        public string itemType { get; set; }
        public string isEquip { get; set; }
        public string useCount { get; set; }
    }

    public bool ServerConnected
    {
        get { return this.isConnected; }
    }

    public string ServerDebugMessage
    {
        get { return this.DebugMessage; }
    }

    //  Connect to Server
    public void Connect(string IP, int Port, string ServerName)
    {
        try
        {
            string ServerAddress = IP + ":" + Port.ToString();
            this.peer = new PhotonPeer(this, ConnectionProtocol.Udp);
            if (!this.peer.Connect(ServerAddress, ServerName))
            {
                ConnectEvent(false);
            }
        }
        catch (Exception e)
        {
            ConnectEvent(false);
            throw e;
        }
    }

    public void Disconnect()
    {
        try
        {
            if (peer != null)
            {
                this.peer.Disconnect();
            }
        }
        catch (Exception e)
        {
            throw e;
        }
    }


    public void Service()
    {
        try
        {
            if (this.peer != null)
                this.peer.Service();
        }
        catch (Exception e)
        {
            throw e;
        }
    }

    public void DebugReturn(DebugLevel level, string message)
    {
        this.DebugMessage = message;
    }

    void IPhotonPeerListener.DebugReturn(DebugLevel level, string message)
    {
        throw new NotImplementedException();
    }

    // 收到 伺服器傳來的事件時
    void IPhotonPeerListener.OnEvent(EventData eventResponse)
    {
        //Debug.Log(eventData.Code.ToString());
        switch (eventResponse.Code)
        {
            // 重複登入
            case (byte)LoginOperationCode.ReLogin:
                ReLoginEvent();
                break;

            // 配對成功 傳入 房間ID、對手資料、老鼠資料
            case (byte)MatchGameResponseCode.Match:
                Global.RoomID = (int)eventResponse.Parameters[(byte)MatchGameParameterCode.RoomID];
                Global.OtherData.Nickname = (string)eventResponse.Parameters[(byte)MatchGameParameterCode.Nickname];
                Global.OtherData.PrimaryID = (int)eventResponse.Parameters[(byte)MatchGameParameterCode.PrimaryID];
                Global.OtherData.Team = Json.Deserialize((string)eventResponse.Parameters[(byte)MatchGameParameterCode.Team]) as Dictionary<string, object>;
                Global.OtherData.RoomPlace = (string)eventResponse.Parameters[(byte)MatchGameParameterCode.RoomPlace];
                Global.nextScene = (int)Global.Scene.Battle;
                LoadSceneEvent();
                break;

            //同步開始遊戲
            case (byte)MatchGameResponseCode.SyncGameStart:
                string time = (string)eventResponse.Parameters[(byte)MatchGameParameterCode.ServerTime];
                //                Debug.Log(time);
                Global.ServerTime = DateTime.ParseExact(time, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                Debug.Log(Global.ServerTime);
                Global.GameTime = (int)eventResponse.Parameters[(byte)MatchGameParameterCode.GameTime];
                GameStartEvent();
                break;

            // 被踢出房間了
            case (byte)BattleResponseCode.KickOther:
                if (Global.isGameStart)
                {
                    Global.nextScene = (int)Global.Scene.MainGame;
                    LoadSceneEvent();
                    Global.isGameStart = false;
                    Global.isMatching = false;
                    Debug.Log("Recive Kick!" + (string)eventResponse.Parameters[(byte)BattleResponseCode.DebugMessage]);
                }
                break;

            // 他斷線 我也玩不了 離開房間
            case (byte)BattleResponseCode.Offline:
                if (Global.isGameStart)
                {
                    Global.nextScene = (int)Global.Scene.MainGame;
                    LoadSceneEvent();
                    Global.isGameStart = false;
                    Global.isMatching = false;
                    Debug.Log("Recive Offline!" + (string)eventResponse.Parameters[(byte)BattleResponseCode.DebugMessage]);
                }
                break;
            // 接收 技能傷害
            case (byte)BattleResponseCode.ApplySkillMice:
                short miceID = (short)eventResponse.Parameters[(byte)BattleParameterCode.MiceID];
                ApplySkillMiceEvent(miceID);
                Debug.Log("Recive Skill Mice!" + (string)eventResponse.Parameters[(byte)BattleResponseCode.DebugMessage]);
                Debug.Log("OpCode:" + (short)BattleResponseCode.ApplySkillMice);
                break;

            // 接收 技能傷害
            case (byte)BattleResponseCode.ApplySkillItem:
                short itemID = (short)eventResponse.Parameters[(byte)ItemParameterCode.ItemID];
                ApplySkillItemEvent(itemID);
                Debug.Log("Recive Skill Item!" + (string)eventResponse.Parameters[(byte)BattleResponseCode.DebugMessage]);
                Debug.Log("OpCode:" + (short)BattleResponseCode.ApplySkillItem);
                break;

            //取得對方分數
            case (byte)BattleResponseCode.GetScore:
                Int16 otherScore = (Int16)eventResponse.Parameters[(byte)BattleParameterCode.OtherScore];
                Int16 otherEnergy = (Int16)eventResponse.Parameters[(byte)BattleParameterCode.Energy];
                //Debug.Log("Recive otherScore!"+otherScore);
                OtherScoreEvent(otherScore, otherEnergy);
                break;

            //取得任務
            case (byte)BattleResponseCode.Mission:
                Mission mission = (Mission)eventResponse.Parameters[(byte)BattleParameterCode.Mission];
                Int16 missionScore = (Int16)eventResponse.Parameters[(byte)BattleParameterCode.MissionScore];
                ApplyMissionEvent(mission, missionScore);
                break;

            //取得對方任務分數
            case (byte)BattleResponseCode.GetMissionScore:
                Int16 otherMissionReward = (Int16)eventResponse.Parameters[(byte)BattleParameterCode.MissionReward];
                OtherMissionScoreEvent(otherMissionReward);
                Debug.Log("Recive Get Other MissionReward!" + otherMissionReward);
                break;

            //取得對方對BOSS傷害
            case (byte)BattleResponseCode.BossDamage:
                Int16 damage = (Int16)eventResponse.Parameters[(byte)BattleParameterCode.Damage];
                BossInjuredEvent(damage, false);
                //Debug.Log("GET OTHER:" + damage);
                break;
        }

    }

    //當收到Server資料時
    void IPhotonPeerListener.OnOperationResponse(OperationResponse operationResponse)
    {
        switch (operationResponse.OperationCode)
        {

            #region JoinMember 加入會員

            case (byte)JoinMemberResponseCode.JoinMember://登入
                {
                    if (operationResponse.ReturnCode == (short)ErrorCode.Ok)  // if success
                    {
                        string returnCode = (string)operationResponse.Parameters[(byte)JoinMemberParameterCode.Ret];
                        JoinMemberEvent(true, returnCode, operationResponse.DebugMessage.ToString()); // send member data to loginEvent

                    }
                    else//假如登入失敗 
                    {
                        string returnCode = (string)operationResponse.Parameters[(byte)JoinMemberParameterCode.Ret];
                        JoinMemberEvent(false, returnCode, operationResponse.DebugMessage.ToString()); // send member data to loginEvent
                    }
                    break;
                }

            #endregion

            #region Login 登入

            case (byte)LoginOperationCode.Login://登入
                {
                    //try
                    //{
                    if (operationResponse.ReturnCode == (short)ErrorCode.Ok)  // if success
                    {
                        Debug.Log("login");
                        Global.Ret = Convert.ToString(operationResponse.Parameters[(byte)LoginParameterCode.Ret]);
                        Global.Account = Convert.ToString(operationResponse.Parameters[(byte)LoginParameterCode.Account]);
                        Global.Nickname = Convert.ToString(operationResponse.Parameters[(byte)LoginParameterCode.Nickname]);
                        Global.Sex = Convert.ToByte(operationResponse.Parameters[(byte)LoginParameterCode.Sex]);
                        Global.Age = Convert.ToByte(operationResponse.Parameters[(byte)LoginParameterCode.Age]);
                        Global.PrimaryID = Convert.ToInt32(operationResponse.Parameters[(byte)LoginParameterCode.PrimaryID]);
                        Global.MemberType = (MemberType)operationResponse.Parameters[(byte)LoginParameterCode.MemberType];

                        Global.LoginStatus = true;

                        Global.photonService.LoadPlayerData(Global.Account);
                        Global.photonService.LoadMiceData();
                        Global.photonService.LoadSkillData();
                        LoginEvent(true, operationResponse.DebugMessage, operationResponse.ReturnCode.ToString()); // send member data to loginEvent

                    }
                    else//假如登入失敗 傳空值
                    {
                        Debug.Log("login fail :" + operationResponse.OperationCode);
                        DebugReturn(0, operationResponse.DebugMessage.ToString());
                        LoginEvent(false, operationResponse.DebugMessage.ToString(), operationResponse.ReturnCode.ToString()); // send error message to loginEvent
                    }

                    //}
                    //catch (Exception e)
                    //{
                    //    Debug.Log(e.Message + e.StackTrace);
                    //}
                }
                break;

            #endregion

            #region GetProfile 取得SNS個人資料

            case (byte)LoginOperationCode.GetProfile://登入
                {
                    try
                    {
                        Debug.Log("LoginOperationCode.GetProfile1");
                        GetProfileEvent();
                        Debug.Log("LoginOperationCode.GetProfile2");
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message + e.StackTrace);
                    }
                }
                break;

            #endregion


            #region ExitRoom 離開房間

            case (byte)BattleOperationCode.ExitRoom:    // 離開房間
                {
                    try
                    {
                        Global.isGameStart = false;
                        Global.nextScene = (int)Global.Scene.MainGame;
                        LoadSceneEvent();
                        Debug.Log("房間資訊：" + operationResponse.DebugMessage.ToString());
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message + e.StackTrace);
                    }
                }
                break;

            #endregion

            #region ExitWaitingRoom  離開等待房間 這裡以後可能發生錯誤 當MatchGame改變時

            case (byte)MatchGameResponseCode.ExitWaiting:// 離開等待房間
                {
                    try
                    {
                        Global.isMatching = false;
                        ExitWaitingEvent();
                        Debug.Log("房間資訊：" + operationResponse.DebugMessage.ToString());
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message + e.StackTrace);
                    }
                }
                break;

            #endregion

            #region WaitingGameStart 等待開始

            case (byte)MatchGameResponseCode.WaitingGameStart:    // 等待開始
                {
                    try
                    {
                        WaitingPlayerEvent();
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message + e.StackTrace);
                    }
                }
                break;

            #endregion


            #region LoadPlayerData 載入玩家資料

            case (byte)PlayerDataResponseCode.LoadedPlayer:   // 載入玩家資料
                {
                    try
                    {
                        if (operationResponse.ReturnCode == (short)ErrorCode.Ok)
                        {
                            //Global.Account = (string)operationResponse.Parameters[(byte)PlayerDataParameterCode.Account];
                            Global.Rank = (byte)operationResponse.Parameters[(byte)PlayerDataParameterCode.Rank];
                            Global.EXP = (byte)operationResponse.Parameters[(byte)PlayerDataParameterCode.EXP];
                            Global.MaxCombo = (Int16)operationResponse.Parameters[(byte)PlayerDataParameterCode.MaxCombo];
                            Global.MaxScore = (int)operationResponse.Parameters[(byte)PlayerDataParameterCode.MaxScore];
                            Global.SumScore = (int)operationResponse.Parameters[(byte)PlayerDataParameterCode.SumScore];
                            Global.SumLost = (Int16)operationResponse.Parameters[(byte)PlayerDataParameterCode.SumLost];
                            Global.SumKill = (int)operationResponse.Parameters[(byte)PlayerDataParameterCode.SumKill];
                            Global.SumWin = (int)operationResponse.Parameters[(byte)PlayerDataParameterCode.SumWin];
                            Global.SumBattle = (int)operationResponse.Parameters[(byte)PlayerDataParameterCode.SumBattle];
                            Global.dictMiceAll = Json.Deserialize((string)operationResponse.Parameters[(byte)PlayerDataParameterCode.MiceAll]) as Dictionary<string, object>;
                            Global.dictTeam = Json.Deserialize((string)operationResponse.Parameters[(byte)PlayerDataParameterCode.Team]) as Dictionary<string, object>;
                            Global.dictSortedItem = Json.Deserialize((string)operationResponse.Parameters[(byte)PlayerDataParameterCode.SortedItem]) as Dictionary<string, object>;
                            Global.dictFriends = Json.Deserialize((string)operationResponse.Parameters[(byte)PlayerDataParameterCode.Friend]) as Dictionary<string, object>;

                            Global.photonService.LoadCurrency(Global.Account);      // 載入玩家資料時一起載入貨幣資料

                            //Debug.Log("OperationCode:" + operationResponse.OperationCode + "  Message:" + (string)operationResponse.DebugMessage);
                            Global.isPlayerDataLoaded = true;
                            LoadPlayerDataEvent();

                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message + e.StackTrace);
                    }
                }
                break;

            #endregion

            #region LoadPlayerItem 載入玩家道具

            case (byte)PlayerDataResponseCode.LoadedItem: // 購買道具
                {
                    try
                    {
                        if (operationResponse.ReturnCode == (short)ErrorCode.Ok)
                        {
                            string playerItem = (string)operationResponse.Parameters[(byte)PlayerDataParameterCode.PlayerItem];
                            //Global.playerItem = convertUtility.Json2Array(playerItem);
                            Debug.Log("Server Response : LoadPlayerItem");

                            Global.playerItem = Json.Deserialize(playerItem) as Dictionary<string, object>;
                            LoadPlayerItemEvent();


                            #region json.net to class bak
                            /* json.net to class 
                           PlayerItemData[] playerItemData = JsonConvert.DeserializeObject<PlayerItemData[]>(playerItem);
                            string[,] data = new string[playerItemData.Length, 5];
                            for (int i = 0; i < playerItemData.Length; i++)
                            {
                                for (int j = 0; j < 5; j++)
                                {
                                    if (j == 0)
                                        data[i, j] = playerItemData[i].itemID;
                                    if (j == 1)
                                        data[i, j] = playerItemData[i].itemCount;
                                    if (j == 2)
                                        data[i, j] = playerItemData[i].itemType;
                                    if (j == 3)
                                        data[i, j] = playerItemData[i].isEquip;
                                    if (j == 4)
                                        data[i, j] = playerItemData[i].useCount;

                                    //Debug.Log(data[i, j]);
                                }
                            }
                             Global.playerItem = data;
                             */

                            #endregion

                            Global.isPlayerItemLoaded = true;
                        }
                        else
                        {
                            Debug.Log("Server DebugMessage: " + operationResponse.DebugMessage);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                    }
                }
                break;

            #endregion

            #region UpdatedItem 更新玩家道具

            case (byte)PlayerDataResponseCode.UpdatedItem: // 購買道具
                {
                    try
                    {

                        string returnCode = (string)operationResponse.Parameters[(byte)PlayerDataParameterCode.Ret];

                        if (operationResponse.ReturnCode == (short)ErrorCode.Ok)
                        {
                            Debug.Log("UpdatedItem: " + returnCode + " " + operationResponse.DebugMessage);
                            string playerItem = (string)operationResponse.Parameters[(byte)PlayerDataParameterCode.PlayerItem];
                            Global.playerItem = Json.Deserialize(playerItem) as Dictionary<string, object>;
                        }
                        else
                        {
                            Debug.Log("Server DebugMessage: " + returnCode + " " + operationResponse.DebugMessage);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                    }
                }
                break;
            #endregion

            #region LoadCurrency 載入貨幣資料

            case (byte)CurrencyResponseCode.Loaded: // 取得貨幣資料
                {
                    try
                    {
                        if (operationResponse.ReturnCode == (short)ErrorCode.Ok)
                        {
                            Global.Rice = (int)operationResponse.Parameters[(byte)CurrencyParameterCode.Rice];
                            Global.Gold = (Int16)operationResponse.Parameters[(byte)CurrencyParameterCode.Gold];

                            Global.isCurrencyLoaded = true;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message + e.StackTrace);
                    }
                }
                break;

            #endregion

            #region LoadMice 載入老鼠資料

            case (byte)MiceResponseCode.LoadMice:   // 取得老鼠資料
                {
                    if (operationResponse.ReturnCode == (short)ErrorCode.Ok)
                    {
                        string miceData = (string)operationResponse.Parameters[(byte)MiceParameterCode.MiceData];
                        Global.miceProperty = Json.Deserialize(miceData) as Dictionary<string, object>;
                        //Global.MiceProperty[] playerItemData = JsonConvert.DeserializeObject<Global.MiceProperty[]>(miceData);
                        Global.isMiceLoaded = true;
                    }
                }
                break;
            #endregion

            #region LoadSkill 載入技能資料

            case (byte)SkillResponseCode.LoadSkill:   // 取得技能資料
                {
                    if (operationResponse.ReturnCode == (short)ErrorCode.Ok)
                    {
                        string skillData = (string)operationResponse.Parameters[(byte)SkillParameterCode.SkillData];
                        Global.dictSkills = Json.Deserialize(skillData) as Dictionary<string, object>;
                        //Global.MiceProperty[] playerItemData = JsonConvert.DeserializeObject<Global.MiceProperty[]>(miceData);
                        Global.isLoadedSkill = true;
                        Debug.Log("Loaded Skills");

                        //foreach (KeyValuePair<string, object> item in Global.dictSkills)
                        //{
                        //    var nested = item.Value as Dictionary<string, object>;
                        //    Debug.Log("Key: " + item.Key);
                        //    foreach (KeyValuePair<string, object> item2 in nested)
                        //    {
                        //        Debug.Log("Key: " + item2.Key + "  Value: " + item2.Value);
                        //    }
                        //}
                    }
                }
                break;
            #endregion

            #region LoadStore 載入商店資料

            case (byte)StoreResponseCode.LoadStore:   // 取得老鼠資料
                {
                    Debug.Log("Server Response : LoadStore");
                    if (operationResponse.ReturnCode == (short)ErrorCode.Ok)
                    {
                        string storeData = (string)operationResponse.Parameters[(byte)StoreParameterCode.StoreData];


                        Global.storeItem = Json.Deserialize(storeData) as Dictionary<string, object>;
                        Global.isStoreLoaded = true;
                        LoadStoreDataEvent();
                    }
                }
                break;
            #endregion

            #region LoadItem 載入道具資料

            case (byte)ItemResponseCode.LoadItem:   // 取得道具屬性資料
                {
                    Debug.Log("Server Response : LoadItem");
                    if (operationResponse.ReturnCode == (short)ErrorCode.Ok)
                    {
                        string itemData = (string)operationResponse.Parameters[(byte)ItemParameterCode.ItemData];
                        Global.itemProperty = Json.Deserialize(itemData) as Dictionary<string, object>;
                        Global.isItemLoaded = true;
                    }
                }
                break;
            #endregion

            #region UpdateScore 更新分數 取得對方分數資料

            case (byte)BattleResponseCode.UpdateScore:// 取得對方分數資料
                {
                    try
                    {
                        if (operationResponse.ReturnCode == (short)ErrorCode.Ok)
                        {
                            Int16 score = (Int16)operationResponse.Parameters[(byte)BattleParameterCode.Score];
                            Int16 energy = (Int16)operationResponse.Parameters[(byte)BattleParameterCode.Energy];
                            UpdateScoreEvent(score, energy);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message + e.StackTrace);
                    }
                }
                break;

            #endregion

            #region UpdateScoreRate 更新分數倍率

            case (byte)BattleResponseCode.UpdatedScoreRate:// 取得對方分數資料
                {
                    try
                    {
                        if (operationResponse.ReturnCode == (short)ErrorCode.Ok)
                        {
                            float scoreRate = (float)operationResponse.Parameters[(byte)BattleParameterCode.ScoreRate];
                            Debug.Log("Updated scoreRate:" + scoreRate);
                            //UpdateScoreRateEvent(scoreRate);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message + e.StackTrace);
                    }
                }
                break;

            #endregion

            #region UpdatedEnergyRate 更新能量倍率

            case (byte)BattleResponseCode.UpdatedEnergyRate:// 取得對方分數資料
                {
                    try
                    {
                        if (operationResponse.ReturnCode == (short)ErrorCode.Ok)
                        {
                            float energyRate = (float)operationResponse.Parameters[(byte)BattleParameterCode.ScoreRate];
                            Debug.Log("Updated energyRate:" + energyRate);
                            //UpdateEnergyRateEvent(energyRate);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message + e.StackTrace);
                    }
                }
                break;

            #endregion

            #region Updated 更新玩家資料

            case (byte)PlayerDataResponseCode.UpdatedPlayer:   // 載入玩家資料
                {
                    try
                    {
                        if (operationResponse.ReturnCode == (short)ErrorCode.Ok)
                        {
                            Debug.Log("Updated Player Data.");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message + e.StackTrace);
                    }
                }
                break;

            #endregion

            #region UpdatedMice 更新老鼠資料

            case (byte)PlayerDataResponseCode.UpdatedMice:   // 載入玩家資料
                {
                    try
                    {
                        if (operationResponse.ReturnCode == (short)ErrorCode.Ok)
                        {
                            Global.dictMiceAll = Json.Deserialize((string)operationResponse.Parameters[(byte)PlayerDataParameterCode.MiceAll]) as Dictionary<string, object>;
                            Global.dictTeam = Json.Deserialize((string)operationResponse.Parameters[(byte)PlayerDataParameterCode.Team]) as Dictionary<string, object>;
                            Debug.Log("Updated Mice.");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message + e.StackTrace);
                    }
                }
                break;

            #endregion

            #region BuyItem 購買道具

            case (byte)PlayerDataResponseCode.SortedItem: // 購買道具
                {
                    try
                    {
                        if (operationResponse.ReturnCode == (short)ErrorCode.Ok)
                        {
                            Global.dictSortedItem = Json.Deserialize((string)operationResponse.Parameters[(byte)PlayerDataParameterCode.SortedItem]) as Dictionary<string, object>;
                            Debug.Log("Sorted Item !");
                        }
                        else
                        {
                            Debug.Log("Server DebugMessage: " + operationResponse.DebugMessage);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                    }
                }
                break;

            #endregion

            #region Recive Mission 接收任務

            case (byte)BattleResponseCode.Mission://取得老鼠資料
                {
                    try
                    {
                        if (operationResponse.ReturnCode == (short)ErrorCode.Ok)
                        {
                            Mission mission = (Mission)operationResponse.Parameters[(byte)BattleParameterCode.Mission];
                            Int16 missionScore = (Int16)operationResponse.Parameters[(byte)BattleParameterCode.MissionScore];
                            ApplyMissionEvent(mission, missionScore);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message + e.StackTrace);
                    }
                }
                break;

            #endregion

            #region  Mission Completed 任務完成 接收獎勵

            case (byte)BattleResponseCode.MissionCompleted://取得老鼠資料
                {
                    try
                    {
                        if (operationResponse.ReturnCode == (short)ErrorCode.Ok)
                        {
                            Int16 missionReward = (Int16)operationResponse.Parameters[(byte)BattleParameterCode.MissionReward];
                            MissionCompleteEvent(missionReward);
                            Debug.Log("RECIVE MissionCompleted ! missionReward:" + missionReward);
                        }
                        else
                        {
                            Debug.Log("RECIVE MissionCompleted ERROR !");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message + e.StackTrace);
                    }
                }
                break;

            #endregion

            #region  BossDamage 接收BOSS受傷

            case (byte)BattleResponseCode.BossDamage:// 接收BOSS受傷
                {
                    //                    Debug.Log("GET!");
                    try
                    {
                        if (operationResponse.ReturnCode == (short)ErrorCode.Ok)
                        {
                            Int16 damage = (Int16)operationResponse.Parameters[(byte)BattleParameterCode.Damage];
                            BossInjuredEvent(damage, true);
                            //                            Debug.Log("RECIVE BossDamage !");
                        }
                        else
                        {
                            Debug.Log("RECIVE BossDamage ERROR !");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message + e.StackTrace);
                    }
                }
                break;

            #endregion

            #region  BossDamage 接收BOSS技能攻擊

            case (byte)BattleResponseCode.SkillBoss:// 接收BOSS受傷
                {
                    //                    Debug.Log("GET!");
                    try
                    {
                        if (operationResponse.ReturnCode == (short)ErrorCode.Ok)
                        {
                            ENUM_Skill skill = (ENUM_Skill)operationResponse.Parameters[(byte)SkillParameterCode.SkillID];
                            BossSkillEvent(skill);
                            //                            Debug.Log("RECIVE BossDamage !");
                        }
                        else
                        {
                            Debug.Log("RECIVE Boss Skill ERROR !");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message + e.StackTrace);
                    }
                }
                break;

            #endregion

            #region BuyItem 購買道具

            case (byte)StoreResponseCode.BuyItem: // 購買道具
                {
                    try
                    {
                        if (operationResponse.ReturnCode == (short)ErrorCode.Ok)
                        {
                            Global.Rice = (int)operationResponse.Parameters[(byte)CurrencyParameterCode.Rice];
                            Global.Gold = (Int16)operationResponse.Parameters[(byte)CurrencyParameterCode.Gold];
                            Global.dictMiceAll = Json.Deserialize((string)operationResponse.Parameters[(byte)PlayerDataParameterCode.MiceAll]) as Dictionary<string, object>; ;
                            UpdateCurrencyEvent();
                            ShowMessageEvent();
                        }
                        else
                        {
                            Debug.Log("Server DebugMessage: " + operationResponse.DebugMessage);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                    }
                }
                break;

            #endregion


            #region  GameOver 接收遊戲結束時資料
            case (byte)BattleResponseCode.GameOver:
                {
                    if (operationResponse.ReturnCode == (short)ErrorCode.Ok)
                    {
                        int score = (int)operationResponse.Parameters[(byte)BattleParameterCode.Score];     // 這是GameScore不含扣分
                        byte exp = (byte)operationResponse.Parameters[(byte)BattleParameterCode.EXPReward];
                        Int16 sliverReward = (Int16)operationResponse.Parameters[(byte)BattleParameterCode.SliverReward];
                        byte battleResult = (byte)operationResponse.Parameters[(byte)BattleParameterCode.BattleResult];

                        Global.MaxCombo = (Int16)operationResponse.Parameters[(byte)PlayerDataParameterCode.MaxCombo];
                        Global.MaxScore = (int)operationResponse.Parameters[(byte)PlayerDataParameterCode.MaxScore];
                        Global.SumLost = (Int16)operationResponse.Parameters[(byte)PlayerDataParameterCode.SumLost];
                        Global.SumKill = (int)operationResponse.Parameters[(byte)PlayerDataParameterCode.SumKill];
                        Global.dictSortedItem = Json.Deserialize((string)operationResponse.Parameters[(byte)PlayerDataParameterCode.SortedItem]) as Dictionary<string, object>; ;
                        Global.Rank = (byte)operationResponse.Parameters[(byte)PlayerDataParameterCode.Rank];

                        GameOverEvent(score, exp, sliverReward, battleResult);
                        Debug.Log("GameOver:" + (int)operationResponse.Parameters[(byte)BattleParameterCode.Score]);
                        Debug.Log("RECIVE GameOver !");
                    }
                    else
                    {
                        Debug.Log("RECIVE GameOver ERROR !" + " 錯誤碼:" + operationResponse.ReturnCode + "  錯誤訊息:" + operationResponse.DebugMessage);
                    }
                    break;
                }
            #endregion

            default:
                Debug.LogError("The given key not found! " + operationResponse.OperationCode);
                break;
        }
    }

    // 當連線狀態改變時
    void IPhotonPeerListener.OnStatusChanged(StatusCode statusCode)
    {
        switch (statusCode)
        {
            case StatusCode.Connect:
                this.peer.EstablishEncryption(); //  連線後開啟加密功能
                break;
            case StatusCode.Disconnect:
                this.peer = null;
                this.isConnected = false;
                ConnectEvent(false);
                break;
            case StatusCode.EncryptionEstablished: // 加密啟動成功後
                this.isConnected = true;
                ConnectEvent(true); // 此時才算是完成連線動作
                break;
        }
    }

    #region Login 登入會員
    /// <summary>
    /// 登入會員 傳送資料到Server
    /// </summary>
    public void Login(string Account, string Password, MemberType memberType)
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> { 
                             { (byte)LoginParameterCode.Account,Account },   { (byte)LoginParameterCode.Password, Password },   { (byte)LoginParameterCode.MemberType, memberType }
                        };

            this.peer.OpCustom((byte)LoginOperationCode.Login, parameter, true, 0, true);
        }
        catch (Exception e)
        {
            throw e;
        }

    }
    #endregion

    #region JoinMember 加入會員
    /// <summary>
    /// 加入會員 
    /// </summary>
    public void JoinMember(string Email, string Password, string Nickname, byte Age, byte Sex, string IP, MemberType memberType)
    {
        try
        {
            byte age = Convert.ToByte(Age);
            byte sex = Convert.ToByte(Sex);
            char[] splitText = { '@' };
            string[] Account = Email.Split(splitText);

            Dictionary<byte, object> parameter = new Dictionary<byte, object> { 
                           { (byte)JoinMemberParameterCode.Email,Email },  { (byte)JoinMemberParameterCode.Account,Account[0] },   { (byte)JoinMemberParameterCode.Password, Password }  ,{ (byte)JoinMemberParameterCode.Nickname, Nickname }  ,
                             { (byte)JoinMemberParameterCode.Age, age }  ,{ (byte)JoinMemberParameterCode.Sex, sex }  , { (byte)JoinMemberParameterCode.JoinDate, DateTime.Now.ToString()}, { (byte)JoinMemberParameterCode.IP, IP}, { (byte)JoinMemberParameterCode.MemberType, memberType }  
                        };

            this.peer.OpCustom((byte)JoinMemberOperationCode.JoinMember, parameter, true, 0, true); // operationCode is 21
        }
        catch (Exception e)
        {
            throw e;
        }

    }
    #endregion

    #region ExitRoom 離開房間
    /// <summary>
    /// 離開房間
    /// </summary>
    public void ExitRoom()
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                 {(byte)BattleParameterCode.RoomID,Global.RoomID},{(byte)BattleParameterCode.PrimaryID,Global.PrimaryID}
            };

            this.peer.OpCustom((byte)BattleOperationCode.ExitRoom, parameter, true, 0, true);
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region CheckStatus 檢查對手線上狀態
    /// <summary>
    /// 檢查對手線上狀態
    /// </summary>
    public void CheckStatus()
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                 {(byte)BattleParameterCode.RoomID,Global.RoomID},{(byte)BattleParameterCode.PrimaryID,Global.PrimaryID} 
            };

            this.peer.OpCustom((byte)BattleOperationCode.CheckStatus, parameter, true, 0, true);
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region KickOther 把對手踢了
    /// <summary>
    /// 把對手踢了
    /// </summary>
    public void KickOther()
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                 {(byte)BattleParameterCode.RoomID,Global.RoomID},{(byte)BattleParameterCode.PrimaryID,Global.PrimaryID}
            };

            this.peer.OpCustom((byte)BattleOperationCode.KickOther, parameter, true, 0, true); // operationCode is RoomSpeak
            Debug.Log("Send Kick!");
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion


    #region MatchGame 開始配對遊戲
    /// <summary>
    /// 開始配對遊戲
    /// </summary>
    public void MatchGame(int PrimaryID, Dictionary<string, object> team)
    {
        string dictTeam = Json.Serialize(team);
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                 { (byte)MatchGameParameterCode.PrimaryID,PrimaryID},{ (byte)MatchGameParameterCode.Team,dictTeam}
            };

            this.peer.OpCustom((byte)MatchGameOperationCode.MatchGame, parameter, true, 0, true);
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region SendSkillMice 傳送老鼠技能攻擊
    /// <summary>
    /// 傳送技能攻擊 傳送資料到Server
    /// </summary>
    public void SendSkillMice(short miceID) //攻擊測試
    {
        Debug.Log("IN Services SendSkill:" + miceID);
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                 { (byte)BattleParameterCode.MiceID,miceID } ,{ (byte)BattleParameterCode.RoomID,Global.RoomID },{ (byte)BattleParameterCode.PrimaryID,Global.PrimaryID }
            };

            this.peer.OpCustom((byte)BattleOperationCode.SendSkillMice, parameter, true, 0, true);
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region SendSkillItem 傳送道具技能攻擊
    /// <summary>
    /// 傳送技能攻擊 傳送資料到Server
    /// </summary>
    public void SendSkillItem(short itemID, short skillType) //攻擊測試
    {
        Debug.Log("IN Services SendSkillItem:" + itemID);
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                 { (byte)ItemParameterCode.ItemID,itemID } ,{ (byte)SkillParameterCode.SkillType,skillType } ,{ (byte)BattleParameterCode.RoomID,Global.RoomID },{ (byte)BattleParameterCode.PrimaryID,Global.PrimaryID }
            };

            this.peer.OpCustom((byte)BattleOperationCode.SendSkillItem, parameter, true, 0, true);
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region ExitWaitingRoom 離開等待房間
    /// <summary>
    /// 離開等待房間 傳送資料到Server
    /// </summary>
    public void ExitWaitingRoom()
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object>();
            this.peer.OpCustom((byte)MatchGameOperationCode.ExitWaiting, parameter, true, 0, true); // operationCode is RoomSpeak
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region LoadPlayerData 載入玩家資料
    /// <summary>
    /// 載入玩家資料 傳送資料到Server
    /// </summary>
    public void LoadPlayerData(string account)
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)PlayerDataParameterCode.Account, account } };
            this.peer.OpCustom((byte)PlayerDataOperationCode.LoadPlayer, parameter, true, 0, true);
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region LoadPlayerItem 載入玩家道具資料
    /// <summary>
    /// 載入玩家道具資料
    /// </summary>
    public void LoadPlayerItem(string account)
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)PlayerDataParameterCode.Account, account } };
            this.peer.OpCustom((byte)PlayerDataOperationCode.LoadItem, parameter, true, 0, true);
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region UpdatePlayerData 更新玩家資料
    /// <summary>
    /// 更新玩家資料
    /// </summary>                                              
    public void UpdatePlayerData(string account, byte rank, byte exp, Int16 maxCombo, int maxScore, int sumScore, Int16 sumLost, int sumKill, Dictionary<string, object> item, Dictionary<string, object> miceAll, Dictionary<string, object> team, Dictionary<string, object> friend)
    {
        string dictItem = Json.Serialize(item);
        string dictMiceAll = Json.Serialize(miceAll);
        string dictTeam = Json.Serialize(team);
        string dictFriend = Json.Serialize(friend);

        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
            { (byte)PlayerDataParameterCode.Account, account }, { (byte)PlayerDataParameterCode.Rank, rank }, { (byte)PlayerDataParameterCode.EXP, exp },
             { (byte)PlayerDataParameterCode.MaxCombo, maxCombo }, { (byte)PlayerDataParameterCode.MaxScore, maxScore }, { (byte)PlayerDataParameterCode.SumScore, sumScore },
             { (byte)PlayerDataParameterCode.SumLost, sumLost },{ (byte)PlayerDataParameterCode.SumKill, sumKill },{ (byte)PlayerDataParameterCode.SortedItem, dictItem },
             { (byte)PlayerDataParameterCode.MiceAll, dictMiceAll }, { (byte)PlayerDataParameterCode.Team, dictTeam },
             { (byte)PlayerDataParameterCode.Friend, dictFriend }};
            this.peer.OpCustom((byte)PlayerDataOperationCode.UpdatePlayer, parameter, true, 0, true); // operationCode is RoomSpeak
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region UpdatePlayerItem 更新玩家道具資料(裝備狀態)
    /// <summary>
    /// 更新玩家道具資料(裝備狀態)
    /// </summary>         
    /// <param name="itemID">物品ID</param>
    /// <param name="isEquip">裝備狀態</param>
    public void UpdatePlayerItem(Int16 itemID, bool isEquip)
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
            { (byte)PlayerDataParameterCode.Account, Global.Account },{ (byte)PlayerDataParameterCode.SortedItem, itemID},{ (byte)PlayerDataParameterCode.Equip, isEquip}};
            this.peer.OpCustom((byte)PlayerDataOperationCode.UpdateItem, parameter, true, 0, true); // operationCode is RoomSpeak
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region UpdatePlayerItem 更新玩家道具資料(物品數量)
    /// <summary>
    /// 更新玩家道具資料(物品數量)
    /// 須使用多層字典，編號為ItemID
    /// </summary>             
    /// <param name="jsonString">JsonString 1:itemCount , 2:useCount</param>
    public void UpdatePlayerItem(string jsonString)
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                { (byte)PlayerDataParameterCode.Account, Global.Account }, { (byte)PlayerDataParameterCode.UseCount, jsonString}
            };

            this.peer.OpCustom((byte)PlayerDataOperationCode.UpdateItem, parameter, true, 0, true); // operationCode is RoomSpeak
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion


    #region SortPlayerItem 更新玩家道具資料(道具排序)
    /// <summary>
    /// 更新玩家道具資料(物品數量)
    /// 須使用多層字典，編號為ItemID
    /// </summary>             
    /// <param name="dictItem"></param>
    public void SortPlayerItem(Dictionary<string, object> dictItem)
    {
        string jsonString = Json.Serialize(dictItem);
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                { (byte)PlayerDataParameterCode.Account, Global.Account }, { (byte)PlayerDataParameterCode.SortedItem, jsonString}
            };

            this.peer.OpCustom((byte)PlayerDataOperationCode.SortItem, parameter, true, 0, true); // operationCode is RoomSpeak
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region UpdateMiceData 更新玩家資料
    /// <summary>
    /// 更新老鼠資料
    /// </summary>
    public void UpdateMiceData(string account, Dictionary<string, object> miceAll, Dictionary<string, object> team)
    {

        string dictMice = Json.Serialize(miceAll);
        string dictTeam = Json.Serialize(team);
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
            { (byte)PlayerDataParameterCode.Account, account },  { (byte)PlayerDataParameterCode.MiceAll, dictMice },
            { (byte)PlayerDataParameterCode.Team, dictTeam },
             };
            this.peer.OpCustom((byte)PlayerDataOperationCode.UpdateMice, parameter, true, 0, true); // operationCode is RoomSpeak
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region LoadCurrency 載入貨幣資料
    /// <summary>
    /// 載入貨幣資料
    /// </summary>
    public void LoadCurrency(string account)
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)CurrencyParameterCode.Account, account } };
            this.peer.OpCustom((byte)CurrencyOperationCode.Load, parameter, true, 0, true);
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region LoadRice 載入遊戲幣資料
    /// <summary>
    /// 載入遊戲幣資料
    /// </summary>
    public void LoadRice(string account)
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)CurrencyParameterCode.Account, account } };
            this.peer.OpCustom((byte)CurrencyOperationCode.LoadRice, parameter, true, 0, true); // operationCode is RoomSpeak
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region LoadGold 載入金幣資料
    /// <summary>
    /// 載入金幣資料
    /// </summary>
    public void LoadGold(string account)
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> { { (byte)CurrencyParameterCode.Account, account } };
            this.peer.OpCustom((byte)CurrencyOperationCode.LoadGold, parameter, true, 0, true); // operationCode is RoomSpeak
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region LoadMiceData 載入老鼠資料
    /// <summary>
    /// 載入老鼠資料
    /// </summary>
    public void LoadMiceData()
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
            this.peer.OpCustom((byte)MiceOperationCode.LoadMice, parameter, true, 0, false); // operationCode is RoomSpeak
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region LoadMiceData 載入老鼠資料
    /// <summary>
    /// 載入老鼠資料
    /// </summary>
    public void LoadSkillData()
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
            this.peer.OpCustom((byte)SkillOperationCode.LoadSkill, parameter, true, 0, false); // operationCode is RoomSpeak
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region LoadStoreData 載入老鼠資料
    /// <summary>
    /// 載入老鼠資料
    /// </summary>
    public void LoadStoreData()
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
            this.peer.OpCustom((byte)StoreOperationCode.LoadStore, parameter, true, 0, false); // operationCode is RoomSpeak
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region LoadItemData 載入道具資料
    /// <summary>
    /// 載入道具資料
    /// </summary>
    public void LoadItemData()
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> { };
            this.peer.OpCustom((byte)ItemOperationCode.LoadItem, parameter, true, 0, false); // operationCode is RoomSpeak
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion



    #region UpdateScore 更新分數 地鼠用
    /// <summary>
    /// 更新分數 地鼠用
    /// </summary>
    public void UpdateScore(short miceID, float time)
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> { 
            { (byte)BattleParameterCode.PrimaryID, Global.PrimaryID }, { (byte)BattleParameterCode.RoomID, Global.RoomID },
            { (byte)BattleParameterCode.MiceID, miceID }, { (byte)BattleParameterCode.Time, time }
            };

            this.peer.OpCustom((byte)BattleOperationCode.UpdateScore, parameter, true, 0, false); // operationCode is RoomSpeak
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region SendMission 傳送任務
    /// <summary>
    /// 傳送技能攻擊 傳送資料到Server
    /// </summary>
    public void SendMission(byte mission, float missionRate) //攻擊測試
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
                 { (byte)BattleParameterCode.Mission,mission } , { (byte)BattleParameterCode.MissionRate,missionRate } ,{ (byte)BattleParameterCode.RoomID,Global.RoomID },{ (byte)BattleParameterCode.PrimaryID,Global.PrimaryID }
            };

            this.peer.OpCustom((byte)BattleOperationCode.Mission, parameter, true, 0, false);
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region MissionCompleted 任務完成 要求獎勵
    /// <summary>
    /// 任務完成 接收獎勵
    /// </summary>
    /// <param name="mission">任務名稱</param>
    /// <param name="missionRate">任務倍率</param>
    /// <param name="customValue">自訂參數 無填0</param>
    public void MissionCompleted(byte mission, float missionRate, Int16 customValue, string customString)
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
            { (byte)BattleParameterCode.PrimaryID, Global.PrimaryID }, { (byte)BattleParameterCode.RoomID, Global.RoomID },
            { (byte)BattleParameterCode.Mission, mission }, { (byte)BattleParameterCode.MissionRate, missionRate } , { (byte)BattleParameterCode.CustomValue, customValue }, { (byte)BattleParameterCode.CustomString, customString }};
            this.peer.OpCustom((byte)BattleOperationCode.MissionCompleted, parameter, true, 0, false); // operationCode is RoomSpeak
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region SyncGameStart 同步開始遊戲
    /// <summary>
    /// 同步開始遊戲
    /// </summary>
    /// <param name="roomID">房間ID</param>
    /// <param name="primaryID">主索引</param>
    public void SyncGameStart()
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
            { (byte)MatchGameParameterCode.PrimaryID, Global.PrimaryID }, { (byte)MatchGameParameterCode.RoomID, Global.RoomID },
            };

            this.peer.OpCustom((byte)MatchGameOperationCode.SyncGameStart, parameter, true, 0, false); // operationCode is RoomSpeak
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region BossDamage 對BOSS造成傷害
    /// <summary>
    /// 對BOSS造成傷害
    /// </summary>
    /// <param name="roomID">房間ID</param>
    /// <param name="primaryID">主索引</param>
    public void BossDamage(Int16 damage)
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
            { (byte)BattleParameterCode.PrimaryID, Global.PrimaryID }, { (byte)BattleParameterCode.RoomID, Global.RoomID },{ (byte)BattleParameterCode.Damage,damage },
            };

            this.peer.OpCustom((byte)BattleOperationCode.BossDamage, parameter, true, 0, true); // operationCode is RoomSpeak
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region GameOver 遊戲結束 回傳資料計算結果

    /// <summary>
    /// GameOver 遊戲結束 回傳資料計算結果
    /// </summary>
    /// <param name="gameScore">遊戲中獲得分數</param>
    /// <param name="maxCombo">最大Combo</param>
    /// <param name="killMice">清除的老鼠</param>
    /// <param name="lostMice">沒打到的老鼠</param>
    /// <param name="itemAmount">使用的道具數量</param>
    public void GameOver(Int16 gameScore, Int16 otherScore, Int16 gameTime, Int16 maxCombo, int killMice, Int16 lostMice)
    {
        try
        {
            Debug.Log("Send GameOver:" + Global.dictSortedItem);
            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
            { (byte)PlayerDataParameterCode.Account, Global.Account },
            { (byte)BattleParameterCode.Score, gameScore },{ (byte)BattleParameterCode.OtherScore, otherScore },
            { (byte)BattleParameterCode.Time, gameTime },{ (byte)PlayerDataParameterCode.MaxCombo, Global.MaxCombo },
            { (byte)PlayerDataParameterCode.SumKill, killMice },{ (byte)PlayerDataParameterCode.SumLost, lostMice },
            { (byte)PlayerDataParameterCode.SortedItem, Json.Serialize(Global.dictSortedItem) },
            };

            this.peer.OpCustom((byte)BattleOperationCode.GameOver, parameter, true, 0, true); // operationCode is RoomSpeak
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region BuyItem 購買商品
    /// <summary>
    /// 購買商品
    /// </summary>
    public void BuyItem(string account, string[] goods)
    {
        Debug.Log(goods[0] + " " + goods[1] + " " + goods[2] + " " + goods[3]);
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
            { (byte)PlayerDataParameterCode.Account, account },{ (byte)PlayerDataParameterCode.MiceAll, Json.Serialize(Global.dictMiceAll) },
            { (byte)StoreParameterCode.ItemID,Int16.Parse(goods[0])},{ (byte)StoreParameterCode.ItemName,goods[1]},{ (byte)StoreParameterCode.ItemType,byte.Parse(goods[2])},{ (byte)StoreParameterCode.CurrencyType,byte.Parse(goods[3])},
            { (byte)StoreParameterCode.BuyCount,Int16.Parse(goods[4])}};
            this.peer.OpCustom((byte)StoreOperationCode.BuyItem, parameter, true, 0, true); // operationCode is RoomSpeak
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region UpdateScoreRate 更新分數倍率
    /// <summary>
    /// 更新分數倍率
    /// </summary>
    /// <param name="rate">倍率</param>
    public void UpdateScoreRate(ENUM_ScoreRate rate)
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
            { (byte)PlayerDataParameterCode.Account, Global.Account}, { (byte)BattleParameterCode.ScoreRate, rate}};
            this.peer.OpCustom((byte)BattleOperationCode.UpdateScoreRate, parameter, true, 0, true); // operationCode is RoomSpeak
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region UpdateScoreRate 更新分數倍率
    /// <summary>
    /// 更新分數倍率
    /// </summary>
    /// <param name="rate">倍率</param>
    public void UpdateEnergyRate(ENUM_ScoreRate rate)
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
            { (byte)PlayerDataParameterCode.Account, Global.Account}, { (byte)BattleParameterCode.ScoreRate, rate}};
            this.peer.OpCustom((byte)BattleOperationCode.UpdateEnergyRate, parameter, true, 0, true); // operationCode is RoomSpeak
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region SendSkillBoss 傳送技能老鼠技能
    /// <summary>
    /// 傳送技能老鼠技能
    /// </summary>
    /// <param name="rate">倍率</param>
    public void SendBossSkill(ENUM_Skill skill)
    {
        try
        {
            Dictionary<byte, object> parameter = new Dictionary<byte, object> {
            { (byte)BattleParameterCode.PrimaryID, Global.PrimaryID},{ (byte)BattleParameterCode.RoomID, Global.RoomID}, { (byte)SkillParameterCode.SkillID, skill}};
            this.peer.OpCustom((byte)StoreOperationCode.BuyItem, parameter, true, 0, true); // operationCode is RoomSpeak
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    #endregion
}
