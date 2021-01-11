using UnityEngine;
using System.Collections;

public class ScrollView : MonoBehaviour
{
    public int startPos, endPos;                // 起始位置、結束位置 
    public int denominator = 10;               // 回彈邊界
    public float scrollSpeed = 0.1f;           // 捲動速度 平滑移動速度 (0~1f)
    public float  lerpSpeed = 0.1f;             // 捲動速度 平滑移動速度 (0~1f)
// public float panOffset = 0;                   // 邊界偏移量

    private Touch _touch;                            // 接收觸控
    private Vector3 _currentCamePos;   // 目前Camera座標
    private float _lastCameraX;                 // 上一次Camera的X座標
    private bool _bScroll;                            // 是否開啟捲動


    void Start()
    {
        _bScroll = true;
    }

    void Update()
    {
#if UNITY_ANDROID || UNITY_IOS

        _currentCamePos = Camera.main.transform.localPosition;

        if (Input.touchCount > 0 && _bScroll)
        {
            _touch = Input.GetTouch(0);
            // 依照Touch狀態判斷選單動作
            switch (_touch.phase)   
            {
                case TouchPhase.Began:
                    {
                        _lastCameraX = _currentCamePos.x;
                        StopAllCoroutines();
                        break;
                    }
                case TouchPhase.Moved:
                    {
                        float moveDetla = _currentCamePos.x - _touch.deltaPosition.x;
                        //如果在 限制範圍內(-+邊界偏移量) 移動選單(3DCamera)
                        if (moveDetla > endPos && moveDetla < startPos)
                            Camera.main.transform.Translate(-_touch.deltaPosition.x * scrollSpeed * Time.deltaTime, 0, 0);
                        break;
                    }
                case TouchPhase.Ended:
                    {
                        Move();
                        break;
                    }
            }
        }
#endif
    }

    // 開始移動
    private void Move()
    {
        int toPos = 0;
        // 如果移動完畢了 比上次X小 (往商店移動)
        if (_currentCamePos.x < _lastCameraX)
        {
            //如果移動範圍 沒有超出界線 回到 開始選單 
            if (_currentCamePos.x >= -Screen.width / denominator)
                toPos = startPos;
            //如果移動範圍 超出界線 移動至 商店 
            else
                toPos = endPos;
        } // 如果 比上次X大 (往選單移動)
        else
        {
            if (_currentCamePos.x >= -(Screen.width - (Screen.width / denominator)))
                toPos = startPos;
            else
                toPos = endPos;
        }
        StartCoroutine(IEMove(toPos));
    }

    IEnumerator IEMove(int toPos) // 緩慢的移動至開始位置
    {
        for (int i = 0; i < Screen.width; i++)
        {
            //商店 到 選單時 如果還沒到達位置 就一直往位置移動
            if (Mathf.RoundToInt(_currentCamePos.x) != toPos)
            {
                Camera.main.transform.localPosition = Vector3.Lerp(_currentCamePos, new Vector3(toPos, 0), lerpSpeed);
                yield return new WaitForSeconds(0.01f);
            }
            // 到達時
            else if (Vector2.Distance(new Vector2(_currentCamePos.x, 0), new Vector2(toPos, 0)) <= 1)
            {
                Camera.main.transform.localPosition = new Vector3(toPos, 0);
                break;
            }
        }
    }
}