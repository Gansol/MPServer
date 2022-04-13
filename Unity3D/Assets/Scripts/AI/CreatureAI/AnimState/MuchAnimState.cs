using UnityEngine;
using System.Collections;

public class MuchAnimState : IAnimatorState {

    public MuchAnimState(GameObject go, bool isBoss, float lerpSpeed, float upSpeed, float upDistance, float lifeTime)
        : base(go, isBoss, lerpSpeed, upSpeed, upDistance, lifeTime)
    {
    }

    public override void Initialize()
    {
        base.Initialize();
        _tmpSpeed = _upAnimSpeed /= 10;
        _toFlag = _toScale = false;
    }

    public override void UpdateAnimation()
    {
        base.UpdateAnimation();

        if (m_Animator != null)
        {
            animatorStateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);      // 取得目前動畫狀態 (0) = Layer
            //Debug.Log("currentState : " + currentState.nameHash);
            if (animatorStateInfo.fullPathHash == Animator.StringToHash("Layer1.Hello"))         // 如果 目前 動化狀態 是 up
            {
                animState = ENUM_AnimatorState.Hello;
                _animTime = animatorStateInfo.normalizedTime;

                // 目前播放的動畫 "總"時間
                if (_animTime >= _helloTime)   // 動畫撥放完畢時
                {
                    if (animatorStateInfo.fullPathHash == Animator.StringToHash("Layer1.Hello") && animState != ENUM_AnimatorState.Died) m_Animator.Play("Idle");   // 老鼠開始吃東西
                }
            }
            else if (animatorStateInfo.fullPathHash == Animator.StringToHash("Layer1.Die"))              // 如果 目前 動畫狀態 是 die
            {
                _animTime = animatorStateInfo.normalizedTime;                                         // 目前播放的動畫 "總"時間
                _bAnimationUp = false;
                animState = ENUM_AnimatorState.Died;
                //Debug.Log("Die 1" + _bDead);
                if (!_bDead)       // 限制執行一次
                {
                    if (_animTime >= _deadTime / 3 && _animTime < _deadTime)
                    {
                        _bAnimationDown = true;
                    }
                    else if (_animTime >= _deadTime)   // 動畫撥放完畢時
                    {
                        animState = ENUM_AnimatorState.None;
                        _bDead = true;
                    }
                }
            }
            else if (animatorStateInfo.fullPathHash == Animator.StringToHash("Layer1.Idle"))
            {
                _animTime = animatorStateInfo.normalizedTime;

                    if ((_animTime > _lifeTime || _survivalTime > _lifeTime) && !_isBoss)                       // 動畫撥放完畢時
                    {
                        Play(ENUM_AnimatorState.Eat, m_go);
                    }
            }
            else if (animatorStateInfo.fullPathHash == Animator.StringToHash("Layer1.Eat"))
            {
                _animTime = animatorStateInfo.normalizedTime;
                if (!_bEating)        // 限制執行一次
                {
                    if (animatorStateInfo.normalizedTime > 1 && !m_Animator.IsInTransition(0))                       // 動畫撥放完畢時
                    {
                        Play(ENUM_AnimatorState.Died, m_go);
                        _bEating = true;
                    }
                }
            }
            else if (animatorStateInfo.fullPathHash == Animator.StringToHash("Layer1.OnHit"))
            {
                _animTime = animatorStateInfo.normalizedTime;
                animState = ENUM_AnimatorState.OnHit;

                if (_animTime >= .5f && _isBoss)                       // 動畫撥放完畢時
                {
                    animState = ENUM_AnimatorState.Idle;
                    Play(animState,m_go);
                }
            }
        }
    }

    public override void Play(ENUM_AnimatorState animState,GameObject go)
    {
        this.animState = animState;
        m_Animator = go.GetComponentInChildren<Animator>();

        switch (animState)
        {
            case ENUM_AnimatorState.Hello:
                m_Animator.Play("Hello");
                break;
            case ENUM_AnimatorState.Idle:
                m_Animator.Play("Idle");
                break;
            case ENUM_AnimatorState.Eat:
                m_Animator.Play("Eat");
                break;
            case ENUM_AnimatorState.Died:
                m_Animator.Play("Die");
                break;
            case ENUM_AnimatorState.OnHit:
                m_Animator.Play("OnHit");
                break;
            default:
                this.animState = ENUM_AnimatorState.None;
                Debug.Log("animState = None");
                break;
        }
    }

    protected override void  AnimationUp()
    {
        //base.AnimationUp();
        //_upDistance = _isBoss ? go.GetComponent<BoxCollider2D>().size.x * 0.4f : _upDistance;
        //float moveTo = go.transform.localPosition.y + _upDistance;
        //iTween.MoveTo(go, iTween.Hash("y", moveTo.ToString(), "time", "1", "easyType", "easeOutCirc"));
        _upDistance = _isBoss ? m_go.GetComponent<BoxCollider2D>().size.x * 0.4f : _upDistance;
        _tmpSpeed = Mathf.Lerp(_tmpSpeed, 1, _lerpSpeed);

        if (m_go.transform.localPosition.y + _tmpSpeed >= _upDistance)
        {
            m_go.transform.localPosition = new Vector3(0, _upDistance, 0);
            _bAnimationUp = false;
        }
        else
        {
            m_go.transform.localPosition += new Vector3(0, _tmpSpeed, 0);
        }
    }

    protected override void AnimationDown() // 2   = 2 ~ 1
    {
        _tmpSpeed = Mathf.Lerp(_tmpSpeed, 20, _lerpSpeed);

        if (m_go.transform.localPosition.y - 20 <= -_upDistance)
        {
            Vector3 _tmp;
            _tmp = new Vector3(0, -_upDistance, 0);
            m_go.transform.localPosition = _tmp;
            _upDistance = _defaultUpDistance;
            animState = ENUM_AnimatorState.Died;
            // go.SendMessage("OnDead", _survivalTime);
            //anims.StopPlayback();
            _bAnimationDown = false;
        }
        else
        {
            m_go.transform.localPosition -= new Vector3(0, _tmpSpeed, 0);
            //transform.localPosition = Vector3.Slerp(transform.localPosition, new Vector3(0, -_upDistance * 2, 0), Time.deltaTime * 5);
        }
    }

    protected override void AnimationTo()
    {
       // _tmpSpeed = Mathf.Lerp(_tmpSpeed, 1, _lerpSpeed);
        float distance = Vector3.Distance(m_go.transform.position, _toWorldPos);
        if (distance >= 0 && distance <= 0.05f)
        {
            m_go.transform.position = _toWorldPos;
            _toFlag = false;
        }
        else
        {
            m_go.transform.position = Vector3.Lerp(m_go.transform.position, _toWorldPos, _lerpSpeed);
        }
    }

    protected override void AnimationScale()
    {
        if (m_go.transform.localScale != _scale)
        {
            m_go.transform.localScale = Vector3.Lerp(m_go.transform.localScale, _scale, _lerpSpeed);
        }
    }

}
