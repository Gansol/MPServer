using UnityEngine;
using System.Collections;

public abstract class IAnimatorState
{
    protected GameObject m_go;
    protected Animator m_Animator;
    protected AnimatorStateInfo animatorStateInfo;
    protected ENUM_AnimatorState animState = ENUM_AnimatorState.None;
    protected bool _bAnimationUp, _bDead, _bAnimationDown, _bEating, _timeFlag, _bClick, _isBoss, _bMotion;
    protected float _survivalTime, _animTime, _lerpSpeed, _tmpSpeed, _upAnimSpeed, _defaultUpDistance, _upDistance = -1, _lifeTime, _lastTime;
    protected float _deadTime = 0.5f, _helloTime = 1.2f;
    protected bool _toFlag, _toScale;
    protected Vector3 _toWorldPos, _scale;

    public enum ENUM_AnimatorState
    {
        None = -1,
        Hello = 0,
        Idle,
        Eat,
        Byebye,
        Died,
        OnHit,
        Frozen,
        Fire,
        MiceRunAway,
    }

    public IAnimatorState(GameObject go, bool isBoss, float lerpSpeed, float upSpeed, float upDistance, float lifeTime)
    {
        _isBoss = isBoss;
        _lerpSpeed = lerpSpeed;
        _defaultUpDistance = _upDistance = upDistance;
        _lifeTime = lifeTime;
        _upAnimSpeed = upSpeed;
        m_go = go;
        m_Animator = go.GetComponentInChildren<Animator>();
        animatorStateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);

        Initialize();
    }

    public virtual void Initialize()
    {
        animState = ENUM_AnimatorState.None;

        _bAnimationUp = true;
        _bMotion = true;
        _bDead = false;
        _bAnimationDown = false;
        _bEating = false;
        _toFlag = _toScale = false;

        _lastTime = Time.time;
    }




    public virtual void UpdateAnimation()
    {
        _survivalTime = Time.time - _lastTime;
      //  Debug.Log("UpdateAnimation:" + m_go.transform.localPosition.y + "    " + _upDistance);
        if (_bMotion && !_toFlag && (_bAnimationUp && m_go.transform.localPosition.y < _upDistance))        // AnimationUp
            AnimationUp();

        if (_bMotion && !_toFlag && (_bAnimationDown && m_go.transform.localPosition.y > -_upDistance)) // AnimationDown
            AnimationDown();

        if (_bMotion && _toFlag && (Vector3.Distance(m_go.transform.position, _toWorldPos) > 0)) // AnimationTo
            AnimationTo();

        if (_bMotion && _toScale && (Vector3.Distance(m_go.transform.position, _toWorldPos) > 0)) // AnimationScale
            AnimationScale();
    }











    public virtual void Play(ENUM_AnimatorState animState)
    {
        this.animState = animState;
        m_Animator = m_go.GetComponentInChildren<Animator>();

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

    /// <summary>
    /// 往上動畫
    /// </summary>
    protected virtual void AnimationUp()
    {
        Debug.Log("AnimationUp");
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

    /// <summary>
    /// 往下動畫
    /// </summary>
    protected virtual void AnimationDown() // 2   = 2 ~ 1
    {
        _tmpSpeed = Mathf.Lerp(_tmpSpeed, 20, _lerpSpeed);

        // 如果 達到 逃跑距離 移動到洞口下位置，改變為逃跑狀態
        if (m_go.transform.localPosition.y - 20 <= -_upDistance)
        {
            m_go.transform.localPosition = new Vector3(0, -_upDistance, 0); ;
            _upDistance = _defaultUpDistance;
            animState = ENUM_AnimatorState.MiceRunAway;
            
            //anims.StopPlayback();
            _bAnimationDown = false;
        }
        else
        {
            // 持續向下移動
            m_go.transform.localPosition -= new Vector3(0, _tmpSpeed, 0);
            //transform.localPosition = Vector3.Slerp(transform.localPosition, new Vector3(0, -_upDistance * 2, 0), Time.deltaTime * 5);
        }
    }

    /// <summary>
    /// 移動動畫
    /// </summary>
    protected virtual void AnimationTo()
    {
        // _tmpSpeed = Mathf.Lerp(_tmpSpeed, 1, _lerpSpeed);
        float distance = Vector3.Distance(m_go.transform.position, _toWorldPos);
        if (distance >= 0 && distance <= 0.05f)
        {
            m_go.transform.transform.position = _toWorldPos;
            _toFlag = false;
        }
        else
        {
            m_go.transform.position = Vector3.Lerp(m_go.transform.transform.position, _toWorldPos, _lerpSpeed);
        }
    }

    /// <summary>
    /// 放大動畫
    /// </summary>
    protected virtual void AnimationScale()
    {
        if (m_go.transform.localScale == _scale)
            _toScale = false;
        m_go.transform.localScale = Vector3.Lerp(m_go.transform.localScale, _scale, 0.1f);

    }






    /// <summary>
    /// 設定 移動位置
    /// </summary>
    /// <param name="toWorldPos"></param>
    public void SetAminationPosTo(Vector3 toWorldPos)
    {
        _toWorldPos = toWorldPos;
        _toFlag = true;
    }

    /// <summary>
    /// 設定 放大大小
    /// </summary>
    /// <param name="scale"></param>
    public void SetAminationScaleTo(Vector3 scale)
    {
        _scale = scale;
        _toScale = true;
    }

    /// <summary>
    /// 設定 是否執行動畫
    /// </summary>
    /// <param name="value"></param>
    public void SetMotion(bool value)
    {
        _bMotion = value;
    }

    /// <summary>
    /// 取得目前動畫狀態(自訂狀態)
    /// </summary>
    /// <returns></returns>
    public ENUM_AnimatorState GetENUM_AnimState()
    {
        return animState;
    }

    /// <summary>
    /// 取得目前動畫狀態值
    /// </summary>
    /// <returns></returns>
    public AnimatorStateInfo GetAnimStateInfo()
    {
        return animatorStateInfo;
    }

    /// <summary>
    /// 取得存活時間(應該要改到MICE)
    /// </summary>
    /// <returns></returns>
    public float GetSurvivalTime()
    {
        return _survivalTime;
    }
}
