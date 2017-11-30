using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public class AttrFactory : FactoryBase
{
    /*
    /// <summary>
    /// 取得老鼠屬性 EatingRate MiceSpeed EatFull Skill HP MiceCose LifeTime
    /// </summary>
    /// <param name="miceName">老鼠名稱</param>
    /// <param name="property">屬性欄位名稱</param>
    /// <returns></returns>
    public float GetMiceProperty(string miceName, string property)
    {
        object miceProperty;
        int itemID = MPGFactory.GetObjFactory().GetIDFromName(Global.miceProperty, "ItemID", miceName);
        Global.miceProperty.TryGetValue(itemID.ToString(), out miceProperty);
        Dictionary<string, object> dictMiceProperty = miceProperty as Dictionary<string, object>;
        dictMiceProperty.TryGetValue(property, out miceProperty);

        return System.Convert.ToSingle(miceProperty);
    }
    */

    public MiceAttr GetMiceProperty(string itemID)
    {
        MiceAttr attr = new MiceAttr();
        Dictionary<string, object> data = new Dictionary<string, object>();
        Global.miceProperty.TryGet<Dictionary<string, object>>(itemID, out data);

        // Get Type String因為 Dictionary > JSON 只剩下String型態了
        attr.name = (string)data.Get<string>("ItemName");
        attr.EatingRate = Convert.ToSingle(data.Get<string>("EatingRate"));
        attr.MiceSpeed = Convert.ToSingle(data.Get<string>("MiceSpeed"));
        attr.EatFull = Convert.ToInt16(data.Get<string>("EatFull"));
        attr.SkillID = Convert.ToInt16(data.Get<string>("SkillID"));
        attr.SetMaxHP(Convert.ToInt32(data.Get<string>("HP")));
        attr.SetHP(Convert.ToInt32(data.Get<string>("HP")));
        attr.MiceCost = Convert.ToByte(data.Get<string>("MiceCost"));
        attr.SkillTimes = Convert.ToByte(data.Get<string>("SkillTimes"));
        attr.LifeTime = Convert.ToSingle(data.Get<string>("LifeTime"));
        attr.EatingRate = Convert.ToSingle(data.Get<string>("EatingRate"));

        return attr;
    }

    //public MiceAttr GetStoreProperty(string itemID)
    //{
    //    MiceAttr attr = new MiceAttr();
    //    Dictionary<string, object> data = new Dictionary<string, object>();
    //    Global.miceProperty.TryGet<Dictionary<string, object>>(itemID, out data);

    //    // Get Type String因為 Dictionary > JSON 只剩下String型態了
    //    attr.name = (string)data.Get<string>("ItemName");
    //    attr.EatingRate = Convert.ToSingle(data.Get<string>("EatingRate"));
    //    attr.MiceSpeed = Convert.ToSingle(data.Get<string>("MiceSpeed"));
    //    attr.EatFull = Convert.ToInt16(data.Get<string>("EatFull"));
    //    attr.SkillID = Convert.ToInt16(data.Get<string>("SkillID"));
    //    attr.SetMaxHP(Convert.ToInt32(data.Get<string>("HP")));
    //    attr.SetHP(Convert.ToInt32(data.Get<string>("HP")));
    //    attr.MiceCost = Convert.ToByte(data.Get<string>("MiceCost"));
    //    attr.LifeTime = Convert.ToSingle(data.Get<string>("LifeTime"));
    //    attr.EatingRate = Convert.ToSingle(data.Get<string>("EatingRate"));

    //    return attr;
    //}
}
