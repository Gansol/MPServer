using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using MiniJSON;
using System;
using System.Linq;
using MPProtocol;

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
    private AssetLoaderSystem m_AssetLoaderSystem;
    private GameObject m_RootUI;
    private AttachBtn_BattleUI UI;

    #region Variables 變數
    private Dictionary<string, Dictionary<string, ICreature>> _dictMicePool; // 物件池老鼠
    private Dictionary<int, string> _dictSkillObject;      // 技能資源列表
    private Dictionary<int, string> _dictMiceObject;    // 老鼠資源列表
    private Dictionary<string, object> _dictSkillMice;  // 技能老鼠 錯誤 現在是直接抓Global.Team
    private Dictionary<int, string> _dictSpecialObject;// 特殊老鼠資源列表

    private GameObject objectPool;  // 物件池位置

    private static readonly float _lerpSpeed = 0.1f;     // 動畫間格     錯誤 數值不應該在這裡 應該在實體化時輸入
    private static readonly float _upSpeed = 6;             // 上升速度     錯誤 數值不應該在這裡 應該在實體化時輸入
    private static readonly float _upDantance = 60;    // 上升距離     錯誤 數值不應該在這裡 應該在實體化時輸入
    private Vector3 _bossScale;         // boss縮放
    private Vector3 _skillScale;           // 技能縮放
    private int _dataLoadedCount;    // 資料載入量
    private float _lastTime;                  // 遊戲開始時間
    private bool _bLoadedAsset;       // 是否載入資產
    private bool _bMergeMiceData; // 合併老鼠完成
    private bool _bLoadPlayerItem; // 是否載入玩家道具完成
    private bool _bPoolingComplete;// 初始化物件池

    [Range(3, 12)]
    private int _clearLimit;        // 物件池上限(各種類分開)
    [Range(10, 20)]
    private int _clearTime;        // 清除間格時間
    [Range(3, 12)]
    private int _spawnCount;   // 產生數量
    [Range(2, 12)]
    private int _reserveCount; // 物件池保留量(各種類分開)





    //  [Tooltip("技能位置")]
    //  public GameObject skillArea;
    //private Dictionary<int, ICreature> dictMiceRefs;
    //private Dictionary<int, GameObject> dictBossPool;
    #endregion

    public PoolSystem(MPGame MPGame) : base(MPGame)
    {
        Debug.Log("--------------- PoolSystem Create ----------------");
        m_CreatureSystem = MPGame.GetCreatureSystem();
        m_AssetLoaderSystem = m_MPGame.GetAssetLoaderSystem();

        _bossScale = new Vector3(0.7f, 0.7f, 0.7f);
        _skillScale = new Vector3(0.6f, 0.6f, 0.6f);
    }

    public override void Initialize()
    {
        Debug.Log("--------------- PoolSystem Initialize ----------------");
        Global.photonService.LoadPlayerItemEvent += OnLoadPlayerItem;
        Global.photonService.LoadPlayerDataEvent += OnLoadPlayerData;
        Global.photonService.LoadPlayerItem(Global.Account);
        Global.photonService.LoadPlayerData(Global.Account);

        _dictMicePool = new Dictionary<string, Dictionary<string, ICreature>>();
        _dictMiceObject = new Dictionary<int, string>();
        _dictSpecialObject = new Dictionary<int, string>();
        _dictSkillObject = new Dictionary<int, string>();
        _dictSkillMice = new Dictionary<string, object>();

        m_RootUI = GameObject.Find(Global.Scene.BattleAsset.ToString());
        UI = m_RootUI.GetComponentInChildren<AttachBtn_BattleUI>();
        objectPool = m_RootUI.transform.Find("Battle(Panel)").Find("ObjectPool").gameObject;

        _dataLoadedCount = (int)ENUM_Data.None;
        _lastTime = 0;
        _clearTime = 10;
        _spawnCount = 5;
        _clearLimit = 5;
        _clearTime = 15;
        _reserveCount = 3;
        _bPoolingComplete = false;      // 初始化物件池
        _bLoadedAsset = false;
        _bLoadPlayerItem = false;

        MergeBothPlayerMiceData();
        LoadSpecialMice();
        LoadSkillData();
        LoadAssets();
    }

    #region -- LoadAssets 載入所有必要資產 --
    /// <summary>
    /// 載入Battle所有必要資產
    /// </summary>
    private void LoadAssets()
    {
        // 載入 老鼠資產
        foreach (KeyValuePair<int, string> item in _dictMiceObject)
            m_AssetLoaderSystem.LoadAssetFormManifest(Global.MicePath + item.Value + "/unique/" + item.Value + Global.ext);

        // 載入 特殊老鼠
        foreach (KeyValuePair<int, string> item in _dictSpecialObject)
            m_AssetLoaderSystem.LoadAssetFormManifest(Global.CreaturePath + item.Value + Global.ext);

        // 載入 道具資產
        foreach (KeyValuePair<int, string> item in _dictSkillObject)
            m_AssetLoaderSystem.LoadAssetFormManifest(Global.ItemIconUniquePath + Global.IconSuffix + item.Value + Global.ext);

        // 載入 特效資產
        foreach (KeyValuePair<int, string> item in _dictMiceObject)
        {
            int itemID = Convert.ToInt16(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.miceProperty, "ItemID", item.Key));
            string itemName = MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.itemProperty, "ItemName", itemID).ToString();
            //Debug.Log("Item Name:  " + itemName);
            //if(item.Key!=10001)
            m_AssetLoaderSystem.LoadAssetFormManifest(Global.EffectsUniquePath + Global.EffectSuffix + itemName + Global.ext);
        }
        m_AssetLoaderSystem.SetLoadAllAseetCompleted();
        _bLoadedAsset = true;
    }
    #endregion


    public override void Update()
    {
        InitializePoolMiceGameObject();
        ClearPool();
    }

    #region -- InitializePoolMiceGameObject 初始化物件池老鼠物件  -- 
    /// <summary>
    /// 初始化物件池
    /// </summary>
    private void InitializePoolMiceGameObject()
    {
        if (m_AssetLoaderSystem.IsLoadAllAseetCompleted && _bLoadedAsset && (GetMustLoadedDataCount() == _dataLoadedCount) && _bMergeMiceData && !_bPoolingComplete)
        {
            Debug.Log("Pool OK");
            m_AssetLoaderSystem.Initialize();
            _bLoadedAsset = false;
            _dictSkillMice = Global.dictTeam;

            InstantiateMice(_dictMiceObject);
             Debug.Log("Instantiate Mice Completed!");
            InstantiateMice(_dictSpecialObject);
              Debug.Log("Instantiate SpecialMice Completed!");
            InstantiateSkillMice(_dictSkillMice);
              Debug.Log("Pooling Mice Completed ! ");
            Global.photonService.SendRoomMice(Global.RoomID, _dictMiceObject.Keys.Select(x => (x).ToString()).ToArray());
            _bPoolingComplete = true;
        }
    }
    #endregion

    #region -- ClearPool 定時清理老鼠  -- 
    /// <summary>
    /// 定時清除物件池
    /// </summary>
    private void ClearPool()
    {
        if (Time.time - _lastTime > _clearTime)     // 達到清除時間時
        {
            for (int i = 0; i < objectPool.transform.childCount; i++)       // 跑遍動態池
            {
                if (objectPool.transform.GetChild(i).childCount > _clearLimit)           // 如果動態池超過限制數量
                {
                    for (int j = 0; j < objectPool.transform.GetChild(i).childCount - _reserveCount; j++)    // 銷毀物件
                    {
                        GameObject.Destroy(objectPool.transform.GetChild(i).GetChild(j).gameObject);
                    }
                }
            }
            _lastTime = Time.time;
        }
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
                    m_CreatureSystem.RemoveHoleMiceRefs(hole);
                }
            }

            ICreature boss = new MiceBoss();
            MiceAttr miceAttr = MPGFactory.GetAttrFactory().GetMiceProperty(miceID.ToString());
            ISkill skill = MPGFactory.GetSkillFactory().GetSkill(Global.miceProperty, miceID);
            MiceAnimState animState = new MiceAnimState(boss.m_go, true, lerpSpeed, miceAttr.MiceSpeed, upDistance, miceAttr.LifeTime);
            GameObject go = MPGFactory.GetObjFactory().Instantiate(m_AssetLoaderSystem.GetAsset(miceAttr.name), hole, miceID.ToString(), Vector3.zero, Vector3.one, Vector2.one, -1);

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

    #region -- InstantiateSkillMice 實體化技能老鼠 -- 
    void InstantiateSkillMice(Dictionary<string, object> objectData)
    {
        int i = 0;
        float lerpSpeed = 0.1f;
        float upDistance = 30f;
        GameObject clone;
        // int energyValue = 0;

        foreach (KeyValuePair<string, object> item in objectData)
        {
            if (m_AssetLoaderSystem.GetAsset(item.Value.ToString().ToLower()) != null)
            {
                GameObject bundle = m_AssetLoaderSystem.GetAsset(item.Value.ToString().ToLower());
                Vector3 scale = Vector3.zero;
                Transform parent = UI.SkillArea.transform.GetChild(i);
                clone = new GameObject();

                scale = (i == 4) ? _bossScale : _skillScale;

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
                    if (m_AssetLoaderSystem.GetAsset(Global.IconSuffix + itemName) != null)
                    {
                        bundle = m_AssetLoaderSystem.GetAsset(Global.IconSuffix + itemName);
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
                Debug.LogError("bundle is null! "+ item.Value.ToString());
            }

        }
        Debug.Log("InstantiateSkillMice Completed!");
    }
    #endregion

    #region -- GatPoolMice 取得物件池老鼠  -- 
    private ICreature GatPoolMice(string miceID, Transform hole)
    {
        string hashID = "";
        ICreature creature = null;
        if (_dictMicePool[miceID].Count > 0)
        {
            foreach (KeyValuePair<string, ICreature> mice in _dictMicePool[miceID])
            {
                hashID = mice.Key;
                creature = mice.Value;

                if (mice.Value.m_go.GetComponent<BoxCollider2D>())
                    mice.Value.m_go.GetComponent<BoxCollider2D>().enabled = true;

                m_MPGame.GetCreatureSystem().AddMiceRefs(hole, miceID, hashID, mice.Value);
                //     m_MPGame.GetCreatureSystem().AddHoleMiceRefs(hole, mice.Value);

                break;
            }
            _dictMicePool[miceID].Remove(hashID);
            creature.Initialize();
            return creature;
        }
        else
        {
            string bundleName = (string)MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.miceProperty, "ItemName", miceID.ToString());
            InstantiateMice(new Dictionary<int, string>() { { int.Parse(miceID), bundleName } });
            //ActiveMice(miceID, hole); // 可能造成無線迴圈 會和下面一起造成產生了卻沒被使用的情況
        }
        return null; // 錯誤 回傳空的
    }
    #endregion

    #region -- ActiveMice 顯示老鼠 --
    /// <summary>
    /// 顯示老鼠 還不完整 錯誤
    /// </summary>
    /// <param name="poolManager"></param>
    /// <param name="miceID"></param>
    /// <param name="miceSize"></param>
    /// <param name="hole"></param>
    /// <param name="impose">強制產生</param>
    /// <returns></returns>
    public ICreature ActiveMice(short miceID, float miceSize, Transform hole, bool impose)
    {
        if (Global.isGameStart)
        {
            // Debug.Log("InstantiateMice: Hole:" + hole + "  miceID: " + miceID);
            Vector3 _miceSize;
            ICreature creature;
            // 如果老鼠洞是打開的 或 強制產生
            if (hole.GetComponent<HoleState>().holeState == HoleState.State.Open || impose)
            {
                // 如果強制產生 且 老鼠在存活列表中 強制將老鼠死亡
                if (impose && m_CreatureSystem.GetActiveHoleMice(hole.transform) != null)
                {
                    creature = m_MPGame.GetCreatureSystem().GetMice(miceID.ToString(), m_CreatureSystem.GetActiveHoleMice(hole.transform).m_go.name);
                    if (creature != null)
                    {
                        creature.Play(IAnimatorState.ENUM_AnimatorState.Died);
                        //creature.OnHit();  // 錯誤  應該+0分
                        //  creature.GetAI().SetAIState(new DiedAIState(creature.GetAI()));
                        m_CreatureSystem.RemoveHoleMiceRefs(hole);
                    }
                    //GetHoleMiceRefs(hole.transform).GetComponentInChildren<IMice>().SendMessage("OnDead", 0.0f); //錯誤
                }

                //// 強制移除老鼠 沒有發送死亡訊息
                //if (GetActiveHoleMice(hole.transform))      // 錯誤 FUCK  還沒始就強制移除索引
                //    RemoveHoleMiceRefs(hole.transform); //錯誤

                // 取得物件池老鼠
                creature = GatPoolMice(miceID.ToString(), hole);
                if (creature == null)
                    creature = GatPoolMice(miceID.ToString(), hole);

                // 如果物件池老鼠不是空的 產生老鼠
                if (creature.m_go != null)
                {
                    //Debug.Log(creature.m_go.name);
                    hole.GetComponent<HoleState>().holeState = HoleState.State.Closed;
                    _miceSize = hole.transform.Find("ScaleValue").localScale / 10 * miceSize;   // Scale 版本
                    creature.m_go.transform.parent = hole;              // hole[-1]是因為起始值是0 
                    creature.m_go.layer = hole.gameObject.layer;
                    creature.m_go.transform.localPosition = Vector2.zero;
                    creature.m_go.transform.localScale = hole.transform.GetChild(0).localScale - _miceSize;  // 公式 原始大小分為10等份 10等份在減掉 要縮小的等份*乘洞的倍率(1.4~0.9) => 1.0整份-0.2份*1(洞口倍率)=0.8份 
                                                                                                             //clone.GetComponent<BoxCollider2D>().enabled = true;
                    creature.m_go.GetComponent<BoxCollider2D>().enabled = true;
                    creature.m_go.transform.gameObject.SetActive(false);
                    creature.m_go.transform.gameObject.SetActive(true);
                    creature.GetAminState().Initialize();
                    creature.Play(IAnimatorState.ENUM_AnimatorState.Hello);
                    return creature;
                }
            }
        }
        Debug.Log("NULL Creature!!!!");
            return null;
      
    }
    #endregion

    #region -- InstantiateMice 實體化老鼠 --
    /// <summary>
    /// 實體化老鼠、組裝 並放入物件池
    /// </summary>
    /// <param name="objectID"></param>
    /// <param name="bundle"></param>
    void InstantiateMice(Dictionary<int, string> diceMiceData)
    {
        foreach (KeyValuePair<int, string> data in diceMiceData)
        {
            string objectID = data.Key.ToString();
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
                _dictMicePool.Add(objectID, new Dictionary<string, ICreature>());

            }

            // 預先實體化老鼠
            for (int i = 0; i < _spawnCount; i++)
            {
                clone = MPGFactory.GetObjFactory().Instantiate(m_AssetLoaderSystem.GetAsset(data.Value), objGroup, objectID, Vector3.zero, Vector3.one, Vector2.zero, -1);

                ICreature creature;
                MiceAttr miceAttr = MPGFactory.GetAttrFactory().GetMiceProperty(objectID);
                miceAttr.SetMaxHP(1);   // 錯誤 FUCK 生命應該在Server

                // 附加 Mice 並初始化動畫 錯誤 FUCK 數值應該在Server
                if (objectID == "11001")
                {
                    creature = new Bali();
                    creature.SetAnimState(new MiceAnimState(clone, false, _lerpSpeed, miceAttr.MiceSpeed, _upDantance, miceAttr.LifeTime));
                }
                else if (objectID == "11002")
                {
                    creature = new Much();
                    creature.SetAnimState(new MuchAnimState(clone, false, _lerpSpeed, miceAttr.MiceSpeed, _upDantance, miceAttr.LifeTime));
                }
                else if (objectID == "11003")
                { 
                    creature = new HeroMice();
                    creature.SetAnimState(new HeroMiceAnimState(clone, false, _lerpSpeed, miceAttr.MiceSpeed, _upDantance, miceAttr.LifeTime));
                }
                else
                { 
                    creature = new Mice();
                    creature.SetAnimState(new MiceAnimState(clone, false, _lerpSpeed, miceAttr.MiceSpeed, _upDantance, miceAttr.LifeTime));
                }

                // 組合數值
                creature.SetArribute(miceAttr);
                creature.SetGameObject(clone);
                creature.SetAI(new MiceAI(creature));

                // 加入物件池
                _dictMicePool[objectID].Add(clone.GetHashCode().ToString(), creature);
                //      Debug.Log("Pooling : MiceID:" + objectID + "   HashID:" + clone.GetHashCode() + "   Creature:" + creature.GetArribute().name);
                clone.gameObject.SetActive(false);
            }
        }
    }
    #endregion

    /// <summary>
    /// 載入特殊老鼠 (亂寫)
    /// </summary>
    private void LoadSpecialMice()
    {
        _dictSpecialObject.Add(11001, "Bali");  // 亂寫
        _dictSpecialObject.Add(11002, "Much");
        _dictSpecialObject.Add(11003, "HeroMice");
    }

    #region -- MergeBothPlayerMiceData 將雙方的老鼠合併 --
    /// <summary>
    /// 將雙方的老鼠合併 剔除相同的老鼠
    /// </summary>
    public void MergeBothPlayerMiceData()
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
             //   Debug.Log("----------------Pooling Mice ID:" + miceID + "----------------------");
            }
        }

        // 亂寫 FUCK
        if (!_dictMiceObject.ContainsKey(10001))
            _dictMiceObject.Add(10001, "eggmice");
        //  Debug.Log(_dictObject);
        _bMergeMiceData = true;
        Debug.Log("Merge Mice Completed ! " + _bMergeMiceData);
    }
    #endregion

    /// <summary>
    /// 載入技能資料
    /// </summary>
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
    public void AddMicePool(string miceID, string hashID, IMice oldMice)
    {
        MiceAttr miceAttr = MPGFactory.GetAttrFactory().GetMiceProperty(miceID);
        ICreature mice = new Mice();

        mice.SetArribute(miceAttr);
        mice.SetGameObject(oldMice.m_go);
        mice.SetAI(new MiceAI(mice));
        mice.m_go.transform.parent = objectPool.transform.Find(miceID);
        mice.m_go.transform.GetChild(0).localScale = Vector3.one;
        mice.m_go.transform.GetChild(0).localRotation = Quaternion.identity;
        mice.m_go.transform.GetChild(0).localPosition = Vector3.zero;
        mice.m_go.GetComponent<BoxCollider2D>().enabled = true;
        mice.m_go.SetActive(false);
        oldMice.Release();
        _dictMicePool[miceID].Add(hashID, mice); // 錯誤 應該給新的Hash

        Debug.Log("Return Pooling MiceID: " + miceID);
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

    public bool GetPoolingComplete()
    {
        return _bPoolingComplete;
    }

    #region -- OnLoadData 載入資料區 -- 


   private void OnLoadPlayerData()
    {
        Debug.Log("OnLoadPlayerData UI");
        _dataLoadedCount *= (int)ENUM_Data.PlayerData;
    }
    private void OnLoadPlayerItem()
    {
        Debug.Log("OnLoadPlayerItem UI");
        _dataLoadedCount *= (int)ENUM_Data.PlayerItem;
    }

    private int GetMustLoadedDataCount()
    {
        return (int)ENUM_Data.PlayerItem * (int)ENUM_Data.PlayerData;
    }
    #endregion

    public override void Release()
    {
        if(_dictMicePool!=null)
            _dictMicePool.Clear();
        Global.photonService.LoadPlayerItemEvent -= OnLoadPlayerItem;
        Global.photonService.LoadPlayerDataEvent -= OnLoadPlayerData;
    }

    ~PoolSystem()
    {
        Global.photonService.LoadPlayerItemEvent -= OnLoadPlayerItem;
        Global.photonService.LoadPlayerDataEvent -= OnLoadPlayerData;
    }
}
