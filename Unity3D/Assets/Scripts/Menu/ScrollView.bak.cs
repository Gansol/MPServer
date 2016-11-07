using UnityEngine;
using System.Collections;

/*
 * 過度往邊邊移動還有小BUG在修改
 * 
 *
 * 
 */

public class ScrollView2 : MonoBehaviour
{
    private float lastCameraX;      // 上一次Camera的X座標
    private float currentCameraX;   // 目前Camera的X座標
    private bool startFlag;         // 移動到啟始位置的Flag
    private bool endFlag;           // 移動到結束位置的Flag

    public int startPos;            // 起始位置
    public int endPos;              // 結束位置
    public float scrollSpeed=0.1f;  // 捲動速度
    public float lerpSpeed=0.1f;    // 平滑移動速度 (0~1f)
    public float panOffset=0;       // 邊界偏移量
    //public GameObject label;        // 測試用Label(NGUI)

    void Start()
    {
        ///currentCameraX = 0;  等待刪除
        //lastCameraX = 0;      等待刪除
        startFlag = false;
        endFlag = false;
    }

    void Update()
    {
        Touch touch = Input.GetTouch(0);
        currentCameraX = Camera.main.transform.localPosition.x;

        switch (touch.phase)
        {
            case TouchPhase.Began:
                {
                    StopAllCoroutines();
                    break;
                }
            case TouchPhase.Moved:
                {
                    //label.GetComponent<UILabel>().text = "(M)W/5:" + Screen.width / 5 + " CamX:" + Mathf.Abs(currentCameraX);
                    //限制範圍-+邊界偏移量預設0
                    if ((Camera.main.transform.localPosition.x - touch.deltaPosition.x) > (endPos - panOffset) && (Camera.main.transform.localPosition.x - touch.deltaPosition.x) < (startPos + panOffset))
                    {
                        float touchDeltaPositionX = touch.deltaPosition.x;
                        Camera.main.transform.Translate(-touchDeltaPositionX * scrollSpeed * 10 * Time.deltaTime, 0, 0);
                    }
                    break;
                }
            case TouchPhase.Ended:
                {
                    // 如果 比上次X小 (往商店移動)
                    if (currentCameraX < lastCameraX)
                    {
                        //如果移動範圍 沒有超出界線 回到 開始選單  -215 >= -216
                        if (currentCameraX >= -Screen.width / 5)
                        {
                            startFlag = true;

                            //label.GetComponent<UILabel>().text = "(S1)CamX:" + Mathf.Abs(currentCameraX);
                        }//如果移動範圍 超出界線 移動至 商店 -217 < -216
                        else if (currentCameraX < -Screen.width / 5)
                        {
                            //label.GetComponent<UILabel>().text = "(S2)CamX:" + Mathf.Abs(currentCameraX);
                            endFlag = true;
                        }
                    } // 如果 比上次X大 (往選單移動)
                    else if (currentCameraX > lastCameraX)
                    {// -1080 >= -864 
                        if (currentCameraX >= -(Screen.width - (Screen.width / 5)))
                        {
                            //label.GetComponent<UILabel>().text = "(E1)CamX:" + Mathf.Abs(currentCameraX);
                            startFlag = true;
                        }//-1070 < -864
                        else if (currentCameraX < -(Screen.width - (Screen.width / 5)))
                        {
                            //label.GetComponent<UILabel>().text = "(E2)CamX:" + Mathf.Abs(currentCameraX);
                            endFlag = true;
                        }
                    }
                    lastCameraX = currentCameraX;
                    break;
                }
        }

        if (startFlag)
        {
            startFlag = false;
            StartCoroutine(GoStart());
        }

        if (endFlag)
        {
            endFlag = false;
            StartCoroutine(GoEnd());
        }
    }

    IEnumerator GoStart()
    {
        for (int i = 0; i < Screen.width; i++)
        {
            if (Mathf.RoundToInt(Camera.main.transform.localPosition.x) < startPos)
            {
                Camera.main.transform.localPosition = Vector3.Lerp(Camera.main.transform.localPosition, new Vector3(startPos, 0), lerpSpeed);
                //label.GetComponent<UILabel>().text += "\n(GoStart2)CamX:" + Camera.main.transform.localPosition.x;
                yield return new WaitForSeconds(0.01f);
            }
            else if (Mathf.RoundToInt(Camera.main.transform.localPosition.x) > startPos)
            {
                Camera.main.transform.localPosition = Vector3.Lerp(Camera.main.transform.localPosition, new Vector3(startPos, 0), lerpSpeed);
                //label.GetComponent<UILabel>().text += "\n(GoStart2)CamX:" + Camera.main.transform.localPosition.x;
                yield return new WaitForSeconds(0.01f);
            }
            else if (Mathf.RoundToInt(Camera.main.transform.localPosition.x) == startPos)
            {
                Camera.main.transform.localPosition = new Vector3(startPos, 0);
                startFlag = false;
                //label.GetComponent<UILabel>().text += "start Break";
                break;
            }
        }
    }

    IEnumerator GoEnd()
    {
        for (int i = 0; i < Screen.width; i++)
        {
            Vector3 _cameraPos = Camera.main.transform.localPosition;
            if (Mathf.RoundToInt(Camera.main.transform.localPosition.x) > endPos)
            {
                Camera.main.transform.localPosition = Vector3.Lerp(_cameraPos, new Vector3(endPos, 0), lerpSpeed);
                //label.GetComponent<UILabel>().text += "\n(GoEnd2)CamX:" + Camera.main.transform.localPosition.x;
                yield return new WaitForSeconds(0.01f);
            }
            else if (Mathf.RoundToInt(Camera.main.transform.localPosition.x) < endPos)
            {
                Camera.main.transform.localPosition = Vector3.Lerp(_cameraPos, new Vector3(endPos, 0), lerpSpeed);
                //label.GetComponent<UILabel>().text += "\n(GoEnd2)CamX:" + Camera.main.transform.localPosition.x;
                yield return new WaitForSeconds(0.01f);
            }
            else if (Mathf.RoundToInt(_cameraPos.x) == endPos)
            {
                Camera.main.transform.localPosition = new Vector3(endPos, 0);
                endFlag = false;
                //label.GetComponent<UILabel>().text += "end Break";
                break;
            }

        }

    }
}