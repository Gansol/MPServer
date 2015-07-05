using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MiniJSON;

//目前只比較bytes差別，本檔案沒有判斷檔案關開等例外情況
public class CreateJSON : MonoBehaviour
{
    AssetBundlesHash bundleHash ;

    private string[] pathFiles;
    private byte[] bytesFile;

 
    public void AssetBundlesJSON() //建立 檔案列表
    {
        bundleHash = gameObject.AddComponent<AssetBundlesHash>();
        Dictionary<string, object> dictBundles = new Dictionary<string, object>();

        string hash;
        string itemListURL = Application.persistentDataPath + "/List/";
        string pathURL = Application.persistentDataPath + "/AssetBundles/";

        if (!System.IO.Directory.Exists(itemListURL))
            System.IO.Directory.CreateDirectory(itemListURL);

        if (!System.IO.Directory.Exists(pathURL))
            System.IO.Directory.CreateDirectory(pathURL);

        pathFiles = Directory.GetFiles(pathURL); //取得 本機資料夾 全部檔案

        
        foreach (string path in pathFiles) // 尋遍所有資料夾下 檔案路徑
        {
            bytesFile = File.ReadAllBytes(path); //讀取檔案bytes
            hash = bundleHash.SHA1Complier(bytesFile);//Hash bytes
            dictBundles.Add(Path.GetFileName(path), hash);//把hash過的值存入字典檔
        }
        CreateFile(Json.Serialize(dictBundles), itemListURL , Global.sItemList); //建立 新 檔案列表
    }


    protected void CreateFile(string contant,string path,string fileName) //建立檔案
    {
        if (!System.IO.Directory.Exists(path))
            System.IO.Directory.CreateDirectory(path);

        using (FileStream fs = File.Create(path + fileName)) //using 會自動關閉Stream 建立檔案
        {
            fs.Write(new UTF8Encoding(true).GetBytes(contant), 0, contant.Length); //寫入檔案
            fs.Dispose(); //避免錯誤 在寫一次關閉
        }
    }

    
    public IEnumerator DelList(HashSet<string> hashSet) //刪除列表值 並 重建
    {
        string pathURL = Application.persistentDataPath + "/List/";
        string _listText = File.ReadAllText( pathURL + Global.sItemList);

        Dictionary<string, object> dictJsonLocalList = MiniJSON.Json.Deserialize(_listText) as Dictionary<string, object>;//本機列表存入字典
        
        foreach (string bundles in hashSet) // 從字典檔中移除 要刪除的值
            dictJsonLocalList.Remove(bundles);

        yield return _listText;

        CreateFile(Json.Serialize(dictJsonLocalList), pathURL ,Global.sItemList); //再把字典檔建立 新 檔案列表
    }
     

}
