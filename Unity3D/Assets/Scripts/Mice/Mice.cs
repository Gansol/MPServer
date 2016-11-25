using UnityEngine;
using System.Collections;

public class Mice : MonoBehaviour
{
    public BattleManager battleManager;
    private bool _upFlag, _bDead, _isDisappear, _bEating, _timeFlag, _bClick, _isBoss;
    private float _survivalTime, _animTime, _lerpSpeed, _tmpSpeed, _upSpeed, _tmpDistance, _upDistance, _lifeTime, _lastTime;
    private float _deadTime = 0.5f, _helloTime = 1.2f;

    public void Initialize(float lerpSpeed, float upSpeed, float upDistance, float lifeTime)
    {
        battleManager = GameObject.FindGameObjectWithTag("GM").GetComponent<BattleManager>();
        _upFlag = true;
        _bDead = false;
        _isDisappear = false;
        _bEating = false;
        _bClick = false;
        _timeFlag = true;
        _isBoss = false;

        this._lerpSpeed = lerpSpeed;
        this._upDistance = _tmpDistance = upDistance;
        this._upSpeed = _tmpSpeed = upSpeed;
        this._lifeTime = lifeTime;

        _lastTime = 0;
        collider2D.enabled = true;
    }

    public void getValue()
    {
        Debug.Log(_lerpSpeed + " " + _upDistance);
    }

    public void Play()
    {
        battleManager = GameObject.FindGameObjectWithTag("GM").GetComponent<BattleManager>();
        _upFlag = true;
        _bDead = false;
        _isDisappear = false;
        _bEating = false;
        _bClick = false;
        collider2D.enabled = true;
        _timeFlag = true;
        _isBoss = false;
        _tmpSpeed = _upSpeed;
        _upDistance = _tmpDistance;
        GetComponentInChildren<Animator>().Play("Hello");
        transform.localPosition = new Vector3(0, 0);
    }


    public void Update()
    {
        if (Global.isGameStart)
        {
            #region Amination

            if (_upFlag && transform.localPosition.y < _upDistance)        // AnimationUp
                StartCoroutine(AnimationUp());

            if (_isDisappear && transform.localPosition.y > -_upDistance) // AnimationDown
                StartCoroutine(AnimationDown());

            if (transform.gameObject.activeSelf == true && _timeFlag)  // 如果被Spawn儲存現在時間 注意 DisActive時Time還是會一直跑 所以要存起來減掉
            {
                _timeFlag = false;
                _lastTime = Time.fixedTime;
            }


            Animator anims = GetComponentInChildren<Animator>();   // 播放 死亡動畫
            if (anims != null)
            {
                AnimatorStateInfo currentState = anims.GetCurrentAnimatorStateInfo(0);      // 取得目前動畫狀態 (0) = Layer
                //Debug.Log("currentState : " + currentState.nameHash);
                if (currentState.nameHash == Animator.StringToHash("Layer1.Hello"))         // 如果 目前 動化狀態 是 up
                {
                    _animTime = currentState.normalizedTime;
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
                        if (_animTime >= _deadTime / 3)
                        {
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
                        if (_animTime > _lifeTime && !_isBoss)                       // 動畫撥放完畢時
                        {
                            _survivalTime = Time.fixedTime - _lastTime;                // 老鼠存活時間 
                            _isDisappear = true;
                            _bEating = true;
                        }
                    }
                }
                else if (currentState.nameHash == Animator.StringToHash("Layer1.OnHit"))
                {
                    _animTime = currentState.normalizedTime;

                    if (_animTime >= .5f && _isBoss)                       // 動畫撥放完畢時
                    {
                        GetComponentInChildren<Animator>().Play("Idle");
                    }
                }
            }
            #endregion
        }
        else
        {
            gameObject.SetActive(false);
        }

    }

    private void OnHit()
    {
        if (Global.isGameStart)
        {
            if (!_isBoss)
            {
                if (!_bClick)  //＊＊＊＊＊＊＊超快還是會combo ＊＊＊＊＊　有時間在改
                {
                    _bClick = true;
                    collider2D.enabled = false;
                    _survivalTime = Time.fixedTime - _lastTime;                // 老鼠存活時間 
                    GetComponentInChildren<Animator>().Play("Die");
                }
            }
            else
            {
                if (GetComponent<BossPorperty>().hp != 0)
                    Global.photonService.BossDamage(1);
                GetComponentInChildren<Animator>().Play("OnHit");
            }
        }
    }

    private void OnDead(float lifeTime)
    {
        if (Global.isGameStart)
        {
            if (_isBoss) Destroy(GetComponent<BossPorperty>());
            this.transform.parent = GameObject.Find("ObjectPool/" + name).transform;
            gameObject.SetActive(false);


            _lastTime = lifeTime;
            if (!_isBoss && _bClick)
                battleManager.UpadateScore(name, _survivalTime);  // 增加分數
            else if (!_isBoss && !_bClick)
                battleManager.LostScore(name, lifeTime);  // 增加分數
            Global.MiceCount--;
        }
    }

    public void AsBoss(bool isBoss)
    {
        Play();
        _isBoss = true;
    }

    private IEnumerator AnimationUp()
    {
        collider2D.enabled = true;
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

        if (transform.localPosition.y - 20  <= -_upDistance)
        {
            Vector3 _tmp;
            _tmp = new Vector3(0, -_upDistance, 0);
            transform.localPosition = _tmp;
            _upDistance = _tmpDistance;
            OnDead(_survivalTime);
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
