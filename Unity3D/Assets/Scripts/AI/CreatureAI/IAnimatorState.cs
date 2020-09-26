using UnityEngine;
using System.Collections;

public abstract class IAnimatorState
{
    protected GameObject obj;
    protected Animator anims;
    protected AnimatorStateInfo currentState;
    protected ENUM_AnimatorState animState = ENUM_AnimatorState.None;
    protected bool _upFlag, _bDead, _isDisappear, _bEating, _timeFlag, _bClick, _isBoss, _bMotion;
    protected float _survivalTime, _animTime, _lerpSpeed, _tmpSpeed, _upSpeed, _tmpDistance, _upDistance = -1, _lifeTime, _lastTime;
    protected float _deadTime = 0.5f, _helloTime = 1.2f;

    public enum ENUM_AnimatorState
    {
        None = -1,
        Hello = 0,
        Idle,
        Eat,
        Die,
        OnHit,
        Frozen,
        Fire,
    }

    public abstract void Init(GameObject obj, bool isBoss, float lerpSpeed, float upSpeed, float upDistance, float lifeTime);

    public IAnimatorState(GameObject obj, bool isBoss, float lerpSpeed, float upSpeed, float upDistance, float lifeTime)
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

    }

    public virtual void Initialize(GameObject obj, bool isBoss, float lerpSpeed, float upSpeed, float upDistance, float lifeTime)
    {
        _isBoss = isBoss;
        _lerpSpeed = lerpSpeed;
        _tmpSpeed = _upSpeed = upSpeed;
        _tmpDistance = _upDistance = upDistance;
        _lifeTime = lifeTime;
        this.obj = obj;

        _upFlag = true;
        _bDead = false;
        _isDisappear = false;
        _bEating = false;
    }

    public abstract void Play(ENUM_AnimatorState animState);
    public abstract void UpdateAnimation();

    public virtual void SetMotion(bool value)
    {
        _bMotion = value;
    }

    public ENUM_AnimatorState GetAnimState()
    {
        return animState;
    }

    public AnimatorStateInfo GetAnimStateInfo()
    {
        return currentState;
    }
}
