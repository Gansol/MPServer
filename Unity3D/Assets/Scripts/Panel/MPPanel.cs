using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
/* ***************************************************************
 * -----Copyright © 2015 Gansol Studio.  All Rights Reserved.-----
 * -----------            CC BY-NC-SA 4.0            -------------
 * -----------  @Website:  EasyUnity@blogspot.com    -------------
 * -----------  @Email:    GansolTW@gmail.com        -------------
 * -----------  @Author:   Krola.                    -------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * MPPanel 為Panel Base類
 * ***************************************************************
 *                           ChangeLog
 * 20161102 v1.0.0  建立    MPPanel  Base類                                  
 * ****************************************************************/

public abstract class MPPanel : MonoBehaviour
{
    public AssetLoader assetLoader;

    private static Dictionary<string, GameObject> _dictActor = new Dictionary<string, GameObject>(); // 已載入角色參考
    private GameObject _tmpActor, _clone;

    void Awake()
    {
        assetLoader = gameObject.AddMissingComponent<AssetLoader>();
    }

    #region -- LoadActor 載入老鼠角色 --
    public bool LoadActor(GameObject btn_click, Transform parent, Vector3 scale)
    {
        GameObject _miceImage;
        string assetName = btn_click.transform.GetComponentInChildren<UISprite>().name;
        Debug.Log(_dictActor.Count);
        //if (tmpActor != null) tmpActor.SetActive(false);          // 如果暫存老鼠圖片不是空的(防止第一次點擊出錯)，將上一個老鼠圖片隱藏

        if (_dictActor.TryGetValue(assetName, out _miceImage))       // 假如已經載入老鼠圖片了 直接顯示
        {
            if (_tmpActor != null) _tmpActor.SetActive(false);
            _miceImage.transform.parent = parent;
            _miceImage.gameObject.layer = parent.gameObject.layer;
            _miceImage.transform.localPosition = Vector3.zero;
            _miceImage.transform.localScale = scale;
            _miceImage.SetActive(true);
            _tmpActor = _miceImage;
            return false;
        }
        else
        {
            assetLoader.init();
            assetLoader.LoadAsset(assetName + "/", assetName);
            assetLoader.LoadPrefab(assetName + "/", assetName);
            return true;
        }
    }
    #endregion

    #region -- InstantiateActor 實體化老鼠角色 --
    public bool InstantiateActor(string actorName, Transform parent, Vector3 scale)
    {
        ObjectFactory insObj = new ObjectFactory();
        GameObject bundle = (GameObject)assetLoader.GetAsset(actorName);

        if (bundle != null)                  // 已載入資產時
        {
            if (!GetLoadedActor(bundle.name))
                _clone = insObj.InstantiateActor(bundle, parent.transform, actorName, scale);
            SetLoadedActor(_clone);
            return false;
        }
        else
        {
            Debug.LogError("Assetbundle reference not set to an instance. at InstantiateActor.");
            return true;
        }
    }
    #endregion

    #region -- LoadIconObject 載入載入ICON物件 --
    /// <summary>
    /// 載入ICON物件 by BundleName
    /// </summary>
    /// <param name="itemData">物件陣列</param>
    /// <param name="folder">資料夾名稱(不含/)</param>
    /// <param name="bKeyOrValue">0=keyName;1=valueName</param>
    public bool LoadIconObject(Dictionary<string, object> itemData, string folder)    // 載入遊戲物件
    {
        if (itemData != null)
        {
            foreach (KeyValuePair<string, object> item in itemData)
            {
                if (!string.IsNullOrEmpty(item.Value.ToString())) assetLoader.LoadPrefab(folder + "/", item.Value.ToString() + "ICON");
            }
            return true;
        }
        else
        {
            Debug.Log("LoadIconObject:itemData is Null !!");
            return false;
        }
    }
    #endregion


    #region -- CreateEmptyObject 建立空物件 --
    /// <summary>
    /// 建立空物件群組
    /// </summary>
    /// <param name="parent">上層物件</param>
    /// <param name="itemType">群組類型(名稱)</param>
    /// <returns></returns>
    public GameObject CreateEmptyObject(Transform parent, int itemType)
    {
        GameObject emptyGroup = new GameObject(itemType.ToString());   // 商品物件空群組
        emptyGroup.transform.parent = parent;
        emptyGroup.layer = parent.gameObject.layer;
        emptyGroup.transform.localPosition = Vector3.zero;
        emptyGroup.transform.localScale = Vector3.one;
        return emptyGroup;
    }
    #endregion

    #region -- GetItemInfoFromType 取得道具(類別)資訊  --
    public Dictionary<string, object> GetItemInfoFromType(Dictionary<string, object> itemData, int type)
    {
        Dictionary<string, object> data = new Dictionary<string, object>();

        foreach (KeyValuePair<string, object> item in itemData)
        {
            var nestedData = item.Value as Dictionary<string, object>;
            object itemType;
            nestedData.TryGetValue("ItemType", out itemType);
            if (itemType != null)
                if (itemType.ToString() == type.ToString())
                {
                    data.Add(nestedData["ItemID"].ToString(), nestedData);
                }
        }
        return data;
    }
    #endregion


    #region -- GetItemInfoFromType 取得道具(類別)資訊  --
    public Dictionary<string, object> GetItemInfoFromID(Dictionary<string, object> itemData,int type)
    {
        Dictionary<string, object> data = new Dictionary<string, object>();

        foreach (KeyValuePair<string, object> item in itemData)
        {
            var nestedData = item.Value as Dictionary<string, object>;
            object itemID;
            nestedData.TryGetValue("ItemID", out itemID);
            

            char[] itemType = itemID.ToString().ToCharArray(0,1);
            if (itemType[0].ToString() == type.ToString())
                {
                    data.Add(nestedData["ItemID"].ToString(), nestedData);
                }
        }
        return data;
    }
    #endregion

    #region -- GetItemNameFromID 從道具ID取得道具名稱 --
    /// <summary>
    /// 從道具ID取得道具名稱
    /// </summary>
    /// <param name="itemName">道具名稱</param>
    /// <param name="itemData">2d Dictionary</param>
    /// <returns>itemName</returns>
    public string GetItemNameFromID(string itemID, Dictionary<string, object> itemData)
    {
        object objNested;

        itemData.TryGetValue(itemID.ToString(), out objNested);
        if (objNested != null)
        {
            var dictNested = objNested as Dictionary<string, object>;
            return dictNested["ItemName"].ToString();
        }
        return "";
    }
    #endregion

    #region -- GetItemNameFromID 從道具名稱取得道具ID --
    /// <summary>
    /// 從道具名稱取得道具ID
    /// </summary>
    /// <param name="itemName">道具名稱</param>
    /// <param name="itemData">2d Dictionary</param>
    /// <returns>itemName</returns>
    public string GetItemIDFromName(string itemName, Dictionary<string, object> itemData)
    {
        object value;
        foreach (KeyValuePair<string, object> item in itemData)
        {
            var nestedData = item.Value as Dictionary<string, object>;
            nestedData.TryGetValue("ItemName", out value);
            if (itemName == value.ToString())
            {
                nestedData.TryGetValue("ItemID", out value);
                return value.ToString();
            }
        }
        return "";
    }
    #endregion

    #region -- GetDontNotLoadAsset 取得未載入Asset --
    /// <summary>
    /// 取得未載入Asset
    /// </summary>
    /// <param name="ServerDict"></param>
    /// <param name="itemNameData">name Data</param>
    /// <returns></returns>
    public Dictionary<string, object> GetDontNotLoadAsset(Dictionary<string, object> ServerDict)
    {
        Dictionary<string, object> dictNotLoadedAsset = new Dictionary<string, object>();

        foreach (KeyValuePair<string, object> item in ServerDict)       // 取得未載入物件
        {
            string imageName = item.Value.ToString();

            if (!string.IsNullOrEmpty(imageName) && assetLoader.GetAsset(imageName) == null)
            {
                dictNotLoadedAsset.Add(imageName, imageName);
            }
        }
        return dictNotLoadedAsset;
    }

    /// <summary>
    /// 取得未載入Asset (nestedDict)
    /// </summary>
    /// <param name="ServerDict"></param>
    /// <param name="itemNameData">name Data</param>
    /// <returns></returns>
    public Dictionary<string, object> GetDontNotLoadAsset(Dictionary<string, object> ServerDict, Dictionary<string, object> itemNameData)
    {
        Dictionary<string, object> dictNotLoadedAsset = new Dictionary<string, object>();

        foreach (KeyValuePair<string, object> item in ServerDict)       // 取得未載入物件
        {
            string imageName = GetItemNameFromID(item.Key, itemNameData);

            if (!string.IsNullOrEmpty(imageName) && assetLoader.GetAsset(imageName) == null)
            {
                dictNotLoadedAsset.Add(imageName, imageName);
            }
        }
        return dictNotLoadedAsset;
    } 
    #endregion

    public void SetLoadedActor(GameObject actor)
    {
        if (!_dictActor.ContainsKey(name))
            _dictActor.Add(actor.name, actor);

        if (_tmpActor != null)
            _tmpActor.SetActive(false);

        _tmpActor = actor;
    }

    public bool GetLoadedActor(string actorName)
    {
        if (_dictActor.ContainsKey(actorName))
            return true;
        else
            return false;
    }


    public abstract void OnLoading();

    protected abstract void OnLoadPanel();
}
