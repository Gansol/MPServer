using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Text;
using MiniJSON;
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
 * 建立AssetBundle Hash資訊
 * 自動建立AssetBundle與相關依賴物件(無壓縮)
 * 自動建立AssetBundle與相關依賴物件(LZMA壓縮)
 * ※禁止使用相同檔名，否則會覆蓋並出現錯誤※
 * 
 * ***************************************************************/

public class AssetBundlesCreator : EditorWindow
{
    public static string perfabFolder =
#if UNITY_ANDROID
 "/AssetBundles/";
#elif UNITY_IPHONE
    "/AssetBundles/";
#elif UNITY_STANDALONE
    "/AssetBundles/";
#endif

    public static string exportFolder =
#if UNITY_ANDROID
 "/_AssetBundles/Android/";
#elif UNITY_IPHONE
    "/_AssetBundles/iOS/";
#elif UNITY_STANDALONE
    "/_AssetBundles/STANDALONE/";
#endif

    public Planform planform =
#if UNITY_ANDROID
 Planform.Android;
#elif UNITY_IPHONE
        Planform.iOS;
#elif UNITY_STANDALONE
    Planform.STANDALONE;
#endif

    public static string fileExtension = ".unity3d";

    static BuildTarget buildTarget = BuildTarget.WP8Player;
    static BuildAssetBundleOptions deterministicAssetBundle;

    static Dictionary<string, object> files = new Dictionary<string, object>();
    static List<string> ext = new List<string> { };
    static string[] hashType = new string[] { "SHA1", "MD5", "SHA512" };

    public Rect baseWindowRect = new Rect(10, 25, 400, 50);
    public Rect buildWindowRect = new Rect(10, 250, 400, 50);
    public Rect hashWindowsRect = new Rect(10, 450, 400, 50);

    static bool _uncompressed;
    static bool _dependent;
    static bool _otherPlatform;
    static bool _bPrefab;
    static bool _bMat;
    static bool _bPng;
    static bool _bJpge;
    static int _hashIndex;

    static string _hash;
    static string _targetDir = Application.dataPath + exportFolder; // 建置目錄
    static string _perfabDir = Application.dataPath + perfabFolder; // 預置物件目錄

    public enum Planform : byte
    {
        Android,
        iOS,
        STANDALONE,
    }


    [MenuItem("Gansol/Build AssetBundle %&b")]
    static void Init()  // 初始化編輯視窗
    {
        AssetBundlesCreator window = (AssetBundlesCreator)EditorWindow.GetWindow(typeof(AssetBundlesCreator));
        window.Show();
    }

    void OnGUI()
    {
        BeginWindows();
        baseWindowRect = GUILayout.Window(0, baseWindowRect, BaseWondow, "Base Setting");
        buildWindowRect = GUILayout.Window(1, buildWindowRect, BuildAssetWondow, "Build Platform");
        hashWindowsRect = GUILayout.Window(2, hashWindowsRect, BuildHashWondow, "AssetBundle Hash");
        EndWindows();
    }



    void BaseWondow(int unusedWindowID) // 基礎選項視窗物件
    {
        // unusedWindowID = 視窗ID
        perfabFolder = EditorGUILayout.TextField("AssetBundles folder: ", perfabFolder);
        exportFolder = EditorGUILayout.TextField("Export folder: ", exportFolder);
        fileExtension = EditorGUILayout.TextField("Bundle file ext: ", fileExtension);
        EditorGUILayout.LabelField("");
        EditorGUILayout.LabelField("Bundle Type");
        GUILayout.BeginHorizontal();
        _bPrefab = GUILayout.Toggle(_bPrefab, "*.prefab"); // CheckBox 核取欄位
        _bMat = GUILayout.Toggle(_bMat, "*.mat"); // CheckBox 核取欄位
        _bPng = GUILayout.Toggle(_bPng, "*.png"); // CheckBox 核取欄位
        _bJpge = GUILayout.Toggle(_bJpge, "*.jpg"); // CheckBox 核取欄位
        GUILayout.EndHorizontal();
        EditorGUILayout.LabelField("");
        _uncompressed = GUILayout.Toggle(_uncompressed, "Uncompressed AssetBundle"); // CheckBox 核取欄位
        _dependent = GUILayout.Toggle(_dependent, "Dependent AssetBundle");
        EditorGUILayout.LabelField("");
    }

    void BuildAssetWondow(int unusedWindowID)   // 建置選項視窗物件
    {
        // unusedWindowID = 視窗ID
        if (GUILayout.Button("Android"))
        {
            buildTarget = BuildTarget.Android;
            BuildAssetBundle();
        }

        if (GUILayout.Button("iPhone"))
        {
            buildTarget = BuildTarget.iPhone;
            BuildAssetBundle();
        }

        if (GUILayout.Button("Web Player"))
        {
            buildTarget = BuildTarget.WP8Player;
            BuildAssetBundle();
        }

        if (GUILayout.Button("Standalone Windows"))
        {
            buildTarget = BuildTarget.StandaloneWindows;
            BuildAssetBundle();
        }

        if (GUILayout.Button("Standalone OSX Universal"))
        {
            buildTarget = BuildTarget.StandaloneOSXUniversal;
            BuildAssetBundle();
        }

        _otherPlatform = EditorGUILayout.BeginToggleGroup("Other Platform", _otherPlatform);    // BeginToggleGroup = 核取區域 打勾後才會開啟
        buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build Target", buildTarget);  // ListBox 下拉式選單(用enum當選項值)

        if (GUILayout.Button("Build AssetBundle"))
            BuildAssetBundle();

        EditorGUILayout.EndToggleGroup();   //核取區域結尾
    }

    void BuildHashWondow(int unusedWindowID)    // 雜湊選項視窗物件
    {
        EditorGUILayout.LabelField("Hash Type");
        _hashIndex = EditorGUILayout.Popup(_hashIndex, hashType);   // ListBox 下拉式選單(只能用字串陣列當選項值)
        EditorGUILayout.LabelField("");
        if (GUILayout.Button("Build AssetBundle Hash"))
        {
            BuildAssetHash();
        }
    }

    private static void BundleTypeSelect()
    {
        Debug.Log(_bPrefab);
        if (_bPrefab) ext.Add(".prefab"); else ext.Remove(".prefab");
        if (_bMat) ext.Add(".mat"); else ext.Remove(".mat");
        if (_bPng) ext.Add(".png"); else ext.Remove(".png");
        if (_bJpge) ext.Add(".jpg"); else ext.Remove(".jpg");

        foreach (string item in ext)
        {
            Debug.Log("ITEM:" + item);
        }
    }

    private static void BuildAssetBundle()  // 建置
    {
        BundleTypeSelect();

        if (_uncompressed)
            BuildAssetNoLZMA();
        else
            BuildAssetLZMA();
    }

    public static void BuildAssetLZMA() // Build AssetBundle(Compressed) 建立壓縮的AssetBundle
    {
        // BuildAssetLZMA 和 BuildAssetNoLZMA 只差別在 BuildAssetBundleOptions.UncompressedAssetBundle
        InfoCreator();  // 建立資產訊息

        if (!Directory.Exists(_targetDir))  // 如果資料夾不存在則建立
            Directory.CreateDirectory(_targetDir);

        string[] folders = Directory.GetDirectories(_perfabDir);    // 取得目錄所有下資料夾

        foreach (string folder in folders)  // 尋遍所有資料夾
        {
            if (_dependent) // 如果是依賴資產型態
            {
                string[] innFolder = Directory.GetDirectories(folder + "/");    // 取得目錄下所有子資料夾

                //共用資產
                if (!File.Exists(innFolder[0] + "/" + "BundleInfo.json"))   // 如果找不到資產清單 報錯
                {
                    Debug.LogError("BundleInfo.json exist folder! Please PreBuild BundleInfo." + "   Path Info:" + innFolder[0]);
                }
                else
                {
                    LoadFile(innFolder[0] + "/" + "BundleInfo.json");   // innFolder[0] = Share資料夾 載入子資料夾下資產清單(共用物件)

                    BuildPipeline.PushAssetDependencies();  //共用資產打包開始;

                    Object obj;
                    uint crc = 0;   // crc檢驗碼

                    foreach (KeyValuePair<string, object> file in files)    // 建立AssetBundle 尋遍資料夾下所有檔案
                    {
                        string filePath = _targetDir + file.Key + fileExtension;    // file.Key = 檔案名稱 fileExtension = AssetBundle副檔名
                        obj = AssetDatabase.LoadMainAssetAtPath(file.Value.ToString()); // 載入資產

                        Debug.Log("Building Share Prefab:" + obj.name);

                        if (File.Exists(filePath))  // 如果輸出資料夾下已經有舊檔案，刪除檔案
                            File.Delete(filePath);
                        //Build;
                        if (BuildPipeline.BuildAssetBundle(obj, null, _targetDir + file.Key + fileExtension, out crc, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.DeterministicAssetBundle, buildTarget))
                            AssetDatabase.Refresh();    // 加入依賴訊息後並重新整理資產訊息
                    }
                }

                //獨立資產
                LoadFile(innFolder[1] + "/" + "BundleInfo.json");   // innFolder[1] = Unique資料夾 載入子資料夾下資產清單(獨立物件)
                if (!File.Exists(innFolder[1] + "/" + "BundleInfo.json"))
                {
                    Debug.LogError("BundleInfo.json exist folder! Please PreBuild BundleInfo." + "   Path Info:" + innFolder[1]);
                }
                else
                {
                    BundleUniqueAssetBundle();  // 建立獨立資產
                }


                BuildPipeline.PopAssetDependencies();   //共用資產打包結束;
            }
            else // 如果不是依賴資源
            {
                if (!File.Exists(folder + "/" + "BundleInfo.json"))
                {
                    Debug.LogError("BundleInfo.json exist folder! Please PreBuild BundleInfo." + "   Path Info:" + folder);
                }
                else
                {
                    LoadFile(folder + "/" + "BundleInfo.json");
                    BundleUniqueAssetBundle();// 建立獨立資產
                }
            }

            Debug.Log("*****Building AssetBundle Complete!*****");
        }
    }

    public static void BuildAssetNoLZMA()   // Build AssetNoLZMA 建立基本的的AssetBundle
    {
        // 說明同上
        InfoCreator();

        if (!Directory.Exists(_targetDir))
            Directory.CreateDirectory(_targetDir);

        string[] folders = Directory.GetDirectories(_perfabDir);

        foreach (string folder in folders)
        {
            if (_dependent)
            {
                string[] innFolder = Directory.GetDirectories(folder + "/");

                //共用資產
                if (!File.Exists(innFolder[0] + "/" + "BundleInfo.json"))
                {
                    Debug.LogError("BundleInfo.json exist folder! Please PreBuild BundleInfo." + "   Path Info:" + innFolder[0]);
                }
                else
                {
                    LoadFile(innFolder[0] + "/" + "BundleInfo.json");

                    BuildPipeline.PushAssetDependencies();  //共用資產打包開始;

                    Object obj;
                    uint crc = 0;

                    foreach (KeyValuePair<string, object> file in files)
                    {
                        string filePath = _targetDir + file.Key + fileExtension;
                        obj = AssetDatabase.LoadMainAssetAtPath(file.Value.ToString());

                        Debug.Log("Building Share Prefab:" + obj.name);

                        if (File.Exists(filePath))
                            File.Delete(filePath);
                        //Build;
                        if (BuildPipeline.BuildAssetBundle(obj, null, _targetDir + file.Key + fileExtension, out crc, BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.DeterministicAssetBundle, buildTarget))
                        {
                            AssetDatabase.Refresh();
                        }
                    }
                }

                //獨立資產
                LoadFile(innFolder[1] + "/" + "BundleInfo.json");
                if (!File.Exists(innFolder[1] + "/" + "BundleInfo.json"))
                {
                    Debug.LogError("BundleInfo.json exist folder! Please PreBuild BundleInfo." + "   Path Info:" + innFolder[1]);
                }
                else
                {
                    BundleUniqueAssetBundle();
                }


                BuildPipeline.PopAssetDependencies();   //共用資產打包結束;
            }
            else
            {
                if (!File.Exists(folder + "/" + "BundleInfo.json"))
                {
                    Debug.LogError("BundleInfo.json exist folder! Please PreBuild BundleInfo." + "   Path Info:" + folder);
                }
                else
                {
                    LoadFile(folder + "/" + "BundleInfo.json");
                    BundleUniqueAssetBundle();
                }
            }

            Debug.Log("*****Building AssetBundle Complete!*****");
        }
    }

    private static bool LoadFile(string path)   // 載入JSON列表檔案
    {
        // path = 資料夾物件下BundleInfo.json
        // 載入文字後並解析存入字典檔
        string text = File.ReadAllText(path);

        if (text != null)
        {
            files = Json.Deserialize(text) as Dictionary<string, object>;
            return true;
        }
        return false;
    }



    public static void BuildAssetHash() // 建立AssetBundle Hash
    {
        string bundleFolderPath = Application.dataPath + exportFolder;  //輸出路徑
        string[] pathFiles;
        byte[] bytesFile;


        if (!Directory.Exists(bundleFolderPath))
        {
            Debug.LogError("Bundle Folder Not Found !" + "  Path:" + bundleFolderPath);
        }
        else
        {
            Dictionary<string, object> dictBundles = new Dictionary<string, object>();
            pathFiles = Directory.GetFiles(bundleFolderPath, "*" + fileExtension); //取得 本機資料夾 全部檔案


            foreach (string file in pathFiles) // 尋遍所有資料夾下 檔案路徑
            {
                bytesFile = File.ReadAllBytes(file); //讀取檔案bytes
                switch (_hashIndex)
                {
                    case 0: // SHA1
                        _hash = AssetBundlesHash.SHA1Complier(bytesFile);    // Hash bytes
                        break;
                    case 1: // MD5
                        _hash = AssetBundlesHash.MD5Complier(bytesFile);     // Hash bytes
                        break;
                    case 2: // SHA512
                        _hash = AssetBundlesHash.SHA512Complier(bytesFile);  // Hash bytes
                        break;
                }
                dictBundles.Add(Path.GetFileName(file), _hash);//把hash過的值存入字典檔
            }
            CreateFile(Json.Serialize(dictBundles), bundleFolderPath, "BundleHash.json"); //建立 新 檔案列表
        }
        Debug.Log("*****Bundle Hash Completed!*****" + "  Path:" + bundleFolderPath);
    }



    public static void InfoCreator() //Bunlde InfoCreator 負責建立 Bundle資訊(物件名稱、Assets路徑)
    {
        string folderPath = Application.dataPath + perfabFolder;
        if (!Directory.Exists(folderPath))
        {
            Debug.LogError("Bundle Folder Not Found !" + "  Path:" + folderPath);
        }
        else
        {
            string[] folders = Directory.GetDirectories(folderPath); //取得 AssetBundles資料夾下 全部資料夾

            foreach (string folder in folders) // 尋遍所有資料夾下 檔案路徑
            {
                if (_dependent)
                {
                    string[] innFolders = Directory.GetDirectories(folder + "/");

                    foreach (string innfolder in innFolders)
                        AssetInfoJSON(innfolder);
                }
                else
                {
                    AssetInfoJSON(folder);
                }
            }
            Debug.Log("*****Bundle BundleInfo Completed!*****" + "  Path:" + perfabFolder);
        }
    }

    public static void AssetInfoJSON(string folder) //解析 檔案列表路徑
    {
        Dictionary<string, object> dictBundles = new Dictionary<string, object>();

        Debug.Log("folder:" + folder);

        // 這個方法可以把LINQ字串轉換成string用豆點分開
        //string files = string.Join(",",Directory.GetFiles(folder + "/", "*.*").Where(x => ext.Any(e=> x.EndsWith(e))).ToArray()) ;
        // 這個方法可以把LINQ字串轉換成string[]
        //Where 查詢哪裡的物件 x = ext副檔名陣列值  再從任意值中取出一個值(e)比較  1.Where查詢 2.Any 來判斷序列是否包含任何項目
        string[] files = Directory.GetFiles(folder + "/", "*.*").Where(x => ext.Any(e => x.EndsWith(e))).Select(s => s).ToArray(); ;

        dictBundles.Clear();

        foreach (string file in files)
        {
            Debug.Log("file Path:" + file.Substring(System.Environment.CurrentDirectory.ToString().Replace("'\'", "/").Length + 1));
            string filePath = file.Substring(System.Environment.CurrentDirectory.ToString().Replace("'\'", "/").Length + 1);
            string extension = System.IO.Path.GetExtension(Path.GetFileName(file)); // 取出檔案副檔名
            string fileName = Path.GetFileName(file.Substring(0, file.Length - extension.Length));  // 檔名-副檔名長度=檔案名稱
            dictBundles.Add(fileName, filePath);
        }
        CreateFile(Json.Serialize(dictBundles), folder + "/", "BundleInfo.json"); //建立 新 檔案列表
    }

    protected static void CreateFile(string contant, string path, string fileName) //建立JSON檔案
    {
        if (File.Exists(path + fileName))
            File.Delete(path + fileName);
        using (FileStream fs = File.Create(path + fileName)) //using 會自動關閉Stream 建立檔案
        {
            fs.Write(new UTF8Encoding(true).GetBytes(contant), 0, contant.Length); //寫入檔案
            fs.Dispose(); //避免錯誤 在寫一次關閉
        }
    }


    #region BundleUniqueAssetBundle
    static void BundleUniqueAssetBundle()
    {
        Object obj;
        uint crc = 0;

        foreach (KeyValuePair<string, object> file in files)
        {
            //獨立資產打包;
            BuildPipeline.PushAssetDependencies();
            obj = AssetDatabase.LoadMainAssetAtPath(file.Value.ToString());
            Debug.Log("Building Unique Prefab:" + obj.name);
            //Build;
            if (_uncompressed)
            {
                if (BuildPipeline.BuildAssetBundle(obj, null, _targetDir + file.Key + fileExtension, out crc, BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.DeterministicAssetBundle, buildTarget))
                    AssetDatabase.Refresh();
            }
            else
            {
                if (BuildPipeline.BuildAssetBundle(obj, null, _targetDir + file.Key + fileExtension, out crc, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.DeterministicAssetBundle, buildTarget))
                    AssetDatabase.Refresh();
            }
            BuildPipeline.PopAssetDependencies();
        }
    }
    #endregion
}
