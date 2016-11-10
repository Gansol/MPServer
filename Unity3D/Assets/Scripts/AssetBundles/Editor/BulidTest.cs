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
 * AssetBundlesCreator Alpha v1.0.0
 * 
 * 建立AssetBundle Hash資訊
 * 自動建立AssetBundle與相關依賴物件(無壓縮)
 * 自動建立AssetBundle與相關依賴物件(LZMA壓縮)
 * ※禁止使用相同檔名，否則會覆蓋並出現錯誤※
 * 
 * 使用方法:
 * 在AssetBundle Folder下(可修改)，建立資料夾放入資產即可開始打包
 * 可建立多個資料夾分類
 * 
 * 1.AssetBundles Folder: 資產資料夾路徑 i.e. /AssetFolderName/
 * 2.Export Folder: 輸出資料夾 i.e. /ExportFolderName/Paltform/
 * 3.Bundle file ext: 輸出資產的副檔名 。
 * 4.資料夾路徑會隨著平台轉，自動切換預設位置(常用平台)。
 * 5.BundleType: 可選擇要打包的種類(目前常用項目4項)。
 * 6.Uncompressed AssetBundle: 勾選後將不會壓縮AssetBundle。
 * 7-1.Dependent AssetBundle: 勾選後將啟用建置依賴包。
 * 7-2.勾選後必須再建立子資料夾 "第一排序"資料夾必定是被依賴物件。
 *     第二個資料夾資產會依賴第一個資料夾內資產，且為"第二排序"。
 * 7-3.AssetBundle/MyObject/Share <--  AssetBundle/MyObject/Unique
 * 8.Build Platform: AssetBundle建置平台
 * 9.Other Platform: 其他平台(可能不支援，無法測試)
 * 10.AssetBundle Hash: 可以建立物件對應的Hash 輸出JSON文件
 * 11.HashType: 目前提供三種Hash MD5(32) SHA1(40) SHA512(64)
 * ***************************************************************/

public class BulidTest : EditorWindow
{
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

    static string fileExtension = ".unity3d";

    static BuildTarget buildTarget = BuildTarget.WP8Player;
    static BuildAssetBundleOptions deterministicAssetBundle;

    static Dictionary<string, object> files = new Dictionary<string, object>();
    static List<string> ext = new List<string> { };
    static string[] hashType = new string[] { "SHA1", "MD5", "SHA512" };

    Rect baseWindowRect = new Rect(10, 25, 400, 50);
    Rect buildWindowRect = new Rect(10, 290, 400, 50);
    Rect hashWindowsRect = new Rect(10, 500, 400, 50);

    static bool _uncompressed;
    static bool _dependent;
    static bool _otherPlatform;
    static bool _bPrefab;
    static bool _bMat;
    static bool _bPng;
    static bool _bJpge;
    static int _hashIndex;
    static bool _bMp3;
    static bool _bWav;
    static bool _bOgg;
    static bool _bAnim;
    static bool _bController;

    static string _hash;
    static string _targetDir = Application.dataPath + exportFolder; // 建置目錄
    static string _perfabDir = Application.dataPath + perfabFolder; // 預置物件目錄



    [MenuItem("Gansol/Build Test %&b")]
    static void Init()  // 初始化編輯視窗
    {
        BulidTest window = (BulidTest)EditorWindow.GetWindow(typeof(BulidTest));
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



    private void BaseWondow(int unusedWindowID) // 基礎選項視窗物件
    {
        // unusedWindowID = 視窗ID
        perfabFolder = EditorGUILayout.TextField("AssetBundles folder: ", perfabFolder);
        exportFolder = EditorGUILayout.TextField("Export folder: ", exportFolder);
        fileExtension = EditorGUILayout.TextField("Bundle file ext: ", fileExtension);
        EditorGUILayout.LabelField("Bundle Type");
        GUILayout.BeginHorizontal();
        _bPrefab = GUILayout.Toggle(_bPrefab, "*.prefab"); // CheckBox 核取欄位
        _bMat = GUILayout.Toggle(_bMat, "*.mat"); // CheckBox 核取欄位
        _bPng = GUILayout.Toggle(_bPng, "*.png"); // CheckBox 核取欄位
        _bJpge = GUILayout.Toggle(_bJpge, "*.jpg"); // CheckBox 核取欄位
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        _bMp3 = GUILayout.Toggle(_bMp3, "*.mp3"); // CheckBox 核取欄位
        _bWav = GUILayout.Toggle(_bWav, "*.wav"); // CheckBox 核取欄位
        _bOgg = GUILayout.Toggle(_bOgg, "*.ogg"); // CheckBox 核取欄位
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        _bAnim = GUILayout.Toggle(_bAnim, "*.anim"); // CheckBox 核取欄位
        _bController = GUILayout.Toggle(_bController, "*.controller"); // CheckBox 核取欄位
        GUILayout.EndHorizontal();
        EditorGUILayout.LabelField("");
        _uncompressed = GUILayout.Toggle(_uncompressed, "Uncompressed AssetBundle"); // CheckBox 核取欄位
        _dependent = GUILayout.Toggle(_dependent, "Dependent AssetBundle");
        EditorGUILayout.LabelField("");
        if (GUILayout.Button("Clear Cache"))
        {
            Caching.CleanCache();
            Debug.Log("*****LoadFromCacheordDownload Clear!*****");
        }
    }

    private void BuildAssetWondow(int unusedWindowID)   // 建置選項視窗物件
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

    private void BuildHashWondow(int unusedWindowID)    // 雜湊選項視窗物件
    {
        EditorGUILayout.LabelField("Hash Type");
        _hashIndex = EditorGUILayout.Popup(_hashIndex, hashType);   // ListBox 下拉式選單(只能用字串陣列當選項值)
        EditorGUILayout.LabelField("");
        if (GUILayout.Button("Build AssetBundle Hash"))
        {
            BuildAssetHash();
        }
    }

    private static void BundleTypeSelect()  // 選擇要打包的副檔名
    {
        Debug.Log(_bPrefab);
        if (_bPrefab) ext.Add(".prefab"); else ext.Remove(".prefab");
        if (_bMat) ext.Add(".mat"); else ext.Remove(".mat");
        if (_bPng) ext.Add(".png"); else ext.Remove(".png");
        if (_bJpge) ext.Add(".jpg"); else ext.Remove(".jpg");
        if (_bMp3) ext.Add(".mp3"); else ext.Remove(".mp3");
        if (_bWav) ext.Add(".wav"); else ext.Remove(".wav");
        if (_bOgg) ext.Add(".ogg"); else ext.Remove(".ogg");
        if (_bAnim) ext.Add(".anim"); else ext.Remove(".anim");
        if (_bController) ext.Add(".controller"); else ext.Remove(".controller");
    }

    private static void BuildAssetBundle()  // 建置
    {
        BundleTypeSelect();

        if (_uncompressed)
            BuildAssetLZMA();
        else
            BuildAssetLZMA();
    }


    private static Object[] SelectDenpendenices(string filePath)
    {
        string[] paths = new string[] { filePath };
        string[] dependencies = AssetDatabase.GetDependencies(paths);

        List<Object> depenObj = new List<Object>();


        foreach (string depen in dependencies)
        {
            depenObj.Add(AssetDatabase.LoadMainAssetAtPath(depen));
        }

        return depenObj.ToArray();
    }



    #region BuildAssetLZMA
    private static void BuildAssetLZMA() // Build AssetBundle(Compressed) 建立壓縮的AssetBundle
    {
        // BuildAssetLZMA 和 BuildAssetNoLZMA 只差別在 BuildAssetBundleOptions.UncompressedAssetBundle
        InfoCreator();  // 建立資產訊息

        if (!Directory.Exists(_targetDir))  // 如果資料夾不存在則建立
            Directory.CreateDirectory(_targetDir);

        string[] folders = Directory.GetDirectories(_perfabDir);    // 取得目錄所有下資料夾

       

        foreach (string folder in folders)  // 尋遍所有資料夾
        {
            string folderName = Path.GetFileName(Path.GetDirectoryName(folder + "/"));
            if (!Directory.Exists(_targetDir + folderName))  // 如果資料夾不存在則建立
                Directory.CreateDirectory(_targetDir + folderName + "/");
            string outputFolder = _targetDir + folderName + "/";

            if (_dependent) // 如果是依賴資產型態
            {
                string[] innFolder = Directory.GetDirectories(folder + "/");    // 取得目錄下所有子資料夾

                

                //獨立資產
                Dictionary<string, object> files = LoadFile(innFolder[1] + "/" + "BundleInfo.json");   // innFolder[1] = Unique資料夾 載入子資料夾下資產清單(獨立物件)
                if (innFolder[1] != null)
                {
                    if (!File.Exists(innFolder[1] + "/" + "BundleInfo.json"))
                    {
                        Debug.LogError("BundleInfo.json exist folder! Please PreBuild BundleInfo." + "   Path Info:" + innFolder[1]);
                    }
                    else
                    {
                        BundleUniqueAssetBundle(files, innFolder, outputFolder);  // 建立獨立資產  (1)
                    }
                }
                else
                {
                    Debug.LogError("Dependencies Floder Error!");
                }












                BundleShareAssetBundle(innFolder[0], outputFolder);

                





            }
            else // 如果不是依賴資源
            {
                /*
                if (!File.Exists(folder + "/" + "BundleInfo.json"))
                {
                    Debug.LogError("BundleInfo.json exist folder! Please PreBuild BundleInfo." + "   Path Info:" + folder);
                }
                else
                {
                     Dictionary<string, object> files = LoadFile(folder + "/" + "BundleInfo.json");
                     BundleUniqueAssetBundle(files,innFolder, outputFolder);// 建立獨立資產
                }
                 * */
            }

            Debug.Log("*****Building AssetBundle Complete!*****");
        }
    }
    #endregion
   

    private static void BundleShareAssetBundle(string innFolder, string outputFolder)   // 打包獨立資產
    {
        
        //共用資產 獨立打包
        if (!File.Exists(innFolder + "/" + "BundleInfo.json"))   // 如果找不到資產清單 報錯
        {
            Debug.LogError("BundleInfo.json exist folder! Please PreBuild BundleInfo." + "   Path Info:" + innFolder);
        }
        else
        {

            Dictionary<string, object> files = LoadFile(innFolder + "/" + "BundleInfo.json");   // innFolder[0] = Share資料夾 載入子資料夾下資產清單(共用物件)

            //共用資產打包開始;

            Object obj;
            uint crc = 0;   // crc檢驗碼

            
            foreach (KeyValuePair<string, object> file in files)    // 建立AssetBundle 尋遍資料夾下所有檔案
            {
                BuildAssetBundleOptions bundleOptions = BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.CompleteAssets;
                string filePath = _targetDir + file.Key + fileExtension;    // file.Key = 檔案名稱 fileExtension = AssetBundle副檔名
                obj = AssetDatabase.LoadMainAssetAtPath(file.Value.ToString()); // 載入資產

                Debug.Log("Building Share Prefab:" + obj.name);

                if (File.Exists(filePath))  // 如果輸出資料夾下已經有舊檔案，刪除檔案
                    File.Delete(filePath);

                Object[] depenObj = SelectDenpendenices(file.Value.ToString());

                if (_uncompressed) bundleOptions |= BuildAssetBundleOptions.UncompressedAssetBundle;
                //Build;
                BuildPipeline.PushAssetDependencies();
                if (BuildPipeline.BuildAssetBundle(null, depenObj, outputFolder + file.Key + fileExtension, out crc, bundleOptions, buildTarget))
                    AssetDatabase.Refresh();    // 加入依賴訊息後並重新整理資產訊息
                BuildPipeline.PopAssetDependencies();
                AssetDatabase.Refresh();
            }
        }
    }

    #region BundleUniqueAssetBundle
    private static void BundleUniqueAssetBundle(Dictionary<string, object> dictUniquePaths, string[] innFolders, string outputFolder)   // 打包獨立資產
    {
        Object obj;
        uint crc = 0;
        
        
        // 歷遍所有要打包的物件
        foreach (KeyValuePair<string, object> file in dictUniquePaths)
        {
            BuildAssetBundleOptions bundleOptions = BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.CollectDependencies;
            obj = AssetDatabase.LoadMainAssetAtPath(file.Value.ToString());
            Debug.Log("Building Unique Prefab:" + obj.name);

            Dictionary<string, object> dictSharePaths = LoadFile(innFolders[0] + "/" + "BundleInfo.json");

            //Object[] uniqueDepenObjs = SelectDenpendenices(file.Value.ToString());
            string[] paths = new string[] { file.Value.ToString() };
            string[] uniqueDependPaths = AssetDatabase.GetDependencies(paths);
            
            List<Object> equalPrefabs = new List<Object>();

            for (int i = 0; i < uniqueDependPaths.Length; i++)
            {
                if(dictSharePaths.ContainsValue(uniqueDependPaths[i]))
                    equalPrefabs.Add(AssetDatabase.LoadMainAssetAtPath(uniqueDependPaths[i]));
            }

            // 開始建立父系依賴(NEW)
             
            BuildPipeline.PushAssetDependencies();

            foreach (Object prefab in equalPrefabs)    // 建立AssetBundle 尋遍資料夾下所有檔案
            {
                string filePath = _targetDir + file.Key + fileExtension;    // file.Key = 檔案名稱 fileExtension = AssetBundle副檔名

                Debug.Log("Building Share Prefab:" + prefab.name);

                if (File.Exists(filePath))  // 如果輸出資料夾下已經有舊檔案，刪除檔案
                    File.Delete(filePath);

                if (_uncompressed) bundleOptions |= BuildAssetBundleOptions.UncompressedAssetBundle;
                //Build;

                if (BuildPipeline.BuildAssetBundle(prefab, null, outputFolder + prefab.name + fileExtension, out crc, bundleOptions, buildTarget))
                    AssetDatabase.Refresh();    // 加入依賴訊息後並重新整理資產訊息
            }






            //獨立資產打包;
            
             if (_uncompressed) bundleOptions |= BuildAssetBundleOptions.UncompressedAssetBundle;
            //Build;
             BuildPipeline.PushAssetDependencies();
             if (BuildPipeline.BuildAssetBundle(obj, null, outputFolder + file.Key + fileExtension, out crc, bundleOptions, buildTarget))
                    AssetDatabase.Refresh();
             BuildPipeline.PopAssetDependencies();

            BuildPipeline.PopAssetDependencies();
            AssetDatabase.Refresh();
        }

    }
    #endregion

    #region BuildAssetHash
    private static void BuildAssetHash() // 建立AssetBundle Hash
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
            string[] folders = Directory.GetDirectories(bundleFolderPath);
            Dictionary<string, object> dictBundles = new Dictionary<string, object>();

            foreach (string folder in folders)
            {
                pathFiles = Directory.GetFiles(folder, "*" + fileExtension); //取得 本機資料夾 全部檔案

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
                    dictBundles.Add(Path.GetFileName(Path.GetDirectoryName(file)) + "/" + Path.GetFileName(file), _hash);//把hash過的值存入字典檔
                }
            }
            CreateFile(Json.Serialize(dictBundles), bundleFolderPath, "BundleHash.json"); //建立 新 檔案列表
        }
        Debug.Log("*****Bundle Hash Completed!*****" + "  Path:" + bundleFolderPath);
    }
    #endregion


    private static Dictionary<string,object> LoadFile(string path)   // 載入JSON列表檔案
    {
        Dictionary<string, object> files = new Dictionary<string, object>();

        // path = 資料夾物件下BundleInfo.json
        // 載入文字後並解析存入字典檔
        string text = File.ReadAllText(path);

        if (text != null)
        {
            files = Json.Deserialize(text) as Dictionary<string, object>;
            return files;
        }
        return null;
    }



    private static void InfoCreator() //Bunlde InfoCreator 負責建立 Bundle資訊(物件名稱、Assets路徑)
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

    private static void AssetInfoJSON(string folder) //解析 檔案列表路徑
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

    private static void CreateFile(string contant, string path, string fileName) //建立JSON檔案
    {
        if (File.Exists(path + fileName))
            File.Delete(path + fileName);
        using (FileStream fs = File.Create(path + fileName)) //using 會自動關閉Stream 建立檔案
        {
            fs.Write(new UTF8Encoding(true).GetBytes(contant), 0, contant.Length); //寫入檔案
            fs.Dispose(); //避免錯誤 在寫一次關閉
        }
    }


}
