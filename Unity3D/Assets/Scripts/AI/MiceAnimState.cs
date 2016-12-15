using UnityEngine;
using System.Collections;

public class MiceAnimState : AnimatorState
{

   // private BattleManager battleManager = null;

    void Update()
    {
        if (_upFlag && obj.transform.localPosition.y < _upDistance)        // AnimationUp
            StartCoroutine(AnimationUp());

        if (_isDisappear && obj.transform.localPosition.y > -_upDistance) // AnimationDown
            StartCoroutine(AnimationDown());



        Animator anims = GetComponentInChildren<Animator>();   // 播放 死亡動畫
        if (anims != null)
        {
            AnimatorStateInfo currentState = anims.GetCurrentAnimatorStateInfo(0);      // 取得目前動畫狀態 (0) = Layer
            //Debug.Log("currentState : " + currentState.nameHash);
            if (currentState.nameHash == Animator.StringToHash("Layer1.Hello"))         // 如果 目前 動化狀態 是 up
            {
                _animTime = currentState.normalizedTime;
                animState = ENUM_AnimatorState.Hello;
                //            Debug.Log(animTime);
                // 目前播放的動畫 "總"時間
                if (_animTime >= _helloTime)   // 動畫撥放完畢時
                {
                    anims.Play("Idle");   // 老鼠開始吃東西
                }
            }
            else if (currentState.nameHash == Animator.StringToHash("Layer1.Die"))              // 如果 目前 動畫狀態 是 die
            {
                _animTime = currentState.normalizedTime;                                         // 目前播放的動畫 "總"時間

                if (!_bDead)       // 限制執行一次
                {
                    animState = ENUM_AnimatorState.Die;
                    if (_animTime >= _deadTime / 3)
                    {
                        animState = ENUM_AnimatorState.None;
                        _isDisappear = true;
                    }
                    if (_animTime >= _deadTime)   // 動畫撥放完畢時
                    {
                        _bDead = true;
                    }
                }
            }
            else if (currentState.nameHash == Animator.StringToHash("Layer1.Idle"))
            {
                _animTime = currentState.normalizedTime;

                if (!_bEating)        // 限制執行一次
                {
                    animState = ENUM_AnimatorState.Idle;
                    //Skill.OnSkill(gameObject, Arribute);
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
                    GetComponentInChildren<Animator>().Play("Idle");
                }
            }
        }
    }

    public override void Initialize(GameObject obj, bool isBoss, float lerpSpeed, float upSpeed, float upDistance, float lifeTime)
    {
        //battleManager = GameObject.FindGameObjectWithTag("GM").GetComponent<BattleManager>();

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


    public override void Play(ENUM_AnimatorState animState)
    {
        Animator animator = obj.GetComponentInChildren<Animator>();
        this.animState = animState;

        switch (animState)
        {
            case ENUM_AnimatorState.Hello:
                _upFlag = true;
                _bDead = false;
                _isDisappear = false;
                _bEating = false;
                _tmpSpeed = _upSpeed;
                _upDistance = _tmpDistance;
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
                break;
        }
    }

    private IEnumerator AnimationUp()
    {
        obj.GetComponent<BoxCollider2D>().enabled = true;
        _upDistance = _isBoss ? GetComponent<BoxCollider2D>().size.x * 0.4f : _upDistance;
        _tmpSpeed = Mathf.Lerp(_tmpSpeed, 1, _lerpSpeed);

        if (transform.localPosition.y + _tmpSpeed >= _upDistance)
        {
            transform.localPosition = new Vector3(0, _upDistance, 0);
            _upFlag = false;
        }
        else
        {
            transform.localPosition += new Vector3(0, _tmpSpeed, 0);
        }
        yield return null;
    }

    private IEnumerator AnimationDown() // 2   = 2 ~ 1
    {
        _tmpSpeed = Mathf.Lerp(_tmpSpeed, 20, _lerpSpeed);

        if (transform.localPosition.y - 20 <= -_upDistance)
        {
            Vector3 _tmp;
            _tmp = new Vector3(0, -_upDistance, 0);
            transform.localPosition = _tmp;
            _upDistance = _tmpDistance;
            obj.SendMessage("OnDead", _lifeTime);
            _isDisappear = false;
        }
        else
        {
            transform.localPosition -= new Vector3(0, _tmpSpeed, 0);
            //transform.localPosition = Vector3.Slerp(transform.localPosition, new Vector3(0, -_upDistance * 2, 0), Time.deltaTime * 5);
        }
        yield return null;
    }

}
