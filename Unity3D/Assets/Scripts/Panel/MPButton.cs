using UnityEngine;
using System.Collections;

public class MPButton : MonoBehaviour {
    [Range(0, 1)]
    [Tooltip("回彈速度")]
    public float lerpSpeed = 0.25f;
    [Range(0, 1)]
    [Tooltip("漸變速度")]
    public float tweenColorSpeed = 0.1f;
    [Range(0, 1)]
    [Tooltip("滑過時顏色")]
    public float hoverColor = 0.95f;
    [Range(0, 1)]
    [Tooltip("按下時顏色")]
    public float clickColor = 0.85f;
    [Range(0, 1)]
    [Tooltip("失效時時顏色")]
    public float disableColor = 0.5f;
    [Tooltip("隊伍離開距離")]                                 // 可以增加老鼠離開時自動加到最後
    public float leftDist = 100f;
    public bool _isTrigged;                                   // 按鈕啟動狀態

    #region -- EnDisableBtn 啟動/關閉按鈕(內部使用) --
    /// <summary>
    /// 改變物件功能 開/關
    /// </summary>
    /// <param name="go">生效物件</param>
    /// <param name="enable">功能開關</param>
    public void EnDisableBtn(GameObject go, bool enable)
    {
        if (enable)
        {
            go.GetComponent<ButtonSwitcher>().enabled = enable;
            go.GetComponent<ButtonSwitcher>()._activeBtn = enable;
            TweenColor.Begin(go, tweenColorSpeed, Color.white);
        }
        else
        {
            go.GetComponent<ButtonSwitcher>()._activeBtn = enable;
            go.GetComponent<ButtonSwitcher>().enabled = enable;
            TweenColor.Begin(go, tweenColorSpeed, new Color(disableColor, disableColor, disableColor));
        }
        go.GetComponent<BoxCollider>().isTrigger = false;
        go.GetComponent<UIDragObject>().enabled = enable;
    }
    #endregion

    #region -- DisableBtn 關閉按鈕(外部呼叫) --
    public void DisableBtn()
    {
        TweenColor.Begin(this.gameObject, tweenColorSpeed, new Color(disableColor, disableColor, disableColor));
        GetComponent<ButtonSwitcher>()._activeBtn = false;
        GetComponent<ButtonSwitcher>().enabled = false;
        GetComponent<UIDragObject>().enabled = false;
        GetComponent<BoxCollider>().isTrigger = false;
        _isTrigged = false;
    }
    #endregion

    #region -- EnableBtn 關閉按鈕(外部呼叫) --
    public void EnableBtn()
    {
        TweenColor.Begin(this.gameObject, tweenColorSpeed, Color.white);
        GetComponent<ButtonSwitcher>().enabled = true;
        GetComponent<ButtonSwitcher>()._activeBtn = true;
        GetComponent<UIDragObject>().enabled = true;
        GetComponent<BoxCollider>().isTrigger = false;
        _isTrigged = false;
    }
    #endregion
}
