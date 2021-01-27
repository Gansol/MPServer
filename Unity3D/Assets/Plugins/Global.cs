using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using MPProtocol;
using System.Linq;
using System.Diagnostics;

public static class Global
{
    public static string serverIP = "180.218.166.204";
    public static string serverPath = "http://180.218.166.204:58767/MicePowBETA";//Server路徑

    //Android or iOS or Win 伺服器中的 檔案列表路徑
    public static readonly string serverListPath = serverPath +
#if UNITY_ANDROID
 "/AndroidList/";//  "/AndroidList/";   
#elif UNITY_IPHONE
    "/iOSList/";
#else
 string.Empty;
#endif

    //Android or iOS or Win 伺服器中的 檔案路徑
    public static readonly string assetBundlesPath = serverPath +
#if UNITY_ANDROID
 "/AndroidBundles/"; // "/AndroidBundles/";
#elif UNITY_IPHONE
    "/iOSBundles/";
#else
 string.Empty;
#endif

    public static string perfabFolder =
#if UNITY_ANDROID
 "/AssetBundles/";
#elif UNITY_IPHONE || UNITY_IOS
    "/AssetBundles/";
#elif UNITY_WEBPLAYER
    "/AssetBundles/";
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
    "/AssetBundles/";
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        "/AssetBundles/";
#endif

    public static string exportFolder =
#if UNITY_ANDROID
 "/_AssetBundles/Android/";
#elif UNITY_IPHONE  || UNITY_IOS
    "/_AssetBundles/iOS/";
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    "/_AssetBundles/STANDALONE_WIN/";
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
    "/_AssetBundles/STANDALONE_OSX/";
#endif

    public static string dataPath =
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
 Application.dataPath + "/StreamingAssets";
#elif UNITY_ANDROID
    "jar:file://" + Application.dataPath + "!/assets/";
#elif UNITY_IPHONE  || UNITY_IOS
    Application.dataPath + "/Raw";
#endif

    public static readonly string defaultVersion = "{\"vision4\": \"v1.0.4\"}";
    public static PhotonService photonService = new PhotonService();    // Photon ServerClient服務

    public delegate void ShowMessageHandler(string message, string MessageBoxType, int prevMask);
    public static event ShowMessageHandler ShowMessageEvent;

    public static readonly string patchFile = "patch.txt"; // 資源版本
    public static readonly string itemListFile = "ItemList.json";          // 道具列表
    public static readonly string visionListFile = "VersionList.json";      // 版本列表
    public static readonly string downloadListFile = "DownloadList.json";  // 下載列表
    public static readonly string fullPackageFile = "FullPackageList.json";// 完整下載列表
    public static readonly string bundleVersionFile = "BundleVersion.json"; // 資源版本
    public static readonly string MusicsFile = "MusicsList.json"; // 資源版本
    public static readonly string SoundsFile = "SoundsList.json"; // 資源版本

    public static bool isCheckBundle = false;       // 是否檢查資源
    public static bool isNewlyVision = true;        // 是否為新版本
    public static bool isVisionDownload = false;    // 是否開始下載版本列表
    public static bool isCompleted = false;         // 是否下載完成
    public static bool isReplaced = false;          // 是否取代列表完成
    public static bool isJoinMember = true;         // 是否加入會員

    public static bool isPlayerDataLoaded = false;  // 是否載入玩家資料
    public static bool isPlayerItemLoaded = false;  // 是否載入玩家資料
    public static bool isCurrencyLoaded = false;    // 是否載入玩家資料
    public static bool isMiceLoaded = false;        // 是否載入老鼠資料
    public static bool isLoadedSkill = false;       // 是否載入技能資料
    public static bool isStoreLoaded = false;        // 是否載入老鼠資料
    public static bool isItemLoaded = false;        // 是否載入老鼠資料
    public static bool isArmorLoaded = false;        // 是否載入老鼠資料
    public static bool isLoaded = false;            // 是否載入場景

    public static bool connStatus = false;          // 連線狀態
    public static bool LoginStatus = false;	        // true = 已登入,  false = 未登入
    public static bool BattleStatus = false;        // 是否開始對戰
    public static bool isMatching = false;          // 是否配對成功
    public static bool isFriendMatching = false;    // 是否配對成功
    public static bool isExitingRoom = false;       // 是否離開房間
    public static bool isSyncGame = false;          // 是否載入場景
    public static bool isGameStart = false;         // 是否開始遊戲
    public static bool isApplySkill = false;        // 是否受到技能傷害
    public static bool spawnFlag = false;           // 是否產生完成
    //public static bool isMissionCompleted = false;  // 是否任務完成
    public static bool missionFlag = true;         // 是否執行任務
    public static bool exitingGame = false;         // 是否執行任務
    public static string prevScene = Scene.LogoScene;  // 上一個場景
    public static string nextScene = Scene.BundleCheck;  // 要被載入的場景
    public static int extIconLength = 4;            // 圖片名稱(副檔)長度
    public static int maxConnTimes = 5;                         // 重新連限次數
    public static DateTime ServerTime = System.DateTime.Now;    // 伺服器時間
    public static int GameTime = 150;        // 遊戲時間
    public static int WaitTime = 2;        // 配對等待時間
    public static int OnlineActor = 0;        // 配對等待時間

    public static string PlayerImage = "";  // 玩家圖片
    public static string Ret = "";          // 回傳值
    public static int PrimaryID = 0;       // 主索引
    public static string Account = "";      // 帳號
    public static string Email = "";
    public static string Hash = "";           // ****欄位
    public static string Nickname = "";     // 暱稱

    public static string IconSuffix = "icon_";  // ICON附檔名
    public static string EffectSuffix = "effect_";  // ICON附檔名
    public static string PanelPath = "panel/";  // panel路徑
    public static string PanelUniquePath = "panel/unique/";  // panel路徑
    public static string MicePath = "mice/";  // mice路徑
    public static string CreaturePath = "creature/unique/";  // mice路徑
    public static string MiceIconUniquePath = "miceicon/unique/";  // miceicon路徑
    public static string ItemIconUniquePath = "itemicon/unique/";  // itemicon路徑
    public static string MusicsPath = "musics/bgm/";  // miceicon路徑
    public static string SoundsPath = "musics/se/";  // itemicon路徑
    public static string EffectsUniquePath = "effects/unique/";  // effects路徑
    public static string GashaponUniquePath = "gashapon/unique/";  // gashapon路徑
    public static string InvItemAssetName = "invitem";
    public static string StoreItemAssetName = "item";
    public static string PurchaseItemAssetName = "purchaseitem"; 

    public static int RoomID = -1;          // 房間ID
    public static byte Sex = 0;              // 性別
    public static byte Age = 0;              // 年齡
    public static MemberType MemberType;     // 
    public static int Rice = 0;             // 遊戲幣
    public static Int16 Gold = 0;           // 金幣
    public static Int16 Bonus = 0;           // 紅利

    public static byte Rank = 0;            // 等級
    public static short Exp = 0;             // 經驗
    public static Int16 MaxCombo = 0;       // 最大連擊
    public static int MaxScore = 0;         // 最高分
    public static int SumScore = 0;         // 總分
    public static int SumLost = 0;          // 總漏掉的老鼠
    public static int SumKill = 0;          // 總除掉的老鼠
    public static int SumWin = 0;          // 總勝場
    public static int SumBattle = 0;          // 總場次
    public static string gameVersion = "";    // 資產版本
    public static uint bundleVersion = 1;    // 資產版本
    public static int MenuObjetDepth = 10000; // 主選單物件深度

    public static List<string> dictFriends = new List<string>();                                                // 好友列表 List資料
    public static List<string> GashaponItemList = new List<string>();                                           // 轉到的轉蛋列表 List資料
    public static Dictionary<string, object> dictSortedItem = new Dictionary<string, object>();                 // 全部老鼠 JSON資料;         // 漏掉的老鼠
    public static Dictionary<string, object> dictMiceAll = new Dictionary<string, object>();                    // 全部老鼠 JSON資料
    public static Dictionary<string, object> dictTeam = new Dictionary<string, object>();                       // 隊伍老鼠 JSON資料
    
    public static Dictionary<string, object> dictOnlineFriendsDetail = new Dictionary<string, object>();          // 好友詳細列表 JSON資料
    public static Dictionary<string, object> dictOnlineFriendsState = new Dictionary<string, object>();          // 好友詳細列表 JSON資料
    public static Dictionary<string, object> dictSkills = new Dictionary<string, object>();                     // 技能列表 JSON資料
    public static Dictionary<string, object> miceProperty = new Dictionary<string, object>();                   // 老鼠屬性資料 
    public static Dictionary<string, object> itemProperty = new Dictionary<string, object>();                   // 道具屬性資料 
    public static Dictionary<string, object> storeItem = new Dictionary<string, object>();                      // 商店屬性資料 
    public static Dictionary<string, object> gashaponItem = new Dictionary<string, object>();                      // 轉蛋屬性資料 
    public static Dictionary<string, object> playerItem = new Dictionary<string, object>();                     // 玩家道具屬性資料 
    public static Dictionary<string, object> purchaseItem = new Dictionary<string, object>();                     //商城道具屬性資料 
   

    public static Dictionary<string, GameObject> dictLoadedScene = new Dictionary<string, GameObject>();
  //  public static Dictionary<Transform, GameObject> dictBattleMiceRefs = new Dictionary<Transform, GameObject>();   // <hole,mice>
  //  public static Dictionary<Transform, GameObject> dictSkillMice = new Dictionary<Transform, GameObject>();
    //public static Dictionary<Transform, GameObject> dictBossMice = new Dictionary<Transform, GameObject>();

    public static string ReturnMessage = "";       // 回傳訊息

    public static string ext = ".unity3d";       // AB副檔名


    public delegate void ExitGameHandler();
    public static event ExitGameHandler ExitGameEvent;

    public class MessageBoxType
    {
        public const string NonChk = "MsgBox_NonChk";
        public const string Yes = "MsgBox_Yes";
        public const string YesNo = "MsgBox_YesNo";
        public const string SystemCrash = "MsgBox_Yes";
    }

    /// <summary>
    /// 顯示訊息視窗
    /// </summary>
    /// <param name="message">訊息</param>
    /// <param name="MessageBoxType">訊息視窗類型</param>
    /// <param name="prevMask">返回事件遮罩級數</param>
    public static void ShowMessage(string message, string MessageBoxType, int prevMask)
    {
        ShowMessageEvent(message, MessageBoxType, prevMask);
    }

    public static void ExitGame()
    {
        exitingGame = true;
        ExitGameEvent();
    }

    public static class OpponentData
    {
        public static int PrimaryID = 0;        // 主索引
        public static string Account = "";        // 主索引
        public static string Nickname = "";     // 暱稱
        public static byte Sex = 0;              // 性別
        public static Dictionary<string, object> Team = new Dictionary<string, object>();         // 隊伍老鼠 JSON資料
        public static string RoomPlace = "";    // 另一位玩家的房間位置
        public static string Image = "";
    }

    //public static int MiceCount = 0;        // 目前 對戰老鼠數量 要移到BattleData  FUCK 錯誤



    /*
    public class MiceProperty
    {
        int miceID { get; set; }
        string miceName { get; set; }
        float eatingRate { get; set; }
        float miceSpeed { get; set; }
        int eatFull { get; set; }
        int skill { get; set; }
        int hp { get; set; }
        int miceCost { get; set; }
    }
    */
    public class Scene
    {
        public const string LogoScene = "LogoScene";
        public const string BundleCheck = "BundleCheck";
        public const string MainGame = "MainGame";
        public const string Battle = "Battle";
        public const string LoadScene = "LoadScene";



        public const string MainGameAsset = "menuui";
        public const string BattleAsset = "gameui";

    }

    public enum UILayer
    {
        Nothing = 0,
        Default = 1,
        Battle = 8,
        HUD = 9,
        Store = 10,
        Player = 11,
        ItemInfo = 12,
        Message = 13,
    }

    public enum Camrea
    {
        MainCamera,
        HUDCamera,
    }

    public static void RenameKey<TKey, TValue>(this IDictionary<TKey, TValue> dic,
                                  TKey fromKey, TKey toKey)
    {
        TValue value = dic[fromKey];
        dic.Remove(fromKey);
        dic[toKey] = value;
    }

    /// <summary>
    /// 交換字典物件
    /// </summary>
    /// <param name="vaule1"></param>
    /// <param name="value2"></param>
    /// <param name="dict"></param>
    public static void SwapDictKeyByValue(string vaule1, string value2, Dictionary<string, object> dict)
    {
        string myKey = "", otherKey = "";
        //object myValue = "", otherValue = "";

        myKey = dict.FirstOrDefault(x => x.Value.ToString() == vaule1).Key;
        otherKey = dict.FirstOrDefault(x => x.Value.ToString() == value2).Key;

        dict[myKey] = value2;
        dict[otherKey] = vaule1;
        RenameKey(dict, myKey, "x");
        RenameKey(dict, otherKey, myKey);
        RenameKey(dict, "x", otherKey);
    }

    /// <summary>
    /// 交換字典物件
    /// </summary>
    /// <param name="key1"></param>
    /// <param name="key2"></param>
    /// <param name="dict"></param>
    public static bool SwapDictValueByKey<TKey, TValue>(TKey key1, TKey key2, Dictionary<TKey, TValue> dict)
    {
        if (dict.ContainsKey(key1) && dict.ContainsKey(key2))
        {
            TValue value1, value2;

            dict.TryGetValue(key1, out value1);
            dict.TryGetValue(key2, out value2);

            dict[key1] = value2;
            dict[key2] = value1;

            return true;
        }

        return false;
    }

    /// <summary>
    /// 交換字典Key
    /// </summary>
    /// <param name="key1"></param>
    /// <param name="key2"></param>
    /// <param name="tmpKey">交換用臨時Key</param>
    /// <param name="dict"></param>
    public static bool SwapDictKey<TKey, TValue>(TKey key1, TKey key2, TKey tmpKey, Dictionary<TKey, TValue> dict)
    {
        if (dict.ContainsKey(key1) && dict.ContainsKey(key2))
        {
            RenameKey(dict, key1, tmpKey);
            RenameKey(dict, key2, key1);
            RenameKey(dict, tmpKey, key2);
            return true;
        }
        return false;
    }
    public static bool DictionaryCompare<TKey, TValue>(Dictionary<TKey, TValue> dict1, Dictionary<TKey, TValue> dict2)
    {
        // early-exit checks
        if (null == dict2)
            return null == dict1;
        if (null == dict1)
            return false;
        if (object.ReferenceEquals(dict1, dict2))
            return true;
        if (dict1.Count != dict2.Count)
            return false;

        int i = 0;
        foreach (KeyValuePair<TKey, TValue> dict1Item in dict1)
        {
            foreach (KeyValuePair<TKey, TValue> dict2Item in dict2)
            {
                int j = 0;
                if (i == j)
                {
                    if (!dict1Item.Key.Equals(dict2Item.Key) || !dict1Item.Value.Equals(dict2Item.Value))
                        return false;
                    break;
                }
                j++;
            }
            i++;
        }

        // check keys are the same
        foreach (TKey k in dict1.Keys)
            if (!dict2.ContainsKey(k))
                return false;

        // check values are the same
        foreach (TKey k in dict1.Keys)
            if (!dict1[k].Equals(dict2[k]))
                return false;

        return true;
    }

    public static TimeSpan Time(Action action)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }

}