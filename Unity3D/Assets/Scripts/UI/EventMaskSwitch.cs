using UnityEngine;
using System.Collections.Generic;
//
/* ***************************************************************
 * -----Copyright c 2018 Gansol Studio.  All Rights Reserved.-----
 * -----------            CC BY-NC-SA 4.0            -------------
 * -----------  @Website:  EasyUnity@blogspot.com    -------------
 * -----------  @Email:    GansolTW@gmail.com        -------------
 * -----------  @Author:   Krola.                    -------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * 
 *                      負責 所有相機事件遮罩
 *                      
 * ***************************************************************
 *                           ChangeLog           
 * 20171225 v1.1.2   修正階層問題                                                                  
 * ****************************************************************/
//
public static class EventMaskSwitch
{
    public static GameObject lastPanel;
    public static GameObject openedPanel { get { return _openedPanel; } }
    private static GameObject _openedPanel;
    private static Dictionary<string, List<LayerMask>> dictSceneDefaultEventMask = new Dictionary<string, List<LayerMask>>();   // 儲存初始事件遮罩
    private static List<LayerMask> defalutLayerMask = new List<LayerMask>();
    private static List<LayerMask> prevLayerMask = new List<LayerMask>();

    /// <summary>
    /// 注意! 只能在第一次載入時使用
    /// </summary>
    public static void Init()
    {
        // 儲存初始事件遮罩 減少尋找時間
        if (!dictSceneDefaultEventMask.ContainsKey(Application.loadedLevelName))
        {
            defalutLayerMask = new List<LayerMask>();
            foreach (Camera c in Camera.allCameras)
            {
                defalutLayerMask.Add(c.GetComponent<UICamera>().eventReceiverMask);
            }
            dictSceneDefaultEventMask.Add(Application.loadedLevelName, defalutLayerMask);
        }
    }

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
    /// /// <param name="nextPanel">是否開啟下一個階層</param>
    public static void Switch(GameObject obj)
    {
        foreach (Camera c in Camera.allCameras)
        {
            prevLayerMask.Add(c.GetComponent<UICamera>().eventReceiverMask);
            c.GetComponent<UICamera>().eventReceiverMask = 1 << obj.layer;
        }
        _openedPanel = obj;
    }
    #endregion

    /// <summary>
    /// 回復N動事件遮罩
    /// </summary>
    /// mask = prevLayerMask % allCamerasCount = 要返回的遮罩位子 123 [1]23 << mask  123 1[2]3 << mask  123 12[3] << mask 
    /// i-mask = 倒數第一組遮罩在陣列的位子     123 123 [123]<<這個
    public static void Prev(int level)
    {
        while (level > 0 && prevLayerMask.Count != 0)
        {
            foreach (Camera c in Camera.allCameras)
            {
                int i = prevLayerMask.Count - 1;    // 數量-1=陣列長度
                int mask = i % Camera.allCamerasCount;
                c.GetComponent<UICamera>().eventReceiverMask = prevLayerMask[i - mask];
                prevLayerMask.Remove(prevLayerMask[i - mask]);
            }
            level--;
        }
    }

    /// <summary>
    /// 回復N動事件遮罩
    /// </summary>
    public static void PrevToFirst()
    {
        int level = (prevLayerMask.Count / Camera.allCamerasCount);
        Prev(level);
        prevLayerMask.Clear();  // 最後要清除 defaultMask 因為彈出的訊息視窗會回到DefaultMask
    }

    /// <summary>
    /// 回復初始事件遮罩
    /// </summary>
    public static void Resume()
    {
        int i = Camera.allCamerasCount - 1;
        foreach (Camera c in Camera.allCameras)
        {
            c.GetComponent<UICamera>().eventReceiverMask = dictSceneDefaultEventMask[Application.loadedLevelName][i];
            i--;
        }
        prevLayerMask.Clear(); // 最後要清除所有Mask 因為會回到DefaultMask
    }
}
