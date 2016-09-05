using UnityEngine;
using System.Collections;

public static class EventMaskSwitch  {

    public static GameObject openedPanel;

    #region -- EventMaskSwtich --
    // 位元移位運算子<< >> 2近位 左移、右移  0 << 1(左移1)  --->  00 = 0
    // 1 << 1(左移1)  -->  001 = 1
    // 1 >> 1(右移1)  -->  010 = 2
    // 1 >> 2(右移2)  -->  0100 = 4
    // 1 << 8(左移8)  -->  000000001
    /// <summary>
    /// 改變UICamera事件觸發遮罩
    /// </summary>
    /// <param name="obj">指定物件圖層</param>
    public static void Switch(GameObject obj)
    {
        foreach (Camera c in Camera.allCameras)
            c.GetComponent<UICamera>().eventReceiverMask = 1 << obj.layer;
    }
    #endregion
}
