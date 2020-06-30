using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

//% control, # (shift), & (alt).

/// <summary>
/// 使用方法
/// 在尋找到遺失的Atlas後，把newatlas 的 atlas 配置為正確的 Atlas
/// </summary>
public class FindNGUIMissing : MonoBehaviour
{
    /// <summary>
    /// 更新遺失的NGUI Atlas
    /// </summary>
    [MenuItem("Gansol/NGUI Repair/Replace MissingAtlas %&#r")]
    static void GetAllDependenciesForScenes()
    {
        UISprite newAtlasSprite = new UISprite();
        GameObject[] objs = Selection.gameObjects;

        foreach (GameObject obj in objs)
        {
            Transform newAtlas = obj.transform.Find("Gansol(newatlas)");
            if (newAtlas != null)
            {
                if (newAtlas.GetComponent<UISprite>().atlas != null)
                {
                    newAtlasSprite.atlas = obj.transform.Find("Gansol(newatlas)").GetComponent<UISprite>().atlas;
                    foreach (Transform tran in obj.GetComponentsInChildren<Transform>())
                    {
                        UISprite sprite = tran.GetComponent<UISprite>();
                        if (sprite != null)
                        {
                            if (sprite.atlas == null)
                            {
                                sprite.atlas = newAtlasSprite.atlas;
                                Debug.Log("Replace: " + tran.name);
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("NewAtlas atlas is null !");
                }
            }
            else
            {
                Debug.LogError("NewAtlas Not Found !");
            }
        }

        //foreach (GameObject obj in objs)
        //{
        //    Transform newAtlas = obj.transform.Find("Gansol(newatlas)");

        //    if( newAtlas != null)
        //        DestroyImmediate(newAtlas.gameObject);
        //}

        }

    /// <summary>
    /// 尋找遺失的NGUI Atlas
    /// </summary>
    [MenuItem("Gansol/NGUI Repair/Find MissingAtlas %&#f")]
    static void FindMissingAtlasInScene()
    {
        GameObject[] objs = Selection.gameObjects;
        bool bnull = false;

        foreach (GameObject obj in objs)
        {
            foreach (Transform tran in obj.GetComponentsInChildren<Transform>())
            {
                if (tran.GetComponent<UISprite>() != null)
                {
                    if (tran.GetComponent<UISprite>().atlas == null)
                    {
                        if (obj.transform.Find("Gansol(newatlas)") == null && !bnull)
                        {
                            bnull = !bnull;
                            GameObject go = new GameObject();
                            go.transform.parent = obj.transform;
                            go.AddComponent<UISprite>();
                            go.name = "Gansol(newatlas)";
                            go.GetComponent<UISprite>().atlas = null;
                            Debug.Log("Gansol(newatlas)");
                        }
                        Debug.Log("Name: " + tran.name);
                    }
                }
            }
        }

        if (!bnull)
        {
            Debug.Log("Missing Atlas not found!");
        }
    }

    /// <summary>
    /// 尋找相同的的NGUI Find&Replace SameAtlas
    /// </summary>
    [MenuItem("Gansol/NGUI Repair/Find&Replace SameAtlas %&#p")]
    static void FindAReplaceSameAtlasInScene()
    {
        GameObject[] objs = Selection.gameObjects;
        UISprite oldatlas,newatlas;
        int count = 0;

        foreach (GameObject obj in objs)
        {
            oldatlas = obj.transform.Find("Gansol(oldatlas)").GetComponent<UISprite>();
            newatlas = obj.transform.Find("Gansol(newatlas)").GetComponent<UISprite>();
            if (oldatlas.atlas != null && newatlas.atlas !=null && oldatlas.atlas != newatlas.atlas )
            {
                foreach (Transform tran in obj.GetComponentsInChildren<Transform>())
                {
                    UISprite sprite = tran.GetComponent<UISprite>();
                    if (sprite!=null && sprite.atlas != null)
                    {
                        if (oldatlas.atlas == sprite.atlas && sprite.name!= "Gansol(oldatlas)" && sprite.name != "Gansol(newatlas)")
                        {
                            count++;
                            sprite.atlas = newatlas.atlas;
                            Debug.Log("Name: " + sprite.name);
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("newatlas not set atlas!");
            }
        }

        if (count == 0)
            Debug.Log("No old atlas found!");
    }

    [MenuItem("Gansol/NGUI Repair/test  %&t")]
    static void test()
    {
        GameObject obj = Selection.activeGameObject;
        Debug.Log(obj.GetComponent<UILabel>().bitmapFont.ToString());
    }

    /// <summary>
    /// 尋找相同的的NGUI Find&Replace SameAtlas
    /// </summary>
    [MenuItem("Gansol/NGUI Repair/Find&Replace SameFont %&#t")]
    static void FindAReplaceSameFontInScene()
    {
        GameObject[] objs = Selection.gameObjects;
        UILabel oldfont, newfont;
        int count = 0;

        foreach (GameObject obj in objs)
        {
            oldfont = obj.transform.Find("Gansol(oldfont)").GetComponent<UILabel>();
            newfont = obj.transform.Find("Gansol(newfont)").GetComponent<UILabel>();
            try
            {
                if (oldfont.bitmapFont != null && newfont.bitmapFont != null && oldfont.bitmapFont != newfont.bitmapFont)
                {
                    foreach (Transform tran in obj.GetComponentsInChildren<Transform>())
                    {
                        UILabel label = tran.GetComponent<UILabel>();
                        if (label != null && label.bitmapFont != null)
                        {
                            if (oldfont.bitmapFont == label.bitmapFont && label.name != "Gansol(oldfont)" && label.name != "Gansol(newfont)")
                            {
                                count++;
                                label.bitmapFont = newfont.bitmapFont;
                                Debug.Log("Name: " + label.name);
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("oldfont not set font!");
                }
            }catch (NullReferenceException e)
            {
                throw;
            }
            if (count ==0)
                Debug.Log("Not old font found!");
        }
    }

    /// <summary>
    /// Create OldAtlas
    /// </summary>
    [MenuItem("Gansol/NGUI Repair/Create OldAtlas")]
    static void CreateOldAtlas()
    {
        GameObject[] objs = Selection.gameObjects;

        foreach (GameObject obj in objs)
        {
            if (obj.transform.Find("Gansol(oldatlas)") == null)
            {
                GameObject go = new GameObject();
                go.transform.parent = obj.transform;
                go.AddComponent<UISprite>();
                go.name = "Gansol(oldatlas)";
                go.GetComponent<UISprite>().atlas = null;
                Debug.Log("Gansol(oldatlas) Create!");
            }
        }
    }

    /// <summary>
    /// Create OldFont
    /// </summary>
    [MenuItem("Gansol/NGUI Repair/Create OldFont")]
    static void CreateOldFont()
    {
        GameObject[] objs = Selection.gameObjects;

        foreach (GameObject obj in objs)
        {
            if (obj.transform.Find("Gansol(oldfont)") == null)
            {
                GameObject go = new GameObject();
                go.transform.parent = obj.transform;
                go.AddComponent<UILabel>();
                go.name = "Gansol(oldfont)";
                go.GetComponent<UILabel>().bitmapFont = null;
                Debug.Log("Gansol(oldfont) Create!");
            }
        }
    }
}
        //[MenuItem("Gansol/Find EXP")]
        //static void Test()
        //{

        //    try
        //    {
        //        GameObject obj = Selection.activeGameObject;
        //      Debug.Log(  ReferenceEquals(obj.transform.parent.GetChild(0).GetComponent<UISprite>().atlas, obj.transform.parent.GetChild(1).GetComponent<UISprite>().atlas));


        //    }
        //    catch (MissingReferenceException) // General Object like GameObject/Sprite etc
        //    {
        //        Debug.LogError("The provided reference is missing!");
        //    }
        //    catch (MissingComponentException) // Specific for objects of type Component
        //    {
        //        Debug.LogError("The provided reference is missing!");
        //    }
        //    catch (UnassignedReferenceException) // Specific for unassigned fields
        //    {
        //        Debug.LogWarning("The provided reference is null!");
        //    }
        //    catch (NullReferenceException ex)
        //    {
        //        Debug.Log(ex+ "myLight was not set in the inspector");
        //    }
        //}

   
