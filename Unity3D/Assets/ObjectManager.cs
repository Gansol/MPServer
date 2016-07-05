using UnityEngine;
using System.Collections;

public class ObjectManager : MonoBehaviour {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="go">要改變深度的遊戲物件</param>
    /// <param name="parent">要改變Layer父系位置(遊戲物件)</param>
    /// <returns>返回深度值</returns>
    public static int SwitchDepthLayer(GameObject go, GameObject parent, int depth)  //使用遞迴改變老鼠圖片深度  ※※注意遞迴※※
    {
        foreach (Transform child in go.transform)
        {
            //Debug.Log("Layer : " + child.name);
            UISprite sprite = child.GetComponent<UISprite>();
            if (sprite != null)
            {
                sprite.depth += depth;
                child.gameObject.layer = parent.layer;
            }
            else
            {
                child.gameObject.layer = parent.layer;
            }
            SwitchDepthLayer(child.gameObject, parent, depth);  // 遞迴
        }

        return depth;
    }
}
