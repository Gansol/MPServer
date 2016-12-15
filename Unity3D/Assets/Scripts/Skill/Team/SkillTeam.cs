using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillTeam : MonoBehaviour
{
    private bool upFlag;
    private bool downFlag;
    private float lerpSpeed = 8f;
    private float _lerpSpeed = 0.1f;
    private float _upDistance = 30f;
    private float _energyValue = 0.2f;

    void Start()
    {
        //skillType = 0;
        upFlag = true;
    }

    public void init(float lerpSpeed, float upDistance,float energyValue)
    {
        this._lerpSpeed = lerpSpeed;
        this._upDistance = upDistance;
        this._energyValue = energyValue;
    }

    // Update is called once per frame
    void Update()
    {
        if (BattleManager.energy >= _energyValue && upFlag)
        {
            StartCoroutine(AnimationUp());
        }

        if (BattleManager.energy < _energyValue && downFlag)
        {
            StartCoroutine(AnimationDown());
        }
    }

    public void OnClick()
    {
        if (BattleManager.energy >= _energyValue)
        {
            Global.photonService.SendSkill(transform.GetChild(0).name);
            GameObject.FindGameObjectWithTag("GM").GetComponent<BattleManager>().UpadateEnergy(-_energyValue);
        }
    }

    public IEnumerator AnimationUp()
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
        yield return null;
    }

    public IEnumerator AnimationDown() // 2   = 2 ~ 1
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
        yield return null;
    }
}
