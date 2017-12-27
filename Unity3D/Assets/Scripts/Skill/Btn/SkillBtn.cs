using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillBtn : MonoBehaviour
{
    MiceAttr attr;

    private int _energyValue;
    private bool _upFlag, _downFlag, _btnFlag, bClick;                                                          // 是否向上、是否向下、按鈕切換
    private byte _useTimes, _skillTimes, _miceUseCount, _cost;                                                                // 使用次數、技能次數上限、使用量
    private short _miceID, _itemID, _skillID, _skillType, _miceCount, _itemCount, _miceUsed, _itemUsed; // 老鼠ID、道具ID、技能ID、技能類別、老鼠數量、道具數量、老鼠使用量、道具使用量
    private float _upDistance = 30, _lerpSpeedParm1 = 0.1f, lerpSpeedParm2 = 8f;   // 上升速度、能量值、加速參數1、加速參數2

    private UILabel label;
    private BattleManager battleManager;
    UISprite sprite;
    // 初始化
    public void init(short miceID, float lerpSpeed, float upDistance, int energyValue)
    {
        attr = new MiceAttr();

        _upFlag = true;
        _downFlag = _btnFlag = false;
        _useTimes = 0;
        _miceUsed = _itemUsed = 0;
        _btnFlag = false;

        _miceID = miceID;
        _lerpSpeedParm1 = lerpSpeed;
        _upDistance = upDistance;
        _energyValue = energyValue;

        _skillTimes = GetSkillTimes();
        _miceCount = System.Convert.ToInt16(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.playerItem, "ItemCount", miceID.ToString()));
        _itemID = System.Convert.ToInt16(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.miceProperty, "ItemID", miceID.ToString()));
        _itemCount = System.Convert.ToInt16(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.playerItem, "ItemCount", _itemID.ToString()));
        _skillID = System.Convert.ToInt16(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.itemProperty, "SkillID", _itemID.ToString()));
        _skillType = System.Convert.ToInt16(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.dictSkills, "SkillType", _skillID.ToString()));
        _cost = System.Convert.ToByte(MPGFactory.GetObjFactory().GetColumnsDataFromID(Global.miceProperty, "MiceCost", miceID.ToString()));
        Debug.Log("ItemID:" + _itemID + "  " + _itemCount +"這還沒寫好 進入對戰時 道具不足要顯示黑的");
        //if(energyValue=4)
        sprite = transform.parent.Find("SkillBg").GetChild(_energyValue).GetComponent<UISprite>();
    }

    void Start()
    {
        label = GetComponentInChildren<UILabel>();
        battleManager = GameObject.FindGameObjectWithTag("GM").GetComponent<BattleManager>();
        label.text = _useTimes.ToString() + " / " + _skillTimes.ToString();

//        Debug.Log("**************************Item Name*************************:" + transform.GetChild(2).gameObject.name);
    }

    // Update is called once per frame
    void Update()
    {

        if (BattleManager.Energy  >= _cost)
            AnimationColor(true);
        if (BattleManager.Energy < _cost)
            AnimationColor(false);

        //if (BattleManager.energy >= _energyValue && _upFlag)
        //    AnimationUp();

        //if (BattleManager.energy < _energyValue && _downFlag)
        //    AnimationDown();
    }

    public void OnClick()
    {



        // 防止狂暗的白癡判斷 要改
        if (enabled && !bClick)
        {
            bClick = !bClick;
            if (!_btnFlag)
            {
                // 能量是否足夠使用技能
                if (BattleManager.Energy >= _cost && _miceCount > _miceUsed)
                {
                    Global.photonService.SendSkillMice(_miceID, _cost);
                    battleManager.UpadateEnergy(-_cost);
                    
                    _useTimes++;
                    _miceUsed++;

                    Dictionary<string, object> data = battleManager.dictMiceUseCount[_miceID.ToString()];
                    data["UseCount"] = _miceUsed;
                }

                // to do change Item iamge
                if (_useTimes == _skillTimes)
                {
                    _useTimes = 0;
                    if (_itemCount > _itemUsed)
                    {
                        transform.GetChild(1).gameObject.SetActive(false);
                       
                        // 如果道具不足 不切換 不顯示

                        transform.GetChild(2).gameObject.SetActive(true);
                        _btnFlag = !_btnFlag;
                    }
                }
            }
            else if (_itemCount > _itemUsed)
            {
                Global.photonService.SendSkillItem(System.Convert.ToInt16(_itemID), _skillType);
                transform.GetChild(1).gameObject.SetActive(true);
                // 如果道具不足 切換 顯示黑色
                transform.GetChild(2).gameObject.SetActive(false);
                _itemUsed++;

                Dictionary<string, object> data = battleManager.dictItemUseCount[_itemID.ToString()];
                data["UseCount"] = _miceUsed;

                _btnFlag = !_btnFlag;
                //if (_miceCount - _miceUsed != 0) { }
                // to do 黑掉
            }

            label.text = _useTimes.ToString() + " / " + _skillTimes.ToString();
            bClick = !bClick;
        }



        //if (enabled && !bClick)
        //{
        //    bClick = true;
        //    if (!_btnFlag)
        //    {
        //        if (BattleManager.energy *100 >= _cost && _miceCount > _miceUsed)
        //        {
        //            Global.photonService.SendSkillMice(_miceID);
        //            battleManager.UpadateEnergy(-_cost / 100f);
        //            _useTimes++;
        //            _miceUsed++;
        //            label.text = _useTimes.ToString() + " / " + _skillTimes.ToString();
        //        }

        //        // to do change Item iamge
        //        if (_useTimes == _skillTimes)
        //        {
        //            Debug.Log("Change Image");
        //            _useTimes = 0;
        //            transform.GetChild(1).gameObject.SetActive(false);
        //            // 如果道具不足 不切換 不顯示
        //            if (_itemCount - _itemUsed != 0)
        //                transform.GetChild(2).gameObject.SetActive(true);

        //            _btnFlag = !_btnFlag;
        //        }
        //    }
        //    else
        //    {
        //        Global.photonService.SendSkillItem(System.Convert.ToInt16(_itemID), _skillType);
        //        transform.GetChild(1).gameObject.SetActive(true);
        //        // 如果道具不足 切換 顯示黑色
        //        transform.GetChild(2).gameObject.SetActive(false);
        //        _btnFlag = !_btnFlag;
        //        if (_miceCount - _miceUsed != 0) { }
        //        // to do 黑掉
        //    }
        //}
        //bClick = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="colored">是否染色</param>
    private void AnimationColor(bool colored)
    {
        Color color = sprite.color;
        if (colored)
            sprite.color = Color.Lerp(new Color(color.r, color.g, color.b), new Color(color.r, color.g, .33f), 0.1f);
        else
            sprite.color = Color.Lerp(new Color(color.r, color.g, color.b), new Color(color.r, color.g, .7255f), 0.1f);
    }

    //// 按鈕向上動畫
    //public void AnimationUp()
    //{
    //    _lerpSpeedParm1 = Mathf.Lerp(_lerpSpeedParm1, 1, lerpSpeedParm2);
    //    if (transform.GetChild(0).localPosition.y + _lerpSpeedParm1 > _upDistance)
    //    {
    //        transform.GetChild(0).localPosition = new Vector3(0, _upDistance, 0);
    //        _upFlag = false;
    //        _downFlag = true;
    //    }
    //    else
    //    {
    //        transform.GetChild(0).localPosition += new Vector3(0, _lerpSpeedParm1, 0);
    //    }
    //}

    //// 按鈕向下動畫
    //public void AnimationDown() // 2   = 2 ~ 1
    //{
    //    _lerpSpeedParm1 = Mathf.Lerp(_lerpSpeedParm1, 1, lerpSpeedParm2);
    //    if (transform.GetChild(0).localPosition.y - 10 <= -_upDistance)
    //    {
    //        Vector3 _tmp;
    //        _tmp = new Vector3(0, -_upDistance, 0);
    //        transform.GetChild(0).localPosition = _tmp;
    //        _downFlag = false;
    //        _upFlag = true;
    //    }
    //    else
    //    {
    //        transform.GetChild(0).localPosition = Vector3.Slerp(transform.GetChild(0).localPosition, new Vector3(0, -_upDistance * 2, 0), Time.deltaTime * 1);
    //    }
    //}

    // 取得技能次數
    private byte GetSkillTimes()
    {
        MiceAttr attr = MPGFactory.GetAttrFactory().GetMiceProperty(_miceID.ToString());
        return attr.SkillTimes;
    }
}
