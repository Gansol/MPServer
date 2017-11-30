using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public static class SortChildren
{

    [MenuItem("AShim/SortChildrenByName")]

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