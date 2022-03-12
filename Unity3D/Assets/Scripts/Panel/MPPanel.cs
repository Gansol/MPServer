﻿using UnityEngine;
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
 * 20171016 v1.0.1  加入    MPGame
 * ****************************************************************/

public abstract class MPPanel : MonoBehaviour
{
    protected static AssetLoader assetLoader;
    protected MPGame m_MPGame;
    public static GameObject _lastEmptyItemGroup;
    private static Dictionary<string, GameObject> _dictActor = new Dictionary<string, GameObject>(); // 已載入角色參考
    private GameObject _tmpActor, _clone;

    public MPPanel( MPGame MPGame)
    {
        m_MPGame = MPGame;
        assetLoader = m_MPGame.AssetLoader();
        Debug.Log(assetLoader.name);
    }

    #region -- LoadActor 載入老鼠角色 --
    /// <summary>
    /// 
    /// </summary>
    /// <param name="btn_click"></param>
    /// <param name="parent"></param>
    /// <param name="scale"></param>
    /// <returns>true:開始載入Asset false:已載入Asset</returns>
    public bool LoadActor(GameObject btn_click, Transform parent, Vector3 scale)
    {
        GameObject _miceImage;
        UISprite sprite = btn_click.transform.GetComponentInChildren<UISprite>();
        string assetName = sprite.spriteName.Remove(sprite.spriteName.Length - 4);
//        Debug.Log(_dictActor.Count);
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
        GameObject bundle = (GameObject)assetLoader.GetAsset(actorName);

        if (bundle != null)                  // 已載入資產時
        {
            if (!GetLoadedActor(bundle.name))
                _clone = MPGFactory.GetObjFactory().InstantiateActor(bundle, parent.transform, actorName, scale, 500); // 老鼠Depth是手動輸入的!! 錯誤
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


    #region -- InstantiateICON 實體化背包物件背景--
    /// <summary>
    /// 實體化商店物件背景
    /// </summary>
    /// <param name="itemData">資料字典</param>
    /// <param name="itemPanel">實體化父系位置</param>
    /// <param name="itemType">道具類別</param>
    public Dictionary<string, GameObject> InstantiateICON(Dictionary<string, object> itemData, string itemName, Transform itemPanel, Vector2 offset, int tableCount, int rowCount)
    {
        if (itemPanel.transform.childCount == 0)                // 如果還沒建立道具
        {
            _lastEmptyItemGroup = CreateEmptyObject(itemPanel, 1);
            Vector2 pos = new Vector2();
            Dictionary<string, GameObject> dictItem = new Dictionary<string, GameObject>();

            int i = 0;
            foreach (KeyValuePair<string, object> item in itemData)
            {
                if (assetLoader.GetAsset(itemName) != null)                  // 已載入資產時
                {

                    GameObject bundle = assetLoader.GetAsset(itemName);
                    pos = sortItemPos(12, 5, offset, pos, i);
                    bundle = MPGFactory.GetObjFactory().Instantiate(bundle, _lastEmptyItemGroup.transform, item.Key, new Vector3(pos.x, pos.y), Vector3.one, Vector2.zero, -1);

                    string iconName = item.Value + "ICON";
                    if (assetLoader.GetAsset(iconName) != null)                  // 已載入資產時
                    {
                        GameObject icon = assetLoader.GetAsset(iconName);
                        bundle = MPGFactory.GetObjFactory().Instantiate(icon, bundle.transform.FindChild("Image"), item.Key, Vector3.zero, Vector3.one, Vector2.zero, -1);
                        bundle.transform.parent.parent.gameObject.tag = "InventoryICON";
                        GameObject.Destroy(bundle.transform.parent.parent.GetComponent<ButtonSwitcher>());
                        GameObject.Destroy(bundle.transform.parent.parent.GetComponent<UIDragScrollView>());
                        GameObject.Destroy(bundle.transform.parent.parent.GetComponent<Rigidbody>());
                        GameObject.Destroy(bundle.transform.parent.parent.GetComponent<UIDragObject>());
                        bundle.transform.parent.parent.gameObject.AddComponent<ChangeICON>();
                    }
                    pos.x += offset.x;
                }

                i++;
            }
        }
        return null;
    }

    #endregion

    #region -- InstantiateItem 實體化背包物件背景--
    /// <summary>
    /// 實體化商店物件背景
    /// </summary>
    /// <param name="itemData">資料字典</param>
    /// <param name="itemPanel">實體化父系位置</param>
    /// <param name="itemType">道具類別</param>
    public virtual Dictionary<string, GameObject> InstantiateItem(Dictionary<string, object> itemData, string itemName, int itemType, Transform itemPanel, Vector2 offset, int tableCount, int rowCount)
    {
        if (itemType != -1)
            itemData = MPGFactory.GetObjFactory().GetItemInfoFromType(itemData, itemType);     // 取得對應道具類別資料

        if (itemPanel.transform.childCount == 0)                // 如果還沒建立道具
        {
            _lastEmptyItemGroup = CreateEmptyObject(itemPanel, itemType);
            return InstantiateItem2(itemData, itemName, itemType, _lastEmptyItemGroup.transform, itemData.Count, offset, tableCount, rowCount);
        }                                                       // 已建立道具時
        else
        {
            if (itemPanel.FindChild(itemType.ToString()))       // 如果有對應道具類別
            {
                _lastEmptyItemGroup.SetActive(false);
                _lastEmptyItemGroup = itemPanel.FindChild(itemType.ToString()).gameObject;
                _lastEmptyItemGroup.SetActive(true);
            }                                                   // 如果沒有對應道具類別資料 建立道具
            else if (_lastEmptyItemGroup != itemPanel.FindChild(itemType.ToString()))
            {
                _lastEmptyItemGroup.SetActive(false);
                _lastEmptyItemGroup = CreateEmptyObject(itemPanel, itemType);
                return InstantiateItem2(itemData, itemName, itemType, _lastEmptyItemGroup.transform, itemData.Count, offset, tableCount, rowCount);
            }
        }

        return null;
    }

    private Dictionary<string, GameObject> InstantiateItem2(Dictionary<string, object> itemData, string itemName, int itemType, Transform parent, int itemCount, Vector2 offset, int tableCount, int rowCount)
    {
        Vector2 pos = new Vector2();
        Dictionary<string, GameObject> dictItem = new Dictionary<string, GameObject>();
        int count = parent.childCount, i = 0;

        foreach (KeyValuePair<string, object> item in itemData)
        {
            var nestedData = item.Value as Dictionary<string, object>;
            object itemID;
            nestedData.TryGetValue("ItemID", out itemID);

            if (assetLoader.GetAsset(itemName) != null)                  // 已載入資產時
            {
                pos = sortItemPos(tableCount, rowCount, offset, pos, count + i);
                GameObject bundle = assetLoader.GetAsset(itemName);

                bundle = MPGFactory.GetObjFactory().Instantiate(bundle, parent, itemID.ToString(), new Vector3(pos.x, pos.y), Vector3.one, Vector2.zero, -1);
                if (bundle != null) dictItem.Add(itemID.ToString(), bundle);    // 存入道具資料索引
                pos.x += offset.x;
            }
            i++;
        }

        return dictItem;
    }
    #endregion

    #region -- SortItemPos 排序道具位置  --
    /// <summary>
    /// 排序道具位置
    /// </summary>
    /// <param name="xCount">第一頁最大數量</param>
    /// <param name="yCount">每行道具數量</param>
    /// <param name="offset">目前物件位置</param>
    /// <param name="pos">初始位置</param>
    /// <param name="counter">計數</param>
    /// <returns>物件位置</returns>
    public Vector2 sortItemPos(int xCount, int yCount, Vector2 offset, Vector2 pos, int counter)
    {
        // 物件位置排序
        if (counter % xCount == 0 && counter != 0) // 3 % 9 =0
        {
            pos.x = offset.x * 3;
            pos.y = 0;
        }
        else if (counter % yCount == 0 && counter != 0)//3 3 =0
        {
            pos.y += offset.y;
            pos.x = 0;
        }
        return pos;
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
            string imageName = MPGFactory.GetObjFactory().GetColumnsDataFromID(itemNameData, "ItemName", item.Key.ToString()).ToString();

            if (!string.IsNullOrEmpty(imageName) && imageName != "-1" && assetLoader.GetAsset(imageName) == null)
            {
                dictNotLoadedAsset.Add(item.Key.ToString(), imageName);
            }
        }
        return dictNotLoadedAsset;
    }
    #endregion

    // 復原視窗焦點
    protected void ResumeToggleTarget()
    {
        EventMaskSwitch.Resume();
        GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().Panel[5].SetActive(false);
        EventMaskSwitch.Switch(gameObject, false);
        EventMaskSwitch.lastPanel = gameObject;
    }

    public void SetLoadedActor(GameObject actor)
    {
        if (!_dictActor.ContainsKey(actor.name))    // FUCK   del MonoBehaviour name> actor.name
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


    protected abstract void OnLoading();

    protected abstract void OnLoadPanel();
}
