using UnityEngine;
using System.Collections;

public class MiceAnimState : AnimatorState
{

    public MiceAnimState(GameObject obj, bool isBoss, float lerpSpeed, float upSpeed, float upDistance, float lifeTime)
        : base(obj, isBoss, lerpSpeed, upSpeed, upDistance, lifeTime)
    {
    }
    // private BattleManager battleManager = null;

    public override void UpdateAnimation()
    {
        _survivalTime = Time.time - _lastTime;
        if (_bMotion && (_upFlag && obj.transform.localPosition.y < _upDistance))        // AnimationUp
            AnimationUp();

        if (_bMotion && (_isDisappear && obj.transform.localPosition.y > -_upDistance)) // AnimationDown
            AnimationDown();



        Animator anims = obj.GetComponentInChildren<Animator>();   // 播放 死亡動畫
        if (anims != null)
        {
            AnimatorStateInfo currentState = anims.GetCurrentAnimatorStateInfo(0);      // 取得目前動畫狀態 (0) = Layer
            //Debug.Log("currentState : " + currentState.nameHash);
            if (currentState.nameHash == Animator.StringToHash("Layer1.Hello"))         // 如果 目前 動化狀態 是 up
            {
                _animTime = currentState.normalizedTime;

                // 目前播放的動畫 "總"時間
                if (_animTime >= _helloTime)   // 動畫撥放完畢時
                {
                    if (currentState.nameHash == Animator.StringToHash("Layer1.Hello") && animState != ENUM_AnimatorState.Die) anims.Play("Idle");   // 老鼠開始吃東西
                }
            }
            else if (currentState.nameHash == Animator.StringToHash("Layer1.Die"))              // 如果 目前 動畫狀態 是 die
            {
                _animTime = currentState.normalizedTime;                                         // 目前播放的動畫 "總"時間
                _upFlag = false;
                //Debug.Log("Die 1" + _bDead);
                if (!_bDead)       // 限制執行一次
                {
                    if (_animTime >= _deadTime / 3 && _animTime < _deadTime)
                    {
                        _isDisappear = true;
                    }
                    else if (_animTime >= _deadTime)   // 動畫撥放完畢時
                    {
                        animState = ENUM_AnimatorState.None;
                        _bDead = true;
                    }
                }
            }
            else if (currentState.nameHash == Animator.StringToHash("Layer1.Idle"))
            {
                _animTime = currentState.normalizedTime;

                if (!_bEating)        // 限制執行一次
                {
                    if (_animTime > _lifeTime && !_isBoss)                       // 動畫撥放完畢時
                    {
                        _isDisappear = true;
                        _bEating = true;
                    }
                }
            }
            else if (currentState.nameHash == Animator.StringToHash("Layer1.OnHit"))
            {
                _animTime = currentState.normalizedTime;
                animState = ENUM_AnimatorState.OnHit;

                if (_animTime >= .5f && _isBoss)                       // 動畫撥放完畢時
                {
                    animState = ENUM_AnimatorState.Idle;
                    obj.GetComponentInChildren<Animator>().Play("Idle");
                }
            }
        }
    }

    public override void Play(ENUM_AnimatorState animState)
    {
        Animator animator = obj.GetComponentInChildren<Animator>();
        this.animState = animState;

        switch (animState)
        {
            case ENUM_AnimatorState.Hello:
                animator.Play("Hello");
                break;
            case ENUM_AnimatorState.Idle:
                animator.Play("Idle");
                break;
            case ENUM_AnimatorState.Die:
                animator.Play("Die");
                break;
            case ENUM_AnimatorState.OnHit:
                animator.Play("OnHit");
                break;
            default:
                this.animState = ENUM_AnimatorState.None;
                Debug.Log("animState = None");
                break;
        }
    }

    private void AnimationUp()
    {
        //_upDistance = _isBoss ? obj.GetComponent<BoxCollider2D>().size.x * 0.4f : _upDistance;
        //float moveTo = obj.transform.localPosition.y + _upDistance;
        //iTween.MoveTo(obj, iTween.Hash("y", moveTo.ToString(), "time", "1", "easyType", "easeOutCirc"));
        _upDistance = _isBoss ? obj.GetComponent<BoxCollider2D>().size.x * 0.4f : _upDistance;
        _tmpSpeed = Mathf.Lerp(_tmpSpeed, 1, _lerpSpeed);

        if (obj.transform.localPosition.y + _tmpSpeed >= _upDistance)
        {
            obj.transform.localPosition = new Vector3(0, _upDistance, 0);
            _upFlag = false;
        }
        else
        {
            obj.transform.localPosition += new Vector3(0, _tmpSpeed, 0);
        }
    }

    private void AnimationDown() // 2   = 2 ~ 1
    {
        _tmpSpeed = Mathf.Lerp(_tmpSpeed, 20, _lerpSpeed);

        if (obj.transform.localPosition.y - 20 <= -_upDistance)
        {
            Vector3 _tmp;
            _tmp = new Vector3(0, -_upDistance, 0);
            obj.transform.localPosition = _tmp;
            _upDistance = _tmpDistance;
            obj.SendMessage("OnDead", _survivalTime);
            _isDisappear = false;
        }
        else
        {
            obj.transform.localPosition -= new Vector3(0, _tmpSpeed, 0);
            //transform.localPosition = Vector3.Slerp(transform.localPosition, new Vector3(0, -_upDistance * 2, 0), Time.deltaTime * 5);
        }
    }

}
