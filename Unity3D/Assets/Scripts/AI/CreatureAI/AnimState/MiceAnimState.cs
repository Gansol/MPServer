using UnityEngine;
using System.Collections;

public class MiceAnimState : IAnimatorState
{
    public MiceAnimState(GameObject go, bool isBoss, float lerpSpeed, float upSpeed, float upDistance, float lifeTime)
        : base(go, isBoss, lerpSpeed, upSpeed, upDistance, lifeTime)
    {
        m_go = go;
    }
    // private BattleManager battleManager = null;

    public override void Initialize()
    {
        base.Initialize();
        _tmpSpeed = _upAnimSpeed /= 10;
    }

    public override void UpdateAnimation()
    {
        base.UpdateAnimation();

        if (m_Animator != null)
        {
            // Debug.Log("UpdateAnimation");
            animatorStateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);      // 取得目前動畫狀態 (0) = Layer

            if (animatorStateInfo.fullPathHash == Animator.StringToHash("Layer1.Hello"))         // 如果 目前 動化狀態 是打招呼 (up)
            {
                //   Debug.Log("currentState : Hello");
                animState = ENUM_AnimatorState.Hello;
                _animTime = animatorStateInfo.normalizedTime;

                // 目前播放的動畫 "總"時間 > 打招呼時間 (動畫撥放完畢時)  且 非 死亡狀態
                if (_animTime >= _helloTime && animState != ENUM_AnimatorState.Died)
                    m_Animator.Play("Idle");   // 老鼠開始吃東西
            }

            if (animatorStateInfo.fullPathHash == Animator.StringToHash("Layer1.Idle"))         // 如果 目前 動化狀態 是吃東西
            {
                _animTime = animatorStateInfo.normalizedTime;

                if (!_bEating)        // 限制執行一次
                {
                    if ((_animTime > _lifeTime || _survivalTime > _lifeTime) && !_isBoss)                       // 動畫撥放完畢時
                    {
                        _bEating = _bAnimationDown = true;
                        //      Debug.Log("_animTime > _lifeTime || _survivalTime > _lifeTime  " + _animTime + "  " + _lifeTime + "  " + _survivalTime + "  " + _lifeTime);
                    }
                }
            }

            if (animatorStateInfo.fullPathHash == Animator.StringToHash("Layer1.OnHit"))
            {
                _animTime = animatorStateInfo.normalizedTime;
                animState = ENUM_AnimatorState.OnHit;

                if (_animTime >= .5f && _isBoss)                       // 動畫撥放完畢時
                {
                    animState = ENUM_AnimatorState.Died;
                    _bAnimationDown = true;
                    Play(animState, m_go);
                }
            }

            if (animatorStateInfo.fullPathHash == Animator.StringToHash("Layer1.Die"))              // 如果 目前 動畫狀態 是 die
            {
                _animTime = animatorStateInfo.normalizedTime;                                         // 目前播放的動畫 "總"時間
                _bAnimationUp = false;
                animState = ENUM_AnimatorState.Died;
                //  Debug.Log("Die 0:" + _bDead);
                if (!_bDead)       // 限制執行一次
                {
                    // Debug.Log("1 _animTime >= _deadTime / 3 && _animTime < _deadTime   " + _animTime + "   " + _deadTime / 3 + "   " + _animTime + "   " + _deadTime);
                    if (_animTime >= _deadTime / 3 && _animTime < _deadTime)
                    {
                        // Debug.Log("2 _animTime >= _deadTime / 3 && _animTime < _deadTime   " + _animTime + "   "+ _deadTime / 3 + "   " + _animTime + "   " + _deadTime);
                        _bAnimationDown = true;
                    }
                    else if (_animTime >= _deadTime)   // 動畫撥放完畢時
                    {
                        //     Debug.Log("3 _animTime >= _deadTime / 3 && _animTime < _deadTime   " + _animTime + "   "+ _deadTime / 3 + "   " + _animTime + "   " + _deadTime);
                        Debug.Log(ENUM_AnimatorState.MiceRunAway.ToString() + "  " + m_go.GetHashCode() + " go:" + m_go.name + " hole: " + m_go.transform.parent.name);
                        animState = ENUM_AnimatorState.MiceRunAway;
                        _bDead = true;
                        //Debug.Log("Die 3:" + _bDead);
                    }
                }
            }
        }
    }
}
