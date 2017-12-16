using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
 * 負責 開啟/關閉/載入 Team的的所有處理
 * NGUI BUG : Team交換時Tween會卡色
 * + pageVaule 還沒加入翻頁值
 * ***************************************************************
 *                           ChangeLog
 * 20171211 v1.1.0   重製、修正索引問題                     
 * ****************************************************************/
public class BtnSwitch : MonoBehaviour
{

    [Range(0, 1)]
    [Tooltip("回彈速度")]
    public float lerpSpeed = 0.25f;
    [Range(0, 1)]
    [Tooltip("漸變速度")]
    public float tweenColorSpeed = 0.1f;
    [Range(0, 1)]
    [Tooltip("滑過時顏色")]
    public float hoverColor = 0.95f;
    [Range(0, 1)]
    [Tooltip("按下時顏色")]
    public float clickColor = 0.85f;
    [Range(0, 1)]
    [Tooltip("失效時時顏色")]
    public float disableColor = 0.5f;
    [Tooltip("隊伍離開距離")]                                 // 可以增加老鼠離開時自動加到最後
    public float leftDist = 50f;

    protected GameObject _clone, _other;           // 複製物件、碰撞時對方物件、原老鼠位置
    protected int _depth, teamCountMax;                          // UISprite深度
    protected float _distance;                                  // 兩物件間距離
    protected bool _isTrigged, _goTarget, _isPress, _destroy, _bMoveMotion;   // 是否觸發、移動目標、是否按下、是否摧毀
    protected Vector3 _originPos, _toPos;                       // 原始座標、目標作標

    Transform _parent;
    //public GameObject root;
    private static Dictionary<string, GameObject> _dictLoadedMiceBtnRefs, _dictLoadedTeamBtnRefs;
    enum enum_BtnMethod
    {
        Return = 0, // 返回原位
        Delete, // 移除Clone
        Switch, // 交換位置
        Add,    // 增加隊員
        Change, // 改變老鼠
    }

    public void init(ref Dictionary<string, GameObject> dictLoadedMiceBtnRefs, ref   Dictionary<string, GameObject> dictLoadedTeamBtnRefs, ref Transform parentArea)
    {
        _parent = parentArea;
        _dictLoadedMiceBtnRefs = dictLoadedMiceBtnRefs;
        _dictLoadedTeamBtnRefs = dictLoadedTeamBtnRefs;
        // _manager = manager;
    }

    void OnEnable()
    {
        //string refName = "";

        //if (transform.childCount > 0)
        //{
        //    refName = transform.GetChild(0).name;

        //    if (_dictLoadedMiceBtnRefs.ContainsKey(refName) && tag=="MiceIcon")
        //    {
        //        _dictLoadedMiceBtnRefs[refName] = gameObject;
        //    }
        //    else if (_dictLoadedTeamBtnRefs.ContainsKey(refName) && tag == "TeamIcon")
        //    {
        //        _dictLoadedTeamBtnRefs[refName] = gameObject;
        //    }
        //}
    }

    void Awake()
    {
        _originPos = transform.localPosition;
        _bMoveMotion=_goTarget = false;
        _other = gameObject;
        teamCountMax = 5;
    }

    void Update()
    {
        if (transform.childCount == 0)  // 如果沒有圖片 取消交換、拖曳功能、按鈕功能
        {
            GetComponent<UIDragObject>().enabled = false;
            enabled = false;
        }
        if (_bMoveMotion)
        MoveMotion();
    }

    #region -- 滑鼠事件片段程式碼 --
    void OnPress(bool isPress)
    {
        if (enabled)
        {
            _isPress = isPress;
            if (isPress)
            {
                GetComponent<BoxCollider>().isTrigger = true;
                TweenColor.Begin(gameObject, tweenColorSpeed, new Color(clickColor, clickColor, clickColor));
                Move2Clone();
            }
            else
            {
                GetComponent<BoxCollider>().isTrigger = false;
                BtnMethod();
            }
        }
    }

    void OnTriggerEnter(Collider triggedObject)
    {
        if (_isPress && triggedObject != gameObject)            // 撞到的不是自己 _isTrigger =true
        {
            _other = triggedObject.gameObject;
            _toPos = triggedObject.transform.localPosition;     // 要移動的位置 為 對方物件位置
            _isTrigged = true;
        }
    }

    void OnTriggerStay(Collider triggedObject)
    {
        _other = triggedObject.gameObject;
    }

    void OnTriggerExit()
    {
        _isTrigged = false;
        _other = gameObject;
    }

    void OnHover(bool isOver)
    {
        if (enabled)
        {
            if (isOver)
            {
                TweenColor.Begin(this.gameObject, tweenColorSpeed, new Color(hoverColor, hoverColor, hoverColor));
            }
            else if (!isOver)
            {
                TweenColor.Begin(this.gameObject, tweenColorSpeed, Color.white);
            }
        }
    }
    #endregion

    #region -- MoveMotion 按鈕移動動畫 --
    void MoveMotion()
    {
        // 如果物件距離 非常接近時 且 _goTarget(前往目標)為真 
        if (Vector3.Distance(transform.localPosition, _toPos) < 3f && _goTarget)
        {

            if (_other.tag == "TeamIcon" && tag == "MiceIcon")      // A>B
            {
                //_other.transform.GetComponent<UISprite>().spriteName = transform.GetChild(0).GetComponent<UISprite>().spriteName;
                transform.GetChild(0).parent = _other.transform;    // MiceIcon移到TeamBtn內
                _other.transform.GetChild(0).localPosition = Vector3.zero;                       // 移到正確作標上 因為Lerp永遠到不了 會差0.00001
                EnDisableBtn(_other, true);                         // 啟動TeamBtn功能
                Destroy(gameObject);                                // 移除移動的物件
                TeamSequence(_other, true);
            }
            else
            {
                Debug.Log("FUCK");
            }
            // A>A B>B 
            _originPos = _toPos;                                    // B1>B2時會改變原始做標，所以要儲存移動後的座標位置
            _bMoveMotion= _goTarget = false;                                      // 到達目標
        }

        // 按扭回彈/移動至時 clone刪除的時機 使用距離判斷
        if (Vector3.Distance(transform.localPosition, _toPos) <= Mathf.Round(_distance / 3) && _goTarget && _destroy && _clone != null)
            Destroy(_clone); _destroy = false;

        if (_goTarget)  // _goTarget=true 持續往目標移動
            transform.localPosition = Vector3.Lerp(transform.localPosition, _toPos, lerpSpeed);
    }
    #endregion

    #region -- BtnMethod 交換或返回選擇 --
    void BtnMethod()
    {
        //string teamName = transform.GetChild(0).name;
        //Debug.Log(_dictLoadedMiceBtnRefs[teamName].transform.parent.name + "   " + _dictLoadedMiceBtnRefs[teamName].transform.parent.parent.name + "   " + _dictLoadedMiceBtnRefs[teamName].transform.parent.parent.parent.name + "   " + _dictLoadedMiceBtnRefs[teamName].transform.parent.parent.parent.parent.name);
        //Debug.Log(_dictLoadedTeamBtnRefs[teamName].transform.parent.name + "   " + _dictLoadedTeamBtnRefs[teamName].transform.parent.parent.name + "   " + _dictLoadedTeamBtnRefs[teamName].transform.parent.parent.parent.name + "   " + _dictLoadedTeamBtnRefs[teamName].transform.parent.parent.parent.parent.name);

        _toPos = _originPos;
        _distance = Vector3.Distance(transform.localPosition, _toPos);
        bool bActiveBtn = false;

        if ((_other.tag == "MiceIcon" || _other.tag == "TeamIcon") && _other.name != gameObject.name)
            bActiveBtn = _other.GetComponent<BtnSwitch>().isActiveAndEnabled;

        if (tag == _other.tag && _other.transform.childCount == 1 && _other.name != gameObject.name && bActiveBtn)
        {
            // 成員交換
            SwitchMember(tag);
        }
        else if (tag != "TeamIcon" && _other.tag == "TeamIcon" && _other.transform.childCount == 0)
        {
            // 將老鼠加入隊伍
            AddTeamMember();
        }
        else if (tag == "MiceIcon" && _other.tag == "TeamIcon" || tag == "TeamIcon" && _other.tag == "MiceIcon" && _other.transform.childCount != 0 && bActiveBtn && _other.transform.GetChild(0).name != transform.GetChild(0).name)
        {
            // 改變隊伍成員 Mice>Team
            ChangeTeamMember();
        }
        else if (tag == "TeamIcon" && _distance > leftDist)  // B1>out
        {
            // 移除隊伍成員
            RemoveTeamMember();
        }
        else
        {
            // 返回
            ReturnOriginLocation();
            Destroy(_clone);
        }

        Global.photonService.UpdateMiceData(Global.Account, Global.dictMiceAll, Global.dictTeam);
    }
    #endregion

    private void RemoveTeamMember()
    {
        string teamName = transform.GetChild(0).name;
        _other = gameObject;

        TeamSequence(gameObject, false);
        RemoveTeamMember(teamName);

        gameObject.transform.localPosition = _originPos;
        Destroy(transform.GetChild(0).gameObject);
        Destroy(gameObject.GetComponent<TweenColor>());
        Destroy(_clone);
    }

    private void ReturnOriginLocation()
    {
        _other = gameObject;
        _bMoveMotion= _goTarget = _destroy = true;
        TweenColor.Begin(this.gameObject, tweenColorSpeed, Color.white);
       // DepthManager.SwitchDepthLayer(gameObject, transform, -Global.MeunObjetDepth);
    }


    private void AddTeamMember()
    {
        if (_other.transform.childCount == 0)                                   // 如果移動到Team的位置"沒有"Mice 移至Team
        {
            _toPos = _other.transform.localPosition + new Vector3(0, 10, 0);
            _distance = Vector3.Distance(transform.localPosition, _toPos);
            string miceID = transform.GetChild(0).name;
            Mice2Team(miceID);
            AddTeam(miceID, null);
            _bMoveMotion = true;
        }
    }

    /// <summary>
    /// 交換隊伍成員 by Tag
    /// </summary>
    /// <param name="tag">tag</param>
    private void SwitchMember(string tag)
    {
        Dictionary<string, object> data = Global.dictMiceAll;

        if (tag == "TeamIcon")
            data = Global.dictTeam;

        // Team Switch
        string myName = transform.GetChild(0).name;
        string otherName = _other.transform.GetChild(0).name;

        SwitchMemeber(myName, otherName, data);
        GameObject tmp = _other.transform.GetChild(0).gameObject;
        transform.GetChild(0).parent = _other.transform;
        tmp.transform.parent = transform;

        transform.GetChild(0).localPosition = Vector3.zero;

        _other.transform.GetChild(0).localPosition = Vector3.zero;
        _other.transform.GetChild(0).GetComponent<UISprite>().depth -= _depth ;
        _toPos = _originPos;

        Destroy(GetComponent<TweenColor>());            // NGUI BUG!!
        Destroy(_other.GetComponent<TweenColor>());     // NGUI BUG!!
        _bMoveMotion = _goTarget = true;
        Destroy(_clone);

        SortChildren.SortChildrenByID(transform.parent.gameObject, Global.extIconLength);
        // Mice Switch

        foreach (KeyValuePair<string, object> item in data)
        {
            Debug.Log("Exchange Object" + item.Key + "  " + item.Value);
        }
    }


    private void ChangeTeamMember()
    {
        if (_other.transform.GetChild(0).name != transform.GetChild(0).name)
        {
            string miceID = transform.GetChild(0).name;
            string otherName = _other.transform.GetChild(0).name;

            _toPos = _other.transform.localPosition + new Vector3(0, 10, 0);
            _distance = Vector3.Distance(transform.localPosition, _toPos);

            if (tag == "MiceIcon")
            {
                EnDisableBtn(GetLoadedMice(otherName), true);               // 將移出的的老鼠回復至Mice並恢復Btn功能
                RemoveTeamMemberRefs(otherName);           // 之後移除參考
                Mice2Team(miceID);
                AddTeam(miceID, _other.transform);
                Destroy(_other.transform.GetChild(0).gameObject);
            }
            else if (tag == "TeamIcon")
            {
                EnDisableBtn(GetLoadedMice(otherName), false);               // 將移出的的老鼠回復至Mice並恢復Btn功能
                UpdateTeam(otherName, _clone);
                _clone.transform.GetChild(0).name = _other.transform.GetChild(0).name;
                _clone.transform.GetChild(0).GetComponent<UISprite>().spriteName = _other.transform.GetChild(0).GetComponent<UISprite>().spriteName;
                EnDisableBtn(_clone, true);
                Destroy(gameObject);
                SortChildren.SortChildrenByID(_clone.transform.parent.gameObject, Global.extIconLength);
            }
        }
    }

    #region -- Mice2Team 更新按鈕字典參考 --
    /// <summary>
    /// 老鼠移動至隊伍，更新按鈕字典參考(無紀錄順序)
    /// </summary>
    /// <param name="miceID"></param>
    void Mice2Team(string miceID)
    {
        AddTeamMemberRefs(miceID, _clone); // 新增_clone的按鈕參考
        ModifyMiceRefs(miceID, null, _clone);//重新參考至黑掉的Clone物件(新的Mice按鈕)，原本的Mice按鈕會被銷毀

        EnDisableBtn(_other, true);
        EnDisableBtn(_clone, false);

        Debug.Log(transform.GetChild(0).GetComponent<UISprite>().name + "  " + transform.GetChild(0).GetComponent<UISprite>().depth);
        transform.GetChild(0).GetComponent<UISprite>().depth -= _depth; // 恢復深度值
       _bMoveMotion= _goTarget = true;
        _destroy = false;
    }
    #endregion

    #region -- Move2Clone 當按下且移動時，產生Clone --
    /// <summary>
    /// 當按下且移動時，產生Clone
    /// </summary>
    void Move2Clone()
    {
        // _depth = DepthManager.SwitchDepthLayer(gameObject, transform, Global.MeunObjetDepth); // 移動時深度提到最高防止遮擋
        _clone = MPGFactory.GetObjFactory().Instantiate(gameObject, transform.parent, gameObject.name, transform.localPosition, transform.localScale, Vector2.zero, 400);

        //  Transform btnArea = transform.parent;
        //  _clone.GetComponent<BtnSwitch>().init(ref _dictLoadedMiceBtnRefs, ref _dictLoadedTeamBtnRefs, ref btnArea);

        //   Debug.Log("parent: "+ transform.parent.parent.parent.parent.name+  " Clone   " + _dictLoadedMiceBtnRefs[transform.GetChild(0).name].transform.parent.parent.parent.parent.name);

        EnDisableBtn(_clone, false);
        _clone.tag = gameObject.tag;

    }
    #endregion



    #region -- Mice2Click 雙擊老鼠 返回原始位置 (注意Mice Team不同) --
    void Mice2Click()
    {
        if (tag == "MiceIcon")
        {
            string miceName = transform.GetChild(0).name;

            if (!GetLoadedMice(miceName) && GetTeamMemberCount() != teamCountMax)  // full and same
            {
                Dictionary<string, object> team = Global.dictTeam;
                GameObject teamBtn = _parent.transform.GetChild(team.Count).gameObject;

                Move2Clone();

                AddTeamMemberRefs(miceName, _clone);
                ModifyMiceRefs(miceName, null, _clone);
                AddTeam(miceName, null);

                transform.GetChild(0).parent = teamBtn.transform;
                teamBtn.transform.GetChild(0).localPosition = Vector3.zero;

                EnDisableBtn(teamBtn, true);
                Destroy(gameObject);
            }
        }
        else
        {

        }
    }
    #endregion

    #region -- ExchangeTeam 交換隊伍成員(JSON) --
    /// <summary>
    /// 交換隊伍成員(JSON)
    /// </summary>
    /// <param name="myName">自己的名稱</param>
    /// <param name="otherName">對方的名稱</param>
    /// <param name="dictServerData">要交換內容位子的字典</param>
    private void ExchangeObject(string myName, string otherName, Dictionary<string, object> dictServerData)
    {
        Global.SwapDictValueByKey(myName, otherName, dictServerData);

    }
    #endregion
    #region -- AddTeam 交換/加入隊伍 --
    /// <summary>
    /// 交換/加入隊伍(JSON)(順序採自動累計)
    /// </summary>
    ///<param name="miceID">老鼠名稱</param>
    ///<param name="teamBtn">=null時為增加成員</param>
    private void AddTeam(string miceID, Transform teamBtn)
    {
        Dictionary<string, object> dictTeam = new Dictionary<string, object>(Global.dictTeam);

        if (teamBtn != null)  // 交換隊伍成員
        {
            if (teamBtn.childCount != 0)
            {
                string teamID = teamBtn.transform.GetChild(0).name;
                string spriteName = GetLoadedMice(miceID).GetComponentInChildren<UISprite>().spriteName;

                dictTeam[teamID] = spriteName.Remove(spriteName.Length - Global.extIconLength);
                Global.RenameKey(dictTeam, teamID, miceID);
                EnDisableBtn(GetLoadedMice(teamID), true);

            }
        }
        else // 增加隊伍成員
        {
            object miceName = "";
            Global.dictMiceAll.TryGetValue(miceID, out miceName);
            dictTeam.Add(miceID, miceName);
        }
        Global.dictTeam = dictTeam;

    }
    #endregion

    #region -- UpdateTeam 更新隊伍 --
    /// <summary>
    /// 減少隊伍數量(JSON)
    /// 【請先排序】後再進行移除
    /// </summary>
    private void UpdateTeam(string toID, GameObject teamBtn)
    {
        Dictionary<string, object> dictTeam = new Dictionary<string, object>(Global.dictTeam);

        if (teamBtn != null)  // 交換隊伍成員
        {
            if (teamBtn.transform.childCount != 0)
            {
                string teamID = teamBtn.transform.GetChild(0).name;
                string spriteName = GetLoadedMice(toID).GetComponentInChildren<UISprite>().spriteName;

                dictTeam[teamID] = spriteName.Remove(spriteName.Length - Global.extIconLength);
                Global.RenameKey(dictTeam, teamID, toID);
                EnDisableBtn(GetLoadedMice(teamID), true);
                ModifyTeamRefs(teamID, toID, null);
                ModifyTeamRefs(toID, null, teamBtn);   // 要加Team的_clone才對
            }
        }

        foreach (KeyValuePair<string, object> item in dictTeam)
        {
            Debug.Log("Upadate Team" + item.Key + "  " + item.Value);
        }

        Global.dictTeam = dictTeam;
    }
    #endregion

    #region -- EnDisableBtn 啟動/關閉按鈕(內部使用) --
    /// <summary>
    /// 改變物件功能 開/關
    /// </summary>
    /// <param name="go">生效物件</param>
    /// <param name="enable">功能開關</param>
    private void EnDisableBtn(GameObject go, bool enable)
    {
        Debug.Log("EnDisableBtn: " + go.name);
        if (enable)
        {
            go.GetComponent<BtnSwitch>().enabled = enable;
            TweenColor.Begin(go, tweenColorSpeed, Color.white);
            Debug.Log(go.name + "   " + transform.parent.name + "  B  " + go.GetComponent<BtnSwitch>().isActiveAndEnabled);
        }
        else
        {
            go.GetComponent<BtnSwitch>().enabled = enable;
            TweenColor.Begin(go, tweenColorSpeed, new Color(disableColor, disableColor, disableColor));
        }
        go.GetComponent<BoxCollider>().isTrigger = false;
        go.GetComponent<UIDragObject>().enabled = enable;
    }
    #endregion

    #region -- DisableBtn 關閉按鈕(外部呼叫) --
    private void DisableBtn()
    {
        TweenColor.Begin(this.gameObject, tweenColorSpeed, new Color(disableColor, disableColor, disableColor));

        GetComponent<UIDragObject>().enabled = false;
        _isTrigged = GetComponent<BoxCollider>().isTrigger = false;

        enabled = false;
    }
    #endregion

    #region -- EnableBtn 開啟按鈕(外部呼叫) --
    public void EnableBtn()
    {

        TweenColor.Begin(this.gameObject, tweenColorSpeed, Color.white);

        GetComponent<UIDragObject>().enabled = true;
        _isTrigged = GetComponent<BoxCollider>().isTrigger = false;

        enabled = true;
        Debug.Log("   " + transform.parent.name + "  B2  " + GetComponent<BtnSwitch>().isActiveAndEnabled);
    }
    #endregion

















    /// <summary>
    /// 修改物件參考 toID= null 、修改ID newRef= null
    /// </summary>
    /// <param name="id"></param>
    /// <param name="toID"></param>
    /// <param name="newRef"></param>
    public void ModifyMiceRefs(string id, string toID, GameObject newRef)
    {
        if (newRef != null)
            _dictLoadedMiceBtnRefs[id] = newRef;

        if (!string.IsNullOrEmpty(toID))
            Global.RenameKey(_dictLoadedMiceBtnRefs, id, toID);
    }

    public void ModifyTeamRefs(string id, string toID, GameObject newRef)
    {
        if (newRef != null)
            _dictLoadedTeamBtnRefs[id] = newRef;

        if (!string.IsNullOrEmpty(toID))
            Global.RenameKey(_dictLoadedTeamBtnRefs, id, toID);
    }


    public bool AddMiceMemberRefs(string key, GameObject value)
    {
        if (!_dictLoadedMiceBtnRefs.ContainsKey(key))
        {
            _dictLoadedMiceBtnRefs.Add(key, value);
            return true;
        }
        return false;
    }

    public bool AddTeamMemberRefs(string key, GameObject value)
    {
        if (!_dictLoadedTeamBtnRefs.ContainsKey(key))
        {
            _dictLoadedTeamBtnRefs.Add(key, value);
            return true;
        }
        return false;
    }

    public bool RemoveMiceMemberRefs(string key)
    {
        _dictLoadedMiceBtnRefs.Remove(key);
        return _dictLoadedMiceBtnRefs.ContainsKey(key) ? true : false;
    }

    public bool RemoveTeamMemberRefs(string key)
    {
        _dictLoadedTeamBtnRefs.Remove(key);
        return _dictLoadedTeamBtnRefs.ContainsKey(key) ? true : false;
    }

    #region -- TeamSequence 隊伍整理佇列 --
    /// <summary>
    /// 隊伍整理佇列 bDragOut=True : 拉出隊伍整理   bDragOut=False : 拉入隊伍整理
    /// </summary>
    /// <param name="btnRefs">受影響的按鈕</param>
    /// <param name="bDragOut">拉入T 或 移出F</param>
    public void TeamSequence(GameObject btnRefs, bool bDragOut)
    {
        Dictionary<string, object> team = new Dictionary<string, object>(Global.dictTeam);
        int btnNo = int.Parse(btnRefs.name.Remove(0, btnRefs.name.Length - 1));
        // string miceName = teamRef.transform.GetChild(0).name;

        // 整理隊伍排序位置
        if (bDragOut)
        {
            // 拉出對隊伍時
            if (btnNo >= team.Count)
            {
                int offset = team.Count == 0 ? 0 : 1; ; // 當Btn=0時 防止溢位
                Transform teamBtn = btnRefs.transform.parent.GetChild(team.Count - offset);

                btnRefs.transform.GetChild(0).parent = teamBtn;
                teamBtn.GetChild(0).localPosition = Vector3.zero;
                teamBtn.SendMessage("EnableBtn");
            }
        }
        else
        {
            // 拉入隊伍時
            if (btnNo < team.Count)                                           // 2<5
            {
                for (int i = 0; i < (team.Count - btnNo); i++)                  // 5-2=3  
                {
                    GameObject outBtn;
                    Transform teamIcon = _parent.transform.GetChild(btnNo + i).GetChild(0);
                    Transform pervTeamBtn = _parent.transform.GetChild(btnNo + i - 1);

                    if (_dictLoadedTeamBtnRefs.TryGetValue(teamIcon.name, out outBtn))
                    {
                        _dictLoadedTeamBtnRefs[teamIcon.name] = pervTeamBtn.gameObject; //teamIcon.name  = ID
                    }
                    teamIcon.parent = pervTeamBtn; // team[2+i]=team[2+i-1] parent =>team[2]=team[1]

                    if (i == 0)     // 因為第一個物件會有2個Child所以要GetChild(1) 下一個按鈕移過來的Mice
                        pervTeamBtn.GetChild(1).localPosition = Vector3.zero;
                    else
                        pervTeamBtn.GetChild(0).localPosition = Vector3.zero;

                    pervTeamBtn.GetChild(0).localPosition = Vector3.zero;
                    pervTeamBtn.SendMessage("EnableBtn");
                }
                Global.dictTeam = team;
            }
        }
    }
    #endregion

    public void RemoveTeamMember(string teamName)
    {

        // 移除隊伍成員
        Global.dictTeam.Remove(teamName);
        //_dictLoadedMiceBtnRefs[teamName].gameObject.GetComponent<BtnSwitch>().EnDisableBtn(_dictLoadedMiceBtnRefs[teamName], true);                // 啟動按鈕
        _dictLoadedMiceBtnRefs[teamName].gameObject.GetComponent<BtnSwitch>().enabled = true;


        Debug.Log("RemoveTeamMember   " + _dictLoadedMiceBtnRefs[teamName].transform.parent.parent.parent.parent.name);
        _dictLoadedMiceBtnRefs[teamName].gameObject.SetActive(true);
        _dictLoadedMiceBtnRefs[teamName].gameObject.SendMessage("EnableBtn");

        _dictLoadedTeamBtnRefs.Remove(teamName);                                                        // 移除隊伍參考

        Dictionary<string, GameObject> buffer = new Dictionary<string, GameObject>(_dictLoadedTeamBtnRefs);

        int i = 0;
        foreach (KeyValuePair<string, GameObject> item in buffer)
        {
            Global.RenameKey(_dictLoadedTeamBtnRefs, item.Key, i.ToString());
            i++;
        }

        i = 0;
        foreach (KeyValuePair<string, object> item in Global.dictTeam)
        {
            Global.RenameKey(_dictLoadedTeamBtnRefs, i.ToString(), item.Key);
            i++;
        }
    }

    //#region -- ActiveMice 隱藏/顯示老鼠 --
    ///// <summary>
    ///// 隱藏/顯示老鼠
    ///// </summary>
    ///// <param name="dictData">要被無效化的老鼠</param>
    //private void ActiveMice(Dictionary<string, object> dictData) // 把按鈕變成無法使用 如果老鼠已Team中
    //{
    //    var dictEnableMice = Global.dictMiceAll.Except(dictData);

    //    foreach (KeyValuePair<string, object> item in dictData)
    //    {
    //        if (_dictLoadedMiceBtnRefs.ContainsKey(item.Key.ToString()))
    //            _dictLoadedMiceBtnRefs[item.Key.ToString()].SendMessage("DisableBtn");
    //    }

    //    foreach (KeyValuePair<string, object> item in dictEnableMice)
    //    {
    //        _dictLoadedMiceBtnRefs[item.Key.ToString()].SendMessage("EnableBtn");
    //    }
    //}
    //#endregion

    //#region -- Add2Refs 加入載入按鈕參考 --
    ///// <summary>
    ///// 加入載入按鈕參考
    ///// </summary>
    ///// <param name="loadedBtnRefs">按鈕參考字典</param>
    ///// <param name="itemID">按鈕ID</param>
    ///// <param name="myParent"></param>
    //void Add2Refs(Dictionary<string, GameObject> loadedBtnRefs, int position, string itemID, GameObject myParent)
    //{
    //    List<string> keys = loadedBtnRefs.Keys.ToList();

    //    // 檢查長度 防止溢位 position 初始值0
    //    if (position < loadedBtnRefs.Count && loadedBtnRefs.Count > 0)
    //    {
    //        // 如果已載入按鈕有重複Key
    //        if (loadedBtnRefs.ContainsKey(itemID))
    //        {
    //            // 如果Key值不同 移除舊資料
    //            if (keys[position] != itemID)
    //            {
    //                loadedBtnRefs.Remove(itemID);

    //                // 如果小於 載入按鈕的索引長度 直接修改索引 超過則新增
    //                if (position < loadedBtnRefs.Count)
    //                {
    //                    Global.RenameKey(loadedBtnRefs, keys[position], itemID);
    //                    loadedBtnRefs[itemID] = myParent;
    //                }
    //                else
    //                {
    //                    loadedBtnRefs.Add(itemID, myParent);
    //                }
    //            }
    //            else
    //            {
    //                // 重新索引按鈕對應位置
    //                loadedBtnRefs[itemID] = myParent;
    //            }
    //        }
    //        else
    //        {
    //            // 如果小於 載入按鈕的索引長度 直接修改索引
    //            Global.RenameKey(loadedBtnRefs, keys[position], itemID);
    //            loadedBtnRefs[itemID] = myParent;
    //        }
    //    }
    //    else
    //    {
    //        // 大於 載入按鈕的索引長度 則新增索引
    //        loadedBtnRefs.Add(itemID, myParent.gameObject);
    //    }

    //}
    //#endregion

    #region -- SwitchMemeber 交換隊伍成員 --
    /// <summary>
    /// 交換隊伍成員
    /// </summary>
    /// <param name="key1">要交換的物件Key值</param>
    /// <param name="key2">要交換的物件Key值</param>
    /// <param name="dict">來源資料字典</param>
    public void SwitchMemeber(string key1, string key2, Dictionary<string, object> dict)
    {
        Dictionary<string, GameObject> tmpDict = _dictLoadedMiceBtnRefs;

        if (dict == Global.dictTeam)
            tmpDict = _dictLoadedTeamBtnRefs;

        // 交換 clientData Key and Value
        Global.SwapDictKey(key1, key2, "x", dict);
        Global.SwapDictValueByKey(key1, key2, dict);

        // 交換 btnRefs Key and Value
        Global.SwapDictKey(key1, key2, "x", tmpDict);
        Global.SwapDictValueByKey(key1, key2, tmpDict);
    }
    #endregion

    #region --字典 檢查/取值 片段程式碼 --

    public GameObject GetLoadedMice(string miceID)
    {
        GameObject obj;
        if (_dictLoadedMiceBtnRefs.TryGetValue(miceID, out obj))
            return obj;
        return null;
    }

    public GameObject GetLoadedTeam(string miceID)
    {
        GameObject obj;
        if (_dictLoadedTeamBtnRefs.TryGetValue(miceID, out obj))
            return obj;
        return null;
    }
    #endregion

    public int GetMiceMemberCount()
    {
        return _dictLoadedMiceBtnRefs.Count;
    }

    public int GetTeamMemberCount()
    {
        return _dictLoadedTeamBtnRefs.Count;
    }
}
