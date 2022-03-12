﻿using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

public static class SortChildrenTool
{
#if UNITY_EDITOR
    [MenuItem("Gansol/SortChildrenByName %&s")]
    public static void SortChildrenByName()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            List<Transform> children = new List<Transform>();
            for (int i = obj.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = obj.transform.GetChild(i);
                children.Add(child);
                child.parent = null;
            }
            children.Sort((Transform t1, Transform t2) => { return t1.name.CompareTo(t2.name); });
            foreach (Transform child in children)
            {
                child.parent = obj.transform;
            }
        }
    }
#endif
}
