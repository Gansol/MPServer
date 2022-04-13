using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
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
 * 20171225 v1.1.3   修正階層問題    
 * 20171225 v1.1.2   修正階層問題                                                                  
 * ****************************************************************/
//
public static class EventMaskSwitch
{
    public static GameObject OpenedPanel { get; private set; }
    public static GameObject LastPanel;

    private static Dictionary<string, List<LayerMask>> _dictSceneDefaultEventMask = new Dictionary<string, List<LayerMask>>();   // 儲存場景初始事件遮罩
    private static List<LayerMask> _prevLayerMask = new List<LayerMask>();            // 儲存連續的上個遮罩 1~N
    private static List<LayerMask> _defalutLayerMask;                                                        // 儲存初始遮罩


    #region -- Initialize 初始化 --
    /// <summary>
    /// 注意! 只能在場景第一次載入時使用
    /// </summary>
    public static void Initialize()
    {
        // 儲存初始事件遮罩 減少尋找時間
        if (!_dictSceneDefaultEventMask.ContainsKey(SceneManager.GetActiveScene().name))
        {
            _defalutLayerMask = new List<LayerMask>();
            // 儲存目前場景所有攝影機 事件遮罩
            foreach (Camera c in Camera.allCameras)
                _defalutLayerMask.Add(c.GetComponent<UICamera>().eventReceiverMask);
            // 存入目前場景遮罩
            _dictSceneDefaultEventMask.Add(SceneManager.GetActiveScene().name, _defalutLayerMask);
        }
    }
    #endregion

    #region -- EventMaskSwtich 改變UICamera事件觸發遮罩 --
    // 位元移位運算子<< >> 2近位 左移、右移  0 << 1(左移1)  --->  00 = 0
    // 1 << 1(左移1)  -->  001 = 1
    // 1 >> 1(右移1)  -->  010 = 2
    // 1 >> 2(右移2)  -->  0100 = 4
    // 1 << 8(左移8)  -->  000000001
    /// <summary>
    /// 改變UICamera事件觸發遮罩
    /// </summary>
    /// <param name="go">指定物件圖層</param>
    /// /// <param name="nextPanel">是否開啟下一個階層</param>
    public static void Switch(GameObject go)
    {
        foreach (Camera cam in Camera.allCameras)
        {
            _prevLayerMask.Add(cam.GetComponent<UICamera>().eventReceiverMask);
            cam.GetComponent<UICamera>().eventReceiverMask = 1 << go.layer;
        }
        OpenedPanel = go;
    }
    #endregion

    #region --  Prev回復N動事件遮罩   --  
    /// <summary>
    /// 回復N動事件遮罩
    /// </summary>
    /// mask = prevLayerMask % allCamerasCount = 要返回的遮罩位子 123 [1]23 << mask  123 1[2]3 << mask  123 12[3] << mask 
    /// i-mask = 倒數第一組遮罩在陣列的位子     123 123 [123]<<這個
    public static void Prev(int level)
    {
        while (level > 0 && _prevLayerMask.Count != 0)
        {
            foreach (Camera cam in Camera.allCameras)
            {
                int i = _prevLayerMask.Count - 1;    // 數量-1=陣列長度
                int mask = i % Camera.allCamerasCount;
                cam.GetComponent<UICamera>().eventReceiverMask = _prevLayerMask[i - mask];
                _prevLayerMask.Remove(_prevLayerMask[i - mask]);
            }
            level--;
        }
    }
    #endregion

    #region --  PrevToFirst 回到第一個開啟的事件遮罩階層   --  
    /// <summary>
    /// 回到第一個開啟的事件遮罩階層
    /// </summary>
    public static void PrevToFirst()
    {
        int level = (_prevLayerMask.Count / Camera.allCamerasCount);
        if (LastPanel != null) level = 1;
        Prev(level);
        _prevLayerMask.Clear();  // 最後要清除 defaultMask 因為彈出的訊息視窗會回到DefaultMask
    }
    #endregion

    #region --  Resume 回復初始事件遮罩  --  
    /// <summary>
    /// 回復初始事件遮罩
    /// </summary>
    public static void Resume()
    {
        int i = Camera.allCamerasCount - 1;
        foreach (Camera cam in Camera.allCameras)
        {
            cam.GetComponent<UICamera>().eventReceiverMask = _dictSceneDefaultEventMask[SceneManager.GetActiveScene().name][i];
           // Debug.Log(SceneManager.GetActiveScene().name + "    "+i);
            i--;
        }
        _prevLayerMask.Clear(); // 最後要清除所有Mask 因為會回到DefaultMask
    }
    #endregion 
}
