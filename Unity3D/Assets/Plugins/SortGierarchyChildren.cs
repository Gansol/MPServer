using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class SortChildren
{
    /// <summary>
    /// 重新排序Hierarchy子物件順序
    /// </summary>
    /// <param name="parent">父物件</param>
    /// <param name="length">名稱長度(不含編號)</param>
    public static void SortChildrenByID(GameObject parent, int length)
    {

        List<Transform> children = new List<Transform>();
        for (int i = parent.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.transform.GetChild(i);
            children.Add(child);
            child.parent = null;
        }
        children.Sort((Transform t1, Transform t2) => { return int.Parse(t1.name.Remove(0, length)).CompareTo(int.Parse(t2.name.Remove(0, length))); });
        foreach (Transform child in children)
        {
            child.parent = parent.transform;
        }
    }
}