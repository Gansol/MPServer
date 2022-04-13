using UnityEngine;
using System.Collections;

public class HeroMiceAnimState : IAnimatorState
{
    public HeroMiceAnimState(GameObject go, bool isBoss, float lerpSpeed, float upSpeed, float upDistance, float lifeTime)
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
                    if (animatorStateInfo.fullPathHash == Animator.StringToHash("Layer1.Hello") && animState != ENUM_AnimatorState.Died) 
                        m_Animator.Play("Idle");   // 老鼠開始吃東西
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
                        _bAnimationDown = true;
                        _bEating = true;
                    }
            }
            else if (animatorStateInfo.fullPathHash == Animator.StringToHash("Layer1.Eat"))
            {
                _animTime = animatorStateInfo.normalizedTime;
                animState = ENUM_AnimatorState.Eat;  
                if (!_bEating)        // 限制執行一次
                {
                    if (animatorStateInfo.normalizedTime > .8f && animatorStateInfo.normalizedTime < 1 && !m_Animator.IsInTransition(0))                       // 動畫撥放完畢時
                    {
                        animState = ENUM_AnimatorState.Died;  
                    }
                    else if (animatorStateInfo.normalizedTime > 1 && !m_Animator.IsInTransition(0))                       // 動畫撥放完畢時
                    {
                        Play(ENUM_AnimatorState.Died,m_go);
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
}
