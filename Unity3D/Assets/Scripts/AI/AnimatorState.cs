using UnityEngine;
using System.Collections;

public abstract class AnimatorState : MonoBehaviour
{

    public enum ENUM_AnimatorState
    {
        None =-1,
        Hello = 0,
        Idle = 1,
        Die = 2,
        OnHit = 3,
    }

    protected GameObject obj;
    protected ENUM_AnimatorState animState = ENUM_AnimatorState.None;
    protected bool _upFlag, _bDead, _isDisappear, _bEating, _timeFlag, _bClick, _isBoss;
    protected float _survivalTime, _animTime, _lerpSpeed, _tmpSpeed, _upSpeed, _tmpDistance, _upDistance=-1, _lifeTime, _lastTime;
    protected float _deadTime = 0.5f, _helloTime = 1.2f;

    public abstract void Initialize(GameObject obj,bool isBoss, float lerpSpeed, float upSpeed, float upDistance, float lifeTime);
    public abstract void Play(ENUM_AnimatorState animState);
}
