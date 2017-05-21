using UnityEngine;
using System.Collections.Generic;
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
 * 負責按鈕間的交換作業
 *  NGUI BUG : Team交換時Tween會卡色
 * ***************************************************************
 *                           ChangeLog
 * 20160705 v1.0.0  0版完成，Team點兩下回彈還沒寫
 * 20161029 v1.0.1  改變陣列搜尋至字典搜尋
 * ****************************************************************/
public class TeamSwitcher : MonoBehaviour
{
    #region 欄位
    public GameObject team;

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
    public bool _activeBtn;                                   // 按鈕啟動狀態

    private GameObject _clone, _other;           // 複製物件、碰撞時對方物件、原老鼠位置
    private int _depth, teamCountMax;                          // UISprite深度
    private float _distance;                                  // 兩物件間距離
    private bool _isTrigged, _goTarget, _isPress, _destroy;   // 是否觸發、移動目標、是否按下、是否摧毀
    private Vector3 _originPos, _toPos;                       // 原始座標、目標作標

    TeamManager tm;
    #endregion

    void Awake()
    {
        tm = team.GetComponent<TeamManager>();
        _originPos = transform.localPosition;
        _goTarget = false;
        _activeBtn = true;
        _other = gameObject;
        teamCountMax = 5;
    }

    void Update()
    {
        if (transform.childCount == 0)  // 如果沒有圖片 取消交換、拖曳功能、按鈕功能
        {
            GetComponent<UIDragObject>().enabled = false;
            GetComponent<TeamSwitcher>().enabled = false;
            _activeBtn = false;

        }
        MoveMotion();
    }

    #region -- 滑鼠事件片段程式碼 --
    void OnPress(bool isPress)
    {
        if (_activeBtn)
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
                RetOrSwitch();
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
        if (_activeBtn)
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
            transform.localPosition = _toPos;                       // 移到正確作標上 因為Lerp永遠到不了 會差0.00001
            if (_other.tag == "TeamIcon" && tag == "MiceIcon")      // A>B
            {
                transform.GetChild(0).parent = _other.transform;    // MiceIcon移到TeamBtn內
                EnDisableBtn(_other, true);                         // 啟動TeamBtn功能
                Destroy(gameObject);                                // 移除移動的物件
                TeamSequence(_other, true);
            }
            // A>A B>B 
            _originPos = _toPos;                                    // B1>B2時會改變原始做標，所以要儲存移動後的座標位置
            _goTarget = false;                                      // 到達目標
        }

        // 按扭回彈/移動至時 clone刪除的時機 使用距離判斷
        if (Vector3.Distance(transform.localPosition, _toPos) <= Mathf.Round(_distance / 3) && _goTarget && _destroy && _clone != null) { Destroy(_clone); _destroy = false; }

        if (_goTarget)  // _goTarget=true 持續往目標移動
            transform.localPosition = Vector3.Lerp(transform.localPosition, _toPos, lerpSpeed);
    }
    #endregion

    #region -- RetOrSwitch 交換或返回選擇 --
    void RetOrSwitch()
    {
        if (!_isTrigged && _other != gameObject && _other.tag != "MiceIcon")    // 按下時發生碰撞 且 放開時 交換物件   OnPress>RetOrSwitch 所以已經放開了
        {
            SwitchBtn();
        }
        else if (_isTrigged && _other.name != gameObject.name && _other.tag != "MiceIcon")
        {
            SwitchBtn();
        }
        else if (!_isTrigged || _other.name == gameObject.name)  // 無碰撞且放開時 或撞到自己的分身 返回 
        {                                                        // _other.name == gameObject.name 
            RetOrigin();                                         // 因為TriggerExit時會=自己(沒撞到) 或撞到自己時
            Destroy(_clone);
        }
        else
        {
            RetOrigin();
        }
    }
    #endregion

    #region -- RetOrigin 返回原位 --
    void RetOrigin()
    {
        _toPos = _originPos;
        _distance = Vector3.Distance(transform.localPosition, _toPos);

        if (tag == "MiceIcon")
        {
            _other = gameObject;
            _goTarget = _destroy = true;
            TweenColor.Begin(this.gameObject, tweenColorSpeed, Color.white);
//            Debug.Log(transform.GetChild(0).GetComponent<UISprite>().depth);
            DepthManager.SwitchDepthLayer(gameObject, transform, -Global.MeunObjetDepth);
         //   Debug.Log(transform.GetChild(0).GetComponent<UISprite>().depth);
        }
        else if (tag == "TeamIcon")
        {
            if (_distance <= leftDist)  // B1>return
            {
                _other = gameObject;
                _goTarget = _destroy = true;
                TweenColor.Begin(this.gameObject, tweenColorSpeed, Color.white);
                DepthManager.SwitchDepthLayer(gameObject, transform, -Global.MeunObjetDepth);
                //Debug.Log("return.");
            }
            else if (_distance > leftDist)  // B1>out
            {

                string teamName = transform.GetChild(0).name;
                _other = gameObject;

                TeamSequence(gameObject, false);
                RemoveTeam(teamName);
                tm.dictLoadedMice[transform.GetChild(0).name].GetComponent<TeamSwitcher>().enabled = true; // 要先Active 才能SendMessage
                tm.dictLoadedMice[transform.GetChild(0).name].SendMessage("EnableBtn");                    // 啟動按鈕
                tm.dictLoadedTeam.Remove(teamName);                                                        // 移除隊伍參考

                gameObject.transform.localPosition = _originPos;
                Destroy(transform.GetChild(0).gameObject);
                Destroy(gameObject.GetComponent<TweenColor>());
                Destroy(_clone);
            }
        }
    }
    #endregion

    #region -- SwitchBtn 按鈕交換 --
    void SwitchBtn()
    {
        if (tag == "MiceIcon" && _other.tag == "TeamIcon" && _other != gameObject)    // A>B
        {
            string miceID = transform.GetChild(0).name;

            if (_other.transform.childCount == 0)                                   // 如果移動到Team的位置"沒有"Mice 移至Team
            {
                Mice2Team(miceID);
                AddTeam(miceID, null);

            }
            else if (_other.transform.childCount != 0 && tm.GetLoadedTeam(_other.transform.GetChild(0).name) != null) // 如果移動到Team的位置有Mice移至Team Team老鼠返回
            {
                string otherName = _other.transform.GetChild(0).name;
                tm.GetLoadedTeam(otherName).SendMessage("EnableBtn");               // 將移出的的老鼠回復至Mice並恢復Btn功能
                tm.dictLoadedTeam.Remove(otherName);                       // 之後移除參考
                Mice2Team(miceID);
                AddTeam(miceID, _other);
                Destroy(_other.transform.GetChild(0).gameObject);
            }
            else
            {
                Debug.LogError("A>B Bug!");
            }
        }
        else if (tag == "TeamIcon" && _other.tag == "TeamIcon") // B1 > B2
        {
            if (_other.transform.childCount == 1 && _other.name != gameObject.name)  // 有物件作交換
            {
                string myName = transform.GetChild(0).name;
                string otherName = _other.transform.GetChild(0).name;
                ExchangeObject(myName, otherName, Global.dictTeam);

                GameObject tmp = _other.transform.GetChild(0).gameObject;
                transform.GetChild(0).parent = _other.transform;
                tmp.transform.parent = transform;
                transform.GetChild(0).localPosition = Vector3.zero;

                _other.transform.GetChild(0).localPosition = Vector3.zero;
                _other.GetComponent<TeamSwitcher>()._toPos = _originPos;
                _other.transform.GetChild(0).GetComponent<UISprite>().depth -= _depth;
                _toPos = _originPos;

                Destroy(GetComponent<TweenColor>());            // NGUI BUG!!
                Destroy(_other.GetComponent<TweenColor>());     // NGUI BUG!!
                _goTarget = true;
                Destroy(_clone);
            }
            else
            {
                RetOrigin();
            }
        }
        else
        {
            RetOrigin();
            Debug.LogError("SwitchBtn Error!");//RetOrigin();
        }
    }
    #endregion

    #region -- Mice2Team 更新按鈕字典參考 --
    /// <summary>
    /// 老鼠移動至隊伍，更新按鈕字典參考(無紀錄順序)
    /// </summary>
    /// <param name="miceID"></param>
    void Mice2Team(string miceID)
    {
        tm.dictLoadedTeam.Add(miceID, _clone);   // 要加Team的_clone才對
        tm.dictLoadedMice[miceID] = _clone;      //重新參考至黑掉的Clone物件(新的Mice按鈕)，原本的Mice按鈕會被銷毀

        _other.GetComponent<TeamSwitcher>().enabled = true;
        _other.GetComponent<TeamSwitcher>().EnableBtn();

        _clone.GetComponent<TeamSwitcher>().DisableBtn();
        transform.GetChild(0).GetComponent<UISprite>().depth -= _depth; // 恢復深度值
        _goTarget = true;
        _destroy = false;
    }
    #endregion

    #region -- Move2Clone 當按下且移動時，產生Clone --
    /// <summary>
    /// 當按下且移動時，產生Clone
    /// </summary>
    void Move2Clone()
    {
        _clone = (GameObject)Instantiate(gameObject);
        EnDisableBtn(_clone, false);
        _clone.transform.parent = transform.parent;
        _clone.transform.localPosition = _originPos;
        _clone.transform.localScale = transform.localScale;
        _clone.name = gameObject.name;
        _clone.tag = gameObject.tag;
        _depth = DepthManager.SwitchDepthLayer(gameObject, transform, Global.MeunObjetDepth); // 移動時深度提到最高防止遮擋
    }
    #endregion

    #region -- TeamSequence 隊伍整理佇列 --
    /// <summary>
    /// 隊伍整理佇列
    /// </summary>
    /// <param name="teamRef">整理索引物件</param>
    /// <param name="bDragOut">拉入T 或 移出F</param>
    private void TeamSequence(GameObject teamRef, bool bDragOut)
    {
        Dictionary<string, object> team = new Dictionary<string, object>(Global.dictTeam);
        int btnNo = int.Parse(teamRef.name.Remove(0, teamRef.name.Length - 1));
       // string miceName = teamRef.transform.GetChild(0).name;

        if (bDragOut)
        {
            if (btnNo >= team.Count)
            {
                int offset;
                offset = team.Count == 0 ? 0 : 1;
                _other.transform.GetChild(0).parent = tm.infoGroupsArea[2].transform.GetChild(team.Count - offset);
                tm.infoGroupsArea[2].transform.GetChild(team.Count - offset).GetChild(0).localPosition = Vector3.zero;
                tm.infoGroupsArea[2].transform.GetChild(team.Count - offset).GetComponent<TeamSwitcher>().enabled = true;
                tm.infoGroupsArea[2].transform.GetChild(team.Count - offset).GetComponent<TeamSwitcher>().SendMessage("EnableBtn");
            }
        }
        else
        {
            if (btnNo < team.Count)                                           // 2<5
            {
                for (int i = 0; i < (team.Count - btnNo); i++)                  // 5-2=3  
                {
                    tm.infoGroupsArea[2].transform.GetChild(btnNo + i).GetChild(0).parent = tm.infoGroupsArea[2].transform.GetChild(btnNo + i - 1); // team[2+i]=team[2+i-1] parent =>team[2]=team[1]
                    if (i == 0)     // 因為第一個物件會有2個Child所以要GetChild(1) 下一個按鈕移過來的Mice
                        tm.infoGroupsArea[2].transform.GetChild(btnNo + i - 1).GetChild(1).localPosition = Vector3.zero;
                    else
                        tm.infoGroupsArea[2].transform.GetChild(btnNo + i - 1).GetChild(0).localPosition = Vector3.zero;
                    tm.infoGroupsArea[2].transform.GetChild(btnNo + i - 1).GetChild(0).localPosition = Vector3.zero;
                    tm.infoGroupsArea[2].transform.GetChild(btnNo + i - 1).GetComponent<TeamSwitcher>().enabled = true;
                    tm.infoGroupsArea[2].transform.GetChild(btnNo + i - 1).GetComponent<TeamSwitcher>().SendMessage("EnableBtn");
                }
                Global.dictTeam = team;
            }
        }
    }
    #endregion

    #region -- Mice2Click 雙擊老鼠 返回原始位置 (注意Mice Team不同) --
    void Mice2Click()
    {
        if (tag == "MiceIcon")
        {
            string miceName = transform.GetChild(0).name;

            if (!tm.dictLoadedTeam.ContainsKey(miceName) && tm.dictLoadedTeam.Count != teamCountMax)  // full and same
            {
                Dictionary<string, object> team = Global.dictTeam;
                GameObject teamBtn = tm.infoGroupsArea[2].transform.GetChild(team.Count).gameObject;

                Move2Clone();

                tm.dictLoadedTeam.Add(miceName, _clone);
                tm.dictLoadedMice[miceName] = _clone;
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
    /// <param name="dictServerData">對方的名稱</param>
    private void ExchangeObject(string myName, string otherName, Dictionary<string, object> dictServerData)
    {
        Global.SwapDictValueByKey(myName, otherName, dictServerData);
        Global.photonService.UpdateMiceData(Global.Account, Global.dictMiceAll, dictServerData);
    }
    #endregion

    #region -- AddTeam 交換/加入隊伍 --
    /// <summary>
    /// 交換/加入隊伍(JSON)(順序採自動累計)
    /// </summary>
    ///<param name="miceID">老鼠名稱</param>
    ///<param name="teamBtn">=null時為增加成員</param>
    private void AddTeam(string miceID, GameObject teamBtn)
    {
        Dictionary<string, object> dictTeam = new Dictionary<string, object>(Global.dictTeam);

        if (teamBtn != null)  // 交換隊伍成員
        {
            if (teamBtn.transform.childCount != 0)
            {
                string teamID = teamBtn.transform.GetChild(0).name;
                string spriteName = tm.GetLoadedMice(miceID).GetComponentInChildren<UISprite>().spriteName;

                dictTeam[teamID] = spriteName.Remove(spriteName.Length - 4);    
                Global.RenameKey(dictTeam, teamID, miceID);
                tm.dictLoadedMice[teamID].SendMessage("EnableBtn");
            }
        }
        else // 增加隊伍成員
        {
            object miceName = "";
            Global.dictMiceAll.TryGetValue(miceID, out miceName);
            dictTeam.Add(miceID, miceName);
        }
        Global.dictTeam = dictTeam;
        Global.photonService.UpdateMiceData(Global.Account, Global.dictMiceAll, dictTeam);
    }
    #endregion

    #region -- RemoveTeam 移除隊伍 --
    /// <summary>
    /// 減少隊伍數量(JSON)
    /// 【請先排序】後再進行移除
    /// </summary>
    private void RemoveTeam(string itemID)
    {
        // 移除隊伍成員
        Dictionary<string, object> dictTeam = Global.dictTeam;
        dictTeam.Remove(itemID);
        Global.dictTeam = dictTeam;
        Global.photonService.UpdateMiceData(Global.Account, Global.dictMiceAll, dictTeam);
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
        if (enable)
        {
            go.GetComponent<TeamSwitcher>().enabled = enable;
            go.GetComponent<TeamSwitcher>()._activeBtn = enable;
            TweenColor.Begin(go, tweenColorSpeed, Color.white);
        }
        else
        {
            go.GetComponent<TeamSwitcher>()._activeBtn = enable;
            go.GetComponent<TeamSwitcher>().enabled = enable;
            TweenColor.Begin(go, tweenColorSpeed, new Color(disableColor, disableColor, disableColor));
        }
        go.GetComponent<BoxCollider>().isTrigger = false;
        go.GetComponent<UIDragObject>().enabled = enable;
    }
    #endregion

    #region -- DisableBtn 關閉按鈕(外部呼叫) --
    public void DisableBtn()
    {
        TweenColor.Begin(this.gameObject, tweenColorSpeed, new Color(disableColor, disableColor, disableColor));
        GetComponent<UIDragObject>().enabled = false;
        GetComponent<BoxCollider>().isTrigger = false;
        GetComponent<TeamSwitcher>()._activeBtn = false;
        GetComponent<TeamSwitcher>().enabled = false;
        _isTrigged = false;
    }
    #endregion

    #region -- DisableBtn 關閉按鈕(外部呼叫) --
    public void EnableBtn()
    {
        GetComponent<TeamSwitcher>().enabled = true;
        GetComponent<TeamSwitcher>()._activeBtn = true;
        GetComponent<UIDragObject>().enabled = true;
        GetComponent<BoxCollider>().isTrigger = false;

        TweenColor.Begin(this.gameObject, tweenColorSpeed, Color.white);
        _isTrigged = false;
    }
    #endregion
}


// dictinary.FirstOrDefault(x => x.Value == "one").Key;
