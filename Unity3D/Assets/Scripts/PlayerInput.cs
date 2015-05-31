using UnityEngine;
using System.Collections;

/* ***************************************************************
 * -----Copyright © 2015 Gansol Studio.  All Rights Reserved.-----
 * -----------            CC BY-NC-SA 4.0            -------------
 * -----------  @Website:  EasyUnity@blogspot.com    -------------
 * -----------  @Email:    GansolTW@gmail.com        -------------
 * -----------  @Author:   Krola.                    -------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * 
 * MicePow遊戲專用 玩家Input管理類
 * 點擊到物件 傳送訊息給該物件的 類別方法
 * 
 * ***************************************************************/

public class PlayerInput : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);
        if (hit && hit.collider != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (hit.transform.name == "anims")
                {
                    hit.transform.SendMessage("OnHit");
                }
                }
            }
        }
}
