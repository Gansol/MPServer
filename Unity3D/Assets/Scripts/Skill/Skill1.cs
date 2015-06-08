using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Skill1 : MonoBehaviour
{
    public GameObject gameManager;

    private double _energy;
    private bool flag;
    private bool upFlag;
    private bool downFlag;
    private float lerpSpeed = 8f;
    private float _lerpSpeed = 0.1f;
    private float _upDistance = 30f;

    void Start()
    {
        _energy = 0;
        upFlag = true;
        flag = true;
    }

    // Update is called once per frame
    void Update()
    {
        _energy = gameManager.GetComponent<BattleManager>().energy;

        if (_energy >= 0.2f && upFlag)
        {
            StartCoroutine(AnimationUp());
        }

        if (_energy < 0.2f && downFlag)
        {
            StartCoroutine(AnimationDown());
        }
    }

    void OnHit()
    {
        if (_energy >= 0.2f)
        {
            Global.photonService.SendSkill(transform.GetChild(0).name);
            gameManager.GetComponent<BattleManager>().UpadateEnergy(-0.2f);
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
