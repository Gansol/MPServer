using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
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
 * ***************************************************************
 *                          ChangeLog
 * 20161110 v1.1.0  修正依賴打包方式
 * ***************************************************************/


//[System.Serializable]
public class AssetBundlesCreator : EditorWindow
{
    public static string sourcePath = Application.dataPath + "/Assetbundles";
    static readonly string AssetBundlesOutputPath = "Assets/_Assetbundles";

    public static string manifestName =
#if UNITY_ANDROID
 "AndroidBundles";
#elif UNITY_IPHONE  || UNITY_IOS
    "iOSBundles";
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    "STANDALONE_WIN";
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
    "STANDALONE_OSX";
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
 "/_AssetBundles/AndroidBundles/";
#elif UNITY_IPHONE  || UNITY_IOS
    "/_AssetBundles/iOSBundles/";
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    "/_AssetBundles/STANDALONE_WIN/";
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
    "/_AssetBundles/STANDALONE_OSX/";
#endif

    public static string publishFolder =
#if UNITY_ANDROID
 "D:/MicePowBATA/AndroidBundles/";
#elif UNITY_IPHONE || UNITY_IOS
    "E:/MicePowBETA/iOSBundles/";
#endif

    public static string publishHashFolder =
#if UNITY_ANDROID
 "D:/MicePowBATA/AndroidList/";
#elif UNITY_IPHONE  || UNITY_IOS
    "E:/MicePowBETA/iOSList/";
#endif

    static string fileExtension = ".unity3d";

 //   static BuildTarget buildTarget = BuildTarget.Android;
    static BuildAssetBundleOptions deterministicAssetBundle;

    //static Dictionary<string, object> files = new Dictionary<string, object>();
    static List<string> ext = new List<string> { };
    static string[] hashType = new string[] { "SHA1", "MD5", "CRC" };
     
    Rect baseWindowRect = new Rect(10, 10, 450, 10);
    //Rect buildWindowRect = new Rect(10, 300, 450, 50);
    Rect hashWindowsRect = new Rect(10, 200, 450, 50);

    static bool _uncompressed, _dependent, _otherPlatform, _bPublish, _bFullPackage, _bPublishAssetBundle,
                _bPrefab, _bMat, _bPng, _bJpge, _bMp3, /*_bWav, *//*_bOgg,/* _bAnim,*/ _bController;
    static int _hashIndex;
    static string _hash;
    // static string _targetDir = Application.dataPath + exportFolder; // 建置目錄
    //static string _perfabDir = Application.dataPath + perfabFolder; // 預置物件目錄



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
        hashWindowsRect = GUILayout.Window(2, hashWindowsRect, BuildHashWondow, "AssetBundle Hash");
        EndWindows();
    }
     


    private void BaseWondow(int unusedWindowID) // 基礎選項視窗物件
    {
        // unusedWindowID = 視窗ID
        perfabFolder = EditorGUILayout.TextField("AssetBundles folder: ", perfabFolder);
        exportFolder = EditorGUILayout.TextField("Export folder: ", exportFolder);
        publishFolder = EditorGUILayout.TextField("Publish folder: ", publishFolder);
        publishHashFolder = EditorGUILayout.TextField("Publish hash folder: ", publishHashFolder);
        fileExtension = EditorGUILayout.TextField("Bundle file ext: ", fileExtension);
        //EditorGUILayout.LabelField("Bundle Type");
        //GUILayout.BeginHorizontal();
        //_bPrefab = GUILayout.Toggle(_bPrefab, "*.prefab"); // CheckBox 核取欄位
        //_bMat = GUILayout.Toggle(_bMat, "*.mat"); // CheckBox 核取欄位
        //_bPng = GUILayout.Toggle(_bPng, "*.png"); // CheckBox 核取欄位
        //_bJpge = GUILayout.Toggle(_bJpge, "*.jpg"); // CheckBox 核取欄位
        //GUILayout.EndHorizontal();
        //GUILayout.BeginHorizontal();
        //_bMp3 = GUILayout.Toggle(_bMp3, "*.mp3"); // CheckBox 核取欄位
        //_bWav = GUILayout.Toggle(_bWav, "*.wav"); // CheckBox 核取欄位
        //_bOgg = GUILayout.Toggle(_bOgg, "*.ogg"); // CheckBox 核取欄位
        //GUILayout.EndHorizontal();
        //GUILayout.BeginHorizontal();
        //_bAnim = GUILayout.Toggle(_bAnim, "*.anim"); // CheckBox 核取欄位
        //_bController = GUILayout.Toggle(_bController, "*.controller"); // CheckBox 核取欄位
        //GUILayout.EndHorizontal();
        //EditorGUILayout.LabelField("");
        //_uncompressed = GUILayout.Toggle(_uncompressed, "Uncompressed AssetBundle"); // CheckBox 核取欄位
        //_dependent = GUILayout.Toggle(_dependent, "Dependent AssetBundle");
        //EditorGUILayout.LabelField("");
        if (GUILayout.Button("Clear Cache"))
        {
            Caching.ClearCache();
            Debug.Log("*****LoadFromCacheordDownload Clear!*****");
        }
    }

    private void BuildHashWondow(int unusedWindowID)    // 雜湊選項視窗物件
    {
        EditorGUILayout.LabelField("Hash Type");
        _hashIndex = EditorGUILayout.Popup(_hashIndex, hashType);   // ListBox 下拉式選單(只能用字串陣列當選項值)
        EditorGUILayout.LabelField("");

        GUILayout.BeginHorizontal();
        _bPublish = GUILayout.Toggle(_bPublish, "Publish Hash"); // CheckBox 核取欄位
        _bFullPackage = GUILayout.Toggle(_bFullPackage, "Publish Full Hash"); // CheckBox 核取欄位
        _bPublishAssetBundle= GUILayout.Toggle(_bPublishAssetBundle, "Publish Assetbundles"); // CheckBox 核取欄位
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Build AssetBundle Hash"))
        {
            BuildAssetHash();
        }
        if (GUILayout.Button("Build AssetBundle"))
        {
            BuildAssetBundle(); 
        }
    }

    public static void BuildAssetBundle()
    {
        ClearAssetBundlesName();

        Pack(sourcePath);

        string outputPath = Path.Combine(AssetBundlesOutputPath, Platform.GetPlatformFolder(EditorUserBuildSettings.activeBuildTarget));

        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        //根據目前平台打包
        BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension, EditorUserBuildSettings.activeBuildTarget);

        if (_bPublishAssetBundle)
        {
            if (!Directory.Exists(publishFolder))
                Directory.CreateDirectory(publishFolder);
             
            BuildPipeline.BuildAssetBundles(publishFolder, BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension, EditorUserBuildSettings.activeBuildTarget);
        }
            


        AssetDatabase.Refresh();

        Debug.Log("打包完成");

    }

    /// <summary>  
    /// 清除之前設置過的AssetBundleName，避免產生不必要的資源也打包  
    /// 之前說過，只要設置了AssetBundleName的，都會進行打包，不論在什麼目錄下  
    /// </summary>  
    static void ClearAssetBundlesName()
    {
        int length = AssetDatabase.GetAllAssetBundleNames().Length;
        Debug.Log(length);
        string[] oldAssetBundleNames = new string[length];
        for (int i = 0; i < length; i++)
        {
            oldAssetBundleNames[i] = AssetDatabase.GetAllAssetBundleNames()[i];
        }

        for (int j = 0; j < oldAssetBundleNames.Length; j++)
        {
            AssetDatabase.RemoveAssetBundleName(oldAssetBundleNames[j], true);
        }
        length = AssetDatabase.GetAllAssetBundleNames().Length;
        Debug.Log(length);
    }

    static void Pack(string source)
    {
        DirectoryInfo folder = new DirectoryInfo(source);
        FileSystemInfo[] files = folder.GetFileSystemInfos();
        int length = files.Length;
        for (int i = 0; i < length; i++)
        {
            if (files[i] is DirectoryInfo)
            {
                Pack(files[i].FullName);
            }
            else
            {
                if (!files[i].Name.EndsWith(".meta"))
                {
                    file(files[i].FullName);
                }
            }
        }
    }

    static void file(string source)
    {
        string _source = Replace(source);
        string _assetPath = "Assets" + _source.Substring(Application.dataPath.Length);
        string _assetPath2 = _source.Substring(Application.dataPath.Length + 1);
        //Debug.Log (_assetPath);  

        //在代碼中給資源設置AssetBundleName  
        AssetImporter assetImporter = AssetImporter.GetAtPath(_assetPath);
        string assetName = _assetPath2.Substring(_assetPath2.IndexOf("/") + 1);
        assetName = assetName.Replace(Path.GetExtension(assetName), ".unity3d");

        //Debug.Log (assetName);  
        assetImporter.assetBundleName = assetName;
    }

    static string Replace(string s)
    {
        return s.Replace("\\", "/");
    }

    public class Platform
    {

        public static string GetPlatformFolder(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.Android:
                    return "AndroidBundles";
                case BuildTarget.iOS:
                    return "IOSBundles";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
                case BuildTarget.StandaloneOSX:
                    return "OSX";
                default:
                    return null;
            }
        }

    }

    //private static void BundleTypeSelect()  // 選擇要打包的副檔名
    //{
    //    Debug.Log(_bPrefab);
    //    if (_bPrefab) ext.Add(".prefab"); else ext.Remove(".prefab");
    //    if (_bMat) ext.Add(".mat"); else ext.Remove(".mat");
    //    if (_bPng) ext.Add(".png"); else ext.Remove(".png");
    //    if (_bJpge) ext.Add(".jpg"); else ext.Remove(".jpg");
    //    if (_bMp3) ext.Add(".mp3"); else ext.Remove(".mp3");
    //    if (_bWav) ext.Add(".wav"); else ext.Remove(".wav");
    //    if (_bOgg) ext.Add(".ogg"); else ext.Remove(".ogg");
    //    if (_bAnim) ext.Add(".anim"); else ext.Remove(".anim");
    //    if (_bController) ext.Add(".controller"); else ext.Remove(".controller");
    //}



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



    #region BuildAssetHash
    private static void BuildAssetHash() // 建立AssetBundle Hash
    {
        string bundleFolderPath = Application.dataPath + exportFolder;  //輸出路徑
        Dictionary<string, object> dictBundles = new Dictionary<string, object>();


        if (!Directory.Exists(bundleFolderPath))
        {
            Debug.LogError("Bundle Folder Not Found !" + "  Path:" + bundleFolderPath);
        }
        else
        {
            string[] rootFolders = Directory.GetDirectories(bundleFolderPath);



            // 第一層資料夾
            foreach (string folders in rootFolders)
            {
                string[] innerFolders = Directory.GetDirectories(folders);

                // 第二層資料夾
                foreach (string folder in innerFolders)
                {
                    string[] paths = Directory.GetDirectories(folder);

                    // 第三層資料夾
                    foreach (string path in paths)
                    {
                        HashComplier(path, dictBundles, fileExtension);
                    }
                    HashComplier(folder, dictBundles, fileExtension);
                }
                HashComplier(folders, dictBundles, fileExtension);
            }
            List<string> pathFiles = new List<string>();
           // string[] manifestFile1 = Directory.GetFiles(Application.dataPath + exportFolder, manifestName);
            //string[] manifestFile2 = Directory.GetFiles(Application.dataPath + exportFolder, manifestName + ".manifest");

              
            HashComplier(Application.dataPath + exportFolder, dictBundles, ".");
            HashComplier(Application.dataPath + exportFolder, dictBundles, ".manifest");

            CreateFile(Json.Serialize(dictBundles), bundleFolderPath, "BundleHash.json"); //建立 新 檔案列表

            if (_bPublish)
            { 
                if (_bFullPackage)
                    CreateFile(Json.Serialize(dictBundles), publishHashFolder, "FullPackageList.json"); //建立 新 檔案列表

                CreateFile(Json.Serialize(dictBundles), publishHashFolder, "ItemList.json"); //建立 新 檔案列表

                string vision = LoadTxt(publishHashFolder + "BundleVersion.json");
                CreateFile((int.Parse(vision) + 1).ToString(), publishHashFolder, "BundleVersion.json"); //建立 新 檔案列表
            }

        }
        Debug.Log("*****Bundle Hash Completed!*****" + "  Path:" + bundleFolderPath);
    }

    private static Dictionary<string, object> HashComplier(string folders, Dictionary<string, object> dictBundles, string fileExtension)
    {
        List<string> pathFiles;
        byte[] bytesFile;
        uint crc;

        pathFiles = Directory.GetFiles(folders, "*" + fileExtension).ToList(); //取得 本機資料夾 全部檔案



        foreach (string file in pathFiles) // 尋遍所有資料夾下 檔案路徑
        {
            string path = file.Replace(@"\", "/");
            int pathLength = (Application.dataPath + exportFolder).Length;
            path = path.Remove(0, pathLength);
            Debug.Log(path);

            switch (_hashIndex)
            {
                case 0: // SHA1
                    bytesFile = File.ReadAllBytes(file); //讀取檔案bytes
                    _hash = AssetBundlesHash.SHA1Complier(bytesFile);    // Hash bytes
                    break;
                case 1: // MD5
                    bytesFile = File.ReadAllBytes(file); //讀取檔案bytes
                    _hash = AssetBundlesHash.MD5Complier(bytesFile);     // Hash bytes
                    break;
                case 2: // CRC
                    BuildPipeline.GetCRCForAssetBundle(file, out crc);
                    _hash = crc.ToString();     // Hash bytes
                    break;
                    //case 2: // SHA512
                    //    _hash = AssetBundlesHash.SHA512Complier(bytesFile);  // Hash bytes
                    //    break;
            }
            // Debug.Log(Application.dataPath + perfabFolder);
            // Debug.Log(Path.GetFileName(Path.GetDirectoryName(file)) + "/" + Path.GetFileName(file));
            dictBundles.Add(path, _hash);//把hash過的值存入字典檔
        }

        return dictBundles;
    }

    #endregion


    private static Dictionary<string, object> LoadFile(string path)   // 載入JSON列表檔案
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

    private static string LoadTxt(string path)   // 載入JSON列表檔案
    {
        // path = 資料夾物件下BundleInfo.json
        // 載入文字後並解析存入字典檔
        string text = File.ReadAllText(path);

        if (text != null)
            return text;

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
            Debug.Log("file Path:" + file.Substring(System.Environment.CurrentDirectory.ToString().Replace(@"\", "/").Length + 1));
            string filePath = file.Substring(System.Environment.CurrentDirectory.ToString().Replace(@"\", "/").Length + 1);
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


    private static void BuildHash(string assetBundlePath) //建立JSON檔案
    {
        Hash128 hash;
        uint crc;

        BuildPipeline.GetHashForAssetBundle(assetBundlePath, out hash);
        BuildPipeline.GetCRCForAssetBundle(assetBundlePath, out crc);

        Debug.Log(hash + "   " + crc);

    }



}
