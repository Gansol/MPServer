using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillBtn : MonoBehaviour
{
    AttrFactory attrFactory;
    MiceAttr attr;

    private bool _upFlag, _downFlag, _btnFlag;                                                          // 是否向上、是否向下、按鈕切換
    private byte _useTimes, _skillTimes,_miceUseCount;                                                                // 使用次數、技能次數上限、使用量
    private short _miceID, _itemID, _skillID, _skillType, _miceCount, _itemCount, _miceUsed, _itemUsed; // 老鼠ID、道具ID、技能ID、技能類別、老鼠數量、道具數量、老鼠使用量、道具使用量
    private float _upDistance = 30, _energyValue = 0.2f, _lerpSpeedParm1 = 0.1f, lerpSpeedParm2 = 8f;   // 上升速度、能量值、加速參數1、加速參數2


    // 初始化
    public void init(short miceID, float lerpSpeed, float upDistance, float energyValue)
    {
        attrFactory = new AttrFactory();
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
        _miceCount = System.Convert.ToInt16(ObjectFactory.GetColumnsDataFromID(Global.playerItem, "ItemCount", miceID.ToString()));
        _itemID = System.Convert.ToInt16(ObjectFactory.GetColumnsDataFromID(Global.miceProperty, "ItemID", miceID.ToString()));
        _itemCount = System.Convert.ToInt16(ObjectFactory.GetColumnsDataFromID(Global.playerItem, "ItemCount", _itemID.ToString()));
        _skillID = System.Convert.ToInt16(ObjectFactory.GetColumnsDataFromID(Global.itemProperty, "SkillID", _itemID.ToString()));
        _skillType = System.Convert.ToInt16(ObjectFactory.GetColumnsDataFromID(Global.dictSkills, "SkillType", _skillID.ToString()));
    }



    // Update is called once per frame
    void Update()
    {
        if (BattleManager.energy >= _energyValue && _upFlag)
            AnimationUp();

        if (BattleManager.energy < _energyValue && _downFlag)
            AnimationDown();
    }

    public void OnClick()
    {
        if (enabled)
        {
            if (!_btnFlag)
            {
                if (BattleManager.energy >= _energyValue && _miceCount > _miceUsed)
                {
                    Global.photonService.SendSkillMice(_miceID);
                    GameObject.FindGameObjectWithTag("GM").GetComponent<BattleManager>().UpadateEnergy(-_energyValue);
                    
                    _useTimes++;
                    _miceUsed++;
                    _miceUseCount++;

                    GameObject.FindGameObjectWithTag("GM").GetComponent<BattleManager>().UpdateUseCount(_miceID, _miceUseCount);
                    _btnFlag = !_btnFlag;
                }

                // to do change Item iamge
                if (_useTimes >= _skillTimes)
                {
                    Debug.Log("Change Image");
                    _useTimes = 0;

                    transform.GetChild(0).gameObject.SetActive(false);
                    // 如果道具不足 不切換 不顯示
                    if(_itemCount - _itemUsed !=0)
                        transform.GetChild(1).gameObject.SetActive(true);
                }
            }
            else
            {
                Global.photonService.SendSkillItem(System.Convert.ToInt16(_itemID), _skillType);
                transform.GetChild(0).gameObject.SetActive(true);
                // 如果道具不足 切換 顯示黑色
                transform.GetChild(1).gameObject.SetActive(false);
                _btnFlag = !_btnFlag;
                if (_miceCount - _miceUsed != 0) { }
                    // to do 黑掉
            }
        }
    }

    // 按鈕向上動畫
    public void AnimationUp()
    {
        _lerpSpeedParm1 = Mathf.Lerp(_lerpSpeedParm1, 1, lerpSpeedParm2);
        if (transform.GetChild(0).localPosition.y + _lerpSpeedParm1 > _upDistance)
        {
            transform.GetChild(0).localPosition = new Vector3(0, _upDistance, 0);
            _upFlag = false;
            _downFlag = true;
        }
        else
        {
            transform.GetChild(0).localPosition += new Vector3(0, _lerpSpeedParm1, 0);
        }
    }

    // 按鈕向下動畫
    public void AnimationDown() // 2   = 2 ~ 1
    {
        _lerpSpeedParm1 = Mathf.Lerp(_lerpSpeedParm1, 1, lerpSpeedParm2);
        if (transform.GetChild(0).localPosition.y - 10 <= -_upDistance)
        {
            Vector3 _tmp;
            _tmp = new Vector3(0, -_upDistance, 0);
            transform.GetChild(0).localPosition = _tmp;
            _downFlag = false;
            _upFlag = true;
        }
        else
        {
            transform.GetChild(0).localPosition = Vector3.Slerp(transform.GetChild(0).localPosition, new Vector3(0, -_upDistance * 2, 0), Time.deltaTime * 1);
        }
    }

    // 取得技能次數
    private byte GetSkillTimes()
    {
        MiceAttr attr = attrFactory.GetMiceProperty(_miceID.ToString());
        return attr.SkillTimes;
    }
}
