using UnityEngine;
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

public class PoolManager : MonoBehaviour
{
    private AssetLoader assetLoader;
    MPGame MPGame;
    //private Dictionary<string, object> _tmpDict;
    private Dictionary<int, string> _dictMiceObject;
    private Dictionary<int, string> _dictSpecialObject;
    private Dictionary<int, string> _dictSkillObject;
    //private HashSet<int> _myMice;
    //private HashSet<int> _otherMice;

    private GameObject clone;

    private float _lastTime;
    private float _currentTime;
    static readonly float lerpSpeed = 0.1f, upSpeed = 6, upDantance = 60;    // 錯誤 數值不應該在這裡 應該在實體化時輸入

    Dictionary<string, object> _dictSkillMice;

    public GameObject Panel;

    [Tooltip("物件池位置")]
    public GameObject ObjectPool;

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

    private void OnEnable()
    {
        Global.photonService.LoadPlayerItemEvent += OnLoadPlayerItem;
    }


    void Awake()
    {

        Global.photonService.LoadPlayerItem(Global.Account);

        assetLoader = MPGame.Instance.GetAssetLoader();
        _lastTime = 0;
        _currentTime = 0;
        clearTime = 10;
        PoolingFlag = false;      // 初始化物件池
        _dictMiceObject = new Dictionary<int, string>();
        _dictSpecialObject = new Dictionary<int, string>();
        _dictSkillObject = new Dictionary<int, string>();

        _dictSpecialObject.Add(11001, "Bali");  // 亂寫
        _dictSpecialObject.Add(11002, "Much");
        _dictSpecialObject.Add(11003, "HeroMice");
        _dictSkillMice = new Dictionary<string, object>();

    }

    void Start()
    {
        MergeMice();
        LoadItemDataFromDict();
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


    void Update()
    {
        //if (!_poolingFlag && !string.IsNullOrEmpty(assetLoader.ReturnMessage))
        //    Debug.Log("Message:" + assetLoader.ReturnMessage /*+ "_loadedCount:" + assetLoader._loadedCount + "_objCount:" + assetLoader._objCount*/);

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

        _currentTime = Time.time;

        // 定時清除物件池
        if (_currentTime - _lastTime > clearTime)     // 達到清除時間時
        {
            for (int i = 0; i < ObjectPool.transform.childCount; i++)       // 跑遍動態池
            {
                if (ObjectPool.transform.GetChild(i).childCount > clearLimit)           // 如果動態池超過限制數量
                {
                    for (int j = 0; j < ObjectPool.transform.GetChild(i).childCount - reserveCount; j++)    // 銷毀物件
                    {
                        Destroy(ObjectPool.transform.GetChild(i).GetChild(j).gameObject);
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
                Destroy(clone.GetComponent<BoxCollider2D>());
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

    /// <summary>
    /// 每一次顯示一個GameObject。如果GameObject不足，Spawn一個物件並顯示。
    /// </summary>
    /// <param name="objectID">使用Name找Object</param>
    /// <returns>回傳 ( GameObject / null )</returns>
    public GameObject ActiveObject(string objectID)
    {
        //Debug.Log("_dictObject.Count:" + _dictObject.Count);
        //int objectID = _dictObject.FirstOrDefault(x => x.Value == objectName).Key;       // 找Key

        if (!ObjectPool.transform.Find(objectID))
        {
            Debug.Log("Pooling can't find :" + objectID);
        }
        else if (ObjectPool.transform.Find(objectID).childCount == 0)
        {
            string bundleName = (string)MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.miceProperty, "ItemName", objectID.ToString());
            Instantiate(objectID, assetLoader.GetAsset(bundleName));
        }
        else
        {
            for (int i = 0; i < ObjectPool.transform.Find(objectID).childCount; i++)
            {
                GameObject clone = ObjectPool.transform.Find(objectID).GetChild(i).gameObject;
                MiceBase mice = clone.GetComponent(typeof(MiceBase)) as MiceBase;
                MiceAttr miceAttr = MPGFactory.GetAttrFactory().GetMiceProperty(objectID);
                if (mice.enabled == false) mice.enabled = true;

                if (clone.name == objectID && !clone.gameObject.activeSelf)
                {
                    clone.SetActive(true);
                    miceAttr.SetHP(1);
                    mice.SetArribute(miceAttr);
                    mice.Initialize(false, lerpSpeed, miceAttr.MiceSpeed, upDantance, miceAttr.LifeTime);
                    clone.transform.GetChild(0).localScale = Vector3.one;
                    clone.transform.GetChild(0).localRotation = Quaternion.identity;
                    clone.transform.GetChild(0).localPosition = Vector3.zero;

                    if (clone.GetComponent<BoxCollider2D>()) clone.GetComponent<BoxCollider2D>().enabled = true;

                    clone.SetActive(false);
                    clone.SetActive(true);
                    return clone;
                }
            }
        }
        return null;
    }

    void Instantiate(string objectID, GameObject bundle)
    {
        Transform objGroup = ObjectPool.transform.Find(objectID);

        // 建立空GameObject群組
        if (objGroup == null)
        {
            clone = new GameObject();
            clone.name = objectID;
            clone.transform.parent = ObjectPool.transform;
            clone.layer = clone.transform.parent.gameObject.layer;
            clone.transform.localScale = Vector3.one;
            objGroup = clone.transform;
        }

        // 預先實體化老鼠
        for (int i = 0; i < spawnCount; i++)
        {
            clone = MPGFactory.GetObjFactory().Instantiate(bundle, objGroup, objectID, Vector3.zero, Vector3.one, Vector2.zero, -1);

            MiceBase mice;
            MiceAttr miceAttr = MPGFactory.GetAttrFactory().GetMiceProperty(objectID);
            miceAttr.SetMaxHP(1);   // 錯誤 FUCK 生命應該在Server

            // 設定動畫 錯誤 FUCK 數值應該在Server
            if (objectID == "11001")
            {
                // 附加 Mice 並初始化
                mice = clone.GetComponent<MiceBase>();
                mice.SetAnimState(new MiceAnimState(clone, false, lerpSpeed, miceAttr.MiceSpeed, upDantance, miceAttr.LifeTime));
            }
            else if (objectID == "11002")
            {
                // 附加 Mice 並初始化
                mice = clone.GetComponent<MiceBase>();
                mice.SetAnimState(new MuchAnimState(clone, false, lerpSpeed, miceAttr.MiceSpeed, upDantance, miceAttr.LifeTime));
            }
            else if (objectID == "11003")
            {            // 附加 Mice 並初始化
                mice = clone.GetComponent<MiceBase>();
                mice.SetAnimState(new HeroMiceAnimState(clone, false, lerpSpeed, miceAttr.MiceSpeed, upDantance, miceAttr.LifeTime));
            }
            else
            {            // 附加 Mice 並初始化
                mice = clone.AddMissingComponent<Mice>();
                mice.SetAnimState(new MiceAnimState(clone, false, lerpSpeed, miceAttr.MiceSpeed, upDantance, miceAttr.LifeTime));
            }
            // 設定數值
            mice.SetArribute(miceAttr);
            mice.Initialize(false, lerpSpeed, upSpeed, upDantance, miceAttr.LifeTime);

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


    private void LoadItemDataFromDict()
    {
        foreach (KeyValuePair<int, string> item in _dictMiceObject)
        {
            int itemID = Convert.ToInt16(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.miceProperty, "ItemID", item.Key.ToString()));
            string itemName = MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.itemProperty, "ItemName", itemID.ToString()).ToString();
            _dictSkillObject.Add(itemID, itemName);
        }
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

    private void OnDisable()
    {
        Global.photonService.LoadPlayerItemEvent -= OnLoadPlayerItem;
    }
}
