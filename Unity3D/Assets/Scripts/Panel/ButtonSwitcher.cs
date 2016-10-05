using UnityEngine;
using System.Collections.Generic;
using MiniJSON;
using System.Linq;
/* ***************************************************************
 * -----Copyright c 2015 Gansol Studio.  All Rights Reserved.-----
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
 * ****************************************************************/
public class ButtonSwitcher : MonoBehaviour
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
    public float leftDist = 100f;
    public bool _activeBtn;                                   // 按鈕啟動狀態

    private GameObject _clone, _other, _miceTarget, _pressingIcon;           // 複製物件、碰撞時對方物件、原老鼠位置
    private int _depth, teamCountMax;                          // UISprite深度
    private float _distance;                                  // 兩物件間距離
    private bool _isTrigged, _goTarget, _isPress, _destroy;   // 是否觸發、移動目標、是否按下、是否摧毀
    private Vector3 _originPos, _toPos;                       // 原始座標、目標作標
    private PlayerManager pm;

    public Collider coll;
    #endregion

    void Awake()
    {
        pm = GetComponentInParent<PlayerManager>();
        coll = GetComponent<Collider>();
        _originPos = transform.localPosition;
        _goTarget = false;
        _activeBtn = false;
        _other = gameObject;
        teamCountMax = 5;
    }

    void Update()
    {


    }

    #region -- 滑鼠事件片段程式碼 --
    void OnPress(bool isPress)
    {
        if (_activeBtn)
        {
            _isPress = isPress;
            if (isPress)
            {
                _originPos = transform.localPosition;
                GetComponent<BoxCollider>().isTrigger = true;
                TweenColor.Begin(gameObject, tweenColorSpeed, new Color(clickColor, clickColor, clickColor));
                Move2Clone();
                if (tag == "Inventory")
                {
                    PlayerManager.dictLoadedItem[_clone.name] = _clone;
                    gameObject.GetComponent<UIDragScrollView>().enabled = false;

                    gameObject.transform.GetComponentInParent<UIPanel>().clipping = UIDrawCall.Clipping.None;
                    gameObject.transform.GetChild(2).GetComponent<UISprite>().enabled = false;
                }
                else
                {
                    PlayerManager.dictLoadedEquiped[_clone.name] = _clone;
                }
                if (transform.Find("Image").childCount != 0)
                    _pressingIcon = gameObject.GetComponentInChildren<UISprite>().gameObject;
            }
            else
            {
                EnDisableBtn(_clone, true);
                GetComponent<BoxCollider>().isTrigger = false;
                Destroy(gameObject);
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
        if (_isPress && triggedObject != gameObject)            // 撞到的不是自己 _isTrigger =true
        {
            _other = triggedObject.gameObject;
            _isTrigged = true;
        }
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

    #region -- RetOrSwitch 交換或返回選擇 --
    void RetOrSwitch()
    {
        pm = GetComponentInParent<PlayerManager>();
        if (_activeBtn)
        {
            if (!_isTrigged && _other != gameObject && _other.tag != tag && transform.parent.name==_other.transform.parent.name)    // 按下時發生碰撞 且 放開時 交換物件   OnPress>RetOrSwitch 所以已經放開了
            {
                SwitchBtn();
            }
            else if (_isTrigged && (_other.name != gameObject.name || _other.tag != tag))
            {
                SwitchBtn();
                Debug.Log("BUG!!!!!!!!!!");
            }
            else if (!_isTrigged || _other.name == gameObject.name)  // 無碰撞且放開時 或撞到自己的分身 返回 
            {                                                        // _other.name == gameObject.name 
                RetOrigin();                                         // 因為TriggerExit時會=自己(沒撞到) 或撞到自己時
                //Destroy(_clone);
            }
            else
            {
                RetOrigin();
            }
        }
    }
    #endregion

    #region -- RetOrigin 返回原位 --
    void RetOrigin()
    {
        _distance = Vector3.Distance(transform.localPosition, _originPos);
        GameObject image = transform.GetComponentInChildren<UISprite>().gameObject;

        if (tag == "Inventory")
        {
            Destroy(gameObject);
            EnDisableBtn(_clone, true);
        }
        else if (tag == "Item" || tag == "Equip")
        {
            if (_distance <= leftDist)  // B1>return
            {
                TweenColor.Begin(this.gameObject, tweenColorSpeed, Color.white);
                transform.GetComponentInChildren<UISprite>().depth -= _depth; //有問題 UnityException: Transform child out of bounds
                //Debug.Log("return.");
            }
            else if (_distance > leftDist && !_isTrigged)  // B1>out
            {
                string imageName = image.name.Remove(image.name.Length - 4);
                // TeamSequence(gameObject, false);
                //RemoveTeam();
                //PlayerManager.dictLoadedItem[image.name].GetComponent<TeamSwitcher>().enabled = true; // 要先Active 才能SendMessage
                //PlayerManager.dictLoadedItem[image.name].SendMessage("EnableBtn");                    // 啟動按鈕
                //PlayerManager.dictLoadedEquiped.Remove(image.name);                                                        // 移除隊伍參考

                gameObject.transform.localPosition = _originPos;
                _clone.name = "Item";
                EnDisableBtn(_clone, false);
                
                PlayerManager.dictLoadedItem[gameObject.name].SendMessage("EnableBtn");
                PlayerManager.dictLoadedEquiped.Remove(imageName);

                string itemID = pm.GetItemIDFromName(imageName, Global.itemProperty);
                Global.photonService.UpdatePlayerItem(System.Convert.ToInt16(itemID), false);

                Destroy(_clone.GetComponentInChildren<UISprite>().gameObject);
                Destroy(gameObject);

            }
        }
    }
    #endregion

    void SwitchICON()
    {
        string imageName = _pressingIcon.name.Remove(_pressingIcon.name.Length - 4);
        _other.name = imageName;
        _pressingIcon.transform.parent = _other.transform.Find("Image");
        _pressingIcon.transform.localScale = Vector3.one;
        _pressingIcon.transform.localPosition = Vector3.zero;
    }

    void SwitchBtn()
    {// A>B B>A A>A B>B
        if (_other.tag != "Untagged")
        {
            bool activeBtn = _other.GetComponent<ButtonSwitcher>()._activeBtn;
            string imageName = _pressingIcon.name.Remove(_pressingIcon.name.Length - 4);


            if (tag != _other.tag)
            {   // A>B B=empty
                if (_other.transform.GetChild(0).childCount == 0)   // 要設定 Equip不能和道具交換
                {
                    _other.name = imageName;
                    _pressingIcon.transform.parent = _other.transform.Find("Image");
                    _pressingIcon.transform.localScale = Vector3.one;
                    _pressingIcon.transform.localPosition = Vector3.zero;
                    Destroy(_pressingIcon.GetComponent<TweenColor>());
                    _pressingIcon.SetActive(false); _pressingIcon.SetActive(true);
                    EnDisableBtn(_other, true);

                    EnDisableBtn(_clone, false);
                    if (tag == "Inventory")
                        PlayerManager.dictLoadedEquiped.Add(imageName, _other);   // 在A>B時 改變索引至Clone 、 B>A空倉庫時，加入道具索引

                   string itemID= pm.GetItemIDFromName (imageName, Global.itemProperty);

                   Global.photonService.UpdatePlayerItem(System.Convert.ToInt16(itemID), true);
                }
                else // A>B B!=empty
                {// 道具欄位上有物件，交換物件
                    string otherName = _other.GetComponentInChildren<UISprite>().spriteName.Remove(_other.GetComponentInChildren<UISprite>().spriteName.Length - 4);
                    if (tag == "Inventory" && _other.GetComponent<ButtonSwitcher>()._activeBtn)
                    {
                        _other.name = imageName;
                        PlayerManager.dictLoadedEquiped.Remove(otherName); PlayerManager.dictLoadedEquiped.Add(imageName, _other);
                        UISprite sprite = _other.GetComponentInChildren<UISprite>();
                        sprite.gameObject.name = sprite.spriteName = gameObject.GetComponentInChildren<UISprite>().spriteName;
                        PlayerManager.dictLoadedItem[otherName].SendMessage("EnableBtn");
                        Destroy(gameObject);
                        EnDisableBtn(_clone, false);

                        string itemID = pm.GetItemIDFromName(imageName, Global.itemProperty);
                        Global.photonService.UpdatePlayerItem(System.Convert.ToInt16(itemID), false);

                        itemID = pm.GetItemIDFromName(otherName, Global.itemProperty);
                        Global.photonService.UpdatePlayerItem(System.Convert.ToInt16(itemID), true);
                    }
                    else if (_other.tag == "Inventory")
                    {
                        PlayerManager.dictLoadedEquiped.Remove(imageName);
                        PlayerManager.dictLoadedItem[imageName].SendMessage("EnableBtn");
                        Destroy(gameObject);
                        Debug.Log("BBBBUUUUUUUUGGGGGGGGGG!!!!!!!!!");
                    }
                    else
                    {
                        RetOrigin();
                    }
                }
            }
            else if (tag == _other.tag)//A1>A2 B1>B2 has object
            { //avtive

                

                if (_other.GetComponent<ButtonSwitcher>()._activeBtn)
                {
                    string otherName = _other.GetComponentInChildren<UISprite>().spriteName.Remove(_other.GetComponentInChildren<UISprite>().spriteName.Length - 4);
                    string spriteName = _other.GetComponentInChildren<UISprite>().spriteName;
                    UISprite sprite = _other.GetComponentInChildren<UISprite>();
                    sprite.gameObject.name = sprite.spriteName = GetComponentInChildren<UISprite>().spriteName;

                    sprite = _clone.GetComponentInChildren<UISprite>();
                    sprite.gameObject.name = sprite.spriteName = spriteName;
                    
                    _other.name = imageName;
                    _clone.name = otherName;

                    if (tag == "Inventory")
                    {
                        PlayerManager.dictLoadedItem[imageName] = _other;
                        PlayerManager.dictLoadedItem[otherName] = _clone;
                    }
                }// not active A1>A2 empty object
                else if (!_other.GetComponent<ButtonSwitcher>()._activeBtn)
                {
                    if (_other.transform.Find("Image").childCount == 0)
                    {
                        _clone.name = "Item";
                        GameObject tmp = _clone.GetComponentInChildren<UISprite>().gameObject;
                        _clone.GetComponent<TweenColor>().from = Color.white;
                        _clone.GetComponentInChildren<UISprite>().transform.parent = _other.transform.Find("Image");
                        tmp.transform.localPosition = Vector3.zero;
                        tmp.SetActive(false); tmp.SetActive(true);
                        _other.name = imageName;

                        if (_other.GetComponent<TweenColor>()) Destroy(_other.GetComponent<TweenColor>());

                        _other.GetComponentInChildren<UISprite>().color = Color.white;
                        EnDisableBtn(_clone, false);
                        EnDisableBtn(_other, true);
                    }
                }
                else
                {
                    RetOrigin();
                }
            }
            else
            {
                RetOrigin();
            }
        }
    }


    //void SortBtn()
    //{
    //    GameObject _tmp;
    //    _tmp = _other.transform.GetComponentInChildren<UI2DSprite>().gameObject;
    //    _pressingIcon.transform.parent  = _other.transform;
    //    _pressingIcon.transform.localScale = Vector3.one;
    //    _pressingIcon.transform.localPosition = Vector3.zero;

    //    _tmp.transform.parent = transform;
    //    _tmp.transform.localScale = Vector3.one;
    //    _tmp.transform.localPosition = Vector3.zero;
    //}

    #region -- Move2Clone 當按下且移動時，產生Clone --
    /// <summary>
    /// 當按下且移動時，產生Clone
    /// </summary>
    void Move2Clone()
    {
        if (_activeBtn)
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
    }
    #endregion



    #region -- EnDisableBtn 啟動/關閉按鈕(內部使用) --
    /// <summary>
    /// 改變物件功能 開/關
    /// </summary>
    /// <param name="go">生效物件</param>
    /// <param name="enable">功能開關</param>
    void EnDisableBtn(GameObject go, bool enable)
    {
        if (enable)
        {
            if (tag == "Inventory")
                gameObject.transform.GetComponentInParent<UIPanel>().clipping = UIDrawCall.Clipping.SoftClip;
            //go.GetComponent<UIDragScrollView>().enabled = enable;
            go.GetComponent<ButtonSwitcher>().enabled = enable;
            go.GetComponent<ButtonSwitcher>()._activeBtn = enable;
            go.GetComponent<UIDragObject>().enabled = enable;
            TweenColor.Begin(go, tweenColorSpeed, Color.white);
        }
        else
        {
            go.GetComponent<ButtonSwitcher>()._activeBtn = enable;
            go.GetComponent<ButtonSwitcher>().enabled = enable;
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
        GetComponent<ButtonSwitcher>()._activeBtn = false;
        GetComponent<ButtonSwitcher>().enabled = false;
        GetComponent<UIDragObject>().enabled = false;
        GetComponent<BoxCollider>().isTrigger = false;
        _isTrigged = false;
    }
    #endregion

    #region -- EnableBtn 關閉按鈕(外部呼叫) --
    public void EnableBtn()
    {
        TweenColor.Begin(this.gameObject, tweenColorSpeed, Color.white);
        GetComponent<ButtonSwitcher>().enabled = true;
        GetComponent<ButtonSwitcher>()._activeBtn = true;
        GetComponent<UIDragObject>().enabled = true;
        GetComponent<BoxCollider>().isTrigger = false;
        _isTrigged = false;
    }
    #endregion
}


// dictinary.FirstOrDefault(x => x.Value == "one").Key;
