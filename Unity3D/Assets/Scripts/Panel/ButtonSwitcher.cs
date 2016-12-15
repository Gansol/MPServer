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
 *  目前道具排序只有寫好一半(更新、取的資料完成) 實體化更新後的資料(未完成)
 * ***************************************************************
 *                           ChangeLog
 * 20160705 v1.0.0  0版完成，Team點兩下回彈還沒寫 
 * 20161029 v1.0.1  改變陣列搜尋至字典搜尋
 * ****************************************************************/
public class ButtonSwitcher : MPButton
{
    #region 欄位
    public GameObject team;

    public bool _activeBtn;                                   // 按鈕啟動狀態

    private GameObject _clone, _other, _pressingIcon;           // 複製物件、碰撞時對方物件、原老鼠位置
    private int _depth, teamCountMax;                          // UISprite深度
    private float _distance;                                  // 兩物件間距離
    private bool _isPress;   // 是否觸發、移動目標、是否按下、是否摧毀
    private Vector3 _originPos, _toPos;                       // 原始座標、目標作標
    //private PlayerManager pm;

    public Collider coll;
    #endregion

    void Awake()
    {
        //pm = GetComponentInParent<PlayerManager>();
        coll = GetComponent<Collider>();
        _originPos = transform.localPosition;
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
                if (tag == "Inventory")
                    gameObject.transform.GetComponentInParent<UIPanel>().clipping = UIDrawCall.Clipping.SoftClip;
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
        //pm = GetComponentInParent<PlayerManager>();
        if (_activeBtn)
        {
            if (!_isTrigged && _other != gameObject && _other.tag != tag && transform.parent.name == _other.transform.parent.name)    // 按下時發生碰撞 且 放開時 交換物件   OnPress>RetOrSwitch 所以已經放開了
            {
                SwitchBtn();
            }
            else if (_isTrigged && (_other.name != gameObject.name || _other.tag != tag))
            {
                SwitchBtn();
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
        //GameObject image = transform.GetComponentInChildren<UISprite>().gameObject;

        if (tag == "Inventory")
        {
            Destroy(gameObject);
            gameObject.transform.GetComponentInParent<UIPanel>().clipping = UIDrawCall.Clipping.SoftClip;
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
                string itemID = gameObject.name;

                gameObject.transform.localPosition = _originPos;
                _clone.name = "Item";
                EnDisableBtn(_clone, false);

                GameObject invButton = PlayerManager.dictLoadedItem[itemID];
                if (invButton.transform.parent.name != PanelManager._lastEmptyItemGroup.name)
                {
                    invButton.transform.parent.gameObject.SetActive(true);  // 白癡寫法
                    invButton.SendMessage("EnableBtn");
                    invButton.transform.parent.gameObject.SetActive(false); // // 白癡寫法
                }
                else
                {
                    invButton.SendMessage("EnableBtn");
                }
                PlayerManager.dictLoadedEquiped.Remove(itemID);

                Global.photonService.UpdatePlayerItem(short.Parse(itemID), false);

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
        string itemID = gameObject.name;
        string otherItemID = _other.name;

        if (_other.tag != "Untagged")
        {
            // A>B

            #region A>B
            if (tag == "Inventory" && tag != _other.tag && transform.parent.name == _other.transform.parent.name) // 如果 tag不同且 父系(道具類別)相同
            {
                // A>B B count=0
                if (_other.transform.GetChild(0).childCount == 0)
                {
                    _other.name = itemID;
                    _pressingIcon.transform.parent = _other.transform.Find("Image");
                    _pressingIcon.transform.localScale = Vector3.one;
                    _pressingIcon.transform.localPosition = Vector3.zero;

                    Destroy(_pressingIcon.GetComponent<TweenColor>());

                    _pressingIcon.SetActive(false); _pressingIcon.SetActive(true);
                    EnDisableBtn(_other, true);
                    EnDisableBtn(_clone, false);

                    PlayerManager.dictLoadedEquiped.Add(itemID, _other);   // 在A>B時 改變索引至Clone 、 B>A空倉庫時，加入道具索引
                    _other.GetComponentInChildren<UISprite>().depth -= Global.MeunObjetDepth;
                    Global.photonService.UpdatePlayerItem(short.Parse(itemID), true);
                }// A>B B has object > switch
                else if (_other.transform.GetChild(0).childCount != 0)
                {
                    _other.name = itemID;
                    PlayerManager.dictLoadedEquiped.Remove(otherItemID); PlayerManager.dictLoadedEquiped.Add(itemID, _other);
                    UISprite sprite = _other.GetComponentInChildren<UISprite>();
                    sprite.gameObject.name = sprite.spriteName = gameObject.GetComponentInChildren<UISprite>().spriteName;
                    if (PlayerManager.dictLoadedItem[otherItemID].activeSelf)
                        PlayerManager.dictLoadedItem[otherItemID].SendMessage("EnableBtn");
                    Destroy(gameObject);
                    EnDisableBtn(_clone, false);

                    Global.photonService.UpdatePlayerItem(short.Parse(itemID), true);
                    Global.photonService.UpdatePlayerItem(short.Parse(otherItemID), false);
                }
                gameObject.transform.GetComponentInParent<UIPanel>().clipping = UIDrawCall.Clipping.SoftClip;
            }
            #endregion

            #region A>A B>B
            else if (tag == _other.tag && tag == "Inventory") // && tag=="Inventory" 寫好實體化排序 和 裝備排序後去除
            {
                string otherName = _other.name;
                string spriteName = GetComponentInChildren<UISprite>().spriteName;

                Dictionary<string, object> playerItem = new Dictionary<string, object>(Global.SortedItem);

                Global.SwapDictValueByKey(itemID, otherName, playerItem);   // 交換值

                // 交換鍵
                Global.RenameKey(playerItem, itemID, "x");
                Global.RenameKey(playerItem, otherName, itemID);
                Global.RenameKey(playerItem, "x", otherName);

                // 交換載入物件位置(鍵)
                Global.RenameKey(PlayerManager.dictLoadedItem, itemID, "x");
                Global.RenameKey(PlayerManager.dictLoadedItem, otherName, itemID);
                Global.RenameKey(PlayerManager.dictLoadedItem, "x", otherName);

                // other name = my name
                _other.name = itemID;
                string otherSpriteName = _other.GetComponentInChildren<UISprite>().spriteName;
                _other.GetComponentInChildren<UISprite>().spriteName = spriteName;

                // my name = other name
                _clone.name = otherName;
                _clone.GetComponentInChildren<UISprite>().spriteName = otherSpriteName;

                Global.photonService.SortPlayerItem(playerItem);
            }
            #endregion
        }
        else
        {
            RetOrigin();
        }
        //}
        //else if (tag != "Inventory") // B>out
        //{
        //    GameObject invButton = PlayerManager.dictLoadedItem[gameObject.name];
        //    if (invButton.transform.parent.name != pm._lastEmptyItemGroup.name)
        //    {
        //        invButton.transform.parent.gameObject.SetActive(true);  // 白癡寫法
        //        invButton.SendMessage("EnableBtn");
        //        invButton.transform.parent.gameObject.SetActive(false); // // 白癡寫法
        //    }
        //    else
        //    {
        //        invButton.SendMessage("EnableBtn");
        //    }
        //    PlayerManager.dictLoadedEquiped.Remove(itemID);

        //    //string itemID = pm.GetItemIDFromName(imageName, Global.itemProperty);
        //    Global.photonService.UpdatePlayerItem(short.Parse(itemID), false);

        //    Destroy(_clone.GetComponentInChildren<UISprite>().gameObject);
        //    Destroy(gameObject);
        //}
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




}


// dictinary.FirstOrDefault(x => x.Value == "one").Key;
