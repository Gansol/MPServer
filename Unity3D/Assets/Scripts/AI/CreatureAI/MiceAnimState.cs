using UnityEngine;
using System.Collections;

public class MiceAnimState : IAnimatorState
{
    private bool _toFlag, _toScale;
    private Vector3 _toWorldPos, _scale;

    public MiceAnimState(GameObject obj, bool isBoss, float lerpSpeed, float upSpeed, float upDistance, float lifeTime)
        : base(obj, isBoss, lerpSpeed, upSpeed, upDistance, lifeTime)
    {
        anims = obj.GetComponentInChildren<Animator>();
        if (anims != null)
            currentState = anims.GetCurrentAnimatorStateInfo(0);
        _tmpSpeed = _upSpeed = upSpeed / 10;
    }
    // private BattleManager battleManager = null;

    public void SetToPos(Vector3 toWorldPos)
    {
        _toWorldPos = toWorldPos;
        _toFlag = true;
    }

    public void SetToScale(Vector3 scale)
    {
       
        _scale = scale;
        _toScale = true;
    }

    public override void UpdateAnimation()
    {
        //anims = obj.GetComponentInChildren<Animator>();
        _survivalTime = Time.time - _lastTime;
        if (_bMotion && !_toFlag && (_upFlag && obj.transform.localPosition.y < _upDistance))        // AnimationUp
            AnimationUp();

        if (_bMotion && !_toFlag && (_isDisappear && obj.transform.localPosition.y > -_upDistance)) // AnimationDown
            AnimationDown();

        if (_bMotion && _toFlag && (Vector3.Distance(obj.transform.position, _toWorldPos) > 0)) // AnimationTo
            AnimationTo();

        if (_bMotion && _toScale && (Vector3.Distance(obj.transform.position, _toWorldPos) > 0)) // AnimationScale
            AnimationScale();

        if (anims != null)
        {
            currentState = anims.GetCurrentAnimatorStateInfo(0);      // 取得目前動畫狀態 (0) = Layer
            //Debug.Log("currentState : " + currentState.nameHash);
            if (currentState.nameHash == Animator.StringToHash("Layer1.Hello"))         // 如果 目前 動化狀態 是 up
            {
                animState = ENUM_AnimatorState.Hello;
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
                animState = ENUM_AnimatorState.Die;
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
                    if ((_animTime > _lifeTime || _survivalTime > _lifeTime) && !_isBoss)                       // 動畫撥放完畢時
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
                    Play(animState);
                }
            }
        }
    }

    public override void Play(ENUM_AnimatorState animState)
    {
        this.animState = animState;
        anims = obj.GetComponentInChildren<Animator>();

        switch (animState)
        {
            case ENUM_AnimatorState.Hello:
                anims.Play("Hello");
                break;
            case ENUM_AnimatorState.Idle:
                anims.Play("Idle");
                break;
            case ENUM_AnimatorState.Die:
                anims.Play("Die");
                break;
            case ENUM_AnimatorState.OnHit:
                anims.Play("OnHit");
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
            //anims.StopPlayback();
            _isDisappear = false;
        }
        else
        {
            obj.transform.localPosition -= new Vector3(0, _tmpSpeed, 0);
            //transform.localPosition = Vector3.Slerp(transform.localPosition, new Vector3(0, -_upDistance * 2, 0), Time.deltaTime * 5);
        }
    }

    private void AnimationTo()
    {
        // _tmpSpeed = Mathf.Lerp(_tmpSpeed, 1, _lerpSpeed);
        float distance = Vector3.Distance(obj.transform.GetChild(0).transform.position, _toWorldPos);
        if (distance >= 0 && distance <= 0.05f)
        {
            obj.transform.GetChild(0).transform.position = _toWorldPos;
            _toFlag = false;
        }
        else
        {
            /* if (obj.name != "10001")*/
            //Debug.Log(obj.transform.parent.name + "\nPos:" + obj.transform.GetChild(0).transform.position+"\nLerp:" + Vector3.Lerp(obj.transform.GetChild(0).transform.position, _toWorldPos, _lerpSpeed));
            obj.transform.GetChild(0).transform.position = Vector3.Lerp(obj.transform.GetChild(0).transform.position, _toWorldPos, _lerpSpeed);
        }
    }

    private void AnimationScale()
    {
        if (obj.transform.localScale == _scale)
            _toScale = false;
        obj.transform.localScale = Vector3.Lerp(obj.transform.localScale, _scale, 0.1f);

    }
    public override void Init(GameObject obj, bool isBoss, float lerpSpeed, float upSpeed, float upDistance, float lifeTime)
    {
        animState = ENUM_AnimatorState.None;
        _isBoss = isBoss;
        _lerpSpeed = lerpSpeed;
        _tmpDistance = _upDistance = upDistance;
        _lifeTime = lifeTime;

        _lastTime = Time.time;
        this.obj = obj;
        _upFlag = true;
        _bMotion = true;
        _bDead = false;
        _isDisappear = false;
        _bEating = false;
        _tmpSpeed = _upSpeed = upSpeed / 10;
        _toFlag = _toScale = false;
    }
}
