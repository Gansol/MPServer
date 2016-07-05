using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using MPProtocol;

public class Global
{
    public static readonly string serverPath = "http://192.168.88.77:58767/MicePow";//Server路徑
    //Android or iOS or Win 伺服器中的 檔案列表路徑
    public static readonly string serverListPath = serverPath +
#if UNITY_ANDROID
 "/AndroidList/";
#elif UNITY_IPHONE
    "/iOSList/";
#elif UNITY_STANDALONE_WIN
    "/WebList/";
#else
 string.Empty;
#endif
    //Android or iOS or Win 伺服器中的 檔案路徑
    public static readonly string assetBundlesPath = serverPath +
#if UNITY_ANDROID
 "/AndroidBundles/";
#elif UNITY_IPHONE
    "/iOSBundles/";
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
    "/WebBundles/";
#else
 string.Empty;
#endif

    public static PhotonService photonService = new PhotonService();    // Photon ServerClient服務

    public static readonly string sItemList = "ItemList.json";          // 道具列表
    public static readonly string sVisionList = "VisionList.json";      // 版本列表
    public static readonly string sDownloadList = "DownloadList.json";  // 下載列表
    public static readonly string sFullPackage = "FullPackageList.json";// 完整下載列表

    public static bool isCheckBundle = false;       // 是否檢查資源
    public static bool isNewlyVision = true;        // 是否為新版本
    public static bool isVisionDownload = false;    // 是否開始下載版本列表
    public static bool isCompleted = false;         // 是否下載完成
    public static bool isReplaced = false;          // 是否取代列表完成
    public static bool isJoinMember = true;         // 是否加入會員

    public static bool isPlayerDataLoaded = false;  // 是否載入玩家資料
    public static bool isCurrencyLoaded = false;    // 是否載入玩家資料
    public static bool isMiceLoaded = false;        // 是否載入老鼠資料
    public static bool isLoaded = false;            // 是否載入場景

    public static bool LoginStatus = false;	        // true = 已登入,  false = 未登入
    public static bool BattleStatus = false;        // 是否開始對戰
    public static bool isMatching = false;          // 是否配對成功
    public static bool isExitingRoom = false;       // 是否離開房間
    public static bool isGameStart = false;         // 是否開始遊戲
    public static bool isApplySkill = false;        // 是否受到技能傷害
    public static bool spawnFlag = false;           // 是否產生完成
    public static bool isMissionCompleted = false;  // 是否任務完成
    public static bool missionFlag = true;         // 是否執行任務

    public static int loadScene;            // 要被載入的場景

    public static int maxConnTimes = 5;  // 重新連限次數

    public static string Ret = "";          // 回傳值
    public static int PrimaryID = 0;       // 主索引
    public static string Account = "";      // 帳號
    public static string Nickname = "";     // 暱稱
    public static int RoomID = -1;          // 房間ID
    public static byte Sex = 0;              // 性別
    public static byte Age = 0;              // 年齡
    public static MemberType MemberType;     // 年齡
    public static int Rice = 0;             // 遊戲幣
    public static Int16 Gold = 0;           // 金幣

    public static byte Rank = 0;            // 等級
    public static byte EXP = 0;             // 經驗
    public static Int16 MaxCombo = 0;       // 最大連擊
    public static int MaxScore = 0;         // 最高分
    public static int SumScore = 0;         // 總分
    public static Int16 SumLost = 0;          // 總漏掉的老鼠
    public static int SumKill = 0;          // 總除掉的老鼠
    public static int MeunObjetDepth = 10000; // 主選單物件深度
    public static string Item = "";         // 漏掉的老鼠
    public static string MiceAll = "";      // 全部老鼠 JSON資料
    public static string Team = "";         // 隊伍老鼠 JSON資料
    public static string MiceAmount = "";   // 老鼠數量 JSON資料
    public static string Friend = "";       // 好友列表 JSON資料

    public static string ext = ".unity3d";       // AB副檔名

    public static class OtherData
    {
        public static int PrimaryID = 0;        // 主索引
        public static string Nickname = "";     // 暱稱
        public static byte Sex = 0;              // 性別
        public static string Team = "";         // 隊伍老鼠 JSON資料
        public static string RoomPlace = "";    // 另一位玩家的房間位置
    }

    public static int MiceCount = 0;        // 目前 對戰老鼠數量 要移到BattleData

    public static Dictionary<string, object> miceProperty = new Dictionary<string, object>();   // 老鼠屬性資料 JSON資料

}