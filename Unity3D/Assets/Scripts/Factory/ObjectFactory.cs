using System;
using System.Collections.Generic;
using UnityEngine;
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
 * ***************************************************************
 *                           ChangeLog
 * 20160914 v1.0.0 新增實體化物件、角色                         
 * ****************************************************************/
public class ObjectFactory : IFactory
{
    GameObject _clone;
    SpawnAI spawnAI;
    //MPGame m_MPGame;
    public ObjectFactory()
    {
     //   m_MPGame = MPGame.Instance;
     //   spawnAI = new SpawnAI();
    }

    //public void TestMethod()
    //{
    //    Debug.Log("ObjectFactory Init!");
    //}

    #region -- Instantiate 實體化物件 --
    /// <summary>
    /// 實體化物件
    /// </summary>
    /// <param name="bundle">實體化物件</param>
    /// <param name="parent">上層</param>
    /// <param name="name">名稱</param>
    /// <param name="localPosition">位置</param>
    /// <param name="localScale">縮放</param>
    /// <param name="spriteScale">2D圖形縮放(Witdh Height) Vetor2.zero = not scale</param>
    /// <param name="depth">深度值 -1=不改變</param>
    /// <returns></returns>
    public GameObject Instantiate(GameObject bundle, Transform parent, string name, Vector3 localPosition, Vector3 localScale, Vector2 spriteScale, int depth)
    {
        try
        {
            if (bundle != null)
            {
                _clone = GameObject.Instantiate(bundle);             // 實體化
                DepthManager.SwitchDepthLayer(_clone, parent, depth);
                _clone.transform.parent = parent;
                _clone.name = name;

                _clone.transform.localPosition = localPosition;
                _clone.transform.localScale = localScale;


                if (spriteScale != Vector2.zero && _clone.GetComponent<UISprite>() != null)
                {
                    _clone.GetComponent<UISprite>().width = System.Convert.ToInt32(spriteScale.x);
                    _clone.GetComponent<UISprite>().height = System.Convert.ToInt32(spriteScale.y);
                }
                return _clone;
            }
        }
        catch
        {
            throw;
        }
        Debug.Log("  Instantiate bundle is null !!!! " + name);
        return null;
    }
    #endregion

    #region -- InstantiateActor 實體化老鼠角色  --
    /// <summary>
    /// 實體化老鼠角色
    /// </summary>
    /// <param name="bundle">實體化物件</param>
    /// <param name="parent">上層</param>
    /// <param name="name">名稱</param>
    /// <param name="sacle">縮放</param>
    /// <param name="depth">深度</param>
    /// <returns></returns>-
    public GameObject InstantiateActor(GameObject bundle, Transform parent, string name, Vector3 sacle, int depth=500)      // 老鼠Depth是手動輸入的!! 錯誤
    {
        _clone = Instantiate(bundle, parent, name, Vector3.zero, sacle, Vector2.zero, -1);
        _clone.SetActive(false);
        DepthManager.SwitchDepthLayer(_clone, parent, depth);
        _clone.SetActive(true);

        return _clone;
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

    //#region -- InstantiateMice 實體化老鼠 --
    ///// <summary>
    ///// 產生老鼠 還不完整
    ///// </summary>
    ///// <param name="poolManager"></param>
    ///// <param name="miceID"></param>
    ///// <param name="miceSize"></param>
    ///// <param name="hole"></param>
    ///// <param name="impose">強制產生</param>
    ///// <returns></returns>
    //public GameObject InstantiateMice(short miceID, float miceSize, GameObject hole, bool impose)
    //{
    //    Vector3 _miceSize;
    //    // 如果老鼠洞是打開的 或 強制產生
    //    if (hole.GetComponent<HoleState>().holeState == HoleState.State.Open || impose)
    //    {
    //        // 如果強制產生 且 老鼠在存活列表中 強制將老鼠死亡
    //        if (impose && m_MPGame.GetPoolSystem().GetActiveMice(hole.transform))
    //         /   m_MPGame.GetPoolSystem().GetMiceRefs(hole.transform).GetComponentInChildren<IMice>().SendMessage("OnDead", 0.0f); //錯誤

    //        // 強制移除老鼠 沒有發送死亡訊息
    //        if (m_MPGame.GetPoolSystem().GetActiveMice(hole.transform))      // 錯誤 FUCK  還沒始就強制移除索引
    //            m_MPGame.GetPoolSystem().RemoveMiceRefs(hole.transform); //錯誤

    //        // 取得物件池老鼠
    //        GameObject clone = m_MPGame.GetPoolSystem().ActiveObject(miceID.ToString());

    //        // 如果物件池老鼠不是空的 產生老鼠
    //        if (clone != null)
    //        {
    //            hole.GetComponent<HoleState>().holeState = HoleState.State.Closed;
    //            _miceSize = hole.transform.Find("ScaleValue").localScale / 10 * miceSize;   // Scale 版本
    //            clone.transform.parent = hole.transform;              // hole[-1]是因為起始值是0 
    //            clone.layer = hole.layer;
    //            clone.transform.localPosition = Vector2.zero;
    //            clone.transform.localScale = hole.transform.GetChild(0).localScale - _miceSize;  // 公式 原始大小分為10等份 10等份在減掉 要縮小的等份*乘洞的倍率(1.4~0.9) => 1.0整份-0.2份*1(洞口倍率)=0.8份 
    //            //clone.GetComponent<BoxCollider2D>().enabled = true;

    //            clone.GetComponent<ICreature>().Play(IAnimatorState.ENUM_AnimatorState.Hello);
    //            m_MPGame.GetPoolSystem().AddMiceRefs(clone.transform.parent, clone);
    //            clone.transform.gameObject.SetActive(false);
    //            clone.transform.gameObject.SetActive(true);
    //            return clone;
    //        }
    //    }
    //    return null;
    //}
    //#endregion

    //#region -- SpawnBoss --
    //public void SpawnBoss( GameObject hole, short miceID, float lerpSpeed, float lerpTime, float upSpeed, float upDistance)// 怪怪的 程式碼太長 錯誤
    //{
    //    Debug.Log("------------------- Mice Boss ID:  " + miceID + " ------------------------");
    //    try
    //    {
    //        // 如果Hole上有Mice 移除Mice
    //        if (hole.GetComponent<HoleState>().holeState == HoleState.State.Closed)
    //        {
    //            m_MPGame.GetPoolSystem().RemoveMiceRefs(hole.transform);
    //            if (hole.transform.GetComponentInChildren<Mice>())
    //                m_MPGame.GetPoolSystem().
    //            /    hole.transform.GetComponentInChildren<Mice>().gameObject.SendMessage("OnDead", 0.0f);
    //        }

    //        // 播放洞口動畫
    //        hole.GetComponent<Animator>().enabled = true;
    //       hole.GetComponent<Animator>().Play("Layer1.HoleScale", -1, 0f);

    //        // 產生MicBoss 並移除Mice Component

    //        GameObject clone = m_MPGame.GetPoolSystem().ActiveObject(miceID.ToString());
    //        // MiceBase mice = clone.GetComponent(typeof(MiceBase)) as MiceBase;
    //        MiceAttr miceAttr = MPGFactory.GetAttrFactory().GetMiceProperty(miceID.ToString());

    //        // if (mice.enabled) mice.enabled = false;
    //        GameObject.Destroy(clone.GetComponent<IMice>());

    //        clone.transform.gameObject.SetActive(false);
    //        clone.transform.parent =hole.transform;
    //        clone.transform.localScale = new Vector3(1.3f, 1.3f, 0f);
    //        clone.transform.localPosition = new Vector3(0, 0, 0);
    //        //   clone.AddComponent(clone, miceAttr.name + "Boss"); //需要改寫 新增BOSS PREFEB 初始就附加上去
    //        Debug.Log("--------------------miceAttr.name" + miceAttr.name);
    //        string titleUpcase = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(miceAttr.name.Replace("mice", "").ToLower()); // 第一個字母大寫
    //        Type t = Type.GetType(titleUpcase + "MiceBoss");
    //        clone.AddComponent(t);
    //        MiceBossBase bossBase = clone.GetComponent(t) as MiceBossBase;
    //        bossBase.enabled = true;

    //        // 初始化 MiceBoss數值

    //        MiceBossBase boss = clone.GetComponent(typeof(MiceBossBase)) as MiceBossBase;
    //        SkillBase skill = MPGFactory.GetSkillFactory().GetSkill(Global.miceProperty, miceID);
    //        MiceAnimState animState = new MiceAnimState(clone, true, lerpSpeed, miceAttr.MiceSpeed, upDistance, miceAttr.LifeTime);

    //        boss.SetArribute(miceAttr);
    //        boss.SetSkill(skill);
    //        boss.SetAnimState(animState);
    //        boss.Initialize(0.1f, 6f, 60f, miceAttr.LifeTime);  // 錯誤 可能非必要
    //        clone.SetActive(true);

    //        Debug.Log("Skill : " + MPGFactory.GetSkillFactory().GetSkill(Global.miceProperty, miceID));

    //        Debug.Log("----------------------------------------- " + clone.activeSelf + "----------------------------------------- ");
    //        // 加入老鼠陣列
    //        m_MPGame.GetPoolSystem().AddMiceRefs(clone.transform.parent, clone);
    //    }
    //    catch
    //    {
    //        throw;
    //    }
    //}
    //#endregion

    /// <summary>
    /// 從道具名稱取得道具ID
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="columns">ItemID SkillID etc</param>
    /// <param name="miceName"></param>
    /// <returns>null = -1</returns>
    #region -- GetIDFromName --
    public int GetIDFromName(Dictionary<string, object> dictionary, string columns, string miceName)
    {
        object value;
        foreach (KeyValuePair<string, object> item in dictionary)
        {
            var nestedData = item.Value as Dictionary<string, object>;
            nestedData.TryGetValue("ItemName", out value);
            if (miceName == value.ToString().ToLower())
            {
                nestedData.TryGetValue(columns, out value);
                return int.Parse(value.ToString());
            }
        }
        return -1;
    }
    #endregion

    /// <summary>
    /// 從特定ID取得欄位資料
    /// </summary>
    /// <param name="dictionary">字典資料</param>
    /// <param name="columns">欄位</param>c
    /// <param name="objectID">ID</param>
    /// <returns></returns>
    public T  GetColumnsDataFromID<T>(Dictionary<string, T> dictionary, string columns, T objectID)
    {

        if (dictionary.TryGetValue(objectID.ToString(), out T value))
        {
            Dictionary<string, T> dictSkill = value as Dictionary<string, T>;
            if (dictSkill.TryGetValue(columns, out T nestedValue))
                return nestedValue;
        }
        return   default(T);    //回傳 NULL會導致錯誤
    }

    #region -- GetItemInfoFromType 取得特定(類別)道具詳細資料  --
    public Dictionary<string, object> GetItemDetailsInfoFromType(Dictionary<string, object> itemData, int type)
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
    /// <summary>
    /// 取得道具(類別)資訊
    /// </summary>
    /// <param name="itemData">道具資料</param>
    /// <param name="columns">要取得資料的欄位</param>
    /// <param name="type">道具類型</param>
    /// <returns></returns>
    public Dictionary<string, object> GetItemInfoFromID(Dictionary<string, object> itemData, string columns, int type)
    {
        Dictionary<string, object> data = new Dictionary<string, object>();

        foreach (KeyValuePair<string, object> item in itemData)
        {
            var nestedData = item.Value as Dictionary<string, object>;
            object itemID;
            nestedData.TryGetValue(columns, out itemID);


            char[] itemType = itemID.ToString().ToCharArray(0, 1);
            if (itemType[0].ToString() == type.ToString())
            {
                data.Add(nestedData[columns].ToString(), nestedData);
            }
        }
        return data;
    }
    #endregion

    //#region -- GetItemNameFromID 從道具ID取得道具名稱 --
    ///// <summary>
    ///// 從道具ID取得道具名稱
    ///// </summary>
    ///// <param name="itemName">道具名稱</param>
    ///// <param name="itemData">2d Dictionary</param>
    ///// <returns>itemName</returns>
    //public static  string GetItemNameFromID(string itemID, Dictionary<string, object> itemData)
    //{
    //    object objNested;

    //    itemData.TryGetValue(itemID.ToString(), out objNested);
    //    if (objNested != null)
    //    {
    //        var dictNested = objNested as Dictionary<string, object>;
    //        return dictNested["ItemName"].ToString();
    //    }
    //    return "";
    //}
    //#endregion

    //#region -- GetItemIDFromName 從道具名稱取得道具ID --
    ///// <summary>
    ///// 從道具名稱取得道具ID
    ///// </summary>
    ///// <param name="itemName">道具名稱</param>
    ///// <param name="columns">ID欄位名稱</param>
    ///// <param name="itemData">2d Dictionary</param>
    ///// <returns></returns>
    //public static string GetItemIDFromName(string itemName,string columns, Dictionary<string, object> itemData)
    //{
    //    object value;
    //    foreach (KeyValuePair<string, object> item in itemData)
    //    {
    //        var nestedData = item.Value as Dictionary<string, object>;
    //        nestedData.TryGetValue("ItemName", out value);
    //        if (itemName == value.ToString())
    //        {
    //            nestedData.TryGetValue(columns, out value);
    //            return value.ToString();
    //        }
    //    }
    //    return "";
    //}
    //#endregion
}
