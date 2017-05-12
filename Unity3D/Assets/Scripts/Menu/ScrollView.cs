using UnityEngine;
using System.Collections;

public class ScrollView : MonoBehaviour
{
    public bool scroll;
    public int startPos, endPos, denominator = 10;            // 起始位置 結束位置 回彈邊界
    public float scrollSpeed = 0.1f, lerpSpeed = 0.1f;  // 捲動速度 平滑移動速度 (0~1f)
//    public float panOffset = 0;       // 邊界偏移量
    
    private float lastCameraX;      // 上一次Camera的X座標
    private Vector3 currentCamePos;   // 目前Camera座標
    private Touch touch;

    void Awake()
    {
        scroll = true;
    }

    void Update()
    {
#if UNITY_ANDROID || UNITY_IOS

        currentCamePos = Camera.main.transform.localPosition;

        if (Input.touchCount > 0 && scroll)
        {
            touch = Input.GetTouch(0);
            switch (touch.phase)    // 依照Touch狀態判斷選單動作
            {
                case TouchPhase.Began:
                    {
                        lastCameraX = currentCamePos.x;
                        StopAllCoroutines();
                        break;
                    }
                case TouchPhase.Moved:
                    {
                        float moveDetla = currentCamePos.x - touch.deltaPosition.x;
                        //如果在 限制範圍內(-+邊界偏移量) 移動選單(3DCamera)
                        if (moveDetla > endPos && moveDetla < startPos)
                            Camera.main.transform.Translate(-touch.deltaPosition.x * scrollSpeed * Time.deltaTime, 0, 0);
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
        if (currentCamePos.x < lastCameraX)
        {
            //如果移動範圍 沒有超出界線 回到 開始選單 
            if (currentCamePos.x >= -Screen.width / denominator)
                toPos = startPos;
            //如果移動範圍 超出界線 移動至 商店 
            else
                toPos = endPos;
        } // 如果 比上次X大 (往選單移動)
        else
        {
            if (currentCamePos.x >= -(Screen.width - (Screen.width / denominator)))
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
            if (Mathf.RoundToInt(currentCamePos.x) != toPos)
            {
                Camera.main.transform.localPosition = Vector3.Lerp(currentCamePos, new Vector3(toPos, 0), lerpSpeed);
                yield return new WaitForSeconds(0.01f);
            }
            // 到達時
            else if (Vector2.Distance(new Vector2(currentCamePos.x, 0), new Vector2(toPos, 0)) <= 1)
            {
                Camera.main.transform.localPosition = new Vector3(toPos, 0);
                break;
            }
        }
    }
}