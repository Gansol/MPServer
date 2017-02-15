using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillBtn : MonoBehaviour
{
    private short miceID;
    private bool upFlag;
    private bool downFlag;
    private float lerpSpeed = 8f;
    private float _lerpSpeed = 0.1f;
    private float _upDistance = 30f;
    private float _energyValue = 0.2f;

    private static int useTimes = 0;
    private static int maxTimes = 1;


    void Start()
    {
        //skillType = 0;
        upFlag = true;
        useTimes = 0;
    }

    public void init(short miceID,float lerpSpeed, float upDistance,float energyValue)
    {
        this.miceID = miceID;
        this._lerpSpeed = lerpSpeed;
        this._upDistance = upDistance;
        this._energyValue = energyValue;
        useTimes = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (BattleManager.energy >= _energyValue && upFlag)
        {
            AnimationUp();
        }

        if (BattleManager.energy < _energyValue && downFlag)
        {
            AnimationDown();
        }
    }

    public void OnClick()
    {
        if (enabled)
        {
            if (BattleManager.energy >= _energyValue)
            {
                Global.photonService.SendSkillMice(miceID);
                GameObject.FindGameObjectWithTag("GM").GetComponent<BattleManager>().UpadateEnergy(-_energyValue);
                useTimes++;
            }

            if (useTimes >= maxTimes)
            {
                // to do change Item iamge
                Debug.Log("Change Image");
                useTimes = 0;
                
                gameObject.SetActive(false);
                // 如果道具不足 不切換 不顯示
                gameObject.transform.parent.GetChild(1).gameObject.SetActive(true);
            }
        }
    }

    public void AnimationUp()
    {
        _lerpSpeed = Mathf.Lerp(_lerpSpeed, 1, lerpSpeed);
        if (transform.GetChild(0).localPosition.y + _lerpSpeed > _upDistance)
        {
            transform.GetChild(0).localPosition = new Vector3(0, _upDistance, 0);
            upFlag = false;
            downFlag = true;
        }
        else
        {
            transform.GetChild(0).localPosition += new Vector3(0, _lerpSpeed, 0);
        }
    }

    public void AnimationDown() // 2   = 2 ~ 1
    {
        _lerpSpeed = Mathf.Lerp(_lerpSpeed, 1, lerpSpeed);
        if (transform.GetChild(0).localPosition.y - 10 <= -_upDistance)
        {
            Vector3 _tmp;
            _tmp = new Vector3(0, -_upDistance, 0);
            transform.GetChild(0).localPosition = _tmp;
            downFlag = false;
            upFlag = true;
        }
        else
        {
            transform.GetChild(0).localPosition = Vector3.Slerp(transform.GetChild(0).localPosition, new Vector3(0, -_upDistance * 2, 0), Time.deltaTime * 1);
        }
    }
}
