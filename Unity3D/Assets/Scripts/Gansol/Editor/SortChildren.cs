using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public static class SortChildren
{
    public static void SortChildrenByID(GameObject obj)
    {

            List<Transform> children = new List<Transform>();
            for (int i = obj.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = obj.transform.GetChild(i);
                children.Add(child);
                child.parent = null;
            }
            children.Sort((Transform t1, Transform t2) => { return int.Parse(t1.name.Remove(0, 4)).CompareTo(int.Parse(t2.name.Remove(0, 4))); });
            foreach (Transform child in children)
            {
                child.parent = obj.transform;
            }
        }

}