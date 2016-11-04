using UnityEngine;
using System.Collections;

public class Mice : MonoBehaviour {

    public BattleManager battleManager;

    private float aliveTime;
    private float animTime;

    private bool upFlag;

    private bool dieFlag;
    private bool isDisappear;
    private bool eatingFlag;
    private bool clickFlag;

    [Range(0.01f, 0.99f)]
    private static float lerpSpeed;
    private static float upSpeed;
    private static float upDistance;
    private float _lastTime;
    private bool _timeFlag;
    private bool _isBoss;

    public void Initialize(float _lerpSpeed, float upSpeed, float upDistance)
    {
        battleManager = GameObject.Find("BattleManager").GetComponent<BattleManager>();
        upFlag = true;
        dieFlag = false;
        isDisappear = false;
        eatingFlag = false;
        clickFlag = false;
        _timeFlag = true;
        _isBoss = false;

        Mice.lerpSpeed = _lerpSpeed;
        Mice.upDistance = upDistance;
        Mice.upSpeed = upSpeed;

        _lastTime = 0;
        collider2D.enabled = true;
    }

    public void getValue(){
        Debug.Log(lerpSpeed + " " + upDistance);
    }

    public void Play()
    {
        battleManager = GameObject.Find("BattleManager").GetComponent<BattleManager>();
        upFlag = true;
        dieFlag = false;
        isDisappear = false;
        eatingFlag = false;
        clickFlag = false;
        collider2D.enabled = true;
        _timeFlag = true;
        _isBoss = false;
        GetComponent<Animator>().Play("Hello");
        transform.parent.localPosition = new Vector3(0, 0);
    }


    public void Update()
    {
        if (Global.isGameStart)
        {
            #region Amination

            if (upFlag && transform.parent.localPosition.y < upDistance)        // AnimationUp
                StartCoroutine(AnimationUp());

            if (isDisappear && transform.parent.localPosition.y > -upDistance) // AnimationDown
                StartCoroutine(AnimationDown());

            if (transform.gameObject.activeSelf == true && _timeFlag)  // 如果被Spawn儲存現在時間 注意 DisActive時Time還是會一直跑 所以要存起來減掉
            {
                _timeFlag = false;
                _lastTime = Time.fixedTime;
            }

            aliveTime = Time.fixedTime - _lastTime;                                                              // 老鼠存活時間 
            Animator anims = GetComponent("Animator") as Animator;   // 播放 死亡動畫                  
            AnimatorStateInfo currentState = anims.GetCurrentAnimatorStateInfo(0);      // 取得目前動畫狀態 (0) = Layer
            //Debug.Log("currentState : " + currentState.nameHash);
            if (currentState.nameHash == Animator.StringToHash("Layer1.Hello"))                    // 如果 目前 動化狀態 是 up
            {
                animTime = currentState.normalizedTime;
                //            Debug.Log(animTime);
                // 目前播放的動畫 "總"時間
                if (animTime > 1)   // 動畫撥放完畢時
                {

                    anims.Play("Idle");   // 老鼠開始吃東西
                    upFlag = true;
                }
            }
            else if (currentState.nameHash == Animator.StringToHash("Layer1.Die"))              // 如果 目前 動畫狀態 是 die
            {
                animTime = currentState.normalizedTime;                                         // 目前播放的動畫 "總"時間
                if (!dieFlag)       // 限制執行一次
                {
                    if (animTime > 0.5)   // 動畫撥放完畢時
                    {
                        OnDied(aliveTime);
                        dieFlag = true;
                    }
                }
            }
            else if (currentState.nameHash == Animator.StringToHash("Layer1.Idle"))
            {
                animTime = currentState.normalizedTime;

                if (!eatingFlag)        // 限制執行一次
                {
                    if (animTime > 2.64f && !_isBoss)                       // 動畫撥放完畢時
                    {
                        isDisappear = true;
                        eatingFlag = true;
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
                if (!clickFlag)  //＊＊＊＊＊＊＊超快還是會combo ＊＊＊＊＊　有時間在改
                {
                    clickFlag = true;
                    collider2D.enabled = false;
                    GetComponent<Animator>().Play("Die");
                }
            }
            else
            {
                if (GetComponent<BossPorperty>().hp != 0)
                    Global.photonService.BossDamage(1);
            }
        }
    }

    private void OnDisappear(float aliveTime)
    {
        if (Global.isGameStart)
        {
            this.transform.parent.parent = GameObject.Find("ObjectPool/" + transform.parent.name).transform;
            gameObject.SetActive(false);
            //Debug.Log("OnDisappear : " + aliveTime);

                _lastTime = aliveTime;
                battleManager.LostScore(transform.parent.name, aliveTime);  // 跑掉掉分

            Global.MiceCount--;
        }
    }

    private void OnDied(float aliveTime)
    {
        if (Global.isGameStart)
        {
            if (_isBoss) Destroy(GetComponent<BossPorperty>());
            this.transform.parent.parent = GameObject.Find("ObjectPool/" + transform.parent.name).transform;
            gameObject.SetActive(false);


                _lastTime = aliveTime;
                if (!_isBoss)
                    battleManager.UpadateScore(transform.parent.name, aliveTime);  // 增加分數
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
        float speed = upSpeed;
        if (_isBoss)
            upDistance = GetComponent<BoxCollider2D>().size.x * 0.4f;    // ＊＊＊＊會影響原本老鼠
        collider2D.enabled = true;
        speed = Mathf.Lerp(speed, 1, lerpSpeed);

        if (transform.parent.localPosition.y + speed > upDistance)
        {
            transform.parent.localPosition = new Vector3(0, upDistance, 0);
            upFlag = false;
        }
        else
        {
            transform.parent.localPosition += new Vector3(0, speed, 0);
        }
        yield return null;
    }

    private IEnumerator AnimationDown() // 2   = 2 ~ 1
    {
        if (transform.parent.localPosition.y - 10 <= -upDistance)
        {
            Vector3 _tmp;
            _tmp = new Vector3(0, -upDistance * 2, 0);
            transform.parent.localPosition = _tmp;
            OnDisappear(aliveTime);
            isDisappear = false;
        }
        else
        {
            transform.parent.localPosition = Vector3.Slerp(transform.parent.localPosition, new Vector3(0, -upDistance * 2, 0), Time.deltaTime * 5);
        }
        yield return null;
    }
}
