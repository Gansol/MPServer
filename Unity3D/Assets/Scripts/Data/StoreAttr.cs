using UnityEngine;
using System.Collections;
using System;

public class StoreAttr : AttrBase {

    /// <summary>
    /// 道具ID
    /// </summary>
    public int itemID;
    /// <summary>
    /// 道具名稱
    /// </summary>
    public string itemName;
    /// <summary>
    /// 價格
    /// </summary>
    public short price;
    /// <summary>
    /// 貨幣類別
    /// </summary>
    public byte currencyType;
    /// <summary>
    /// 道具類別
    /// </summary>
    public byte itemType;
    /// <summary>
    /// 促銷數量
    /// </summary>
    public DateTime promotionsCount;
    /// <summary>
    /// 限時
    /// </summary>
    public short limitTime;
    /// <summary>
    /// 總購買量
    /// </summary>
    public int BuyCount;
}
