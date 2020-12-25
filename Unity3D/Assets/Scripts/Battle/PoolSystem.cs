﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using MiniJSON;
using System;
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
 * 物件池 提供一個高效率的物件生成、回收方式
 * 231行，EggMice和ID必須由程式控制 現在亂寫
 * 170~ lerpSpeed等 錯誤 應由伺服器
 * 173、技能使用量 錯誤 應由伺服器
 * ***************************************************************/

public class PoolSystem : IGameSystem
{
    private CreatureSystem m_CreatureSystem;
    private AssetLoaderSystem assetLoader;
    private GameObject m_RootUI;

    //private Dictionary<string, object> _tmpDict;
    private Dictionary<int, string> _dictMiceObject; // 老鼠資源列表
    private Dictionary<int, string> _dictSpecialObject;// 特殊老鼠資源列表
    private Dictionary<int, string> _dictSkillObject; // 技能資源列表


    //private Dictionary<int, ICreature> dictMiceRefs;

    //private Dictionary<int, GameObject> dictBossPool;

    // 10001 guid ICreature  objectpool 
    private Dictionary<string, Dictionary<string, ICreature>> dictMicePool; // 物件池老鼠


    //private GameObject clone;

    private float _lastTime;
    private float _currentTime;
    static readonly float lerpSpeed = 0.1f, upSpeed = 6, upDantance = 60;    // 錯誤 數值不應該在這裡 應該在實體化時輸入

    Dictionary<string, object> _dictSkillMice;


    [Tooltip("物件池位置")]
    private GameObject objectPool;


    [Tooltip("技能位置")]
    public GameObject skillArea;

    [Tooltip("產生數量")]
    [Range(3, 12)]
    public int spawnCount = 5;

    [Tooltip("物件池上限(各種類分開)")]
    [Range(3, 12)]
    public int clearLimit = 5;

    [Tooltip("清除間格時間")]
    [Range(10, 20)]
    public int clearTime = 15;

    [Tooltip("物件池保留量(各種類分開)")]
    [Range(2, 12)]
    public int reserveCount = 3;

    public bool MergeFlag { get; private set; }// 合併老鼠完成
= false;// 合併老鼠完成
    public bool PoolingFlag { get; private set; }// 初始化物件池
= false;// 初始化物件池
    public bool IsloadPlayerItem { get; private set; } = false;

    private Vector3 bossScale = new Vector3(0.7f, 0.7f, 0.7f);
    private Vector3 skillScale = new Vector3(0.6f, 0.6f, 0.6f);

    public PoolSystem(MPGame MPGame) : base(MPGame)
    {
        Debug.Log("--------------- PoolSystem Created ----------------");
        m_CreatureSystem = MPGame.GetCreatureSystem();
    }

    public override void Initialize()
    {
        Debug.Log("--------------- PoolSystem Initialize ----------------");
        Global.photonService.LoadPlayerItemEvent += OnLoadPlayerItem;
        Global.photonService.LoadPlayerItem(Global.Account);
        assetLoader = m_MPGame.GetAssetLoaderSystem();
        m_RootUI = GameObject.Find(Global.Scene.BattleAsset.ToString());
        Debug.Log(m_RootUI.name);
        objectPool = m_RootUI.transform.Find("Battle(Panel)").Find("ObjectPool").gameObject;
        Debug.Log(objectPool);
        _lastTime = 0;
        _currentTime = 0;
        clearTime = 10;
        PoolingFlag = false;      // 初始化物件池
        _dictMiceObject = new Dictionary<int, string>();
        _dictSpecialObject = new Dictionary<int, string>();
        _dictSkillObject = new Dictionary<int, string>();

        dictMicePool = new Dictionary<string, Dictionary<string, ICreature>>();

        _dictSpecialObject.Add(11001, "Bali");  // 亂寫
        _dictSpecialObject.Add(11002, "Much");
        _dictSpecialObject.Add(11003, "HeroMice");
        _dictSkillMice = new Dictionary<string, object>();

        // start 舊版位置 20201208
        MergeMice();
        LoadSkillData();
        LoadAssets();
    }


    /// <summary>
    /// 載入Battle所有必要資產
    /// </summary>
    private void LoadAssets()
    {
        // 載入 老鼠資產
        foreach (KeyValuePair<int, string> item in _dictMiceObject)
            assetLoader.LoadAssetFormManifest(Global.MicePath + item.Value + "/unique/" + item.Value + Global.ext);

        // 載入 特殊老鼠
        foreach (KeyValuePair<int, string> item in _dictSpecialObject)
            assetLoader.LoadAssetFormManifest(Global.CreaturePath + item.Value + Global.ext);

        // 載入 道具資產
        foreach (KeyValuePair<int, string> item in _dictSkillObject)
            assetLoader.LoadAssetFormManifest(Global.ItemIconUniquePath + Global.IconSuffix + item.Value + Global.ext);

        // 載入 特效資產
        foreach (KeyValuePair<int, string> item in _dictMiceObject)
        {
            int itemID = Convert.ToInt16(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.miceProperty, "ItemID", item.Key.ToString()));
            string itemName = MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.itemProperty, "ItemName", itemID.ToString()).ToString();
            Debug.Log("Item Name:  " + itemName);
            //if(item.Key!=10001)
            assetLoader.LoadAssetFormManifest(Global.EffectsUniquePath + Global.EffectSuffix + itemName + Global.ext);
        }

    }


    public override void Update()
    {
        //if (!_poolingFlag && !string.IsNullOrEmpty(assetLoader.ReturnMessage))
        //    Debug.Log("Message:" + assetLoader.ReturnMessage /*+ "_loadedCount:" + assetLoader._loadedCount + "_objCount:" + assetLoader._objCount*/);
        InitPool();
        ClearPool();
        _currentTime = Time.time;
    }


    /// <summary>
    /// 初始化物件池
    /// </summary>
    private void InitPool()
    {
        if (assetLoader.bLoadedObj && IsloadPlayerItem && MergeFlag && !PoolingFlag)
        {
            _dictSkillMice = Global.dictTeam;

            InstantiateObjects(_dictMiceObject);
            // Debug.Log("Instantiate Mice Completed!");
            InstantiateObjects(_dictSpecialObject);
            //  Debug.Log("Instantiate SpecialMice Completed!");
            InstantiateSkillMice(_dictSkillMice);
            PoolingFlag = true;
            //  Debug.Log("Pooling Mice Completed ! " + _poolingFlag);

            Global.photonService.SendRoomMice(Global.RoomID, _dictMiceObject.Keys.Select(x => (x).ToString()).ToArray());
        }
    }

    /// <summary>
    /// 定時清除物件池
    /// </summary>
    private void ClearPool()
    {
        if (_currentTime - _lastTime > clearTime)     // 達到清除時間時
        {
            for (int i = 0; i < objectPool.transform.childCount; i++)       // 跑遍動態池
            {
                if (objectPool.transform.GetChild(i).childCount > clearLimit)           // 如果動態池超過限制數量
                {
                    for (int j = 0; j < objectPool.transform.GetChild(i).childCount - reserveCount; j++)    // 銷毀物件
                    {
                        GameObject.Destroy(objectPool.transform.GetChild(i).GetChild(j).gameObject);
                    }
                }
            }
            _lastTime = _currentTime;
        }
    }

    void InstantiateObjects(Dictionary<int, string> objectData)
    {

        foreach (KeyValuePair<int, string> item in objectData)
            Instantiate(item.Key.ToString(), assetLoader.GetAsset(item.Value));
    }

    void InstantiateSkillMice(Dictionary<string, object> objectData)
    {
        int i = 0;
        float lerpSpeed = 0.1f;
        float upDistance = 30f;
        GameObject clone;
        // int energyValue = 0;

        foreach (KeyValuePair<string, object> item in objectData)
        {
            if (assetLoader.GetAsset(item.Value.ToString().ToLower()) != null)
            {
                GameObject bundle = assetLoader.GetAsset(item.Value.ToString().ToLower());
                Vector3 scale = Vector3.zero;
                Transform parent = skillArea.transform.GetChild(i);
                clone = new GameObject();

                scale = (i == 4) ? bossScale : skillScale;

                // instantiate skill btn
                clone = MPGFactory.GetObjFactory().Instantiate(bundle, parent, item.Key, Vector3.zero, scale, Vector2.zero, 100);
                GameObject.Destroy(clone.GetComponent<BoxCollider2D>());
                clone.transform.parent.gameObject.AddComponent<SkillBtn>();
                clone.transform.parent.gameObject.GetComponent<SkillBtn>().init(Convert.ToInt16(item.Key), lerpSpeed * (i + 1), upDistance, i);
                clone.transform.parent.gameObject.AddComponent<UIButton>();
                clone.transform.parent.gameObject.AddComponent<BoxCollider2D>();
                clone.transform.parent.gameObject.GetComponent<BoxCollider2D>().size = new Vector2(300, 250);
                //EventDelegate.Set(clone.GetComponent<UIButton>().onClick, clone.GetComponent<SkillBtn>().OnClick);


                // instantiate SkillItem btn
                int itemID = -1;
                string itemName = "";

                itemID = Convert.ToInt16(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.miceProperty, "ItemID", item.Key.ToString()));
                itemName = MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.itemProperty, "ItemName", itemID.ToString()).ToString();

                if (!string.IsNullOrEmpty(itemName))
                {
                    if (assetLoader.GetAsset(Global.IconSuffix + itemName) != null)
                    {
                        bundle = assetLoader.GetAsset(Global.IconSuffix + itemName);
                        clone = MPGFactory.GetObjFactory().Instantiate(bundle, parent, itemName.ToString(), Vector3.zero, Vector3.one, new Vector2(180, 180), 100);
                        clone.SetActive(false);
                        //EventDelegate.Set(clone.GetComponent<UIButton>().onClick, clone.GetComponent<ItemBtn>().OnClick);
                    }
                    else
                    {
                        Debug.LogError("*****************Item Bundle is Null!***********************");
                    }
                }
                else
                {
                    Debug.LogError("*****************Item Name is Null!***********************");
                }

                // load skillTimes
                Dictionary<string, object> miceProp;

                Global.miceProperty.TryGetValue(item.Key, out object value);
                miceProp = value as Dictionary<string, object>;
                miceProp.TryGetValue("SkillTimes", out value);

                parent.GetChild(0).GetComponentInChildren<UILabel>().text = "0 / " + value.ToString();
                parent.transform.gameObject.SetActive(true);
                i++;
            }
            else
            {
                Debug.LogError("bundle is null!");
            }

        }

        Debug.Log("InstantiateSkillMice Completed!");
    }

    ///// <summary>
    ///// 每一次顯示一個GameObject。如果GameObject不足，Spawn一個物件並顯示。
    ///// </summary>
    ///// <param name="objectID">使用Name找Object</param>
    ///// <returns>回傳 ( GameObject / null )</returns>
    //public GameObject ActiveObject(string objectID)
    //{
    //    //Debug.Log("_dictObject.Count:" + _dictObject.Count);
    //    //int objectID = _dictObject.FirstOrDefault(x => x.Value == objectName).Key;       // 找Key

    //    if (!objectPool.transform.Find(objectID))
    //    {
    //        Debug.Log("Pooling can't find :" + objectID);
    //    }
    //    else if (objectPool.transform.Find(objectID).childCount == 0)
    //    {
    //        string bundleName = (string)MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.miceProperty, "ItemName", objectID.ToString());
    //        Instantiate(objectID, assetLoader.GetAsset(bundleName));
    //    }
    //    else
    //    {
    //        for (int i = 0; i < objectPool.transform.Find(objectID).childCount; i++)
    //        {
    //            GameObject clone = objectPool.transform.Find(objectID).GetChild(i).gameObject;
    //            IMice mice =   clone.GetComponent(typeof(IMice)) as IMice;
    //            MiceAttr miceAttr = MPGFactory.GetAttrFactory().GetMiceProperty(objectID);
    //            // ICreatureAI miceAI =
    //            if (mice.enabled == false) mice.enabled = true;

    //            if (clone.name == objectID && !clone.gameObject.activeSelf)
    //            {
    //                clone.SetActive(true);
    //                miceAttr.SetHP(1);
    //                mice.SetArribute(miceAttr);
    //                mice.SetAI(new MiceAI(mice));
    //                mice.Initialize();
    //                clone.transform.GetChild(0).localScale = Vector3.one;
    //                clone.transform.GetChild(0).localRotation = Quaternion.identity;
    //                clone.transform.GetChild(0).localPosition = Vector3.zero;

    //                if (clone.GetComponent<BoxCollider2D>()) clone.GetComponent<BoxCollider2D>().enabled = true;

    //                clone.SetActive(false);
    //                clone.SetActive(true);
    //                return clone;
    //            }
    //        }
    //    }
    //    return null;
    //}

    public ICreature ActiveMice(string miceID, Transform hole)
    {
        string hashID = "";
        ICreature creature = null;
        if (dictMicePool[miceID].Count > 0)
        {
            foreach (KeyValuePair<string, ICreature> mice in dictMicePool[miceID])
            {
                hashID = mice.Key;
                creature = mice.Value;

                if (mice.Value.m_go.GetComponent<BoxCollider2D>())
                    mice.Value.m_go.GetComponent<BoxCollider2D>().enabled = true;

                m_MPGame.GetCreatureSystem().AddMiceRefs(hole,miceID, hashID, mice.Value);
           //     m_MPGame.GetCreatureSystem().AddHoleMiceRefs(hole, mice.Value);

                break;
            }
            dictMicePool[miceID].Remove(hashID);
            creature.Initialize();
            return creature;
        }
        else
        {
            string bundleName = (string)MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.miceProperty, "ItemName", miceID.ToString());
            Instantiate(miceID, assetLoader.GetAsset(bundleName));
            ActiveMice(miceID, hole); // 可能造成無線迴圈 會和下面一起造成產生了卻沒被使用的情況
        }
        return null; // 錯誤 回傳空的
    }


    #region -- InstantiateMice 實體化老鼠 --
    /// <summary>
    /// 產生老鼠 還不完整
    /// </summary>
    /// <param name="poolManager"></param>
    /// <param name="miceID"></param>
    /// <param name="miceSize"></param>
    /// <param name="hole"></param>
    /// <param name="impose">強制產生</param>
    /// <returns></returns>
    public IMice InstantiateMice(short miceID, float miceSize, Transform hole, bool impose)
    {
        Vector3 _miceSize;
        IMice mice;
        // 如果老鼠洞是打開的 或 強制產生
        if (hole.GetComponent<HoleState>().holeState == HoleState.State.Open || impose)
        {
            // 如果強制產生 且 老鼠在存活列表中 強制將老鼠死亡
            if (impose && m_CreatureSystem.GetActiveHoleMice(hole.transform)!=null)
            {
                mice = (IMice)m_MPGame.GetCreatureSystem().GetMice(miceID.ToString(), m_CreatureSystem.GetActiveHoleMice(hole.transform).m_go.name);
                if (mice != null)
                {
                    mice.Play(IAnimatorState.ENUM_AnimatorState.Died);
                    mice.GetAIState().SetAIState(new DiedAIState());
                    m_CreatureSystem.RemoveHoleMiceRefs(hole);
                }
                //GetHoleMiceRefs(hole.transform).GetComponentInChildren<IMice>().SendMessage("OnDead", 0.0f); //錯誤
            }

            //// 強制移除老鼠 沒有發送死亡訊息
            //if (GetActiveHoleMice(hole.transform))      // 錯誤 FUCK  還沒始就強制移除索引
            //    RemoveHoleMiceRefs(hole.transform); //錯誤

            // 取得物件池老鼠
            mice = (IMice)ActiveMice(miceID.ToString(), hole);

            // 如果物件池老鼠不是空的 產生老鼠
            if (mice.m_go != null)
            {
                hole.GetComponent<HoleState>().holeState = HoleState.State.Closed;
                _miceSize = hole.transform.Find("ScaleValue").localScale / 10 * miceSize;   // Scale 版本
                mice.m_go.transform.parent = hole;              // hole[-1]是因為起始值是0 
                mice.m_go.layer = hole.gameObject.layer;
                mice.m_go.transform.localPosition = Vector2.zero;
                mice.m_go.transform.localScale = hole.transform.GetChild(0).localScale - _miceSize;  // 公式 原始大小分為10等份 10等份在減掉 要縮小的等份*乘洞的倍率(1.4~0.9) => 1.0整份-0.2份*1(洞口倍率)=0.8份 
                                                                                                     //clone.GetComponent<BoxCollider2D>().enabled = true;
                mice.Play(IAnimatorState.ENUM_AnimatorState.Hello);

                mice.m_go.transform.gameObject.SetActive(false);
                mice.m_go.transform.gameObject.SetActive(true);
                return mice;
            }
        }
        return null;
    }
    #endregion

    #region -- SpawnBoss --
    public void SpawnBoss(Transform hole, short miceID, float lerpSpeed, float lerpTime, float upSpeed, float upDistance)// 怪怪的 程式碼太長 錯誤
    {
        Debug.Log("------------------- Mice Boss ID:  " + miceID + " ------------------------");

        // new MiceBoss (default scale x 1.3)
        // init mice go
        // Composite go attr skill ai state anim
        // if hole closed kill mice
        // play hole anim
        // spawn

        try
        {
            if (hole.GetComponent<HoleState>().holeState == HoleState.State.Closed)
            {
                ICreature creature = m_CreatureSystem.GetMice(hole);

            if (creature != null)
                {
                    creature.Play(IAnimatorState.ENUM_AnimatorState.Died);
                    creature.GetAIState().SetAIState(new DiedAIState());
                    m_CreatureSystem.RemoveHoleMiceRefs(hole);
                }
            }

            ICreature boss = new MiceBoss();
            MiceAttr miceAttr = MPGFactory.GetAttrFactory().GetMiceProperty(miceID.ToString());
            ISkill skill = MPGFactory.GetSkillFactory().GetSkill(Global.miceProperty, miceID);
            MiceAnimState animState = new MiceAnimState(boss.m_go, true, lerpSpeed, miceAttr.MiceSpeed, upDistance, miceAttr.LifeTime);
            GameObject go = MPGFactory.GetObjFactory().Instantiate(assetLoader.GetAsset(miceAttr.name), hole, miceID.ToString(), Vector3.zero, Vector3.one, Vector2.one, -1);

            // 播放洞口動畫
            hole.GetComponent<Animator>().enabled = true;
            hole.GetComponent<Animator>().Play("Layer1.HoleScale", -1, 0f);

            miceAttr.SetHP(); // 沒有BOSS的血量
            boss.SetArribute(miceAttr);
            boss.SetGameObject(go);
            boss.SetSkill(skill);
            boss.SetAnimState(animState);
            boss.Initialize();  // 錯誤 可能非必要
            boss.m_go.SetActive(true);

            m_CreatureSystem.AddMiceRefs(hole, miceID.ToString(), go.GetHashCode().ToString(), boss);
        }
        catch
        {
            throw;
        }
    }
    #endregion

    /// <summary>
    /// 實體化老鼠、組裝 並放入物件池
    /// </summary>
    /// <param name="objectID"></param>
    /// <param name="bundle"></param>
    void Instantiate(string objectID, GameObject bundle)
    {
        Transform objGroup = objectPool.transform.Find(objectID);
        GameObject clone;
        // 建立空GameObject群組
        if (objGroup == null)
        {
            clone = new GameObject();
            clone.name = objectID;
            clone.transform.parent = objectPool.transform;
            clone.layer = clone.transform.parent.gameObject.layer;
            clone.transform.localScale = Vector3.one;
            objGroup = clone.transform;
            dictMicePool.Add(objectID, new Dictionary<string, ICreature>());
        }

        // 預先實體化老鼠
        for (int i = 0; i < spawnCount; i++)
        {
            clone = MPGFactory.GetObjFactory().Instantiate(bundle, objGroup, objectID, Vector3.zero, Vector3.one, Vector2.zero, -1);

            ICreature creature ;
            MiceAttr miceAttr = MPGFactory.GetAttrFactory().GetMiceProperty(objectID);
            miceAttr.SetMaxHP(1);   // 錯誤 FUCK 生命應該在Server

            // 設定動畫 錯誤 FUCK 數值應該在Server
            if (objectID == "11001")
            {
                // 附加 Mice 並初始化
                creature = new Mice();
                creature.SetAnimState(new MiceAnimState(clone, false, lerpSpeed, miceAttr.MiceSpeed, upDantance, miceAttr.LifeTime));
            }
            else if (objectID == "11002")
            {
                // 附加 Mice 並初始化
                creature = new Much();
                creature.SetAnimState(new MuchAnimState(clone, false, lerpSpeed, miceAttr.MiceSpeed, upDantance, miceAttr.LifeTime));
            }
            else if (objectID == "11003")
            {            // 附加 Mice 並初始化
                creature = new HeroMice();
                creature.SetAnimState(new HeroMiceAnimState(clone, false, lerpSpeed, miceAttr.MiceSpeed, upDantance, miceAttr.LifeTime));
            }
            else
            {            // 附加 Mice 並初始化
                creature = new Mice();
                creature.SetAnimState(new MiceAnimState(clone, false, lerpSpeed, miceAttr.MiceSpeed, upDantance, miceAttr.LifeTime));
            }

            // 設定數值
            creature.SetArribute(miceAttr);
            creature.SetGameObject(clone);
            creature.SetAI(new MiceAI(creature));
            //creature.Initialize();

            // Add Mice Pool
            dictMicePool[objectID].Add(clone.GetHashCode().ToString(), creature);

            clone.gameObject.SetActive(false);
        }
    }



    /// <summary>
    /// 將雙方的老鼠合併 剔除相同的老鼠
    /// </summary>
    public void MergeMice()
    {
        Dictionary<string, object> dictMyMice = new Dictionary<string, object>(Global.dictTeam);
        Dictionary<string, object> dictOtherMice = new Dictionary<string, object>(Global.OpponentData.Team);
        int miceID = -1;

        Debug.Log("dictMyMice.Count:" + dictMyMice.Count);
        Debug.Log("dictOtherMice.Count:" + dictOtherMice.Count);

        foreach (KeyValuePair<string, object> item in dictMyMice)
        {
            if (dictOtherMice.ContainsKey(item.Key))
                dictOtherMice.Remove(item.Key);
        }

        dictMyMice = dictMyMice.Concat(dictOtherMice).ToDictionary(x => x.Key, x => x.Value); ;

        foreach (KeyValuePair<string, object> item in dictMyMice)
        {
            //  Debug.Log("----------------Before Pooling Mice Name:" + item.Value + "----------------------");
            miceID = MPGFactory.GetObjFactory().GetIDFromName(Global.miceProperty, "MiceID", item.Value.ToString().ToLower());
            if (!_dictMiceObject.ContainsKey(miceID) && miceID != -1)
            {
                _dictMiceObject.Add(miceID, item.Value.ToString().ToLower());
                Debug.Log("----------------Pooling Mice ID:" + miceID + "----------------------");
            }
        }

        // 亂寫 FUCK
        if (!_dictMiceObject.ContainsKey(10001))
            _dictMiceObject.Add(10001, "eggmice");
        //  Debug.Log(_dictObject);
        MergeFlag = true;
        Debug.Log("Merge Mice Completed ! " + MergeFlag);
    }


    private void LoadSkillData()
    {
        foreach (KeyValuePair<int, string> item in _dictMiceObject)
        {
            int itemID = Convert.ToInt16(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.miceProperty, "ItemID", item.Key.ToString()));
            string itemName = MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.itemProperty, "ItemName", itemID.ToString()).ToString();
            _dictSkillObject.Add(itemID, itemName);
        }
    }


    /// <summary>
    /// 將 戰鬥老鼠 初始化 並 放入物件池
    /// </summary>
    /// <param name="miceID">老鼠類別ID</param>
    /// <param name="hashID">老鼠HashID</param>
    /// <param name="mice">老鼠Class</param>
    public void AddMicePool(string miceID, string hashID, IMice mice)
    {
        MiceAttr miceAttr = MPGFactory.GetAttrFactory().GetMiceProperty(miceID);
        mice.Release();
        mice.SetArribute(miceAttr);
        mice.SetGameObject(mice.m_go);
        mice.SetAI(new MiceAI(mice));
        mice.m_go.transform.GetChild(0).localScale = Vector3.one;
        mice.m_go.transform.GetChild(0).localRotation = Quaternion.identity;
        mice.m_go.transform.GetChild(0).localPosition = Vector3.zero;

        dictMicePool[miceID].Add(hashID, mice);

        System.GC.Collect();
    }

    public List<int> GetPoolMiceIDs()
    {
        return _dictMiceObject.Keys.ToList();
    }

    public List<int> GetPoolSkillMiceIDs()
    {
        // 先把key轉換成list 再把list轉換int型態
        return _dictSkillMice.Keys.ToList().Select(keys => int.Parse(keys)).ToList();
    }
    private void OnLoadPlayerItem()
    {
        IsloadPlayerItem = true;
        Debug.Log("OnLoadPlayerItem");
    }

    public override void Release()
    {
        Global.photonService.LoadPlayerItemEvent -= OnLoadPlayerItem;
    }

    ~PoolSystem()
    {
        Global.photonService.LoadPlayerItemEvent -= OnLoadPlayerItem;
    }
}